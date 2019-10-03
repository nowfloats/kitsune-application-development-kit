// We only need to import the modules necessary for initial render
import React from 'react';
import Home from '../components/home/index';
import Preview from '../components/preview/page';
import PreviewProject from '../components/preview/project';
import PreviewComponent from '../components/preview/component';
import FourOhFour from '../components/404/index';
import { Route, IndexRoute } from 'react-router';
import App from '../components/app';
import Login from '../components/login/index';
import isUserLoggedIn from '../components/login/isLoggedIn';
import  MaintenancePage from '../components/maintenance/index';



export const createRoutes = store => (
	<Route path='/' component={App}>
		<Route path='maintenance' component={MaintenancePage} />
		<Route path='login' component={Login} />
		<Route component={isUserLoggedIn}>
			<IndexRoute component={Home} />
			<Route path='project' component={Home} />
			<Route path='project/:projectid' component={Home} />
			<Route path='preview/project=:projectid/page=:pagename/data=:dataset' component={Preview} />
			<Route path='preview/project=:projectid/customer=:customerid' component={PreviewProject} />
			<Route path='preview/project=:projectid/component=:componentid/path=:path/customer=:customerid'
				component={PreviewComponent} />
			<Route path='*' component={FourOhFour} />
		</Route>
	</Route>
	
);

export default createRoutes;
