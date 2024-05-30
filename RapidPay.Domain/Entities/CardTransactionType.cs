namespace RapidPay.Domain.Entities
{
    public class CardTransactionType
    {
        public string SystemCode { get; init; }

        public string Name { get; init; }

        public int Sign { get; init; }
        public bool GeneratesFee { get; init; }

        public CardTransactionType(string systemCode, int sign, bool generatesFee, string name = "")
        {
            if (string.IsNullOrWhiteSpace(systemCode))
                throw new ArgumentNullException("systemCode");
            SystemCode = systemCode;

            Sign = sign;
            if (string.IsNullOrWhiteSpace(name))
                name = systemCode;

            Name = name;
            GeneratesFee = generatesFee;
        }
    }
}
