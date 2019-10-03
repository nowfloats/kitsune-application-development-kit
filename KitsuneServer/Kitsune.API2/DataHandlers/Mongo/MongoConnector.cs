
using AmazonAWSHelpers.SQSQueueHandler;
using AWS.Services.CloudFrontHelper;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.EnvConstants;
using Kitsune.API2.Utils;
using Kitsune.BasePlugin.Utils;
//using Kitsune.Helper;
//using Kitsune.Helper.CrawlerConstants;
using Kitsune.Models;
using Kitsune.Models.Krawler;
using Kitsune.Models.Project;
using Kitsune.Models.WebsiteModels;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kitsune.API2.DataHandlers.Mongo
{
    public static partial class MongoConnector
    {
        private static bool _isDev = true.Equals(EnvironmentConstants.ApplicationConfiguration.IsDev);
        private static MongoClient _kitsuneServer;
        private static MongoClient _kitsuneSchemaServer;
        private static IMongoDatabase _kitsuneDatabase;
        private static IMongoDatabase _kitsuneSchemaDatabase;
        static IMongoClient floatDBclient;
        static IMongoDatabase _floatDatabase;

        static IMongoClient logDBclient;
        static IMongoDatabase _logDatabase;


        //private static ILogger logger = Log.Logger;

        public static string KitsuneWebsiteUserCollectionName = "KitsuneWebsiteUsers";
        public static string KitsuneWebsiteCollectionName = "KitsuneWebsites";
        public static string KitsuneUsagePredictionCollectionName = "kUsagePredictions";
        public static string KitsuneBalanceAlertCollectionName = "kBalanceAlert";
        public static string KitsuneWebsiteDNSInfoCollectionName = "KitsuneWebsiteDNS";
        public static string KitsuneWebsiteCacheStatusCollectionName = "KitsuneWebsiteCacheStatus";


        public static string KitsuneUserCollectionName = "users";
        public static string KitsuneLanguageCollectionName = "KitsuneLanguages";
        public static string KitsuneProdLanguageCollectionName = "KitsuneLanguagesProduction";
        public static string TaskDownloadQueueCollectionName = "KitsuneDownloadTask";
        public static string KitsunePublishStatsCollectionName = "KitsunePublishStats";

        public static string KitsuneProjectsCollectionName = "KitsuneProjects";
        public static string KitsuneResourceCollectionName = "KitsuneResources";
        public static string AuditProjectCollectionName = "KitsuneProjectsAudit";
        public static string AuditResourcesCollectionName = "KitsuneResourcesAudit";
        public static string ProductionProjectCollectionName = "KitsuneProjectsProduction";
        public static string ProductionResorcesCollectionName = "KitsuneResourcesProduction";

        public static string KitsuneCloudProviderCollectionName = "KitsuneCloudProviderDetails";

        public static string BuildStatusCollectionName = "KitsuneBuildStatus";
        public static string EnquiryCollectionName = "KitsuneEnquiryCollection";

        public static string KitsuneKrawlStatsCollection = "KitsuneKrawlStats";
        public static string KitsuneWordPressCollection = "KitsuneWordPress";
        public static string KitsuneBillingCollectionName = "KitsuneBillingRecords";
        public static string InstamojoTransactionLogCollectionName = "PaymentTransactionLogs";
        public static string MonthlyInvoiceCollectionName = "KitsuneInvoice";
        public static string KitsuneOptimizationReportsCollection = "KitsuneOptimizationReports";

        public static string KitsuneActivityCollection = "KitsuneActivity";

        private static string nfDBName = "floatdb";
        private static string logDBName = "logdb";

        private static readonly string ProductionPageCollection = "ProductionThemePages";

        #region collection keys
        private const string COLLECTION_KEY_ID = "_id";
        private const string COLLECTION_KEY_CREATED_ON = "createdon";
        private const string COLLECTION_KEY_UPDATED_ON = "updatedon";
        private const string COLLECTION_KEY_PARENT_CLASS_ID = "_parentClassId";
        private const string COLLECTION_KEY_PARENT_CLASS_NAME = "_parentClassName";
        private const string COLLECTION_KEY_PROPERTY_NAME = "_propertyName";
        private const string COLLECTION_KEY_KID = "_kid";
        private const string COLLECTION_KEY_WEBISTE_ID = "websiteid";
        private const string COLLECTION_KEY_IS_ARCHIVED = "isarchived";
        private const string COLLECTION_KEY_REFLECTION_ID = "_reflectionId";
        #endregion

        #region function keys
        private const string FUNCTION_NAME_LENGTH = "length";

        #endregion

        internal static void InitializeConnection()
        {
            try
            {
                var configurationSettings = EnvironmentConstants.ApplicationConfiguration;

                try
                {
                    if (_kitsuneServer == null)
                    {
                        _kitsuneServer = new MongoClient(configurationSettings.DBConnectionStrings.KitsuneDBMongoConnectionUrl);
                        _kitsuneDatabase = _kitsuneServer.GetDatabase(configurationSettings.DatabaseName);
                    }
                }
                catch { }

                try
                {
                    if (_kitsuneSchemaServer == null)
                    {
                        _kitsuneSchemaServer = new MongoClient(configurationSettings.DBConnectionStrings.KitsuneSchemaDBMongoConnectionUrl);
                        _kitsuneSchemaDatabase = _kitsuneSchemaServer.GetDatabase(configurationSettings.SchemaDatabaseName);
                    }
                }
                catch { }

                try
                {
                    if (floatDBclient == null)
                    {
                        floatDBclient = new MongoClient(configurationSettings.DBConnectionStrings.FPDBMongoConnectionUrl);
                        _floatDatabase = floatDBclient.GetDatabase(nfDBName);
                    }
                }
                catch { }

                try
                {
                    if (logDBclient == null)
                    {
                        logDBclient = new MongoClient(configurationSettings.DBConnectionStrings.FPDBMongoConnectionUrl);
                        _logDatabase = logDBclient.GetDatabase(logDBName);
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                var emailRequestModel = new EmailRequestWithAttachments
                {
                    EmailBody = $"Mongo Exception  : {ex.Message}, stack trace : {ex.StackTrace}",
                    Subject = "Kitsune API MongoException",
                    From = "team@getkitsune.com",
                    To = new List<string> { "chirag.m@getkitsune.com", }
                };
                new EmailHelper().SendEmail(emailRequestModel, EmailUserConfigType.WEBSITE_DEVELOPER, Kitsune.Constants.ClientIdConstants._defaultClientId);
                throw ex;
            }
        }

        #region Developer details - READ, UPDATE

        internal static GetUserIdResult GetUserIdFromUserEmail(GetUserIdRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var collection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);

                ProjectionDefinitionBuilder<UserModel> project = new ProjectionDefinitionBuilder<UserModel>();
                var userId = collection.Find(x => x.UserName == requestModel.UserEmail).Project(x => x._id).First();

                if (String.IsNullOrEmpty(userId))
                    return new GetUserIdResult { IsError = true, Message = "user id not found" };

                return new GetUserIdResult { Id = userId, IsError = false };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static UserModel GetDeveloperDetailsFromId(string userId)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var collection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
                return collection.Find(x => x._id == userId).First();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }

        internal static GetDeveloperProfileResponseModel GetDeveloperProfileDetails(GetDeveloperProfileRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var userCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
                var KitsuneProjectsCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);

                //var themeCollection = KitsuneProjectsCollection;
                var ProjectsProject = new ProjectionDefinitionBuilder<KitsuneProject>();
                var UserProject = new ProjectionDefinitionBuilder<UserModel>();
                var userProject = UserProject
                    .Include(x => x.About)
                    .Include(x => x.CreatedOn)
                    .Include(x => x.DisplayName)
                    .Include(x => x.Facebook)
                    .Include(x => x.Github)
                    .Include(x => x.Google)
                    .Include(x => x.IsArchived)
                    .Include(x => x.Level)
                    .Include(x => x.ProfilePic)
                    .Include(x => x.Twitter)
                    .Include(x => x.UpdatedOn)
                    .Include(x => x.Wallet)
                    .Include(x => x.UserName)
                    .Include(x => x.Address)
                    .Include(x => x.PhoneNumber)
                    .Include(x => x.CreatedOn)
                    .Include(x => x.GSTIN)
                    .Include(x => x.FirstName)
                    .Include(x => x.LastName);

                var users = userCollection.Find(x => x.UserName == requestModel.UserEmail).Project<UserModel>(userProject).Limit(1).ToList();
                if (users != null && users.Any())
                {
                    var UserProjects = KitsuneProjectsCollection.Find(x => x.UserEmail == requestModel.UserEmail && x.IsArchived == false).Project<KitsuneProject>(ProjectsProject.Include(x => x.ProjectName).Include(x => x.ProjectId)).ToList();
                    var userDetails = users.First();
                    if (userDetails != null)
                    {
                        var responseModel = new GetDeveloperProfileResponseModel
                        {
                            About = userDetails.About,
                            DisplayName = userDetails.DisplayName,
                            Email = userDetails.UserName,
                            Facebook = userDetails.Facebook,
                            FollowersCount = 0,
                            FollowingCount = 0,
                            Github = userDetails.Github,
                            Google = userDetails.Google,
                            ProfilePic = userDetails.ProfilePic,
                            Twitter = userDetails.Twitter,
                            UpdatedOn = userDetails.UpdatedOn,
                            UserName = userDetails.UserName,
                            PhoneNumber = userDetails.PhoneNumber,
                            Projects = UserProjects != null ? UserProjects.Select(x => new ProjectItem { ProjectId = x.ProjectId, ProjectName = x.ProjectName }).ToList() : null,
                            CreatedOn = userDetails.CreatedOn,
                            GSTIN = userDetails.GSTIN,
                            FirstName = userDetails.FirstName,
                            LastName = userDetails.LastName,
                            Level = userDetails.Level,
                            Address = userDetails.Address,
                            Wallet = userDetails.Wallet
                        };



                        return responseModel;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        internal static GetUserPaymentStatsResponseModel GetDeveloperPaymentTransactionLogs(GetUserPaymentStatsRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var collection = _kitsuneDatabase.GetCollection<PaymentTransactionLog>(InstamojoTransactionLogCollectionName);

                var res = collection.Find<PaymentTransactionLog>(x => x.UserProfileId == requestModel.UserEmail).ToListAsync().Result;
                if (res == null)
                    return null;

                GetUserPaymentStatsResponseModel list = new GetUserPaymentStatsResponseModel
                {
                    WalletStats = new List<PaymentStats>()
                };
                foreach (var walletDetails in res)
                {
                    try
                    {
                        if (walletDetails.Status.ToLower() == "credit" || walletDetails.Status.ToLower() == "debit")
                            list.WalletStats.Add(new PaymentStats { Amount = walletDetails.Amount, InvoiceId = walletDetails.InvoiceId, UpdatedOn = walletDetails.CreatedOn, Status = walletDetails.Status, DebitDetail = walletDetails.DebitDetail });
                    }
                    catch (Exception ex)
                    {

                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// TODO: re-check and optimize
        internal static GetDeveloperDebitDetailsResponseModel<KitsuneBillingModel, DebitDetailsMetaData> GetDeveloperDebitDetails(GetDeveloperDebitDetailsRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                var month = requestModel.MonthAndYear;
                var component = requestModel.Component;
                if (String.IsNullOrEmpty(component))
                    component = "WEBREQUESTS"; //TODO: Get all the components from DB

                if (requestModel.MonthAndYear == default(DateTime))
                    month = DateTime.Now;

                if (_kitsuneServer == null)
                    InitializeConnection();

                var collection = _kitsuneDatabase.GetCollection<KitsuneBillingModel>(KitsuneBillingCollectionName);

                //get first billing date from DB
                var date = collection.Find(x => x.for_user == requestModel.UserEmail && x.component.Equals(component))
                    .SortBy(x => x.from_date).Project(x => x.from_date).Limit(1).FirstOrDefault();

                if (date == default(DateTime))
                    date = DateTime.Now;

                List<DateTime> months = new List<DateTime>();
                for (DateTime i = date; i <= DateTime.Now; i = i.AddMonths(1))
                {
                    months.Add(i);
                }

                #region Meta Data

                DebitDetailsMetaData metaData = new DebitDetailsMetaData
                {
                    CurrentComponent = component,
                    CurrentMonthAndYear = month,
                    ComponentsUsed = new List<string>() { component },
                    MonthAndYears = months
                };

                #endregion

                #region Debit Details

                var startDate = new DateTime(month.Year, month.Month, 1);
                var nextDate = startDate.AddMonths(1);
                nextDate = new DateTime(nextDate.Year, nextDate.Month, 1);
                var debitsDebitDetails = collection
                    .Find(x => x.for_user == requestModel.UserEmail && x.component.Equals(component)
                                                               && x.from_date >= startDate
                                                               && x.from_date < nextDate)
                    .SortByDescending(x => x.from_date)
                    .ToList();

                #endregion

                return new GetDeveloperDebitDetailsResponseModel<KitsuneBillingModel, DebitDetailsMetaData> { Usage = debitsDebitDetails, Meta = metaData };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static double UpdateDeveloperWalletDetails(UpdateDeveloperWalletRequestModel command)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var pdb = new ProjectionDefinitionBuilder<UserModel>();
                var pd = pdb.Include(x => x.Wallet);

                var userCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
                var user = userCollection.Find(x => x.UserName == command.UserEmail).Project<UserModel>(pd).FirstOrDefaultAsync().Result;
                if (user != null)
                {

                    var builder = new UpdateDefinitionBuilder<UserModel>();
                    var currentTime = DateTime.Now;
                    if (user.Wallet != null && command.Add)
                    {
                        user.Wallet.Balance += command.Amount;
                        user.Wallet.UpdatedOn = currentTime;
                    }
                    else if (user.Wallet != null && !command.Add)
                    {
                        user.Wallet.UpdatedOn = currentTime;
                        user.Wallet.Balance -= command.Amount;
                    }
                    else if (user.Wallet == null && command.Add)
                        user.Wallet = new Wallet { Balance = command.Amount, UpdatedOn = DateTime.Now };

                    userCollection.UpdateOne((x => x.UserName == command.UserEmail), builder.Set(x => x.Wallet, user.Wallet));
                    return user.Wallet.Balance;
                }

            }
            catch (Exception ex)
            {
            }
            return -1;
        }

        internal static bool UpdateDeveloperDetails(UpdateDeveloperDetailsRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var pdb = new ProjectionDefinitionBuilder<UserModel>();
                var pd = pdb.Include(x => x.Wallet);
                var userCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
                var user = userCollection.Find(x => x.UserName == requestModel.UserEmail).Project<UserModel>(pd).FirstOrDefaultAsync().Result;
                if (user != null)
                {
                    var builder = new UpdateDefinitionBuilder<UserModel>();
                    var update = builder.Set(x => x.UpdatedOn, DateTime.Now);
                    if (!string.IsNullOrEmpty(requestModel.GSTIN))
                        update = update.Set(x => x.GSTIN, requestModel.GSTIN);
                    if (!string.IsNullOrEmpty(requestModel.PhoneNumber))
                        update = update.Set(x => x.PhoneNumber, requestModel.PhoneNumber);
                    if (requestModel.Address != null)
                        update = update.Set(x => x.Address, requestModel.Address);
                    if (!string.IsNullOrEmpty(requestModel.Name))
                        update = update.Set(x => x.DisplayName, requestModel.Name);
                    if (!string.IsNullOrEmpty(requestModel.FirstName))
                        update = update.Set(x => x.FirstName, requestModel.FirstName);
                    if (!string.IsNullOrEmpty(requestModel.LastName))
                        update = update.Set(x => x.LastName, requestModel.LastName);

                    userCollection.UpdateOne((x => x.UserName == requestModel.UserEmail), update);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal static bool DeveloperPasswordReset(string username, string oldpassword, string newpassword)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var userCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
                username = username.Trim().ToLower();
                var loginResponse = DeveloperLogin(username, oldpassword);
                if (loginResponse != null)
                {
                    var securityStamp = GenerateMD5Hash(newpassword.Trim());
                    var updateResult = userCollection.UpdateOne<UserModel>((x => x._id == loginResponse.Id), 
                        new UpdateDefinitionBuilder<UserModel>().Set(x => x.SecurityStamp, securityStamp).Set(x => x.UpdatedOn, DateTime.UtcNow));
                    if (updateResult != null && updateResult.IsAcknowledged && updateResult.ModifiedCount == 1)
                    {
                        return true;
                    }
                }

                
            }
            catch
            {

            }
            return false;
        }
        internal static LoginResponse DeveloperLogin(string username, string password)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var userCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
                username = username.Trim().ToLower();
                var securityStamp = GenerateMD5Hash(password.Trim());
                var userEmailCheck = userCollection.Find(x => x.Email == username && x.SecurityStamp == securityStamp).Limit(1).FirstOrDefault();
                if(userEmailCheck != null)
                {
                    return new LoginResponse
                    {
                        DisplayName = userEmailCheck.DisplayName,
                        Email = userEmailCheck.Email,
                        FirstName = userEmailCheck.FirstName,
                        LastName = userEmailCheck.LastName,
                        Id = userEmailCheck._id,
                        ProfilePic = userEmailCheck.ProfilePic
                    };
                }
            }
            catch
            {

            }
            return null;
        }
        internal static string CreateDeveloperProfile(CreateDeveloperProfileRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                requestModel.UserEmail = requestModel.UserEmail.Trim().ToLower();

                var userCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
                var userEmailCheck = userCollection.Find(x => x.Email == requestModel.UserEmail).Limit(1).FirstOrDefault();
                if (userEmailCheck != null)
                    throw new Exception("UserEmail already Exist");
                var dateNow = DateTime.Now;
                var userId = ObjectId.GenerateNewId().ToString();

                if(!string.IsNullOrEmpty(requestModel.Password))
                {
                    requestModel.SecurityStamp = GenerateMD5Hash(requestModel.Password.Trim());
                    requestModel.Logins = new List<LoginField>
                    {
                        new LoginField
                        {
                            LoginProvider = "kitsune-custom"
                        }
                    };
                }

                var newDeveloper = new UserModel
                {
                    _id = userId,
                    UserName = requestModel.UserName,
                    Email = requestModel.UserEmail,
                    DisplayName = requestModel.DisplayName,
                    ProfilePic = requestModel.ProfilePic,
                    Logins = requestModel.Logins,
                    CreatedOn = dateNow,
                    UpdatedOn = dateNow,
                    SecurityStamp = requestModel.SecurityStamp,
                    FirstName = requestModel.FirstName,
                    LastName = requestModel.LastName,

                };
                userCollection.InsertOne(newDeveloper);
                return userId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal static string GenerateMD5Hash(string input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input))
                {
                    var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    var md5data = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
                    return System.Text.Encoding.ASCII.GetString(md5data);
                }
            }
            catch
            {
            }
            return null;

        }
        internal static GetInvoiceResponseModel GetInvoice(GetInvoiceRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var monthlyInvoiceCollection = _kitsuneDatabase.GetCollection<MonthlyInvoiceModel>(MonthlyInvoiceCollectionName);
                string[] dateArray = { "01", requestModel.month.ToString(), requestModel.year.ToString() };
                string dateString = string.Join(" ", dateArray);
                DateTime periodDateTime = DateTime.ParseExact(dateString, "d M yyyy", null);
                string queryPeriodEnd = periodDateTime.ToString(" MMM, yyyy");
                ProjectionDefinitionBuilder<MonthlyInvoiceModel> projectInvoice = new ProjectionDefinitionBuilder<MonthlyInvoiceModel>();

                MonthlyInvoiceModel InvoiceLink = monthlyInvoiceCollection.Find(x => x.context.user_id == requestModel.UserEmail && x.context.period.EndsWith(queryPeriodEnd) && x.context.charge_components.Any(y => y.name == "Web Requests"))
                                                           .Project<MonthlyInvoiceModel>(projectInvoice.Include(x => x.link)).Limit(1).FirstOrDefault();
                if (InvoiceLink != null)
                {
                    return new GetInvoiceResponseModel
                    {
                        status = "Found",
                        S3Link = InvoiceLink.link
                    };
                }
                return new GetInvoiceResponseModel
                {
                    status = "NotFound",
                };
            }
            catch (Exception ex)
            {
                return new GetInvoiceResponseModel
                {
                    status = "Error"
                };
            }
        }

        #endregion

        #region Domain mapping details

        internal static KitsuneMapDomainResponseModel MapDomainToCustomer(KitsuneMapDomainRequestModel command)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);
                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var website = websiteCollection.Find(x => x._id == command.WebsiteId && x.IsActive == true && x.IsArchived == false).Project<KitsuneWebsiteCollection>(new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>().Include(y => y.ClientId)).FirstOrDefault();
                if (website == null)
                    throw new Exception($"No active website found with websiteid : {command.WebsiteId}");
                ProjectionDefinitionBuilder<KitsuneWebsiteCollection> projectDefination = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();

                //Update DNS details Collection


                var result = websiteDNSCollection.FindOneAndUpdate(x => x.WebsiteId == command.WebsiteId && x.DNSStatus == DNSStatus.Pending,
                    new UpdateDefinitionBuilder<WebsiteDNSInfo>().Set(x => x.DNSStatus, DNSStatus.Active));


                if (result == null)
                    throw new Exception("websiteid not found");
                else
                {
                    //Update all active domain to inactive apart from current
                    var config = BasePluginConfigGenerator.GetBasePlugin(website.ClientId);
                    var subdomain = config.GetSubDomain();
                    var dnsUpdateResult = websiteDNSCollection.UpdateMany((x => x.WebsiteId == command.WebsiteId
                                                                   && x._id != result._id
                                                                   && (x.DNSStatus == DNSStatus.Active)
                                                                   && !x.RootPath.Contains(subdomain)), new UpdateDefinitionBuilder<WebsiteDNSInfo>()
                                                                                                           .Set(x => x.DNSStatus, DNSStatus.InActive));

                    //Only kitsune websites has to be added to kitsune cdn
                    if (string.IsNullOrEmpty(website.ClientId) || website.ClientId == Constants.ClientIdConstants._defaultClientId)
                    {
                        //Add to CDN
                        UpdateCloudfrontDistribution.AddDomainToDistribution(EnvironmentConstants.ApplicationConfiguration.AWSCDNConfiguration.AWS_AccessKey,
                                EnvironmentConstants.ApplicationConfiguration.AWSCDNConfiguration.AWS_SecretKey, Kitsune.API2.EnvConstants.Constants.KitsuneIdentiferDistributionId, result.DomainName);
                    }

                    //Update domain WebsiteCollection
                    var websiteUpdateResult = websiteCollection.UpdateOne((x => x._id == command.WebsiteId), new UpdateDefinitionBuilder<KitsuneWebsiteCollection>()
                         .Set(x => x.WebsiteUrl, result.DomainName)
                         .Set(x => x.UpdatedOn, DateTime.UtcNow));
                    if (websiteUpdateResult != null && websiteUpdateResult.IsAcknowledged && websiteUpdateResult.ModifiedCount == 1)
                        return new KitsuneMapDomainResponseModel { IsError = false, Message = "success" };
                    return new KitsuneMapDomainResponseModel { IsError = true, Message = "could not update the website" };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static KitsuneCheckAndMapDomainResponseModel VerifyAndUpdateDomainMappingForCustomer(KitsuneCheckAndMapDomainRequestModel command, WebsiteDNSInfo dnsInfo = null)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                //get the domain name and kitsuneUrl
                var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);
                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

                WebsiteDNSInfo dnsDetails = null;

                //To avoid getting domain detail immediatly after insert (readreplica is not in sync)
                if (dnsInfo == null)
                {
                    dnsDetails = websiteDNSCollection
                                               .Find(x => x.WebsiteId == command.WebsiteId && x.DNSStatus == DNSStatus.Pending)
                                               .FirstOrDefault();
                }
                else
                {
                    dnsDetails = dnsInfo;
                }

                var kitsuneUrl = websiteCollection.Find(x => x._id == command.WebsiteId).Limit(1).FirstOrDefault();

                if (dnsDetails == null || kitsuneUrl == null)
                    throw new Exception("website dns not found");

                //if the domain is mapped update db and add to cloudfront

                //TODO : fatch the subdomain from the config plugin
                string kitsuneDomain = BasePluginConfigGenerator.GetBasePlugin(kitsuneUrl.ClientId).GetSubDomain();
                //Added edge-a.nowfloats.net support for nf Akamai domains auto verify
                if (DNSChecker.CheckIfCNAMEMapped(dnsDetails.DomainName, new List<string> { $"{kitsuneUrl.WebsiteTag.ToLower()}{kitsuneDomain.ToLower()}", "edge-a.nowfloats.net"}))
                {
                    var result = MapDomainToCustomer(new KitsuneMapDomainRequestModel() { WebsiteId = command.WebsiteId });
                    if (!result.IsError)
                    {
                        return new KitsuneCheckAndMapDomainResponseModel { IsError = false, IsMapped = true, Message = "success" };
                    }
                    else
                    {
                        throw new Exception(result.Message);
                    }
                }
                else
                {
                    return new KitsuneCheckAndMapDomainResponseModel { IsError = false, IsMapped = false };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static KitsuneProjectsWithDomainNameNotMappedResponseModel GetDomainNotMappedProjects(KitsuneProjectsWithDomainNameNotMappedRequestModel query)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);

                var listOfUnverifiedDomains = websiteDNSCollection.Find(x => x.DNSStatus == DNSStatus.Pending && x.CreatedOn >= DateTime.Now.AddDays((-1) * query.Days)).ToList();
                if (listOfUnverifiedDomains != null && listOfUnverifiedDomains.Any())
                {
                    return new KitsuneProjectsWithDomainNameNotMappedResponseModel
                    {
                        DomainList = listOfUnverifiedDomains.Select(x => new DomainNotMapped
                        {
                            CustomerId = x.WebsiteId,
                            Domain = x.DomainName
                        }).ToList()
                    };

                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static KitsuneUpdateDomainResponseModel UpdateDomainName(KitsuneUpdateDomainRequestModel command)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                if (String.IsNullOrEmpty(command.CustomerId) || String.IsNullOrEmpty(command.NewDomain))
                    throw new Exception("customerid or new domain cannot be null");

                var domain = command.NewDomain.Trim().ToUpper().Replace("HTTP://", "").Replace("HTTPS://", "");

                //get the domain name and kitsuneUrl
                var collection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var dnsCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);
                var project = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();
                var websiteDetails = collection.Find(x => x._id == command.CustomerId)
                                                .Project<KitsuneWebsiteCollection>(project.Include(x => x.WebsiteUrl)
                                                                               .Include(x => x.IsActive)
                                                                               .Include(x => x.IsArchived))
                                                .Limit(1).FirstOrDefault();

                if (websiteDetails == null) throw new Exception("website not found");
                if (!websiteDetails.IsActive) throw new Exception("website is not active");
                if (websiteDetails.IsArchived) throw new Exception("website is archived");

                var config = BasePluginConfigGenerator.GetBasePlugin(websiteDetails.ClientId);
                var subdomain = config.GetSubDomain();
                if (!domain.EndsWith(subdomain))
                {
                    var dnsUpdateResult = dnsCollection.UpdateMany((x => x.WebsiteId == command.CustomerId
                                                               && (x.DNSStatus == DNSStatus.Pending)
                                                               && !x.RootPath.Contains(subdomain)), new UpdateDefinitionBuilder<WebsiteDNSInfo>()
                                                                                                       .Set(x => x.DNSStatus, DNSStatus.InActive));
                    //if (pendingCount > 0)
                    //{
                    //    throw new Exception("Domain already requested");
                    //}

                    var dnsInfo = new WebsiteDNSInfo()
                    {
                        CreatedOn = DateTime.UtcNow,
                        DNSStatus = DNSStatus.Pending,
                        DomainName = domain,
                        IsSSLEnabled = false,
                        RootPath = domain,
                        WebsiteId = command.CustomerId
                    };

                    dnsCollection.InsertOne(dnsInfo);

                    if (!string.IsNullOrWhiteSpace(dnsInfo._id))
                    {
                        //Verify the domain if CNAME already updated
                        //Skip verify domain for subpath
                        if (command.NewDomain.IndexOf('/') == -1)
                            MongoConnector.VerifyAndUpdateDomainMappingForCustomer(new KitsuneCheckAndMapDomainRequestModel { WebsiteId = websiteDetails._id }, dnsInfo);

                        return new KitsuneUpdateDomainResponseModel { IsError = false, Message = "successfully updated new domain", Domain = command.NewDomain };
                    }
                    else
                        throw new Exception("error updating the document");
                }
                else
                {
                    //Dont add subdomain again. Its already added when website is created
                    return new KitsuneUpdateDomainResponseModel { IsError = false, Message = "subdomain already exist", Domain = command.NewDomain };
                }



            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static KitsuneRequestedDomainResponseModel RequestedDomain(KitsuneRequestedDomainRequestModel command)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);

                var listOfRequestedDomains = websiteDNSCollection.Find(x => x.DNSStatus == DNSStatus.Pending && x.WebsiteId.Equals(command.WebsiteId)).ToList();
                if (listOfRequestedDomains != null)
                {
                    return new KitsuneRequestedDomainResponseModel
                    {
                        RequestedDomains = listOfRequestedDomains
                    };

                }
                return new KitsuneRequestedDomainResponseModel { RequestedDomains = new List<WebsiteDNSInfo> { } };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Crawler Details

        internal static bool UpdateKitsuneProjectStatus(UpdateKitsuneProjectStatusRequestModel command)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var kitsuneProjectsColletion = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);

                var update = Builders<KitsuneProject>.Update.Set(x => x.ProjectStatus, command.ProjectStatus);
                kitsuneProjectsColletion.UpdateOne(x => x.ProjectId.Equals(command.ProjectId), update);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static bool UpdateWebsiteDetailsCommandHandler(UpdateWebsiteDetailsRequestModel command)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var kitsuneProjectsColletion = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);

                var update = Builders<KitsuneProject>.Update.Set(x => x.UpdatedOn, DateTime.Now);
                if (command.FaviconUrl != null)
                    update = update.Set(x => x.FaviconIconUrl, command.FaviconUrl);
                if (command.ScreenShotUrl != null)
                    update = update.Set(x => x.ScreenShotUrl, command.ScreenShotUrl);

                kitsuneProjectsColletion.UpdateOne(x => x.ProjectId.Equals(command.ProjectId), update);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static StartKrawlResponseModel StartKrawling(StartKrawlRequestModel command)
        {
            try
            {

                Uri uri = null;
                if (!Uri.TryCreate(command.Url, UriKind.Absolute, out uri))
                    return new StartKrawlResponseModel { IsError = true, Message = "invalid url" };

                if (_kitsuneServer == null)
                    InitializeConnection();

                var krawlStatsCollection = _kitsuneDatabase.GetCollection<KitsuneKrawlerStats>(KitsuneKrawlStatsCollection);

                #region Create New Project

                string projectName = String.IsNullOrEmpty(command.ProjectName) ? command.Url : command.ProjectName;
                var projectId = MongoConnector.CreateNewProject(projectName, command.UserEmail, command.ClientId, 0, ProjectType.CRAWL, ProjectStatus.CRAWLING);

                #endregion

                #region Create Krawl Stats

                KitsuneKrawlerStats krawlStats = new KitsuneKrawlerStats
                {
                    ProjectId = projectId,
                    Url = command.Url,
                    Stage = KitsuneKrawlerStatusCompletion.Initialising,
                    CreatedOn = DateTime.Now,
                    StopCrawl = false,
                    LinksLimit = 10000,
                    CrawlType = command.IsDeepKrawl ? KrawlType.DeepKrawl : KrawlType.ShallowKrawl
                };
                krawlStatsCollection.InsertOne(krawlStats);

                #endregion

                #region Push to SQS

                var sqsHandler = new AmazonSQSQueueHandlers<KrawlSQSModel>(AmazonAWSConstants.KrawlerSQSUrl);
                var result = sqsHandler.PushMessageToQueue(new KrawlSQSModel { ProjectId = projectId },
                    EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey);
                if (result == null)
                    return new StartKrawlResponseModel { IsError = true, Message = "error pushing to queue", ProjectId = projectId };

                #endregion

                return new StartKrawlResponseModel { IsError = false, Message = "success", ProjectId = projectId };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static CommonAPIResponse StopCrawling(StopCrawlRequestModel command)
        {
            if (command == null)
                return CommonAPIResponse.InternalServerError(new ArgumentNullException(nameof(command)));

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var krawlStatsCollection = _kitsuneDatabase.GetCollection<KitsuneKrawlerStats>(KitsuneKrawlStatsCollection);
                var kitsuneCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);


                #region Check Project Details

                var projectionBuilder = new ProjectionDefinitionBuilder<KitsuneKrawlerStats>();
                var project = projectionBuilder.Include(x => x.ProjectId)
                                               .Include(x => x.Stage)
                                               .Include(x => x.StopCrawl);
                var projectDetails = krawlStatsCollection.Find(x => x.ProjectId.Equals(command.ProjectId)).FirstOrDefault();
                if (projectDetails == null)
                    return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Project not found."));
                if (projectDetails.StopCrawl)
                    return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Crawling already stopped."));

                #endregion

                #region Update Krawl Status

                var updateKrawlStats = Builders<KitsuneKrawlerStats>.Update.Set(x => x.ProjectId, command.ProjectId);
                if (command.StopCrawl)
                    updateKrawlStats = updateKrawlStats.Set(x => x.StopCrawl, true);
                if (command.LinksLimit != null)
                    updateKrawlStats = updateKrawlStats.Set(x => x.LinksLimit, command.LinksLimit);
                var updateresult = krawlStatsCollection.UpdateOne(x => x.ProjectId.Equals(command.ProjectId), updateKrawlStats);

                #endregion

                #region Delete Project

                var updateDefinationBuilder = new UpdateDefinitionBuilder<KitsuneProject>();
                var update = updateDefinationBuilder.Set(x => x.IsArchived, true);
                var deleteResult = kitsuneCollection.UpdateOne(x => x.ProjectId.Equals(command.ProjectId), update);
                if (deleteResult.IsAcknowledged)
                {
                    return CommonAPIResponse.OK("Crawling stopped successfully");
                }
                else
                {
                    return CommonAPIResponse.OK("Failed to delete project");
                }

                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static StartKrawlResponseModel ReCrawl(ReCrawlRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var kitsuneProjectsColletion = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var krawlStatsCollection = _kitsuneDatabase.GetCollection<KitsuneKrawlerStats>(KitsuneKrawlStatsCollection);

                #region Check project status

                //Dont process if the project is already processing
                var filter = Builders<KitsuneProject>.Projection.Include(x => x.ProjectStatus)
                                                              .Include(x => x.ProjectId)
                                                              .Exclude(x => x._id);

                var projectDetails = kitsuneProjectsColletion.Find(x => x.ProjectId.Equals(requestModel.ProjectId)).Project<ReCrawlProjectProjection>(filter).Limit(1).FirstOrDefault();
                if (projectDetails == null)
                {
                    throw new Exception("Unable to get Project Details");
                }

                var projectActiveStatus = new List<ProjectStatus>() { ProjectStatus.BUILDING, ProjectStatus.CRAWLING, ProjectStatus.PUBLISHING, ProjectStatus.QUEUED };
                if (projectActiveStatus.Contains(projectDetails.ProjectStatus))
                    throw new Exception($"Project is {projectDetails.ProjectStatus}");

                #endregion

                #region Update Collection

                //Update KitsuneProject
                var updateDefination = Builders<KitsuneProject>.Update.Set(x => x.ProjectStatus, ProjectStatus.CRAWLING);
                kitsuneProjectsColletion.UpdateOne(x => x.ProjectId.Equals(requestModel.ProjectId), updateDefination);

                //Update CrawlStats
                var statsUpdateDefination = Builders<KitsuneKrawlerStats>.Update.Set(x => x.Stage, KitsuneKrawlerStatusCompletion.Initialising)
                                                                                .Set(x => x.Assets, new List<AssetDetails>())
                                                                                .Set(x => x.Scripts, new List<AssetDetails>())
                                                                                .Set(x => x.Styles, new List<AssetDetails>())
                                                                                .Set(x => x.Links, new List<AssetDetails>())
                                                                                .Set(x => x.LinksFound, 0)
                                                                                .Set(x => x.AssetsFound, 0)
                                                                                .Set(x => x.ScriptsFound, 0)
                                                                                .Set(x => x.StylesFound, 0)
                                                                                .Set(x => x.LinksReplaced, 0)
                                                                                .Set(x => x.ScriptsDownloaded, 0)
                                                                                .Set(x => x.AssetsDownloaded, 0)
                                                                                .Set(x => x.ScriptsDownloaded, 0)
                                                                                .Set(x => x.StylesDownloaded, 0)
                                                                                .Set(x => x.Url, requestModel.Url)
                                                                                .Set(x => x.ProjectId, requestModel.ProjectId);
                krawlStatsCollection.UpdateOne(x => x.ProjectId.Equals(requestModel.ProjectId), statsUpdateDefination, new UpdateOptions { IsUpsert = true });

                #endregion

                #region Push to Queue

                var sqsHandler = new AmazonSQSQueueHandlers<KrawlSQSModel>(AmazonAWSConstants.KrawlerSQSUrl);
                var result = sqsHandler.PushMessageToQueue(new KrawlSQSModel { ProjectId = requestModel.ProjectId, ReCrawl = true },
                    EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey);

                #endregion

                if (result == null)
                    throw new Exception("Error Pushing to Queue");

                return new StartKrawlResponseModel { IsError = false, Message = "success", ProjectId = requestModel.ProjectId };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static bool KrawlingCompletedUpdateKitsuneProjects(KrawlingCompletedRequestModel command)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var kitsuneProjectsColletion = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var krawlStatsCollection = _kitsuneDatabase.GetCollection<KitsuneKrawlerStats>(KitsuneKrawlStatsCollection);
                var kitsuneResourcesCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);


                #region Update KitsuneProjectDB

                //Filter Defination
                var fdb = new FilterDefinitionBuilder<KitsuneProject>();
                var filter = fdb.Where(x => x.ProjectId == command.ProjectId);

                //Update Defination
                var udb = new UpdateDefinitionBuilder<KitsuneProject>();
                var update = udb.Set(x => x.ProjectStatus, ProjectStatus.IDLE)
                                .Set(x => x.UpdatedOn, DateTime.Now);

                //Process
                var result = kitsuneProjectsColletion.UpdateOne(filter, update);

                #endregion

                #region Create Object in KitsuneResources

                var project = new ProjectionDefinitionBuilder<KitsuneKrawlerStats>();
                var foundResources = krawlStatsCollection.Find(x => x.ProjectId == command.ProjectId)
                                               .Project<ProjectResources>(project.Include(x => x.Links)
                                                               .Include(x => x.Scripts)
                                                               .Include(x => x.Styles)
                                                               .Include(x => x.Assets)
                                                               .Exclude(x => x._id)).Limit(1).FirstOrDefault();

                ProjectConfigHelper configHelper = new ProjectConfigHelper(command.ProjectId);

                var regexToExclude = configHelper.GetSourceSyncExcludeRegex();
                var regexString = "^\\/kitsune\\-settings.json$";
                if (regexToExclude != null)
                    regexString = $"{regexString}|{regexToExclude}";

                var filterBuilder = new FilterDefinitionBuilder<KitsuneResource>();
                if (String.IsNullOrEmpty(command.ProjectId))
                    throw new Exception("ProjectId cannot be Empty");
                var projectIdFilter = filterBuilder.Eq(x => x.ProjectId, command.ProjectId);
                var regexFilter = filterBuilder.Regex(x => x.SourcePath, new BsonRegularExpression(regexString));
                var notRegexFilter = filterBuilder.Not(regexFilter);
                var ignoreFilesFilter = filterBuilder.And(projectIdFilter, notRegexFilter);

                var resultsa = kitsuneResourcesCollection.DeleteMany(ignoreFilesFilter);

                //kitsuneResourcesCollection.DeleteMany(x => x.ProjectId.Equals(command.ProjectId));

                if (foundResources != null)
                {
                    var kitsuneResources = new List<KitsuneResource>();
                    if (foundResources.Links != null)
                    {
                        try
                        {
                            kitsuneResources = Helpers.CreateKitsuneResource(foundResources.Links, command.ProjectId, ResourceType.LINK);
                            kitsuneResourcesCollection.InsertMany(kitsuneResources, new InsertManyOptions { IsOrdered = false });
                        }
                        catch (Exception ex)
                        {
                            //LOG
                        }
                    }
                    if (foundResources.Scripts != null)
                    {
                        try
                        {
                            kitsuneResources = Helpers.CreateKitsuneResource(foundResources.Scripts, command.ProjectId, ResourceType.SCRIPT);
                            kitsuneResourcesCollection.InsertMany(kitsuneResources, new InsertManyOptions { IsOrdered = false });
                        }
                        catch (Exception ex)
                        {
                            //LOG
                        }
                    }
                    if (foundResources.Styles != null)
                    {
                        try
                        {
                            kitsuneResources = Helpers.CreateKitsuneResource(foundResources.Styles, command.ProjectId, ResourceType.STYLE);
                            kitsuneResourcesCollection.InsertMany(kitsuneResources, new InsertManyOptions { IsOrdered = false });
                        }
                        catch (Exception ex)
                        {
                            //LOG
                        }
                    }
                    if (foundResources.Assets != null)
                    {
                        try
                        {
                            kitsuneResources = Helpers.CreateKitsuneResource(foundResources.Assets, command.ProjectId, ResourceType.FILE);
                            kitsuneResourcesCollection.InsertMany(kitsuneResources, new InsertManyOptions { IsOrdered = false });
                        }
                        catch (Exception ex)
                        {
                            //LOG
                        }
                    }
                }

                #endregion

                #region Push to build SQS

                var userEmail = kitsuneProjectsColletion.Find(x => x.ProjectId.Equals(command.ProjectId))
                                                      .Project(x => x.UserEmail).Limit(1).FirstOrDefault();

                //Call Build
                CreateOrUpdateKitsuneStatusRequestModel buildRequest = new CreateOrUpdateKitsuneStatusRequestModel() { ProjectId = command.ProjectId, UserEmail = userEmail };
                MongoConnector.CreateOrUpdateKitsuneStatus(buildRequest);

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static GetListOfDomainsResponseModel GetListOfDomainsFound(ListOfDomainsFoundRequestModel query)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var krawlStatsCollection = _kitsuneDatabase.GetCollection<KitsuneKrawlerStats>(KitsuneKrawlStatsCollection);

                ProjectionDefinitionBuilder<KitsuneKrawlerStats> project = new ProjectionDefinitionBuilder<KitsuneKrawlerStats>();

                var result = krawlStatsCollection.Find(x => x.ProjectId.Equals(query.ProjectId))
                                    .Project(x => x.DomainsFound).Limit(1).FirstOrDefault();
                return new GetListOfDomainsResponseModel { IsError = false, DomainList = result };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static bool SaveSelectedDomain(SaveSelectedDomainRequestModel command)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var krawlStatsCollection = _kitsuneDatabase.GetCollection<KitsuneKrawlerStats>(KitsuneKrawlStatsCollection);

                #region CHECK THE CRAWLING STATUS

                var result = krawlStatsCollection.Find(x => x.ProjectId.Equals(command.ProjectId) && !x.IsLocked && x.Stage.Equals(KitsuneKrawlerStatusCompletion.IdentifyingExternalDomains)).FirstOrDefault();
                if (result == null)
                    throw new Exception($"Unable to find crawling project with ProjectId:{command.ProjectId}");

                #endregion

                #region Update KrawlStats

                var update = new UpdateDefinitionBuilder<KitsuneKrawlerStats>();
                if (command.Domains != null)
                {
                    var updateResult = krawlStatsCollection.UpdateOne(x => x.ProjectId == command.ProjectId,
                    update.Set(x => x.SelectedDomains, command.Domains)
                          .Set(x => x.Stage, KitsuneKrawlerStatusCompletion.DownloadingAllStaticAssetsToStorage));
                }
                else
                {
                    var updateResult = krawlStatsCollection.UpdateOne(x => x.ProjectId == command.ProjectId,
                    update.Set(x => x.Stage, KitsuneKrawlerStatusCompletion.DownloadingAllStaticAssetsToStorage));
                }

                #endregion

                #region Push to SQS

                var sqsHandler = new AmazonSQSQueueHandlers<KrawlSQSModel>(AmazonAWSConstants.KrawlerSQSUrl);
                sqsHandler.PushMessageToQueue(new KrawlSQSModel { ProjectId = command.ProjectId }
                , EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey);

                #endregion

                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static AnalyseDetailsResponseModel GetAnalyseDetails(GetAnalyseDetailsRequestModel query)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                ProjectionDefinitionBuilder<KitsuneKrawlerStats> project = new ProjectionDefinitionBuilder<KitsuneKrawlerStats>();

                var krawlStatsCollection = _kitsuneDatabase.GetCollection<KitsuneKrawlerStats>(KitsuneKrawlStatsCollection);
                var result = krawlStatsCollection.Find(doc => doc.ProjectId == query.ProjectId)
                    .Project<AnalyseDetailsResponseModel>(project.Include(x => x.AssetsFound)
                                                          .Include(x => x.ScriptsFound)
                                                          .Include(x => x.LinksFound)
                                                          .Include(x => x.StylesFound)
                                                          .Include(x => x.Stage)
                                                          .Exclude(x => x._id)).Limit(1).FirstOrDefault();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static FilesDownloadDetailsResponseModel GetFilesDownloadDetails(FilesDownloadDetailsRequestModel query)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var krawlStatsCollection = _kitsuneDatabase.GetCollection<KitsuneKrawlerStats>(KitsuneKrawlStatsCollection);

                ProjectionDefinitionBuilder<KitsuneKrawlerStats> project = new ProjectionDefinitionBuilder<KitsuneKrawlerStats>();

                var result = krawlStatsCollection.Find(x => x.ProjectId.Equals(query.ProjectId))
                                               .Project<FilesDownloadDetailsResponseModel>(project.Include(x => x.StylesDownloaded)
                                                                                           .Include(x => x.ScriptsDownloaded)
                                                                                           .Include(x => x.AssetsDownloaded)
                                                                                           .Include(x => x.AssetsFound)
                                                                                           .Include(x => x.ScriptsFound)
                                                                                           .Include(x => x.StylesFound)
                                                                                           .Include(x => x.Stage)
                                                                                           .Exclude(x => x._id))
                                               .Limit(1).FirstOrDefault();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static GetNumberOfLinksReplacedResponseModel GetListOfLinksReplaced(GetNumberOfLinksReplacedRequestModel query)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var krawlStatsCollection = _kitsuneDatabase.GetCollection<KitsuneKrawlerStats>(KitsuneKrawlStatsCollection);

                ProjectionDefinitionBuilder<KitsuneKrawlerStats> project = new ProjectionDefinitionBuilder<KitsuneKrawlerStats>();

                var result = krawlStatsCollection.Find(x => x.ProjectId.Equals(query.ProjectId))
                                               .Project<GetNumberOfLinksReplacedResponseModel>(project.Include(x => x.LinksReplaced)
                                                                                               .Include(x => x.LinksFound)
                                                                                               .Include(x => x.Stage)
                                                                                               .Exclude(x => x._id))
                                               .Limit(1).FirstOrDefault();

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Payment and wallet

        public static bool IsWalletCritical(string Email)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var collection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
                var filter = Builders<UserModel>.Filter.Eq(x => x.Email, Email);
                var wallet = collection.Find(filter).First().Wallet;
                if (wallet == null)
                    throw new Exception("Couldn't fetch the balance");
                var netWalletBalance = Helpers.GetNetWalletBalance(wallet);
                if (netWalletBalance < 100)
                    return true;
                return false;

            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't fectch the balance");
            }

        }

        #endregion

        #region Event

        public static CreateEventResponse RegisterEvent(CreateEventRequest requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                string projectId = requestModel.ProjectId;
                switch (requestModel.Event)
                {
                    case EventTypeWebEngage.Optimization:
                        OptimisationCompleted(projectId);
                        break;
                    case EventTypeWebEngage.CrawlerAnalysed:
                        CrawlAnalyserPhaseCompleted(projectId);
                        break;
                    case EventTypeWebEngage.Transpilation:
                        TranspilationCompleted(projectId);
                        break;
                    case EventTypeWebEngage.TranspilationFailed:
                        TranspilationFailed(projectId);
                        break;
                    case EventTypeWebEngage.Publish:
                        break;
                    default:
                        break;
                }
                return new CreateEventResponse() { IsError = false };
            }
            catch (Exception ex)
            {
                //LOg ex.Message
                return new CreateEventResponse() { IsError = true, Message = "Error Sending the Report" };
            }
        }

        #endregion

        #region Optimization Reports

        internal static OptimizedPercentageResponseModel GetOptimizedPercentage(OptimizedPercentageRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var collection = _kitsuneDatabase.GetCollection<OptimizationReports>(KitsuneOptimizationReportsCollection);

                ProjectionDefinitionBuilder<OptimizationReports> project = new ProjectionDefinitionBuilder<OptimizationReports>();
                double? optimizedPercentage = null;
                optimizedPercentage = collection.Find(x => x.ProjectId == requestModel.ProjectId).SortBy(x => x.CreatedOn).Project(x => x.Total.Improvement).Limit(1).FirstOrDefault();

                if (optimizedPercentage == null)
                    return new OptimizedPercentageResponseModel
                    {
                        Message = "Calculating...",
                        NextTick = 500,
                        Success = true
                    };

                string message = "this website is {0}% faster now";
                string value = "10";
                if (optimizedPercentage < 10)
                {
                    var random = new Random((int)optimizedPercentage);
                    value = random.Next(10, 15).ToString();
                }
                else
                {
                    value = optimizedPercentage.ToString();
                }

                return new OptimizedPercentageResponseModel
                {
                    Message = String.Format(message, value),
                    NextTick = 0,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new OptimizedPercentageResponseModel
                {
                    Success = false,
                    NextTick = 500
                };
            }
        }

        #endregion
    }
}
