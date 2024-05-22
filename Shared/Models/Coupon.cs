using Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public class Coupon
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Name { get; set; }
		public string Code { get; set; } = Guid.NewGuid().ToString();
		public DateTime CreateDate { get; set; } = DateTime.Now;
		public DateTime ExpireDate { get; set; }
		public CouponType CouponType { get; set; }	
		public int TotalAmount { get; set; }
		public int AmountLeft { get; set; }
		public bool IsPercentage { get; set; } = false;
		public bool IsFixPrice { get; set; } = true;
		public decimal? FixPriceReduce { get; set; }
		public decimal? PercentageReduce { get; set; }
	}
}
