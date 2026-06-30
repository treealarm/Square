import './custom.css';
import { createRoot } from 'react-dom/client';

import { CssBaseline, ThemeProvider } from '@mui/material';
import { Provider } from 'react-redux';
import { theStore } from './store/configureStore.ts';

import App from './App.jsx';
import { netTerrainTheme } from './theme/netTerrainTheme';

const rootElement = document.getElementById('root');
const root = createRoot(rootElement);

root.render(
  <Provider store={theStore}>
    <ThemeProvider theme={netTerrainTheme}>
      <CssBaseline />
      <App />
    </ThemeProvider>
  </Provider>
);

