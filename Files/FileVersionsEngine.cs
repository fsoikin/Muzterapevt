using System;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using erecruit.Composition;
using Mut.Data;

namespace Name.Files
{
	[Export, TransactionScoped]
	public class FileVersionsEngine
	{
		private readonly IRepository<FileVersion> _versions;
		private readonly IRepository<FileData> _data;
		private readonly IUnitOfWork _uow;

		public FileVersionsEngine( IUnitOfWork uow, IRepository<FileVersion> vers, IRepository<FileData> data ) {
			Contract.Requires( uow != null );
			Contract.Requires( vers != null );
			Contract.Requires( data != null );
			_uow = uow;
			_versions = vers;
			_data = data;
		}

		public FileVersion GetFileVersion( string path, string versionKey, Func<FileData, FileVersion> transform ) {
			var existing = _versions.All.FirstOrDefault( fv => fv.File.FilePath == path && fv.Key == versionKey );
			if ( existing != null ) return existing;

			var fileObj = _data.All.Include( fd => fd.File ).FirstOrDefault( fl => fl.File.FilePath == path );
			if ( fileObj == null ) return null;

			var newVersion = transform( fileObj );
			newVersion.File = fileObj.File;
			newVersion.Key = versionKey;

			var res = _versions.Add( newVersion );
			_uow.Commit();

			return res;
		}
	}
}