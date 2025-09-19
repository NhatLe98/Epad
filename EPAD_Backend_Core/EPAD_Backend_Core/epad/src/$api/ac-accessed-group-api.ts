import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class AC_AccessedGroupApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAccessedGroupAtPage(page: number, filter: string, limit: number, departments: Array<number>, groups: Array<number> ) {
        return this.post("GetAccessedGroupAtPage", {page, filter, limit,departments,groups});
    }

    public AddEmployeeAccessedGroup(data: any) {
		return this.post('AddEmployeeAccessedGroup', data);
	}
	public UpdateEmployeeAccessedGroup(data: any) {
		return this.put('UpdateEmployeeAccessedGroup', data);
	}
	public async DeleteEmployeeAccessedGroup(arrEmployeeAccessedGroup: Array<any> | Array<number>): Promise<any> {
		return this.delete('DeleteEmployeeAccessedGroup', { data: arrEmployeeAccessedGroup });
	}
	
  
}

export const ac_AccessedGroupApi = new AC_AccessedGroupApi("AC_AccessedGroup");