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
		public DateTime CreateDate { get; set; } = DateTime.Now;
		public DateTime PaidDate { get; set; } 
		public string Description { get; set; }
		public int UserId { get; set; }
		public UserProfile User { get; set; }
		public decimal CurrentPrice { get; set; }
		public decimal OriginalPrice { get; set; }
		public bool IsSale { get; set; }
		[Column(TypeName = "nvarchar(30)")]
		public OrderStatus Status { get; set; } = OrderStatus.WAIT_FOR_PAYMENT;
		public IList<OrderItem> Items { get; set; } = new List<OrderItem>();

	}
}
