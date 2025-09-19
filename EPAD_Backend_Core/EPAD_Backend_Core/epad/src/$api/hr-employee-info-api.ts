import { log } from 'util';
import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class HR_EmployeeInfoApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetEmployeesAtPage(filter: string, departments: Array<number>, page: number, pageSize: number,
         listWorkingStatus: Array<number>, fromDate?: Date, toDate?: Date): Promise<any> {
        const employeeInfoRequest: HR_EmployeeInfoRequest = {
            Filter: filter,
            DepartmentIDs: departments,
            ListWorkingStatus: listWorkingStatus,
            Page: page,
            PageSize: pageSize,
            UserType: null,
            FromDate: fromDate,
            ToDate: toDate
        };

        return this.post('GetEmployeesAtPage', employeeInfoRequest)
    }

    public GetEmployeeAtPage(filter: string, departments: Array<number>, page: number, pageSize: number, listWorkingStatus: Array<number>): Promise<any> {
        const params = new URLSearchParams();
        params.append('filter', filter.toString());
        departments.forEach(e => {
            params.append('d', e.toString());
        });

        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        listWorkingStatus.forEach(e => {
            params.append('listWorkingStatus', e.toString());
        });
        //params.append('userType', userType.toString());

        return this.get('GetEmployeeAtPage', { params: params })
    }

    public GetAllEmployee(): Promise<any> {
        return this.get('Get_HR_EmployeeInfos');
    }

    public GetEmployeeInfoByUserDepartment(): Promise<any> {
        return this.get('GetEmployeeInfoByUserDepartment');
    }

    public GetEmployeeInfoByDepartment(departmentIndex: number): Promise<any> {
        return this.get('GetEmployeeInfoByDepartment',{ params: departmentIndex });
    }

    public GetEmployeeInfoByUserRootDepartment(): Promise<any> {
        return this.get('GetEmployeeInfoByUserRootDepartment');
    }

    public GetEmployeeInfoByRootDepartment(departmentIndex: number): Promise<any> {
        return this.get('GetEmployeeInfoByRootDepartment',{ params: departmentIndex });
    }

    public AddEmployee(data: any): Promise<any> {
        const params = {
            ...data,
            //'EmployeeType': tabIndex,
        }

        return this.post('Post_HR_EmployeeInfo', params);
    }

    public UpdateEmployee(employeeATID: string, data: any): Promise<any> {
        if( Misc.isEmpty(data.PositionIndex) ) {
            data.PositionIndex = null;
        }

        const params = {
            ...data,
            //'EmployeeType': tabIndex,
        }
        return this.put(`Put_HR_EmployeeInfo/${employeeATID}`, params);
    }

    public DeleteEmployeeMulti(e: Array<string>): Promise<any> {
        return this.delete('DeleteEmployeeMulti', { data: e });
    }

    public GetUserContactInfoById(id: number): Promise<any> {
        console.log(id);
        return this.get('GetUserContactInfoById', { params: { id } });
    }
}

export interface HR_EmployeeInfoRequest {
    Filter: string;
    DepartmentIDs: Array<number>;
    ListWorkingStatus: Array<number>;
    UserType?: number;
    Page: number;
    PageSize: number;
    FromDate?: Date;
    ToDate?: Date;
}

export const hrEmployeeInfoApi = new HR_EmployeeInfoApi("HR_EmployeeInfo");
