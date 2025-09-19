import { Component, Vue, Ref } from "vue-property-decorator";
import { ElLoadingComponent } from 'element-ui/types/loading';
import { licenseApi } from '@/$api/license-api';

@Component({
  name: "activate-license",
  components: {  }
})

export default class ActivateLicense extends Vue {
    @Ref('ruleForm') inputForm: any;
    dataModel = {
      LicenseKey: '',
      LicenseData: '',
      IsOffline: false,
    };
    loading: ElLoadingComponent = null;
    dataRules = {};
    fileList = [];
    ComputerIdentify = '1111111111';
  
    async beforeMount() {
      this.translate();
      this.dataModel = Object.assign(
        {},
        { LicenseKey: '', LicenseData: '', IsOffline: false },
        {}
      );
      licenseApi.useCustomErrorHandler(true);
    }
  
    translate() {
      this.dataRules = {
        LicenseKey: [
          {
            required: true,
            message: this.$t('RequiredField'),
            trigger: 'blur',
          },
        ],
        LicenseData: [
          {
            required: true,
            message: this.$t('RequiredField'),
            trigger: 'blur',
          },
        ],
      };
    }
  
    async confirmLicense() {
      await this.inputForm.validate((valid) => {
        if (valid) {
          this.sendRequest();
        }
      });
    }
  
    async sendRequest() {
      this.loading = this.$loading({
        lock: true,
      });
      const { LicenseKey, LicenseData, IsOffline } = this.dataModel;
      await licenseApi.ActivateLicense({ LicenseKey, LicenseData, IsOffline })
      .then(res => {
        this.$router.push('/login');
      })
      .catch(err => {
        const errMessage = err.response.data as string;
        if(errMessage === 'NoConnection'){
          this.$confirm(
            this.$t('NoConnectionActiveOffline').toString(),
            this.$t('NoConnection').toString()
          ).then(async () => {
            Object.assign(this.dataModel, { IsOffline: true }, {});
            await this.getID2();
          });
        }
        else {
          this.$alert(this.$t(errMessage).toString());
        }
      })
      .finally(() => {
        this.loading.close();
      });
    }

    async getID2(){
        await licenseApi.GetID2().then((id2: any) => this.ComputerIdentify = id2.data).catch(() => {});
    }
  
    uploadLicenseFile(file, fileList) {
      console.log('File', file);
      const reader = new FileReader();
      reader.readAsText(file.raw, 'UTF-8');
      reader.onload = (evt) => {
        const LicenseData = evt.target.result;
        Object.assign(this.dataModel, { LicenseData }, {});
      };
    }
  
    async copyComputerIdentify() {
      (this.$refs.inputID2 as any).select();
      document.execCommand('copy');
    }
}
