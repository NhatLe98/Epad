import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class HR_CustomerInfoApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetCustomerAtPage(employeeATID: Array<string>, filter: string, page: number, pageSize: number, employeeType: number): Promise<any> {
        const params = new URLSearchParams();
        params.append('filter', filter.toString());
        employeeATID.forEach(e => {
            params.append('employeeATID', e.toString());
        });
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        params.append('employeeType', employeeType.toString());

        return this.get('GetCustomerAtPage', { params: params })
    }

    public GetCustomerAtPageAdvance(employeeATID: Array<string>, filter: string, page: number, 
        pageSize: number, employeeType: number, studentOfParent: Array<string>, 
        filterDepartments: Array<number>): Promise<any> {
        var param = {
            employeeATID: employeeATID,
            filter: filter,
            page: page,
            pageSize: pageSize,
            employeeType: employeeType,
            studentOfParent: studentOfParent,
            filterDepartments: filterDepartments
        };
        return this.post('GetCustomerAtPageAdvance', param);
    }

    public GetCustomerAtPageByEmployeeATID(employeeATID: Array<string>, filter: string, page: number, pageSize: number, employeeType: number): Promise<any> {
        const params = new URLSearchParams();
        params.append('filter', filter.toString());
        employeeATID.forEach(e => {
            params.append('employeeATID', e.toString());
        });
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        params.append('employeeType', employeeType.toString());

        return this.get('GetCustomerAtPageByEmployeeATID', { params: params })
    }

    public GetCustomerAtPageAdvanceByEmployeeATID(employeeATID: Array<string>, filter: string, page: number, 
        pageSize: number, employeeType: number, studentOfParent: Array<string>, 
        filterDepartments: Array<number>): Promise<any> {
        var param = {
            employeeATID: employeeATID,
            filter: filter,
            page: page,
            pageSize: pageSize,
            employeeType: employeeType,
            studentOfParent: studentOfParent,
            filterDepartments: filterDepartments
        };
        return this.post('GetCustomerAtPageAdvanceByEmployeeATID', param);
    }

    public GetAllCustomer(): Promise<any> {
        return this.get('Get_HR_CustomerInfos');
    }

    public GetNewestActiveCustomerInfo(): Promise<any> {
        return this.get('GetNewestActiveCustomerInfo');
    }

    public GetNewestCustomerInfo(): Promise<any> {
        return this.get('GetNewestCustomerInfo');
    }

    public GetCustomerInfoExcludeExpired(): Promise<any> {
        return this.get('GetCustomerInfoExcludeExpired');
    }

    public GetCustomerAndContractorInfo(): Promise<any> {
        return this.get('GetCustomerAndContractorInfo');
    }

    public GetCustomerById(id: string): Promise<any> {
        return this.get('GetCustomerById', { params: { id } });
    }

    public AddCustomer(data: any, employeeType: number): Promise<any> {
        data.EmployeeType = employeeType;
        return this.post('Post_HR_CustomerInfo', data);
    }

    public UpdateCustomer(employeeATID: string, data: any, employeeType: number): Promise<any> {
        data.EmployeeType = employeeType;
        return this.put(`Put_HR_CustomerInfo/${employeeATID}`, data);
    }

    public UpdateCustomerCardNumber(employeeATID: string, cardNumber: string): Promise<any> {
        const data = {
            EmployeeATID: employeeATID,
            CardNumber: cardNumber
        };
        return this.post('UpdateCustomerCardNumber', data);
    }

    public ReturnOrDeleteCard(cardNumber: string) {
        return this.post("ReturnOrDeleteCard?cardNumber=" + cardNumber)
    }

    public DeleteCustomerMulti(e: Array<string>): Promise<any> {
        return this.delete('DeleteCustomerMulti', { data: e });
    }

    public AddCustomerFromExcel(arrEmployee, employeeType: number) {
        return this.post("AddCustomerFromExcel?userType=" + employeeType.toString(), arrEmployee)
    }

    public DeleteCustomerFromExcel(addedParams: Array<AddedParam>, userType: number) {
        return this.post("DeleteCustomerFromExcel?userType=" + userType.toString(), addedParams)
    }

    public InfoCustomerTemplateImport(){
        return this.get('InfoCustomerTemplateImport');
    }
    public InfoContractorTemplateImport(){
        return this.get('InfoContractorTemplateImport');
    }

    public ImportDataFromGoogleSheet(): Promise<any> {
        return this.get('ImportDataFromGoogleSheet');
    }
}

export interface AddedParam {
    Key: string;
    Value: any;

}

export const hrCustomerInfoApi = new HR_CustomerInfoApi("HR_CustomerInfo");
