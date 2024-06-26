﻿using Microsoft.Extensions.Caching.Distributed;
using Repository.Interface.User;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Implementation.User
{
    public class UserProfileRepository : RepositoryBase<UserProfile>
    {
        public UserProfileRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
