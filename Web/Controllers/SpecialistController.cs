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
using Mut.UI;
using Mut.Web;

namespace Mut.Controllers
{
	public class SpecialistController : Controller
	{
		[Import] public IRepository<Specialist> Specialists { get; set; }
		[Import] public IRepository<Country> Countries { get; set; }
		[Import] public IRepository<Region> Regions { get; set; }
		[Import] public IRepository<Organization> Organizations { get; set; }
		[Import] public IRepository<SpecialistProfession> Professions { get; set; }
		[Import] public IRepository<SpecialistSpecialization> Specializations { get; set; }
		[Import] public IRepository<SpecialistExperienceBracket> ExperienceBrackets { get; set; }
		[Import] public AttachmentUI Attachments { get; set; }
		[Import] public IRepository<Name.Files.File> Files { get; set; }
		[Import] public PageUI PageUI { get; set; }

		[Export]
		private static readonly IMarkdownCustomModule ApplicationMarkdownTag = MarkdownCustomModule.Create(
			"specialist-application", new ClassRef { Class = "ApplicationVm", Module = "BL/specialist/application" } );

		[Export]
		private static readonly IMarkdownCustomModule SearchMarkdownTag = MarkdownCustomModule.Create(
			"specialist-search", new ClassRef { Class = "SearchVm", Module = "BL/specialist/search" } );

		[EditPermission]
		public ViewResult Moderator() {
			return View();
		}

