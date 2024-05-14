using Shared.IdentityConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface ICustomIdentityUserRepository : IRepositoryBase<CustomIdentityUser>
    { }
    public interface ICustomIdentityUserTokenRepository : IRepositoryBase<CustomIdentityUserToken> 
    { }
    public interface ICustomIdentityUserLoginsRepository : IRepositoryBase<CustomIdentityUserLogins> 
    { }
    public interface ICustomIdentityUserClaimsRepository : IRepositoryBase<CustomIdentityUserClaims> 
    { }
    public interface ICustomIdentityRoleRepository : IRepositoryBase<CustomIdentityRole>
    { }
    public interface ICustomIdentityUserRoleRepository : IRepositoryBase<CustomIdentityUserRole> 
    { }
    public interface ICustomIdentityRoleClaimRepository : IRepositoryBase<CustomIdentityRoleClaim> 
    { }
}
