//Reserved for future use
export const getListOfAllProjects = 'getListOfAllProjects';
export const getListOfAllLiveProjects = 'getListOfAllLiveProjects';
export const sortAllProjectsByCreatedOn = 'sortAllProjectsByCreatedOn';
export const getAllProjectsBySearchQuery = 'getAllProjectsBySearchQuery';
export const toggleStatus = 'toggleStatus';
export const getUserDetails = 'getUserDetails';
export const userDetailsReceived = 'userDetailsReceived';
export const projectListReceived = 'projectListReceived';
export const setDefaultUserEmail = 'setDefaultUserEmail';
export const updateAddMoneyComponent = 'updateAddMoneyComponent';
export const updateBillingFormComponent = 'updateBillingFormComponent';
export const updateProcessingPaymentComponent = 'updateProcessingPaymentComponent';
export const updateOverlayComponent = 'updateOverlayComponent';
export const updatePaymentReceived = 'updatePaymentReceived';
export const updatePaymentStatus = 'updatePaymentStatus';
export const updateInstamojoPaymentRequestLink = 'updateInstamojoPaymentRequestLink';
export const updateInstamojoPaymentRequestStatus = 'updateInstamojoPaymentRequestStatus';
export const userDetailsUpdateRequested = 'userDetailsUpdateRequested';
export const userDetailsUpdateRequestSuccess = 'userDetailsUpdateRequestSuccess';
export const isFetchingUsageData = "isFetchingUsageData";
export const isUserInfoUpdated = 'isUserInfoUpdated';
export const accountComponent = "accountComponent";
export const setLowBalanceDetails = "setLowBalanceDetails";
export const triggerLowBalanceModal = 'triggerLowBalanceModal';
export const isFetchingTransactionData = "isFetchingTransactionData";
export const setProjectForRenaming = 'setProjectForRenaming';
export const apiCallProgressStatus = 'apiCallProgressStatus';
export const setApiStatus = "setApiStatus";

export const toggleOnResize = 'toggleOnResize';
export const changeContainerCounter  = 'changeContainerCounter';
export const setContainerCounter = 'setContainerCounter';

export const toggleActionModal = 'toggleActionModal';
export const setDeleteProjectDetails = 'setDeleteProjectDetails';
export const setIsProjectPublishedInDeleteProject = 'setIsProjectPublishedInDeleteProject';

export const setDeleteProjectComponentStatus = 'setDeleteProjectComponentStatus';
export const setCardsSkeletonLoaderComponentStatus = 'setCardsSkeletonLoaderComponentStatus';
export const newProjectId = "newProjectId";

export const toggleLoader = 'toggleLoader';
export const setUploadDetails = 'setUploadDetails';
export const setUploadOnProjectDetails = 'setUploadOnProjectDetails';
export const setIsFolderDropped = 'setIsFolderDropped';
export const resetContainerHistory = 'resetContainerHistory';
export const addFailedFiles = 'addFailedFiles';
export const setUploadedFilesCount = 'setUploadedFilesCount';
export const toggleFullScreenLoader = 'toggleFullScreenLoader';
export const setCloudDetailsPresent = 'setCloudDetailsPresent';
export const resetCloudDetails = 'resetCloudDetails';
export const setDeactivateSiteDetails = 'setDeactivateSiteDetails';
export const setDomainForCloud = 'setDomainForCloud';

// Migration mutations

