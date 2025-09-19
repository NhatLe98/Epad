import { Component, Vue } from 'vue-property-decorator';
import { authApi } from '@/$api/auth-api';
import { UserManager } from 'oidc-client';
import { initOidc } from '@/$core/oidc-services';
import { Mutation } from 'vuex-class';
import axios from 'axios';
import { API_URL } from '@/$core/config';

@Component({
  name: 'login-callback',
})
export default class LoginCallbackComponent extends Vue {
  [x: string]: any;
  @Mutation('setToken', { namespace: 'Token' }) setToken;
  tabActive = {
    componentName: 'login-callback',
  };
  loading = false;
  userManager: UserManager;
  userName: string;
  isHuman: boolean = true;
  redirect = '';
  async created() {}

  async mounted() {
    this.userManager = await initOidc();
    this.loading = true;

    await this.userManager.signinRedirectCallback().then((user: any)=>{
      this.userName = user.profile.email;
    }).catch((error) => {
      console.error(error);
    });

    await this.check()
      .catch((error) => {
        console.log('Check login fail', error);
      });
  }

  async check() {
    if(this.userName){
      await authApi.CheckLogin(this.userName)
      .then((res:any) => {
        self.localStorage.removeItem('wrongCount');
        this.isHuman = true;
        this.loading = false;
        this.setToken(res.data.access_token);

        this.$router.push(this.redirect || '/dashboard').catch((err) => {}).finally(() => {
          if(res.data.message && res.data.message != ""){
            this.$alert(`${this.$t(res.data.message).toString()}`, this.$t('Warning').toString(), {
              dangerouslyUseHTMLString: true
            });
          }
        });
        this.notify(this.$t('Notify').toString(), this.$t('LoginSuccess').toString(), 's', 'tr');
      })
      .catch((error) => {
        if (error.response.status === 401) {
          var count = self.localStorage.getItem('wrongCount');
          if (count === null) {
            self.localStorage.setItem('wrongCount', '1');
          } else {
            self.localStorage.setItem('wrongCount', parseInt(count) + 1 + '');
          }
          if (parseInt(count) > 1 || count === '2') {
            this.isHuman = false;
          }
          this.loading = false;
          const message = error.response.data.message || 'UsernamePasswordInvalid';
          if (message === 'MSG_LicenseInvalid' || message === 'MSG_LicenseExpired') {
            // this.$alertRequestError(null, null, this.$t('Notify').toString(), this.$t(message).toString())
            this.$alert(this.$t(message).toString(), this.$t('Notify').toString(), {
              confirmButtonText: 'OK',
              callback: (action) => {
                this.$router.push('activate');
              },
            });
          } else {
            this.$alertRequestError(null, null, this.$t('Notify').toString(), this.$t('LoginFaild').toString());
          }
        }
      });
    }else{
      this.$alertRequestError(null, null, this.$t('Notify').toString(), this.$t('LoginFaild').toString());
    }
   
  }

  async logout() {
    await this.userManager
      .signoutRedirect()
      .then((e) => {
        console.log('logout', e);
      })
      .catch((error) => {
        console.error('logout error');
        console.error(error);
      });
  }
}
