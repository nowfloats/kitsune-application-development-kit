const menuItems = state => state.menu.items
const user = state => state.user
const useremail = state => state.app.UserEmail
const paymentDetails = state => state.app.payment
const componentStatus = state => state.app.componentIsActive
const windowSize = state => state.app.windowSize
const actionContainerCounter = state => state.app.actionContainerCounter
const areMandatoryAPIcallsCompleted = state => state.app.baseApiCalls.UserDetails
&& state.app.baseApiCalls.ProjectList
&& state.app.baseApiCalls.pollingProjectsInProcess

const getDeleteProjectDetails = state => state.app.deleteProject
const loader = state => state.app.loader;
const requestedForAna = state => state.app.requestedForAna;
const anaRegistrationDetails = state => state.app.anaRegistrationDetails;
const apiStatus = state => state.app.apiStatus;

export {
  menuItems,
	user,
	componentStatus,
	windowSize,
	actionContainerCounter,
	areMandatoryAPIcallsCompleted,
	useremail,
	paymentDetails,
	getDeleteProjectDetails,
	loader,
	requestedForAna,
	anaRegistrationDetails,
	apiStatus
}
