import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from '@/$core/base-api'

class DashboardApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig){
        super(_module, config);
    }
        
    public GetDashboard(): Promise<any> {
        return this.get('GetDashboard');
    }

    public SaveDashboard(jsonString)
    {
        const dashboard: IC_DashboardDTO = {
            DashboardConfig: jsonString
        }; 
        return this.post("SaveDashboard", dashboard);
    }
}
export interface AddedParam {
    Key: string,
    Value: any
}

export interface IC_DashboardDTO {
    DashboardConfig: string;
}

export const dashboardApi = new DashboardApi('Dashboard');