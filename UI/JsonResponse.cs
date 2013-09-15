using System;
using System.Collections.Generic;
using System.Web.Mvc;
using erecruit.Utils;
using log4net;
using Newtonsoft.Json;
using Mut.UI;

namespace Mut.Web
{
	public interface IJsonResponse<out T>
	{
		bool Success { get; }
		T Result { get; }
		IList<string> Messages { get; }
	}

	// interface "JsonResponse", defined in js/common.ts
	public class JsonResponse<T> : ActionResult, IJsonResponse<T>
	{
		public static readonly JsonResponse<T> Void = new JsonResponse<T> { Success = true };
		public static readonly JsonResponse<T> NotFound = new JsonResponse<T> { Success = false, Messages = { "Not found" } };
		public static readonly JsonResponse<T> AccessDenied = new JsonResponse<T> { Success = false, Messages = { "Access Denied" } };
		public static JsonResponse<T> Error( params string[] messages ) { return new JsonResponse<T> { Success = false, Messages = messages }; }

		public bool Success { get; set; }
		public T Result { get; set; }
		public IList<string> Messages { get; private set; }

		public JsonResponse() { this.Messages = new List<string>(); }

		public override void ExecuteResult( ControllerContext context )
		{
			var rsp = context.HttpContext.Response;
			rsp.ContentType = "application/json"; // TODO: [fs] should be declared as a constant somewhere
			rsp.Write( JsonConvert.SerializeObject( this ) );
		}

		public override string ToString()
		{
			var s = JsonConvert.SerializeObject( this );
			return s.Length < 100 ? s : (s.Substring( 0, 100 ) + " ...");
		}
	}

	public static class JsonResponse
	{
		public static JsonResponse<T> Error<T>( params string[] messages ) { return JsonResponse<T>.Error( messages ); }
		public static JsonResponse<T> Create<T>( T result ) { return new JsonResponse<T> { Success = true, Result = result }; }
		public static JsonResponse<T> Catch<T>( Func<T> f, ILog log ) { return Catch( () => Create( f() ), log ); }
		public static JsonResponse<T> Catch<T>( Action f, ILog log ) { return Catch( () => { f(); return JsonResponse<T>.Void; }, log ); }
		public static JsonResponse<T> Catch<T>( Func<JsonResponse<T>> f, ILog log )
		{
			try { return f(); }
			catch ( Exception ex ) { log.Error( ex ); return Error<T>( ex.Message ); }
		}
	}
}