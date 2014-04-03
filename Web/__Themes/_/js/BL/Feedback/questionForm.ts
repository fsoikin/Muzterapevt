/// <amd-dependency path="css!./Templates/QuestionForm.css" />
/// <amd-dependency path="text!./Templates/QuestionForm.html" />
import c = require( "../../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );
import infoBox = require( "../../Controls/InfoBox" );
import text = require( "../text" );

var Ajax = {
	Send: "feedback/question"
};

export class FormVm extends c.TemplatedControl {
	Name = ko.observable( "" );
	Email = ko.observable( "" );
	Subject = ko.observable( "" );
	Text = ko.observable( "" );
	Simplified = this._args && this._args.simplified;

	InfoBox = new infoBox();
	IsSending = ko.observable( false );
	Sent = ko.observable( false );
	DoneText = new text.TextView( { id: 'Feedback.QuestionSent' } );
	PromptText = new text.TextView( { id: 'Feedback.QuestionPrompt' });

	constructor( private _args?: { simplified?: boolean } ) {
		super( Template );
	}

	Send() {
		if ( !this.Name() || !this.Email() || !this.Subject() ) {
			this.InfoBox.Error( "Пожалуйста заполните все обязательные поля." );
		}

		var data: server.FeedbackQuestion = {
			Name: this.Name(), Email: this.Email(), Subject: this.Subject(), Text: this.Text()
		};

		this.InfoBox.Info( "Вопрос отправляется..." );
		c.Api.Post( Ajax.Send, data, this.IsSending, this.InfoBox.Error, () => {
			this.InfoBox.Clear();
			this.Sent( true );
		} );
	}
}

var Template = $( require( "text!./Templates/QuestionForm.html" ) );