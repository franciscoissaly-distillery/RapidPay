using RapidPay.Domain.Adapters;
using RapidPay.Domain.Entities;

namespace RapidPay.Fees.Mocks
{
    public class RandomPaymentFeesManager : IPaymentFeesAdapter
    {

        // Singleton,
        // not really necessary, since the instance to use is injected in clients of IPaymentManager
        // and may be administered as a singleton by the DI injector;
        // nontheless, still a explicit part of the requirement
        private static RandomPaymentFeesManager _instance = null!;
        public static RandomPaymentFeesManager Instance
        {
            get
            {
                if (_instance == null)
                    lock (typeof(RandomPaymentFeesManager))
                        if (_instance == null)
                            _instance = new RandomPaymentFeesManager();
                return _instance;
            }
        }
        protected RandomPaymentFeesManager()
        { }


        private decimal _feeFactor;
        private DateTime _feeFactorExpiration;
        private decimal _lastFeeAmount;


        public async Task<decimal> CalculatePaymentFee(CardTransaction payment)
        // current implementation ignores the received payment, but any more realistic implementation
        // would most probably require data contextual to the payment, at least the payment's amount
        {
            if (payment == null)
                return 0;

            return await Task.Run(() =>
            {
                lock (this)
                {
                    decimal feeFactor = GetCurrentFeeFactor();
                    decimal newFee = feeFactor * _lastFeeAmount;
                    _lastFeeAmount = newFee;
                    return newFee;
                }
            });
        }

        private decimal GetCurrentFeeFactor()
        {
            if (IsFeeFactorExpired(DateTime.Now))
            {
                _feeFactorExpiration = DateTime.Now.AddHours(1);
                _feeFactor = Convert.ToDecimal(Random.Shared.NextDouble() * 2);
            }

            if (_lastFeeAmount == 0)
                _lastFeeAmount = 1;//avoids permanent multiplication by zero

            return _feeFactor;
        }

        private bool IsFeeFactorExpired(DateTime currentDateTime)
        {
            return _feeFactorExpiration == default || currentDateTime.Subtract(_feeFactorExpiration).TotalHours > 1;
        }
    }
}
