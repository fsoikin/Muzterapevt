require.config( {
	deps: [
		'Lib/Polyfill/polyfill',
		'Base/koBindings',
		'bootstrap'
	],

	waitSeconds: 20,

	paths: {
		// Roots
		'styles': '../css',

		'ko': 'Lib/Knockout/knockout-2.2.1.debug',
		'ko.mapping': 'Lib/Knockout/knockout.mapping-latest',

		'jQuery': 'Lib/jQuery/jQuery-1.9.1',
		'jQueryUI': 'Lib/jQuery/jQuery-ui-1.10.3.custom.min',

		// RequireJS plugins
		'text': '../require/text',
		'css': '../require/css',
		'normalize': '../require/normalize'
	},

	map: {
		'ko.mapping': {
			knockout: 'ko'
		}
	},

	packages: [
		{ name: "rx", location: "Lib/Rx" },
		{ name: "jQuery", location: "Lib/jQuery" }
	]
});

define('RootUrl', ['require'], function (r) { return r.toUrl('../../../'); });