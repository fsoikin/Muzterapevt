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
	countries: dsApi.createLookup.fromStdUrlScheme<server.Country>( "specialist/countries-" ),
	professions: dsApi.create.fromServer<server.SpecialistProfession>( "specialist/professions" ),
	specializations: dsApi.create.fromServer<server.SpecialistSpecialization>( "specialist/specializations" ),
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
	City = ko.observable( "" );
	ProfessionDescription = ko.observable( "" );
	SpecializationDescription = ko.observable( "" );
	Email = ko.observable( "" );
	Phone = ko.observable( "" );
	IsEmailPublic = ko.observable( false );
	IsPhonePublic = ko.observable( false );
	Resume = ko.observable( "" );
	Url = ko.observable( "" );

	Organization = sel.CreateSelect( Ds.orgs(), addTermOptions );
	Country = sel.CreateMultiSelect( Ds.countries(), addTermOptions );
	Profession = sel.CreateSelect( Ds.professions() );
	Specialization = sel.CreateSelect( Ds.specializations() );

	PhotoHolder = ko.observable<Ko.Observable<att.IAttachment>>();
	Photo = ko.computed( () => this.PhotoHolder() && this.PhotoHolder()() );
	PhotoVisual = ko.computed( () => this.Photo() && this.Photo().Render() );

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
		if ( !this.Country.SelectedItems().length ||
			!this.Profession.SelectedId() ||
			!this.Specialization.SelectedId() ||
			!this.FirstName() ||
			!this.LastName() ||
			!this.City() ||
			!this.Resume() ) {
			this.InfoBox.Error( "Пожалуйста заполните все обязательные поля." );
		}

		var data: server.SpecialistRegistrationRequest = {
			FirstName: this.FirstName(),
			LastName: this.LastName(),
			PatronymicName: this.PatronymicName(),
			City: this.City(),
			Organization: this.Organization.SelectedItem() && this.Organization.SelectedItem().Name,
			Countries: this.Country.SelectedItems().map( c => c.Name ),
			Profession: this.Profession.SelectedId(),
			Specialization: this.Specialization.SelectedId(),
			ProfessionDescription: this.ProfessionDescription(),
			SpecializationDescription: this.SpecializationDescription(),
			Email: this.Email(),
			Phone: this.Phone(),
			IsEmailPublic: !!this.IsEmailPublic(),
			IsPhonePublic: !!this.IsPhonePublic(),
			Resume: this.Resume(),
			Url: this.Url(),
			Photo: this.Photo() && this.Photo().FileId()
		};

		this.InfoBox.Info( "Загружаем фотографию..." );
		c.koToRx( this.Uploader.IsUploading )
			.where( uploading => !uploading )
			.take( 1 )
			.subscribe( () => {
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