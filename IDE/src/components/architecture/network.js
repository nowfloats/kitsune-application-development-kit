export const NETWORK_OPTIONS = {
	nodes: {
		shape: 'image',
		font: {
			color: 'white'
		},
		borderWidth: 2,
		shadow: true
	},
	edges: {
		width: 2,
		shadow: true,
		arrows: {
			to: {
				scaleFactor	: 0.5
			},
			from: {
				scaleFactor	: 0.5
			}
		}
	},
	interaction: {
		hover: true
	},
	manipulation: {
		enabled: true
	}
};

export const aws = {
	staticWebsite: {
		componentCount: 2,
		nodes: [
			{
				id: 1,
				label: 'Users',
				image: 'https://www.getkitsune.com/assets/images/aws/users.png',
				description: 'This represents the primary user of your application',
				font: {
					color: 'white'
				}
			}, {
				id: 2,
				label: 'CDN',
				image: 'https://www.getkitsune.com/assets/images/aws/cf.png',
				description: 'CDN (Content Delivery Network) is used to accelerate the content delivery to the user.',
				font: {
					color: 'white'
				}
			}, {
				id: 3,
				label: 'S3',
				image: 'https://www.getkitsune.com/assets/images/aws/s3.png',
				description: 'S3 is used to store the resources of your application that do not change frequently.',
				font: {
					color: 'white'
				}
			}
		],
		edges: [
			{
				from: 1,
				to: 2,
				length: 300,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 2,
				to: 3,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}
		]
	},
	dynamicWebsite: {
		componentCount: 8,
		nodes: [
			{
				id: 1,
				group: 0,
				label: 'Users',
				image: 'https://www.getkitsune.com/assets/images/aws/users.png',
				description: 'This represents the primary user of your application'
			}, {
				id: 2,
				group: 0,
				label: 'CDN',
				image: 'https://www.getkitsune.com/assets/images/aws/cf.png',
				description: 'CDN (Content Delivery Network) is used to accelerate the content delivery to the user.'
			}, {
				id: 3,
				group: 0,
				label: 'Lambda @Edge',
				image: 'https://www.getkitsune.com/assets/images/aws/lambda.png',
				description: 'Function Compute which encapsulates the part of app-logic which does not need database access. Hence this has been configured on edge compute - making it the closest to the user\'s location.',
				color: {
					opacity: 0.2
				}
			}, {
				id: 4,
				group: 0,
				label: 'S3',
				image: 'https://www.getkitsune.com/assets/images/aws/s3.png',
				description: 'S3 is used to store the resources of your application that do not change frequently.'
			}, {
				id: 5,
				group: 0,
				label: 'Lambda\n(Layout Manager)',
				image: 'https://www.getkitsune.com/assets/images/aws/lambda.png',
				description: 'This is the main application compute logic, which handles all database related access. With the compute being built with Lambda, you only need to pay for the exact processing time. This makes it cost-effective as you never have to pay for up time.'
			}, {
				id: 6,
				group: 0,
				label: 'MongoDB',
				image: 'https://www.getkitsune.com/assets/images/aws/mongo.png',
				description: 'Mongo is used as persistent storage provider. The NoSql architecture enables you to add/edit new dynamic properties in your application.'
			}, {
				id: 7,
				group: 1,
				label: 'API Gateway',
				image: 'https://www.getkitsune.com/assets/images/aws/api.png',
				description: 'API Gateway is used to enable REST based dynamic data-management APIs. These APIs can be integrated with downstream applications (like ERP, CRM, Inventory Mgmt etc) to add/edit dynamic values stored in Mongo.'
			}, {
				id: 8,
				group: 1,
				label: 'Lambda\n(Cache Evictor)',
				image: 'https://www.getkitsune.com/assets/images/aws/lambda.png',
				description: 'A function compute based cache manager. Whenever there is a change in dynamic properties, this function knows how to invalidate the appropiate assets on CDN.'
			}, {
				id: 9,
				group: 1,
				label: 'Web Admin\n(CMS/API)',
				image: 'https://www.getkitsune.com/assets/images/aws/user.png',
				description: 'This represents the admin of your application who can login via the CMS and edit dynamic properties. You can also integrate with other down stream applications directly by using the Management APIs created as part of this architecture.'
			}, {
				id: 10,
				group: 1,
				label: 'SQS\n(Event Queue)',
				image: 'https://www.getkitsune.com/assets/images/aws/sqs.png',
				description: 'SQS is used as the queue provider to handle large volume of data change. Even if you change more than a million dynamic values, the system is designed to scale up in real-time.'
			}],
		edges: [
			{
				from: 1,
				to: 2,
				arrows: "to;from",
				length: 300,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 2,
				to: 3,
				arrows: "to;from",
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 3,
				to: 4,
				arrows: "to;from",
				length:200,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 3,
				to: 5,
				arrows: "to;from",
				length: 200,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 5,
				to: 6,
				arrows: "to;from",
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 5,
				to: 4,
				arrows: "to;from",
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 9,
				to: 7,
				length: 300,
				arrows: "to;from",
				dashes: [2, 2, 10, 10],
				color: {
					color: 'lightgrey',
					highlight: 'yellow'
				}
			}, {
				from: 7,
				to: 6,
				length: 300,
				arrows: "to;from",
				dashes: [2, 2, 10, 10],
				color: {
					color: 'lightgrey',
					highlight: 'yellow'
				}
			}, {
				from: 6,
				to: 10,
				arrows: "to;from",
				dashes: [2, 2, 10, 10],
				color: {
					color: 'lightgrey',
					highlight: 'yellow'
				}
			}, {
				from: 10,
				to: 8,
				arrows: "to;from",
				dashes: [2, 2, 10, 10],
				color: {
					color: 'lightgrey',
					highlight: 'yellow'
				}
			}, {
				from: 8,
				to: 2,
				arrows: "to;from",
				length: 300,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'lightgrey',
					highlight: 'yellow'
				}
			}]
	},
	reportsComponent:{
		componentCount: 2,
		nodes: [
			{
			id: 11,
			group: 2,
			label: 'Cloudwatch Alarm',
			image: 'https://www.getkitsune.com/assets/images/aws/cloudwatch.png',
			description: 'This component is used to generate scheduled triggers.'
		}, {
			id: 12,
			group: 2,
			label: 'Lambda\n(Report Generator)',
			image: 'https://www.getkitsune.com/assets/images/aws/lambda.png',
			description: 'This function compute encapsulates the logic of sending out the scheduled dynamic reports.'
		}],
		edges: [{
			from: 11,
			to: 12,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}, {
			from: 12,
			to: 1,
			length: 600,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}]
	},
	callTrackerComponent:{
		componentCount: 2,
		nodes: [{
			id: 21,
			group: 3,
			label: 'Third Party Call-Tracking\nNumber Provider',
			image: 'https://www.getkitsune.com/assets/images/aws/third_party_endpoint.png'
		}, {
			id: 22,
			group: 3,
			label: 'Lambda\n(Call Tracking\nNumber Generator)',
			image: 'https://www.getkitsune.com/assets/images/aws/lambda.png'
		}],
		edges: [{
			from: 22,
			to: 21,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}, {
			from: 10,
			to: 22,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}]
	},
	pgComponent:{
		componentCount: 2,
		nodes: [{
			id: 31,
			group: 4,
			label: 'Lambda @Edge\n(Price Checksum Validator)',
			image: 'https://www.getkitsune.com/assets/images/aws/lambda.png'
		}, {
			id: 32,
			group: 4,
			label: 'Lambda\n(k-pay handler)',
			image: 'https://www.getkitsune.com/assets/images/aws/lambda.png'
		}, {
			id: 33,
			group: 4,
			label: 'Payment Gateway\nProvider',
			image: 'https://www.getkitsune.com/assets/images/aws/third_party_endpoint.png'
		}],
		edges: [{
			from: 31,
			to: 1,
			length: 600,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 2,
			to: 31,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 2,
			to: 32,
			length: 300,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 32,
			to: 33,
			dashes: true,
			label: 'callback',
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}]
	},
	containerisedAppComponent:{
		componentCount: 2,
		nodes: [{
			id: 41,
			group: 5,
			label: 'ECS',
			image: 'https://www.getkitsune.com/assets/images/aws/ecs.png',
			description: 'ECS (Elastic Container Service) is used to scale the containerised version of your legacy application.'
		}, {
			id: 42,
			group: 5,
			label: 'C1',
			image: 'https://www.getkitsune.com/assets/images/docker.png',
			description: 'Represents a single container instance of your legacy application'
		}, {
			id: 43,
			group: 5,
			label: 'C2',
			image: 'https://www.getkitsune.com/assets/images/docker.png',
			description: 'Represents a single container instance of your legacy application'
		},
		{
			id: 44,
			group: 5,
			label: 'C3',
			image: 'https://www.getkitsune.com/assets/images/docker.png',
			description: 'Represents a single container instance of your legacy application'
		}],
		edges: [{
			from: 2,
			to: 41,
			length: 700,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 41,
			to: 42,
			length: 100,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 41,
			to: 43,
			length: 100,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 41,
			to: 44,
			length: 100,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}]	
	}
};

