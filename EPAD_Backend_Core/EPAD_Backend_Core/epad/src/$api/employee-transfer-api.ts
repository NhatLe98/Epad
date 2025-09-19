import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class EmployeeTransferApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public EmployeeTransfer(
        page: number,
        filter: string,
        fromDate: string,
        toDate: string,
        isPenddingApprove: boolean,
        limit
    ): Promise<BaseResponse> {
        return this.get("GetEmployeeTransferAtPage", { params: { page, filter, fromDate, toDate, isPenddingApprove, limit } });
    }

    public GetWaitingApproveList() {
        return this.get("GetWaitingApproveList");
    }

    public AddEmployeeTransfer(employeeTransfer: IC_EmployeeTransfer) {
        return this.post("AddEmployeeTransfer", employeeTransfer);
    }

    public AddEmployeesTransfer(employeeTransfer: IC_EmployeeTransfer, ArrEmployeeATID: Array<string>) {
        var employeesTransfer = Object.assign(employeeTransfer, { ArrEmployeeATID });
        return this.post("AddEmployeesTransfer", employeesTransfer);
    }

    public AddEmployeeTransferFromExcel(arrEmployeeTransfer: Array<IC_EmployeeTransfer>) {
        return this.post("AddEmployeeTransferFromExcel", arrEmployeeTransfer);
    }

    public AddEmployeesTransferFromExcel(arrEmployeeTransfer: Array<IC_EmployeeTransfer>) {
        return this.post("AddEmployeesTransferFromExcel", arrEmployeeTransfer);
    }

    public ApproveOrRejectEmployeeTransfer(employeeTransfer: Array<IC_EmployeeTransfer>) {
        return this.post("ApproveOrRejectEmployeeTransfer", employeeTransfer);
    }
    public UpdateEmployeeTransfer(employeeTransfer: IC_EmployeeTransfer) {
        return this.post("UpdateEmployeeTransfer", employeeTransfer);
    }
    public UpdateEmployeeTransferNew(employeeTransfers: Array<IC_EmployeeTransfer>) {
        return this.post("UpdateEmployeeTransferNew", employeeTransfers);
    }

    public DeleteEmployeeTransfer(employeeTransfer: IC_EmployeeTransfer) {
        return this.post("DeleteEmployeeTransfer", employeeTransfer);
    }

    public DeleteApproveEvent(listParam: Array<WaitingApproveResult>) {
        return this.post("DeleteApproveEvent", listParam);
    }

    public ExportEmployeeTransfer(addedParam: Array<AddedParam>) {
        return this.post("ExportEmployeeTransfer", addedParam, {responseType: 'blob'});
    }

}
export interface IC_EmployeeTransfer {
    WokingInfoIndex?: number;
    EmployeeATID?: string;
    NewDepartment?: number;
    FromTime?: Date;
    ToTime?: Date;
    IsFromTime?: string;
    IsToTime?: string;
    OldDepartment?: number;
    RemoveFromOldDepartment?: boolean;
    AddOnNewDepartment?: boolean;
    IsSync?: boolean;
    Description?: string;
    TemporaryTransfer?: boolean;
    Status?: number;
    TransferNow?: boolean

}
export interface WaitingApproveResult {
    Index: number;
    EmployeeATID: string;
    CompanyIndex: number;
    FromDate: Date | string;
    NewDepartmentIndex: number;
    Type: number
    EmployeeName: string;
    NewDepartment: string;
    OldDepartment: string;
    ToDate: Date | string | null;
    SuggestUser: string;
    SuggestDate: Date | string | null;
    IsChecked: boolean;
    Status?: number;
}
export interface AddedParam {
    Key: string,
    Value: any
}
export interface ExportEmployeeTransferRequest {
    fromDate: string,
    toDate: string,
}

export const employeeTransferApi = new EmployeeTransferApi("EmployeeTransfer");
