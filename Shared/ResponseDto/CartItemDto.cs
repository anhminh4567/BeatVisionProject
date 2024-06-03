using Shared.Enums;
using Shared.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shared.ResponseDto
{
	public class CartItemDto
	{
		public int Id { get; set; }
		public int? UserId { get; set; }
		public CartItemType ItemType { get; set; }
		public int ItemId { get; set; }
		public TrackResponseDto? Track { get; set; }
	
	}
}