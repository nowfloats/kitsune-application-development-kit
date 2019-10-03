const cheerio = require('cheerio');
const fileHandler = require('./aws');
const kerror = require('./../helpers/error');
const logger_helper = require('./../helpers/logger');

const download = require('download');
const request = require('request');
const aws = require('./../helpers/aws');
const Client =  require('node-rest-client').Client;
const fs = require('fs');
const keywordLink = process.env.KSEARCH;
const kScriptTag = process.env.KSCRIPTTAG;
const kMetaTag = process.env.KITSUNEMETATAG;
const kitsuneScriptAssetTag = process.env.KITSUNECSSASSETSTAG;
const kitsuneStyleAssetTag = process.env.KITSUNEJSASSETSTAG;

const jsAsset = process.env.KITSUNEJSASSETS;
const cssAsset = process.env.KITSUNECSSASSETS;
const localPathOfFiles = process.env.OPTIMIZERLOCALSTORAGE;
const currentWorkingDirectoryInS3 = process.env.CURRENT_WORKING_DIRECTORY;

exports.getKeywords = function (text, limiter) {
    return new Promise((resolve, reject) => {
        let options =
            {
                method: "POST",
                url: process.env.KEYWORDSAPI,
                body: text
            };

        limiter.request(options).then(response => {
            try {
                resolve(JSON.parse(response.body));
            } catch (err) {
                reject(`[keywords] error: ${err} body: ${response.body}`);
            }
        }).catch(err => {
            reject(err);
        });
    });
};

exports.getKeywordsFromHTMLFile = function (path , isStatic, limiter, project_id) {
    const logger = logger_helper.get_logger(project_id);

    return new Promise((resolve,reject)=>{

        if(!isStatic)
            return resolve([]);

        let methodName = "getKeywordsFromHTMLFile";

        if(typeof path !== 'string')
            return reject(new kerror('parameters not valid',methodName));

        fileHandler.readTextFromFile(path)
            .then((htmlString)=>{
                const $ = cheerio.load(htmlString, {_useHtmlParser2: true});
                let htmltext = $.text();
                if(htmltext){
                    let maxWaitForKeywords = setTimeout(() => {
                        logger.error(`Max wait for keywords fetch exceeded. Timing out.`);
                        resolve([]);
                    }, process.env.KEYWORDS_SLA_TIME);
                    this.getKeywords(htmltext, limiter)
                        .then((keywords)=>{
                            clearTimeout(maxWaitForKeywords);
                            resolve(keywords);
                        })
                        .catch((err)=>{
                            clearTimeout(maxWaitForKeywords);
                            logger.error(`error while getting keywords from html file ${path} due to ${err}`);
                            resolve([]);
                        })
                }
                else {
                    logger.error(new kerror(`error while getting text from html file ${path}`, methodName));
                    resolve([]);
                }
            })
            .catch((err)=>{
                reject(new kerror('error while reading html file from local disk', methodName,err));
            })
    });
};

exports.appendKeywordsToHTMLFile = function (path, keywords , isStatic, projectSettings, projectId) {
    return new Promise((resolve,reject)=>{

        if(!isStatic)
            return resolve(path)

        let methodName = "appendKeywordsToHTMLFile";

        if(typeof path != 'string' || typeof keywords != 'object')
            return reject(new kerror('parameters not valid' , methodName));


        fileHandler.readTextFromFile(path)
            .then((htmlString) => {
                let MetaTag = kMetaTag.replace('{keywords}', keywords.join(','));
                const $ = cheerio.load(htmlString,
                    {decodeEntities: projectSettings['html.decodeEntities'],
                    _useHtmlParser2: true});

                $('head').append(MetaTag);
                if (projectSettings['seo.footer']) {
                    let arrayOfKeywords = [];
                    keywords.forEach((x) => {
                        let keyword = x.trim().replace(/\s+/g, '+');
                        let element = {};
                        element.name = x;
                        element.href = keywordLink.replace('{keyword}', keyword);
                        arrayOfKeywords.push(element);
                    });

                    let ScriptTag = kScriptTag.replace('{keywords}', JSON.stringify(arrayOfKeywords));

                    $('html').append(ScriptTag);
                    let $body = $('body');
                    var basepath = process.env.ASSET_CDN_LINK + projectId + '/cwd';
                    $body.append(kitsuneScriptAssetTag.replace('{basePath}', basepath));
                    $body.append(kitsuneStyleAssetTag.replace('{basePath}', basepath));
                } else {
                    console.log(`[footer] Not adding keywords footer because seo.footer is ${projectSettings['seo.footer']}`);
                }
                fileHandler.writeToFile($.html(), path)
                    .then((x)=>{
                        resolve(x);
                    })
                    .catch((err)=>{
                        reject(new kerror('error while writing to the html file',methodName,err));
                    })

            })
            .catch((err) => {
                reject(new kerror('error reading html file from local disk',methodName,err));
            })
    })
};

