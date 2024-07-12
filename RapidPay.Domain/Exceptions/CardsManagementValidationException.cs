using System.Text;

namespace RapidPay.Domain.Exceptions
{
    [Serializable]
    public class CardsManagementValidationException : Exception
    {
        public string? MemberName { get; set; }
        public string? ValueText { get; set; }
        public InvalidCategoryEnum InvalidCategory { get; set; }

        public CardsManagementValidationException(string? message) : base(message)
        { }

        public CardsManagementValidationException(string? message, Exception? innerException) : base(message, innerException)
        { }

        public string GetValidationMessage()
        {
            var resultMessage = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(MemberName))
                resultMessage.Append($" ({MemberName}");

            if (!string.IsNullOrWhiteSpace(ValueText))
            {
                if (resultMessage.Length > 0)
                    resultMessage.Append(": ");
                else
                    resultMessage.Append(" (");

                resultMessage.Append($"'{ValueText}'");
            }

            if (resultMessage.Length > 0)
                resultMessage.Append(")");

            resultMessage.Insert(0, Message);

            return resultMessage.ToString();
        }
    }
}
