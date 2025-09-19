import { Component, Vue } from 'vue-property-decorator';
import { UserManager } from 'oidc-client';
import { disponseOidc, initOidc } from '@/$core/oidc-services';

@Component({
  name: 'signout-oidc',
})
export default class SignoutOidcComponent extends Vue {
  loading = false;
  userManager: UserManager;
  logOutSSO = '';
  async beforeMount() {
    console.log('signout-oidc');
		Misc.readFileAsync('static/variables/oidc-config.json').then(x => {
			this.logOutSSO = x.authority;
		});
	}

  async mounted() {
    this.loading = true;
    this.userManager = await initOidc();
    await this.userManager
      .signoutRedirectCallback()
      .then(async (us) => {
        self.localStorage.removeItem('access_token');
        await disponseOidc();
        console.log('Đăng xuất thành công');
      })
      .then(async () => {
        await setTimeout(() => {
          Object.keys(localStorage).forEach((key) => {
            if (key.startsWith('oidc.')) {
              localStorage.removeItem(key);
            }
          });
        }, 2000);
        await disponseOidc();
      }).finally(() => {
        window.location.assign(this.logOutSSO);
      });
    await this.userManager.revokeAccessToken();
  }
}
