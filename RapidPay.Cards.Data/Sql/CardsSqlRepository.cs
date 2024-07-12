using RapidPay.Cards.Domain.Entities;
using RapidPay.Cards.Domain.Repository;

namespace RapidPay.Cards.Data.Sql
{

    public class CardsSqlRepository : CardsRepositoryBase
    {

        private CardsDbContext _db;
        public CardsSqlRepository(CardsDbContext db, DefaultEntities defaultEntities)
            : base(defaultEntities)
        {
            ArgumentNullException.ThrowIfNull(db);
            _db = db;
        }

        protected override bool IsInitialized
        {
            get
            {
                return _hasInitializationEverHappened || base.IsInitialized;
            }
            set
            {
                _hasInitializationEverHappened = value;
                base.IsInitialized = value;
            }
        }
        private static bool _hasInitializationEverHappened; // to avoid repeating initialization per instance 

        protected override IQueryable<TEntity> OnGetQueryable<TEntity>()
        {
            return _db.Set<TEntity>();
        }

        protected override int OnSaveAndReturnSavedCount<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            _db.Set<TEntity>().AddRange(entities);
            var updatedRecords = _db.SaveChanges();
            return updatedRecords;
        }

        protected override int OnDeleteAndReturnDeletedCount<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            _db.Set<TEntity>().RemoveRange(entities);
            var updatedRecords = _db.SaveChanges();
            return updatedRecords;
        }


        protected override CardTransactionType? OnGetTransactionType(string systemCode)
        {
            return _db.Set<CardTransactionType>().Find(systemCode);
        }

        protected override Card? OnGetCard(string cardNumber)
        {
            return _db.Set<Card>().Find(cardNumber);
        }
    }
}
