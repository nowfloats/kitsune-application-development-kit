import os
import time
import boto3
import base64
import json
import subprocess
import logging
import urllib
import shutil
import zipfile
import requests
import watchtower
from datetime import date, datetime
from models import *
from typing import List
from mongoengine import connect, QuerySet, register_connection
from mongoengine.context_managers import switch_db
import re
from bson import ObjectId

log_format = '[{}] [%(levelname)s] [%(asctime)s] %(filename)s:%(lineno)d > %(funcName)s : %(message)s'
logging.basicConfig(format=log_format.format('K'), level=logging.INFO)


def run_aws_cli(command):
    output = subprocess.check_output(command)
    return output.decode('utf-8')


def directory_zip(root_dir, dir_to_zip, ziph):
    # ziph is zipfile handle
    os.chdir(root_dir)
    for root, dirs, files in os.walk(dir_to_zip):
        for file in files:
            ziph.write(os.path.join(root, file))
    os.chdir('../../')


def get_website_data(website_ids: List[str]):
    for website_id in website_ids:
        website = json.loads(KitsuneWebsite.objects(id=website_id).first().to_json())
        dns_query = KitsuneWebsiteDNS.objects(website_id=website_id)
        website['dns'] = json.loads(dns_query.to_json())
        website['users'] = json.loads(KitsuneWebsiteUsers.objects(website_id=website_id).to_json())
        yield website


