using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using RapidPay.Domain.Entities;
using RapidPay.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPay.DataAccess.Sql
{
    public class CardsManagementDbContext : DbContext
    {
        private readonly ILogger<CardsManagementDbContext> _logger;
        private readonly DefaultEntities _defaultEntities;

        public CardsManagementDbContext(DbContextOptions<CardsManagementDbContext> options,
            ILogger<CardsManagementDbContext> logger,
            DefaultEntities defaultEntities)
            : base(options)
        {
            _logger = logger;

            if (defaultEntities == null)
                defaultEntities = new DefaultEntities();
            _defaultEntities = defaultEntities;
        }

        public DbSet<Card> Cards { get; set; }
        public DbSet<CardTransaction> CardTransactions { get; set; }
        public DbSet<CardTransactionType> CardTransactionsTypes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("CardsManagement");

            modelBuilder.Entity<CardTransactionType>(entity =>
            {
                entity.ToTable("TransactionTypes");
                entity.HasKey(x => x.SystemCode);
                entity.Property(x => x.Name);
                entity.Property(x => x.Sign);
                entity.HasData(_defaultEntities.CardTransactionTypes);
            });

            modelBuilder.Entity<Card>(entity =>
            {
                entity.ToTable("Cards");
                entity.HasKey(x => x.Number);
                entity.Property(x => x.Number)
                      .HasMaxLength(15)
                      .IsFixedLength();
            });


            modelBuilder.Entity<CardTransaction>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TransactionDate)
                      .HasDefaultValueSql("GetUtcDate()");

                entity.Property(x => x.TransactionAmount)
                      .HasPrecision(17, 4)
                      .HasDefaultValue(0);

                entity.Property(x => x.FeeAmount)
                      .HasDefaultValue(0)
                      .HasPrecision(17, 4);

                entity.Property(x => x.CardBalanceAmount)
                      .HasDefaultValue(0)
                      .HasPrecision(17, 4);

                entity.HasOne(x => x.Card).WithMany()
                      .HasForeignKey(x => x.CardNumber)
                      .HasPrincipalKey(card => card.Number);

                entity.HasOne(x => x.TransactionType).WithMany()
                      .HasForeignKey("TypeSystemCode")
                      .HasPrincipalKey(transactionType => transactionType.SystemCode)                      ;

                entity.Navigation(x => x.TransactionType)
                      .AutoInclude();
            });
        }
    }
}
