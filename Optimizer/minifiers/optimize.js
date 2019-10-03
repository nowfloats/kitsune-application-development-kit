// const hasPlaceholders = require("../helpers/auditFile").hasPlaceholders;
const mongoose = require('mongoose');
const models = require('./../models/new_models');
const minifier = require("./minifyjs");
const imageCompressor = require('./compress_image');
const aws = require('./../helpers/aws');
const fs = require('fs');
const utility = require('./../helpers/utility');
const ObjectID = mongoose.Types.ObjectId;
const database = require('./../helpers/mongo');
const logger_helper = require('./../helpers/logger');
const kerror = require('./../helpers/error');
const replace = require('replace-in-file');
const rimraf = require('rimraf');
const analyzer = require('./../analyzer');
const path = require('path');
const RateLimiter = require('request-rate-limiter');
const asyncEach = require('async/eachLimit');

mongoose.Promise = global.Promise;

RegExp.escape = function(str) {
    return String(str).replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
};
exports.Optimize = class {
    constructor(ProjectId, messageProcessingCompleted) {
        this.clean_build = false;
        this.ProjectId = ProjectId;
        this.Project = null;

        // Default Settings
        this.ProjectSettings = {
            'cleanBuild': false,
            'seo.footer': true,
            'html.compression': true,
            'script.compression': true,
            'style.compression': true,
            'image.compression.png': true,
            'image.compression.jpg': true,
            'html.decodeEntities': true,
            'useMinFileExtensions': true,
            'appendBuildVersion' : true
        };
        // if USE_STATIC_ASSET_DOMAIN_MASK = false => use kit-cdn.com/project_id/version/
        // if USE_STATIC_ASSET_DOMAIN_MASK = true => use rootalias.in/
        this.ProjectSettings[process.env.USE_STATIC_ASSET_DOMAIN_MASK_KEY] = false;

        this.messageProcessingCompleted = messageProcessingCompleted;
        this.PlaceHolderBucket = "";
        this.SourceBucket = "";
        this.KitsuneResourcesUpdatedOnDate = new Date(0);
        this.IsDateSuccessfullyUpdated = false;
        this.DemoBucket = "";
        this.update_references = {
            update_from: [],
            update_to: []
        };
        this.processedFiles = 0;
        this.totalFiles = 0;
        this.files_to_replace = 0;
        this.files_replaced = 0;
        this.StyleProcessed = 0;
        this.ScriptsProcessed = 0;
        this.FilesProcessed = 0;
        this.HtmlProcessed = 0;
        this.IsError = false;
        this.BuildStats = {};
        this.KitsuneResources = [];
        this.BrokenLinkWarnings = [];
        this.rate_limited_requests = new RateLimiter({backoffCode: 502, rate: 200, backoffTime: 20});

        this.logger = logger_helper.set_logger(this.ProjectId);
        this.files_to_optimize = {};
        this.optimizer_timedout = false;
        this.replacer_called = false;

        this.optimizer_timeout = null;

        this.initialize();
    }

    async initialize() {
        // Connect to Database
        this.logger.info('initializing database connection');
        let connectionString = '';
        if (process.env.KITSUNE_ENV === 'DEV') {
            connectionString = process.env.MONGO_DEV_URI;
        } else if (process.env.KITSUNE_ENV === 'PROD') {
            connectionString = process.env.MONGO_PROD_URI;
        }

        await mongoose.connect(connectionString, {useNewUrlParser: true});

        try {
            // Get Project Settings
            const settings = await utility.GetProjectSettings(this.ProjectId);
            
            Object.keys(settings).forEach(setting => {
                
                this.ProjectSettings[setting] = settings[setting];
            });
            this.getAllDetailsFromDatabase();
        } catch (err) {
            this.logger.error(`Error in fetching Project Settings due to: ${err}`);
            await this.getAllDetailsFromDatabase();
        }
    }

    async getAllDetailsFromDatabase() {
        this.clean_build = this.ProjectSettings['cleanBuild'];

        (this.clean_build) ? this.logger.warn('[config] Force Building All') : null;

        try {
            const project = await database.getKitsuneProject(this.ProjectId);
            this.logger.info(`[base] successfully received KitsuneProject from database with Project._id Document : ${project._id}`);
            this.Project = project;
            this.PlaceHolderBucket = project.BucketNames.placeholder;
            this.SourceBucket = project.BucketNames.source;
            this.DemoBucket = project.BucketNames.demo;
            const dataObj = await database.getLastBuildDateForProject(this.ProjectId);

            this.logger.info(`[base] successfully received LastBuildDateForProject from KitsuneBuildStatus , ${this.ProjectId}`);

            if (dataObj.isSuccess)
                this.KitsuneResourcesUpdatedOnDate = (this.clean_build) ? new Date(0) : dataObj.date;

            this.logger.info(`[base] using Last Build date ${this.KitsuneResourcesUpdatedOnDate}`);
            const buildStat = await database.getKitsuneBuildStat(this.ProjectId);

            this.logger.info(`[base] successfully received buildstats with Id : ${buildStat._id} , ${this.ProjectId}`);
            this.BuildStats = buildStat;
            const resources = await models.KitsuneProjectResources.find({
                ProjectId: this.ProjectId
                // TODO: IsArchived: false, Don't do this yet. Also being used in conflict resolution in compressed filename
            }, {SourcePath: true, _id: false});

            this.logger.info(`[base] successfully received KitsuneResources with Id : ${this.ProjectId}`);
            this.KitsuneResources = resources.map(function (path) {
                return path.SourcePath;
            });
            let FaviconUrl = this.Project.FaviconIconUrl;
            if (FaviconUrl) {
                FaviconUrl = FaviconUrl.replace(new RegExp('.*' + this.ProjectId), '');
                this.KitsuneResources.push(FaviconUrl);
            }
            await this.start_analysis();
        } catch (err) {
            let kerr = new kerror('error while getting data from database', 'getAllDetailsFromDatabase', JSON.stringify(err));
            this.logger.error(kerr);
            this.logger.error(`Unable to fetch all details from database due to `, err);
            this.error_handler(kerr);
            database.updateKitsuneProjectStatus(this.ProjectId, models.ProjectStatus.BUILDINGERROR);
            this.messageProcessingCompleted();
        }
    }

    async start_analysis() {
        this.logger.info(`[analyzer] Starting for ${this.ProjectId}`);
        this.BuildStats.Stage = models.BuildStages.Analyzer.value;
        this.BuildStats.Analyzer = models.AnalyzerFiles;
        this.BuildStats.Analyzer.LINK = 0;
        this.BuildStats.Analyzer.STYLE = 0;
        await this.BuildStats.save();

        try {
            const resources = await models.KitsuneProjectResources.find({
                ProjectId: this.ProjectId,
                ResourceType: {$in: [models.ResourceType.LINK.value, models.ResourceType.STYLE.value]},
                UpdatedOn: {$gte: this.KitsuneResourcesUpdatedOnDate},
                IsArchived: false
            });

            this.logger.info(`[analyzer] Got ${resources.length} resources to add placeholders for ${this.ProjectId}`);

            this.files_to_replace = resources.length;
            this.files_replaced = 0;

            this.BuildStats.Analyzer.TOTAL = this.files_to_replace;
            this.BuildStats.Analyzer.TOTAL_LINK = resources.filter(res => res.ResourceType === models.ResourceType.LINK.value).length;
            this.BuildStats.Analyzer.TOTAL_STYLE = resources.filter(res => res.ResourceType === models.ResourceType.STYLE.value).length;
            await this.BuildStats.save();

            if (this.files_to_replace === 0) this.start_optimizer();
            await asyncEach(resources, process.env.ASYNC_EACH_LIMIT, (resource, cb) => {
                this.add_placeholders(resource, cb);
            });
        } catch(error) {
            if (error.ErrorMessage === 'No new resources to optimize') {
                this.logger.warn(`[database] ${error.ErrorMessage} for ProjectId ${this.ProjectId}`);
            }
            let kerr = new kerror(`[database] Couldn't get resources/project for Project  : ${this.ProjectId} ${error}`, 'start_analysis', error);
            if (this.BuildStats) {
                this.BuildStats.Stage = models.BuildStages.Error.value;
                await this.BuildStats.save();
            }

            this.logger.error(kerr);
            this.IsError = true;
            this.finished();
        }
    }

    add_to_brokenlinks(broken_links, file) {
        let BrokenLinks = Object.keys(broken_links);
        if (BrokenLinks.length !== 0) {
            let ErrorMessage = 'Broken Links: ' + BrokenLinks.toString();
            let warning = new kerror(ErrorMessage, 'Broken References Analyzer', '', file.SourcePath, 1, 1 );
            this.BrokenLinkWarnings.push(warning);
        }
    }

    async add_placeholders(file, cb) {
        try {
            let local_file = await aws.downloadFileFromS3({
                bucket: (file.IsStatic) ? this.SourceBucket : this.DemoBucket,
                key: file.AbsoluteSourcePath,
                localPath: file.LocalPath
            });

            // Add all placeholders
            const res = await new analyzer().process({local_file: local_file, source_path: file.SourcePath}, this.ProjectId,
                this.KitsuneResources, this.ProjectSettings);

            // this.logger.info(`[analyzer] Done with ${local_file} for ${this.ProjectId}| Guess Content Type`);
            // Guess content type
            local_file = res.local_file;
            this.add_to_brokenlinks(res.broken_links, file);
            let content_type = '';
            switch (file.ResourceType) {
                case models.ResourceType.LINK.value:
                    content_type = 'text/html';
                    break;
                case models.ResourceType.STYLE.value:
                    content_type = 'text/css';
                    break;
            }

            // Upload to S3
            const data = await aws.uploadToS3({
                bucket: this.PlaceHolderBucket,
                key: file.AbsoluteSourcePath,
                localPath: local_file,
                ContentType: content_type,
                DeleteLocalCopy: true
            });
            switch (file.ResourceType) {
                case models.ResourceType.LINK.value:
                    this.BuildStats.Analyzer.LINK++;
                    break;
                case models.ResourceType.STYLE.value:
                    this.BuildStats.Analyzer.STYLE++;
                    break;
            }

            // await this.BuildStats.save();
            this.update_analyzer_progress(cb);
        } catch (err) {
            this.logger.error(`[analyzer] Couldn't add placeholders in file ${file.SourcePath} due to ${err}`);
            this.update_analyzer_progress(cb);
        }
    }

    update_analyzer_progress(callback) {
        callback();
        this.files_replaced++;
        this.logger.info(`[analyzer] ${this.files_replaced} of ${this.files_to_replace} done`);
        this.files_to_replace === this.files_replaced ? this.finished_analyzing() : false;
    }

    warning_handler(warning) {
        new Promise((resolve, reject) => {
            if(this.BuildStats._id != undefined)
            {
                database.updateWarningInBuildStats(this.BuildStats._id, warning)
                .then(res => {
                    this.logger.info('successfully updated the database with response', res);
                })
                .
                catch(err => {
                    this.logger.error(`error while updating warnings in build stats`, err);
                })
            }
        })
    };

    update_warnings() {
        this.BrokenLinkWarnings.forEach((warning) => {
            this.warning_handler(warning);
        });
    }

    finished_analyzing() {
        // Reset vars for Replacer
        this.update_warnings();
        this.files_to_replace = 0;
        this.files_replaced = 0;
        rimraf(`temp/${this.ProjectId}/`, function (files) {
            this.logger.info(`[analyzer] Files deleted for ${this.ProjectId} `);
            this.logger.info('[analyzer] calling get_lassi');
            this.start_optimizer();
        }.bind(this));
        this.logger.info(`[analyzer] completed for ${this.ProjectId}`);
    }

    async start_optimizer() {
        this.logger.info(`[optimizer] starting for ${this.ProjectId}`);
        this.BuildStats.Stage = models.BuildStages.Optimizer.value;
        this.BuildStats.Optimizer = models.OptimizerFiles;
        await this.BuildStats.save();
        try {
            const resources = await database.getKitsuneProjectResources(this.ProjectId, this.KitsuneResourcesUpdatedOnDate);

            this.logger.info(`[optimizer] ${resources.length} Kitsune Resources Found to optimize for buildId : ${this.BuildStats._id} , ${this.ProjectId}`);

            if (resources.length != 0) {
                this.BuildStats.Optimizer.TOTAL = this.totalFiles = resources.length;
                this.BuildStats.Optimizer.TOTAL_LINK = resources.filter(res => res.ResourceType === models.ResourceType.LINK.value).length;
                this.BuildStats.Optimizer.TOTAL_STYLE = resources.filter(res => res.ResourceType === models.ResourceType.STYLE.value).length;
                this.BuildStats.Optimizer.TOTAL_SCRIPT = resources.filter(res => res.ResourceType === models.ResourceType.SCRIPT.value).length;
                this.BuildStats.Optimizer.TOTAL_FILE = resources.filter(res => res.ResourceType === models.ResourceType.FILE.value).length;
                await this.BuildStats.save();

                this.copyKitsuneAssetsToProjectS3Directory();

                asyncEach(resources, process.env.ASYNC_EACH_LIMIT, (resource, cb) => {
                    this.files_to_optimize[resource.SourcePath] = true;
                    this.logger.info('[optimizer] file type : ', resource.ResourceType);
                    switch (resource.ResourceType) {
                        case models.ResourceType.LINK.value:
                            this.process_html(resource, cb);
                            break;
                        case models.ResourceType.SCRIPT.value:
                            this.process_scripts(resource, cb);
                            break;
                        case models.ResourceType.STYLE.value:
                            this.process_styles(resource, cb);
                            break;
                        case models.ResourceType.FILE.value:
                            this.process_images(resource, cb);
                            break;
                        case models.ResourceType.APPLICATION.value:
                            this.process_images(resource, cb);
                            break;
                        default:
                            this.process_images(resource, cb);
                            //this.logger.info('[optimizer] unidentified files type: ', resource);
                            break;
                    }
                });
            }
            else {
                this.BuildStats.Stage = models.BuildStages.Completed.toString();
                this.BuildStats.IsCompleted = true;
                await this.BuildStats.save();
                database.updateKitsuneProjectStatus(this.ProjectId, models.ProjectStatus.IDLE);
                this.messageProcessingCompleted();
            }
        } catch (err) {
            this.logger.error(`[optimizer] error in get_lassi project id : ${this.ProjectId} , buildId : ${this.BuildStats._id} , ERROR : `, err);
            this.error_handler(err);
            this.messageProcessingCompleted();
        }
    }

    async process_html(file, cb) {
        this.logger.info(`[optimizer] processing html file : ${file._id}`, this.ProjectId);
        this.files_to_optimize[file.SourcePath] = 'DOWNLOAD_HTML';

        try {
            let path = await aws.downloadFileFromS3({
                bucket: this.PlaceHolderBucket,
                key: file.AbsoluteSourcePath,
                localPath: file.LocalPath
            });

            this.files_to_optimize[file.SourcePath] = 'GET_KEYWORDS_FROM_HTML';
            let keywords = await utility.getKeywordsFromHTMLFile(path, file.IsStatic, this.rate_limited_requests, this.ProjectId);
            this.files_to_optimize[file.SourcePath] = 'UPDATE_HTML_RESOURCE_DB_FOR_KEYWORDS';
            keywords = await database.updateKitsuneHtmlResourceWithKeywords(file, keywords, file.IsStatic);

            this.files_to_optimize[file.SourcePath] = 'INSERT_KEYWORDS_IN_HTML';
            let res = await utility.appendKeywordsToHTMLFile(file.LocalPath, keywords, file.IsStatic, this.ProjectSettings, this.ProjectId)

            this.files_to_optimize[file.SourcePath] = 'COMPRESS_HTML';
            if (file.IsStatic) {
                path = await minifier.minifyhtml(res, this.ProjectId, this.ProjectSettings);
            } else {
                path = res; // Don't compress dynamic files (in favor of better replacement at KLM. Compression will done by KLM)
            }

            this.files_to_optimize[file.SourcePath] = 'UPLOAD_HTML';
            file.LocalOptimizedPath = file.SourcePath;
            res = await aws.uploadToS3({
                bucket: this.PlaceHolderBucket,
                key: file.AbsoluteOptimizedFilePath,
                localPath: file.LocalPath,
                ContentType: 'text/html',
                DeleteLocalCopy: true
            });

            this.files_to_optimize[file.SourcePath] = 'INCREASE_LINKS_PROCESSED';
            this.logger.info(`[optimizer] successfully processed html, file id : ${file._id} for project id : ${this.ProjectId} , buildId : ${this.BuildStats._id}`);
            await this.increase_links_processed(file, cb);
            return database.updateKitsuneResource(file);
        } catch (err) {
            // console.log("error");
            this.files_to_optimize[file.SourcePath] = 'ERROR_CAUGHT_IN_HTML';
            this.error_handler(err);
            await this.increase_links_processed(file, cb);
            this.logger.error(`[optimizer] error in html file, file id : ${file._id} , project id : ${this.ProjectId} , buildId : ${this.BuildStats._id} , ERROR : `, err);
        }
    }

    process_styles(file, cb) {
        this.logger.info(`[optimizer] processing stylesheet ${file._id} for project id ${this.ProjectId}`);
        this.files_to_optimize[file.SourcePath] = 'DOWNLOAD_CSS';
        aws.downloadFileFromS3({
            bucket: this.PlaceHolderBucket,
            key: file.AbsoluteSourcePath,
            localPath: file.LocalPath
        }).then((res) => {
            this.files_to_optimize[file.SourcePath] = 'COMPRESS_CSS';
            return minifier.minifycss({local_path: res, source_path: file.SourcePath}, this.KitsuneResources, this.ProjectId, this.ProjectSettings)
        }).then((optimized_filepath) => {
            this.files_to_optimize[file.SourcePath] = 'UPLOAD_CSS';
            file.LocalOptimizedPath = optimized_filepath;
            return aws.uploadToS3({
                bucket: this.PlaceHolderBucket,
                key: file.AbsoluteOptimizedFilePath,
                localPath: optimized_filepath,
                ContentType: 'text/css',
                DeleteLocalCopy: file.Source === file.OptimizedPath
            }).then(data => {
                if (file.SourcePath !== file.OptimizedPath) {
                    return aws.uploadToS3({
                        bucket: this.PlaceHolderBucket,
                        key: file.AbsoluteOptimizedUnmodifiedFilePath,
                        localPath: optimized_filepath,
                        ContentType: 'text/css',
                        DeleteLocalCopy: true
                    })
                } else {
                    return data;
                }
            });
        }).then(() => {
            this.files_to_optimize[file.SourcePath] = 'INCREASE_CSS_PROCESSED';
            this.increase_style_processed(file, cb);
            if (!this.ProjectSettings['useMinFileExtensions']) {
                file.OptimizedPath = file.SourcePath;
            }
            database.updateKitsuneResource(file);
            this.logger.info(`[optimizer] stylesheet processing completed, file Id : ${file._id}, project id : ${this.ProjectId} , buildId : ${this.BuildStats._id}`);
        }).catch((err) => {
            this.files_to_optimize[file.SourcePath] = 'CAUGHT_ERROR_IN_CSS';
            this.logger.error(`[optimizer] error while processing stylesheet, file Id : ${file._id}, project id : ${this.ProjectId}, build Id : ${this.BuildStats._id}`, err);
            this.error_handler(file, err);
            this.increase_style_processed(file, cb);
        })
    }

    process_scripts(file, cb) {
        this.logger.info(`[optimizer] processing script with id : ${file._id} for project id :${this.ProjectId}`);
        this.files_to_optimize[file.SourcePath] = 'DOWNLOAD_JS';
        aws.downloadFileFromS3({
            bucket: this.SourceBucket,
            key: file.AbsoluteSourcePath,
            localPath: file.LocalPath
        }).then((res) => {
            this.files_to_optimize[file.SourcePath] = 'COMPRESS_JS';
            return minifier.minifyjs({local_path: res, source_path: file.SourcePath}, this.KitsuneResources, this.ProjectId, this.ProjectSettings);
        }).then((optimized_filepath) => {
            this.files_to_optimize[file.SourcePath] = 'UPLOAD_JS';
            file.LocalOptimizedPath = optimized_filepath;
            return aws.uploadToS3({
                bucket: this.DemoBucket,
                key: file.AbsoluteOptimizedFilePath,
                localPath: optimized_filepath,
                ContentType: 'application/javascript',
                DeleteLocalCopy: file.SourcePath === file.OptimizedPath // Delete if it won't be used
            }).then(data => {
                if (file.SourcePath !== file.OptimizedPath) {
                    return aws.uploadToS3({
                        bucket: this.DemoBucket,
                        key: file.AbsoluteOptimizedUnmodifiedFilePath,
                        localPath: optimized_filepath,
                        ContentType: 'application/javascript',
                        DeleteLocalCopy: true
                    })
                } else {
                    return data;
                }
            });

        }).then((data) => {
            this.files_to_optimize[file.SourcePath] = 'INCREASE_JS_PROCESSED';
            this.increase_scripts_processed(file, cb);
            if (!this.ProjectSettings['useMinFileExtensions']) {
                file.OptimizedPath = file.SourcePath;
            }
            database.updateKitsuneResource(file);
            this.logger.info(`[optimizer] script processing completed, file id : ${file._id} , project id : ${this.ProjectId}, build Id : ${this.BuildStats._id}`);
        }).catch((err) => {
            this.files_to_optimize[file.SourcePath] = 'ERROR_CAUGHT_IN_JS';
            this.logger.error(`[optimizer] error while processing script , file id : ${file._id} , project id : ${this.ProjectId}, build Id : ${this.BuildStats._id}`, err);
            this.increase_scripts_processed(file, cb);
            this.error_handler(err);
        })
    }

    process_images(file, cb) {
        if (file && (file.SourcePath.endsWith('.jpg') || file.SourcePath.endsWith('.jpeg') || file.SourcePath.endsWith('.png') )) {
            this.logger.info(`[optimizer] processing image with id : ${file._id} , Project Id : ${this.ProjectId}`);
            let fileExtension = file.SourcePath.split('.').pop().toLowerCase();
            this.files_to_optimize[file.SourcePath] = 'DOWNLOAD_IMAGE';
            aws.downloadFileFromS3({
                bucket: this.SourceBucket,
                key: file.AbsoluteSourcePath,
                localPath: file.LocalPath
            })
                .then((res) => {
                    this.files_to_optimize[file.SourcePath] = 'COMPRESS_IMAGE';
                    switch (fileExtension) {
                        case "jpg":
                        case "jpeg":
                            if (this.ProjectSettings['image.compression.jpg'] === true)
                                return imageCompressor.compress_jpg(res, this.ProjectId);
                            else
                                return res;
                        case "png":
                            if (this.ProjectSettings['image.compression.png'] === true)
                                return imageCompressor.compress_png(res, this.ProjectId);
                            else
                                return res;
                        default:
                            this.logger.error(`Unknown image type to optimized.
                             Got extension: ${fileExtension} from file: ${file.SourcePath}`);
                    }
                })
                .then((optimized_filepath) => {
                    this.files_to_optimize[file.SourcePath] = 'UPLOAD_IMAGE';
                    file.LocalOptimizedPath = optimized_filepath;
                    return aws.uploadToS3({
                        bucket: this.DemoBucket,
                        key: file.AbsoluteOptimizedFilePath,
                        localPath: optimized_filepath,
                        ContentType: `image/${fileExtension}`,
                        DeleteLocalCopy: file.SourcePath === file.OptimizedPath // Delete if it won't be used
                    }).then(data => {
                        if (file.SourcePath !== file.OptimizedPath) {
                            return aws.uploadToS3({
                                bucket: this.DemoBucket,
                                key: file.AbsoluteOptimizedUnmodifiedFilePath,
                                localPath: optimized_filepath,
                                ContentType: `image/${fileExtension}`,
                                DeleteLocalCopy: true
                            })
                        } else {
                            return data;
                        }
                    });
                })
                .then((data) => {
                    this.files_to_optimize[file.SourcePath] = 'INCREASE_IMAGE_PROCESSED';
                    this.increase_files_processed(file, cb);
                    database.updateKitsuneResource(file);
                    this.logger.info(`[optimizer] image successfully processed, file id : ${file._id} , Project Id : ${this.ProjectId} , build Id : ${this.BuildStats._id}`);
                })
                .catch((err) => {
                    this.files_to_optimize[file.SourcePath] = 'ERROR_CAUGHT_IN_JPEG';
                    this.logger.error(`[optimizer] error while processing image, file id : ${file._id} , Project Id : ${this.ProjectId}, build Id : ${this.BuildStats._id}`, err);
                    this.increase_files_processed(file, cb);
                    this.error_handler(err);
                })
        }
        else {
            if (!file.IsStatic) {
                // Skip Copy if Dynamic File && Resource Type: FILE
                this.increase_files_processed(file, cb);
                return
            }
            file.LocalOptimizedPath = file.SourcePath;
            let copyObjectRequest = {
                destinationBucket: this.DemoBucket,
                source: `${this.SourceBucket}/${encodeURIComponent(file.AbsoluteSourcePath)}`,
                destinationKey: file.AbsoluteOptimizedFilePath
            };
            this.files_to_optimize[file.SourcePath] = 'COPY_FILE_TO_DEMO';
            aws.copyObjectFromOneBucketToAnother(copyObjectRequest)
                .then((path) => {
                    this.files_to_optimize[file.SourcePath] = 'INCREASE_FILES_PROCESSED';
                    this.logger.info(`[optimizer] file successfully copied, file id : ${file._id} , Project Id : ${this.ProjectId}, buildId : ${this.BuildStats._id}`);
                    this.increase_files_processed(file, cb);
                    database.updateKitsuneResource(file);
                })
                .catch((err) => {
                    this.files_to_optimize[file.SourcePath] = 'ERROR_CAUGHT_IN_FILE';
                    this.increase_files_processed(file, cb);
                    this.logger.error(`[optimizer] error while copying , file id : ${file._id} , Project Id : ${this.ProjectId}, build Id ${this.BuildStats._id}`, err);
                    this.error_handler(err);
                })
        }
    }

    async increase_style_processed(file, cb) {
        this.StyleProcessed++;
        this.BuildStats.Optimizer.STYLE = this.StyleProcessed;
        // await this.BuildStats.save();
        this.check_if_completed(file, cb);
    }

    async increase_links_processed(file, cb) {
        this.HtmlProcessed++;
        this.BuildStats.Optimizer.LINK = this.HtmlProcessed;
        // await this.BuildStats.save();
        this.check_if_completed(file, cb);
    }

    async increase_scripts_processed(file, cb) {
        this.ScriptsProcessed++;
        this.BuildStats.Optimizer.SCRIPT = this.ScriptsProcessed;
        // await this.BuildStats.save();
        this.check_if_completed(file, cb);
    }

    async increase_files_processed(file, cb) {
        this.FilesProcessed++;
        this.BuildStats.Optimizer.FILE = this.FilesProcessed;
        // await this.BuildStats.save();
        this.check_if_completed(file, cb);
    }

    check_if_completed(file, callback) {
        callback();
        this.logger.info(`[optimizer] files optimized ${(this.ScriptsProcessed + this.StyleProcessed + this.HtmlProcessed + this.FilesProcessed)} of ${this.totalFiles}`);
        this.logger.debug(`[optimizer_done] for file ${file.SourcePath}`);
        (!this.optimizer_timedout) ? delete this.files_to_optimize[file.SourcePath] : null;

        clearTimeout(this.optimizer_timeout);
        this.optimizer_timeout = setTimeout(() => {
            this.IsError = true;
            this.optimizer_timedout = true;
            this.logger.error('[timeout]', {
                'files_left': this.files_to_optimize
            });
            rimraf(`temp/${this.ProjectId}/`, function () {
                (!this.replacer_called) ? this.replacer() : this.logger.error('[optimizer] Duplicate invocation of replacer from TIMEOUT');
            }.bind(this));
        }, process.env.BUILD_TIMEOUT*60*1000);

        if (this.totalFiles === (this.ScriptsProcessed + this.StyleProcessed + this.HtmlProcessed + this.FilesProcessed)) {
            clearTimeout(this.optimizer_timeout);
            this.logger.info(`[optimizer] complete for ${this.ProjectId}`);
            rimraf(`temp/${this.ProjectId}/`, function () {
                (!this.replacer_called) ? this.replacer() : this.logger.error('[optimizer] Duplicate invocation of replacer from ALL FILES DONE');
            }.bind(this));
        }
    }

    error_handler(err) {
        // this.logger.warn(`[error handler] Not handling`, err);
        new Promise((resolve, reject) => {
            this.IsError = true;
            if (typeof err === 'object' && this.BuildStats._id != undefined) {
                if (!err.ErrorMessage)
                    err = new kerror('unknown error', "unknown origin", JSON.stringify(err));

                database.updateErrorInBuildStats(this.BuildStats._id, err)
                    .then(res => {
                        this.logger.info('successfully updated the database with response', res);
                    })
                    .catch(err => {
                        this.logger.error(`error while updating errors in build stats`, err);
                    })
            }

        })
    };

    copyKitsuneAssetsToProjectS3Directory() {
        utility.addKitsuneCSSAssetsToProjectFolderS3(this.ProjectId)
            .then(file => {
                aws.uploadToS3({
                    bucket: this.DemoBucket,
                    key: file.key,
                    localPath: file.localPath,
                    ContentType: 'text/css',
                    DeleteLocalCopy: true
                })
            })
            .then(res => {
                this.logger.info(`css asset successfully uploaded to the project directory in s3 projectId : ${this.ProjectId}`, res);
            })
            .catch(err => {
                let kerr = new kerror(`error while moving css asset to the project directory in s3 projectId : ${this.ProjectId}`, 'copyKitsuneAssetsToProjectS3Directory', err);
                this.logger.error(kerr);
            });

        utility.addKitsuneJSAssetsToProjectFolderS3(this.ProjectId)
            .then(file => {
                aws.uploadToS3({
                    bucket: this.DemoBucket,
                    key: file.key,
                    localPath: file.localPath,
                    ContentType: 'application/javascript',
                    DeleteLocalCopy: true
                })
            })
            .then(res => {
                this.logger.info(`js asset successfully uploaded to the project directory in s3 projectId : ${this.ProjectId}`, res);
            })
            .catch(err => {
                let kerr = new kerror(`error while moving js asset to the project directory in s3 projectId : ${this.ProjectId}`, 'copyKitsuneAssetsToProjectS3Directory', err);
                this.logger.error(kerr);
            });
    }

    get_reference_updates() {
        return new Promise((resolve, reject) => {
            models.KitsuneProjectResources.find({
                ProjectId: this.ProjectId,
                OptimizedPath: {$ne: null},
                ResourceType: {$in: [models.ResourceType.STYLE.value, models.ResourceType.SCRIPT.value, models.ResourceType.FILE.value]},
                $where: 'this.SourcePath !== this.OptimizedPath'
            }, 'SourcePath OptimizedPath').then(function (resources) {
                this.update_references.update_from = resources.map(resource => {
                    return resource.SourcePath;
                });
                this.update_references.update_to = resources.map(resource => {
                    return resource.OptimizedPath;
                });
                this.logger.info(`[replacer] Got ${resources.length} reference updates from DB`);
                resolve()
            }.bind(this)).catch(err => {
                reject(err);
            });
        });
    }

    async replacer() {
        try {
            this.replacer_called = true;
            this.BuildStats.Stage = models.BuildStages.Replacer.value;
            this.BuildStats.Replacer = models.ReplacerFiles;
            this.BuildStats.Replacer.LINK = 0;
            this.BuildStats.Replacer.STYLE = 0;
            await this.BuildStats.save();
            await this.get_reference_updates();
            let preview_cdn_link = process.env.ASSET_CDN_LINK + this.ProjectId + '/cwd';

            let updatePathsFrom = this.update_references.update_from
            let updatePathsTo = this.update_references.update_to

            updatePathsFrom.push.apply(updatePathsFrom, updatePathsFrom.map(path => preview_cdn_link + path))
            updatePathsTo.push.apply(updatePathsTo, updatePathsTo.map(path => preview_cdn_link + path))

            // Placeholders with Base64 encoded path
            this.update_references_from = updatePathsFrom.map(path =>
                new RegExp(RegExp.escape(`[Kitsune:base64_${new Buffer(path).toString('base64')}]`), 'g'))
            this.update_references_to = updatePathsTo.map(path =>
                `[Kitsune:base64_${new Buffer(path).toString('base64')}]`)

            // Old Placeholders for Backwards Compatibility
            this.update_references_from.push.apply(this.update_references_from,
                updatePathsFrom.map(path => new RegExp(RegExp.escape(`[Kitsune_${path}]`), 'g')))
            this.update_references_to.push.apply(this.update_references_to,
                updatePathsTo.map(path => `[Kitsune_${path}]`))

            this.logger.info(`[replacer] Need to update ${this.update_references_from.length} references`);
            this.logger.info(`[replacer] From ${this.update_references_from}`);
            this.logger.info(`[replacer] To ${this.update_references_to}`);

            try {
                // Open all HTMLs and CSSs
                const resources = await models.KitsuneProjectResources.find({
                    ProjectId: this.ProjectId,
                    ResourceType: {$in: [models.ResourceType.LINK.value, models.ResourceType.STYLE.value]},
                    IsArchived: false
                });
                this.files_to_replace = resources.length;
                if (this.files_to_replace === 0) this.finished_replacing();

                this.BuildStats.Replacer.TOTAL = resources.length;
                this.BuildStats.Replacer.TOTAL_LINK = resources.filter(res => res.ResourceType === models.ResourceType.LINK.value).length;
                this.BuildStats.Replacer.TOTAL_STYLE = resources.filter(res => res.ResourceType === models.ResourceType.STYLE.value).length;
                await this.BuildStats.save();

                asyncEach(resources, process.env.ASYNC_EACH_LIMIT, (resource, cb) => {
                    this.replace_placeholders(resource, cb);
                });
            } catch (err) {
                this.logger.error(`[replacer] Error in fetching project resources due to ${err.stack}`);
            }
        } catch (err) {
            this.logger.error(`[replacer] Error in Getting references to update from DB due to ${err.stack}`);
            this.error_handler(err.stack);
        }
    }

    async replace_placeholders(file, cb) {
        try {
            let local_file = await aws.downloadFileFromS3({
                bucket: this.PlaceHolderBucket,
                key: file.AbsoluteOptimizedFilePath,
                localPath: file.LocalOptimizedPath
            })

            // Update References
            let changedFile = await replace({
                files: local_file,
                from: this.update_references_from,
                to: this.update_references_to,
                allowEmptyPaths: true
            })

            this.logger.info(`[replacer] Updated references in ${local_file} {${changedFile.length > 0}} for Project: ${this.ProjectId}`);
            // Remove all placeholders
            let replace_prefix = '';

            // For Websites with Subpath to work
            this.logger.info(`[replacer] get_path_to_home before vvv`);
            if (file.IsStatic && this.ProjectSettings[process.env.USE_STATIC_ASSET_DOMAIN_MASK_KEY]) {
                this.logger.warn(`[replacer] > get_path_to_home for ${file.OptimizedPath}`);
                this.logger.warn(`[replacer] > get_path_to_home as file.IsStatic == ${file.IsStatic}`);
                this.logger.warn(`[replacer] > get_path_to_home as UseMaskedCDNKey == ${process.env.USE_STATIC_ASSET_DOMAIN_MASK_KEY}`);
                this.logger.warn(`[replacer] > get_path_to_home as UseMaskedCDNValue == ${this.ProjectSettings[process.env.USE_STATIC_ASSET_DOMAIN_MASK_KEY]}`);
                replace_prefix = `${this.get_path_to_home(file.OptimizedPath)}`;
            }
            this.logger.info(`[replacer] get_path_to_home after ^^^`);

            // this.placeholder_pattern = /\[Kitsune_(.+?)]/g;  // Hack to handle all edge cases
            // TODO: Use this with testing -----------^ ^--- greedy search may cause issue in nested
            changedFile = await replace({
                files: local_file,
                from: [/\[Kitsune_(\b([-a-zA-Z0-9@:%_+.~#?&/=\s,;()]+))]/g, /\[Kitsune:base64_([a-zA-Z0-9+/=\s]+)]/g],
                to: (...args) => {
                    let matchedGroup = args[1];
                    if (args[0].startsWith('[Kitsune:base64_')) {
                        matchedGroup = new Buffer(matchedGroup, 'base64').toString()
                    }
                    
                    if(this.ProjectSettings['appendBuildVersion']){
                        return replace_prefix + matchedGroup + '?v=' + this.BuildStats.BuildVersion
                    }
                    else{
                        return replace_prefix + matchedGroup
                    }
                },
                allowEmptyPaths: true
            })
            this.logger.info(`[replacer] Removed placeholders in ${local_file} {${changedFile.length > 0}} for Project: ${this.ProjectId}`);

            // Guess content type
            let content_type = '';
            switch (file.ResourceType) {
                case models.ResourceType.LINK.value:
                    content_type = 'text/html';
                    break;
                case models.ResourceType.STYLE.value:
                    content_type = 'text/css';
                    break;
            }

            // // Test if still has placeholders, else discard upload and throw error
            // if (await hasPlaceholders(file.LocalOptimizedPath)) {
            //     this.logger.error(`[replacer] ✘ Placeholders found in file ${file.LocalOptimizedPath} = true`)
            //     throw new Error(`File: ${file.LocalOptimizedPath} still has placeholder, after replacement`)
            // } else {
            //     this.logger.info(`[replacer] ✔ No Placeholders in file ${file.LocalOptimizedPath}`)
            // }

            // Upload to S3
            let data = await aws.uploadToS3({
                bucket: this.DemoBucket,
                key: file.AbsoluteOptimizedFilePath,
                file: file,
                localPath: file.LocalOptimizedPath,
                ContentType: content_type,
                DeleteLocalCopy: file.SourcePath === file.OptimizedPath  // Delete if won't be used further
            });

            if (file.SourcePath !== file.OptimizedPath) {
                data = await aws.uploadToS3({
                    bucket: this.DemoBucket,
                    key: file.AbsoluteOptimizedUnmodifiedFilePath,
                    file: file,
                    localPath: file.LocalOptimizedPath,
                    ContentType: content_type,
                    DeleteLocalCopy: true
                });
            }

            try {
                switch (file.ResourceType) {
                    case models.ResourceType.LINK.value:
                        this.BuildStats.Replacer.LINK++;
                        break;
                    case models.ResourceType.STYLE.value:
                        this.BuildStats.Replacer.STYLE++;
                        break;
                }
                await this.BuildStats.save();
            } catch (e) {
                this.logger.info(`[replacer] Encountered ${e} while updating BuildStats`);
            }
            this.update_replacer_progress(cb);
        } catch (err) {
            this.logger.error(`[replacer] Couldn't replace placeholders in file ${file.LocalOptimizedPath} due to ${err}`);
            await this.error_handler(err)
            this.update_replacer_progress(cb)
        }
    }

    get_path_to_home(filepath) {
        this.logger.warn(`[replacer] > get_path_to_home invoked for ${filepath}`);
        let file_directory = path.dirname(filepath);
        (file_directory === '/') ? file_directory : file_directory += '/';
        let path_to_home = '';
        let count = (file_directory.match(/\//g) || []).length;
        if (count === 0 || count === 1) {
            path_to_home = ".";
        }
        else if (count >= 2) {
            path_to_home = "..";
            count = count - 2;
            while (count > 0) {
                path_to_home = "../" + path_to_home;
                count--;
            }
        }
        return path_to_home;
    }

    update_replacer_progress(callback) {
        callback();
        this.files_replaced++;
        this.logger.info(`[replacer] ${this.files_replaced} of ${this.files_to_replace} done`);
        this.files_to_replace === this.files_replaced ? this.finished_replacing() : false;
    }

    async finished_replacing() {
        this.logger.info('[replacer] Completed replacing!');
        rimraf(`temp/${this.ProjectId}/`, function (files) {
            this.logger.info(`[replacer] Files deleted for ${this.ProjectId} `);
        }.bind(this));
        this.BuildStats.UpdatedOn = new Date().toUTCString();
        this.BuildStats.Stage = (this.IsError) ? models.BuildStages.Error.value : models.BuildStages.Replacer.value; // TODO: Change this to Transpiler.value
        await this.BuildStats.save();
        this.finished();
    }

    finished() {
        this.messageProcessingCompleted();
        let status = this.IsError ? models.ProjectStatus.BUILDINGERROR : models.ProjectStatus.IDLE;
        try {
            if (this.optimizer_timedout) {
                this.logger.error('[missed_files]', {
                    'length': Object.keys(this.files_to_optimize).length,
                    'files_left': this.files_to_optimize
                });
            }
        } catch (e) {
            this.logger.error('Unable to process missed files at finish');
        }

        if (status === models.ProjectStatus.BUILDINGERROR) {
            database.updateKitsuneProjectStatus(this.ProjectId, status)
                .then(res => {
                    this.logger.info(`kitsune project status updated for build id : ${this.BuildStats._id}`);
                })
                .catch(err => {
                    let kerr = new kerror(`error while updating the kitsune project at service completion`, 'finished_replacing', err)
                    this.logger.error(kerr);
                });
        } else {
            // Transpiler to Take Over
            this.logger.info(`Done for ProjectId: ${this.ProjectId} with status: ${status}`);

            // TODO: Send to Transpiler Queue
            utility.UpdateWebEngageForOptimizationCompletion(this.ProjectId)
                .then(res=>{
                    this.logger.info(`API successfully called for updating WebEnagage for build id : ${this.BuildStats._id}`);
                })
                .catch(err=>{
                    this.logger.error(`buildId : ${this.BuildStats._id} , error :${err}`);
                });
        }
    }
};

// new exports.Optimize('59ad055d61d9cd73f00e084e', function () {
//     console.log('Done...');
// });