export const setProjectIdForMigration = 'setProjectIdForMigration';
export const setProjectNameForMigration = 'setProjectNameForMigration';
export const setIsCrawlingStartedForMigration = 'setIsCrawlingStartedForMigration';
export const setMigrationbuildcardsComponentStatus = 'setMigrationbuildcardsComponentStatus';
export const setStageForMigration = 'setStageForMigration';
export const setAnalysisInMigration = 'setAnalysisInMigration';
export const setDomainsFoundInMigration = 'setDomainsFoundInMigration';
export const setDownloadDetailsInMigration = 'setDownloadDetailsInMigration';
export const setReplacedLinksInMigration = 'setReplacedLinksInMigration';
export const setOptimizerAnalyzerInMigration = 'setOptimizerAnalyzerInMigration';
export const setOptimizeInMigration = 'setOptimizeInMigration';
export const setReplacerInMigration = 'setReplacerInMigration';
export const setCrawlerIsCompletedInMigration = 'setCrawlerIsCompletedInMigration';
export const setMigrationCompleted = 'setMigrationCompleted';
export const setIsNewStageReceivedForMigration = 'setIsNewStageReceivedForMigration';
export const setStageReceivedFromAPI = 'setStageReceivedFromAPI';
export const resetMigrationProcess = 'resetMigrationProcess';
export const setStopMigration = 'setStopMigration';
export const completeImportingFilesInMigrationProcess = 'completeImportingFilesInMigrationProcess';
export const completeOptimizingFilesInMigrationProcess = 'completeOptimizingFilesInMigrationProcess';
export const completeInsertingPlaceholdersInMigrationProcess = 'completeInsertingPlaceholdersInMigrationProcess';
export const completeGeneratingHTMLInMigrationProcess = 'completeGeneratingHTMLInMigrationProcess';
export const setIsErrorInStartingCrawlInMigrationProcess = 'setIsErrorInStartingCrawlInMigrationProcess';
export const setIsNetworkError = 'setIsNetworkError';
export const setIsErrorInGettingStageDetailsInMigrationProcess = 'setIsErrorInGettingStageDetailsInMigrationProcess'
export const setIsGettingDomainsInMigrationProcess = 'setIsGettingDomainsInMigrationProcess';
export const setProjectTypeInMigrationProcess = 'setProjectTypeInMigrationProcess';
export const setBuildVersionInMigrationProcess = 'setBuildVersionInMigrationProcess';
export const setIsFirstBuildInMigrationProcess = 'setIsFirstBuildInMigrationProcess';
export const incrementBuildCounter = 'incrementBuildCounter';
export const setSelectedDomains = 'setSelectedDomains';
export const setSelectedCloud = 'DataStoreInstance';
export const setAliCloudDetails = 'setAliCloudDetails';
export const setGCPDetails = 'setGCPDetails';
export const gcpTokenGeneratorUrl = 'gcpTokenGeneratorUrl';
export const setGCPCredsFile = 'setGCPCredsFile';
export const setChooseOwnAccount = 'setChooseOwnAccount';
export const setShowNotificationLiveSite = 'setShowNotificationLiveSite';

export const setEligibleForPublish = 'setEligibleForPublish';
export const setPublishProjectDetails = 'setPublishProjectDetails';
export const setCustomerListInProjectDetails = 'setCustomerListInProjectDetails';
export const setIsGettingListOfCustomersForPublishProject = 'setIsGettingListOfCustomersForPublishProject';
export const setIsCustomerCreationErrorForPublishProject = 'setIsCustomerCreationSuccessForPublishProject';
export const setIsPublishingForPublishProject = 'setIsPublishingForPublishProject';
export const setCustomerForPublishProject = 'setCustomerForPublishProject';
export const setStageForPublishProject = 'setStageForPublishProject';
export const resetStoreForPublishProject = 'resetStoreForPublishProject';
export const setTransactionDetails = 'setTransactionDetails';
export const usageDetails = 'usageDetails';
export const storageDetails = 'storageDetails';
export const invoiceDetails = 'invoiceDetails';
export const isFetchingInvoiceDetails = 'isFetchingInvoiceDetails';
export const setDomainNameForNewCustomerInPublishProject = 'setCustomerNameForNewCustomerInPublishProject';
export const setCustomerDetailsForNewCustomerInPublishProject = 'setCustomerDetailsForNewCustomerInPublishProject';
export const setIsNewCustomerInPublishProject = 'setIsNewCustomerInPublishProject';
export const setIsCreatingNewCustomerInPublishProject = 'setIsCreatingNewCustomerInPublishProject';
export const setIsAPIErrorInPublishProject = 'setIsAPIErrorInPublishProject';
export const currentProjectName = 'currentProjectName';
export const setProjectStage = 'setProjectStageAndStatus';
export const setProjectStatus = 'setProjectStatus';
export const setIsPollingForProjectsInProcess = 'setIsPollingForProjectsInProcess';
export const setPollingCompletedForBaseApiCalls = 'setPollingCompletedForBaseApiCalls';
export const doesWebsiteTagExist = 'doesWebsiteTagExist';
export const setPendingDomains = 'getPendingDomains';

