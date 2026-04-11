using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Auth.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Newtonsoft.Json;
using Shared.Contracts.Models;

namespace Auth.Api.Services;

public class MomoService : IMomoService
{
    private readonly IOptions<MomoOptions> _options;
    private readonly HttpClient _httpClient;
    private readonly IMessageQueueService _messageQueueService;

    public MomoService(
        IOptions<MomoOptions> options,
        HttpClient httpClient,
        IMessageQueueService messageQueueService)
    {
        _options = options;
        _httpClient = httpClient;
        _messageQueueService = messageQueueService;
    }

    public async Task<BaseResponse<MomoExecuteResponseModel>> CreatePaymentUrlAsync(OrderInfoModel model)
    {
        var response = await CreatePaymentAsync(model);
        if (response == null)
        {
            return new BaseResponse<MomoExecuteResponseModel>("Failed to create payment");
        }

        var responseCode = !string.IsNullOrWhiteSpace(response.ErrorCode)
            ? response.ErrorCode
            : response.ResultCode?.ToString();

        if (!string.Equals(responseCode, "0", StringComparison.OrdinalIgnoreCase))
        {
            var momoError = !string.IsNullOrWhiteSpace(response.LocalMessage)
                ? response.LocalMessage
                : response.Message;

            return new BaseResponse<MomoExecuteResponseModel>($"MoMo create payment failed: {momoError ?? "Unknown error"} (code: {responseCode ?? "n/a"})");
        }

        return new BaseResponse<MomoExecuteResponseModel>(response, "Payment URL created successfully");
    }

    public BaseResponse<MomoExecuteResponseModel> BuildCallbackResponse(IQueryCollection collection)
    {
        var response = PaymentExecuteAsync(collection);
        return new BaseResponse<MomoExecuteResponseModel>(response, "Payment callback processed");
    }

    public BaseResponse<string> BuildNotificationResponse()
    {
        return new BaseResponse<string>("ok", "MoMo notification received");
    }

    public async Task<BaseResponse<string>> BuildWebhookResponseAsync(MomoWebhookModel webhookModel, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(webhookModel.Signature))
            {
                return new BaseResponse<string>("Signature is required");
            }

            var v3RawData =
                $"accessKey={webhookModel.AccessKey}&amount={webhookModel.Amount}&extraData={webhookModel.ExtraData}&message={webhookModel.Message}&orderId={webhookModel.OrderId}&orderInfo={webhookModel.OrderInfo}&orderType={webhookModel.OrderType}&partnerCode={webhookModel.PartnerCode}&payType={webhookModel.PayType}&requestId={webhookModel.RequestId}&responseTime={webhookModel.ResponseTime}&resultCode={webhookModel.ResultCode}&transId={webhookModel.TransId}";

            var v3ExpectedSignature = ComputeHmacSha256(v3RawData, _options.Value.SecretKey!);

            if (!string.Equals(v3ExpectedSignature, webhookModel.Signature, StringComparison.OrdinalIgnoreCase))
            {
                return new BaseResponse<string>("Signature verification failed");
            }

            await _messageQueueService.PublishMomoWebhookAsync(webhookModel, cancellationToken);

            return new BaseResponse<string>("ok", "Webhook received and queued for processing");
        }
        catch (Exception ex)
        {
            return new BaseResponse<string>($"Error processing webhook: {ex.Message}");
        }
    }

    private string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        byte[] hashBytes;

        using (var hmac = new HMACSHA256(keyBytes))
        {
            hashBytes = hmac.ComputeHash(messageBytes);
        }

        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
    }

    private async Task<MomoExecuteResponseModel?> CreatePaymentAsync(OrderInfoModel model)
    {
        var redirectUrl = _options.Value.RedirectUrl;
        var ipnUrl = _options.Value.IpnUrl;
        var amount = Convert.ToInt64(Math.Round(model.Amount, MidpointRounding.AwayFromZero));
        var originalSaleId = model.OrderId?.Trim();

        var orderId = string.IsNullOrWhiteSpace(originalSaleId)
            ? DateTime.UtcNow.Ticks.ToString()
            : NormalizeOrderIdForMomo(originalSaleId);

        var extraData = BuildExtraData(originalSaleId);
        
        var requestId = DateTime.UtcNow.Ticks.ToString();
        
        model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;
        
        var rawData =
            $"accessKey={_options.Value.AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={model.OrderInfo}&partnerCode={_options.Value.PartnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={_options.Value.RequestType}";

        var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey!);

        // Request JSON with same field order as signature for consistency
        var requestJson = JsonConvert.SerializeObject(new
        {
            accessKey = _options.Value.AccessKey,
            amount,
            extraData,
            ipnUrl,
            orderId,
            orderInfo = model.OrderInfo,
            partnerCode = _options.Value.PartnerCode,
            redirectUrl,
            requestId,
            requestType = _options.Value.RequestType,
            signature
        });

        var jsonContent = new StringContent(
            requestJson,
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(_options.Value.MomoApiUrl, jsonContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<MomoExecuteResponseModel>(responseContent);
    }

    private static string NormalizeOrderIdForMomo(string orderId)
    {
        if (Guid.TryParse(orderId, out var guidValue))
        {
            return guidValue.ToString("N");
        }

        return orderId.Replace("-", string.Empty).Trim();
    }

    private static string BuildExtraData(string? originalSaleId)
    {
        if (string.IsNullOrWhiteSpace(originalSaleId))
        {
            return string.Empty;
        }

        var metadataJson = JsonConvert.SerializeObject(new
        {
            saleId = originalSaleId
        });

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(metadataJson));
    }

    private static MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
    {
        var orderId = collection.FirstOrDefault(s => s.Key == "orderId").Value.FirstOrDefault() ?? string.Empty;
        var message = collection.FirstOrDefault(s => s.Key == "message").Value.FirstOrDefault() ?? string.Empty;
        var errorCode = collection.FirstOrDefault(s => s.Key == "errorCode").Value.FirstOrDefault() ?? string.Empty;
        var resultCodeString = collection.FirstOrDefault(s => s.Key == "resultCode").Value.FirstOrDefault()?.ToString();
        _ = int.TryParse(resultCodeString, out var resultCode);

        return new MomoExecuteResponseModel
        {
            OrderId = orderId,
            Message = message,
            ErrorCode = errorCode,
            ResultCode = string.IsNullOrWhiteSpace(resultCodeString) ? null : resultCode
        };
    }
}