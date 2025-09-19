import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class ExcusedAbsentApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetExcusedAbsentAtPage(requestParam: ExcusedAbsentRequest) {
        return this.post("GetExcusedAbsentAtPage", requestParam);
    }

    public GetExcusedAbsentReason() {
        return this.get("GetExcusedAbsentReason");
    }

    public ExportTemplateExcusedAbsentReason() {
        return this.get("ExportTemplateExcusedAbsentReason");
    }

    public AddExcusedAbsent(department: HR_ExcusedAbsent) {
        return this.post("AddExcusedAbsent", department);
    }

    public UpdateExcusedAbsent(department: HR_ExcusedAbsent) {
        return this.post("UpdateExcusedAbsent", department);
    }

    public DeleteExcusedAbsent(department: Array<number>) {
        return this.post("DeleteExcusedAbsent", department);
    }

    public AddExcusedAbsentFromExcel(arrDepartment) {
        return this.post("AddExcusedAbsentFromExcel", arrDepartment)
    }
}

export interface HR_ExcusedAbsent {
    Index: number;
    EmployeeATID?: string;
    EmployeeATIDs: Array<string>;
    AbsentDate?: Date;
    AbsentDateString?: string;
    ExcusedAbsentReasonIndex: number;
    Description?: string;
}

export interface ExcusedAbsentRequest {
    page: number;
    limit: number;
    filter: string;
    from: string;
    to: string;
    departments: Array<number>;
}

export const excusedAbsentApi = new ExcusedAbsentApi("HR_ExcusedAbsent");