exports.addKitsuneCSSAssetsToProjectFolderS3 = function (projectId) {
    return new Promise((resolve,reject)=>{
        download(cssAsset)
            .then(data=>{
                // TODO replace with performance value
                let fileName = cssAsset.split('/').pop();
                let path = `${localPathOfFiles}/${fileName}`;
                fs.writeFileSync(path,data);
                let key = `${projectId}${currentWorkingDirectoryInS3}/${fileName}`;
                resolve({localPath : path , key : key})
            })
            .catch(err=>{
                reject(new kerror('error while downloading KitsuneCSSAsset file from s3','addKitsuneJSAssetsToProjectFolderS3',err));
            })
    })
};

exports.addKitsuneJSAssetsToProjectFolderS3 = function (projectId) {
    return new Promise((resolve,reject)=>{
        download(jsAsset)
            .then(data=>{
                data = data.toString().replace('[Kitsune_Optimized_Message_API]', process.env.OPTIMIZED_MESSAGE_API + projectId);
                let fileName = jsAsset.split('/').pop();
                let path = `${localPathOfFiles}/${fileName}`;
                fs.writeFileSync(path,data);
                let key = `${projectId}${currentWorkingDirectoryInS3}/${fileName}`;
                resolve({localPath : path , key : key})
            })
            .catch(err=>{
                reject(new kerror('error while downloading KitsuneJSAsset file from s3','addKitsuneJSAssetsToProjectFolderS3',err));
            })
    })
};

exports.UpdateWebEngageForOptimizationCompletion = function (projectId) {
    return new Promise((resolve,reject)=>{
        let methodName = 'UpdateWebEngageForOptimizationCompletion';

        if(projectId === undefined)
            return reject( new kerror(`parameters not valid , Parameter ProjectId : ${projectId}`,methodName));

        let client = new Client();

        let args = {
            data: { ProjectId : projectId , Event : "Optimization" },
            headers: { "Content-Type": "application/json" }
        };
        client.post(`${process.env.KITSUNE_API_LINK}${process.env.KITSUNE_WEBENGAGE_API_PATH}`, args, function (data, response) {

            if(response.statusCode === 200){
                return resolve(data);
            }
            else{
                return reject(new kerror(`API call for optimization complete failed , with response : ${data}`,methodName));
            }
        });

    })
};

exports.GetProjectSettings = function (projectId) {
    return new Promise((resolve, reject) => {
        request({
            url: `${process.env.PROJECT_SETTINGS_API}${projectId}`,
            headers: { 'clientId' : process.env.PROJECT_SETTINGS_CLIENT_ID }
        }, (err, response, body) => {
            try {
                if (!err && response.statusCode === 200) {
                    let data = JSON.parse(body);
                    let settings = data.File.Content;
                    let settingsObj = JSON.parse(settings);
                    let optimizerSettings = settingsObj.build || settingsObj.optimizer;
                    resolve(optimizerSettings);
                } else {
                    reject(err);
                }
            } catch (e) {
                reject(e);
            }
        });
    });
};