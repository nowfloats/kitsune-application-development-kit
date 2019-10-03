const { NODE_ENV } = process.env;
const isProdEnv = NODE_ENV === 'production';

export const AUTH_CONFIG = {
	domain: 'kitsune.au.auth0.com',
	clientId: 'Uulp9GjKJHub6XTzLv0UiHktKnyJxGJ8',
	callbackUrl: isProdEnv ? 'https://ide.kitsune.tools/login' : 'http://localhost:3000/login'
};
