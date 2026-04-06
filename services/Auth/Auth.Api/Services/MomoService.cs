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
    private readonly ILogger<MomoService> _logger;

    public MomoService(IOptions<MomoOptions> options, HttpClient httpClient, ILogger<MomoService> logger)
    {
        _options = options;
        _httpClient = httpClient;
        _logger = logger;
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
        _logger.LogError("succeeded");
        return new BaseResponse<string>("ok", "MoMo notification received");
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