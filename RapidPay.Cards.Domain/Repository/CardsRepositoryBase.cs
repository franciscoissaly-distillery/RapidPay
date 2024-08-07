﻿using RapidPay.Cards.Domain.Entities;
using RapidPay.Domain.Entities;

namespace RapidPay.Cards.Domain.Repository
{
    public abstract class CardsRepositoryBase : ICardsRepository
    {
        protected CardsRepositoryBase() : this(null!)
        { }

        private DefaultEntities _defaultEntities;
        protected CardsRepositoryBase(DefaultEntities defaultEntities)
        {
            _defaultEntities = defaultEntities;
        }

        protected virtual bool IsInitialized { get; set; }

        protected void EnsureRepositoryIsInitialized()
        {
            if (IsInitialized)
                return;

            lock (typeof(CardsRepositoryBase))
            {
                if (IsInitialized)
                    return;

                if (_defaultEntities == null)
                    _defaultEntities = new DefaultEntities();

                OnInitializeTransactionTypes(_defaultEntities.CardTransactionTypes);
                IsInitialized = true;
            }
        }

        protected void OnInitializeTransactionTypes(IEnumerable<CardTransactionType> expectedInstances)
        {
            if (expectedInstances == null || !expectedInstances.Any())
                return;

            var expectedSystemCodes = (from expected in expectedInstances
                                       select expected.SystemCode).ToList();

            var storedInstances = (from stored in OnGetQueryable<CardTransactionType>()
                                   where expectedSystemCodes.Contains(stored.SystemCode)
                                   select stored).ToList();

            var missingInstances = expectedInstances.Except(from expected in expectedInstances
                                                            from stored in storedInstances
                                                            where stored.SystemCode == expected.SystemCode
                                                            select expected).ToList();

            if (missingInstances.Any())
                OnSaveAndReturnSavedCount(missingInstances);
        }

        protected IQueryable<TEntity> GetQueryable<TEntity>()
            where TEntity : class
        {
            EnsureRepositoryIsInitialized();
            return OnGetQueryable<TEntity>();
        }

        protected abstract IQueryable<TEntity> OnGetQueryable<TEntity>()
            where TEntity : class;


        async protected Task<int> Save<TEntity>(TEntity entity)
            where TEntity : class
        {
            IEnumerable<TEntity> entities = [entity];
            return await Save(entities);
        }

        async protected Task<int> Save<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            EnsureRepositoryIsInitialized();
            return await Task.Run(() => OnSaveAndReturnSavedCount(entities));
        }

        protected abstract int OnSaveAndReturnSavedCount<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class;


        async protected Task<int> Delete<TEntity>(TEntity entity)
            where TEntity : class
        {
            IEnumerable<TEntity> entities = [entity];
            return await Delete(entities);
        }

        async protected Task<int> Delete<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            EnsureRepositoryIsInitialized();
            return await Task.Run(() => OnDeleteAndReturnDeletedCount(entities));
        }

        protected abstract int OnDeleteAndReturnDeletedCount<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class;

        async public Task<CardTransactionType?> GetTransactionType(string systemCode)
        {
            EnsureRepositoryIsInitialized();
            return await Task.Run(() => OnGetTransactionType(systemCode));
        }

        protected virtual CardTransactionType? OnGetTransactionType(string systemCode)
        {
            return GetQueryable<CardTransactionType>().FirstOrDefault(x => x.SystemCode.ToLower() == systemCode.ToLower());
        }

        async public Task<List<Card>> GetAllCards()
        {
            return await Task.Run(() => GetQueryable<Card>().ToList());
        }

        async public Task<Card?> GetCard(string cardNumber)
        {
            EnsureRepositoryIsInitialized();
            return await Task.Run(() => OnGetCard(cardNumber));
        }
        protected virtual Card? OnGetCard(string cardNumber)
        {
            return GetQueryable<Card>().FirstOrDefault(card => card.Number == cardNumber);
        }

        async public Task<List<CardTransaction>> GetAllCardTransactions(Card existingCard, DateTime? asOfDate = null)
        {
            var cardTransactions = OnGetCardTransactionsQuery(existingCard, asOfDate);
            return await Task.Run(() => cardTransactions.ToList());
        }

        async public Task<CardTransaction?> GetCardLastTransaction(Card existingCard, DateTime? asOfDate = null)
        {
            var cardTransactions = OnGetCardTransactionsQuery(existingCard, asOfDate, 1);
            return await Task.Run(() => cardTransactions.SingleOrDefault());
        }

        async public Task<decimal> GetBalanceAmountFromLastTransaction(Card existingCard, DateTime? asOfDate = null)
        {
            var cardTransactions = OnGetCardTransactionsQuery(existingCard, asOfDate, 1);
            return await Task.Run(() => cardTransactions.Sum(x => x.CardBalanceAmount));
        }


        private IQueryable<CardTransaction> OnGetCardTransactionsQuery(Card existingCard, DateTime? asOfDate = default, int? resultsLimit = default)
        {
            IQueryable<CardTransaction> transactions;

            if (asOfDate.GetValueOrDefault() == default)
                asOfDate = DateTime.UtcNow;

            if (existingCard != null)
            {
                transactions = (from eachTransaction in GetQueryable<CardTransaction>()
                                where eachTransaction.Card.Number.ToLower() == existingCard.Number.ToLower()
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
            var success = false;
            if (card != null)
            {
                var updatedRecords = await Save(card);
                success = updatedRecords > 0;
            }
            return success;
        }

        async public Task<bool> DeleteCard(Card card)
        {
            var success = false;
            if (card != null)
            {
                var updatedRecords = await Delete(card);
                success = updatedRecords > 0;
            }
            return success;
        }

        async public Task<bool> SaveTransaction(CardTransaction transaction)
        {
            var success = false;

            if (transaction != null)
            {
                var existingCard = await GetCard(transaction.Card.Number);
                if (existingCard != null)
                {
                    var updatedRecords = await Save(transaction);
                    success = updatedRecords > 0;
                }
            }
            return success;
        }
    }
}
