using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using RapidPay.Domain.Entities;
using RapidPay.Domain.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private static bool _hasEverBeenInitialized; // to avoid initilization per new instance 
        protected override bool IsInitialized
        {
            get
            {
                return  _hasEverBeenInitialized || base.IsInitialized;
            }
            set
            {
                _hasEverBeenInitialized = value;
                base.IsInitialized = value;
            }
        }

        protected override IQueryable<TEntity> OnGetQueryable<TEntity>()
        {
            return _db.Set<TEntity>();
        }

        protected override int OnSaveAndReturnSavedCount<TEntity>(TEntity entity)
            where TEntity : class
        {
            _db.Set<TEntity>().Add(entity);
            int updatedRecords = _db.SaveChanges();
            return updatedRecords;
        }
    }
}
