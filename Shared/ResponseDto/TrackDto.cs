using Shared.Enums;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseDto
{
	public class TrackDto
	{
	}
	public class TrackResponseDto
	{
		public int Id { get; set; }
		public string TrackName { get; set; }
		//public int OwnerId { get; set; }
		//public UserProfile? Owner { get; set; }
		public int PlayCount { get; set; }
		public bool IsAudioPrivate { get; set; }
		public bool IsAudioRemoved { get; set; }
		public bool IsAudioForSale { get; set; } 
		public TrackStatus Status { get; set; }
		public double AudioLenghtSeconds { get; set; }
		public int? AudioChannels { get; set; }
		public int? AudioSampleRate { get; set; }
		public int? AudioBitPerSample { get; set; }
		public IList<TrackCommentDto> Comments { get; set; } = new List<TrackCommentDto>();
		public IList<TagDto> Tags { get; set; } = new List<TagDto>();
		public IList<TrackLicenseDto> Licenses { get; set; } = new List<TrackLicenseDto>();

	}

	

}
