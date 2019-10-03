const project = require('./project.config')
const commons = require('./webpack.config.common')
const webpack = require('webpack')
const bourbon = require('bourbon')
const ExtractTextPlugin = require('extract-text-webpack-plugin')

module.exports = {
	...commons,
	devtool: 'cheap-module-source-map',
	plugins: [
		...commons.plugins,
		new webpack.optimize.OccurrenceOrderPlugin(),
		new webpack.optimize.AggressiveMergingPlugin(),
		new webpack.optimize.CommonsChunkPlugin({
			names: ['vendor']
		}),
		new webpack.optimize.UglifyJsPlugin({
			compress: {
				unused: true,
				dead_code: true,
				warnings: false
			}
		}),
		new ExtractTextPlugin({
			filename: '[name].[contenthash].css',
			allChunks: true
		})
	],
	module: {
		loaders: [
			...commons.module.rules,
			{
				test: /\.s?css$/,
				use: ExtractTextPlugin.extract({
					fallback: 'style-loader',
					use: [
						{ loader: 'css-loader', options: { sourceMap: true } },
						{ loader: 'postcss-loader', options: { sourceMap: true, config: { path: './postcss.config.js' } } },
						{ loader: 'sass-loader', options: { sourceMap: true, includePaths: [
							project.paths.client('styles'),
							bourbon.includePaths
						] } }
					]
				})
			}
		]
	}
}
