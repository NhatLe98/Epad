import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class UserNotificationApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }
    public UserNotification(
        page: number,
        filter: string,
        fromDate: string,
        toDate: string
    ): Promise<BaseResponse> {
        return this.get("GetPage", { params: { page, filter, fromDate, toDate } });
    }
    public PostMany(addedParams: Array<AddParam>) {
        return this.post("PostMany", addedParams);
    }

    public Delete(notify: UserNotification) {
        return this.post("Delete", notify);
    }
    public DeleteAll(listParam: Array<UserNotification>) {
        return this.post("DeleteAll", listParam);
    }
}
export interface AddParam {
    Key: string;
    Value: any;
}

export interface UserNotification {
    Index: number;
    UserName: string | null;
    Data: MessageBody | null;
    Status: number;
    Type: number;
    RouteURL: string | null;
    IsChecked: boolean;
}

export interface MessageBody {
    FromUser: string | null;
    Message: string | null;
    FromDate: Date | string | null;
    ToDate: Date | string | null;
    ApproveDate: Date | string | null;
    Approver: string | null;
    FormatMessage: string | null;
}
export const userNotificationApi = new UserNotificationApi("UserNotification");