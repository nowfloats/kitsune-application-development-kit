const models = require('./../models/new_models');
const logger_helper = require('./logger');
const mongoose = require('mongoose');
const ObjectID = mongoose.Types.ObjectId;
const kerror = require('./../helpers/error');
mongoose.promise = global.Promise;


// mongoose.connect(process.env.MONGO_DEV_URI);

exports.updateKitsuneResource = function (resource) {
    const logger = logger_helper.get_logger(resource.ProjectId);

    let updatedProperties = {
        $set: {
            OptimizedPath: resource.OptimizedPath
        }
    };

    if (resource.IsStatic && resource.SourcePath !== resource.OptimizedPath) {
        updatedProperties['$set']['UrlPattern'] = resource.OptimizedPath
    }

    models.KitsuneProjectResources.updateOne({_id: new ObjectID(resource._id)}, updatedProperties)
        .then((res) => {
            if (res.nModified == 0) {
                logger.info(`document not modified for optimized url for file Id : ${resource._id}`, res);
            } else {
                logger.info(`document successfully updated for optimized url for file Id : ${resource._id}`, res);
            }
        })
        .catch((err) => {
            logger.error(`document not updated for optimized url for file Id : ${resource._id}`, res);
        })
};

exports.getLastBuildDateForProject = function (projectId) {
    return new Promise((resolve, reject) => {
        let methodName = arguments.callee.name;
        if (typeof projectId === 'string' && projectId && projectId != "") {
            models.KitsuneBuildStatus.find({ProjectId: projectId, IsCompleted: true})
                .sort({CreatedOn: -1})
                .limit(1)
                .then(res => {
                    if (res != null && res.length != 0) {
                        var buildStat = res.pop();
                        resolve({isSuccess: true, date: buildStat.CreatedOn});
                    }
                    else {
                        resolve({isSuccess: false, date: ""});
                    }
                })
                .catch(err => {
                    reject(new kerror('error while getting date from build stats', methodName));
                });
        } else {
            reject(new Error('parameters not valid', methodName));
        }

    })
};

exports.updateKitsuneHtmlResourceWithKeywords = function (resource, keywords, isStatic) {
    return new Promise((resolve, reject) => {

        if (!isStatic)
            return resolve([]);

        let methodName = arguments.callee.name;
        models.KitsuneProjectResources.update({_id: new ObjectID(resource._id)},
            {$set: {MetaData: {Keywords: keywords}}})
            .then(res => {
                resolve(keywords);
            })
            .catch(err => {
                reject(new kerror("error while updating keywords in database", methodName, err));
            })

    })
};

exports.getKitsuneBuildStat = function (projectId) {
    return new Promise((resolve, reject) => {
        let methodName = arguments.callee.name;
        if (typeof projectId === 'string' && projectId && projectId != "") {
            models.KitsuneBuildStatus.find({ProjectId: projectId})
                .sort({CreatedOn: -1})
                .limit(1)
                .then(res => {
                    if (res != null && res.length != 0) {
                        resolve(res.pop());
                    }
                    else {
                        reject(new kerror('error no documents found in build stats', methodName));
                    }
                })
                .catch(err => {
                    reject(new kerror('error while getting date from build stats', methodName));
                });
        } else {
            reject(new Error('parameters not valid', methodName));
        }
    });
};

exports.getKitsuneProject = function (projectId) {
    return new Promise((resolve, reject) => {
        let methodName = "getKitsuneProject";

        if (typeof projectId === 'string' && projectId && projectId != "") {
            models.KitsuneProject.findOne({ProjectId: projectId})
                .then(res => {
                    if (res === null) {
                        reject(new kerror('document not found in kitsune projects', methodName));
                    } else {
                        if(res.BucketNames === null)
                            reject(new kerror('BucketNames cannot be null for project',methodName));
                        resolve(res);
                    }
                })
                .catch(err => {
                    reject(new kerror('error while getting document from kitsune projects', methodName));
                })
        } else {
            reject(new Error('parameters not valid', methodName));
        }
    })
};

exports.getKitsuneProjectResources = function (projectId, lastUpdatedOnDate, isStatic) {
    return new Promise((resolve, reject) => {

        if (isStatic)
            return resolve([]);

        let methodName = "getKitsuneProjectResources";

        if (typeof projectId === 'string' && projectId && projectId != "") {
            models.KitsuneProjectResources.find(
                {
                    ProjectId: projectId,
                    UpdatedOn: {$gt: lastUpdatedOnDate},
                    IsArchived: false
                })
                .then(res => {
                    if (res == null) {
                        reject(new kerror("Error finding resources", methodName));
                    } else {
                        resolve(res);
                    }
                })
                .catch(err => {
                    reject(new kerror('error while getting resources from kitsuneResources', methodName, err));
                })
        } else {
            reject(new Error('parameters not valid', methodName));
        }
    })
};

