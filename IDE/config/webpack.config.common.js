const webpack = require('webpack')
const project = require('./project.config')
const HtmlWebpackPlugin = require('html-webpack-plugin')
const StyleLintPlugin = require('stylelint-webpack-plugin')
const styleLintFormatter = require('stylelint-formatter-pretty')
const styleLintOptions = {
	configFile: '.stylelintrc',
	context: 'src',
	files: '**/*.s?(a|c)ss',
	failOnError: true,
	quiet: false,
	syntax: 'scss',
	formatter: styleLintFormatter
}

module.exports = {
	devtool: 'source-map',
	entry: {
		app: [
			project.paths.client('main.js')
		],
		vendor: project.compiler_vendors
	},
	output: {
		filename: `[name].[${project.compiler_hash_type}].js`,
		path: project.paths.dist(),
		publicPath: project.compiler_public_path
	},
	externals: {
		'react/lib/ExecutionEnvironment': true,
		'react/lib/ReactContext': true,
		'react/addons': true
	},
	plugins: [
		new webpack.DefinePlugin(project.globals),
		new StyleLintPlugin(styleLintOptions),
		new HtmlWebpackPlugin({
			template: project.paths.client('index.html'),
			hash: false,
			favicon: project.paths.public('favicon.ico'),
			filename: 'index.html',
			inject: 'body',
			minify: {
				collapseWhitespace: true
			}
		}),
	],
	module: {
		rules: [
			// JS
			{
				test: /\.(js|jsx)$/,
				exclude: /node_modules/,
				use: { loader: 'babel-loader', query: project.compiler_babel }
			},
			// File Loaders
			{
				test: /\.(woff|woff2|ttf|svg)?$/,
				use: { loader: 'url-loader', options: { limit: 10000, name: 'fonts/[path][name].[ext]' } }
			},
			{
				test: /\.(otf|eot)?$/,
				use: { loader: 'file-loader', options: { limit: 10000, name: 'fonts/[path][name].[ext]' } }
			},
			{
				test: /\.(png|jpg|gif)$/,
				use: { loader: 'url-loader', options: { limit: 8192 } }
			},
			{
				test: /\.(html)$/,
				use: {
					loader: 'html-loader',
					options: {
						attrs: [':data-src']
					}
				}
			}
		]
	}
};
