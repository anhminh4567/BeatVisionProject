using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestDto
{
	internal class CartDto
	{
	}
	public class AddItemToCartModel
	{
		[Required]
		[Range(1, int.MaxValue)]
		public int UserId {  get; set; }
		[Required]
		[Range(1, int.MaxValue)]
		public int ItemId { get; set; }
	}
	public class RemoveItemFromCartModel
	{
		[Required]
		[Range(1, int.MaxValue)]
		public int UserId { get; set; }
		[Required]
		[Range(1, int.MaxValue)]
		public int ItemId { get; set; }
	}
}
