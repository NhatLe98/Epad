import { Component, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { configApi, IConfigCollection } from '@/$api/config-api';
import { deviceApi } from "@/$api/device-api";
import { isNullOrUndefined } from 'util';
import { config } from 'vue/types/umd';
import { departmentApi } from '@/$api/department-api';
import { groupDeviceApi } from "@/$api/group-device-api";
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";

const ConfigComponent = () => import('@/components/app-component/config-component/config-component.vue');
const HeaderComponent = () => import("@/components/home/header-component/header-component.vue");
const IntegrateLogRealTimeConfigComponent = () => import('@/components/app-component/config-component/integrate-log-realtime-config.vue');

@Component({
    name: "system-config",
    components: { HeaderComponent, ConfigComponent, IntegrateLogRealTimeConfigComponent }
})
export default class SystemConfigComponent extends Mixins(ComponentBase) {

    usingBasicMenu: boolean = true;
    clientName: string;
    isUsingECMS: boolean = false;
    departmentOptions = [];
    groupDeviceOption = [];
    colorThemes = [{value: 'Default', name: this.$t('Default')}];
    colorTheme = '';
    currentColorTheme = '';
    mounted() {
        document.onkeyup = this.handlerCtrlS;
        this.IsUsingECMS();
    }

    beforeMount() {
        this.colorTheme = (!UI_NAME || UI_NAME == '') ? 'Default' : UI_NAME;
        this.currentColorTheme = this.colorTheme;
        Misc.readFileAsync('static/variables/color.json').then(x => {
            if(x.ColorThemes){
                const colorThemes = Object.entries(x.ColorThemes);
                if(colorThemes && colorThemes.length > 0){
                    colorThemes.forEach(element => {
                        this.colorThemes.push({value: element[0], name: this.$t(element[0])});
                    });
                }
            }
        });
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.usingBasicMenu = x.UsingBasicMenu;
            this.clientName = x.ClientName;
            this.initTimePosData();
            this.initDeviceData();
            this.initGroupData();
            if (this.clientName == "MAY") {
                this.getData_May();
            } else {
                this.getData();
            }
        });
        this.getDepartment();
    }

    async getDepartment() {
        return await departmentApi.GetDepartment().then((res) => {
            const { data } = res as any;
            // let arr = JSON.parse(JSON.stringify(data));
            for (let i = 0; i < data.length; i++) {
                data[i].value = parseInt(data[i].value);
            }
            this.departmentOptions = data;
        });
    }

    configCollection: IConfigCollection = {
        GENERAL_SYSTEM_CONFIG: {
            Title: 'GeneralSystemConfig',
            EventType: 'GENERAL_SYSTEM_CONFIG',
            TimePos: [],
            Email: [],
            AfterHours: 24,
            AlwaysSend: false,
            SendEmailWithFile: false,
            SendMailWhenError: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
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
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
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
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
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
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
        DOWNLOAD_USER: {
            Title: 'AutoDownloadUserFromMachine',
            EventType: 'DOWNLOAD_USER',
            TimePos: [],
            Email: [],
            IsOverwriteData: false,
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
        ADD_OR_DELETE_USER: {
            Title: 'AutoAddOrDeleteUserFromMachine',
            EventType: 'ADD_OR_DELETE_USER',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            IntegrateWhenNotInclareDepartment: false
        },
        EMPLOYEE_INTEGRATE: {
            Title: 'EmployeeIntegrate',
            EventType: 'EMPLOYEE_INTEGRATE',
            TimePos: [],
            WriteToDatabase: false,
            Email: [],
            LinkAPI: "",
            UsingDatabase: true,
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
        EMPLOYEE_SHIFT_INTEGRATE: {
            Title: 'EmployeeShiftIntegrate',
            EventType: 'EMPLOYEE_SHIFT_INTEGRATE',
            TimePos: [],
            WriteToDatabase: false,
            Email: [],
            LinkAPI: "",
            UsingDatabase: true,
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            FromDate: new Date(),
            ToDate: new Date()
        },
        INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL: {
            Title: 'IntegrateEmployeeBusinessTravel',
            EventType: 'INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL',
            TimePos: [],
            WriteToDatabase: false,
            Email: [],
            LinkAPI: "",
            UsingDatabase: true,
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
        INTEGRATE_LOG: {
            Title: 'IntegrateLog',
            EventType: 'INTEGRATE_LOG',
            TimePos: [],
            Email: [],
            PreviousDays: 0,
            WriteToDatabase: false,
            WriteToFile: false,
            WriteToFilePath: '',
            LinkAPI: "",
            UsingDatabase: false,
            AlwaysSend: false,
            SendMailWhenError: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            Password: '',
            UserName: '',
            SoftwareType: 0,
            FileType: 0
        },
        FULL_CAPACITY: {
            Title: 'FullCapacityWarning',
            EventType: 'FULL_CAPACITY',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            NotShowEmailDetail: true,
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
        TIME_SYNC: {
            Title: 'TimeSyncOnDevice',
            EventType: 'TIME_SYNC',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
        DELETE_SYSTEM_COMMAND: {
            Title: 'AutoDeleteSystemCommand',
            EventType: 'DELETE_SYSTEM_COMMAND',
            TimePos: [],
            Email: [],
            AfterHours: 24,
            AlwaysSend: false,
            SendEmailWithFile: false,
            SendMailWhenError: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
        ECMS_DEFAULT_MEAL_CARD_DEPARTMENT: {
            Title: 'DefaultECMSMealCardDepartment',
            EventType: 'ECMS_DEFAULT_MEAL_CARD_DEPARTMENT',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendEmailWithFile: false,
            SendMailWhenError: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            DepartmentIndex: 0
        },
        MANAGE_STOPPED_WORKING_EMPLOYEES_DATA: {
            Title: 'ManageStoppedWorkingEmployeesData',
            EventType: 'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendEmailWithFile: false,
            SendMailWhenError: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            DepartmentIndex: 0,
            RemoveStoppedWorkingEmployeesType: 0,
            RemoveStoppedWorkingEmployeesDays: 0,
            RemoveStoppedWorkingEmployeesWeek: 1,
            RemoveStoppedWorkingEmployeesMonth: 1,
            RemoveStoppedWorkingEmployeesTime: new Date(),
            ShowStoppedWorkingEmployeesType: 0,
            ShowStoppedWorkingEmployeesDays: 0,
            ShowStoppedWorkingEmployeesWeek: 1,
            ShowStoppedWorkingEmployeesMonth: 1,
            ShowStoppedWorkingEmployeesTime: new Date(),
        },
        SEND_MAIL_WHEN_DEVICE_OFFLINE: {
            Title: 'SendEmailWhenDeviceOffline',
            EventType: 'SEND_MAIL_WHEN_DEVICE_OFFLINE',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendEmailWithFile: false,
            SendMailWhenError: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            DepartmentIndex: 0,
        },
        EMPLOYEE_INTEGRATE_TO_DATABASE: {
            Title: 'EmployeeIntegrateToDatabase',
            EventType: 'EMPLOYEE_INTEGRATE_TO_DATABASE',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            Token: '',
            LinkAPIIntegrate: '',
            PreviousDays: 0,
            SoftwareType: 0,
            Password: '',
            UserName: ''
        },
        LOG_INTEGRATE_TO_DATABASE: {
            Title: 'LogIntegrateToDatabase',
            EventType: 'LOG_INTEGRATE_TO_DATABASE',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            Token: '',
            LinkAPIIntegrate: ''
        },
        RE_PROCESSING_REGISTERCARD: {
            Title: 'ReProcessingRegisterCard',
            EventType: 'RE_PROCESSING_REGISTERCARD',
            TimePos: [],
            Email: [],
            PreviousDays: 0,
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: '',
            LinkAPIIntegrate: ''
        },

        DOWNLOAD_PARKING_LOG: {
            Title: 'DownloadParkingLog',
            EventType: 'DOWNLOAD_PARKING_LOG',
            TimePos: [],
            Email: [],
            PreviousDays: 0,
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: '',
            LinkAPIIntegrate: ''
        },
        INTEGRATE_INFO_TO_OFFLINE: {
            Title: 'IntegrateInfoToOffline',
            EventType: 'INTEGRATE_INFO_TO_OFFLINE',
            TimePos: [],
            Email: [],
            PreviousDays: 0,
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: '',
            LinkAPIIntegrate: ''
        },
        AUTO_DELETE_BLACKLIST: {
            Title: 'AutoDeleteBlacklist',
            EventType: 'AUTO_DELETE_BLACKLIST',
            TimePos: [],
            Email: [],
            PreviousDays: 0,
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: '',
            LinkAPIIntegrate: ''
        },
        DOWNLOAD_STATE_LOG: {
            Title: 'DownloadStateLog',
            EventType: 'DOWNLOAD_STATE_LOG',
            TimePos: [],
            Email: [],
            PreviousDays: 0,
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: ''
        },
        CREATE_DEPARTMENT_IMPORT_EMPLOYEE: {
            Title: 'ConfigAutoCreateDepartmentImportEmployee',
            EventType: 'CREATE_DEPARTMENT_IMPORT_EMPLOYEE',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendEmailWithFile: false,
            SendMailWhenError: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: !this.usingBasicMenu,
            ListSerialNumber: [],
            AutoIntegrate: false,
            DepartmentIndex: 0,
            AutoCreateDepartmentImportEmployee: false
        },
        // CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET: {
        //     Title: 'ConfigEmailAllowImportGGSheet',
        //     EventType: 'CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET',
        //     TimePos: [],
        //     Email: [],
        //     AlwaysSend: false,
        //     SendEmailWithFile: false,
        //     SendMailWhenError: false,
        //     TitleEmailSuccess: '',
        //     BodyEmailSuccess: '',
        //     TitleEmailError: '',
        //     BodyEmailError: '',
        //     UsingBasicMenu: !this.usingBasicMenu,
        //     ListSerialNumber: [],
        //     AutoIntegrate: false,
        //     DepartmentIndex: 0,
        //     EmailAllowImportGoogleSheet: []
        // },
    };
    rules = {
        Email: [
            {
                trigger: "change",
                validator: (rule, value: string, callback) => {
                    if (value == 'a' && isNullOrUndefined(value) === false) {
                    }
                    callback();
                }
            },
        ]
    };
    timePosOption: Array<string> = [];
    emailOption: Array<string> = [];
    serialNumberOption: any = [];


    configIntegrateLogRealtime: any = {
        Title: 'IntegrateLogRealTimeTitle',
        IntegrateLogRealtime: false,
        LinkAPI: ''
    };
    isLoading: boolean = true;

    get getListConfig() {
        const lstCfg = Object.keys(this.configCollection).map(key => this.configCollection[key]);
        //console.log('lstCfg', lstCfg);
        return lstCfg;
    }

    async IsUsingECMS() {
        await configApi.IsUsingECMS().then((res) => {
            this.isUsingECMS = res.data;
        });
    }

    getData() {
        configApi.GetAllConfig().then(res => {
            const { Data } = res.data;
            this.configCollection = Misc.mergeDeep(this.configCollection, Data);

            if (isNullOrUndefined(this.configCollection.DOWNLOAD_LOG.PreviousDays)) {
                this.configCollection.DOWNLOAD_LOG.PreviousDays = 0;
            }
            if (isNullOrUndefined(this.configCollection.DOWNLOAD_STATE_LOG.PreviousDays)) {
                this.configCollection.DOWNLOAD_STATE_LOG.PreviousDays = 0;
            }
            if (isNullOrUndefined(this.configCollection.DELETE_LOG.PreviousDays)) {
                this.configCollection.DELETE_LOG.PreviousDays = 0;
            }
            if (isNullOrUndefined(this.configCollection.DOWNLOAD_USER.IsOverwriteData)) {
                this.configCollection.DOWNLOAD_USER.IsOverwriteData = false;
            }
            if (isNullOrUndefined(this.configCollection.EMPLOYEE_INTEGRATE.UsingDatabase)) {
                this.configCollection.EMPLOYEE_INTEGRATE.UsingDatabase = false;
            }
            if (isNullOrUndefined(this.configCollection.EMPLOYEE_SHIFT_INTEGRATE.UsingDatabase)) {
                this.configCollection.EMPLOYEE_SHIFT_INTEGRATE.UsingDatabase = false;
            }
            if (isNullOrUndefined(this.configCollection.INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL.UsingDatabase)) {
                this.configCollection.INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL.UsingDatabase = false;
            }
            if (isNullOrUndefined(this.configCollection.SEND_MAIL_WHEN_DEVICE_OFFLINE.UsingDatabase)) {
                this.configCollection.SEND_MAIL_WHEN_DEVICE_OFFLINE.UsingDatabase = false;
            }
            if (isNullOrUndefined(this.configCollection.INTEGRATE_LOG.UsingDatabase)) {
                this.configCollection.INTEGRATE_LOG.UsingDatabase = false;
            }
            if (isNullOrUndefined(this.configCollection.DELETE_SYSTEM_COMMAND.SendEmailWithFile)) {
                this.configCollection.DELETE_SYSTEM_COMMAND.SendEmailWithFile = false;
            }
            if (isNullOrUndefined(this.configCollection.DELETE_SYSTEM_COMMAND.AfterHours)) {
                this.configCollection.DELETE_SYSTEM_COMMAND.AfterHours = 0;
            }
            this.configCollection.DOWNLOAD_LOG.UsingBasicMenu = true;
            this.configCollection.DELETE_LOG.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.START_MACHINE.UsingBasicMenu = true;
            this.configCollection.INTEGRATE_LOG.UsingBasicMenu = !this.usingBasicMenu;;
            this.configCollection.DELETE_SYSTEM_COMMAND.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.ADD_OR_DELETE_USER.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.DOWNLOAD_USER.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.EMPLOYEE_INTEGRATE.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.EMPLOYEE_SHIFT_INTEGRATE.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.SEND_MAIL_WHEN_DEVICE_OFFLINE.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.FULL_CAPACITY.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.TIME_SYNC.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.ECMS_DEFAULT_MEAL_CARD_DEPARTMENT.UsingBasicMenu = this.isUsingECMS && !this.usingBasicMenu;
            this.configCollection.DOWNLOAD_STATE_LOG.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.RE_PROCESSING_REGISTERCARD.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.DOWNLOAD_PARKING_LOG.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.INTEGRATE_INFO_TO_OFFLINE.UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection.CREATE_DEPARTMENT_IMPORT_EMPLOYEE.UsingBasicMenu = this.clientName == 'Mondelez' ? this.usingBasicMenu : !this.usingBasicMenu;
            this.configCollection.AUTO_DELETE_BLACKLIST.UsingBasicMenu = !this.usingBasicMenu;
            // this.configCollection.CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET.UsingBasicMenu = !this.usingBasicMenu;


            // if(this.clientName == "Ortholite"){
            //     var now = new Date();
            //     this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].UsingBasicMenu = true;
            //     this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].RemoveStoppedWorkingEmployeesTime = new Date(new Date(this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].RemoveStoppedWorkingEmployeesTime).getTime() + now.getTimezoneOffset() * 60000);
            //     this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].ShowStoppedWorkingEmployeesTime = new Date(new Date(this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].ShowStoppedWorkingEmployeesTime).getTime() + now.getTimezoneOffset() * 60000);                
            // }else{
            //     this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].UsingBasicMenu = false;
            //     if (isNullOrUndefined(this.configCollection.EMPLOYEE_SHIFT_INTEGRATE.PreviousDays)) {
            //         this.configCollection.EMPLOYEE_SHIFT_INTEGRATE.PreviousDays = 0;
            //     }
            // }

            var now = new Date();
            this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].UsingBasicMenu = !this.usingBasicMenu;
            this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].RemoveStoppedWorkingEmployeesTime = new Date(new Date(this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].RemoveStoppedWorkingEmployeesTime).getTime() + now.getTimezoneOffset() * 60000);
            this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].ShowStoppedWorkingEmployeesTime = new Date(new Date(this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].ShowStoppedWorkingEmployeesTime).getTime() + now.getTimezoneOffset() * 60000);
            if (isNullOrUndefined(this.configCollection.EMPLOYEE_SHIFT_INTEGRATE.PreviousDays)) {
                this.configCollection.EMPLOYEE_SHIFT_INTEGRATE.PreviousDays = 0;
            }

            this.isLoading = false;
        });
        if (this.usingBasicMenu == false) {
            configApi.GetIntegrateLogRealTimeConfig().then(res => {
                const Data = res.data;
                Object.assign(this.configIntegrateLogRealtime, Data);
            })
        }
    }
    initDeviceData() {
        return deviceApi.GetDeviceAll().then((res) => {
            this.serialNumberOption = res.data;
        });
    }

    initGroupData() {
        return groupDeviceApi.GetGroupDevice().then((res: any) => {
            this.groupDeviceOption = res.data;
        });
    }
    getData_May() {
        configApi.GetAllConfig().then(res => {
            const { Data } = res.data;
            this.configCollection = Misc.mergeDeep(this.configCollection, Data);

            if (isNullOrUndefined(this.configCollection.DOWNLOAD_LOG.PreviousDays)) {
                this.configCollection.DOWNLOAD_LOG.PreviousDays = 0;
            }
            if (isNullOrUndefined(this.configCollection.DELETE_LOG.PreviousDays)) {
                this.configCollection.DELETE_LOG.PreviousDays = 0;
            }
            if (isNullOrUndefined(this.configCollection.DOWNLOAD_USER.IsOverwriteData)) {
                this.configCollection.DOWNLOAD_USER.IsOverwriteData = true;
            }
            if (isNullOrUndefined(this.configCollection.EMPLOYEE_INTEGRATE.UsingDatabase)) {
                this.configCollection.EMPLOYEE_INTEGRATE.UsingDatabase = true;
            }
            if (isNullOrUndefined(this.configCollection.INTEGRATE_LOG.UsingDatabase)) {
                this.configCollection.INTEGRATE_LOG.UsingDatabase = false;
            }
            if (isNullOrUndefined(this.configCollection.DELETE_SYSTEM_COMMAND.SendEmailWithFile)) {
                this.configCollection.DELETE_SYSTEM_COMMAND.SendEmailWithFile = false;
            }
            if (isNullOrUndefined(this.configCollection.DELETE_SYSTEM_COMMAND.AfterHours)) {
                this.configCollection.DELETE_SYSTEM_COMMAND.AfterHours = 0;
            }
            this.configCollection.DOWNLOAD_LOG.UsingBasicMenu = true;
            this.configCollection.DELETE_LOG.UsingBasicMenu = true;
            this.configCollection.START_MACHINE.UsingBasicMenu = true;
            this.configCollection.INTEGRATE_LOG.UsingBasicMenu = !this.usingBasicMenu;;
            this.configCollection.DELETE_SYSTEM_COMMAND.UsingBasicMenu = !this.usingBasicMenu;
            //change apply MAY
            this.configCollection.ADD_OR_DELETE_USER.UsingBasicMenu = false;
            this.configCollection.DOWNLOAD_USER.UsingBasicMenu = false;
            this.configCollection.EMPLOYEE_INTEGRATE.UsingBasicMenu = false;
            this.configCollection.FULL_CAPACITY.UsingBasicMenu = false;
            this.configCollection.DOWNLOAD_PARKING_LOG.UsingBasicMenu = false;
            this.configCollection.RE_PROCESSING_REGISTERCARD.UsingBasicMenu = false;
            this.configCollection.INTEGRATE_INFO_TO_OFFLINE.UsingBasicMenu = false;
            this.configCollection.AUTO_DELETE_BLACKLIST.UsingBasicMenu = false;


            this.configCollection.TIME_SYNC.UsingBasicMenu = !this.usingBasicMenu;
            this.isLoading = false;
        });
        if (this.usingBasicMenu == false) {
            configApi.GetIntegrateLogRealTimeConfig().then(res => {
                const Data = res.data;
                Object.assign(this.configIntegrateLogRealtime, Data);
            })
        }
    }

    initTimePosData() {
        for (let i = 0; i < 24; i++) {
            this.timePosOption.push(`${i.toString().padStart(2, '0')}:00`);
            this.timePosOption.push(`${i.toString().padStart(2, '0')}:30`);
        }
    }

    SaveConfig() {
        if (this.clientName !== "Mondelez") {
            if (this.configCollection.DELETE_LOG && this.configCollection.DELETE_LOG.TimePos.length > 0 && this.configCollection.DELETE_LOG.PreviousDays < 60) {
                this.$alertSaveError(null, null, null, this.$t("Can'tDeleteLogLessThanTwoMonth").toString());
                return;
            }
        }
        // if(this.clientName == "Ortholite"){
        var now = new Date();
        this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].RemoveStoppedWorkingEmployeesTime = new Date(this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].RemoveStoppedWorkingEmployeesTime.getTime() - now.getTimezoneOffset() * 60000);
        this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].ShowStoppedWorkingEmployeesTime = new Date(this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].ShowStoppedWorkingEmployeesTime.getTime() - now.getTimezoneOffset() * 60000);
        // }

        var arr_Email = []
        var regex = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
        arr_Email.push(this.configCollection.DOWNLOAD_LOG.Email)
        arr_Email.push(this.configCollection.DELETE_LOG.Email)
        arr_Email.push(this.configCollection.START_MACHINE.Email)
        arr_Email.push(this.configCollection.DOWNLOAD_USER.Email)
        arr_Email.push(this.configCollection.ADD_OR_DELETE_USER.Email)
        arr_Email.push(this.configCollection.EMPLOYEE_INTEGRATE.Email)
        arr_Email.push(this.configCollection.EMPLOYEE_SHIFT_INTEGRATE.Email)
        arr_Email.push(this.configCollection.INTEGRATE_LOG.Email)
        arr_Email.push(this.configCollection.INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL.Email)
        arr_Email.push(this.configCollection.RE_PROCESSING_REGISTERCARD.Email)
        arr_Email.push(this.configCollection.DOWNLOAD_PARKING_LOG.Email)
        arr_Email.push(this.configCollection.INTEGRATE_INFO_TO_OFFLINE.Email)
        arr_Email.push(this.configCollection.AUTO_DELETE_BLACKLIST.Email)
        if (this.configCollection.SEND_MAIL_WHEN_DEVICE_OFFLINE.EventType == "SEND_MAIL_WHEN_DEVICE_OFFLINE") {
            if (Misc.isEmpty(this.configCollection.SEND_MAIL_WHEN_DEVICE_OFFLINE.TitleEmailError)) {
                this.configCollection.SEND_MAIL_WHEN_DEVICE_OFFLINE.TitleEmailError = this.$t('EmailFromEPAD').toString();
            }
            if (Misc.isEmpty(this.configCollection.SEND_MAIL_WHEN_DEVICE_OFFLINE.BodyEmailError)) {
                this.configCollection.SEND_MAIL_WHEN_DEVICE_OFFLINE.BodyEmailError = this.$t('DeviceOfflined').toString();
            }
        }
        arr_Email.push(this.configCollection.SEND_MAIL_WHEN_DEVICE_OFFLINE.Email)

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

        var isValidLinkAPI = true
        var arr_LinkAPI = []
        if (this.configCollection.EMPLOYEE_INTEGRATE.UsingDatabase === false
            && this.configCollection.EMPLOYEE_INTEGRATE.LinkAPI
            && this.configCollection.EMPLOYEE_INTEGRATE.LinkAPI.trim().length === 0) {
            isValidLinkAPI = false;
        }


        var regexTimePos = /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/
        var arr_TimePos = []
        arr_TimePos.push(this.configCollection.DOWNLOAD_LOG.TimePos)
        arr_TimePos.push(this.configCollection.DELETE_LOG.TimePos)
        arr_TimePos.push(this.configCollection.START_MACHINE.TimePos)
        arr_TimePos.push(this.configCollection.DOWNLOAD_USER.TimePos)
        arr_TimePos.push(this.configCollection.ADD_OR_DELETE_USER.TimePos)
        arr_TimePos.push(this.configCollection.EMPLOYEE_INTEGRATE.TimePos)
        arr_TimePos.push(this.configCollection.EMPLOYEE_SHIFT_INTEGRATE.TimePos)
        arr_TimePos.push(this.configCollection.INTEGRATE_LOG.TimePos)
        arr_TimePos.push(this.configCollection.INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL.TimePos)
        arr_TimePos.push(this.configCollection.SEND_MAIL_WHEN_DEVICE_OFFLINE.TimePos)
        arr_TimePos.push(this.configCollection.RE_PROCESSING_REGISTERCARD.TimePos)
        arr_TimePos.push(this.configCollection.DOWNLOAD_PARKING_LOG.TimePos)
        arr_TimePos.push(this.configCollection.INTEGRATE_INFO_TO_OFFLINE.TimePos)
        arr_TimePos.push(this.configCollection.AUTO_DELETE_BLACKLIST.TimePos)

        var isValidTimePos = true
        for (let i = 0; i < arr_TimePos.length; i++) {
            if (arr_TimePos[i].length === 0) {
                var isInvalidTimePos = arr_TimePos[i].some(item => !regexTimePos.test(item))
                if (isInvalidTimePos === true) {
                    isValidTimePos = false
                    break
                }
            }
        }

        if (isValidEmail === true && isValidTimePos === true && isValidLinkAPI === true) {
            const promiseAllCfg = configApi.SaveConfig(this.configCollection);
            const promiseLogRealtime = configApi.SaveIntegrateLogRealTimeConfig(this.configIntegrateLogRealtime);
            // console.log('configCollection', this.configCollection);
            Promise.all([promiseAllCfg, promiseLogRealtime]).then(res => {
                this.$saveSuccess();
                // window.location.reload();
                //    if(this.clientName == "Ortholite"){
                var now = new Date();
                this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].RemoveStoppedWorkingEmployeesTime = new Date(this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].RemoveStoppedWorkingEmployeesTime.getTime() + now.getTimezoneOffset() * 60000);
                this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].ShowStoppedWorkingEmployeesTime = new Date(this.configCollection["MANAGE_STOPPED_WORKING_EMPLOYEES_DATA"].ShowStoppedWorkingEmployeesTime.getTime() + now.getTimezoneOffset() * 60000);
                // }
            });
        }
        else if (isValidEmail === false) {
            this.$alertSaveError(null, null, null, this.$t("InvalidEmail").toString());
        }
        else if (isValidLinkAPI === false) {
            this.$alertSaveError(null, null, null, this.$t("InvalidLinkAPI").toString());
        }
        else {
            this.$alertSaveError(null, null, null, this.$t("InvalidTimePos").toString());
        }

        if(this.currentColorTheme != this.colorTheme){
            this.changeUpdateUI();
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
    changeUpdateUI(){
        // console.log((window as any).__env.updateUI);
        configApi.ChangeUpdateUI(this.colorTheme).then((res: any) => {
            // console.log(res)
            location.reload();
        }).catch((ex: any) => {
            console.log(ex)
        })
    }
}
