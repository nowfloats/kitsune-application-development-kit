//Flag to check if we are on production environment
export const isProdEnv = process.env.NODE_ENV === 'production';

//TODO figure out a way to get base url dynamically
//export const apiBaseUrl = isProdEnv ? "https://api2.kitsune.tools" : "https://api2.kitsunedev.com";
export const apiBaseUrl = "https://api2.kitsune.tools";
export const anaBaseUrl = isProdEnv ? 'https://gateway.api.ana.chat' :
	'https://chat-dev.nowfloatsdev.com';
// TODO replace s3 link with cdn link
export const apiIdeUrl = apiBaseUrl;
export const defaultImageLinkForCards = "https://s3.ap-south-1.amazonaws.com/kitsune-buildtest-resources/kitsuneassets/ZerothBg.png";
export const projectCardDateFormat =" {mon} {date}, {year}"

export const cookieDomain = isProdEnv ? '.kitsune.tools' : '.kitsunedev.com';

// API CALLS PATH
export const getAllProjects = "/api/Project/v2/Projects?userEmail={userEmail}&skip={skip}&limit={limit}";
export const optimizeProject = "/api/Project/v1/Build?user=";
export const archiveProject = "/api/Conversion/v1/ArchiveProject";
export const projectPublishedOrNot = "/api/Conversion/v1/ProjectPublishedOrNot?crawlId=";
export const getUserDetails = "/api/Developer/v1/UserProfile?useremail=";
export const updateUserDetails = "/api/Developer/v1/UpdateuserDetails";
export const getPaymentRedirectionLink = "/api/Payment/v1/CreatePaymentRequest";
export const processInternationalPaymentRequest = "/api/Payment/v1/CreateInternationalPaymentRequest";
export const getDownloadProjectLink = "/api/Project/v1/download?projectId={projectId}&userEmail={userEmail}";
export const getDownloadProjectLinkStatus = "/api/Conversion/v1/GetProjectDownloadstatus?CrawlId={projectId}";
export const uploadFolder = "/api/ide/";
export const customerListForProject = "/api/website/v1?projectId={projectId}&skip={skip}&limit={limit}";
export const publishProject = "/api/Project/v1/Publish?customerId={customerId}&userEmail={userEmail}";
export const createProject  = "/api/Project/v1/Project";
export const debitDetails = "/api/WebAnalytics/v1/GetAllRequestsPerDayByUserId?developerId=";
export const storageDetails = "/api/WebAnalytics/v1/GetStoragePerDayByUserId?developerId=";
export const invoiceDetails = "/api/Developer/v1/Invoice?userEmail={userEmail}&month={month}&year={year}";
export const paymentDetails = "/api/Developer/v1/PaymentDetails?userEmail=";
export const getAllLivesProject = "/api/website/v1/live?limit={limit}&skip={skip}";
export const createNewCustomer = "/api/website/v1";
export const getUserId = '/api/Developer/v1/GetUserId?useremail={userEmail}';
export const createDeveloperAccount = '/api/Developer/v1/CreateUser';
export const projectsInProcess = "/api/Project/v1/ProjectsInProcess?userEmail={userEmail}";
export const getCustomerDetails = "/api/website/v1/{websiteId}";
export const checkAndMapDomain = "/api/domainmapper/v1/checkandmapdomain?customerId={customerId}";
export const checkIfWebsiteTagExists = "/api/website/v1/WebsiteTagExists/";
export const publishToCloud = '/api/Project/v1/CreateUpdateCloudProvider?projectId=';
export const getCloudProviderDetails = '/api/Project/v1/GetCloudProviderDetails?projectId=';
export const getPendingDomains = "/api/domainMapper/v1/requesteddomains?websiteId=";
export const updateDomain = "/api/domainMapper/v1/updatedomain?customerId={customerId}&newDomain={domain}";
export const getLowBalanceDetails = "/api/WebAnalytics/v1/GetLowWalletBalanceStatus?developerId=";
export const checkApiStatus = isProdEnv ? "https://status.api.kitsune.tools/kitsuneapi"
	: "https://n49f71a7w8.execute-api.ap-south-1.amazonaws.com/v1/kitsuneapi";
export const deactivateSite = '/api/Website/v1/DeactivateWebsites';

// PREVIEW LINK FOR PROJECTS
export const projectPreviewLink = "http://kitsune-resource-demo.s3-accelerate.amazonaws.com/{projectid}/cwd/index.html";

export const IDE = isProdEnv ? "https://ide.kitsune.tools" : "https://ide.kitsunedev.com";
export const DASHBOARD = isProdEnv ? "https://dashboard.kitsune.tools" : "https://dashboard.kitsunedev.com";

export const IDELink = `${IDE}/project/{projectid}`;

export const googlesecretKey = "[[GOOGLE_KEY]].apps.googleusercontent.com";

export const IDEHttp = isProdEnv ? "http://ide.kitsune.tools" : "http://ide.kitsunedev.com";
export const projectIDEPreviewLink = `${IDEHttp}/preview/project={projectId}/customer=default`;

