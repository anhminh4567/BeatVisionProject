using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseDto
{
	public class TrackLicenseDto
	{
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
