import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';
import { EzFile } from "./ez-portal-file-api";

class RulesWarningApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetRulesWarningGroup() {
		return this.get('GetRulesWarningGroup');
	}
	public GetRulesWarningByCompanyIndex() {
		return this.get('GetRulesWarningByCompanyIndex');
    }
	public GetEmailScheduleByRuleWarningIndex(rulesWarningIndex: number) {
		return this.get('GetEmailScheduleByRuleWarningIndex', { params: { rulesWarningIndex } });
    }
	public GetControllerChannelByRuleWarningIndex(rulesWarningIndex: number) {
		return this.get('GetControllerChannelByRuleWarningIndex', { params: { rulesWarningIndex } });
    }
	
	public AddRulesWarning(data: WarningRulesModel) {
		return this.post('AddRulesWarning', data);
	}
	public UpdateRulesWarning(data: WarningRulesModel) {
		return this.put('UpdateRulesWarning', data);
	}
	public DeleteRulesWarning(index: number) {
		return this.delete('DeleteRulesWarning', { params: { index: index } });
	}

	public AddRulesWarningEmailSchedule(data: Array<EmailScheduleRequestModel>) {
		console.log(data)
		return this.post('AddRulesWarningEmailSchedule', data);
	}
	public AddRulesWarningControllerChannels(data: Array<ControllerWarningRequestModel>) {
		return this.post('AddRulesWarningControllerChannels', data);
	}
	public AddEzFileRulesWarning(data: EzFileRequest) {
		return this.post('AddEzFileRulesWarning', data);
	}
	public SendReloadWarningRulesSignal() {
		return this.get('SendReloadWarningRulesSignal');
	}
	public CheckAndSendMail(email: string, data: any) {
		const param: SendEmailByTimeLog = {
			Email: email,
			TimeLog: data
		}
		return this.post('CheckAndSendMail', param);
	}
}

export interface WarningRulesModel {
	Index: number;

    UseSpeaker: boolean;
    SpeakerController: number;
    SpeakerChannel: number;
    SpeakerDescription: string;


    UseLed: boolean;
    LedController: number;
    LedChannel: number;
    LedDescription: string;


    UseEmail: boolean;
    Email: string;
    EmailSendType: number;
	// EmailSchedule: Array<any>;

    UseComputerSound: boolean;
    ComputerSoundPath: string;


    UseChangeColor: boolean;

	RulesWarningGroupIndex: number;
}
export interface EmailScheduleModel {
	Index: number;
	Time: Date;
	DayOfWeekIndex: number;
	Order: number;
	Error: string;
}
export interface ControllerWarningModel {
	Index: number;
	ControllerIndex: number;
	ChannelIndex: number;
	Order: number;
	Error: string;
	LineIndex: number;
	GateIndex: number;
	SerialNumber: string;
	Type: number;
}


export interface EmailScheduleRequestModel {
	Index: number;
	Time: Date;
	DayOfWeekIndex: number;
	RulesWarningIndex: number;
	CompanyIndex: number;
}
export interface ControllerWarningRequestModel {
	Index: number;
	ControllerIndex: number;
	ChannelIndex: number;
	RulesWarningIndex: number;
	CompanyIndex: number;
	LineIndex?: number;
	GateIndex?: number;
	SerialNumber: string;
	Type: number;
}
export interface EzFileRequest{
    Index: number;
    Attachments: Array<EzFile>;
}
export interface SendEmailByTimeLog{
    Email: string;
    TimeLog: any;
}
export const rulesWarningApi = new RulesWarningApi('GC_Rules_Warning');
