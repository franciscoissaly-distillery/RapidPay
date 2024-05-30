using RapidPay.Domain.Entities;

namespace RapidPay.Domain.Repository
{
    public abstract class CardsManagementRepositoryBase : ICardsManagementRepository
    {
        protected CardsManagementRepositoryBase() : this(null!)
        { }

        private readonly DefaultEntities _defaultEntities;
        protected CardsManagementRepositoryBase(DefaultEntities defaultEntities)
        {
            if (defaultEntities == null)
                defaultEntities = new DefaultEntities();

            _defaultEntities = defaultEntities;
        }

        protected virtual bool IsInitialized { get; set; }
        protected void Initialize()
        {
            if (IsInitialized)
                return;

            lock (this)
            {
                if (IsInitialized)
                    return;

                OnInitializeTransactionTypes(_defaultEntities.CardTransactionTypes);
                IsInitialized = true;
            }
        }

        protected void OnInitializeTransactionTypes(IEnumerable<CardTransactionType> preexistingInstances)
        {
            var systemCodes = (from preexisting in preexistingInstances
                               select preexisting.SystemCode).ToList();

            var storedInstances = (from stored in OnGetQueryable<CardTransactionType>()
                                   where systemCodes.Contains(stored.SystemCode)
                                   select stored).ToList();

            var missingInstances = preexistingInstances.Except(from preexisting in preexistingInstances
                                                               from stored in storedInstances
                                                               where stored.SystemCode == preexisting.SystemCode
                                                               select preexisting).ToList();

            foreach (var missing in missingInstances)
                OnSaveAndReturnSavedCount(missing);
        }

        protected IQueryable<TEntity> GetQueryable<TEntity>()
            where TEntity : class
        {
            if (!IsInitialized)
                Initialize();

            return OnGetQueryable<TEntity>();
        }

        protected abstract IQueryable<TEntity> OnGetQueryable<TEntity>()
            where TEntity : class;


        async protected Task<int> Save<TEntity>(TEntity entity)
            where TEntity : class
        {
            if (!IsInitialized)
                Initialize();

            return await Task.Run(() => OnSaveAndReturnSavedCount(entity));
        }


        protected abstract int OnSaveAndReturnSavedCount<TEntity>(TEntity entity)
            where TEntity : class;

        async public Task<CardTransactionType> GetTransactionType(string systemCode)
        {
            return await Task.Run(() => GetQueryable<CardTransactionType>().FirstOrDefault(x => x.SystemCode == systemCode));
        }

        async public Task<List<Card>> GetAllCards()
        {
            return await Task.Run(() => GetQueryable<Card>().ToList());
        }

        async public Task<Card> GetCardByNumber(string cardNumber)
        {
            return await Task.Run(() => GetQueryable<Card>().FirstOrDefault(card => card.Number == cardNumber));
        }

        async public Task<List<CardTransaction>> GetAllCardTransactions(Card existingCard, DateTime? asOfDate = null)
        {
            var cardTransactions = OnGetCardTransactionsQuery(existingCard, asOfDate);
            return await Task.Run(() => cardTransactions.ToList());
        }

        async public Task<CardTransaction> GetCardLastTransaction(Card existingCard, DateTime? asOfDate = null)
        {
            var cardTransactions = OnGetCardTransactionsQuery(existingCard, asOfDate, 1);
            return await Task.Run(() => cardTransactions.FirstOrDefault());
        }

        private IQueryable<CardTransaction> OnGetCardTransactionsQuery(Card existingCard, DateTime? asOfDate = default, int? resultsLimit = default)
        {
            IQueryable<CardTransaction> transactions = null;

            if (asOfDate.GetValueOrDefault() == default)
                asOfDate = DateTime.Now;

            if (existingCard != null)
            {
                transactions = (from eachTransaction in GetQueryable<CardTransaction>()
                                where eachTransaction.Card.Number == existingCard.Number
                                    && eachTransaction.TransactionDate < asOfDate
                                orderby eachTransaction.TransactionDate descending
                                select eachTransaction);

                if (resultsLimit.HasValue && resultsLimit.Value > 0)
                    transactions = transactions.Take(resultsLimit.Value);
            }
            else
                transactions = new List<CardTransaction>().AsQueryable();

            return transactions;
        }


        async public Task<bool> SaveCard(Card card)
        {
            bool success = false;
            if (card != null)
            {
                int updatedRecords = await Save(card);
                success = updatedRecords > 0;
            }
            return success;
        }

        async public Task<bool> SaveTransaction(CardTransaction transaction)
        {
            bool success = false;

            if (transaction != null)
            {
                var existingCard = await GetCardByNumber(transaction.Card.Number);
                if (existingCard != null)
                {
                    int updatedRecords = await Save(transaction);
                    success = updatedRecords > 0;
                }
            }
            return success;
        }
    }
}
