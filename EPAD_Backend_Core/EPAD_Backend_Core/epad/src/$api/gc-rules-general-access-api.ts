import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class RuleGeneralAccessApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetAllRules() {
    return this.get('GetAllRules');
  }
  public AddRuleGeneral(data: RulesGeneralAccessParam) {
    return this.post('AddRuleGeneral', data);
  }
  public UpdateRuleGeneral(data: RulesGeneralAccessParam) {
    return this.put('UpdateRuleGeneral', data);
  }
  public DeleteRuleGeneral(param: number) {
    return this.delete(`DeleteRuleGeneral/${param}`);
  }

}

export interface RulesGeneralAccessParam {
  Index: number;
  Name: string;
  NameInEng: string;
  /*  các qui định về thời gian vào ra cho phép  */
  CheckInByShift: boolean;
  CheckInTime?: string | Date;
  MaxEarlyCheckInMinute: number;
  MaxLateCheckInMinute: number;
  CheckOutByShift: boolean;
  CheckOutTime?: Date | string;
  MaxEarlyCheckOutMinute: number;
  MaxLateCheckOutMinute: number;
  AllowFreeInAndOutInTimeRange: boolean;
  AllowEarlyOutLateInMission: boolean;
  MissionMaxEarlyCheckOutMinute: number;
  MissionMaxLateCheckInMinute: number;
  AdjustByLateInEarlyOut: boolean;
  BeginLastHaftTime?: Date | string;
  EndFirstHaftTime?: Date | string;
  /*  các qui định liên quan đến ra giữa giờ có đăng ký  */
  AllowInLeaveDay: boolean;
  AllowInMission: boolean;
  AllowInBreakTime: boolean;
  /*  các qui định liên quan đến ra giữa giờ không đăng ký */
  AllowCheckOutInWorkingTime: boolean;
  AllowCheckOutInWorkingTimeRange: string;
  MaxMinuteAllowOutsideInWorkingTime: number;
  /*  các qui định cấm vào ra  */
  DenyInLeaveWholeDay: boolean;
  DenyInMissionWholeDay: boolean;
  DenyInStoppedWorkingInfo: boolean;

  CheckLogByAreaGroup: boolean;
  AreaGroups: Array<RuleAreaGroup>;
  ListGatesInfo: Array<EmployeeRulesGate>;
}
export interface RuleAreaGroup {
  AreaGroupIndex: number;
  Rules_GeneralIndex: number;
  Priority: number;
}
export interface EmployeeRulesGate{
  RulesGeneralIndex: number;
  GateIndex: number;
  LineIndexs: string;
}

export const ruleGeneralAccessApi = new RuleGeneralAccessApi('GC_Rules_GeneralAccess');

