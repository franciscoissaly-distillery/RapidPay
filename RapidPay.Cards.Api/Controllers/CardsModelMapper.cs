using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RapidPay.Cards.Api.Models;
using RapidPay.Cards.Domain.Entities;
using RapidPay.Domain.Entities;
using RapidPay.Framework.Api.Mapping;

namespace RapidPay.Cards.Api.Controllers
{

    public class CardsModelMapper : ModelMapper
    {
        protected override void OnConfigure(MappingConfig config)
        {
            config.AddMapping<CardDto, Card>(
                (dto, entity) =>
                {
                    dto.Number = entity.Number;
                });

            config.AddMapping<TransactionDto, CardTransaction>(
                (dto, entity) =>
                {
                    dto.CardNumber = entity.Card.Number;
                    dto.TypeCode = entity.TransactionType.SystemCode;
                    dto.TypeName = entity.TransactionType.Name;
                    dto.Date = entity.TransactionDate;
                    dto.Amount = entity.TransactionAmount;
                    dto.FeeAmount = entity.FeeAmount;
                    dto.BalanceAmount = entity.CardBalanceAmount;
                });
        }
    }
}
