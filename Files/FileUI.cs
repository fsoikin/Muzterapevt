using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using erecruit.Composition;
using erecruit.Utils;
using Mut.Data;

namespace Name.Files
{
	[ContractClass(typeof(Contracts.IFileUIContracts))]
	public interface IFileUI
	{
		ActionResult ServeFile( string domain, string path, bool forceDownload = false );

		ActionResult ServeFileVersion( string domain, string path, string versionKey, Func<FileData, FileVersion> transform, bool forceDownload = false );

		IEnumerable<File> UploadFiles( string domain, string pathPrefix, HttpPostedFileBase[] file );

		IQueryable<File> GetAll( string domain );
	}

	[Export, TransactionScoped]
	class FileUI : IFileUI
	{
		[Import] public IRepository<File> Files { get; set; }
		[Import] public IRepository<FileData> FilesData { get; set; }
		[Import] public IRepository<FileVersion> FilesVersions { get; set; }
		[Import] public IUnitOfWork UnitOfWork { get; set; }

		public ActionResult ServeFile( string domain, string path, bool forceDownload = false ) {
			var file = FilesData.All.Include( f => f.File ).FirstOrDefault( p => p.File.Domain == domain && p.File.FilePath == path );
			if ( file == null ) return new HttpNotFoundResult();
			return new FileResult( file.Data, file.ContentType, forceDownload, file.File.OriginalFileName, file.File.CreatedOn );
		}

		public ActionResult ServeFileVersion( string domain, string path, string versionKey, Func<FileData, FileVersion> transform, bool forceDownload = false ) {
			var file = FilesData.All.Include( f => f.File ).FirstOrDefault( p => p.File.Domain == domain && p.File.FilePath == path );
			if ( file == null ) return new HttpNotFoundResult();

			var ver = GetVersion( domain, path, versionKey, transform );
			return new FileResult( ver == null || ver.Data == null ? file.Data : ver.Data, ver == null ? file.ContentType : ver.ContentType, forceDownload, file.File.OriginalFileName, file.File.CreatedOn );
		}

		public IQueryable<File> GetAll( string domain ) {
			return Files.All.Where( f => f.Domain == domain );
		}

		class FileResult : ActionResult
		{
			readonly byte[] _data;
			readonly string _contentType;
			readonly bool _forceDownload;
			readonly string _fileName;
			readonly DateTime _lastModified;

			public FileResult( byte[] data, string contentType, bool forceDownload, string fileName, DateTime lastModified ) {
				Contract.Requires( data != null );
				_data = data;
				_contentType = contentType ?? System.Net.Mime.MediaTypeNames.Application.Octet;
				_forceDownload = forceDownload;
				_fileName = fileName;
				_lastModified = lastModified;
			}

			public override void ExecuteResult( ControllerContext context ) {
				var resp = context.RequestContext.HttpContext.Response;
				var req = context.RequestContext.HttpContext.Request;

				resp.Cache.SetLastModified( _lastModified );
				resp.Cache.SetCacheability( HttpCacheability.Public );

				var ifModifiedSince = GetIfModifiedSinceHeader( req );
				if ( ifModifiedSince != null && (_lastModified - ifModifiedSince.Value).TotalSeconds < 1 ) {
					resp.StatusCode = 304;
					resp.StatusDescription = "Not Modified";
					return;
				}

				if ( _forceDownload ) {
					resp.AddHeader( "Content-Disposition", "attachment; filename=\"" + _fileName + "\"" );
				}
				resp.ContentType = _contentType;
				resp.OutputStream.Write( _data, 0, _data.Length );
			}

			public static DateTime? GetIfModifiedSinceHeader( HttpRequestBase req ) {
				Contract.Requires( req != null );
				var header = req.Headers["If-Modified-Since"];
				if ( string.IsNullOrEmpty( header ) ) return null;

				DateTime res;
				return
						DateTime.TryParseExact( header, "ddd, dd MMM yyyy HH:mm:ss GMT", null, DateTimeStyles.None, out res ) ?
						(DateTime?)res : null;
			}
		}

		public IEnumerable<File> UploadFiles( string domain, string pathPrefix, HttpPostedFileBase[] file ) {
			pathPrefix = (pathPrefix ?? "").Trim( '/' );
			if ( !pathPrefix.NullOrEmpty() ) pathPrefix = '/' + pathPrefix + '/';

			return from f in file.EmptyIfNull()
						 let fn = Path.GetFileNameWithoutExtension( f.FileName )
						 let ext = Path.GetExtension( f.FileName )
						 let filePath = Enumerable.Range( 1, int.MaxValue )
								 .Select( i => fn + "_" + i + ext )
								 .StartWith( fn + ext )
								 .Select( name => new { name, path = pathPrefix + name } )
								 .Where( n => !Files.All.Any( e => e.Domain == domain && e.FilePath == n.name ) )
								 .Select( n => n.path )
								 .First()
						 let fObj = Files.Add( new File {
							 FilePath = filePath,
							 Domain = domain,
							 OriginalFileName = f.FileName,
							 Data = new FileData { ContentType = f.ContentType, Data = ReadAll( f.InputStream ) }
						 } )
						 let _ = fObj.Data.File = fObj
						 select fObj;
		}

		private byte[] ReadAll( Stream stream ) {
			Contract.Requires( stream != null );
			var buf = new byte[stream.Length];
			int read = 0, ofs = 0;
			do {
				read = stream.Read( buf, ofs, buf.Length - ofs );
				ofs += read;
			} while ( read > 0 );

			return buf;
		}

		private FileVersion GetVersion( string domain, string path, string versionKey, Func<FileData, FileVersion> transform ) {
			var existing = this.FilesVersions.All.FirstOrDefault( fv => fv.File.FilePath == path && fv.File.Domain == domain && fv.Key == versionKey );
			if ( existing != null ) return existing;

			var fileObj = FilesData.All.Include( fd => fd.File ).FirstOrDefault( fl => fl.File.FilePath == path && fl.File.Domain == domain );
			if ( fileObj == null ) return null;

			var newVersion = transform( fileObj );
			if ( newVersion == null ) return null;

			newVersion.File = fileObj.File;
			newVersion.Key = versionKey;

			var res = FilesVersions.Add( newVersion );
			UnitOfWork.Commit();

			return res;
		}
	}

	namespace Contracts
	{
		[ContractClassFor(typeof(IFileUI))]
		abstract class IFileUIContracts : IFileUI
		{
			public ActionResult ServeFile( string domain, string path, bool forceDownload = false ) {
				Contract.Requires( !String.IsNullOrEmpty( path ) );
				Contract.Ensures( Contract.Result<ActionResult>() != null );
				throw new NotImplementedException();
			}

			public ActionResult ServeFileVersion( string domain, string path, string versionKey, Func<FileData, FileVersion> transform, bool forceDownload ) {
				Contract.Requires( !String.IsNullOrEmpty( path ) );
				Contract.Requires( !String.IsNullOrEmpty( versionKey ) );
				Contract.Requires( transform != null );
				Contract.Ensures( Contract.Result<ActionResult>() != null );
				throw new NotImplementedException();
			}

			public IEnumerable<File> UploadFiles( string domain, string pathPrefix, HttpPostedFileBase[] file ) {
				Contract.Requires( file != null );
				Contract.Ensures( Contract.Result<IEnumerable<File>>() != null );	
				throw new NotImplementedException();
			}

			public IQueryable<File> GetAll( string domain ) {
				Contract.Ensures( Contract.Result<IQueryable<File>>() != null );
				return null;
			}
		}
	}
}