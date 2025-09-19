import { Component, Vue, PropSync, Prop, Watch } from "vue-property-decorator";
import { configApi, IConfig } from '@/$api/config-api';
import { isNullOrUndefined } from 'util';
@Component({
  name: "config",
  methods: { isNullOrUndefined }
})
export default class SystemConfigComponent extends Vue {
  @PropSync('configModel') config: IConfig;
  @Prop({default: () => []}) timePosOption: Array<string>;
  @Prop({ default: () => [] }) emailOption: Array<string>;
  @Prop({ default: () => [] }) serialNumberOption: Array<string>;
  @Prop({ default: () => [] }) groupDeviceOption: Array<any>;
  @Prop({ default: () => [] }) departmentOption: Array<any>;
  @Prop({ default: () => false }) isHideMachineSerialSelect: Array<any>;
  LinkAPIIntegrate = '';
  isUsingECMS : boolean = false;
  listRemoveStoppedWorkingEmployeesType = [];
  listShowStoppedWorkingEmployeesType = [];
  listSoftware = [];
  listDayInWeek = [];
  listDayInMonth = [];
  listFileType = [];
  clientName: string;

  async beforeMount() {
    const promiseA = new Promise((resolve, reject) => {
       Misc.readFileAsync('static/variables/common-utils.json').then(x => {
        resolve(this.clientName);
        this.clientName = x.ClientName;
    })
    });
    
}

  mounted() {
    this.listRemoveStoppedWorkingEmployeesType = [{ Index: 0, Name: this.$t("NotUse") }, { Index: 1, Name: this.$t("Day") }, { Index: 2, Name: this.$t("Week") }, { Index: 3, Name: this.$t("Month") }];
    this.listShowStoppedWorkingEmployeesType = [{ Index: 0, Name: this.$t("NotUse") }, { Index: 1, Name: this.$t("Day") }, { Index: 2, Name: this.$t("Week") }, { Index: 3, Name: this.$t("Month") }];
    this.listSoftware = [{Index: 0, Name: 'Chuẩn'}, {Index: 1, Name: '1Office'}, {Index: 2, Name: 'File'}];
    this.listFileType = [{Index: 0, Name: 'txt'}, {Index: 1, Name: 'excel'}]
    this.listDayInWeek = [
			{ Index: 1, Name: this.$t("Monday") },
			{ Index: 2, Name: this.$t("Tuesday") },
			{ Index: 3, Name: this.$t("Wednesday") },
			{ Index: 4, Name: this.$t("Thursday") },
			{ Index: 5, Name: this.$t("Friday") },
			{ Index: 6, Name: this.$t("Saturday") },
			{ Index: 0, Name: this.$t("Sunday") }
		];
    this.addDayInMonth();
    if(this.config.EventType == "SEND_MAIL_WHEN_DEVICE_OFFLINE"){
      if(Misc.isEmpty(this.config.TitleEmailError)){
        this.config.TitleEmailError = this.$t('EmailFromEPAD').toString();
      }
      if(Misc.isEmpty(this.config.BodyEmailError)){
        this.config.BodyEmailError = this.$t('DeviceOfflined').toString();
      }
    }
  }

  addDayInMonth() {
		for (let i = 1; i <= 31; i++) {
			this.listDayInMonth.push({
				Index: i
			})
		}
	}
  get getLabel() {
    if(this.config.EventType === "DELETE_LOG") {
      return this.$t("DeleteFromPreviousDay")
    }
    else if (this.config.EventType === "DELETE_SYSTEM_COMMAND") {
      return this.$t("DeleteAfterHours")
    }
    else if (this.config.EventType === "EMPLOYEE_SHIFT_INTEGRATE") {
      return this.$t("DownloadFromAfterDay");
  }
    else {
      return this.$t("DownloadFromPreviousDay")
    }
  }

  get dayToCurrentDay() {
    if(this.config.EventType === "DELETE_LOG") {
      return this.$t("Day")
    }else if(this.config.EventType === "EMPLOYEE_SHIFT_INTEGRATE"){
      return this.$t('CurrentDayToDay');
    }
    else {
      return this.$t("DayToCurrentDay")
    }
  }

  focus(x) {
    var theField = eval('this.$refs.' + x);
    console.log(theField);
  
    setTimeout(() => {
                
      theField.focus();
      
  }, 200);
  }
}
