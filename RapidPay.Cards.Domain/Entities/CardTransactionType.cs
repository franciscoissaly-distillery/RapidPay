namespace RapidPay.Cards.Domain.Entities
{
    public class CardTransactionType
    {
        public string SystemCode { get; }
        public string Name { get; }
        public int Sign { get; }
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
