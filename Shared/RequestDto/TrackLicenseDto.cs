using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestDto
{
	public class CreateTrackLicenseDto {
		[Required]
		public string LicenceName { get; set; }
		//public bool IsWAVSupported { get; set; }
		//public bool IsMP3Supported { get; set; }
		//public decimal DefaultPrice { get; set; }
		//public decimal CurrentPrice { get; set; }
		public int? DistributionLimit { get; set; } 
		public int? StreamLimit { get; set; } 
		public bool IsProducerTagged { get; set; } = true;
		[Required]
		public IFormFile LicensePdfFile { get; set; }
		//public string LicensePdfBlobPath { get; set; }
	}
	public class RemoveTrackLicenseDto
	{
		[Required]
		[NotNull]
		public int Id { get; set; }
	}
}
