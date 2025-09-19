import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { configApi, IConfig, IConfigCollection } from '@/$api/config-api'
@Component({
    name: "system-config",
    components: { HeaderComponent, DataTableComponent }
})
export default class SystemConfigComponent extends Mixins(ComponentBase) {
    configCollection: IConfigCollection = {
        DOWNLOAD_LOG: {
            Title: 'AutoDownloadLogFromMachine',
            EventType: 'DOWNLOAD_LOG',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: true,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
        DELETE_LOG: {
            Title: 'AutoDeleteLogFromMachine',
            EventType: 'DELETE_LOG',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: false,
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
            UsingBasicMenu: true,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
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
            BodyEmailError: '',
            UsingBasicMenu: false,
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
            UsingBasicMenu: true,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
        EMPLOYEE_INTEGRATE: {
            Title: 'EmployeeIntegrate',
            EventType: 'EMPLOYEE_INTEGRATE',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: true,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
        EMPLOYEE_SHIFT_INTEGRATE: {
            Title: 'EmployeeShiftIntegrate',
            EventType: 'EMPLOYEE_SHIFT_INTEGRATE',
            TimePos: [],
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: true,
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
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: true,
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
            AlwaysSend: false,
            SendMailWhenError: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: true,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            Password: '',
            UserName: '',
            SoftwareType: 0,
            FileType: 0
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
            UsingBasicMenu: true,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00
        },
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
            UsingBasicMenu: true,
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
            UsingBasicMenu: false,
            ListSerialNumber: [],
            AutoIntegrate: false,
            DepartmentIndex: 0
        },
        FULL_CAPACITY: null,
        TIME_SYNC: null,
        MANAGE_STOPPED_WORKING_EMPLOYEES_DATA: null,
        SEND_MAIL_WHEN_DEVICE_OFFLINE: null,
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
            UsingBasicMenu: true,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: '',
            PreviousDays: null,
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
            UsingBasicMenu: false,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: ''
        },
        RE_PROCESSING_REGISTERCARD: {
            Title: 'ReProcessingRegisterCard',
            EventType: 'RE_PROCESSING_REGISTERCARD',
            TimePos: [],
            PreviousDays: 0,
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: false,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: ''
        },
        DOWNLOAD_PARKING_LOG: {
            Title: 'DownloadParkingLog',
            EventType: 'DOWNLOAD_PARKING_LOG',
            TimePos: [],
            PreviousDays: 0,
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: false,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: ''
        },
        INTEGRATE_INFO_TO_OFFLINE: {
            Title: 'IntegrateInfoToOffline',
            EventType: 'INTEGRATE_INFO_TO_OFFLINE',
            TimePos: [],
            PreviousDays: 0,
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: false,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: ''
        },
        AUTO_DELETE_BLACKLIST: {
            Title: 'AutoDeleteBlacklist',
            EventType: 'AUTO_DELETE_BLACKLIST',
            TimePos: [],
            PreviousDays: 0,
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: false,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: ''
        },
        DOWNLOAD_STATE_LOG: {
            Title: 'DownloadStateLog',
            EventType: 'DOWNLOAD_STATE_LOG',
            TimePos: [],
            PreviousDays: 0,
            Email: [],
            AlwaysSend: false,
            SendMailWhenError: false,
            DeleteLogAfterSuccess: false,
            TitleEmailSuccess: '',
            BodyEmailSuccess: '',
            TitleEmailError: '',
            BodyEmailError: '',
            UsingBasicMenu: true,
            ListSerialNumber: [],
            AutoIntegrate: false,
            BodyTemperature: 37.00,
            LinkAPI: '',
            Token: ''
        },
        CREATE_DEPARTMENT_IMPORT_EMPLOYEE: {
            Title: 'AutoCreateDepartmentImportEmployee',
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
            UsingBasicMenu: false,
            ListSerialNumber: [],
            AutoIntegrate: false,
            DepartmentIndex: 0,
            AutoCreateDepartmentImportEmployee: false
        }
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
        //     UsingBasicMenu: false,
        //     ListSerialNumber: [],
        //     AutoIntegrate: false,
        //     DepartmentIndex: 0
        // },
    };
    timePosOption: Array<string> = [];
    emailOption: Array<string> = [];
    serialNumberOption: Array<string> = [];
    beforeMount() {
        this.initTimePosData();
        this.intiEmailData();
        this.initDeviceData();
        this.getData();
    }

    getData() {
        configApi.GetAllConfig().then(res => {
            const { Data } = res.data;
            Object.assign(this.configCollection, Data);
        });
    }

    initDeviceData() {
        this.serialNumberOption.push('dahdahh','dadad','hoang');
    }

    initTimePosData() {
        for (let i = 0; i < 24; i++) {
            this.timePosOption.push(`${i.toString().padStart(2, '0')}:00`);
            this.timePosOption.push(`${i.toString().padStart(2, '0')}:30`);
        }
    }

    intiEmailData() {
        // this.emailOption.push('abc@gmail.com', 'abc1@gmail.com', 'abc2@gmail.com');
    }

    SaveConfig() {
        // Object.keys(this.configCollection).forEach(key => {
        //   this.configCollection[key].AlwaysSend = !this.configCollection[key].SendMailWhenError;
        // });
        console.log(this.configCollection);
        configApi.SaveConfig(this.configCollection).then(res => {
            this.$saveSuccess();
        });
    }
}
