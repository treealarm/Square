import Keycloak from "keycloak-js";

const _kc = new Keycloak('keycloak.json');

/**
 * Initializes Keycloak instance and calls the provided callback function if successfully authenticated.
 *
 * @param onAuthenticatedCallback
 */

const CleanToken = () => {
  localStorage.setItem('kc_token', '');
  localStorage.setItem('kc_refreshToken', '');

  if (_kc != null) {
    _kc.token = null;
  }  
}

const initKeycloak = (onAuthenticatedCallback: any) => {

  var token = localStorage.getItem('kc_token');
  var refreshToken = localStorage.getItem('kc_refreshToken');

  console.log("KC INIT:");

  _kc.onTokenExpired = () => {
    console.log('expired ' + new Date());
    _kc.updateToken(0).then((refreshed) => {
      if (refreshed) {
        console.log('refreshed ' + new Date());
      } else {
        console.log('not refreshed ' + new Date());
        CleanToken();
      }
    }).catch(() => {
      console.error('Failed to refresh token ' + new Date());
      CleanToken();
    });
  }

  _kc.onAuthError = () => {
    console.log('ON_AUTH_ERROR');
  }

  _kc.onAuthLogout = () => {
    console.log('ON_AUTH_LOGOUT');
  }

  _kc.init({
    onLoad: 'check-sso',
    silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
    pkceMethod: 'S256',
    token: token,
    refreshToken: refreshToken
  })
    .then((authenticated) => {

      if (!authenticated) {
        console.log("user is not authenticated..!");
        CleanToken();
      }
      else {
        localStorage.setItem('kc_token', _kc.token);
        localStorage.setItem('kc_refreshToken', _kc.refreshToken);
        console.log('roles', _kc.realmAccess.roles);
        console.log('KC SEEMS TO BE AUTHENTIFICATED', _kc.authenticated);

        if (_kc.isTokenExpired(10)) {
          console.error("KC ERROR: EXPIRED");
          _kc.onTokenExpired();
        }
      }      

      onAuthenticatedCallback();
    })
    .catch((e: any) =>
    {      
      CleanToken();
      console.log("KC ERROR:", e);
    });
};

const doLogin = () => {
  _kc.login();
}

const doLogout = () =>
{  
  _kc.logout();
  CleanToken();
}

const getToken = () => _kc.token;

const isLoggedIn = () => !!_kc.token;

const updateToken = (successCallback: any) =>
  _kc.updateToken(5)
    .then(successCallback)
    .catch(doLogin);

const getUsername = () => _kc.tokenParsed?.preferred_username;

const hasRole = (roles: any) => roles.some((role: any) => _kc.hasRealmRole(role));

const UserService = {
  initKeycloak,
  doLogin,
  doLogout,
  isLoggedIn,
  getToken,
  updateToken,
  getUsername,
  hasRole
};

export default UserService;
