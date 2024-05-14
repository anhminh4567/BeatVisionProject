using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
	public class CartItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int? UserId { get; set; }
		public UserProfile? User { get; set; }
		[Column(TypeName = "nvarchar(30)")]
		public CartItemType ItemType { get; set; }
		public int ItemId { get; set; }
	}
}