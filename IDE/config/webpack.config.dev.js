const project = require('./project.config');
const commons = require('./webpack.config.common');
const webpack = require('webpack');
const bourbon = require('bourbon');

module.exports = {
	...commons,
	entry: {
		app: [
			`webpack-hot-middleware/client?path=${project.compiler_public_path}__webpack_hmr`,
			project.paths.client('main.js')
		],
		vendor: project.compiler_vendors
	},
	plugins: [
		...commons.plugins,
		new webpack.HotModuleReplacementPlugin(),
		new webpack.NoEmitOnErrorsPlugin(),
		new webpack.optimize.CommonsChunkPlugin({
			names : ['vendor']
		})
	],
	module: {
		rules: [
			...commons.module.rules,
			{
				test: /\.s?css$/,
				use: [
					{ loader: 'style-loader', options: { sourceMap: true } },
					{ loader: 'css-loader', options: { sourceMap: true } },
					{ loader: 'postcss-loader', options: { sourceMap: true, config: { path: './postcss.config.js' } } },
					{ loader: 'sass-loader', options: { sourceMap: true, includePaths: [
						project.paths.client('styles'),
						bourbon.includePaths
					] } }
				]
			}
		]
	}
}
