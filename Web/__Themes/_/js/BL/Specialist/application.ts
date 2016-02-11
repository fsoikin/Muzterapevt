/// <amd-dependency path="css!./Templates/Specialist.css" />
/// <amd-dependency path="text!./Templates/Specialist.html" />
import c = require( "../../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );
import upl = require( "../../Lib/Uploader" );
import infoBox = require( "../../Controls/InfoBox" );
import sel = require( "../../Controls/Select" );
import dsApi = require( "../../Lib/DataSourceApi" );
import att = require( "../attachment" );
import text = require( "../text" );

var Ds = {
	orgs: dsApi.createLookup.fromStdUrlScheme<server.Organization>( "specialist/organizations-" ),
	professions: dsApi.create.fromServer<server.SpecialistProfession>( "specialist/professions" ),
	specializations: dsApi.create.fromServer<server.SpecialistSpecialization>( "specialist/specializations" ),
	experienceBrackets: dsApi.create.fromServer<server.SpecialistExperienceBracket>( "specialist/experienceBrackets" ),
	regions: dsApi.create.fromServer<server.Region>( "specialist/regions" )
};

var Ajax = {
	Send: "specialist/send"
};

var addTermOptions: sel.SelectExtraOptions<{ Name: string; Id: number; }> = {
	createSearchChoice: function ( term ) { return { Name: term, Id: -1 }; },
	minimumInputLength: 2
};

export class ApplicationVm extends c.TemplatedControl {
	FirstName = ko.observable( "" );
	LastName = ko.observable( "" );
	PatronymicName = ko.observable( "" );
	ContactEmail = ko.observable( "" );
	PublicEmail = ko.observable( "" );
	PublicPhone = ko.observable( "" );
	Resume = ko.observable( "" );
	Url = ko.observable( "" );
	City = ko.observable( "" );

	Organization = sel.CreateSelect( Ds.orgs(), addTermOptions );
	Regions = sel.CreateMultiSelect(Ds.regions());
	Professions = sel.CreateMultiSelect(Ds.professions());
	Specializations = sel.CreateMultiSelect( Ds.specializations() );
	ExperienceBrackets = sel.CreateSelect( Ds.experienceBrackets() );

	ProfessionDescription = ko.observable( "" );
	SpecializationDescription = ko.observable( "" );
	ExperienceDescription = ko.observable( "" );
	FormalEducation = ko.observable( "" );
	MusicTherapyEducation = ko.observable( "" );

	PhotoHolder = ko.observable<Ko.Observable<att.IAttachment>>();
	Photo = ko.computed( () => this.PhotoHolder() && this.PhotoHolder()() );
	PhotoVisual = ko.computed(() => this.Photo() && this.Photo().Render() );

	ConsentToPersonalDataProcessing = ko.observable( false );
	AgreeWithEthicalGuidelines = ko.observable( false );

	InfoBox = new infoBox();
	Uploader = new upl.Uploader();
	IsSending = ko.observable( false );
	Sent = ko.observable( false );
	DoneText = new text.TextView( { id: 'Specialist.RegistrationSent' } );
	PromptText = new text.TextView( { id: 'Specialist.RegistrationPrompt' });

	constructor() {
		super( Template );
	}

	Send() {
		if (!this.Regions.SelectedItems().length ||
			!this.Professions.SelectedIds().length ||
			!this.Specializations.SelectedIds().length ||
			!this.ExperienceBrackets.SelectedId() ||
			!this.FirstName() ||
			!this.LastName() ||
			!this.Resume() ) {
			this.InfoBox.Error("Пожалуйста заполните все обязательные поля.");
			return;
		}

		this.InfoBox.Info( "Загружаем фотографию..." );
		c.koToRx( this.Uploader.IsUploading )
			.where( uploading => !uploading )
			.take( 1 )
			.subscribe( () => {
				var data: server.SpecialistRegistrationRequest = {
					firstName: this.FirstName(),
					lastName: this.LastName(),
					patronymicName: this.PatronymicName(),

					city: this.City(),
					regions: this.Regions.SelectedIds(),

					professions: this.Professions.SelectedIds(),
					professionDescription: this.ProfessionDescription(),
					specializations: this.Specializations.SelectedIds(),
					specializationDescription: this.SpecializationDescription(),
					experience: this.ExperienceBrackets.SelectedId(),
					experienceDescription: this.ExperienceDescription(),
					formalEducation: this.FormalEducation(),
					musicTherapyEducation: this.MusicTherapyEducation(),

					contactEmail: this.ContactEmail(),
					publicEmail: this.PublicEmail(),
					publicPhone: this.PublicPhone(),

					organization: this.Organization.SelectedItem() && this.Organization.SelectedItem().Name,
					resume: this.Resume(),
					url: this.Url(),
					photo: this.Photo() && this.Photo().FileId()
				};

				this.InfoBox.Info( "Посылаем заявку..." );
				c.Api.Post( Ajax.Send, data, this.IsSending, this.InfoBox.Error, () => {
					this.InfoBox.Clear();
					this.Sent( true );
				} );
			});
	}

	UploadPhoto() {
		this.Uploader.Upload( "/specialist/photo-upload" )
			.selectMany( att.fromUploadResult )
			.doAction( () => this.PhotoHolder( null ) )
			.subscribe( this.PhotoHolder, this.InfoBox.Error );
	}
}

var Template = $( require( "text!./Templates/Specialist.html" ) );