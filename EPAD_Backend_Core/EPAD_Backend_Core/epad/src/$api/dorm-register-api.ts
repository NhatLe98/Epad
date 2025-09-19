import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class DormRegisterApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetDormRegisterAtPage(requestParam: DormRegisterRequest) {
        return this.post("GetDormRegisterAtPage", requestParam);
    }

    public GetDormActivity() {
        return this.get("GetDormActivity");
    }

    public GetDormRation() {
        return this.get("GetDormRation");
    }

    public GetDormLeaveType() {
        return this.get("GetDormLeaveType");
    }

    public GetDormAccessMode() {
        return this.get("GetDormAccessMode");
    }

    // public ExportTemplateExcusedAbsentReason() {
    //     return this.get("ExportTemplateExcusedAbsentReason");
    // }

    public AddDormRegister(department: DormRegisterViewModel) {
        return this.post("AddDormRegister", department);
    }

    public UpdateDormRegister(department: DormRegisterViewModel) {
        return this.post("UpdateDormRegister", department);
    }

    public DeleteDormRegister(department: Array<number>) {
        return this.post("DeleteDormRegister", department);
    }

    public AddDormRegisterFromExcel(arrDepartment) {
        return this.post("AddDormRegisterFromExcel", arrDepartment)
    }
}

export interface DormRegisterViewModel {
    Index: number;
    EmployeeATID: string;
    EmployeeATIDs: Array<string>;
    FullName?: string;
    FromDate: Date;
    FromDateString?: string;
    ToDate: Date;
    ToDateString?: string;
    RegisterDate: Date;
    RegisterDateString?: string;
    StayInDorm: boolean;
    DormRoomIndex: number;
    DormRoomName?: string;
    DepartmentIndex: number;
    DepartmentName?: string;
    DormEmployeeCode: string;
    DormLeaveIndex: number;
    DormLeaveName?: string;
    DormRegisterRation: Array<DormRegisterRationViewModel>;
    DormRegisterRationName?: string;
    DormRegisterActivity: Array<DormRegisterActivityViewModel>;
    DormRegisterActivityName?: string;
}

export interface DormRegisterRationViewModel {
    DormRegisterIndex: number;
    DormRationIndex: number;
    DormRationName: string;
}

export interface DormRegisterActivityViewModel {
    DormRegisterIndex: number;
    DormActivityIndex: number;
    DormActivityName: string;
    DormAccessMode: number;
}

export interface FormDormActivity {
    DormActivityIndex: number;
    DormAccessMode: Array<number>;
    Error: string;
}

export interface DormRegisterRequest {
    page: number;
    limit: number;
    filter: string;
    from: string;
    to: string;
    departments: Array<number>;
    dormRooms: Array<number>;
}

export const dormRegisterApi = new DormRegisterApi("HR_DormRegister");
