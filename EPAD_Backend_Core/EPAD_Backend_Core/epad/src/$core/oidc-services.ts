import {
  Log,
  UserManager,
  UserManagerSettings,
  WebStorageStateStore,
} from 'oidc-client';

if (Misc.isDev) {
  Log.logger = console;
}
const userStore = new WebStorageStateStore({ prefix: '' });

let userManager: UserManager;

async function initOidc(force = false) {
  if (force === true) {
    userManager = null;
  }
  if (Misc.isNullOrUndefined(userManager)) {
    const oUM = [];
    return await Misc.readFileAsync('static/variables/oidc-config.json')
      .then((data) => {
        const defaultConfig: UserManagerSettings = {
          userStore,
          redirect_uri: window.location.origin + '/login-callback',
          response_type: 'id_token token',
          scope: 'openid profile roles',
          post_logout_redirect_uri: window.location.origin + '/signout-oidc',

          automaticSilentRenew: true,
          loadUserInfo: true,
          revokeAccessTokenOnSignout: true,
        };
        Object.keys(localStorage).forEach((key) => {
          if (key.startsWith('oidc.')) {
            oUM.push(key);
          }
        });
        const config: UserManagerSettings = Object.assign(defaultConfig, data);
        return config;
      })
      .then((config) => {
        userManager = new UserManager(config);
        userManager.startSilentRenew();
        return userManager;
      })
      .finally(() => {
        if (oUM.length) {
          setTimeout(() => {
            oUM.forEach((key) => {
              localStorage.removeItem(key);
            });
          }, 10000);
        }
      });
  } else {
    return Promise.resolve(userManager);
  }
}

async function disponseOidc() {
  if (!Misc.isNullOrUndefined(userManager)) {
    userManager.stopSilentRenew();
    await userManager.clearStaleState();
    await userManager.removeUser();
    await userManager.revokeAccessToken();
    userManager = null;
  }
}

export { initOidc, disponseOidc };
