require.config( {
	waitSeconds: 20,

	paths: {
		// Roots
		'styles': '../css',

		'ko': 'External/Knockout/knockout-2.2.1.debug',
		'ko.mapping': 'External/Knockout/knockout.mapping-latest',

		// RequireJS plugins
		'text': '../require/text',
		'css': '../require/css',
		'normalize': '../require/normalize'
	},

	deps: [
		'External/Polyfill/polyfill',
		'Base/koBindings',
		'bootstrap'
	],

	map: {
		'ko.mapping': {
			knockout: 'ko'
		}
	},

	packages: [
		{ name: "rx", location: "External/Rx" },
		{ name: "jQuery", location: "External/jQuery" }
	]
});