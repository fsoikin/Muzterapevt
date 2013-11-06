using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using erecruit.Utils;

namespace Mut
{
	public static class TypedRoutingExtensions
	{
		public static string Action<TController>( this UrlHelper url, Expression<Func<TController, object>> action )
				where TController : IController {
			Contract.Requires( action != null );
			return url.Action<TController>( action, action.CallParameters().AsRouteValues() );
		}

		public static string Action<TController>( this UrlHelper url, Expression<Func<TController, object>> action, object routeValues = null )
				where TController : IController {
			return url.Action<TController>( action, routeValues.ToRouteValues() );
		}

		public static string Action<TController>( this UrlHelper url, Expression<Func<TController, object>> action, RouteValueDictionary routeValues )
				where TController : IController {
			Contract.Requires( action != null );
			return url.Action<TController>( action.MemberName(), routeValues );
		}

		public static string Action<TController>( this UrlHelper url, string action, RouteValueDictionary routeValues )
				where TController : IController {
			Contract.Requires( !String.IsNullOrEmpty( action ) );
			return url.Action( typeof( TController ), action, routeValues );
		}

		public static string Action( this UrlHelper url, Type controllerType, string action, RouteValueDictionary routeValues ) {
			Contract.Requires( url != null );
			Contract.Requires( !String.IsNullOrEmpty( action ) );

			return url.Action( action, GetControllerName( controllerType ), routeValues );
		}

		public static string Action<TController>( this UrlHelper url, string action, object routeValues = null )
				where TController : IController {
			return url.Action<TController>( action, routeValues.ToRouteValues() );
		}

		public static string Action( this UrlHelper url, Type controllerType, string action, object routeValues = null ) {
			return url.Action( controllerType, action, routeValues.ToRouteValues() );
		}

		public static MvcForm BeginForm<TController>( this AjaxHelper ajax, Expression<Func<TController, object>> action, AjaxOptions options ) {
			Contract.Requires( ajax != null );
			Contract.Requires( action != null );
			return ajax.BeginForm( action.MemberName(), GetControllerName( typeof( TController ) ),
					action.CallParameters().AsRouteValues(), options );
		}

		public static MvcForm BeginForm<TController>( this HtmlHelper html, Expression<Func<TController, object>> action, FormMethod method = FormMethod.Post, object attributes = null ) {
			Contract.Requires( html != null );
			Contract.Requires( action != null );
			return html.BeginForm( action.MemberName(), typeof( TController ).AssemblyQualifiedName,
					action.CallParameters().AsRouteValues(), method, attributes.ToRouteValues() );
		}

		public static MvcForm BeginForm<TController>( this HtmlHelper html, string actionName, object routeValues, FormMethod method = FormMethod.Post, object attributes = null ) {
			Contract.Requires( html != null );
			Contract.Requires( !String.IsNullOrEmpty( actionName ) );
			return html.BeginForm( actionName, typeof( TController ).AssemblyQualifiedName, routeValues, method, attributes );
		}

		private static string GetControllerName( Type controllerType ) {
			var n = controllerType.Name;
			if ( n.EndsWith( "Controller" ) ) n = n.Substring( 0, n.Length - "Controller".Length );
			return n;
		}

		private static RouteValueDictionary ToRouteValues( this object rv ) {
			return rv == null ? null : new RouteValueDictionary( rv );
		}

		public static RouteValueDictionary AsRouteValues( this IEnumerable<KeyValuePair<string, object>> parameters ) {
			var res = 
        parameters.SelectMany( pk =>
					pk.Value != null && !pk.Value.GetType().IsPrimitive && !pk.Value.GetType().IsEnum && 
            !(pk.Value is string) && !(pk.Value is Guid) && !HasBinder( pk.Value.GetType() )
            ? new RouteValueDictionary( pk.Value ).AsEnumerable()
            : new[] { pk }
				)
				.Where( pk => pk.Value != null )
				.Distinct( pk => pk.Key, StringComparer.InvariantCultureIgnoreCase )
				.ToDictionary( pk => pk.Key, pk => pk.Value, StringComparer.InvariantCultureIgnoreCase );

			return new RouteValueDictionary( res );
		}

		static readonly Memoizer _memoizer = new Memoizer();
		static bool HasBinder( Type t ) {
			return _memoizer.Memoize( t, _ =>
				t.GetCustomAttributes( typeof( ModelBinderAttribute ), true ).Any() ||
        System.Web.Mvc.ModelBinders.Binders.ContainsKey( t )
			);
		}
	}
}