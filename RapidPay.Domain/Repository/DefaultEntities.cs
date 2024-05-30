﻿using RapidPay.Domain.Entities;

namespace RapidPay.Domain.Repository
{
    public class DefaultEntities
    {
        public CardTransactionType[] CardTransactionTypes { get; set; } = new[]
        {
            new CardTransactionType(CardTransactionTypeEnum.Payment.ToString(), 1, true),
            new CardTransactionType(CardTransactionTypeEnum.Purchase.ToString(), -1, false)
        };
    }
}
