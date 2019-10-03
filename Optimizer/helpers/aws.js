const AWS = require('aws-sdk');
AWS.config.update({
    region: process.env.AWS_REGION,
    useAccelerateEndpoint: true
});

const Promise = require('promise');
const fs = require('fs');
const mkdirp = require('mkdirp');
const logger_helper = require('./logger');
const logger = logger_helper.logger;
const s3 = new AWS.S3();
const sqs = new AWS.SQS({region: process.env.AWS_REGION});
const optimizerQueue = process.env.KITSUNEOPTIMIZERSQS;
const kerror = require('./../helpers/error');

/**
 * creates read stream from the file
 * @param s3_url
 * @returns {Promise}
 */
let readStreamFromFile = function (path) {
    return new Promise(function (resolve, reject) {

        let stream = fs.ReadStream(path);
        if (stream) {
            resolve(stream, path);
        }
        reject("unable to read from the file");

    });
};

exports.readTextFromFile = function (path) {
    return new Promise((resolve, reject) => {
        fs.readFile(path, 'utf-8', (err, data) => {
            if (!err)
                resolve(data)
            else
                reject(err)
        });
    });
};

exports.writeToFile = function (dataToBeWritten, path) {
    return new Promise((resolve, reject) => {
        fs.writeFile(path, dataToBeWritten, (err) => {
            if (err)
                reject(err);
            else
                resolve(path);
        })

    })
};

exports.uploadToS3 = function (s3_file) {
    return new Promise((resolve, reject) => {
        const key_copy = s3_file.key;
        const logger = logger_helper.get_logger(key_copy.split('/')[0]);

        let methodName = "uploadToS3";

        if (typeof s3_file != 'object' || typeof s3_file.key === 'undefined'  ||
            typeof s3_file.bucket === 'undefined' || typeof s3_file.ContentType === 'undefined' ||
            typeof s3_file.localPath === 'undefined') {
            logger.error(`Unable to upload ${s3_file}`);
            return reject(new kerror('parameters not valid', methodName));
        }

        logger.info("uploading : ", s3_file);
        readStreamFromFile(s3_file.localPath)
            .then((stream) => {
                let params = {
                    Bucket: s3_file.bucket,
                    Key: s3_file.key,
                    ACL: "public-read",
                    Body: stream,
                    ContentType: s3_file.ContentType
                };
                s3.upload(params, function (err, data) {
                    if (!err) {
                        logger.info("upload completed : ", s3_file);
                        if (s3_file.DeleteLocalCopy) {
                            fs.unlink(s3_file.localPath, () => {
                                logger.info('[post upload] deleted ', s3_file.localPath);
                            });
                        }
                        resolve();
                    }
                    else {
                        let kerr = new kerror(`error while uploading file to s3`, methodName, err);
                        logger.error(kerr, s3_file);
                        reject(err);
                    }
                });
            })
            .catch((err) => {
                let kerr = new kerror('error while creating read stream', methodName, err);
                logger.error(kerr);
                reject(kerr);
            });
    });
};

exports.downloadFileFromS3 = function (s3_file) {
    return new Promise(function (resolve, reject) {
        const key_copy = s3_file.key;
        const logger = logger_helper.get_logger(key_copy.split('/')[0]);
        let methodName = "downloadFileFromS3";

        if (typeof s3_file != 'object' || typeof s3_file.key === 'undefined' || typeof s3_file.bucket === 'undefined' && typeof s3_file.localPath === 'undefined') {
            logger.error(`Unable to download ${s3_file}`);
            return reject(new kerror('parameters not valid', methodName));
        }

        logger.info(`downloading file : `, s3_file);
        let key = s3_file.key.replace(/^(\/+)/, "");
        let localPath = s3_file.localPath;
        const params = {
            Bucket: s3_file.bucket,
            Key: key
        };
        let directory = localPath.split('/').slice(0, -1).join('/');
        mkdirp(directory, (err) => {
            if (err) {
                return reject(new kerror('error while creating directory', methodName, err));
            }
            const s3Stream = s3.getObject(params).createReadStream();
            const fileStream = fs.createWriteStream(localPath);
            s3Stream.on('error', reject);
            fileStream.on('error', reject);
            fileStream.on('close', () => {
                resolve(localPath);
                logger.info('download complete : ', s3_file);
            });
            s3Stream.pipe(fileStream);
        });
    });
};

exports.getMessageFromSQS = function () {
    return new Promise((resolve, reject) => {

        let params = {
            QueueUrl: optimizerQueue,
            MaxNumberOfMessages: 1,
            WaitTimeSeconds: 10
            // add the visibility timeout
        };
        sqs.receiveMessage(params, function (err, data) {
            if (err) reject(err);
            else {
                if (data.Messages) {
                    resolve(JSON.parse(data.Messages.pop()));
                } else {
                    reject('no messages available');
                }
            }
        });
    });
};

exports.sendMessageToSQS = function (message) {
    return new Promise((resolve, reject) => {
        if (message) {

            if (!(typeof message === 'string'))
                message = JSON.stringify(message);

            let params = {
                MessageBody: message,
                QueueUrl: optimizerQueue,
                DelaySeconds: 0,
            };
            sqs.sendMessage(params, function (err, data) {
                if (err) reject(err);
                else     resolve(data);
            });
        }
        else {
            reject('message not valid');
        }
    })
};

exports.deleteMessageFromSQS = function (receiptHandle) {
    return new Promise((resolve, reject) => {
        if (receiptHandle && receiptHandle.length != 0) {

            let params = {
                QueueUrl: optimizerQueue,
                ReceiptHandle: receiptHandle
            };
            sqs.deleteMessage(params, function (err, data) {
                if (err) reject(err);
                else     resolve(data);
            });

        } else {
            reject('ReceiptHandle not valid');
        }
    })

};

/**
 *
 * @param receiptHandle
 * @param visibilityTimeoutValue in seconds
 * @returns {*|Promise}
 */
exports.updateMessageVisibiltyTimeoutPeriod = function (receiptHandle, visibilityTimeoutValue) {
    return new Promise((resolve, reject) => {
        if (receiptHandle && parseInt(visibilityTimeoutValue) >= 0 && receiptHandle.length != 0) {
            let params = {
                QueueUrl: optimizerQueue,
                ReceiptHandle: receiptHandle,
                VisibilityTimeout: parseInt(visibilityTimeoutValue)
            };
            sqs.changeMessageVisibility(params, function (err, data) {
                if (err) reject(err);
                else     resolve(data);
            });
        } else {
            reject(`invalid parameters receiptHandle : ${receiptHandle} && visibility timeout period : ${visibilityTimeoutValue}`)
        }
    })
};

exports.copyObjectFromOneBucketToAnother = function (file) {
    return new Promise((resolve, reject) => {
        if (file.destinationBucket && file.destinationKey && file.source) {
            let params = {
                Bucket: file.destinationBucket,
                CopySource: file.source,
                Key: file.destinationKey,
                ACL: "public-read-write"
            };
            s3.copyObject(params, function (err, data) {
                if (err) {
                    reject(err);
                }
                else {
                    resolve(data);
                }
            });
        }
        else {
            reject("arguments not valid")
        }
    })
};