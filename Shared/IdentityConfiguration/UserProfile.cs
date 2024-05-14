using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.IdentityConfiguration
{
    public class UserProfile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; } // Required foreign key property
        public CustomIdentityUser User { get; set; } = null!;// Required reference navigation to principal
        public string? Description { get; set; }
    }
}
