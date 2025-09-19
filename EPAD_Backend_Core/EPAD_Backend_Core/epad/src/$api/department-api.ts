import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class DepartmentApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetDepartmentAtPage(page: number, filter, limit) {
        return this.get("GetDepartmentAtPage", { params: { page, filter,limit } });
    }
    public GetDepartment() {
        return this.get("GetDepartment");
    }
    public GetAll() {
        return this.get("GetAll");
    }

    public GetAllDoor() {
        return this.get("GetAllDoor");
    }
    
    public GetAllTimeZone() {
        return this.get("GetAllTimeZone");
    }

    public GetAllGroup() {
        return this.get("GetAllGroup");
    }

    public GetDepartmentTree() {
        return this.get("GetDepartmentTree");
    }

    public GetDepartmentTreeEmployeeScreen(userType? : string) {
        if(userType){
            return this.get("GetDepartmentTreeEmployeeScreen",  { params: { userType } });
        }else{
            return this.get("GetDepartmentTreeEmployeeScreen");
        }
        
    }

    public GetActiveDepartmentByPermission() {
        return this.get("GetActiveDepartmentByPermission");
    }

    public GetActiveDepartmentAndDeviceByPermission() {
        return this.get("GetActiveDepartmentAndDeviceByPermission");
    }

    public GetDevicesInOutDepartment(departmentIndex: number) {
        return this.get("GetDevicesInOutDepartment", {
            params: { departmentIndex }
        });
    }
    public AddDepartment(department: IC_Department) {
        return this.post("AddDepartment", department);
    }

    public UpdateDepartment(department: IC_Department) {
        return this.post("UpdateDepartment", department);
    }

    public DeleteDepartment(department: Array<IC_Department>) {
        return this.post("DeleteDepartment", department);
    }

    public AddDepartmentFromExcel(arrDepartment) {
        return this.post("AddDepartmentFromExcel", arrDepartment)
    }
}

class DepartmentAndDeviceApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetDepartmentAndDeviceAtPage(page: number, filter) {
        return this.get("GetDepartmentAndDeviceAtPage", { params: { page, filter } });
    }

    public UpdateDepartmentAndDevice(department: IC_DepartmentAndDevice) {
        return this.post("UpdateDepartmentAndDevice", department);
    }

    public GetDepartmentAndDeviceLookup() {
        return this.get("GetDepartmentAndDeviceLookup");
    }
}

export interface IC_DepartmentAndDevice {
    DepartmentIndex: number;
    ListDeviceSerial: Array<string>;
}


export interface IC_Department {
    Index?:number
    Name?: string;
    Location?: string;
    Description?: string;
    Code?: string;
    ParentIndex?: number;
    IsContractorDepartment?: boolean;
    IsDriverDepartment?: boolean;
}
export interface IC_DepartmentAndDevice {
    DepartmentIndex: number;
    ListDeviceSerial: Array<string>;
}


export const departmentApi = new DepartmentApi("Department");
export const departmentAndDeviceApi = new DepartmentAndDeviceApi("DepartmentAndDevice");