		[HttpPost]
		public IJsonResponse<unit> Send( [JsonRequestBody] JS.SpecialistRegistrationRequest request ) {
			var res = from req in request.MaybeDefined()

								where Maybe.Holds( !req.firstName.NullOrEmpty() ).OrFail( "Имя не указано." )
								where Maybe.Holds( !req.lastName.NullOrEmpty() ).OrFail( "Отчество не указано." )
								where Maybe.Holds( !req.city.NullOrEmpty() ).OrFail( "Город не указан." )
								where Maybe.Holds( !req.resume.NullOrEmpty() ).OrFail( "Краткое резюме не заполнено." )
								where Maybe.Holds( !req.regions.NullOrEmpty() ).OrFail( "Укажите хотя бы один регион." )

								let professionIDs = req.professions.EmptyIfNull()
								let specializationIDs = req.specializations.EmptyIfNull()

								from created in Specialists.Add( new Specialist {
									Approved = false,
									FirstName = req.firstName,
									LastName = req.lastName,
									PatronymicName = req.patronymicName,
									Resume = req.resume,
									ContactEmail = req.contactEmail,
									PublicEmail = req.publicEmail,
									ContactPhone = req.publicPhone, // We don't ask for contact phone right now
									PublicPhone = req.publicPhone,
									Url = req.url,

									City = req.city,
									Regions = Regions.All.Where( r => req.regions.Contains( r.ID ) ).ToList(),
									Organization = req.organization.NullOrEmpty() ? null :
										Organizations.All.FirstOrDefault( x => x.Name == req.organization ) ?? Organizations.Add( new Organization { Name = req.organization } ),

									Professions = Professions.All.Where( p => professionIDs.Contains( p.Id ) ).ToList(),
									ProfessionDescription = req.professionDescription,

									Specializations = Specializations.All.Where( s => specializationIDs.Contains( s.Id ) ).ToList(),
									SpecializationDescription = req.specializationDescription,

									Experience = ExperienceBrackets.Find( req.experience ),
									ExperienceDescription = req.experienceDescription,
									FormalEducation = req.formalEducation,
									MusicTherapyEducation = req.musicTherapyEducation,

									Photo = Files.Find( req.photo )
								} )

								where Maybe.Holds( created.Regions.EmptyIfNull().Any() ).OrFail( "Регион не указан." )
								where Maybe.Holds( !created.Professions.NullOrEmpty() ).OrFail( "Профессия не указана." )
								where Maybe.Holds( !created.Specializations.NullOrEmpty() ).OrFail( "Специализация не указана." )

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
													id = s.Id,
													firstName = s.FirstName,
													lastName = s.LastName,
													patronymicName = s.PatronymicName,
													contactEmail = s.ContactEmail,
													publicEmail = s.PublicEmail,
													contactPhone = s.ContactPhone,
													publicPhone = s.PublicPhone,
													city = s.City,
													organization = s.Organization == null ? null : s.Organization.Name,
													profession = string.Join( ", ", s.Professions.Select( p => p.Name ) ),
													professionDescription = s.ProfessionDescription,
													specialization = string.Join( ", ", s.Specializations.Select( sp => sp.Name ) ),
													specializationDescription = s.SpecializationDescription,
													experience = s.Experience.Name,
													experienceDescription = s.ExperienceDescription,
													formalEducation = s.FormalEducation,
													musicTherapyEducation = s.MusicTherapyEducation,
													url = s.Url,
													resume = s.Resume
												}
												select new {
													v,
													Regions = s.Regions.Select( c => c.Name ),
													PhotoPath = s.Photo.FilePath
												}
							select res.ToList()
								.Do( x => {
									x.v.regions = x.Regions.ToArray();
									x.v.photoUrl = GetPhotoUrl( x.PhotoPath );
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
		public IJsonResponse<IEnumerable<JS.SpecialistProfession>> AllProfessions() { return All( Professions, c => new JS.SpecialistProfession { Id = c.Id, Name = c.Name, IsNull = c.IsNull }, c => c.Order ); }

		[ActionName( "specializations" )]
		public IJsonResponse<IEnumerable<JS.SpecialistSpecialization>> AllSpecializations() { return All( Specializations, c => new JS.SpecialistSpecialization { Id = c.Id, Name = c.Name, IsNull = c.IsNull }, c => c.Order ); }

		[ActionName( "experienceBrackets" )]
		public IJsonResponse<IEnumerable<JS.SpecialistExperienceBracket>> AllExperienceBrackets() { return All( ExperienceBrackets, c => new JS.SpecialistExperienceBracket { Id = c.Id, Name = c.Name, IsNull = c.IsNull }, c => c.Order ); }

		[ActionName( "regions" )]
		public IJsonResponse<IEnumerable<object>> AllRegions() {
			return Maybe.Eval( () => {
					var regionsLookup = Regions
						.All
						.Select( r => new { r.ID, r.Name, ParentID = (int?)r.Parent.ID } )
						.ToLookup( r => r.ParentID );

					Func<int?, JS.Region[]> getChildrenOf = null;
					getChildrenOf = Func.Create( ( int? parentID ) =>
							regionsLookup[parentID]
							.Select( r => new JS.Region { Id = r.ID, Name = r.Name, children = getChildrenOf( r.ID ) } )
							.ToArray() );

					return getChildrenOf( null );
				} )
				.LogErrors( Log.Error )
				.AsJsonResponse();
		}

		IJsonResponse<IEnumerable<object>> Lookup<T>( string term, IRepository<T> all, Expression<Func<T,string>> name, Func<T,object> toJson )
			where T : class {
			return Maybe.Eval( () => 
				all.All.Where( name.Compose( n => n.Contains( term ) ) ).Select( toJson ) )
				.LogErrors( Log.Error )
				.AsJsonResponse();
		}
		
		IJsonResponse<IEnumerable<R>> All<T, R>( IRepository<T> all, Func<T, R> toJson, Expression<Func<T, int>> order )
			where T : class {
			return Maybe
				.Eval( () => all.All.OrderBy( order ).Select( toJson ) )
				.LogErrors( Log.Error )
				.AsJsonResponse();
		}

		static Expression<Func<Specialist, string>>[] _lookupProperties = new Expression<Func<Specialist, string>>[] {
			s => s.FirstName, s => s.LastName, s => s.PatronymicName,
			s => s.ProfessionDescription, s => s.SpecializationDescription,
			s => s.ExperienceDescription, s => s.FormalEducation, s => s.MusicTherapyEducation,
			s => s.Resume, s => s.City
		};

		[HttpPost]
		public IJsonResponse<IEnumerable<JS.SpecialistPublicView>> Search( JS.SpecialistSearchRequest req ) {
			return (
				from regions in req.regions.EmptyIfNull().MaybeDefined()
				from keywords in (req.keywords ?? "").Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries )

				let res0 = Specialists.All.Where( s => s.Approved )
				let res1 = ApplyRegions( res0, req.regions )
				let res2 = ApplyKeywords( res1, keywords )
				let res = ProjectSearchView( res2 )

				select res )
				.LogErrors( Log.Error )
				.AsJsonResponse();
		}

		private IQueryable<Specialist> ApplyKeywords( IQueryable<Specialist> all, string[] keywords ) {
			var kws = keywords.EmptyIfNull().Where( w => w.Length > 2 ).ToList();
			if ( !kws.Any() ) return all;

			var keywordsExpr = (
				from word in kws

				let containsWord = (
					from prop in _lookupProperties
					select prop.Compose( s => s.Contains( word ) ))
					.Fold( Expression.OrElse )
					.Or( s => s.Specializations.Any( sp => sp.Name.Contains( word ) ) )
					.Or( s => s.Professions.Any( p => p.Name.Contains( word ) ) )

				select containsWord )
				.Fold( Expression.OrElse );

			return all.Where( keywordsExpr );
    }

		private IQueryable<Specialist> ApplyRegions( IQueryable<Specialist> all, int[] regions ) {
			if ( regions.NullOrEmpty() ) return all;
			return all.Where( s => s.Regions.Any( r => regions.Contains( r.ID ) ) );
    }

		private IEnumerable<JS.SpecialistPublicView> ProjectSearchView( IQueryable<Specialist> all ) {
			return all
				.Select( s => new {
					s,
					Regions = s.Regions.Select( r => r.Name ),
					Organization = s.Organization.Name,
					Specializations = s.Specializations.Select( sp => sp.Name ),
					Professions = s.Professions.Select( p => p.Name ),
					Experience = s.Experience.IsNull ? null : s.Experience.Name,
					PhotoPath = s.Photo.FilePath
				} )
				.AsEnumerable()
				.Select( s => new JS.SpecialistPublicView {
					id = s.s.Id,
					firstName = s.s.FirstName,
					lastName = s.s.LastName,
					patronymicName = s.s.PatronymicName,
					email = s.s.PublicEmail,
					phone = s.s.PublicPhone,
					url = s.s.Url,
					city = s.s.City,
					organization = s.Organization,

					specialization = string.Join( ", ", s.Specializations ),
					specializationDescription = s.s.SpecializationDescription,
					profession = string.Join( ", ", s.Professions ),
					professionDescription = s.s.ProfessionDescription,
					experience = s.Experience,
					experienceDescription = s.s.ExperienceDescription,
					formalEducation = s.s.FormalEducation,
					musicTherapyEducation = s.s.MusicTherapyEducation,

					photoUrl = GetPhotoUrl( s.PhotoPath ),
					regions = s.Regions.ToArray(),
					resume = s.s.Resume
				} );
    }

		string GetPhotoUrl( string photoPath ) {
			return photoPath.NullOrEmpty() ? null :
				Url.Mixin( ( SpecialistController c ) => c.Photo() ).Action( m => m.Crop( photoPath, 150, 120 ) );
		}
	}
}