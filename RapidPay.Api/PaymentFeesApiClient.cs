using RapidPay.Api.Framework.ApiClient;
using RapidPay.Api.Framework.Authentication;
using RapidPay.Domain.Adapters;

public class PaymentFeesApiClient : WebApiClientBase, IPaymentFeesAdapter
{
    public PaymentFeesApiClient(
        IHttpClientFactory clientFactory, 
        IAuthTokenProvider tokenProvider, 
        IConfiguration configuration) 
        : base(clientFactory, tokenProvider, configuration)
    { }

    public async Task<decimal> CalculatePaymentFee(GetFeeRequest request)
    {
        return await InvokeGet<decimal, GetFeeRequest>("PaymentFees", request);
    }
}