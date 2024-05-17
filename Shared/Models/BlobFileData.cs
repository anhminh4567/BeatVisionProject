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
	public class BlobFileData
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public decimal? SizeMb { get; set; }
		public string OriginalFileName { get; set; }
		public string GeneratedName { get; set; }
		public string PathUrl { get; set; }
		[Column(TypeName = "nvarchar(30)")]
		public BlobDirectoryType DirectoryType { get; set; } 
		public string FileExtension { get; set; }
		public string ContentType { get; set; }
		public bool IsPublicAccess { get; set; }  
		public bool IsPaidContent { get; set; }
		public DateTime UploadedDate { get; set; } = DateTime.Now;

	}
}
