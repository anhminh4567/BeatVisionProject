using Microsoft.AspNetCore.Http;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestDto
{
	internal class UserProfileDto
	{
	}
	public class UpdateUserProfileDto
	{
		public string? Description { get; set; }
		public string? Fullname { get; set; }
		public IFormFile? NewProfileImage { get; set; }
		public DateTime? Birthday { get; set; }= DateTime.Now;
		public string? Caption { get; set; }
		public string? Instagram { get; set; }
		public string? Youtube { get; set; }
		public string? SoundCloud { get; set; }
		public string? Facebook { get; set; }
	}
}