// Download Project Mutations
export const setIsLinkRequestedInDownloadProject = 'setIsLinkRequestedInDownloadProject';
export const setDownloadLinkInDownloadProject = 'setDownloadLinkInDownloadProject';
export const setIsPollingInDownloadProject = 'setIsPollingInDownloadProject';
export const setIsApiCallErrorInDownloadProject = 'setIsApiCallErrorInDownloadProject';
export const setProjectIdInDownloadProject = 'setProjectIdInDownloadProject';


// optimization
export const setProjectIdAndNameInOptimizeProject = 'setProjectIdAndNameInOptimizeProject';
export const setIsApiCallCompletedInOptimizeProject = 'setIsApiCallCompletedInOptimizeProject';
export const setIsApiCallSuccessfullInOptimizeProject = 'setIsApiCallSuccessfullInOptimizeProject';
export const setOptimizeProjectInComponentStatus = 'setOptimizeProjectInComponentStatus';
export const setOptimizeErrorStatus = 'getOptimizeErrorStatus';

export const setUserIdInUserInformationForLogging = 'setUserIdInUserInformationForLogging';
export const setUserValidationCompletedInUserInformationForLogging =
	'setUserValidationCompletedInUserInformationForLogging';
export const setIsGettingUserIdInUserInformationForLogging = 'setIsGettingUserIdInUserInformationForLogging';
export const setIsUpdatingUserDetialsInformationForLogging = 'setIsUpdatingUserDetialsInformationForLogging';
export const setUserDataForUpdatingDatabase = 'setUserDataForUpdatingDatabase';

// CustomerDetails LiveSites
export const setCustomerInCustomerDetails = 'setCustomerInCustomerDetails';
export const resetCustomerDetails = 'resetCustomerDetails';
export const setCustomerIdInCustomerDetails = 'setCustomerIdInCustomerDetails';
export const setIsFetchingCustomerDetailsInCustomerDetails = 'setIsFetchingCustomerDetailsInCustomerDetails';
export const setAreDetailsFetchedSuccessfullyInCustomerDetails = 'setAreDetailsFetchedSuccessfullyInCustomerDetails';

// verify domain
export const setIsFetchingDetailsInVerifyDomain = 'setIsFetchingDetailsInVerifyDomain';
export const setcustomerIdInVerifyDomain = 'setcustomerIdInVerifyDomain';
export const setIsApiCallSuccessInVerifyDomain = 'setIsApiCallSuccessInVerifyDomain';
export const setdomainDetailsInVerifyDomain = 'setdomainDetailsInVerifyDomain';
export const resetVerifyDomain = 'resetVerifyDomain';

// dnsDetails
export const setWebsiteIdInDnsDetails = 'setWebsiteIdIdInDnsDetails';

export const setTitleAndMessageForToastr = 'setTitleAndMessageForToastr';

// projects
export const setProjectSearchQueryInProjects = 'setProjectSearchQueryInProjects';

// preview Project
export const setProjectIdInProjectPreview = 'setProjectIdForProjectPreview';
export const setCustomerListInProjectPreview = 'setCustomerListInProjectPreview';
export const setIsFetchingDetailsInProjectPreview = 'setIsFetchingDetailsInProjectPreview';
export const showPreviewModal = 'showPreviewModal';

//ana
export const requestedForAna = 'requestedForAna';
export const anaRegistrationDetails = 'anaRegistrationDetails';
export const setFlowList = 'setFlowList';
export const toggleShowArrorBots = 'toggleShowArrowBots';
export const isAnaAccountAlreadyPresent = 'isAccountAlreadyPresent';

//notifications
export const addUpdates = 'addUpdates';
export const updateBadgeNumber = 'updateBadgeNumber';

export const setLazyLoadDetails = 'setLazyLoadDetails';
