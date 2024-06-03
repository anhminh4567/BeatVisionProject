using Shared.Enums;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseDto
{
	public class OrderDto
	{
		public int Id { get; set; }
		public DateTime PaidDate { get; set; }
		public string Description { get; set; }
		public int UserId { get; set; }
		//public bool IsSale { get; set; }
		public int OriginalPrice { get; set; }

		// tu day tro xuong se le phan lien quan toi payment
		// tu day tro xuong se le phan lien quan toi payment
		public string Status { get; set; } //= OrderStatus.PENDING; // =status
		public int Price { get; set; } // == Amount 
		public int? PricePaid { get; set; } // == AmountPaid
		public int? PriceRemain { get; set; } // == AmountRemain
		public DateTime? CreateDate { get; set; } // CreateAt
		public DateTime? CancelAt { get; set; } // CancelAt
		public string? CancellationReasons { get; set; }
		public long? OrderCode { get; set; }
		public string? PaymentLinkId { get; set; }
		public IList<OrderTransactionDto>? OrderTransactions { get; set; } = new List<OrderTransactionDto>();
		public IList<OrderItemDto>? OrderItems { get; set; } = new List<OrderItemDto>();
	}

	public class OrderItemDto
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public decimal OriginalPrice { get; set; }
		public decimal CurrentPrice { get; set; } // == Price
		public bool IsSale { get; set; } = false;
		public string TrackName { get; set; } // == Name
		public int TrackId { get; set; }
	}

	public class OrderTransactionDto
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public string Reference { get; set; }
		public int Amount { get; set; }
		public string AccountNumber { get; set; } // stk ao tuong ung voi moi don hang, yea, con tk that la nam o counterAccountNumber
		public string TransactionDateTime { get; set; }
		public string? VirtualAccountName { get; set; }
		public string? VirtualAccountNumber { get; set; }
		public string? CounterAccountBankId { get; set; }
		public string? CounterAccountBankName { get; set; }
		public string? CounterAccountName { get; set; }
		public string? CounterAccountNumber { get; set; }
	}
}
