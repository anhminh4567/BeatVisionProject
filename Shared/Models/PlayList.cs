using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace Shared.Models
{
	public class PlayList
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime CreateDate { get;set; } = DateTime.Now;
		public int PlayCount { get; set; }
		public int OwnerId { get;set; }
		public UserProfile Owner { get; set; }
		public IList<Track> Tracks { get; set; }= new List<Track>();	
		public bool IsPrivate { get; set; }	
	}
}