using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using erecruit.Composition;
using erecruit.Utils;
using Mut.Data;

namespace Name.Files
{
	[ContractClass(typeof(Contracts.IPictureUIContracts))]
	public interface IPictureUI
	{
		ActionResult ServeCropped( string path, int width, int height );
		ActionResult ServeStretched( string path, int width, int height );
		ActionResult ServeScaled( string path, int? width, int? height );
	}

	[Export, TransactionScoped]
	class PictureUI : IPictureUI
	{
		[Import] public IFileUI Files { get; set; }

		public ActionResult ServeCropped( string path, int width, int height ) {
			return Files.ServeFileVersion( path, string.Format( "crop_{0}x{1}", width, height ), Crop( width, height ) );
		}
		
		public ActionResult ServeStretched( string path, int width, int height ) {
			return Files.ServeFileVersion( path, string.Format( "crop_{0}x{1}", width, height ), Stretch( width, height ) );
		}

		public ActionResult ServeScaled( string path, int? width, int? height ) {
			return Files.ServeFileVersion( path, string.Format( "scale_{0}", width ?? height ), Scale( width, height ) );
		}

		private Func<FileData, FileVersion> Scale( int? width, int? height ) {
			return Version( pic => {
				var original = BitmapFrame.Create( new MemoryStream( pic.Data ) );

				if ( width.HasValue && original.PixelWidth == width.Value ) return null;
				if ( height.HasValue && original.PixelHeight == height.Value ) return null;
				if ( pic.Data == null ) return null;

				var factor = width.HasValue ? ((double)width.Value) / original.PixelWidth : ((double)height.Value) / original.PixelHeight;
				return new TransformedBitmap( original, new ScaleTransform( factor, factor ) );
			} );
		}

		private Func<FileData, FileVersion> Crop( int width, int height ) {
			return Version( pic => {
				var original = BitmapFrame.Create( new MemoryStream( pic.Data ) );
				var initialCropSize = FitCrop( new Size( original.PixelWidth, original.PixelHeight ), new Size( width, height ) );

				if ( original.PixelWidth == initialCropSize.Width && original.PixelHeight == initialCropSize.Height ) return null;
				if ( pic.Data == null ) return null;

				return new TransformedBitmap(
						new CroppedBitmap( original, new System.Windows.Int32Rect( (original.PixelWidth - initialCropSize.Width) / 2, (original.PixelHeight - initialCropSize.Height) / 2, initialCropSize.Width, initialCropSize.Height ) ),
						new ScaleTransform( ((double)width) / initialCropSize.Width, ((double)height) / initialCropSize.Height ) );
			} );
		}

		private Func<FileData, FileVersion> Stretch( int width, int height ) {
			return Version( pic => {
				var original = BitmapFrame.Create( new MemoryStream( pic.Data ) );

				if ( original.PixelWidth == width && original.PixelHeight == height ) return null;
				if ( pic.Data == null ) return null;

				return new TransformedBitmap( original,
						new ScaleTransform( ((double)width) / original.PixelWidth, ((double)height) / original.PixelHeight ) );
			} );
		}

		Size FitCrop( Size originalSize, Size cropSize ) {
			var scaleFactor = (double)originalSize.Height / cropSize.Height;
			var width = cropSize.Width * scaleFactor;
			if ( width <= originalSize.Width ) return new Size( (int)width, originalSize.Height );

			scaleFactor = (double)originalSize.Width / cropSize.Width;
			var height = cropSize.Height * scaleFactor;
			Contract.Assume( height <= originalSize.Height ); // This must be true - trivial to prove arithmetically
			return new Size( originalSize.Width, (int)height );
		}

		Func<FileData, FileVersion> Version( Func<FileData, BitmapSource> transform ) {
			return fd => {
				var bsrc = transform( fd );

				byte[] imageBytes = null;
				if ( bsrc != null ) {
					using ( var ms = new MemoryStream() ) {
						var png = new PngBitmapEncoder();
						png.Frames.Add( BitmapFrame.Create( bsrc ) );
						png.Save( ms );
						imageBytes = ms.GetBuffer().Take( (int)ms.Length ).ToArray();
					}
				}

				return new FileVersion { Data = imageBytes, ContentType = "image/png" };
			};
		}
	}

	namespace Contracts
	{
		[ContractClassFor(typeof(IPictureUI))]
		abstract class IPictureUIContracts : IPictureUI
		{
			public ActionResult ServeCropped( string path, int width, int height ) {
				Contract.Requires( !String.IsNullOrEmpty( path ) );
				Contract.Requires( width > 0 && height > 0 );
				Contract.Ensures( Contract.Result<ActionResult>() != null );

				throw new NotImplementedException();
			}

			public ActionResult ServeStretched( string path, int width, int height ) {
				Contract.Requires( !String.IsNullOrEmpty( path ) );
				Contract.Requires( width > 0 && height > 0 );
				Contract.Ensures( Contract.Result<ActionResult>() != null );

				throw new NotImplementedException();
			}

			public ActionResult ServeScaled( string path, int? width, int? height ) {
				Contract.Requires( !String.IsNullOrEmpty( path ) );
				Contract.Requires( width != null || height != null );
				Contract.Requires( width == null || width > 0 );
				Contract.Requires( height == null || height > 0 );
				Contract.Ensures( Contract.Result<ActionResult>() != null );

				throw new NotImplementedException();
			}
		}

	}
}