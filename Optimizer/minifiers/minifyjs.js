const fs = require('fs');
const compressor = require('node-minify');
const fileReader = require('./../helpers/aws');
const utility = require('./../helpers/utility');
const htmlMinifier = require('html-minifier').minify;
const logger_helper = require('./../helpers/logger');
const kerror = require('./../helpers/error');

const whiteSpace = new RegExp(/\s/g);

exports.minifyjs = function (file, all_resources, project_id, projectSettings) {
    const logger = logger_helper.get_logger(project_id);
    const path = file.local_path;
    const source_path = file.source_path;

    let fileName = path.split('/').pop();
    let optimizedFilePath = path;
    let isFilePreOptimized = true;
    let methodName = "minifyjs";
    if (!path.endsWith('.min.js')) {
        if ( all_resources.includes(source_path.replace(/\.js$/g, '.min.js')) )
            optimizedFilePath = path.replace(/\.js$/g, `.${new Date().getTime()}.min.js`);
        else
            optimizedFilePath = path.replace(/\.js$/g, '.min.js');
        isFilePreOptimized = false;
    }

    return new Promise(function (resolve, reject) {

        if(isFilePreOptimized)
            return resolve(path);

        if (projectSettings['script.compression']) {
            logger.info(`[optimizer] minifying javascript file ${path}`);
            compressor.minify({
                compressor: "uglifyjs",
                input: path,
                output: optimizedFilePath,
                options: {
                    mangle: false,
                    compress: false
                },
                callback: function (err, min) {
                    if (!err) {

                        if(min == "undefined" || min == undefined){
                            let kerr = new kerror(`error while compressing javascript file ${path} due to output of file was undefined`, methodName);
                            logger.error(kerr);
                            return resolve(path);
                        }

                        logger.info(`minifying javascript file ${path} complete`);
                        return resolve(optimizedFilePath);
                    }
                    else {
                        logger.error(`error while compressing javascript file ${path} due to ${err}`);

                        let kerr = new kerror(`error while compressing javascript file ${path}`, methodName, err);
                        resolve(path);
                    }
                }
            });
        } else {
            logger.info(`[optimizer] Not compressing JS for ${path}, as script.compression is false`);
            return resolve(path);
        }
    })

};

exports.minifycss = function (file, all_resources, project_id, projectSettings) {
    const logger = logger_helper.get_logger(project_id);
    const path = file.local_path;
    const source_path = file.source_path;

    let fileName = path.split('/').pop();
    let optimizedFilePath = path;
    let isFilePreMinified = true;
    // If not 'min' in filename add it
    if (!path.endsWith('.min.css')) {
        if ( all_resources.includes(source_path.replace(/\.css$/g, '.min.css')) )
            optimizedFilePath = path.replace(/\.css$/g, `.${new Date().getTime()}.min.css`);
        else
            optimizedFilePath = path.replace(/\.css$/g, '.min.css');
        isFilePreMinified = false;
    }

    return new Promise(function (resolve, reject) {
        let methodName = 'minifycss';

        if(isFilePreMinified)
            return resolve(path);

        if (projectSettings['style.compression']) {
            compressor.minify({
                compressor: "csso",
                input: path,
                output: optimizedFilePath,
                options: {
                    restructureOff: true
                },
                callback: function (err, min) {
                    if (!err) {
                        logger.info(`[css] successful for file: ${path}`);
                        return resolve(optimizedFilePath);
                    }
                    else {
                        logger.error(`[css] error while compressing stylesheet ${path}`, methodName, err.message);
                        return resolve(path);
                    }
                }
            });
        } else {
            logger.info(`[optimizer] Not compressing CSS for ${path}, as style.compression is false`);
            return resolve(path);
        }
    })

};

exports.minifyhtml = function (path, project_id, projectSettings) {
    const logger = logger_helper.get_logger(project_id);

    return new Promise((resolve, reject) => {
        let methodName = "minifyhtml";

        if (projectSettings['html.compression']) {
            fileReader.readTextFromFile(path)
                .then((htmlstring) => {
                    let options =
                        {
                            removeAttributeQuotes: true,
                            minifyJS: true,
                            minifyCSS: true,
                            removeComments: true,
                            collapseWhitespace: true
                        };
                    try {
                        let result = htmlMinifier(htmlstring, options);
                        if (result) {
                            fileReader.writeToFile(result, path)
                                .then((path) => {
                                    resolve(path);
                                })
                                .catch((err) => {
                                    reject(new kerror(`error while writing to the file ${path}`, methodName, err));
                                })
                        } else if (htmlstring === '' || whiteSpace.test(htmlstring)) {  // If source was blank
                            resolve(path);
                        } else {
                            reject(new kerror(`Html Compressor sent back blank content for ${path}`, methodName));
                        }
                    } catch (err) {
                        logger.warn(`Unable to parse ${path} for html compression`);
                        resolve(path);
                    }
                })
                .catch((err) => {
                    reject(new kerror(`error while reading from file ${path}`, methodName, err));
                })
        } else {
            logger.info(`[optimizer] Not compressing HTML for ${path}, as html.compression is false`);
            resolve(path);
        }

    })
};
