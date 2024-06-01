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
		public decimal CurrentPrice { get; set; }
		public bool IsSale { get; set; }
		public string TrackName { get; set; }
		public string TrackLength { get; set; }
		public int OrignalTrackId { get; set; }

	}
}