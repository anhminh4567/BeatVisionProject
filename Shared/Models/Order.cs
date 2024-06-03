using Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public class Order
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public DateTime? PaidDate { get; set; } 
		public string? Description { get; set; }
		public int UserId { get; set; }
		public UserProfile? User { get; set; }
		//public bool IsSale { get; set; }
		public int OriginalPrice { get; set; }
		
		// tu day tro xuong se le phan lien quan toi payment
		// tu day tro xuong se le phan lien quan toi payment
		[Column(TypeName = "nvarchar(30)")]
		public OrderStatus Status { get; set; } = OrderStatus.PENDING; // =status
		public int Price { get; set; } // == Amount 
		public int? PricePaid { get; set; } // == AmountPaid
		public int? PriceRemain { get; set; } // == AmountRemain
		public DateTime? CreateDate { get; set; } // CreateAt
		public DateTime? CancelAt { get; set; }	// CancelAt
		public string? CancellationReasons { get; set; }
		public long? OrderCode { get; set; }
		public string? PaymentLinkId { get; set; }
		public IList<OrderTransaction>? OrderTransactions { get; set; } = new List<OrderTransaction>();
		public IList<OrderItem>? OrderItems { get; set; } = new List<OrderItem>();

	}
}
