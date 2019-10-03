const mongoose = require('mongoose');

const FileDetails = mongoose.Schema({
    Keywords: [String],
    S3Url: String,
    Title: String,
    Description: String,
    Version: Number,
    LastModified: Date
}, {_id: false});

FileDetails.virtual('relative_path').get(function () {
    return this.S3Url;
});

const iFileDetails = mongoose.Schema({
    Keywords: [String],
    KitsuneLinkUrl: String,
    Title: String,
    Description: String,
    Version: Number,
    LastModified: Date
}, {_id: false});
iFileDetails.virtual('relative_path').get(function () {
    return this.KitsuneLinkUrl;
});

const kitsuneProjectSchema = new mongoose.Schema({
    Links: [FileDetails],
    Styles: [iFileDetails],
    Scripts: [iFileDetails],
    Assets: [iFileDetails],
    originalRobots: String,
    CrawlId: String,
    FaviconUrl: String,
    IsArchived: Boolean
});

kitsuneProjectSchema.virtual('s3_placeholder_bucket').get(function () {
    return 'kitsune-conversion';
});
kitsuneProjectSchema.virtual('s3_demo_bucket').get(function () {
    return 'kitsune-conversion-copy';
});

const KitsuneProjects = mongoose.model('KitsuneProject', kitsuneProjectSchema, 'KitsuneProjects');

exports.KitsuneProjects = KitsuneProjects;






// TODO Make new KitsuneProjects schema
// TODO Make new KitsuneProjectResources schema
// TODO Use dynamic document, ignore (on fetch, not on save)

// exports.KitsuneProjectResources = KitsuneProjectsResources;