using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
	public class Tag
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Name { get; set; }
		public IList<Track> Tracks { get; set; } = new List<Track>();
		public IList<Album> Albums { get; set; } = new List<Album>();
	}
}