using Microsoft.AspNetCore.Identity;
using Shared.IdentityConfiguration;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseDto
{

	public class CustomIdentityUserDto
	{
		public int Id { get; set; }
		public DateTime Dob { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public bool EmailConfirmed { get;set; }
		public DateTime LockoutEnd { get; set; }
		public int AccessFailedCount { get;set; }
		public UserProfileDto? UserProfile { get; set; } // Reference navigation to dependent
		public virtual IList<CustomIdentityUserTokenDto> UserTokens { get; set; } = new List<CustomIdentityUserTokenDto>();
		public virtual IList<CustomIdentityUserClaimsDto> UserClaims { get; set; } = new List<CustomIdentityUserClaimsDto>();
		public virtual IList<CustomIdentityRoleDto> Roles { get; set; } = new List<CustomIdentityRoleDto>();
		public virtual IList<CustomIdentityUserLoginsDto> UserLogins { get; set; } = new List<CustomIdentityUserLoginsDto>();
	}

	public class CustomIdentityRoleDto
	{
		public int Id { get; set; }
		public virtual string? Name { get; set; }
		public string? Description { get; set; }
		public IList<CustomIdentityRoleClaimDto> RoleClaims { get; set; } = new List<CustomIdentityRoleClaimDto>();

	}

	public class CustomIdentityRoleClaimDto
	{
		public int Id { get; set; }
		public string? ClaimType { get; set; }
		public string? ClaimValue { get; set; }
	}

	public class CustomIdentityUserLoginsDto
	{
		public  string LoginProvider { get; set; }
		//public  string ProviderKey { get; set; }
		public string? ProviderDisplayName { get; set; }
	}



	public class CustomIdentityUserClaimsDto
	{
		public int Id { get; set; }
		public string? ClaimType { get; set; }
		public string? ClaimValue { get; set; }
	}

	public class CustomIdentityUserTokenDto
	{
		public int UserId { get; set; }
		public DateTime? ExpiredDate { get; set; }
		public string LoginProvider { get; set; }
		public string Name { get; set; }
		public string? Value { get; set; }
	}
}