export const aliCloud = {
	staticWebsite: {
		componentCount: 2,
		nodes: [
			{
				id: 1,
				label: 'Users',
				image: 'https://www.getkitsune.com/assets/images/aws/users.png',
				description: 'This represents the primary user of your application',
				font: {
					color: 'white'
				}
			}, {
				id: 2,
				label: 'CDN',
				image: 'https://www.getkitsune.com/assets/images/alicloud/cdn.png',
				description: 'CDN (Content Delivery Network) is used to accelerate the content delivery to the user.',
				font: {
					color: 'white'
				}
			}, {
				id: 3,
				label: 'OSS',
				image: 'https://www.getkitsune.com/assets/images/alicloud/oss.png',
				description: 'OSS is used to store the resources of your application that do not change frequently.',
				font: {
					color: 'white'
				}
			}
		],
		edges: [
			{
				from: 1,
				to: 2,
				length: 300,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 2,
				to: 3,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}
		]
	},
	dynamicWebsite: {
		componentCount: 7,
		nodes: [
			{
				id: 1,
				group: 0,
				label: 'Users',
				image: 'https://www.getkitsune.com/assets/images/aws/users.png',
				description: 'This represents the primary user of your application'
			}, {
				id: 2,
				group: 0,
				label: 'CDN',
				image: 'https://www.getkitsune.com/assets/images/alicloud/cdn.png',
				description: 'CDN (Content Delivery Network) is used to accelerate the content delivery to the user.'
			}, {
				id: 4,
				group: 0,
				label: 'OSS',
				image: 'https://www.getkitsune.com/assets/images/alicloud/oss.png',
				description: 'S3 is used to store the resources of your application that do not change frequently.'
			}, {
				id: 5,
				group: 0,
				label: 'Function Compute\n(Layout Manager)',
				image: 'https://www.getkitsune.com/assets/images/alicloud/fc.png',
				description: 'This is the main application compute logic, which handles all database related access. With the compute being built with Lambda, you only need to pay for the exact processing time. This makes it cost-effective as you never have to pay for up time.'
			}, {
				id: 6,
				group: 0,
				label: 'MongoDB',
				image: 'https://www.getkitsune.com/assets/images/alicloud/mongo.png',
				description: 'Mongo is used as persistent storage provider. The NoSql architecture enables you to add/edit new dynamic properties in your application.'
			}, {
				id: 7,
				group: 1,
				label: 'API Gateway',
				image: 'https://www.getkitsune.com/assets/images/alicloud/api.png',
				description: 'API Gateway is used to enable REST based dynamic data-management APIs. These APIs can be integrated with downstream applications (like ERP, CRM, Inventory Mgmt etc) to add/edit dynamic values stored in Mongo.'
			}, {
				id: 8,
				group: 1,
				label: 'Function Compute\n(Cache Evictor)',
				image: 'https://www.getkitsune.com/assets/images/alicloud/fc.png',
				description: 'A function compute based cache manager. Whenever there is a change in dynamic properties, this function knows how to invalidate the appropiate assets on CDN.'
			}, {
				id: 9,
				group: 1,
				label: 'Web Admin\n(CMS/API)',
				image: 'https://www.getkitsune.com/assets/images/aws/user.png',
				description: 'This represents the admin of your application who can login via the CMS and edit dynamic properties. You can also integrate with other down stream applications directly by using the Management APIs created as part of this architecture.'
			}, {
				id: 10,
				group: 1,
				label: 'Message Service\n(Event Queue)',
				image: 'https://www.getkitsune.com/assets/images/alicloud/message_service.png',
				description: 'Message Service is used as the queue provider to handle large volume of data change. Even if you change more than a million dynamic values, the system is designed to scale up in real-time.'
			}],
		edges: [
			{
				from: 1,
				to: 2,
				length: 300,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 2,
				to: 5,
				length: 300,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 5,
				to: 6,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 5,
				to: 4,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 9,
				to: 7,
				length: 300,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'white',
					highlight: 'yellow'
				}
			}, {
				from: 7,
				to: 6,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'white',
					highlight: 'yellow'
				}
			}, {
				from: 6,
				to: 10,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'white',
					highlight: 'yellow'
				}
			}, {
				from: 10,
				to: 8,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'white',
					highlight: 'yellow'
				}
			}, {
				from: 8,
				to: 2,
				length: 300,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'white',
					highlight: 'yellow'
				}
			}]
	},
	reportsComponent:{
		componentCount: 2,
		nodes: [
			{
			id: 11,
			group: 2,
			label: 'Alarm',
			image: 'https://www.getkitsune.com/assets/images/alicloud/alaram.png',
			description: 'This component is used to generate scheduled triggers.'
		}, {
			id: 12,
			group: 2,
			label: 'Function Compute\n(Report Generator)',
			image: 'https://www.getkitsune.com/assets/images/alicloud/fc.png',
			description: 'This function compute encapsulates the logic of sending out the scheduled dynamic reports.'
		}],
		edges: [{
			from: 11,
			to: 12,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}, {
			from: 12,
			to: 1,
			length: 300,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}]
	},
	callTrackerComponent:{
		componentCount: 2,
		nodes: [{
			id: 21,
			group: 3,
			label: 'Third Party Call-Tracking\nNumber Provider',
			image: 'https://www.getkitsune.com/assets/images/aws/third_party_endpoint.png'
		}, {
			id: 22,
			group: 3,
			label: 'Function Compute\n(Call Tracking\nNumber Generator)',
			image: 'https://www.getkitsune.com/assets/images/alicloud/fc.png'
		}],
		edges: [{
			from: 22,
			to: 21,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}, {
			from: 10,
			to: 22,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}]
	},
	pgComponent:{
		componentCount: 1,
		nodes: [{
			id: 32,
			group: 4,
			label: 'Function Compute\n(k-pay handler)',
			image: 'https://www.getkitsune.com/assets/images/alicloud/fc.png'
		}, {
			id: 33,
			group: 4,
			label: 'Payment Gateway\nProvider',
			image: 'https://www.getkitsune.com/assets/images/aws/third_party_endpoint.png'
		}],
		edges: [ {
			from: 2,
			to: 32,
			length: 300,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 32,
			to: 6,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 33,
			to: 32,
			label: 'callback',
			dashes: true,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}]
	},
	containerisedAppComponent:{
		componentCount: 2,
		nodes: [{
			id: 41,
			group: 5,
			label: 'CaaS',
			image: 'https://www.getkitsune.com/assets/images/aws/caas.png',
			description: 'Container As a Service is used to scale the containerised version of your legacy application.'
		}, {
			id: 42,
			group: 5,
			label: 'C1',
			image: 'https://www.getkitsune.com/assets/images/docker.png',
			description: 'Represents a single container instance of your legacy application'
		}, {
			id: 43,
			group: 5,
			label: 'C2',
			image: 'https://www.getkitsune.com/assets/images/docker.png',
			description: 'Represents a single container instance of your legacy application'
		},
		{
			id: 44,
			group: 5,
			label: 'C3',
			image: 'https://www.getkitsune.com/assets/images/docker.png',
			description: 'Represents a single container instance of your legacy application'
		}],
		edges: [{
			from: 2,
			to: 41,
			length: 700,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 41,
			to: 42,
			length: 100,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 41,
			to: 43,
			length: 100,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 41,
			to: 44,
			length: 100,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}]	
	}
};

