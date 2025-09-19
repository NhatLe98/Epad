import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";
import { DateFilter } from 'ag-grid-community';

class HR_CustomerCardApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetCustomerCardAtPage(filter: string, page: number, pageSize: number): Promise<any> {
        const params = new URLSearchParams();
        params.append('filter', filter.toString());
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());

        return this.get('GetCustomerCardAtPage', { params: params })
    }

    public GetAllCustomerCard(): Promise<any> {
        return this.get('GetAllCustomerCard')
    }

    public SyncCustomerCardToDevice(): Promise<any> {
        return this.get('SyncCustomerCardToDevice')
    }

    public GetCustomerCardRequirement(): Promise<any> {
        return this.get('GetCustomerCardRequirement')
    }

    public AddCustomerCard(param: CustomerCardModel): Promise<any> {
        return this.post('AddCustomerCard', param);
    }

    public DeleteCustomerCard(param: Array<any>): Promise<any> {
        return this.post('DeleteCustomerCard', param);
    }
    
    public AddCustomerCardFromExcel(param: Array<any>): Promise<any> {
        return this.post('AddCustomerCardFromExcel', param);
    }
}

export interface CustomerCardModel {
    Index?: string;
    CardNumber?: string;
    CardID?: string;
    UserCode?: string;
    Status?: boolean;
    Object?: string;
    ObjectString?: string;
    UpdatedDate?: Date;
    UpdatedDateString?: string;
    CardUpdatedDate?: Date;
    CardUpdatedDateString?: string;
    IsSyncToDevice?: boolean;
}

export const hrCustomerCardApi = new HR_CustomerCardApi("HR_CustomerCard");
