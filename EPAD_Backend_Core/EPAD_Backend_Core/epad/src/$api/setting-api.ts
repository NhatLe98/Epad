import { AxiosRequestConfig } from 'axios'
import { BaseApi, BaseResponse } from '@/$core/base-api'

class SettingApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config)
	}

	public GetPersonalAccessToken(filter,limit) {
		return this.get('GetPersonalAccessToken', { params: { q: filter,limit } })
	}
	public AddPersonalAccessToken(data: AddPersonalAccessTokenModel) {
		return this.post('AddPersonalAccessToken', data)
	}
	public RevokeAccessToken(ID: string) {
		return this.post('RevokeAccessToken', { ID })
	}
}
export interface AddPersonalAccessTokenModel {
	Name: string
	Scopes: string
	Note: string
	ExpiredDate: Date
}
export interface PersonalAccessTokenModel {
	Index: string
	CompanyIndex: string
	Name: string
	ExpiredDate: Date
	CreatedDate: Date
	Scope: string
	Note: string
}

export const settingApi = new SettingApi('Setting')
