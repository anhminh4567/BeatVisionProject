using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
	public class TrackLicense
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string LicenceName { get; set; }
		public bool IsWAVSupported { get; set; }
		public bool IsMP3Supported { get; set; }
		public decimal DefaultPrice { get; set; }
		public decimal CurrentPrice { get; set; }
		public int DistributionLimit { get; set; }
		public int StreamLimit { get; set; }
		public bool IsProducerTagged { get; set; }
		public string LicensePdfBlobPath { get; set; }
	}
}