//Maps
const monthNoOfDays = [
	[0, 31],
	[1, 28],
	[2, 31],
	[3, 30],
	[4, 31],
	[5, 30],
	[6, 31],
	[7, 31],
	[8, 30],
	[9, 31],
	[10, 30],
	[11, 31],
	[12, 29],
];
export const daysInMonths = new Map(monthNoOfDays);

//Regex
export const regex = {
	projectName: /[^A-z\d_\s-]/,
	phoneNumber: /[^0-9]$/,
};

export const instamojoPaymentRedirectUrl = `${DASHBOARD}/payment`;

export const instamojopaymentStatus = '/api/Payment/v1/GetPaymentStatus'

// Regexs
export const nameRegex = /(^([a-zA-Z .]+)$)/;
export const phoneNumberRegex = /(^([\d]{10})$)/;
export const emailRegex = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
export const addressRegex = /(^([A-Za-z0-9,. ]+)$)/m;
export const cityRegex = /(^([A-Za-z ]+)$)/;
export const pinRegex = /(^([0-9]{6})$)/;
export const domainNameRegex = /^(?:(?:(?:[a-zA-z\-]+)\:\/{1,3})?(?:[a-zA-Z0-9])(?:[a-zA-Z0-9-\.]){1,61}[a-zA-Z0-9](?:\.[a-zA-Z]{2,})+|\[(?:(?:(?:[a-fA-F0-9]){1,4})(?::(?:[a-fA-F0-9]){1,4}){7}|::1|::)\]|(?:(?:[0-9]{1,3})(?:\.[0-9]{1,3}){3}))(?:\:[0-9]{1,5})?$/;
// export const domainNameRegex = /^([a-z0-9]+\.)?[a-z0-9][a-z0-9-]*\.[a-z]{2,6}$/i;
export const siteNameRegex = /^[a-zA-Z0-9]*$/i;
export const subDomainRegex = /[!@#$%^&*()_+\=\[\]{};':"\\|,<>\/?]/;

// Migration And Optimization
export const startCrawl = '/api/krawler/v2/startkrawl';
export const getAnalysisDetails = '/api/krawler/v1/GetAnalyseDetails?projectId={projectId}&userEmail={userEmail}'
export const getDomainsFoundOnSite = '/api/krawler/v1/GetListOfDomainsFound?projectId={projectId}&userEmail={userEmail}'
export const saveSelectedDomains = '/api/krawler/v1/SaveSelectedDomain';
export const getDownloadDetails = '/api/krawler/v1/GetFilesDownloadDetails?projectId={projectId}&userEmail={userEmail}'
export const getReplacedLinks = '/api/krawler/v1/GetNumberOfLinksReplacedQuery?projectId={projectId}';
export const getBuildStats = '/api/project/v1/build?projectid={projectId}&user={userEmail}';
export const updateCrawlComplete = '/api/krawler/v1/updatekrawlcomplete?projectId={projectId}';
export const abortCrawl = '/api/krawler/v1/stopcrawl';

//Ana integration
export const anaRegistration = '/business/accounts/register';
export const getAnaBots = '/business/flows?businessId={businessId}&size=10';
export const getAccountDetails = '/business/accounts/';
export const anaMinBalance = 20000;
export const defaultErrorMessage = 'unknown forces have stopped our noble work.';

//Stripe Constants - pk_test_iQTrlQ5OXVCaG075HAKdeoA2
export const stripePublicKey = isProdEnv ? "[[STRIPE_LIVE_API_KEY]]" : "[[STRIPE_DEV_API_KEY]]";

//emailer
export const emailer = '/api/Internal/v1/SendEmail?configType=1';
export const anaEmailSubject = 'Ana-cloud Subscription Details';
export const coreEmailId = "core@getkitsune.com";

//kadmin
export const kAdminUrl = `/api/Website/v1/KdaminLogin?source=data_console_mode&websiteId=`;
export const gcpTokenGenerator = 'https://guk1lbz8ne.execute-api.ap-south-1.amazonaws.com/v1/auth/oauthurl';

export const actionStates = {
	ACTION_CRAWL: 'ACTION_CRAWL-1',
	ACTION_CREATE: 'ACTION_CREATE-2',
	ACTION_UPLOAD: 'ACTION_UPLOAD-3'
};

//name of the modal components in componentIsActive variable in store
export const overlayModalNames = [
	'overlay',
	'updates',
	'modal',
	'profile',
	'sidebar',
	'migrationbuildcards',
	'profile',
	'sidebar',
	'action',
	'billingform',
	'addmoney',
	'processingpayment',
	'deleteproject',
	'optimizeProject',
	'customerDetails',
	'verifyDomain',
	'dnsDetails',
	'previewProject',
	'credits',
	'lowBalance',
	'renameProject',
	'deactivateSite'
];

//notification callbacks
export const notificationCallback = {
	addMoney: 'ADD MONEY'
};

//error messages
export const errorMessages = {
	ABORT_CRAWL: 'could not abort crawl. please try again.'
};

const cloudOptions = [
	[0, 'aws'],
	[1, 'alicloud'],
	[2, 'gcp'],
	[3, 'azure']
];
export const cloudOptionsMap = new Map(cloudOptions);
