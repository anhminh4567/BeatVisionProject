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
			CreateMap<CustomIdentityUser, CustomIdentityUserDto>();
			CreateMap<CustomIdentityRole, CustomIdentityRoleDto>();
			CreateMap<CustomIdentityRoleClaim, CustomIdentityRoleClaimDto>();
			CreateMap<CustomIdentityUserClaims, CustomIdentityUserClaimsDto>();
			CreateMap<CustomIdentityUserLogins, CustomIdentityUserLoginsDto>();
			CreateMap<CustomIdentityUserToken, CustomIdentityUserTokenDto>();

			CreateMap<UpdateUserProfileDto, UserProfile>();
			CreateMap<Track, TrackResponseDto>();
			CreateMap<Tag, TagDto>();
			CreateMap<TrackLicense, TrackLicenseDto>();
			CreateMap<TrackComment, TrackCommentDto>();
			CreateMap<UserProfile, UserProfileDto>();
			CreateMap<CartItem,CartItemDto >();
			CreateMap<Notification, NotificationDto >();
			CreateMap<Message, MessageDto>();
			//CreateMap<, >();
			


		}
	}
}
