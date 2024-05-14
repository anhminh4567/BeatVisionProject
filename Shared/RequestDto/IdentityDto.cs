using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestDto
{
    public class IdentityDto
    {
    }
    public class RegisterDto
    {
        [DataType(DataType.Password)]
        [Required]
        [NotNull]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [Required]
        [NotNull]
        // ko them validation o day, identity se lo phan nay trong configuratoin cu no
        public string Password { get; set; }
        [DataType(DataType.Date)]
        [AllowNull]
        public DateTime Dob { get; set; }
    }
    public class LoginDto
    {
        [DataType(DataType.Password)]
        [Required]
        [NotNull]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [Required]
        [NotNull]
        public string Password { get; set; }
    }
    public class ConfirmEmailDto
    {
        [DataType(DataType.Password)]
        [Required]
        [NotNull]
        public string Email { get; set; }
    }
    public class CreateRoleDto
    {
        [Required]
        public string RoleName { get; set; }
        [Required]
        public string Description { get; set; }
    }
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
