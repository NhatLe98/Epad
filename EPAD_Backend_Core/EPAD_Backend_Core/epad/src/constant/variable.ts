//***api
import { API_HOST } from '@/$core/config';

export const domain = API_HOST;

export const rootLink = API_HOST;

export const apilink_ResetPassword =
  domain + "/api/UserAccount/SendResetPasswordCode?pUsername=";
export const apilink_Login = domain + "/api/login";

export const apilink_DelDepartment =
  domain + "/api/Department/DeleteDepartment";
export const apilink_SelDepartment = domain + "/api/Department/GetDepartment";
export const apilink_AddDepartment = domain + "/api/Department/AddDepartment";
export const apilink_GetDepartment =
  domain + "/api/Department/GetDepartmentAtPage";
export const apilink_UpdateDepartment =
  domain + "/api/Department/UpdateDepartment";

export const apilink_EmployeeInfoAll =
  domain + "/api/EmployeeInfo/GetEmployeeAtPage";
export const apilink_AddEmployee = domain + "/api/EmployeeInfo/AddEmployee";
export const apilink_UpdateEmployee =
  domain + "/api/EmployeeInfo/UpdateEmployee";
export const apilink_DeleteEmployee =
  domain + "/api/EmployeeInfo/DeleteEmployee";
export const apilink_TreeAsEmployee =
  domain + "/api/EmployeeInfo/GetEmployeeAsTree";

export const apilink_GetDeviceAll = domain + "/api/Device/GetDeviceAll";
export const apilink_GetDeviceAtPage = domain + "/api/Device/GetDeviceAtPage";
export const apilink_AddDevice = domain + "/api/Device/AddDevice";
export const apilink_UpdateDevice = domain + "/api/Device/UpdateDevice";
export const apilink_DeleteDevice = domain + "/api/Device/DeleteDevice";

export const apilink_GetUserAccount =
  domain + "/api/UserAccount/GetUserAccountAtPage";
export const apilink_AddUserAccount =
  domain + "/api/UserAccount/AddUserAccount";
export const apilink_GetUserPrivilegeAtPage =
  domain + "/api/UserPrivilege/GetUserPrivilegeAtPage";
export const apilink_GetEmployeeTranferAtPage =
  domain + "/api/EmployeeTransfer/GetEmployeeTranferAtPage";
export const apilink_AddEmployeeTranfer =
  domain + "/api/EmployeeTransfer/AddEmployeeTranfer";
export const apilink_DeleteEmployeeTranfer =
  domain + "/api/EmployeeTransfer/DeleteEmployeeTranfer";
export const apilink_UpdateEmployeeTranfer =
  domain + "/api/EmployeeTransfer/UpdateEmployeeTranfer";

export const apilink_GetUserAccountInfo =
  domain + "/api/UserAccount/GetUserAccountInfo";

export const apilink_ChangePass =
  domain + "/api/UserAccount/ChangePass";
export const apilink_AddUserPrivilege =
  domain + "/api/UserPrivilege/AddUserPrivilege";

export const apilink_UpdateUserPrivilege =
  domain + "/api/UserPrivilege/UpdateUserPrivilege";

export const apilink_DelUserPrivilege =
  domain + "/api/UserPrivilege/DeleteUserPrivilege";
export const apilink_GetUserPrivilege =
  domain + "/api/UserPrivilege/GetUserPrivilege";
export const apilink_DeleteUserAccount =
  domain + "/api/UserAccount/DeleteUserAccount";
export const apilink_UpdateUserAccount =
  domain + "/api/UserAccount/UpdateUserAccount";

  export const apilink_GetDeviceInfo =
  domain + "/api/SystemCommand/GetDeviceInfo";
  export const apilink_GetUserMachineInfo =
    domain + "/api/UserInfo/GetUserMachineInfo";