export const pingan = {
	staticWebsite: {
		componentCount: 2,
		nodes: [
			{
				id: 1,
				label: 'Users',
				image: 'https://www.getkitsune.com/assets/images/aws/users.png',
				description: 'This represents the primary user of your application',
				font: {
					color: 'white'
				}
			}, {
				id: 2,
				label: 'CDN',
				image: 'https://www.getkitsune.com/assets/images/pingan/cdn.png',
				description: 'CDN (Content Delivery Network) is used to accelerate the content delivery to the user.',
				font: {
					color: 'white'
				}
			}, {
				id: 3,
				label: 'OBS',
				image: 'https://www.getkitsune.com/assets/images/pingan/obs.png',
				description: 'OBS is used to store the resources of your application that do not change frequently.',
				font: {
					color: 'white'
				}
			}
		],
		edges: [
			{
				from: 1,
				to: 2,
				length: 300,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 2,
				to: 3,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}
		]
	},
	dynamicWebsite: {
		componentCount: 8,
		nodes: [
			{
				id: 1,
				group: 0,
				label: 'Users',
				image: 'https://www.getkitsune.com/assets/images/aws/users.png',
				description: 'This represents the primary user of your application'
			}, {
				id: 2,
				group: 0,
				label: 'CDN',
				image: 'https://www.getkitsune.com/assets/images/pingan/cdn.png',
				description: 'CDN (Content Delivery Network) is used to accelerate the content delivery to the user.'
			}, {
				id: 3,
				group: 0,
				label: 'Function Compute',
				image: 'https://www.getkitsune.com/assets/images/pingan/fc.png'
			}, {
				id: 4,
				group: 0,
				label: 'OBS',
				image: 'https://www.getkitsune.com/assets/images/pingan/obs.png',
				description: 'OBS is used to store the resources of your application that do not change frequently.',
			}, {
				id: 5,
				group: 0,
				label: 'Function Compute\n(Layout Manager)',
				image: 'https://www.getkitsune.com/assets/images/pingan/fc.png',
				description: 'This is the main application compute logic, which handles all database related access. With the compute being built with Lambda, you only need to pay for the exact processing time. This makes it cost-effective as you never have to pay for up time.'
			}, {
				id: 6,
				group: 0,
				label: 'MongoDB',
				image: 'https://www.getkitsune.com/assets/images/pingan/mongo.png',
				description: 'Mongo is used as persistent storage provider. The NoSql architecture enables you to add/edit new dynamic properties in your application.'
			}, {
				id: 7,
				group: 1,
				label: 'API Gateway',
				image: 'https://www.getkitsune.com/assets/images/pingan/api.png',
				description: 'API Gateway is used to enable REST based dynamic data-management APIs. These APIs can be integrated with downstream applications (like ERP, CRM, Inventory Mgmt etc) to add/edit dynamic values stored in Mongo.'
			}, {
				id: 8,
				group: 1,
				label: 'Function Compute\n(Cache Evictor)',
				image: 'https://www.getkitsune.com/assets/images/pingan/fc.png',
				description: 'A function compute based cache manager. Whenever there is a change in dynamic properties, this function knows how to invalidate the appropiate assets on CDN.'
			}, {
				id: 9,
				group: 1,
				label: 'Web Admin\n(CMS/API)',
				image: 'https://www.getkitsune.com/assets/images/aws/user.png',
				description: 'This represents the admin of your application who can login via the CMS and edit dynamic properties. You can also integrate with other down stream applications directly by using the Management APIs created as part of this architecture.'
			}, {
				id: 10,
				group: 1,
				label: 'SQS\n(Event Queue)',
				image: 'https://www.getkitsune.com/assets/images/pingan/queue.png'
			}],
		edges: [
			{
				from: 1,
				to: 2,
				length: 300,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 2,
				to: 3,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 3,
				to: 4,
				length:200,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 3,
				to: 5,
				length: 200,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 5,
				to: 6,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 5,
				to: 4,
				color: {
					color: 'yellow',
					highlight: 'yellow'
				}
			}, {
				from: 9,
				to: 7,
				length: 300,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'white',
					highlight: 'yellow'
				}
			}, {
				from: 7,
				to: 6,
				length: 300,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'white',
					highlight: 'yellow'
				}
			}, {
				from: 6,
				to: 10,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'white',
					highlight: 'yellow'
				}
			}, {
				from: 10,
				to: 8,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'white',
					highlight: 'yellow'
				}
			}, {
				from: 8,
				to: 2,
				length: 300,
				dashes: [2, 2, 10, 10],
				color: {
					color: 'white',
					highlight: 'yellow'
				}
			}]
	},
	reportsComponent:{
		componentCount: 2,
		nodes: [
			{
			id: 11,
			group: 2,
			label: 'Cloudwatch Alarm',
			image: 'https://www.getkitsune.com/assets/images/pingan/cloudwatch.png'
		}, {
			id: 12,
			group: 2,
			label: 'Function Compute\n(Report Generator)',
			image: 'https://www.getkitsune.com/assets/images/pingan/fc.png'
		}],
		edges: [{
			from: 11,
			to: 12,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}, {
			from: 12,
			to: 1,
			length: 600,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}]
	},
	callTrackerComponent:{
		componentCount: 2,
		nodes: [{
			id: 21,
			group: 3,
			label: 'Third Party Call-Tracking\nNumber Provider',
			image: 'https://www.getkitsune.com/assets/images/aws/third_party_endpoint.png'
		}, {
			id: 22,
			group: 3,
			label: 'Function Compute\n(Call Tracking\nNumber Generator)',
			image: 'https://www.getkitsune.com/assets/images/pingan/fc.png'
		}],
		edges: [{
			from: 22,
			to: 21,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}, {
			from: 10,
			to: 22,
			dashes: true,
			color: {
				color: 'lightgray',
				highlight: 'yellow'
			}
		}]
	},
	pgComponent:{
		componentCount: 2,
		nodes: [{
			id: 31,
			group: 4,
			label: 'Function Compute\n(Price Checksum Validator)',
			image: 'https://www.getkitsune.com/assets/images/pingan/fc.png'
		}, {
			id: 32,
			group: 4,
			label: 'Function Compute\n(k-pay handler)',
			image: 'https://www.getkitsune.com/assets/images/pingan/fc.png'
		}, {
			id: 33,
			group: 4,
			label: 'Payment Gateway\nProvider',
			image: 'https://www.getkitsune.com/assets/images/aws/third_party_endpoint.png'
		}],
		edges: [{
			from: 31,
			to: 1,
			length: 600,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 2,
			to: 31,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 2,
			to: 32,
			length: 300,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 32,
			to: 33,
			dashes: true,
			label: 'callback',
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}]
	},
	containerisedAppComponent:{
		componentCount: 2,
		nodes: [{
			id: 41,
			group: 5,
			label: 'CaaS',
			image: 'https://www.getkitsune.com/assets/images/pingan/caas.png'
		}, {
			id: 42,
			group: 5,
			label: 'C1',
			image: 'https://www.getkitsune.com/assets/images/docker.png'
		}, {
			id: 43,
			group: 5,
			label: 'C2',
			image: 'https://www.getkitsune.com/assets/images/docker.png'
		},
		{
			id: 44,
			group: 5,
			label: 'C3',
			image: 'https://www.getkitsune.com/assets/images/docker.png'
		}],
		edges: [{
			from: 2,
			to: 41,
			length: 700,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 41,
			to: 42,
			length: 100,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 41,
			to: 43,
			length: 100,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}, {
			from: 41,
			to: 44,
			length: 100,
			color: {
				color: 'yellow',
				highlight: 'yellow'
			}
		}]	
	}
};