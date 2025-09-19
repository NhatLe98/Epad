import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class RuleInOutTimeApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetAllRules() {
    return this.get('GetAllRules');
  }
  public AddRuleInOutTime(data: RulesInOutTimeParam) {
    return this.post('AddRuleInOutTime', data);
  }
  public UpdateRuleInOutTime(data: RulesInOutTimeParam) {
    return this.post('UpdateRuleInOutTime', data);
  }
  public DeleteRuleInOutTime(param: number) {
    return this.delete(`DeleteRuleInOutTime/${param}`);
  }

}

export interface RulesInOutTimeParam {
  Index: number;
  FromDate: any;
  FromDateString: string;
  Description: string;
  /*  các qui định về thời gian vào ra cho phép  */
  CheckInTime?: string | Date;
  MaxEarlyCheckInMinute: number;
  MaxLateCheckInMinute: number;
  CheckOutTime?: Date | string;
  MaxEarlyCheckOutMinute: number;
  MaxLateCheckOutMinute: number;
}

export const ruleInOutTimeApi = new RuleInOutTimeApi('HR_Rules_InOutTime');

