using RapidPay.Domain.Entities;
using RapidPay.Domain.Repository;

namespace RapidPay.DataAccess.Sql
{

    public class CardsManagementSqlRepository : CardsManagementRepositoryBase
    {

        private CardsManagementDbContext _db;
        public CardsManagementSqlRepository(CardsManagementDbContext db, DefaultEntities defaultEntities)
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
            int updatedRecords = _db.SaveChanges();
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
