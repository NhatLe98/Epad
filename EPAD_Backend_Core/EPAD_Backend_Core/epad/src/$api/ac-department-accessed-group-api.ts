import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class AC_DepartmentAccessedGroupApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetDepartmentAccessedGroupAtPage(page: number, filter: string, limit: number, departments: Array<number>, groups: Array<number> ) {
        return this.post("GetDepartmentAccessedGroupAtPage", {page, filter, limit,departments,groups});
    }

    public AddDepartmentAccessedGroup(data: any) {
		return this.post('AddDepartmentAccessedGroup', data);
	}
	public UpdateDepartmentAccessedGroup(data: any) {
		return this.put('UpdateDepartmentAccessedGroup', data);
	}
	public async DeleteDepartmentAccessedGroup(arrEmployeeAccessedGroup: Array<any> | Array<number>): Promise<any> {
		return this.delete('DeleteDepartmentAccessedGroup', { data: arrEmployeeAccessedGroup });
	}
	
  
}

export const ac_DepartmentAccessedGroupApi = new AC_DepartmentAccessedGroupApi("AC_DepartmentAccessedGroup");