import Keycloak, { KeycloakError } from "keycloak-js";

let _kcInstance: Keycloak | null = null;

function getKeycloakInstance(): Keycloak {
  if (!_kcInstance) {
    _kcInstance = new Keycloak('keycloak.json');
  }
  return _kcInstance;
}


class FunctionHolder {
  onUserChangedCallback?(): void;
}

const _fh = new FunctionHolder();

const CleanToken = () => {
  localStorage.setItem('kc_token', '');
  localStorage.setItem('kc_refreshToken', '');

  if (getKeycloakInstance() != null) {
    getKeycloakInstance().token = null;
  }


  if (_fh.onUserChangedCallback != null) {
    console.log('onUserChangedCallback');
    _fh.onUserChangedCallback();
  }
}

/**
 * Initializes Keycloak instance and calls the provided callback function if successfully authenticated.
 *
 * @param onAuthenticatedCallback
 */

const initKeycloak = (
  onAuthenticatedCallback: any,
  onUserChangedCallback: any) => {

  _fh.onUserChangedCallback = onUserChangedCallback;

  var kc_token = localStorage.getItem('kc_token');
  var refreshToken = localStorage.getItem('kc_refreshToken');

  console.log("KC INIT:");

  getKeycloakInstance().onTokenExpired = () => {
    console.log('expired ' + new Date());
    getKeycloakInstance().updateToken(60).then((refreshed:any) => {
      if (refreshed) {
        console.log('refreshed ' + new Date());
      } else {
        console.error('not refreshed ' + new Date());
        CleanToken();
      }
    }).catch((e: any) => {
      console.error('Failed to refresh token ', new Date(), e.message);
      CleanToken();
    });
  }

  getKeycloakInstance().onAuthError = (errorData: KeycloakError) => {
    console.log('ON_AUTH_ERROR', errorData);
  }

  getKeycloakInstance().onAuthLogout = () => {
    console.log('ON_AUTH_LOGOUT');
  }

  getKeycloakInstance().init({
    onLoad: 'check-sso',
    silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
    pkceMethod: 'S256',
    token: kc_token??undefined,
    refreshToken: refreshToken??undefined
  })
    .then((authenticated:any) => {

      if (!authenticated) {
        console.log("user is not authenticated..!");
        CleanToken();
      }
      else {
        localStorage.setItem('kc_token', getKeycloakInstance().token);
        localStorage.setItem('kc_refreshToken', getKeycloakInstance().refreshToken??'');
        console.log('roles', getKeycloakInstance().realmAccess.roles??'');
        console.log('KC SEEMS TO BE AUTHENTIFICATED', getKeycloakInstance().authenticated);

        if (getKeycloakInstance().isTokenExpired(60)) {
          console.error("KC ERROR: EXPIRED");
          getKeycloakInstance().onTokenExpired();
        }
      }      

      onAuthenticatedCallback();
    })
    .catch((e: any) =>
    {      
      CleanToken();
      console.log("KC ERROR:", e);
      _kcInstance = null;
    });
};

const doLogin = () => {
  getKeycloakInstance().login();
}

const doLogout = () =>
{  
  getKeycloakInstance().logout();
  CleanToken();
}

const getToken = () => getKeycloakInstance()?.token;

const isLoggedIn = () => !!getKeycloakInstance()?.token;

const updateToken = (successCallback: any) =>
  getKeycloakInstance().updateToken(60)
    .then(successCallback)
    .catch(doLogin);

const getUsername = () => getKeycloakInstance().tokenParsed?.preferred_username;

const hasRole = (roles: any) => roles.some((role: any) => getKeycloakInstance().hasRealmRole(role));

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
