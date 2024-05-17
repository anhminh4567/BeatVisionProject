using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Poco
{
	public class WavFileMetadata
	{
		public double SecondLenght { get; set; }
		public int? BitPerSample { get; set; }
		public int Channels { get; set; }
		public int SampleRate { get; set; }
		public long SampleCount { get; set; }

	}
	public class Mp3FileMetadata
	{
		public double SecondLenght { get; set; }
	}
}
