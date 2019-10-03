const mongoose = require('mongoose');
const logger_helper = require('../helpers/logger');

const pathOfCurrentWorkingDirectory = process.env.CURRENT_WORKING_DIRECTORY;
const Enum = require('enum');


const ProjectStatus = new Enum({'IDLE':0, 'PUBLISHING':1, 'BUILDING':2, 'CRAWLING':3, 'QUEUED':4, 'PUBLISHINGERROR':-1, 'BUILDINGERROR':-2, 'ERROR':-3});

exports.ProjectStatus = ProjectStatus;

const ProjectType = new Enum({'ERROR':-1 , 'CRAWL':0 , 'DRAGANDDROP':1 , 'NEWPROJECT':2});

exports.ProjectType = ProjectType;

const BucketNames = new mongoose.Schema({
    source: {type: String, default: 'kitsune-buildtest-source'},
    demo: {type: String, default: 'kitsune-buildtest-demo'},
    placeholder: {type: String, default: 'kitsune-buildtest-placeholder'},
    production: {type: String, default: 'kitsune-buildtest-production'}
}, {_id: false});

const KitsuneProjectSchema = new mongoose.Schema({
    ProjectId: String,
    ProjectName: String,
    CreatedOn : Date,
    UserEmail: String,
    FaviconIconUrl: String,
    Version: {type: Number, default: 0},
    ProjectStatus: String,
    ProjectType: String,
    UpdatedOn: {type: Date, default: new Date().toISOString()},
    SchemaId: String,
    BucketNames: BucketNames
}, {_id: true});

const ResourceType = new Enum({'LINK': 'LINK', 'SCRIPT': 'SCRIPT', 'STYLE': 'STYLE', 'FILE': 'FILE', 'APPLICATION': 'APPLICATION'});

exports.ResourceType = ResourceType;

const PageType = new Enum({'DEFAULT': 'DEFAULT', 'LIST': 'LIST', 'DETAILS': 'DETAILS', 'SEARCH': 'SEARCH', 'PARTIAL': 'PARTIAL', 'LARAVEL' : 'LARAVEL'});

exports.PageType = PageType;

const KitsuneResourcesMetaData = new mongoose.Schema({
    Keywords : [String]
},{_id : false});


const KitsuneProjectResourceSchema = new mongoose.Schema({
    ProjectId: String,
    SourcePath: String,
    OptimizedPath: String,
    ClassName: String,
    UrlPattern: String,
    UrlPatternRegex: String,
    Version: {type: Number, default: 0},
    IsStatic: {type: Boolean, default: true},
    ResourceType: String,
    PageType: String,
    UpdatedOn: {type: Date, default: Date.now},
    PublishedVersion: Number,
    MetaData : KitsuneResourcesMetaData
});

KitsuneProjectResourceSchema
    .virtual('LocalPath')
    .get(function () {
        return `${process.env.OPTIMIZERLOCALSTORAGE}/${this.AbsoluteSourcePath}`;
    });
KitsuneProjectResourceSchema
    .virtual('AbsoluteSourcePath')
    .get(function () {
        let cwd = (this.IsStatic) ? '': pathOfCurrentWorkingDirectory;
        return this.ProjectId + cwd + this.SourcePath;
    });
KitsuneProjectResourceSchema
    .virtual('LocalOptimizedPath')
    .get(function() {
        return `${process.env.OPTIMIZERLOCALSTORAGE}/${this.AbsoluteOptimizedFilePath}`;
    })
    .set(function (value) {
        let path = value.split('/');
        path = path.slice(path.indexOf(this.ProjectId) + 1).join('/');
        if(path.indexOf('/') == 0 )
            this.OptimizedPath = `${path}`;
        else
            this.OptimizedPath = `/${path}`;
    });

KitsuneProjectResourceSchema
    .virtual('AbsoluteOptimizedFilePath')
    .get(function () {
        return this.ProjectId + pathOfCurrentWorkingDirectory + this.OptimizedPath;
    });

// I am sorry for this property name
KitsuneProjectResourceSchema
    .virtual('AbsoluteOptimizedUnmodifiedFilePath')
    .get(function () {
        return this.ProjectId + pathOfCurrentWorkingDirectory + this.SourcePath;
    });

const BuildStages = new Enum(
    {'Queued': 'Queued',
    'Compiled': 'Compiled',
    'QueuedBuild': 'QueuedBuild',
    'Analyzer': 'Analyzer',
    'Optimizer': 'Optimizer',
    'Replacer': 'Replacer',
    'Completed': 'Completed',
    'Error': 'Error'});

exports.BuildStages = BuildStages;

const CompilerFiles = new mongoose.Schema({
    LINK: {type: Number, default: 0},
    TOTAL: {type: Number, default: 0}
}, {_id: false});

exports.CompilerFiles = CompilerFiles;

const AnalyzerFiles = new mongoose.Schema({
    LINK: {type: Number, default: 0},
    STYLE: {type: Number, default: 0},
    TOTAL: {type: Number, default: 0},
    TOTAL_LINK: {type: Number, default: 0},
    TOTAL_STYLE: {type: Number, default: 0}
}, {_id: false});

exports.AnalyzerFiles = AnalyzerFiles;

