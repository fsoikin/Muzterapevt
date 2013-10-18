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
		'normalize': '../require/normalize'
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

	packages: [
		{ name: "rx", location: "External/Rx" },
		{ name: "jQuery", location: "External/jQuery" }
	]
});