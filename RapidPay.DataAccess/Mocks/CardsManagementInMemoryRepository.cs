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

        private void SetEntities(Type entityType, IEnumerable<object>? entities)
        {
            ArgumentNullException.ThrowIfNull(entityType);

            if (entities is null)
                _entities.TryRemove(entityType, out _);
            else
                _entities.AddOrUpdate(entityType,
                    addValue: new ConcurrentBag<object>(entities),
                    updateValueFactory: (k, v) => new ConcurrentBag<object>(entities));
        }

        private void SetEntities<TEntity>(IEnumerable<TEntity>? entities)
        {
            if (entities is null)
                SetEntities(typeof(TEntity), null);
            else
                SetEntities(typeof(TEntity), entities.OfType<object>());
        }


        protected override int OnSaveAndReturnSavedCount<TEntity>(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                GetBag(typeof(TEntity)).Add(entity);

            return entities.Count();
        }

        protected override int OnDeleteAndReturnDeletedCount<TEntity>(IEnumerable<TEntity> entities)
        {
            var currentEntities = GetBag(typeof(TEntity));
            var remainingEntities = currentEntities.Except(entities);
            SetEntities(remainingEntities);

            var removedCount = currentEntities.Count() - remainingEntities.Count();
            return removedCount;
        }
    }
}
