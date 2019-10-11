# Kitsune IDE

The cloud IDE from Kitsune, which contains all the tools you need in order to build serverless applications backed by [KitsuneServer](https://github.com/GetKitsune/application-development-kit/tree/master/KitsuneServer), and the [KAdmin](https://github.com/GetKitsune/application-development-kit/tree/master/KAdmin) CMS interface.

This project aims to be a standalone interface custom built to provide easy access to the essential tools, while maintaining the look, feel, and expectations of most common editor interfaces used by developers.

The latest release is deployed to the kit-cloud: [Live editor](https://ide.kitsune.tools)

# Contribution
This project aims to provide developers a single source for building, maintaining, and visualizing their *serverless* application workflows using the *Kitsune language extensions*.

**Thank you** for taking out the time to help build towards an efficient web, through a serverless future.

## Table of Contents
1. [Setup](#setup)
    - [Pre-Requisites](#pre-requisites)
    - [Frameworks used](#frameworks-used)
    - [Build source](#build-source)
    - [Additional scripts](#package-scripts)
1. [Architecture](#architecture)
    - [Folder Structure](#folder-structure)
    - [Routing](#routing)
    - [Testing](#testing)
    - [Deployment](#deployment)
    - [Developer Tools](#developer-tools)
1. [Build System](#build-system)
    - [Configuration](#configuration)
    - [Globals](#globals)
    - [Styles](#styles)
    - [Production Optimization](#production-optimization)


# Setup
### Pre-Requisites
* node `^4.5.0`: Use [nvm](https://github.com/nvm-sh/nvm) to install
* yarn `^0.17.0` or npm `^3.0.0`: Packaged with your installation of node

### Frameworks used
* [react](https://github.com/facebook/react): **v15** => *Functional components*, and *Hooks* **are not** supported.
* [redux](https://github.com/rackt/redux): Centralized state management
* [react-router](https://github.com/rackt/react-router): Superior client-side navigation
* [webpack](https://github.com/webpack/webpack) + [babel](https://github.com/babel/babel): Custom build configuration
* [express](https://github.com/expressjs/express): Development server
* [karma](https://github.com/karma-runner/karma): Test suite
* [eslint](http://eslint.org)

### Build Source
- Get the source:
  ```bash
  # Clone the toolkit
  $ git clone https://github.com/GetKitsune/application-development-kit.git

  # Use the IDE code as root
  $ cd IDE
  ```
- In the `IDE` folder, Install app dependencies.  
**Note**: It is recommended that you use [`yarn`](https://yarnpkg.com/) for deterministic installs, but `npm install` will work just as well.
  ```bash
  $ yarn install  # Install dependencies
  ```
- Start the development server:
  ```bash
  # If you're also running the server locally
  $ yarn start

  # If you want to use the production API endpoints
  $ yard dev:prodapi
  ```

### Package scripts
While the setup commands should serve most development purposes; There are additional scripts at your disposal:

|`yarn <script>`|Description|
|------------------|-----------|
|`start`|Serves your app at `localhost:3000`. HMR will be enabled in development.|
|`compile`|Compiles the application to disk (`~/dist` by default).|
|`dev`|Same as `yarn start`, but enables nodemon for the server as well.|
|`dev:prodapi`|Same as `yarn dev`, but uses the production API endpoints for the Kitsune server.|
|`test`|Runs unit tests with Karma and generates a coverage report.|
|`test:dev`|Runs Karma and watches for changes to re-run tests; does not generate coverage reports.|
|`deploy`|Runs linter, tests, and then, on success, compiles your application to disk.|
|`deploy:dev`|Same as `deploy` but overrides `NODE_ENV` to "development".|
|`deploy:prod`|Same as `deploy` but overrides `NODE_ENV` to "production".|
|`eslint`|Lint all `.js` files.|
|`eslint:fix`|Lint and fix all `.js` files. [Read more on this](http://eslint.org/docs/user-guide/command-line-interface.html#fix).|
|`stylelint`|Lint all stylesheets.|

# Architecture
## Folder Structure

The application structure presented in this repository is **fractal**, where functionality is grouped primarily by feature rather than file type. Please note, however, that this structure is only meant to serve as a guide, it is by no means prescriptive. That said, it aims to represent generally accepted guidelines and patterns for building scalable applications.

```
.
├── bin                      # Build/Start scripts
├── config                   # Project and build configurations
├── public                   # Static public assets (not imported anywhere in source code)
├── server                   # Express application that provides webpack middleware
│   └── main.js              # Server application entry point
├── src                      # Application source code
│   ├── index.html           # Main HTML page container for app
│   ├── main.js              # Application bootstrap and rendering
│   ├── components           # Global Reusable Presentational Components
│   ├── containers           # Global Reusable Container Components
│   ├── layouts              # Components that dictate major page structure
│   │   └── CoreLayout.js    # CoreLayout which receives children for each route
│   │   └── CoreLayout.scss  # Styles related to the CoreLayout
│   │   └── index.js         # Main file for layout
│   ├── routes               # Main route definitions and async split points
│   │   ├── index.js         # Bootstrap main application routes with store
│   │   ├── Home             # Fractal route
│   │   │   ├── index.js     # Route definitions and async split points
│   │   │   ├── assets       # Assets required to render components
│   │   │   ├── components   # Presentational React Components
│   │   │   └── routes **    # Fractal sub-routes (** optional)
│   │   └── Counter          # Fractal route
│   │       ├── index.js     # Counter route definition
│   │       ├── container    # Connect components to actions and store
│   │       ├── modules      # Collections of reducers/constants/actions
│   │       └── routes **    # Fractal sub-routes (** optional)
│   ├── store                # Redux-specific pieces
│   │   ├── createStore.js   # Create and instrument redux store
│   │   └── index.js         # Reducer registry and injection
│   └── styles               # Application-wide styles (generally settings)
└── tests                    # Unit tests
```

## Routing
We use `react-router` [route definitions](https://github.com/ReactTraining/react-router/blob/v3/docs/API.md#plainroute) (`<route>/index.js`) to define units of logic within our application. See the [application structure](#application-structure) section for more information.

## Testing (Coming Soon)
To add a unit test, simply create a `.spec.js` file anywhere in `~/tests`. Karma will pick up on these files automatically, and Mocha and Chai will be available within your test without the need to import them. Coverage reports will be compiled to `~/coverage` by default. If you wish to change what reporters are used and where reports are compiled, you can do so by modifying `coverage_reporters` in `~/config/project.config.js`.

## Deployment
This repository is deployable by serving the `~/dist` folder generated by `npm run deploy` (make sure to specify your target `NODE_ENV` as well).

## Developer Tools
**We recommend using the [Redux DevTools Chrome Extension](https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd).**
Using the chrome extension allows your monitors to run on a separate thread and affords better performance and functionality. It comes with several of the most popular monitors, is easy to configure, filters actions, and doesn’t require installing any packages.

However, adding the DevTools components to your project is simple. First, grab the packages from npm:

```bash
npm i --save-dev redux-devtools redux-devtools-log-monitor redux-devtools-dock-monitor
```

Then follow the [manual integration walkthrough](https://github.com/gaearon/redux-devtools/blob/master/docs/Walkthrough.md).

# Build System

## Configuration

Default project configuration can be found in `~/config/project.config.js`. Here you'll be able to redefine your `src` and `dist` directories, adjust compilation settings, tweak your vendor dependencies, and more. For the most part, you should be able to make changes in here **without ever having to touch the actual webpack build configuration**.

If you need environment-specific overrides (useful for dynamically setting API endpoints, for example), you can edit `~/config/environments.config.js` and define overrides on a per-NODE_ENV basis. There are examples for both `development` and `production`, so use those as guidelines. Here are some common configuration options:

|Key|Description|
|---|-----------|
|`dir_src`|application source code base path|
|`dir_dist`|path to build compiled application to|
|`server_host`|hostname for the Express server|
|`server_port`|port for the Express server|
|`compiler_devtool`|what type of source-maps to generate (set to `false`/`null` to disable)|
|`compiler_vendor`|packages to separate into to the vendor bundle|

Webpack is configured to make use of [resolve.root](http://webpack.github.io/docs/configuration.html#resolve-root), which lets you import local packages as if you were traversing from the root of your `~/src` directory. Here's an example:

```js
// current file: ~/src/views/some/nested/View.js
// What used to be this:
import SomeComponent from '../../../components/SomeComponent'

// Can now be this:
import SomeComponent from 'components/SomeComponent' // Hooray!
```

## Globals

These are global variables available to you anywhere in your source code. If you wish to modify them, they can be found as the `globals` key in `~/config/project.config.js`. When adding new globals, make sure you also add them to `~/.eslintrc`.

|Variable|Description|
|---|---|
|`process.env.NODE_ENV`|the active `NODE_ENV` when the build started|
|`__DEV__`|True when `process.env.NODE_ENV` is `development`|
|`__PROD__`|True when `process.env.NODE_ENV` is `production`|
|`__TEST__`|True when `process.env.NODE_ENV` is `test`|

## Styles

Both `.scss` and `.css` file extensions are supported out of the box. After being imported, styles will be processed with [PostCSS](https://github.com/postcss/postcss) for minification and autoprefixing, and will be extracted to a `.css` file during production builds.

## Production Optimization

Babel is configured to use [babel-plugin-transform-runtime](https://www.npmjs.com/package/babel-plugin-transform-runtime) so transforms aren't inlined. In production, webpack will extract styles to a `.css` file, minify your JavaScript, and perform additional optimizations such as module deduplication.
