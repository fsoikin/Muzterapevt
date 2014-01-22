using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using erecruit.Composition;
using Mut.Data;

namespace Name.Files
{
	public class File
	{
		public int Id { get; set; }
		public Guid SiteId { get; set; }

		public string Domain { get; set; }
		public string FilePath { get; set; }
		public DateTime CreatedOn { get; set; }
		public string OriginalFileName { get; set; }

		public virtual FileData Data { get; set; }
		public virtual ICollection<FileVersion> Versions { get; set; }

		public File() {
			CreatedOn = DateTime.Now;
		}

		[Export]
		class Mapping : IModelMapping
		{
			public void Map( System.Data.Entity.DbModelBuilder mb ) {
				var f = mb.Entity<File>();
				var d = mb.Entity<FileData>();
				var v = mb.Entity<FileVersion>();

				f.HasKey( x => x.Id );
				d.HasKey( x => x.Id );
				f.HasRequired( x => x.Data ).WithRequiredPrincipal( x => x.File ).WillCascadeOnDelete();

				f.ToTable( "Files" );
				d.ToTable( "Files" );

				v.Ignore( x => x.EffectiveData );
				v.Ignore( x => x.EffectiveContentType );
				v.HasRequired( x => x.File ).WithMany( x => x.Versions ).WillCascadeOnDelete();
			}
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

		public DateTime CreatedOn { get; set; }
		public string Key { get; set; }
		public byte[] Data { get; set; }
		public string ContentType { get; set; }

		public byte[] EffectiveData { get { return Data ?? File.Data.Data; } }
		public string EffectiveContentType { get { return Data == null ? File.Data.ContentType : ContentType; } }

		public FileVersion() {
			CreatedOn = DateTime.Now;
		}
	}
}