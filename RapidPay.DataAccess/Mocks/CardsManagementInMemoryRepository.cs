using RapidPay.Domain.Entities;
using RapidPay.Domain.Repository;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;

namespace RapidPay.DataAccess.Mocks
{
    public class CardsManagementInMemoryRepository : CardsManagementRepositoryBase

    {
        public CardsManagementInMemoryRepository(DefaultEntities defaultEntities)
            : base(defaultEntities)
        { }

        private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _entities = new();

        private ConcurrentBag<object> GetBag(Type entityType)
        {
            ArgumentNullException.ThrowIfNull(entityType);
            return _entities.GetOrAdd(entityType, x => new ConcurrentBag<object>());
        }

        protected override IQueryable<TEntity> OnGetQueryable<TEntity>()
        {
            return GetBag(typeof(TEntity)).OfType<TEntity>().AsQueryable();
        }

        protected override int OnSaveAndReturnSavedCount<TEntity>(TEntity entity)
        {
            GetBag(typeof(TEntity)).Add(entity);
            return 1;
        }
    }
}
