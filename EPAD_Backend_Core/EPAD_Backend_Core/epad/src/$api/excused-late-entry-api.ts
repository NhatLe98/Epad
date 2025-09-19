import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class ExcusedLateEntryApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetExcusedLateEntryAtPage(requestParam: ExcusedLateEntryRequest) {
        return this.post("GetExcusedLateEntryAtPage", requestParam);
    }

    public AddExcusedLateEntry(department: HR_ExcusedLateEntry) {
        return this.post("AddExcusedLateEntry", department);
    }

    public UpdateExcusedLateEntry(department: HR_ExcusedLateEntry) {
        return this.post("UpdateExcusedLateEntry", department);
    }

    public DeleteExcusedLateEntry(department: Array<number>) {
        return this.post("DeleteExcusedLateEntry", department);
    }

    public AddExcusedLateEntryFromExcel(arrDepartment) {
        return this.post("AddExcusedLateEntryFromExcel", arrDepartment)
    }
}

export interface HR_ExcusedLateEntry {
    Index: number;
    EmployeeATID?: string;
    EmployeeATIDs: Array<string>;
    LateDate?: Date;
    LateDateString?: string;
    Description?: string;
}

export interface ExcusedLateEntryRequest {
    page: number;
    limit: number;
    filter: string;
    from: string;
    to: string;
    departments: Array<number>;
}

export const excusedLateEntryApi = new ExcusedLateEntryApi("HR_ExcusedLateEntry");