const OptimizerFiles = new mongoose.Schema({
    LINK: {type: Number, default: 0},
    SCRIPT: {type: Number, default: 0},
    STYLE: {type: Number, default: 0},
    FILE: {type: Number, default: 0},
    TOTAL: {type: Number, default: 0},
    TOTAL_LINK: {type: Number, default: 0},
    TOTAL_STYLE: {type: Number, default: 0},
    TOTAL_SCRIPT: {type: Number, default: 0},
    TOTAL_FILE: {type: Number, default: 0}
}, {_id: false});

exports.OptimizerFiles = OptimizerFiles;

const ReplacerFiles = new mongoose.Schema({
    LINK: {type: Number, default: 0},
    STYLE: {type: Number, default: 0},
    TOTAL: {type: Number, default: 0},
    TOTAL_LINK: {type: Number, default: 0},
    TOTAL_STYLE: {type: Number, default: 0}
}, {_id: false});

exports.ReplacerFiles = ReplacerFiles;

const KError = new mongoose.Schema({
    Message: String,
    SourceMethod: String,
    ErrorStackTrace : String,
    Line : String,
    Column : String,
    SourcePath : String
}, {_id: false});

const KitsuneBuildStatusSchema = new mongoose.Schema({
    CreatedOn: {type: Date},
    ProjectId: String,
    BuildVersion : Number,
    Stage: String,
    IsCompleted : Boolean,
    Compiler: CompilerFiles,
    Analyzer: AnalyzerFiles,
    Optimizer: OptimizerFiles,
    Replacer: ReplacerFiles,
    Error : [KError],
    Warning: [KError]
}, {_id: true, versionKey: false});

// UNUSED
// KitsuneBuildStatusSchema.methods.incrementScriptsProcessed = function () {
//     const logger = logger_helper.get_logger(this.ProjectId);
//     this.model('KitsuneBuildStatus').update({ProjectId: this.ProjectId}, {$inc: {ScriptsProcessed: 1}})
//         .then(res => {
//             logger.info(`database successfully updated for scripts for ProjectId : ${this.ProjectId}`);
//         })
//         .catch(err => {
//             logger.error(`database updated failed for scripts for ProjectId : ${this.ProjectId}`);
//         });
// };
//
// KitsuneBuildStatusSchema.methods.incrementStylesProcessed = function () {
//     const logger = logger_helper.get_logger(this.ProjectId);
//     this.model('KitsuneBuildStatus').update({ProjectId: this.ProjectId}, {$inc: {StylesProcessed: 1}})
//         .then(res => {
//             logger.info(`database successfully updated for styles for ProjectId : ${this.ProjectId}`);
//         })
//         .catch(err => {
//             logger.error(`database updated failed for styles for ProjectId : ${this.ProjectId}`);
//         });
// };
//
// KitsuneBuildStatusSchema.methods.incrementLinksProcessed = function () {
//     const logger = logger_helper.get_logger(this.ProjectId);
//     this.model('KitsuneBuildStatus').update({ProjectId: this.ProjectId}, {$inc: {LinksProcessed: 1}})
//         .then(res => {
//             logger.info(`database successfully updated for links for ProjectId : ${this.ProjectId}`);
//         })
//         .catch(err => {
//             logger.error(`database updated failed for links for ProjectId : ${this.ProjectId}`);
//         });
// };
//
// KitsuneBuildStatusSchema.methods.incrementFilesProcessed = function () {
//     const logger = logger_helper.get_logger(this.ProjectId);
//     this.model('KitsuneBuildStatus').update({ProjectId: this.ProjectId}, {$inc: {FilesProcessed: 1}})
//         .then(res => {
//             logger.info(`database successfully updated for files for ProjectId : ${this.ProjectId}`);
//         })
//         .catch(err => {
//             logger.error(`database updated failed for files for ProjectId : ${this.ProjectId}`);
//         });
// };
//
// KitsuneBuildStatusSchema.methods.incrementFilesModified = function () {
//     const logger = logger_helper.get_logger(this.ProjectId);
//     this.model('KitsuneBuildStatus').update({ProjectId: this.ProjectId}, {$inc: {FilesModified: 1}})
//         .then(res => {
//             logger.info(`database successfully updated for FilesModified for ProjectId : ${this.ProjectId}`);
//         })
//         .catch(err => {
//             logger.error(`database updated failed for FilesModified for ProjectId : ${this.ProjectId}`);
//         });
// };

const KitsuneProject = mongoose.model('KitsuneProjects',KitsuneProjectSchema,
    process.env.COLLECTION_KitsuneProjects || 'KitsuneProjects');

exports.KitsuneProject = KitsuneProject;

const KitsuneProjectResources = mongoose.model('KitsuneResources', KitsuneProjectResourceSchema,
    process.env.COLLECTION_KitsuneResources || 'KitsuneResources');

exports.KitsuneProjectResources = KitsuneProjectResources;

const KitsuneBuildStatus = mongoose.model('KitsuneBuildStatus', KitsuneBuildStatusSchema,
    process.env.COLLECTION_KitsuneBuildStatus || 'KitsuneBuildStatus');

exports.KitsuneBuildStatus = KitsuneBuildStatus;