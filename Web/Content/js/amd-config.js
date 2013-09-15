require.config( {
	deps: [
		'app/Lib/Polyfill/polyfill',
		'app/Base/koBindings',
		'app/bootstrap'
	],

	waitSeconds: 20,

	paths: {
		// Roots
		'app': 'js',
		'styles': 'css',

		'ko': 'js/Lib/Knockout/knockout-2.2.1.debug',
		'ko.mapping': 'js/Lib/Knockout/knockout.mapping-latest',

		'jQuery': 'js/Lib/jQuery/jQuery-1.9.1',
		'jQueryUI': 'js/Lib/jQuery/jQuery-ui-1.10.3.custom.min',

		// RequireJS plugins
		'text': 'require/text',
		'css': 'require/css',
		'normalize': 'require/normalize'
	},

	map: {
		'ko.mapping': {
			knockout: 'ko'
		}
	},

	packages: [
		{ name: "rx", location: "js/Lib/Rx" },
		{ name: "jQuery", location: "js/Lib/jQuery" }
	]
});

define('RootUrl', ['require'], function (r) { return r.toUrl('../../'); });