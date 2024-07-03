using RapidPay.Domain.Repository;
using System.Collections.Concurrent;

namespace RapidPay.DataAccess.Mocks
{
    public class CardsManagementInMemoryRepository : CardsManagementRepositoryBase

    {
        public CardsManagementInMemoryRepository(DefaultEntities defaultEntities)
            : base(defaultEntities)
        { }

        private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _entities = new();

        protected override IQueryable<TEntity> OnGetQueryable<TEntity>()
        {
            return GetBag(typeof(TEntity)).OfType<TEntity>().AsQueryable();
        }

        private ConcurrentBag<object> GetBag(Type entityType)
        {
            ArgumentNullException.ThrowIfNull(entityType);
            return _entities.GetOrAdd(entityType, x => new ConcurrentBag<object>());
        }

        protected override int OnSaveAndReturnSavedCount<TEntity>(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                GetBag(typeof(TEntity)).Add(entity);

            return entities.Count();
        }
    }
}
