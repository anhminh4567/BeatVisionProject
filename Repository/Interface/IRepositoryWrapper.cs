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
		IRepositoryBase<Notification> notificationRepository { get; set; }
		IRepositoryBase<Message> messageRepository { get; set; }
		IRepositoryBase<CartItem> cartItemRepository { get; set; }
		IRepositoryBase<Comment> commentRepository { get; set; }
		IRepositoryBase<TrackComment> trackCommentRepository { get; set; }
		IRepositoryBase<AlbumComment> albumCommentRepository { get; set; }
		IRepositoryBase<Track> trackRepository { get; set; }
		IRepositoryBase<TrackLicense> trackLicenseRepository { get; set; }
		IRepositoryBase<Album> albumRepository { get; set; }
		IRepositoryBase<PlayList> playListRepository { get; set; }
		IRepositoryBase<Tag> tagRepository { get; set; }
		IRepositoryBase<BlobFileData> blobFileDataRepository { get; set; }


	}
}
