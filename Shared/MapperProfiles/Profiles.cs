using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Shared.ConfigurationBinding;
using Shared.IdentityConfiguration;
using Shared.Models;
using Shared.RequestDto;
using Shared.ResponseDto;
namespace Shared.MapperProfiles
{
	public class Profiles : Profile
	{
		public Profiles() 
		{
			CreateMap<UpdateUserProfileDto, UserProfile>();
			CreateMap<Track, TrackResponseDto>();
			CreateMap<Tag, TagDto>();
			CreateMap<TrackLicense, TrackLicenseDto>();
			CreateMap<TrackComment, TrackCommentDto>();
		}
	}
}
