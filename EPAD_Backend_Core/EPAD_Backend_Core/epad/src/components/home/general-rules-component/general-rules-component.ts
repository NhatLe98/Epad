import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";

import axios from "axios";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { gatesLinesApi, GatesLines } from '@/$api/gc-gates-lines-api';

import { isNullOrUndefined } from 'util';
import { GeneralRulesModel, RulesGeneralLogRequestModel, rulesGeneralApi } from "@/$api/gc-rules-general-api";
import { areaGroupApi } from "@/$api/gc-area-group-api";

@Component({
	name: 'general-rules',
	components: { HeaderComponent, DataTableComponent }
})

export default class GeneralRules extends Mixins(ComponentBase) {
	roleByRoute: any;
	listColumn = [];
	// rowsObj = [];
	listData = [];
	areaGroupList = [];
	currentRuleLogs = Array<RulesGeneralLogRequestModel>();
	rules = null;
	ruleGeneral: GeneralRulesModel = {
		Index: 0,
		Name: "",
		NameInEng: "",
		FromDate: null,
		ToDate: null,
		StartTimeDay:"",
		MaxAttendanceTime:0,
		IsUsing: false,
		PresenceTrackingTime: 0,
	};
	activeMenuIndex = 0;
	currentGroup = null;
	currentGroupName = '';
	isAdd = true;
	formError = "";
	groupError = "";
	errorSound = "";
	errorSpeaker = "";
	errorLed = "";
	defaultValue: Date = null;
	AreaGroupsEmpty = [];
	TimePickerEmpty = [];
    
	async beforeMount() {
		
		await this.getListData();
		await this.getAllAreaGroup();
		// this.createColumnHeader();
		this.initForm();
		this.initRule();
	}
	async mounted() {
	const access_token = localStorage.getItem('access_token');
		if(access_token){
			if (!this.roleByRoute  || (this.roleByRoute && !this.roleByRoute.length)) {
				// store.dispatch(ActionTypes.LOAD_ROLE_WITH_ROUTE,this.$route.meta.formName);
			}
		}
	}

