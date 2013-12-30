using System;
using System.Linq;
using System.Web.Mvc;
using erecruit.Composition;
using log4net;
using Mut.Data;
using Mut.UI;
using Mut.Web;
using erecruit.Utils;
using erecruit.Mvc;
using Name.Files;
using System.Collections.Generic;
using System.Linq.Expressions;

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

		public ViewResult Page() {
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