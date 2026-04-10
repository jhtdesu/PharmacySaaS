using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Auth.Api.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Contracts.Models;

namespace Auth.Api.Services;

public class MomoService : IMomoService
{
    private readonly IOptions<MomoOptions> _options;
    private readonly HttpClient _httpClient;
    private readonly IMessageQueueService _messageQueueService;

    public MomoService(IOptions<MomoOptions> options, HttpClient httpClient, IMessageQueueService messageQueueService)
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
                return new BaseResponse<string>("Invalid webhook signature", "Signature is required");
            }

            var rawData =
                $"partnerCode={webhookModel.PartnerCode}&accessKey={webhookModel.AccessKey}&requestId={webhookModel.RequestId}&amount={webhookModel.Amount}&orderId={webhookModel.OrderId}&orderInfo={webhookModel.OrderInfo}&orderType={webhookModel.OrderType}&transId={webhookModel.TransId}&message={webhookModel.Message}&resultCode={webhookModel.ResultCode}&payType={webhookModel.PayType}&responseTime={webhookModel.ResponseTime}&extraData={webhookModel.ExtraData}";
            var expectedSignature = ComputeHmacSha256(rawData, _options.Value.SecretKey!);

            if (!string.Equals(expectedSignature, webhookModel.Signature, StringComparison.OrdinalIgnoreCase))
            {
                return new BaseResponse<string>("Invalid webhook signature", "Signature verification failed");
            }

            await _messageQueueService.PublishMomoWebhookAsync(webhookModel, cancellationToken);

            return new BaseResponse<string>("ok", "Webhook received and queued for processing");
        }
        catch (Exception ex)
        {
            return new BaseResponse<string>("Error processing webhook", ex.Message);
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
        model.OrderId = DateTime.UtcNow.Ticks.ToString();
        model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;
        var rawData =
            $"partnerCode={_options.Value.PartnerCode}&accessKey={_options.Value.AccessKey}&requestId={model.OrderId}&amount={model.Amount}&orderId={model.OrderId}&orderInfo={model.OrderInfo}&returnUrl={_options.Value.ReturnUrl}&notifyUrl={_options.Value.NotifyUrl}&extraData=";

        var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey!);

        var requestData = new
        {
            accessKey = _options.Value.AccessKey,
            partnerCode = _options.Value.PartnerCode,
            requestType = _options.Value.RequestType,
            notifyUrl = _options.Value.NotifyUrl,
            returnUrl = _options.Value.ReturnUrl,
            orderId = model.OrderId,
            amount = model.Amount.ToString(CultureInfo.CurrentCulture),
            orderInfo = model.OrderInfo,
            requestId = model.OrderId,
            extraData = string.Empty,
            signature
        };

        var jsonContent = new StringContent(
            JsonConvert.SerializeObject(requestData),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(_options.Value.MomoApiUrl, jsonContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<MomoExecuteResponseModel>(responseContent);
    }

    private static MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
    {
        var orderId = collection.FirstOrDefault(s => s.Key == "orderId").Value.FirstOrDefault() ?? string.Empty;
        var message = collection.FirstOrDefault(s => s.Key == "message").Value.FirstOrDefault() ?? string.Empty;
        var errorCode = collection.FirstOrDefault(s => s.Key == "errorCode").Value.FirstOrDefault() ?? string.Empty;

        return new MomoExecuteResponseModel
        {
            OrderId = orderId,
            Message = message,
            ErrorCode = errorCode
        };
    }
}