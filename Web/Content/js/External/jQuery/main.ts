/// <reference path="../../../tsd/require.d.ts" />

require.config( {
	map: {
		'External/jQuery/jquery-ui-1.10.3.custom.min': {
			jQuery: 'app/External/Rx/rx.min'
		}
	}
});

define( [
	'External/jQuery/jQuery-1.9.1',
	'External/jQuery/jQuery-ui-1.10.3.custom.min',
	'css!styles/jQuery/jQuery-ui-1.10.3.custom.min.css'],
	$ => $ );