class Publish:
    def __init__(self, publish_id, i):
        cloudwatch_handler = watchtower.CloudWatchLogHandler(
            log_group='Kitsune_Publish_Service',
            boto3_session=boto3.Session(region_name=os.environ.get('CLOUDWATCH_REGION')),
            stream_name=f'{date.today()} PublishID - {publish_id}')
        cloudwatch_handler.setFormatter(logging.Formatter(log_format.format(i)))
        cloudwatch_handler.setLevel(logging.INFO)
        logging.getLogger(f'P:{publish_id}').addHandler(cloudwatch_handler)

        self.logger = logging.getLogger(f'P:{publish_id}')
        self.publish_api = os.environ.get('PUBLISH_API')

        connect(host=os.environ.get('MONGO_DB_URI'))
        register_connection('schema-db', host=os.environ.get('MONGO_SCHEMADB_URI'))
        self.css_asset_link = os.environ.get('CSS_ASSET_LINK')
        self.js_asset_link = os.environ.get('JS_ASSET_LINK')
        self.robots_link = os.environ.get('ROBOTS_LINK')
        self.publish_id = publish_id
        self.project_id = None
        self.developer_email = ''
        self.cloud_config = {}

        try:
            self.publish_stats = KitsunePublishStats.objects(id=publish_id).first()
        except KitsunePublishStats.DoesNotExist:
            self.logger.error(f'PublishStats with {publish_id} not found')
            raise

        self.project_id = self.publish_stats.project_id

        self.logger.info(f'Starting for {self.project_id}')

        self.demo_project_dir = f'{self.project_id}/cwd'
        self.prod_project_dir = f'{self.project_id}'

        try:
            self.project: KitsuneProjects = KitsuneProjects.objects(project_id=self.project_id).first()
        except KitsuneProjects.DoesNotExist:
            self.logger.error(f'Project with Id: {self.project_id} not found')
            raise
        self.developer_email = self.project.user_email

        try:
            self.developer: User = User.objects(user_name=self.developer_email).first()
        except User.DoesNotExist:
            self.logger.error(f'User with user_name: {self.developer_email} not found')
            raise

        self.buckets = self.project.bucket_names

        self.project.status = ProjectStatus.PUBLISHING.value
        self.project.save()

        try:
            """ Copy from Demo to Production """
            runtime = self.copy(f's3://{self.buckets.demo}/{self.demo_project_dir}/',
                                f's3://{self.buckets.production}/{self.prod_project_dir}/v{self.publish_stats.version}/',
                                throw=True)
            self.logger.info(f'Copied from demo to production in {runtime}s')

            """ Version Source """
            runtime = self.copy(f's3://{self.buckets.source}/{self.project_id}/',
                                f's3://{self.buckets.demo}/{self.project_id}/v{self.publish_stats.version}/src/',
                                throw=True)
            self.logger.info(f'Copied source as version in demo  in {runtime}s')

            """ Version Demo """
            runtime = self.copy(f's3://{self.buckets.demo}/{self.demo_project_dir}/',
                                f's3://{self.buckets.demo}/{self.project_id}/v{self.publish_stats.version}/demo/',
                                throw=True)
            self.logger.info(f'Copied demo as version in demo in {runtime}s')
        except Exception as err:
            self.project.status = ProjectStatus.PUBLISHINGERROR.value
            self.project.save()
            self.logger.error(f'Error in copying files due to {err}. Terminating this publish')
            return

        try:
            """"Updating cdn in html files"""
            self.update_html_file_cdn()
            self.logger.info('Done updating CDN in HTML files')
        except Exception as err:
            self.project.status = ProjectStatus.PUBLISHINGERROR.value
            self.project.save()
            self.logger.error(f'Error in updating cdn in html files due to {err}. Terminating this publish')
            raise err
            return

        try:
            res = KitsuneProjectResources.objects(project_id=self.project_id,
                                                  source_path='/robots.txt').order_by('-updated_on').first()
            if res is None:
                self.logger.info('robots.txt does not exists in Kitsune Resources. Using default robots.txt')
                self.upload_to_prod(self.robots_link)
            else:
                self.logger.info('Found robots.txt')
                self.copy(f's3://{self.buckets.source}/${self.project_id}/robots.txt',
                          f's3://{self.buckets.production}/${self.prod_project_dir}/v{self.publish_stats.version}//robots.txt')
        except Exception as err:
            self.logger.error(f'Error in updating robots.txt due to {err}')

        try:
            self.upload_to_prod(self.css_asset_link)
            self.upload_to_prod(self.js_asset_link)
            self.logger.info('Uploaded assets to prod')

            api_url = f'{self.publish_api}'
            payload = {'UserEmail': self.developer_email,
                       'PublishStatsId': self.publish_id}
            resp = requests.post(api_url, data=json.dumps(payload),
                                 headers={'Accept': 'application/json', 'Content-Type': 'application/json'})
            response = resp.json()
            self.logger.info(f'API response: {response}')
            if resp.status_code != 200 or response.status == 'fail':
                self.logger.error(f'Error in Publish API for {self.project_id} due to {response}')
                self.project.status = ProjectStatus.PUBLISHINGERROR.value
                self.project.save()
                return
            else:
                # API sets ProjectStatus to IDLE, It needs to be PUBLISHING
                self.project.status = ProjectStatus.PUBLISHING.value
                self.project.save()
        except Exception as e:
            self.project.update(status=ProjectStatus.PUBLISHINGERROR.value)
            self.logger.error(f'Error in calling publish API due to {e}')

        try:
            # Check with API
            response = requests.get(f'{os.environ["CLOUD_CREDS_API"]}={self.project_id}',
                                    headers={'Authorization': str(self.developer.id)})
            if response.status_code != 200:
                self.logger.info('Not creating .kit package')
            else:
                KitsuneProjects.objects(project_id=self.project_id).update_one(status=ProjectStatus.PUBLISHING.value)
                self.cloud_config = response.json()
                self.make_package()

            KitsuneProjects.objects(project_id=self.project_id).update_one(status=ProjectStatus.IDLE.value)
        except Exception as e:
            self.logger.info(f'Error in checking for .kit package due to {e}')
            KitsuneProjects.objects(project_id=self.project_id).update_one(status=ProjectStatus.PUBLISHINGERROR.value)

        self.logger.info(f'Done for {self.project_id}')

    def make_package(self):
        try:
            # Create .kit package
            self.logger.info('Creating .kit package')
            root_dir = 'temp'
            local_directory = f'{root_dir}/{self.project_id}/'
            local_files_directory = f'{local_directory}files/'
            local_data_directory = f'{local_directory}data/'
            compressed_file = f'{root_dir}/{self.project_id}-v{self.publish_stats.version}.kit'

            # Download production files
            runtime = self.copy(
                f's3://{self.buckets.production}/{self.prod_project_dir}/v{self.publish_stats.version}/',
                local_files_directory, action='cp', throw=True, recursive=True)

            # TODO: Delete dynamic .html and .html.dl files from this directory
            # TODO: Remove IsArchived = true files
            self.logger.info(f'Downloaded production to local in {runtime}s')

            if not os.path.exists(local_data_directory):
                os.mkdir(local_data_directory)

            # Add Routing
            routing_query = [os.environ.get('PROD_RESOURCE_COLLECTION_NAME', 'new_KitsuneResourcesProduction'),
                             json.dumps({'ProjectId': self.project_id})]
            routing_response = requests.post(os.environ['ROUTING_API'], data='\n'.join(routing_query))

            trees = {}
            if routing_response.ok:
                trees = routing_response.json()

            if trees == {}:
                self.logger.error('No routes found')
            with open(f'{local_data_directory}routes.json', 'w') as routes_file:
                self.logger.info('.kit: adding routes')
                json.dump(trees, routes_file)

            # Add Resources
            with open(f'{local_data_directory}resources.json', 'w') as resources_file:
                resources: QuerySet = KitsuneResourcesProduction.objects(project_id=self.project_id)
                self.logger.info('.kit: adding resources')
                json.dump({'data': json.loads(resources.to_json())}, resources_file)

            # Add Website (User + Website + DNS + Project)
            with open(f'{local_data_directory}websites.json', 'w') as websites_file:
                website_data = list(get_website_data(self.publish_stats.CustomerIds))
                self.logger.info('.kit: adding websites')
                json.dump({'data': website_data}, websites_file)

            try:
                # Add Schema Definition
                if self.project.schema_id:
                    with switch_db(KitsuneLanguage, 'schema-db') as KitsuneSchema:
                        schema = json.loads(KitsuneSchema.objects(id=self.project.schema_id).first().to_json())
                        with open(f'{local_data_directory}schema.json', 'w') as schema_file:
                            self.logger.info('.kit: adding schema')
                            json.dump({'data': schema}, schema_file)
                else:
                    self.logger.info(f'Project: {self.project_id} has no schema')
            except Exception as e:
                self.logger.warning(f'Unable to get schema with id {self.project.schema_id} due to: {e}')

            # Add Manifest
            with open(f'{local_directory}manifest.json', 'w') as manifest_file:
                self.logger.info('.kit: adding manifest')
                components = self.project['Components'] if 'Components' in self.project else []

                manifest = {
                    'ManifestVersion': '1.0.0',
                    'KitsuneRuntime': '1.0.0',
                    'Components': components,
                    'CreatedOn': datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
                    'Developer': self.project.user_email
                }

                json.dump(manifest, manifest_file)

            # Add to zip
            with zipfile.ZipFile(compressed_file, 'w', zipfile.ZIP_DEFLATED) as zip_file:
                directory_zip(local_directory, '.', zip_file)

            # Copy to Prod / Packages
            runtime = self.copy(compressed_file, f's3://{self.buckets.production}/{self.prod_project_dir}/packages/',
                                action='cp', throw=True)
            self.logger.info(f'Copied .kit to production/packages in {runtime}s')

            # Remove local files
            shutil.rmtree(root_dir)

            # Encrypt Message
            dotkit_package_url = f'https://s3.{os.environ["AWS_BUCKET_REGION"]}.amazonaws.com/' \
                                 f'{self.buckets.production}/{self.project_id}/packages/' \
                                 f'{self.project_id}-v{self.publish_stats.version}.kit'

            deployment_message = {
                'kitfileUrl': dotkit_package_url,
                'providerSettings': self.cloud_config,
            }

            # Send to Cloud Orchestrator SQS
            sqs_client = boto3.client('sqs', region_name=os.environ['AWS_ORCH_QUEUE_REGION'])
            response = sqs_client.send_message(QueueUrl=os.environ['ORCHESTRATOR_QUEUE_URL'],
                                               MessageBody=json.dumps(deployment_message))
            self.logger.info(f'Sent to orchestrator queue: {response}')
        except Exception as e:
            self.logger.error(f'Error in creating .kit package due to {e}')
            raise e

    def copy(self, from_dir, to_dir, throw=False, exclude='', action='sync', recursive=False):
        try:
            t_start = time.time()
            command = ['aws',
                       's3',
                       action,
                       f'{from_dir}',
                       f'{to_dir}',
                       '--acl=public-read',
                       '--region=ap-south-1']
            if exclude != '':
                command.append(f'--exclude={exclude}')

            if recursive:
                command.append('--recursive')

            output = run_aws_cli(command)
            t_end = time.time()
            time_taken = t_end - t_start
            # print(f'{output}')
            # self.logger.info(f'Copied {self.project_id} in {time_taken}s')
            return time_taken
        except Exception as e:
            if not throw:
                self.logger.error(f'Error copying {from_dir} to {to_dir} due to {str(e)}')
                return -1
            else:
                raise Exception(e)

    def upload_to_prod(self, asset_link):
        filename = asset_link.split('/')[-1]
        urllib.request.urlretrieve(asset_link, filename)
        command = ['aws',
                   's3',
                   'cp',
                   f'{filename}',
                   f's3://{self.buckets.production}/{self.prod_project_dir}/v{self.project.version}/',
                   '--acl=public-read',
                   f'--region={os.environ.get("AWS_BUCKET_REGION")}']
        run_aws_cli(command)
        # os.remove(filename)

    def update_html_file_cdn(self):
        # Get All Files for Updating CDN Link
        present_string = f'{os.environ["CDN_PREVIEW"]}{self.project_id}/cwd'
        replace_string = f'{os.environ["CDN_PROD"]}{self.project_id}/v{self.publish_stats.version}'
        html_files = ('.htm', '.html', '.htm.dl', '.html.dl', '.kc', '.css')
        preview_cdn_pattern = re.compile(re.escape(present_string), re.IGNORECASE)

        s3_client = boto3.client('s3', region_name=os.environ['AWS_BUCKET_REGION'])

        objects_list_response = s3_client.list_objects_v2(
            Bucket=self.buckets.production,
            Prefix=f'{self.prod_project_dir}/v{self.publish_stats.version}/'
        )

        files_to_change = [x.get('Key') for x in objects_list_response.get('Contents')
                           if x.get('Key').lower().endswith(html_files)]
        while objects_list_response.get('NextContinuationToken'):
            objects_list_response = s3_client.list_objects_v2(
                Bucket=self.buckets.production,
                Prefix=f'{self.prod_project_dir}/v{self.publish_stats.version}/',
                ContinuationToken=objects_list_response.get('NextContinuationToken')
            )
            files_to_change.extend([x.get('Key') for x in objects_list_response.get('Contents')
                                    if x.get('Key').lower().endswith(html_files)])

        for key in files_to_change:
            object_response = s3_client.get_object(
                Bucket=self.buckets.production,
                Key=key
            )

            file_content = object_response.get('Body').read().decode('utf-8')
            content_type = object_response.get('ContentType')

            if not key.endswith('.kc'):
                file_content = re.sub(preview_cdn_pattern, replace_string, file_content)
                file_content = file_content.encode('utf-8')
            else:
                raw_content = base64.b64decode(file_content)
                raw_content_str = re.sub(preview_cdn_pattern, replace_string, raw_content.decode('utf-8'))
                file_content = base64.b64encode(bytes(raw_content_str, 'utf-8'))

            save_response = s3_client.put_object(
                Body=file_content,
                Bucket=self.buckets.production,
                Key=key,
                ACL='public-read',
                ContentType=content_type
            )


# if __name__ == '__main__':
#     Publish('5b61b2e59b7a5904ff7ad9ce', 7)
