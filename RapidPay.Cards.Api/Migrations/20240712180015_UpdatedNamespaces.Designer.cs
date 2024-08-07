﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RapidPay.Cards.Data.Sql;

#nullable disable

namespace RapidPay.CardsManagement.Api.Migrations
{
    [DbContext(typeof(CardsDbContext))]
    [Migration("20240712180015_UpdatedNamespaces")]
    partial class UpdatedNamespaces
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("CardsManagement")
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("RapidPay.Cards.Domain.Entities.Card", b =>
                {
                    b.Property<string>("Number")
                        .HasMaxLength(15)
                        .HasColumnType("nchar(15)")
                        .IsFixedLength();

                    b.HasKey("Number");

                    b.ToTable("Cards", "CardsManagement");
                });

            modelBuilder.Entity("RapidPay.Cards.Domain.Entities.CardTransactionType", b =>
                {
                    b.Property<string>("SystemCode")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("GeneratesFee")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Sign")
                        .HasColumnType("int");

                    b.HasKey("SystemCode");

                    b.ToTable("TransactionTypes", "CardsManagement");

                    b.HasData(
                        new
                        {
                            SystemCode = "Payment",
                            GeneratesFee = true,
                            Name = "Payment",
                            Sign = 1
                        },
                        new
                        {
                            SystemCode = "Purchase",
                            GeneratesFee = false,
                            Name = "Purchase",
                            Sign = -1
                        });
                });

            modelBuilder.Entity("RapidPay.Domain.Entities.CardTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("CardBalanceAmount")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(17, 4)
                        .HasColumnType("decimal(17,4)")
                        .HasDefaultValue(0m);

                    b.Property<string>("CardNumber")
                        .IsRequired()
                        .HasColumnType("nchar(15)");

                    b.Property<decimal>("FeeAmount")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(17, 4)
                        .HasColumnType("decimal(17,4)")
                        .HasDefaultValue(0m);

                    b.Property<decimal>("TransactionAmount")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(17, 4)
                        .HasColumnType("decimal(17,4)")
                        .HasDefaultValue(0m);

                    b.Property<DateTime>("TransactionDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<string>("TypeSystemCode")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("CardNumber");

                    b.HasIndex("TypeSystemCode");

                    b.ToTable("Transactions", "CardsManagement");
                });

            modelBuilder.Entity("RapidPay.Domain.Entities.CardTransaction", b =>
                {
                    b.HasOne("RapidPay.Cards.Domain.Entities.Card", "Card")
                        .WithMany()
                        .HasForeignKey("CardNumber")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RapidPay.Cards.Domain.Entities.CardTransactionType", "TransactionType")
                        .WithMany()
                        .HasForeignKey("TypeSystemCode");

                    b.Navigation("Card");

                    b.Navigation("TransactionType");
                });
#pragma warning restore 612, 618
        }
    }
}
