import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class EmailDeclareGuestApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetEmailDeclareGuestAtPage(page: number, filter, limit) {
        return this.get("GetEmailDeclareGuestAtPage", { params: { page, filter,limit } });
    }

    public AddEmailDeclareGuest(EmailDeclareGuest: HR_EmailDeclareGuest) {
        return this.post("AddEmailDeclareGuest", EmailDeclareGuest);
    }

    public UpdateEmailDeclareGuest(EmailDeclareGuest: HR_EmailDeclareGuest) {
        return this.post("UpdateEmailDeclareGuest", EmailDeclareGuest);
    }

    public DeleteEmailDeclareGuest(EmailDeclareGuest: Array<HR_EmailDeclareGuest>) {
        return this.post("DeleteEmailDeclareGuest", EmailDeclareGuest);
    }

    public AddEmailDeclareGuestFromExcel(arrEmailDeclareGuest) {
        return this.post("AddEmailDeclareGuestFromExcel", arrEmailDeclareGuest)
    }
}

export interface HR_EmailDeclareGuest {
    Index?:number
    EmployeeATID: string;
    EmailAddressList: Array<string>;
    Description?: string;
}

export const emailDeclareGuestApi = new EmailDeclareGuestApi("EmailDeclareGuest");
