from mongoengine import DynamicEmbeddedDocument, DynamicDocument, StringField, EmbeddedDocumentField, IntField
from mongoengine import DateTimeField, EmbeddedDocumentListField, BooleanField, ListField
from datetime import datetime
from enum import Enum
import os


class KitsunePublishStats(DynamicDocument):
    project_id = StringField(required=True, db_field='ProjectId')
    version = IntField(required=True, db_field='Version')
    stage = IntField(required=True, db_field='Stage')

    meta = {'collection': os.environ.get('PUBLISH_STATS_COLLECTION_NAME', 'KitsunePublishStats')}


class KitsuneProjects(DynamicDocument):
    class BucketNames(DynamicEmbeddedDocument):
        source = StringField(required=True)
        demo = StringField(required=True)
        placeholder = StringField(required=True)
        production = StringField(required=True)

    project_id = StringField(required=True, db_field='ProjectId')
    schema_id = StringField(required=False, db_field='SchemaId')
    user_email = StringField(required=True, db_field='UserEmail')
    version = IntField(required=True, db_field='Version')
    status = StringField(required=True, db_field='ProjectStatus')
    published_version = IntField(required=True, db_field='PublishedVersion')
    bucket_names = EmbeddedDocumentField(BucketNames, required=True, db_field='BucketNames')

    meta = {'collection': os.environ.get('PROJECT_COLLECTION_NAME', 'KitsuneProjects')}


class KitsuneProjectResources(DynamicDocument):
    project_id = StringField(db_field='ProjectId')
    source_path = StringField(db_field='SourcePath')
    updated_on = DateTimeField(db_field='UpdatedOn')

    meta = {'collection': os.environ.get('RESOURCE_COLLECTION_NAME', 'KitsuneResources')}


class KitsuneResourcesProduction(DynamicDocument):
    project_id = StringField(db_field='ProjectId')
    meta = {'collection': os.environ.get('RESOURCE_COLLECTION_NAME', 'KitsuneResourcesProduction')}


class KitsuneWebsite(DynamicDocument):
    project_id = StringField(db_field='ProjectId')
    meta = {'collection': os.environ.get('WEBSITE_COLLECTION_NAME', 'KitsuneWebsites')}


class KitsuneWebsiteDNS(DynamicDocument):
    website_id = StringField(db_field='WebsiteId')
    meta = {'collection': os.environ.get('WEBSITE_DNS_COLLECTION_NAME', 'KitsuneWebsiteDNS')}


class KitsuneWebsiteUsers(DynamicDocument):
    website_id = StringField(db_field='WebsiteId')
    meta = {'collection': os.environ.get('WEBSITE_USERS_COLLECTION_NAME', 'KitsuneWebsiteUsers')}


class KitsuneLanguage(DynamicDocument):
    meta = {'collection': os.environ.get('SCHEMA_DEF_COLLECTION_NAME', 'KitsuneLanguages')}


class User(DynamicDocument):
    user_name = StringField(db_field='UserName')
    meta = {'collection': os.environ.get('USER_COLLECTION_NAME', 'users')}


class ProjectStatus(Enum):
    PUBLISHING = 'PUBLISHING'
    PUBLISHINGERROR = 'PUBLISHINGERROR'
    IDLE = 'IDLE'

# May do if required
# class PublishStats(Document):
#     class Error(DynamicEmbeddedDocument):
#         ErrorMessage = StringField()
#         SourceMethod = StringField()
#         ErrorStackTrace = StringField()
#         LineNumber = StringField()
#         ColumnNumber = StringField()
#         PageName = StringField()
#
#     project_id = StringField(required=True, db_field='ProjectId')
#     customer_id = StringField(required=True, db_field='CustomerId')
#     customer_email = StringField(required=True, db_field='CustomerEmail')
#     developer_id = StringField(required=True, db_field='DeveloperId')
#     error_logs = EmbeddedDocumentListField(Error, db_field='Errors')
#     date_created = DateTimeField(default=datetime.utcnow, required=True, db_field='CreatedOn')
