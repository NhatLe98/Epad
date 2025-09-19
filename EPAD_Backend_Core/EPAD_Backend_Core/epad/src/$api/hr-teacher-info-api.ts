import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class HR_TeacherInfoApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetTeacherAtPage( page: number, pageSize: number): Promise<any> {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());


    return this.get('GetTeacherAtPage', { params: params })
}
  public GetTeacher(employeeATID: string): Promise<any> {
    return this.get('Get_HR_TeacherInfo/' + employeeATID);
  }
}

export const hrTeacherInfoApi = new HR_TeacherInfoApi("HR_TeacherInfo");
