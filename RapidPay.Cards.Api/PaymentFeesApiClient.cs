using RapidPay.Cards.Adapters;
using RapidPay.Framework.Api.ApiClient;
using RapidPay.Framework.Api.Authentication;

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