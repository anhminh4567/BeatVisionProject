using Repository.Interface;
using Repository.Interface.User;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Implementation
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        public RepositoryWrapper(ICustomIdentityUserRepository customIdentityUser, ICustomIdentityUserLoginsRepository customIdentityUserLogins, ICustomIdentityUserClaimsRepository customIdentityUserClaims, ICustomIdentityUserTokenRepository customIdentityUserToken, ICustomIdentityRoleRepository customIdentityRole, ICustomIdentityRoleClaimRepository customIdentityRoleClaim, ICustomIdentityUserRoleRepository customIdentityUserRole, IRepositoryBase<UserProfile> userProfileRepository)
        {
            this.customIdentityUser = customIdentityUser;
            this.customIdentityUserLogins = customIdentityUserLogins;
            this.customIdentityUserClaims = customIdentityUserClaims;
            this.customIdentityUserToken = customIdentityUserToken;
            this.customIdentityRole = customIdentityRole;
            this.customIdentityRoleClaim = customIdentityRoleClaim;
            this.customIdentityUserRole = customIdentityUserRole;
            this.userProfileRepository = userProfileRepository;
        }

        public ICustomIdentityUserRepository customIdentityUser { get; set; }

        public ICustomIdentityUserLoginsRepository customIdentityUserLogins { get; set; }

        public ICustomIdentityUserClaimsRepository customIdentityUserClaims { get; set; }

        public ICustomIdentityUserTokenRepository customIdentityUserToken { get; set; }

        public ICustomIdentityRoleRepository customIdentityRole { get; set; }

        public ICustomIdentityRoleClaimRepository customIdentityRoleClaim { get; set; }

        public ICustomIdentityUserRoleRepository customIdentityUserRole { get; set; }

		//public IRepositoryBase<UserProfile> userProfileRepository { get; set; }

		public IRepositoryBase<UserProfile> userProfileRepository { get; set; }
		public IRepositoryBase<Notification> notificationRepository { get; set; }
		public IRepositoryBase<Message> messageRepository { get; set; }
		public IRepositoryBase<CartItem> cartItemRepository { get; set; }
		public IRepositoryBase<Comment> commentRepository { get; set; }
		public IRepositoryBase<TrackComment> trackCommentRepository { get; set; }
		public IRepositoryBase<AlbumComment> albumCommentRepository { get; set; }
		public IRepositoryBase<Track> trackRepository { get; set; }
		public IRepositoryBase<TrackLicense> trackLicenseRepository { get; set; }
		public IRepositoryBase<Album> albumRepository { get; set; }
		public IRepositoryBase<PlayList> playListRepository { get; set; }
		public IRepositoryBase<Tag> tagRepository { get; set; }
		public IRepositoryBase<BlobFileData> blobFileDataRepository { get; set; }
	}
}
