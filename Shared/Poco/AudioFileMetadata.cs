using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Poco
{
	public class FileMetadata
	{
		public decimal? SizeMb { get; set; }
	}
	public class WavFileMetadata : FileMetadata
	{
		public double SecondLenght { get; set; }
		public int? BitPerSample { get; set; }
		public int Channels { get; set; }
		public int SampleRate { get; set; }
		public long SampleCount { get; set; }

	}
	public class Mp3FileMetadata : FileMetadata
	{
		public double SecondLenght { get; set; }
	}
}
