import Keycloak from "keycloak-js";

const _kc = new Keycloak('keycloak.json');

/**
 * Initializes Keycloak instance and calls the provided callback function if successfully authenticated.
 *
 * @param onAuthenticatedCallback
 */

const initKeycloak = (onAuthenticatedCallback: any) => {

  const token = localStorage.getItem('kc_token');
  const refreshToken = localStorage.getItem('kc_refreshToken');

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
        localStorage.setItem('kc_token', '');
        localStorage.setItem('kc_refreshToken', '');
      }
      else {
        localStorage.setItem('kc_token', _kc.token);
        localStorage.setItem('kc_refreshToken', _kc.refreshToken);
        console.log('roles', _kc.realmAccess.roles);        
      }

      onAuthenticatedCallback();
    })
    .catch(console.error);
};

const doLogin = () => {
  _kc.login();
}

const doLogout = () =>
{
  localStorage.setItem('kc_token', '');
  localStorage.setItem('kc_refreshToken', '');
  _kc.logout();
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
