using System.Text;

namespace RapidPay.Framework.Domain.Exceptions
{
    [Serializable]
    public class DomainValidationException : DomainException
    {
        public string? MemberName { get; set; }
        public string? ValueText { get; set; }
        public InvalidCategoryEnum InvalidCategory { get; set; }

        public DomainValidationException(string? message) : base(message)
        { }

        public DomainValidationException(string? message, Exception? innerException) : base(message, innerException)
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
