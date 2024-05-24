namespace RapidPay.Domain.Entities
{
    public class CardTransactionType
    {
        public CardTransactionTypeEnum SystemCode { get; init; }

        public string Name { get; init; }

        public int Sign { get; init; }
        public bool GeneratesFee { get; init; }

        private CardTransactionType(CardTransactionTypeEnum systemCode, int sign, bool generatesFee, string name = "")
        {
            SystemCode = systemCode;
            Sign = sign;
            if (string.IsNullOrWhiteSpace(name))
                name = systemCode.ToString();

            Name = name;
            GeneratesFee = generatesFee;
        }

        public static CardTransactionType Payment { get; } = new CardTransactionType(CardTransactionTypeEnum.Payment, 1, true);
        public static CardTransactionType Purchase { get; } = new CardTransactionType(CardTransactionTypeEnum.Purchase, -1, false);
    }
}
