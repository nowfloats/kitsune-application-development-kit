require('dotenv').config({path: 'ENV_PROD'});
const Consumer = require('sqs-consumer');
const aws = require('./helpers/aws');
const logger_helper = require('./helpers/logger');
const { Optimize } =  require('./minifiers/optimize');
const queue = process.env.KITSUNEOPTIMIZERSQS;

logger = logger_helper.logger;
logger.info('Starting listener to queue');
logger.info('testing pipeline');

const app = Consumer.create({
    queueUrl: queue,
    handleMessage: (sqsmessage, done) => {
        try
        {
            logger.info('picked up message', sqsmessage.Body);
            let message = JSON.parse(sqsmessage.Body);
            new Optimize(message.ProjectId, done);
        }
        catch (err)
        {
            logger.error(`error while initializing optimizer for message : ${sqsmessage.Body} due to ${err}`);
        }
    }
});

app.on('error', (err) => {
    logger.error(`error while starting the consumer for sqs error :`,err);
});

app.start();

// new Optimize('5a09bae9dcc637000116eff7', function () {
//     console.log('Done');
// });
