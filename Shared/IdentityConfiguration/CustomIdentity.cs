using Microsoft.AspNetCore.Identity;
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
    }

    public class CustomIdentityRoleClaim : IdentityRoleClaim<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
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

