const winston = require("winston");
const WinstonCloudwatch = require("winston-cloudwatch");


winston.loggers.add('root', {
    transports: [
        new winston.transports.Console({
            colorize: true
        }),
        new WinstonCloudwatch({
            awsRegion: 'ap-southeast-2',
            logGroupName: process.env.LOG_GROUP_NAME || 'kitsune_nbuild',
            logStreamName: `${new Date().toISOString().split('T')[0]} Root`
        })
    ]
});


// self.transports.CloudWatch.kthxbye(function() {
//     console.log('bye');
// });

let logger = winston.loggers.get('root');
exports.logger = logger;

function set_logger(project_id) {
    winston.loggers.add(`${project_id}`, {
        transports: [
            new winston.transports.Console({
                colorize: true
            }),
            new WinstonCloudwatch({
                awsRegion: process.env.AWS_REGION,
                logGroupName: process.env.LOG_GROUP_NAME || 'kitsune_nbuild',
                logStreamName: `${new Date().toISOString().replace(/:/g, '.')} ${project_id}`
            })
        ]
    });
    return winston.loggers.get(`${project_id}`);
}

exports.set_logger = set_logger;

function get_logger(project_id) {
    return winston.loggers.get(`${project_id}`);
}

exports.get_logger = get_logger;