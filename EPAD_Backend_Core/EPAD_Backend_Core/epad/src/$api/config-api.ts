import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from '@/$core/base-api'
import { Icon } from "element-ui";

class ConfigApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllConfig(): Promise<any> {
        return this.get('GetAllConfig');
    }
    public IsUsingECMS(): Promise<any> {
        return this.get('IsUsingECMS');
    }

    public GetIntegrateLogRealTimeConfig(): Promise<any> {
        return this.get('GetIntegrateLogRealTimeConfig');
    }
    public GetRealTimeServerLink(): Promise<any> {
        return this.get('GetRealTimeServerLink');
    }
    public GetServerLink(): Promise<any> {
        return this.get('GetServerLink');
    }
    public GetWisenetWaveServerConfig(): Promise<any> {
        return this.get('GetWisenetWaveServerConfig');
    }
    public GetPushNotificatioinLink(): Promise<any> {
        return this.get('GetPushNotificatioinLink');
    }

    public SaveConfig(configCollection: IConfigCollection) {
        let arrConfig: Array<IConfig> = [];
        Object.keys(configCollection).forEach(key => {
            arrConfig.push(configCollection[key]);
        });
        return this.post('UpdateConfig', { Data: configCollection });
    }

    public SaveIntegrateLogRealTimeConfig(config: any) {
        return this.post('UpdateIntegrateLogRealTimeConfig', config);
    }

    public ChangeUpdateUI(uiName: any) {
        return this.get('ChangeUpdateUI?uiName=' + uiName);
    }
}

export interface IConfig {
    Title: string;
    EventType: string;
    TimePos: Array<string>;
    Email: Array<string>;
    SendMailWhenError: boolean;
    SendEmailWithFile?: boolean;
    AfterHours?: number;
    AlwaysSend: boolean;
    PreviousDays?: number;
    WriteToDatabase?: boolean;
    WriteToFile?: boolean;
    WriteToFilePath?: string;
    LinkAPI?: string;
    UsingDatabase?: boolean;
    IsOverwriteData?: boolean;
    DeleteLogAfterSuccess?: boolean;
    NotShowEmailDetail?: boolean;
    TitleEmailSuccess: string;
    BodyEmailSuccess: string;
    TitleEmailError: string;
    BodyEmailError: string;
    UsingBasicMenu: boolean;
    ListSerialNumber: Array<string>;
    AutoIntegrate: boolean;
    BodyTemperature?: number;
    DepartmentIndex?: number;
    RemoveStoppedWorkingEmployeesType?: number;
    RemoveStoppedWorkingEmployeesDays?: number;
    RemoveStoppedWorkingEmployeesWeek?: number;
    RemoveStoppedWorkingEmployeesMonth?: number;
    RemoveStoppedWorkingEmployeesTime?: Date;
    ShowStoppedWorkingEmployeesType?: number;
    ShowStoppedWorkingEmployeesDays?: number;
    ShowStoppedWorkingEmployeesWeek?: number;
    ShowStoppedWorkingEmployeesMonth?: number;
    ShowStoppedWorkingEmployeesTime?: Date;
    Token?: string;
    SoftwareType?: number;
    LinkAPIIntegrate?: string;
    UserName?:string;
    Password?:string;
    FileType?:number;
    IntegrateWhenNotInclareDepartment?: boolean;
    AutoCreateDepartmentImportEmployee?: boolean;
    EmailAllowImportGoogleSheet?: Array<string>;
}

export interface IShiftConfig extends IConfig {
    FromDate: Date;
    ToDate: Date;
}

export interface IConfigCollection {
    DOWNLOAD_LOG: IConfig;
    DELETE_LOG: IConfig;
    START_MACHINE: IConfig;
    DOWNLOAD_USER: IConfig;
    ADD_OR_DELETE_USER: IConfig;
    EMPLOYEE_INTEGRATE: IConfig;
    EMPLOYEE_SHIFT_INTEGRATE: IShiftConfig;
    INTEGRATE_LOG: IConfig;
    FULL_CAPACITY: IConfig;
    TIME_SYNC: IConfig;
    DELETE_SYSTEM_COMMAND: IConfig;
    GENERAL_SYSTEM_CONFIG: IConfig;
    ECMS_DEFAULT_MEAL_CARD_DEPARTMENT: IConfig;
    MANAGE_STOPPED_WORKING_EMPLOYEES_DATA: IConfig;
    INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL: IConfig;
    SEND_MAIL_WHEN_DEVICE_OFFLINE: IConfig;
    EMPLOYEE_INTEGRATE_TO_DATABASE: IConfig;
    LOG_INTEGRATE_TO_DATABASE: IConfig;
    RE_PROCESSING_REGISTERCARD: IConfig;
    DOWNLOAD_STATE_LOG: IConfig;
    DOWNLOAD_PARKING_LOG: IConfig;
    CREATE_DEPARTMENT_IMPORT_EMPLOYEE: IConfig;
    INTEGRATE_INFO_TO_OFFLINE: IConfig;
    AUTO_DELETE_BLACKLIST: IConfig;
    // CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET: IConfig;
}

export const configApi = new ConfigApi('Config');