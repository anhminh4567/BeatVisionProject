using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Repository.Implementation.CachingImplementation;
using Repository.Interface;
using Shared.Helper;
using Shared.IdentityConfiguration;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Repository.Implementation
{

    public class CustomIdentityUserRepository : CachingRepositoryBase<CustomIdentityUser>, ICustomIdentityUserRepository
    {
        public CustomIdentityUserRepository(ApplicationDbContext context, IDistributedCache distributedCache) : base(context, distributedCache)
        {
        }

		public override async Task<CustomIdentityUser> Create(CustomIdentityUser entity)
		{
			var creatResult = await base.Create(entity);
            if (creatResult is null)
                return creatResult;
            var cacheKey = _cacheKey + creatResult.Id;
            _cacheDb.Set(cacheKey,JsonSerializer.SerializeToUtf8Bytes(creatResult),DistributeCacheOptionFactory.CreateRandomTime());
            return creatResult;
        }

		public override async Task<CustomIdentityUser> Delete(CustomIdentityUser entity)
		{
			var result = await base.Delete(entity);
            if (result is null)
                return result;
            var entityCacheKey = _cacheKey + entity.Id;
            _cacheDb.Remove(entityCacheKey);
            return result;
		}

		public override async Task<CustomIdentityUser> Update(CustomIdentityUser entity)
		{
			var updatedResult = await base.Update(entity);
            if (updatedResult is null)
                return updatedResult;
			var entityCacheKey = _cacheKey + entity.Id;
            _cacheDb.Remove(entityCacheKey);
            _cacheDb.Set(entityCacheKey, JsonSerializer.SerializeToUtf8Bytes(updatedResult), DistributeCacheOptionFactory.CreateRandomTime());
            return updatedResult;
		}
	}
    public class CustomIdentityUserLoginsRepository : RepositoryBase<CustomIdentityUserLogins>, ICustomIdentityUserLoginsRepository
    {
        public CustomIdentityUserLoginsRepository(ApplicationDbContext context) : base(context)
		{
        }
    }
    public class CustomIdentityUserClaimsRepository : RepositoryBase<CustomIdentityUserClaims>, ICustomIdentityUserClaimsRepository
    {
        public CustomIdentityUserClaimsRepository(ApplicationDbContext context) : base(context)
		{
        }
    }
    public class CustomIdentityUserTokenRepository : RepositoryBase<CustomIdentityUserToken>, ICustomIdentityUserTokenRepository
    {
        public CustomIdentityUserTokenRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
    public class CustomIdentityUserRoleRepository : RepositoryBase<CustomIdentityUserRole>, ICustomIdentityUserRoleRepository
    {
        public CustomIdentityUserRoleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
    public class CustomIdentityRoleClaimRepository : RepositoryBase<CustomIdentityRoleClaim>, ICustomIdentityRoleClaimRepository
    {
        public CustomIdentityRoleClaimRepository(ApplicationDbContext context ) : base(context)
        {
        }
    }
    public class CustomIdentityRoleRepository : RepositoryBase<CustomIdentityRole>, ICustomIdentityRoleRepository
    {
        public CustomIdentityRoleRepository(ApplicationDbContext context ) : base(context)
        {
        }
    }
}
