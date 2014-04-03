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

var Strings = {
	ru: {
		Error_EmptyFields: "Пожалуйста заполните все обязательные поля.",
		SendingRequest: "Вопрос отправляется...",
		Send: "Отправить",
		Prompt: {
			Name: "Ваше имя:",
			Email: "Ваш e-mail:",
			Subject: "Заголовок:",
			Text: "Сообщение:"
		}
	},
	en: {
		Error_EmptyFields: "Please fill in all fields.",
		SendingRequest: "Sending your request...",
		Send: "Send",
		Prompt: {
			Name: "Your name:",
			Email: "Your e-mail:",
			Subject: "Subject:",
			Text: "Message text:"
		}
	}
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
	Strings = this._args.lang === 'en' ? Strings.en : Strings.ru;

	constructor( private _args?: { simplified?: boolean; lang?: string } ) {
		super( Template );
	}

	Send() {
		if ( !this.Email() || ( !this.Simplified && (!this.Name() || !this.Subject()) ) ) {
			this.InfoBox.Error( this.Strings.Error_EmptyFields );
			return;
		}

		var data: server.FeedbackQuestion = {
			Name: this.Name(), Email: this.Email(), Subject: this.Subject(), Text: this.Text()
		};

		this.InfoBox.Info( this.Strings.SendingRequest );
		c.Api.Post( Ajax.Send, data, this.IsSending, this.InfoBox.Error, () => {
			this.InfoBox.Clear();
			this.Sent( true );
		} );
	}
}

var Template = $( require( "text!./Templates/QuestionForm.html" ) );