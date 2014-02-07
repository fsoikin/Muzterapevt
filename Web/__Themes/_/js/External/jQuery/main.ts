/// <reference path="../../../tsd/require.d.ts" />

require.config( {
	map: {
		'External/jQuery/jQuery-ui-1.10.3.custom.min': {
			jQuery: 'External/jQuery/jQuery-1.9.1'
		}
	},

	shim: {
		'External/jQuery/jQuery-ui-1.10.3.custom.min': ['External/jQuery/jQuery-1.9.1']
	}
});

define( [
	'External/jQuery/jQuery-1.9.1',
	'External/jQuery/jQuery-ui-1.10.3.custom.min',
	'css!styles/jQuery/jQuery-ui-1.10.3.custom.min.css'],
	_ => (<any>window).jQuery.noConflict() );