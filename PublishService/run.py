from os.path import join, dirname
from dotenv import load_dotenv
load_dotenv('ENV_PROD')

from multiprocessing.dummy import Pool as ThreadPool
from multiprocessing import Pool
from publish import Publish
import boto3
import json
import os
import time
import logging
import watchtower

log_format = '[{}] [%(levelname)s] [%(asctime)s] %(filename)s:%(lineno)d > %(funcName)s : %(message)s'
sqs = boto3.client('sqs', region_name=os.environ.get('AWS_QUEUE_REGION'))
logging.basicConfig(level=logging.INFO, format=log_format.format('GLOBAL'))

cloudwatch_handler = watchtower.CloudWatchLogHandler(
    log_group='Kitsune_Publish_Service',
    boto3_session=boto3.Session(region_name=os.environ.get('CLOUDWATCH_REGION')),
    stream_name=f'Queue Pooler')
cloudwatch_handler.setLevel(logging.INFO)


def start_pooling(i):
    cloudwatch_handler.setFormatter(logging.Formatter(log_format.format(i)))
    logging.getLogger(f'Queue Pooler').addHandler(cloudwatch_handler)
    logger = logging.getLogger(f'Queue Pooler')

    while 1:
        response = sqs.receive_message(
            QueueUrl=os.environ.get('PUBLISH_QUEUE'),
            AttributeNames=['SentTimestamp'],
            MaxNumberOfMessages=1,
            MessageAttributeNames=['All'],
            WaitTimeSeconds=10
        )

        if 'Messages' in response:
            message = response['Messages'][0]
            receipt_handle = message['ReceiptHandle']

            try:
                message = json.loads(message['Body'])
                logger.info(f'[{i}] Got Message from Queue ProjectId: {message["PublishId"]}')
                Publish(message['PublishId'], i)
            except KeyError:
                logger.error(f'Invalid body in message from queue. Message was {message}')
            except Exception as err:
                logger.error(f'Failed for {message["PublishId"]} due to {err}')

            sqs.delete_message(
                QueueUrl=os.environ.get('PUBLISH_QUEUE'),
                ReceiptHandle=receipt_handle
            )

        time.sleep(2)


print('Starting to pool from Publish Queue')

start_pooling(1)
# pool = ThreadPool(4)  # Thread Pool
# pool = Pool(4)  # Process Pool
#pool.map(start_pooling, range(3))
#pool.close()
#pool.join()

# message = '{"PublishId":"59e2e6bdbe462609b4314f41"}'
# message = json.loads(message)
# Publish(message['PublishId'], 1)
