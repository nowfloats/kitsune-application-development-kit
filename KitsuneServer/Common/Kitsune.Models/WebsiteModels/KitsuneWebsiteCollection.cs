using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using Kitsune.Models.Cloud;

namespace Kitsune.Models.WebsiteModels
{
	/// <summary>
	/// TO-DO add CDN Config, S3 Logs path
	/// DB model for K-Admin user login
	/// </summary>
	public class KitsuneWebsiteCollection : MongoEntity
	{
		/// <summary>
		/// Kitsune Developer Id
		/// </summary>
		public string DeveloperId { get; set; }

		/// <summary>
		/// Project Id associated with the user
		/// </summary>
		public string ProjectId { get; set; }

		/// <summary>
		/// Cluster of websites grouped under one, used only by Group owner to show stats, billing
		/// </summary>
		public string GroupWebsiteId { get; set; }

		/// <summary>
		/// Client id of the partner
		/// </summary>
		public string ClientId { get; set; }

		/// <summary>
		/// TODO : Check the use case of RootPath/ WWW foler path
		/// </summary>
		public string RootPath { get; set; }

		/// <summary>
		/// Website unique name
		/// [[WEBSITENAME]].getkitsune.com
		/// websitename.demo.getkitsuen.com
		/// </summary>
		public string WebsiteTag { get; set; }

		/// <summary>
		/// Website domain url (.getkitsune.com / custom domain), put rootaliasurl to this only
		/// </summary>
		public string WebsiteUrl { get; set; }

		/// <summary>
		/// Website published version
		/// </summary>
		public int KitsuneProjectVersion { get; set; }

		/// <summary>
		/// Updated on date time
		/// </summary>
		public DateTime UpdatedOn { get; set; }

		/// <summary>
		/// Flag for the site is active or not (updated based on the balance)
		/// </summary>
		public bool IsActive { get; set; }

		/// <summary>
		/// Website is archived or not (permanent remove of website)
		/// </summary>
		public bool IsArchived { get; set; }
	}

	#region CNAME mapper collection
	public enum DNSStatus
	{
		Active = 1,
		Pending = 2,
		InActive = 0,
		Error = -1
	}

	/// <summary>
	/// Centralized CNAME mapping with website collection 
	/// </summary>
	public class WebsiteDNSInfo : MongoEntity
	{
		public string WebsiteId { get; set; }

		public string DomainName { get; set; }

		/// <summary>
		/// TODO : Check the use case of RootPath, only applicable for verfied custom domains
		/// </summary>
		public string RootPath { get; set; }
		public bool IsSSLEnabled { get; set; }

		[BsonRepresentation(BsonType.String)]
		public DNSStatus DNSStatus { get; set; }

		public WebsiteSettings WebsiteConfiguration { get; set; }
	}

	public class WebsiteSettings
	{
		public CloudProvider CloudProviderType;
		public string CDNHostName;
		public string CDNId { get; set; }
		//AliasCNAME - is the central hop for a domain to point, aliasCNAME indeed will point to CDNHostName
		public string AliasCNAME;
	}

	public enum CDNProvider
	{
		CLOUDFRONT,
		AKAMAI,
		AZURE,
		CLOUDFLARE,
		FASTLY,
		GCP
	}

	public class WebsiteCacheStatus : MongoEntity
	{
		public string WebsiteId { get; set; }
		public bool Enabled { get; set; }
		public CloudProvider CloudProvider { get; set; }
		public DateTime LastInvalidate { get; set; }
		public DateTime NextInvalidate { get; set; }
	}
	#endregion

	#region K-Admin user login collection

	/// <summary>
	/// Contact details for k-admin user
	/// </summary>
	public class ContactDetails
	{
		public string FullName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public Address Address { get; set; }
	}

	public enum KitsuneWebsiteAccessType
	{
		/// <summary>
		/// Owner of website, can add or remove the access of the website 
		/// </summary>
		Owner = 0,

		/// <summary>
		/// Read only access of website
		/// </summary>
		ReadOnly = 1,

		/// <summary>
		/// Admin access to website (add / update data), can not update the website user access
		/// </summary>
		Admin = 2
	}

	/// <summary>
	/// Kitsune Website users mongo collection 
	/// </summary>
	public class KitsuneWebsiteUserCollection : MongoEntity
	{
		/// <summary>
		/// Kitsune Developer Id
		/// </summary>
		public string DeveloperId { get; set; }

		/// <summary>
		/// Login UserName / Email
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// Login password 
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// WebsiteId
		/// </summary>
		public string WebsiteId { get; set; }

		/// <summary>
		/// Website access type
		/// </summary>
		[BsonRepresentation(BsonType.String)]
		public KitsuneWebsiteAccessType AccessType { get; set; }

		/// <summary>
		/// User contact details
		/// </summary>
		public ContactDetails Contact { get; set; }

		public bool IsActive { get; set; }

		public DateTime LastLoginTimeStamp { get; set; }
		public DateTime UpdatedOn { get; set; }
	}

	#endregion
}
