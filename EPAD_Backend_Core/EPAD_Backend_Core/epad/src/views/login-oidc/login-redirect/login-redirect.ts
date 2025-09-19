import { Component, Vue } from 'vue-property-decorator';
import { UserManager } from 'oidc-client';
import { initOidc } from '@/$core/oidc-services';

@Component({
  name: 'login-redirect',
})
export default class LoginRedirectComponent extends Vue {
  loading = false;
  userManager: UserManager;
  async created() {}
  async mounted() {
    this.loading = true;
    this.userManager = await initOidc();

    console.log('login-redirect', this.userManager);

    await this.userManager
      .signinRedirect()
      .then((user) => {
        console.log('user');
      })
      .catch((error) => {
        console.error('Login failed');
      })
      .finally(() => {
        setTimeout(() => {
          this.loading = false;
        }, 2000);
      });
  }
}