exports.updateWarningInBuildStats = function (buildId, warning) {
    return new Promise((resolve, reject) => {
        let methodName = 'updateWarningInBuildStats';
        if (typeof buildId != 'object' || typeof  warning != 'object')
            reject(`parameters not valid , Parameters buildId : ${buildId} , Warning : ${warning}`, methodName);

        models.KitsuneBuildStatus.update({_id: new ObjectID(buildId)}, {$push: {Warning: warning}})
            .then(res => {
                resolve(res);
            })
            .catch (err => {
                reject(new kerror('error while updating warnings in kitsune build stats', methodName, err));
            })
    })

};

exports.updateErrorInBuildStats = function (buildId, err) {
    return new Promise((resolve, reject) => {
        let methodName = 'updateErrorInBuildStats';
        if (typeof buildId != 'object' || typeof err != 'object')
            reject(`parameters not valid , Parameters buildId : ${buildId} , err : ${err}`, methodName);

        models.KitsuneBuildStatus.update({_id: new ObjectID(buildId)}, {$push: {Error: err}})
            .then(res => {
                resolve(res);
            })
            .catch(err => {
                reject(new kerror('error while updating errors in kitsune build stats', methodName));
            })
    })
};

//UNUSED
// exports.updateKitsuneBuildStatusStatus = function (buildId) {
//     return new Promise((resolve, reject) => {
//         const logger = logger_helper.logger;
//
//         if (typeof buildId !== 'object') {
//             reject(new kerror(`parameters not valid ,buildId : ${buildId}`, 'updateKitsuneBuildStatusStatus'));
//         }
//
//         models.KitsuneBuildStatus.update({_id: new ObjectID(buildId)}, {$inc: {BuildStatus: 1}})
//             .then(res => {
//                 resolve();
//                 logger.info(`successfully updated kitsuneBuildstatus status for buildID : ${buildId}`);
//             })
//             .catch(err => {
//                 let kerr = new kerror(`error while updating build status for buildId : ${buildId}`, 'updateKitsuneBuildStatusStatus', err);
//                 logger.error(kerr);
//                 reject(kerr);
//             })
//     })
// };

// exports.updateKitsuneProjectStatus = function (projectId) {
//     return new Promise((resolve, reject) => {
//         const logger = logger_helper.get_logger(projectId);
//
//         let methodName = 'updateKitsuneProjectStatus';
//
//         if (typeof projectId != 'string' || projectId.length == 0)
//             return reject(new kerror(`parameters not valid , projectId : ${projectId}`, methodName));
//
//         models.KitsuneProject.update({ProjectId: projectId}, {$set: {ProjectStatus: models.ProjectStatus.BUILDING.toString()}})
//             .then(res => {
//                 resolve();
//                 logger.info(`kitsuneprojects successfully updated for status building for ProjectId : ${projectId} , with response : ${res}`);
//             })
//             .catch(err => {
//                 let kerr = new kerror(`error while updating kitsune project status for project id : ${projectId}`, methodName, err);
//                 logger.error(kerr);
//                 reject(kerr);
//             })
//     })
// }

exports.updateKitsuneProjectStatusForErrorOrCompletion = function (projectId, isError) {
    return new Promise((resolve, reject) => {
        const logger = logger_helper.get_logger(projectId);

        let methodName = 'updateKitsuneProjectStatus';

        if (typeof projectId != 'string' || projectId.length == 0)
            return reject(new kerror(`parameters not valid , projectId : ${projectId}`, methodName));

        models.KitsuneProject.update({ProjectId: projectId}, {
            $set: {
                ProjectStatus: isError ? models.ProjectStatus.BUILDINGERROR.toString() : models.ProjectStatus.IDLE.toString()
            }
        })
            .then(res => {
                logger.info(`kitsuneprojects successfully updated for status building for ProjectId : ${projectId} , with response : ${res}`);
            })
            .catch(err => {
                let kerr = new kerror(`error while updating kitsune project status for project id : ${projectId}`, methodName, err);
            })
    })
}

exports.getKitsuneResourcesforAnalyzer = function (projectId , lastUpdatedDate) {
    return new Promise((resolve,reject)=>{
            models.KitsuneProjectResources.find({
                ProjectId: projectId,
                ResourceType: {$in: [models.ResourceType.LINK.value, models.ResourceType.STYLE.value]},
                UpdatedOn: {$gte: lastUpdatedDate}
            })
            .then(resources=>{

            })
            .catch(err=>{

            })
    })
}


exports.updateKitsuneProjectStatus = function (projectId, status) {
    return new Promise((resolve, reject) => {
        const logger = logger_helper.get_logger(projectId);
        let methodName = 'updateKitsuneProjectStatus';

        if (typeof projectId != 'string' || projectId.length == 0)
            return reject(new kerror(`parameters not valid , projectId : ${projectId}`, methodName));

        models.KitsuneProject.update({ProjectId: projectId}, {
            $set: {
                ProjectStatus: status.toString()
            }
        })
        .then(res => {
            logger.info(`[base] kitsuneProjects successfully updated for status ${status.toString()} for ProjectId : ${projectId} , with response :`,res);
            resolve(res);
        })
        .catch(err => {
            let kerr = new kerror(`[base] error while updating kitsune project status for project id : ${projectId}`, methodName, err);
            reject(kerr);
        })
    })
}
