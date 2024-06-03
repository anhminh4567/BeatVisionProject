using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseDto
{
	public class CreatePaymentResultDto
	{
		public string bin { get; set; }
		public string accountNumber { get; set; }
		public int amount { get; set; }	
		public string description { get; set; }
		public long orderCode { get; set; }
		public string currency { get; set; }
		public string paymentLinkId { get; set; }
		public string status { get; set; }
		public string checkoutUrl { get; set; }
		public string qrCode { get; set; }

	}
	public class PaymentLinkInformationResultDto
	{
		public string id { get; set; }
		public long orderCode { get; set; }

		public int amount { get; set; }
		public int amountPaid { get; set; }
		public int amountRemaining { get; set; }
		public string status { get; set; }
		public string createdAt { get; set; }
		public List<TransactionResultDto> transactions { get; set; }
		public string? canceledAt { get; set; }
		public string? cancellationReason { get; set; }
	}

	public class TransactionResultDto
	{
	}
}
