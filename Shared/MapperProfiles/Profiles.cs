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
namespace Shared.MapperProfiles
{
	public class Profiles : Profile
	{
		public Profiles(AppsettingBinding appsettingBinding) 
		{
			var azureBlobUrl = appsettingBinding.ExternalUrls.AzureBlobBaseUrl;
			//CreateMap<UpdateRoleDto, CustomIdentityRole>()
			//.ForMember();

			CreateMap<UpdateUserProfileDto, UserProfile>();
		}
	}
}
