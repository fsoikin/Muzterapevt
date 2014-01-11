using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using erecruit.Composition;
using erecruit.JS;
using erecruit.Mvc;
using erecruit.Utils;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;

namespace Mut.Controllers
{
	public class SpecialistController : Controller
	{
		[Import] public IRepository<Specialist> Specialists { get; set; }
		[Import] public IRepository<Country> Countries { get; set; }
		[Import] public IRepository<Organization> Organizations { get; set; }
		[Import] public IRepository<SpecialistProfession> Professions { get; set; }
		[Import] public IRepository<SpecialistSpecialization> Specializations { get; set; }
		[Import] public AttachmentUI Attachments { get; set; }
		[Import] public IRepository<Name.Files.File> Files { get; set; }
		[Import] public PageUI PageUI { get; set; }

		[Export]
		private static readonly IMarkdownCustomModule MarkdownTag = MarkdownCustomModule.Create(
			"specialist-application", new ClassRef { Class = "ApplicationVm", Module = "BL/specialist/application" } );

		[EditPermission]
		public ViewResult Moderator() {
			return View();
		}

		[HttpPost]
		public IJsonResponse<unit> Send( [JsonRequestBody] JS.SpecialistRegistrationRequest request ) {
			var res = from req in request.MaybeDefined()
								
								where Maybe.Holds( req.Countries.EmptyIfNull().Any() ).OrFail( "Страна не указана." )
								where Maybe.Holds( !req.FirstName.NullOrEmpty() ).OrFail( "Имя не указано." )
								where Maybe.Holds( !req.LastName.NullOrEmpty() ).OrFail( "Отчество не указано." )
								where Maybe.Holds( !req.City.NullOrEmpty() ).OrFail( "Город не указан." )
								where Maybe.Holds( !req.Resume.NullOrEmpty() ).OrFail( "Краткое резюме не заполнено." )

								from created in Specialists.Add( new Specialist {
									Approved = false,
									FirstName = req.FirstName,
									LastName = req.LastName,
									PatronymicName = req.PatronymicName,
									City = req.City,
									ProfessionDescription = req.ProfessionDescription,
									SpecializationDescription = req.SpecializationDescription,

									Countries = req.Countries.EmptyIfNull()
										.Select( c => Countries.All.FirstOrDefault( x => x.Name == c ) ?? Countries.Add( new Country { Name = c } ) )
										.ToList(),
									Organization = req.Organization.NullOrEmpty() ? null :
										Organizations.All.FirstOrDefault( x => x.Name == req.Organization ) ?? Organizations.Add( new Organization { Name = req.Organization } ),
									Profession = Professions.Find( req.Profession ),
									Specialization = Specializations.Find( req.Specialization ),

									Photo = Files.Find( req.Photo )
								} )

								where Maybe.Holds( created.Profession != null ).OrFail( "Профессия не указана." )
								where Maybe.Holds( created.Specialization != null ).OrFail( "Специализация не указана." )

								from _ in Maybe.Do( UnitOfWork.Commit )
								select unit.Default;

			return res.LogErrors( Log.Error ).AsJsonResponse();
		}

		[Mixin]
		public AttachmentUI.Mixin Photo() {
			return Attachments.AsMixin( "specialists" );
		}

		[EditPermission]
		public IJsonResponse<IEnumerable<JS.SpecialistView>> List( bool? includeIgnored ) {
			return (from all in Specialists.All.MaybeDefined()
							let withoutIgnored = (includeIgnored ?? false) ? all : all.Where( s => !s.Ignored )

							let res = from s in withoutIgnored
												where !s.Approved
												let v = new JS.SpecialistView {
													Id = s.Id,
													FirstName = s.FirstName,
													LastName = s.LastName,
													PatronymicName = s.PatronymicName,
													City = s.City,
													Email = s.Email,
													IsEmailPublic = s.IsEmailPublic,
													IsPhonePublic = s.IsPhonePublic,
													Organization = s.Organization == null ? null : s.Organization.Name,
													Phone = s.Phone,
													Profession = s.Profession == null ? null : s.Profession.Name,
													Specialization = s.Specialization == null ? null : s.Specialization.Name,
													ProfessionDescription = s.ProfessionDescription,
													SpecializationDescription = s.SpecializationDescription,
													Url = s.Url,
													Resume = s.Resume
												}
												select new {
													v,
													Countries = s.Countries.Select( c => c.Name ),
													PhotoPath = s.Photo.FilePath
												}
							select res.ToList()
								.Do( x => {
									x.v.Countries = x.Countries.ToArray();
									x.v.PhotoUrl = x.PhotoPath.NullOrEmpty() ? null :
										Url.Mixin( ( SpecialistController c ) => c.Photo() ).Action( m => m.Crop( x.PhotoPath, 150, 120 ) );
								} )
								.Select( x => x.v )
							)
							.LogErrors( Log.Error )
							.AsJsonResponse();
		}

		[EditPermission, HttpPost]
		public IJsonResponse<unit> Approve( int id ) { return Update( id, s => s.Approved = true ); }
		
		[EditPermission, HttpPost]
		public IJsonResponse<unit> Archive( int id ) { return Update( id, s => s.Ignored = true ); }

		[EditPermission, HttpPost]
		public IJsonResponse<unit> Delete( int id ) { return Update( id, Specialists.Remove ); }

		IJsonResponse<unit> Update( int id, Action<Specialist> upd ) {
			return (from s in Specialists.Find( id ).MaybeDefined().OrFail( "Not found." )
							from _ in Maybe.Do( () => upd( s ) )
							from __ in Maybe.Do( UnitOfWork.Commit )
							select unit.Default
						 )
						 .LogErrors( Log.Error )
						 .AsJsonResponse();
		}

		[ActionName("countries-lookup")]
		public IJsonResponse<IEnumerable<object>> LookupCountry( string term ) { return Lookup( term, Countries, c => c.Name, c => new { c.Id, c.Name } ); }

		[ActionName( "organizations-lookup" )]
		public IJsonResponse<IEnumerable<object>> LookupOrg( string term ) { return Lookup( term, Organizations, c => c.Name, c => new { c.Id, c.Name } ); }

		[ActionName( "professions" )]
		public IJsonResponse<IEnumerable<object>> AllProfessions() { return All( Professions, c => new { c.Id, c.Name } ); }

		[ActionName( "specializations" )]
		public IJsonResponse<IEnumerable<object>> AllSpecializations() { return All( Specializations, c => new { c.Id, c.Name } ); }

		IJsonResponse<IEnumerable<object>> Lookup<T>( string term, IRepository<T> all, Expression<Func<T,string>> name, Func<T,object> toJson )
			where T : class {
			return Maybe.Eval( () => 
				all.All.Where( name.Compose( n => n.Contains( term ) ) ).Select( toJson ) )
				.LogErrors( Log.Error )
				.AsJsonResponse();
		}
		
		IJsonResponse<IEnumerable<object>> All<T>( IRepository<T> all, Func<T, object> toJson )
			where T : class {
			return Maybe
				.Eval( () => all.All.Select( toJson ) )
				.LogErrors( Log.Error )
				.AsJsonResponse();
		}
	}
}