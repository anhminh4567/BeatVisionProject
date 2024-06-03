using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
	public class OrderItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int OrderId { get; set; }
		public Order Order { get; set; }
		public decimal OriginalPrice { get; set; }
		public decimal CurrentPrice { get; set; } // == Price
		public bool IsSale { get; set; } = false;
		public string TrackName { get; set; } // == Name
		public int TrackId { get; set; }
		public Track? Track { get; set; }
		//public int Quantity { get; set; }
	}
}