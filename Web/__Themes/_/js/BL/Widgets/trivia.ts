/// <amd-dependency path="text!./Templates/Trivia.html" />
/// <amd-dependency path="css!./Templates/Trivia.css" />
import c = require( "../../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );

export interface TriviaItem {
	Question: string;
	Answers: string[];
	Response: string;
}

export class Vm implements c.IControl {
	IsExpanded = ko.observable( false );
	Expand = () => this.IsExpanded( true );
	Answers: string[]
	constructor( args: { answers: string[] }) {
		this.Answers = ( args && args.answers ) || [];
	}

	OnLoaded( e: Element ) {
		var $e = $( e ).addClass( "trivia" );
		var response = $e
			.find( ".response" )
			.attr( "data-bind", "{ visible: {flag: IsExpanded, effect: 'slide'} }" );
		Template.clone().insertBefore( response );
		ko.applyBindings( this, e );
	}

	ControlsDescendantBindings = true;
}

var Template = $( require( "text!./Templates/Trivia.html" ) );