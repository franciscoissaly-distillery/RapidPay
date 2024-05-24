using System.Text;

namespace RapidPay.Domain.Exceptions
{
    [Serializable]
    public class CardsManagementException : Exception
    {
        public string? MemberName { get; set; }
        public string? ValueText { get; set; }


        public CardsManagementException(string? message) : base(message)
        { }

        public CardsManagementException(string? message, Exception? innerException) : base(message, innerException)
        { }

        public string GetMessage()
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
