using Microsoft.AspNetCore.Identity;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shared.IdentityConfiguration
{

    public class CustomIdentityUser : IdentityUser<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
        public CustomIdentityUser()
        {
            SecurityStamp = Guid.NewGuid().ToString();
        }
        public CustomIdentityUser(string userName) : this()
        {
            UserName = userName;
        }
        public DateTime? Dob { get; set; }
        public UserProfile? UserProfile { get; set; } // Reference navigation to dependent
		public virtual IList<CustomIdentityUserToken> UserTokens { get; set; } = new List<CustomIdentityUserToken>();
		public virtual IList<CustomIdentityUserClaims> UserClaims { get; set; } =new List<CustomIdentityUserClaims>();
		public virtual IList<CustomIdentityRole> Roles { get; set; } = new List<CustomIdentityRole>();
        public virtual IList<CustomIdentityUserLogins> UserLogins { get; set; } = new List<CustomIdentityUserLogins>();
	}
    public class CustomIdentityRole : IdentityRole<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
        public string? Description { get; set; }
        public CustomIdentityRole()
        {
        }
        public CustomIdentityRole(string roleName) : this()
        {
            Name = roleName;
        }
		public virtual IList<CustomIdentityUser> Users { get; set; } = new List<CustomIdentityUser>();
		public virtual IList<CustomIdentityRoleClaim> RoleClaims { get; set; } = new List<CustomIdentityRoleClaim>();
	}

    public class CustomIdentityRoleClaim : IdentityRoleClaim<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public virtual CustomIdentityRole? Role { get; set; }
	}

    public class CustomIdentityUserRole : IdentityUserRole<int>
    {

	}

    public class CustomIdentityUserClaims : IdentityUserClaim<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
    }

    public class CustomIdentityUserLogins : IdentityUserLogin<int>
    {
    }

    public class CustomIdentityUserToken : IdentityUserToken<int>
    {
        public DateTime? ExpiredDate { get; set; }
    }
}

