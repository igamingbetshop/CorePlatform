﻿namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
	public class ApiUpdatePaymentEntryInput
    {
        public Integration.Payments.Models.SerosPay.PaymentModel EntryModel { get; set; }

        public long PaymentRequestId { get; set; }
    }
}