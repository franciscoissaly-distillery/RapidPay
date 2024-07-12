using RapidPay.CardsManagement.Api.Models;

namespace RapidPay.Api.Test
{

    public class CardsApiTests : CardsApiTestsBase
    {
        [Test]
        public async Task CardFullLifeCycleIntegrationTest()
        {
            var card = await InitTestCard();
            var cardNumber = card.Number;

            var storedTransactions = await GetCardTransactions(cardNumber);
            Assert.That(storedTransactions, Is.Null, "Initial stored transactions");

            var lastCardBalance = await this.GetCardBalance(cardNumber);
            Assert.That(lastCardBalance, Is.EqualTo(0), "Initial card balance");

            decimal[] paymentAmounts = [1000, 2000, 3000];
            var createdTransactions = new Stack<TransactionDto>();
            const string NewTransaction = "New transaction: ";
            foreach (var paymentAmount in paymentAmounts)
            {
                var beforePosting = DateTime.UtcNow;
                var newPayment = await PostNewPayment(cardNumber, paymentAmount);
                var afterPosting = DateTime.UtcNow;

                Assert.That(newPayment.TypeCode, Is.Not.Empty, NewTransaction + nameof(newPayment.TypeCode));
                Assert.That(newPayment.TypeName, Is.Not.Empty, NewTransaction + nameof(newPayment.TypeName));
                Assert.That(newPayment.Date, Is.InRange(beforePosting, afterPosting), NewTransaction + nameof(newPayment.Date));
                Assert.That(newPayment.FeeAmount, Is.GreaterThanOrEqualTo(0), NewTransaction + nameof(newPayment.FeeAmount));
                Assert.That(newPayment.BalanceAmount, Is.GreaterThan(0), NewTransaction + nameof(newPayment.BalanceAmount));

                var newCardBalance = await GetCardBalance(cardNumber);
                Assert.That(newCardBalance, Is.GreaterThanOrEqualTo(lastCardBalance + paymentAmount), "Expected card balance to have raised above prior amount plus current payment");
                Assert.That(newPayment.BalanceAmount, Is.EqualTo(newCardBalance), NewTransaction + nameof(newPayment.BalanceAmount) + " should reflect card balance");

                lastCardBalance = newCardBalance;
                createdTransactions.Push(newPayment);
            }

            storedTransactions = await GetCardTransactions(cardNumber);
            const string FinalStoredTransactions = "Final stored transactions";
            Assert.That(storedTransactions, Is.Not.Null, FinalStoredTransactions);
            Assert.That(storedTransactions, Is.Not.Empty, FinalStoredTransactions);
            Assert.That(storedTransactions, Has.Count.EqualTo(paymentAmounts.Length), FinalStoredTransactions);

            const string StoredTransaction = "Stored transaction: ";
            foreach (var stored in storedTransactions)
            {
                TransactionDto expected = createdTransactions.Pop();
                Assert.That(stored.CardNumber, Is.EqualTo(expected.CardNumber), StoredTransaction + nameof(stored.CardNumber));
                Assert.That(stored.TypeCode, Is.EqualTo(expected.TypeCode), StoredTransaction + nameof(stored.TypeCode));
                Assert.That(stored.TypeName, Is.EqualTo(expected.TypeName), StoredTransaction + nameof(stored.TypeName));
                Assert.That(stored.Amount, Is.EqualTo(expected.Amount), StoredTransaction + nameof(stored.Amount));
                Assert.That(stored.Date, Is.EqualTo(expected.Date), StoredTransaction + nameof(stored.Date));
                Assert.That(stored.FeeAmount, Is.EqualTo(expected.FeeAmount), StoredTransaction + nameof(stored.FeeAmount));
                Assert.That(stored.BalanceAmount, Is.EqualTo(expected.BalanceAmount), StoredTransaction + nameof(stored.BalanceAmount));
            }
        }
    }
}