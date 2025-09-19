import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class HR_StudentInfoApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetStudentAtPage(filter: string, c: Array<string>, page: number, pageSize: number): Promise<any> {
        const params = new URLSearchParams();
        c.forEach(e => {
            params.append('c', e);
        });

        params.append('filter', filter.toString());
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());

        return this.get('GetStudentAtPage', { params: params })
    }

    public GetAllStudent(): Promise<any> {
        return this.get('Get_HR_StudentInfos');
    }

    public AddStudent(data: any): Promise<any> {
        return this.post('Post_HR_StudentInfo', data);
    }

    public UpdateStudent(employeeATID: string, data: any): Promise<any> {
        return this.put(`Put_HR_StudentInfo/${employeeATID}`, data);
    }

    public DeleteStudentMulti(e: Array<string>): Promise<any> {
        return this.delete('DeleteStudentMulti', { data: e });
    }

    public AddStudentFromExcel(arrEmployee): Promise<any> {
        return this.post("AddStudentFromExcel", arrEmployee)
    }

    public ExportToExcel(addedParams: Array<AddedParam>) {
        return this.post("ExportToExcel", addedParams);
    }
}
export interface AddedParam {
    Key: string;
    Value: any;

}
export const hrStudentInfoApi = new HR_StudentInfoApi("HR_StudentInfo");
