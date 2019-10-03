const fs = require('fs');
const imagemin = require('imagemin');
const imageminJpegtran = require('imagemin-jpegtran');
const imageminPngtran = require('imagemin-pngquant');
const logger_helper = require('./../helpers/logger');

const aws = require('./../helpers/aws');

function round(number, precision) {
    let factor = Math.pow(10, precision);
    let tempNumber = number * factor;
    let roundedTempNumber = Math.round(tempNumber);
    return roundedTempNumber / factor;
};

function getFilesizeInKBytes(filename) {
    const stats = fs.statSync(filename);
    const fileSizeInBytes = stats.size;
    return round(fileSizeInBytes / 1024, 2);
}

exports.compress_jpg = (local_filename, project_id) => {
    /**
     * Convert to progressive jpg
     */
    const logger = logger_helper.get_logger(project_id);
    return new Promise((resolve, reject) => {
        var filePath = local_filename.split('/');
        filePath.pop();
        filePath = filePath.join('/');

        imagemin([local_filename], filePath, {
            plugins: [imageminJpegtran({
                progressive: true
            })]
        }).then(files => {
            logger.info(`[jpg] Compressed jpg: ${local_filename}`);
            files.forEach(file => {
                resolve(file.path);
            });
        }).catch(err => {
            logger.warn(`Unable to compress image ${local_filename} due to `, err);
            resolve(local_filename);
        });
    })
}

exports.compress_png = (local_filename, project_id) => {
    /**
     * Compress PNG
     */
    const logger = logger_helper.get_logger(project_id);
    return new Promise((resolve, reject) => {
        var filePath = local_filename.split('/');
        filePath.pop();
        filePath = filePath.join('/');

        imagemin([local_filename], filePath, {
            plugins: [imageminPngtran({
                quality: process.env.PNG_COMPRESSION_QUALITY
            })]
        }).then(files => {
            logger.info(`[png] Compressed png: ${local_filename}`);
            files.forEach(file => {
                resolve(file.path);
            });
        }).catch(err => {
            logger.warn(`Unable to compress image ${local_filename} due to `, err);
            resolve(local_filename);
        });
    })
}



