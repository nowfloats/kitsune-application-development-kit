const { NODE_ENV } = process.env;
const isProdEnv = NODE_ENV === 'production';

export const AUTH_CONFIG = {
	domain: 'kitsune.au.auth0.com',
	clientId: '[[AUTH_0_CLIENT_ID]]',
	callbackUrl: isProdEnv ? 'https://dashboard.kitsune.tools/login' : 'http://localhost:8080/login'
};
