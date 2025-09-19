import { Component, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { groupDeviceApi, IC_GroupDevice } from '@/$api/group-device-api';
import { configByGroupMachineApi, IConfigCollectionGroupDevice } from '@/$api/config-group-api';


import { isNullOrUndefined } from 'util';
import { configApi } from '@/$api/config-api';

const ConfigComponent = () => import('@/components/app-component/config-component/config-component.vue');
const HeaderComponent = () => import("@/components/home/header-component/header-component.vue");
const IntegrateLogRealTimeConfigComponent = () => import('@/components/app-component/config-component/integrate-log-realtime-config.vue');

@Component({
  name: "system-config",
  components: { HeaderComponent, ConfigComponent, IntegrateLogRealTimeConfigComponent }
})
export default class SystemConfigGroupDeviceComponent extends Mixins(ComponentBase) {
  configCollection: IConfigCollectionGroupDevice = {
    DOWNLOAD_LOG: {
      Title: 'AutoDownloadLogFromMachine',
      EventType: 'DOWNLOAD_LOG',
      TimePos: [],
      Email: [],
      PreviousDays: 0,
      AlwaysSend: false,
      SendMailWhenError: false,
      DeleteLogAfterSuccess: false,
      TitleEmailSuccess: '',
      BodyEmailSuccess: '',
      TitleEmailError: '',
      BodyEmailError: ''
    },
    DELETE_LOG: {
      Title: 'AutoDeleteLogFromMachine',
      EventType: 'DELETE_LOG',
      TimePos: [],
      Email: [],
      PreviousDays: 0,
      AlwaysSend: false,
      SendMailWhenError: false,
      DeleteLogAfterSuccess: false,
      TitleEmailSuccess: '',
      BodyEmailSuccess: '',
      TitleEmailError: '',
      BodyEmailError: ''
    },
    START_MACHINE: {
      Title: 'AutoStartMachine',
      EventType: 'START_MACHINE',
      TimePos: [],
      Email: [],
      AlwaysSend: false,
      SendMailWhenError: false,
      DeleteLogAfterSuccess: false,
      TitleEmailSuccess: '',
      BodyEmailSuccess: '',
      TitleEmailError: '',
      BodyEmailError: ''
    },
    DOWNLOAD_USER: {
      Title: 'AutoDownloadUserFromMachine',
      EventType: 'DOWNLOAD_USER',
      TimePos: [],
      Email: [],
      AlwaysSend: false,
      SendMailWhenError: false,
      DeleteLogAfterSuccess: false,
      TitleEmailSuccess: '',
      BodyEmailSuccess: '',
      TitleEmailError: '',
      BodyEmailError: ''
    },
    // RE_PROCESSING_REGISTERCARD: {
    //   Title: 'ReProcessingRegisterCard',
    //   EventType: 'RE_PROCESSING_REGISTERCARD',
    //   TimePos: [],
    //   Email: [],
    //   AlwaysSend: false,
    //   SendMailWhenError: false,
    //   DeleteLogAfterSuccess: false,
    //   TitleEmailSuccess: '',
    //   BodyEmailSuccess: '',
    //   TitleEmailError: '',
    //   BodyEmailError: ''
    // },
    // DOWNLOAD_PARKING_LOG: {
    //   Title: 'DownloadParkingLog',
    //   EventType: 'DOWNLOAD_PARKING_LOG',
    //   TimePos: [],
    //   Email: [],
    //   AlwaysSend: false,
    //   SendMailWhenError: false,
    //   DeleteLogAfterSuccess: false,
    //   TitleEmailSuccess: '',
    //   BodyEmailSuccess: '',
    //   TitleEmailError: '',
    //   BodyEmailError: ''
    // },
  };
  rules = {
    Email: [
      {
        validator: (rule, value: string, callback) => {
          if (value == 'a' && isNullOrUndefined(value) === false) {
          }
          callback();
        }
      },
    ]

  };
  ruleForm: IC_GroupDevice = {
    Name: "",
    Description: "",
  };
  timePosOption: Array<string> = [];
  emailOption: Array<string> = [];

  isLoading: boolean = true;
  comboGroupDevice: any = [];
  get getListConfig() {
    const lstCfg = Object.keys(this.configCollection).map(key => this.configCollection[key]);
    console.log('lstCfg',lstCfg);
    return lstCfg;
  }

  mounted() {
    document.onkeyup = this.handlerCtrlS;
    this.loadComboGroupDevice();
  }

  beforeMount() {
    this.initTimePosData();
    // this.getData(null);
  }

  getData(groupDeviceIndex :number) {
    configByGroupMachineApi.GetAllConfigByGroupMachine(groupDeviceIndex).then(res => {
      const { Data } = res.data;
      this.configCollection = Misc.mergeDeep(this.configCollection, Data);

      if (isNullOrUndefined(this.configCollection.DOWNLOAD_LOG.PreviousDays)) {
        this.configCollection.DOWNLOAD_LOG.PreviousDays = 0;
      }

      if (isNullOrUndefined(this.configCollection.DELETE_LOG.PreviousDays)) {
        this.configCollection.DELETE_LOG.PreviousDays = 0;
      }

      this.isLoading = false;
    });

  }

  initTimePosData() {
    for (let i = 0; i < 24; i++) {
      this.timePosOption.push(`${i.toString().padStart(2, '0')}:00`);
      this.timePosOption.push(`${i.toString().padStart(2, '0')}:30`);
    }
  }

  SaveConfig() {
    var arr_Email = []
    var regex = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
    arr_Email.push(this.configCollection.DOWNLOAD_LOG.Email)
    arr_Email.push(this.configCollection.DELETE_LOG.Email)
    arr_Email.push(this.configCollection.START_MACHINE.Email)
    arr_Email.push(this.configCollection.DOWNLOAD_USER.Email)
    // arr_Email.push(this.configCollection.RE_PROCESSING_REGISTERCARD.Email)

    var isValidEmail = true
    for (let i = 0; i < arr_Email.length; i++) {
      if (arr_Email[i].length > 0) {
        var isInvalidEmail = arr_Email[i].some(item => !regex.test(item))
        if (isInvalidEmail === true) {
          isValidEmail = false
          break
        }
      }
    }

    var regexTimePos = /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/
    var arr_TimePos = []
    arr_TimePos.push(this.configCollection.DOWNLOAD_LOG.TimePos)
    arr_TimePos.push(this.configCollection.DELETE_LOG.TimePos)
    arr_TimePos.push(this.configCollection.START_MACHINE.TimePos)
    arr_TimePos.push(this.configCollection.DOWNLOAD_USER.TimePos)
    // arr_TimePos.push(this.configCollection.RE_PROCESSING_REGISTERCARD.TimePos)


    var isValidTimePos = true
    for (let i = 0; i < arr_TimePos.length; i++) {
      if (arr_TimePos[i].length > 0) {
        var isInvalidTimePos = arr_TimePos[i].some(item => !regexTimePos.test(item))
        if (isInvalidTimePos === true) {
          isValidTimePos = false
          break
        }
      }
    }




    if (isValidEmail === true && isValidTimePos === true) {
      const promiseAllCfg = configByGroupMachineApi.SaveConfig(this.configCollection, this.ruleForm.Index);
      Promise.all([promiseAllCfg]).then(res => {
        this.$saveSuccess();
      });
    }
    else if (isValidEmail === false) {
      this.$alertSaveError(null, null, null, this.$t("InvalidEmail").toString());
    }
    else {
      this.$alertSaveError(null, null, null, this.$t("InvalidTimePos").toString());
    }

  }

  handlerCtrlS(ev: KeyboardEvent) {
    if (ev.ctrlKey && ev.keyCode === 83) {
      ev.preventDefault();
      this.SaveConfig();
    }
  }
  focus(x) {
    var theField = eval('this.$refs.' + x)
    theField.focus()
  }
  async loadComboGroupDevice() {
    return await groupDeviceApi
      .GetGroupDevice()
      .then(res => {
        let a = JSON.parse(JSON.stringify(res.data))
        for (let i = 0; i < a.length; i++) {
          a[i].value = parseInt(a[i].value)
        }
        this.comboGroupDevice = a
      })
  }
   comboGroupDeviceChange() {
      this.getData(this.ruleForm.Index);
  }
}