	initForm() {
		this.ruleGeneral = {
			Index: 0,
			Name: "",
			NameInEng: "",
			FromDate: null,
			ToDate: null,
			StartTimeDay:"",
			MaxAttendanceTime:0,
			IsUsing: false,
			PresenceTrackingTime: 0,
		};
		this.initRulesLog();
		// this.currentRuleLogs = [];
		this.AreaGroupsEmpty[0] = true;
		this.TimePickerEmpty[0] = {
			FromEarlyDate: false,
			ToDate: false,
			FromDate: false,
			ToLateDate: false
		};
	}
	initRulesLog() {
		this.currentRuleLogs = [
			{
				Index: 0,
				FromEarlyDate: new Date(2021, 1, 1, 7, 30),
				FromDate: new Date(2021, 1, 1, 8, 0),
				ToLateDate: new Date(2021, 1, 1, 17, 0),
				ToDate: new Date(2021, 1, 1, 17, 30),
				UseDeviceMode: false,
				UseMinimumLog: false,
				UseSequenceLog: false,
				UseTimeLog: false,
				UseMode: 0,
				FromIsNextDay: false,
				MinimumLog: 1,
				ToIsNextDay: false,
				ToLateIsNextDay: false,
				RuleGeneralIndex: 0,
				AreaGroupIndex: []
			}
		];
	}
	initRule() {
		this.rules = {
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputName'),
					trigger: 'change',
				},
			],
			FromDate: [
				{
					required: true,
					message: this.$t('PleaseInputFromDate'),
					trigger: 'change',
				}
			],
			ToDate: [
				{
					message: this.$t('FromDateCannotLargerToDate'),
					validator: (rule, value: string, callback) => {
						if (this.ruleGeneral.FromDate 
							&& this.ruleGeneral.ToDate 
							&& this.ruleGeneral.FromDate > this.ruleGeneral.ToDate) {
							callback(new Error());
						} else {
							callback();
						}
					},
				}
			],
		};
	}


	checkClass(index) {
		if (this.currentGroup == index) {
			return 'is-active';
		} else {
			return 'un-active';
		}
	}
	changeFormData(index) {
		this.initForm();
		this.listData.forEach(element => {
			if (element.Index == index.Index) {
				this.getListRulesLog(element.Index);
				this.isAdd = false;
				this.activeMenuIndex = element.Index;
				this.currentGroupName = element.GroupName;
				this.currentGroup = index;
				this.ruleGeneral = element;
			}
		});
	}
	async getListRulesLog(rulesGeneralIndex) {
		if (rulesGeneralIndex == 0) {
			this.initRulesLog()
		} else {
			await rulesGeneralApi.GetRuleGeneralLogByRuleGeneralIndex(rulesGeneralIndex).then((res: any) => {
				const list = res.data.Data;
				if (list.length == 0) {
					this.initRulesLog();
				} else {
					this.currentRuleLogs = [];
					list.forEach((element, index) => {
						this.AreaGroupsEmpty[index] = false;
						this.currentRuleLogs.push({
							Index: element.Index,

							FromEarlyDate: element.FromEarlyDate,
							FromDate: element.FromDate,
							ToLateDate: element.ToLateDate,
							ToDate: element.ToDate,
							UseDeviceMode: element.UseDeviceMode,
							UseMinimumLog: element.UseMinimumLog,
							UseSequenceLog: element.UseSequenceLog,
							UseTimeLog: element.UseTimeLog,
							UseMode: element.UseMode,
							FromIsNextDay: element.FromIsNextDay,
							MinimumLog: element.MinimumLog,
							ToIsNextDay: element.ToIsNextDay,
							ToLateIsNextDay: element.ToLateIsNextDay,
							RuleGeneralIndex: element.RuleGeneralIndex,
							AreaGroupIndex: this.getAreaGroupArrayByString(element.AreaGroupIndex)
						})
					});
				}
			});
		}

	}
	async getAllAreaGroup() {
		await areaGroupApi.GetAreaGroupAll().then((res: any) => {
		
			if (res.status == 200 && res.statusText == "OK") {
				this.areaGroupList = res.data.data;
			}
		});
	}
	async getListData() {
		await rulesGeneralApi.GetRulesGeneralByCompanyIndex().then((res: any) => {
			this.listData = res.data;
		});
	}
	getDataDisabled(val) {
		const isExist = this.currentRuleLogs.find(e => e.AreaGroupIndex.toString().split(',').find(a => a == val) != null)
		if (isExist != null) {
			return true;
		} else {
			return false;
		}
		// this.currentRuleLogs.forEach(element => {
		// 	const list = element.AreaGroupIndex.toString().split(',');
		// 	console.log("list", list, val);
		// 	list.forEach(item => {
		// 	if (val == item) {
		// 		console.log("disabled");
		// 			return true;
		// 		}
		// 	});
		// });
		// 	console.log("noooooo disabled");
		// 	return false;
		
		// return this.currentRuleLogs.map(({ AreaGroupIndex }) => AreaGroupIndex);
	}
	getAreaGroupArrayByString(areaGroupIndex: string) {
		const arr = areaGroupIndex.split(',');
		const dummy: Array<number> = [];
		if (arr.length > 0) {
			for (let index = 0; index < arr.length; index++) {
				const element = arr[index];
				dummy.push(Number.parseInt(element));
			}
		} else {
			console.log("getAreaGroupArrayByString arr empty", areaGroupIndex);
		}
		return dummy;
	}

	async submit() {
		(this.$refs.formref as any).validate(async (valid) => {
			console.log("this.form", this.ruleGeneral);
			if (!valid) {
				return;
			}
			else {
				if (this.isAdd) {
					await rulesGeneralApi.AddRulesGeneral(this.ruleGeneral).then((res: any) => {
						if (res.status == 200) {
							this.$alert(this.$t('AddSuccess').toString(), this.$t('Notify').toString());

							this.currentGroup = null;
							const indexCallBack = res.data.Data;
							const list: Array<RulesGeneralLogRequestModel> = [];
							this.currentRuleLogs.forEach((element, idx) => {
								const item: RulesGeneralLogRequestModel = element;
								const dateValid = item.FromDate != null && item.FromEarlyDate != null && item.ToDate != null && item.ToLateDate != null;
								if(item.AreaGroupIndex.length > 0 && dateValid) {
									this.AreaGroupsEmpty[idx] = false;								
									this.TimePickerEmpty[idx].FromDate = false;
									this.TimePickerEmpty[idx].FromEarlyDate = false;
									this.TimePickerEmpty[idx].ToDate = false;
									this.TimePickerEmpty[idx].ToLateDate = false;
									item.AreaGroupIndex = item.AreaGroupIndex.toString();
									item.RuleGeneralIndex = indexCallBack;
									list.push(item);
								} else {
									this.AreaGroupsEmpty[idx] = true;
									if(item.FromDate == null){
										this.TimePickerEmpty[idx].FromDate = true;
									}
									if(item.FromEarlyDate == null){
										this.TimePickerEmpty[idx].FromEarlyDate = true;
									}
									if(item.ToDate == null){
										this.TimePickerEmpty[idx].ToDate = true;
									}
									if(item.ToLateDate == null){
										this.TimePickerEmpty[idx].ToLateDate = true;
									}
								}
							});

							if(list.length == this.currentRuleLogs.length) {
								rulesGeneralApi.AddRulesGeneralLog(list).then((res: any) => {
									if (res.status == 200) {
										this.initRulesLog();
										this.initForm();
									}
								});
							}
						}
					});

					await this.getListData();
				} else {
					await rulesGeneralApi.UpdateRulesGeneral(this.ruleGeneral).then((res: any) => {
						if (res.status == 200) {							
							const indexCallBack = res.data.Data;
							this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
							const list: Array<RulesGeneralLogRequestModel> = [];
							this.currentRuleLogs.forEach((element, idx) => {
								const item: RulesGeneralLogRequestModel = element;
								const dateValid = item.FromDate != null && item.FromEarlyDate != null && item.ToDate != null && item.ToLateDate != null;
								if(item.AreaGroupIndex.length > 0 && dateValid) {
									this.AreaGroupsEmpty[idx] = false;								
									this.TimePickerEmpty[idx].FromDate = false;
									this.TimePickerEmpty[idx].FromEarlyDate = false;
									this.TimePickerEmpty[idx].ToDate = false;
									this.TimePickerEmpty[idx].ToLateDate = false;
									item.AreaGroupIndex = item.AreaGroupIndex.toString();
									item.RuleGeneralIndex = indexCallBack;
									list.push(item);
								} else {
									this.AreaGroupsEmpty[idx] = true;
									if(item.FromDate == null){
										this.TimePickerEmpty[idx].FromDate = true;
									}
									if(item.FromEarlyDate == null){
										this.TimePickerEmpty[idx].FromEarlyDate = true;
									}
									if(item.ToDate == null){
										this.TimePickerEmpty[idx].ToDate = true;
									}
									if(item.ToLateDate == null){
										this.TimePickerEmpty[idx].ToLateDate = true;
									}
								}
							});

							if(list.length == this.currentRuleLogs.length) {
								rulesGeneralApi.AddRulesGeneralLog(list).then((res: any) => {
									if (res.status == 200) {
										this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
										this.initRulesLog();
										this.initForm();
										this.currentGroup = null;
										this.isAdd = true;
									}
								});
							}
						}
					});

					await this.getListData();
				}
			}
		});
	}
	addRuleLog() {
		this.currentRuleLogs.push({
			Index: 0,
			FromEarlyDate: new Date(2021, 1, 1, 7, 30),
			FromDate: new Date(2021, 1, 1, 8, 0),
			ToLateDate: new Date(2021, 1, 1, 17, 0),
			ToDate: new Date(2021, 1, 1, 17, 30),
			UseDeviceMode: false,
			UseMinimumLog: false,
			UseSequenceLog: false,
			UseTimeLog: false,
			UseMode: 0,
			FromIsNextDay: false,
			MinimumLog: 1,
			ToIsNextDay: false,
			ToLateIsNextDay: false,
			RuleGeneralIndex: 0,
			AreaGroupIndex: []
		});
		this.AreaGroupsEmpty[this.AreaGroupsEmpty.length] = true;
		this.TimePickerEmpty[this.TimePickerEmpty.length] = {
			FromEarlyDate: false,
			ToDate: false,
			FromDate: false,
			ToLateDate: false
		};
	}
	IsNullOrEmpty(data: Array<any>) {
		return (data == null || data.length < 1);
	}

	async del() {
		if (this.activeMenuIndex < 1) return;

		this.$confirm(this.$t("AreYouSureWantToDelete?").toString(), this.$t("Warning").toString(), {
			confirmButtonText: this.$t("Delete").toString(),
			cancelButtonText: this.$t("MSG_No").toString(),
			type: 'warning'
		}).then(() => {
			rulesGeneralApi.DeleteRulesGeneral(this.activeMenuIndex).then((res: any) => {
				if (res.status == 200) {
					this.$alert(this.$t('DeleteDataSuccessfully').toString(), this.$t('Notify').toString());
					this.initForm();
					this.getListData();
					this.currentGroup = null;
					this.isAdd = true;
				}
			});
		}).catch(() => {
		});

	}
	add() {
		this.initForm();
		this.isAdd = true;
		this.currentGroup = null;
		this.currentGroupName = "";
	}

	@Watch('currentGroup', { deep: true }) hande(val) {
		if (this.currentGroup != null && this.currentGroup != 0) {
			this.groupError = "";
		}

		console.log('val :>> ', val);
	}

	selectAreaGroupOnChange(idx, value) {
		if(value == null || value == undefined || value.length == 0) {
			this.AreaGroupsEmpty[idx] = true;
		} else {
			this.AreaGroupsEmpty[idx] = false;
		}
	}

	timePickerOnChange(idx, value, type) {
		if(this.isEmpty(value)) {
			this.setTimePickerEmptyStatus(this.TimePickerEmpty, idx, type, true);
		} else {
			this.setTimePickerEmptyStatus(this.TimePickerEmpty, idx, type, false);
		}
	}
	isEmpty(value) {
		return value == null || value == undefined;
	}
	setTimePickerEmptyStatus(obj, idx, type, status) {
		switch (type) {
			case 'FromEarlyDate':
				obj[idx].FromEarlyDate = status;
				break;
			case 'ToDate':
				obj[idx].ToDate = status;
				break;
			case 'FromDate':
				obj[idx].FromDate = status;
				break;
			case 'ToLateDate':
				obj[idx].ToLateDate = status;
				break;
		}
	}
}
