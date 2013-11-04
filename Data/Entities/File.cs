using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mut.Data
{
	public class File
	{
		public int Id { get; set; }

		public string FileName { get; set; }
		public DateTime Time { get; set; }

		public string OriginalFileName { get; set; }
		public bool FullyUploaded { get; set; }

		public virtual FileData Data { get; set; }
		public virtual ICollection<FileVersion> Versions { get; set; }

		public File() {
			Time = DateTime.Now;
		}
	}

	public class FileData
	{
		public int Id { get; set; }
		public virtual File File { get; set; }

		public byte[] Data { get; set; }
		public string ContentType { get; set; }
	}

	public class FileVersion
	{
		public int Id { get; set; }
		public virtual File File { get; set; }

		public DateTime Created { get; set; }
		public string Key { get; set; }
		public byte[] Data { get; set; }
		public string ContentType { get; set; }

		public byte[] EffectiveData { get { return Data ?? File.Data.Data; } }
		public string EffectiveContentType { get { return Data == null ? File.Data.ContentType : ContentType; } }

		public FileVersion() {
			Created = DateTime.Now;
		}
	}
}