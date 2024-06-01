# Self-hosted music storage (frontend)

This template gets you up and running with a simple web app using [Fable](http://fable.io/) and [Feliz](https://github.com/Zaid-Ajaj/Feliz).

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) v8.0 or higher
* [node.js](https://nodejs.org) v18+ LTS

## Editor

Rider only

## Development

```sh
npm start
```

This will start the development server after compiling the project, once it is finished, navigate to http://localhost:8080 to view the application .

To build the application and make ready for production:

```sh
npm run build
```

This command builds the application and puts the generated files into the `deploy` directory (can be overwritten in webpack.config.js).

### Tests

The template includes a test project that ready to go which you can either run in the browser in watch mode or run in the console using node.js and mocha. To run the tests in watch mode:

```sh
npm run test:live
```

This command starts a development server for the test application and makes it available at http://localhost:8085.

To run the tests using the command line and of course in your CI server, you have to use the mocha test runner which doesn't use the browser but instead runs the code using node.js:

```sh
npm test
```