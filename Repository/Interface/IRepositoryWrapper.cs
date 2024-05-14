using Repository.Interface.User;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IRepositoryWrapper
    {
        //Identity
        //Identity
        ICustomIdentityUserRepository customIdentityUser { get; set; }
        ICustomIdentityUserLoginsRepository customIdentityUserLogins { get; set; }
        ICustomIdentityUserClaimsRepository customIdentityUserClaims { get; set; }
        ICustomIdentityUserTokenRepository customIdentityUserToken { get; set; }
        ICustomIdentityRoleRepository customIdentityRole { get; set; }
        ICustomIdentityRoleClaimRepository customIdentityRoleClaim { get; set; }
        ICustomIdentityUserRoleRepository customIdentityUserRole { get; set; }
        //Identity
        //Identity

        IRepositoryBase<UserProfile> userProfileRepository { get; set; }
    }
}
