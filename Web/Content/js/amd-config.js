require.config( {
	waitSeconds: 20,

	paths: {
		// Roots
		'styles': '../css',

		'ko': 'External/Knockout/knockout-2.2.1.debug',
		'ko.mapping': 'External/Knockout/knockout.mapping-latest',
		'linqjs': 'External/LinqJS/linq.min',

		// RequireJS plugins
		'text': '../require/text',
		'css': '../require/css',
		'normalize': '../require/normalize',

		'select2': 'External/Select2/select2_locale_ru'
	},

	deps: [
		'External/Polyfill/polyfill',
		'Base/koBindings',
		'bootstrap'
		//,
		//'http://connect.facebook.net/en_US/all.js#appId=378011798897423'
	],

	map: {
		'*': {
			'linq': 'linqjs'
		},
		'ko.mapping': {
			knockout: 'ko'
		}
	},

	shim: {
		'select2': ['jQuery', 'css!External/Select2/select2.css', 'External/Select2/select2.min']
	},

	packages: [
		{ name: "rx", location: "External/Rx" },
		{ name: "jQuery", location: "External/jQuery" }
	]
});