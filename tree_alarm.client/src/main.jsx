import './custom.css';
import { createRoot } from 'react-dom/client';


import { Provider } from 'react-redux';
import { theStore } from './store/configureStore.ts';

import App from './App.jsx';

const rootElement = document.getElementById('root');
const root = createRoot(rootElement);

root.render(
  <Provider store={theStore}>
    <App />
  </Provider>
);

