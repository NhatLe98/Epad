import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { userAccountApi, IPasswordInfo } from "@/$api/user-account-api";

@Component({
  name: "header-component",
  components: {}
})
export default class HeaderComponent extends Mixins(ComponentBase) {
  user: IPasswordInfo = {
    UserName: "",
    Password: "",
    NewPassword: "",
    ConfirmPassword: "",
    Email: "",
    ServiceId: 0,
  };
  errName = false;
  dialogVisible = false;
  localeOptions = [
    {
      value: "vi",
      label: "Vietnamese"
    },
    {
      value: "en",
      label: "English"
    }
  ];
  value = "";

  btnCancel() {
    this.dialogVisible = false;
  }
  btnOK() {
    if (this.user.NewPassword.includes(this.user.ConfirmPassword) == true) {
      userAccountApi
        .ChangePassword(this.user)
        .then((res: any) => {
          this.notify(
            this.$t("Notify").toString(),
            this.$t("ChangePasswordSuccess").toString(),
            "s",
            "tr"
          );
          this.dialogVisible = false;
          this.errName = false;
        })
        .catch(error => {
        });
    } else {
      this.errName = true;
    }
  }

  beforeMount() {
    var lang = this.$i18n.locale;
    this.value = lang;
    this.LoadUserInFo();
  }
  
  LoadUserInFo() {
    userAccountApi.GetUserAccountInfo().then((res: any) => {
      this.user.UserName = res.data.Name;
    });
  }

  changeLocales() {
    var lang = this.$i18n.locale;
    if (lang != this.value) {
      this.$i18n.locale = this.value;
    }
  }

  handleCommand(command) {
    if (command == "Logout") {
      this.$router.push({ name: "login" });
    }
    else if (command == "ChangePassword") {
    }
  }

  handleClose() {}
}
