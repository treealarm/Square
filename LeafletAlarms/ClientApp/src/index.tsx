import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Provider, useDispatch } from 'react-redux';
import { ConnectedRouter } from 'connected-react-router';
import { createBrowserHistory } from 'history';
import configureMyStore from './store/configureStore';
import App from './App';
import registerServiceWorker from './registerServiceWorker';
import UserService from "./auth/UserService";

// Create browser history to use in the Redux store
const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href') as string;
const history = createBrowserHistory({ basename: baseUrl });

// Get the application-wide store instance, prepopulating with state from the server where available.
const store = configureMyStore(history);
export const useAppDispatch = () => useDispatch<typeof store.dispatch>()

const renderApp = () =>

ReactDOM.render(
    <Provider store={store}>
    <ConnectedRouter history={history}>
    <App />       
    </ConnectedRouter>
    </Provider>,
    document.getElementById('root'));

registerServiceWorker();

if (!UserService.isLoggedIn()) {
  UserService.initKeycloak(renderApp);
}

renderApp();


