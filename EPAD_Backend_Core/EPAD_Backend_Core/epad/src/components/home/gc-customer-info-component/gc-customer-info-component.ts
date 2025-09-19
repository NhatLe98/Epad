import { Component, Mixins, Watch } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { TA_RulesGlobalDTO, taRulesGlobalApi } from '@/$api/ta-rules-global-api';
import { hrCustomerInfoApi } from '@/$api/hr-customer-info-api';
import { truckerDriverLogApi, InOutMode } from '@/$api/gc-trucker-driver-log-api';
import { lineApi } from '@/$api/gc-lines-api';
import { configApi } from "@/$api/config-api";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { realtimeApi } from '@/$api/realtime-api';

@Component({
	name: 'GCCustomerInfo',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class GCCustomerInfo extends Mixins(ComponentBase) {
	// page = 1;
	// showDialog = false;
	// showMessage = false;
	// checked = false;
	// columns = [];
	// rowsObj = [];
	// isEdit = false;
	// listExcelFunction = [];
	ruleForm: TA_RulesGlobalDTO = {
		MaximumAnnualLeaveRegisterByMonth: 0,
		OverTimeNormalDay: 0,
		NightOverTimeNormalDay: 0,
		OverTimeLeaveDay: 0,
		NightOverTimeLeaveDay: 0,
		OverTimeHoliday: 0,
		NightOverTimeHoliday: 0,
	};
	rules: any = {};
	// listLockAttendanceTime = [];

	// activeCollapse = ['a','b','c','d'];

	listAllCustomer: any = [];
    listAllCustomerFilter: any = [];
    listAllCustomerForm: any = [];

	customerID: any = null;
	customerInfo: any = {};

	defaultImage: any;

	connection;
    isConnect = false;

	isNotReturnOrDelete = false;

	beforeMount() {
		this.initRule();
		// for(let i = 1; i <= 31; i++){
		// 	this.listLockAttendanceTime.push({Value: i, Name: i});
		// }
		// this.ruleForm.LockAttendanceTime = this.listLockAttendanceTime[0].Value;
		// this.getData();

		this.getAllCustomer();
		this.loadImageDefault();
	}

	setInterval: ReturnType<typeof setInterval> = setInterval(() => {
		if (this.$route.path == "/gc-customer-info") {
		  realtimeApi.ConnectToServer().then((res: any) => {
			// this.getIp();
			return res;
		  }).catch(err => {
			if (err?.response?.status === 401) {
			  this.$router.push('/login').catch((err) => { });
			}
		  })
		} else {
		  clearInterval(this.setInterval);
		}
	
	  }, 60000);// Call API stay connected 60000 ms => 1 minute

	  

	mounted() {
		this.getRealTimeServer();
		(this.$refs.customerInfoCardNumberHidden as any).focus();
	}

	async checkCardByNumber(){
		if(this.customerInfo.EmployeeATID && this.customerInfo.EmployeeATID.trim().length > 0){
			this.customerInfo.CardNumber = Misc.cloneData(this.customerInfo.CardNumberHidden);
			this.customerInfo.CardNumberHidden = '';
			await truckerDriverLogApi.GetCardNumberByNumber(this.customerInfo.CardNumber).then((cardRes: any) => {
				// console.log(cardRes)
				if(cardRes.data){
					this.customerInfo.CardNumber = cardRes.data.CardNumber;
					if(cardRes.data.CardUserIndex && cardRes.data.CardUserIndex > 0){
						// console.log(cardRes.data.CardUserIndex)
						this.customerInfo.CardUserIndex = cardRes.data.CardUserIndex;
						this.customerInfo.CardIssuanceFor = cardRes.data.UserCode;
						this.customerInfo.CardIssuanceForName = cardRes.data.UserName;
						this.isNotReturnOrDelete = true;
					}else{
						this.customerInfo.CardUserIndex = 0; 
						this.customerInfo.CardIssuanceFor = null;
						this.customerInfo.CardIssuanceForName = null;
						this.isNotReturnOrDelete = false;
					}
					this.$forceUpdate();
				}
			})
		}
	}

	getRealTimeServer() {
        configApi.GetRealTimeServerLink().then((res: any) => {
            this.connectToRealTimeServer(res.data);
        });
    }

	connectToRealTimeServer(link) {
        this.connection = new HubConnectionBuilder()
            .withUrl(link + "/attendanceHub")
            .configureLogging(LogLevel.Information)
            .build();
        const data = this.connection;
        const notify = this.$notify;

        // receive data from server
        const thisVue = this;
        this.connection.on("ReceiveAttendanceLog", async listData => {
			// console.log(listData)
			if(listData && listData.length > 0 && this.customerID && this.customerID != '' 
				&& !this.isNotReturnOrDelete
				// && (this.customerInfo.CardNumber == null || this.customerInfo.CardIssuanceFor == null)
			){
				await lineApi.GetLineBySerialNumber(listData[0].SerialNumber).then(async (res: any) => {
					// console.log(res)
					if(res.data && res.data.LineForCustomer && res.data.LineForCustomerIssuanceReturnCard){
						await truckerDriverLogApi.GetCardNumberById(listData[0].EmployeeATID).then((cardRes: any) => {
							// console.log(cardRes)
							if(cardRes.data){
								this.customerInfo.CardNumber = cardRes.data.CardNumber;
								// if(cardRes.data.UserCode && cardRes.data.UserCode != ''){
								// 	this.customerInfo.CardIssuanceFor = cardRes.data.UserCode;
								// 	this.customerInfo.CardIssuanceForName = cardRes.data.UserName;
								// 	this.isNotReturnOrDelete = true;
								// }
								if(cardRes.data.CardUserIndex && cardRes.data.CardUserIndex > 0){
									this.customerInfo.CardUserIndex = cardRes.data.CardUserIndex;
									this.customerInfo.CardIssuanceFor = cardRes.data.UserCode;
									this.customerInfo.CardIssuanceForName = cardRes.data.UserName;
									this.isNotReturnOrDelete = true;
								}
								else{
									this.customerInfo.CardUserIndex = 0;
									this.customerInfo.CardIssuanceFor = null;
									this.customerInfo.CardIssuanceForName = null;
									this.isNotReturnOrDelete = false;
								}
								this.$forceUpdate();
							}
						})
					}
				});
			}
        });
        this.connection
            .start()
            .then(() => {
                thisVue.isConnect = true;
                data
                    .invoke("AddUserToGroup", 2, self.localStorage.getItem("user") + "_gcCustomerInfo")
                    .catch(err => {
                        // console.log(err.toString());
                    });
            })
            .catch(err => {
                thisVue.isConnect = false;
                // console.log(err.toString());
            });
        this.connection.onclose(() => {
            thisVue.isConnect = false;
			this.connectToRealTimeServer(link);
        });
    }

	oldCardNumber = null;
	async returnOrDeleteCard(type){
		if(type != 'return'){
			this.oldCardNumber = Misc.cloneData(this.customerInfo.CardNumber);
		}
		await hrCustomerInfoApi.ReturnOrDeleteCard(this.customerInfo.CardNumber).then(async (res: any) => {
			// console.log(res)
			if(res.data && res.status == 200){
				this.isNotReturnOrDelete = false;
				if(type == 'return'){
					this.customerID = null;
					this.customerInfo.CardNumber = null;
					this.changeCustomer();
				}
				// this.customerInfo.CardUserIndex = 0;
				this.customerInfo.CardIssuanceFor = null;
				this.customerInfo.CardIssuanceForName = null;
				this.$forceUpdate();
				this.getAllCustomer();
			}
		}).finally(() => {
			// this.getAllCustomer();
		});
	}

	loadImageDefault() {
        Misc.readFileAsync("static/variables/image_default.json").then(value => {
            this.defaultImage = value?.[0]?.["photo"] ?? "";
        });
    }

	initRule() {
		this.rules = {
			NightShiftStartTime: [
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if(!this.ruleForm.NightShiftStartTime && this.ruleForm.NightShiftEndTime){
							callback(new Error(this.$t('PleaseSelectStartTime').toString()));
						}else if(this.ruleForm.NightShiftStartTime && this.ruleForm.NightShiftEndTime){
							const startValue = new Date();
							startValue.setHours((this.ruleForm.NightShiftStartTime as Date).getHours(), 
							(this.ruleForm.NightShiftStartTime as Date).getMinutes(), 
							(this.ruleForm.NightShiftStartTime as Date).getSeconds());
							const endValue = new Date();
							endValue.setHours((this.ruleForm.NightShiftEndTime as Date).getHours(), 
							(this.ruleForm.NightShiftEndTime as Date).getMinutes(), 
							(this.ruleForm.NightShiftEndTime as Date).getSeconds());
							if(this.ruleForm.NightShiftOvernightEndTime){
								endValue.setDate(endValue.getDate() + 1);
							}
							const start = (this.ruleForm.NightShiftStartTime as Date) 
								? Math.trunc(startValue.getTime() / 1000) : 0;
							const end = (this.ruleForm.NightShiftEndTime as Date) 
								? Math.trunc(endValue.getTime() / 1000) : 0;
							if (start > 0 && end > 0 && start > end) {
								callback(new Error(this.$t('StartTimeCannotLargerThanEndTime').toString()));
							}
						}
						callback();
					},
				},
			],
			NightShiftEndTime: [
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if(this.ruleForm.NightShiftStartTime && !this.ruleForm.NightShiftEndTime){
							callback(new Error(this.$t('PleaseSelectEndTime').toString()));
						}else if(this.ruleForm.NightShiftStartTime && this.ruleForm.NightShiftEndTime){
							const startValue = new Date();
							startValue.setHours((this.ruleForm.NightShiftStartTime as Date).getHours(), 
							(this.ruleForm.NightShiftStartTime as Date).getMinutes(), 
							(this.ruleForm.NightShiftStartTime as Date).getSeconds());
							const endValue = new Date();
							endValue.setHours((this.ruleForm.NightShiftEndTime as Date).getHours(), 
							(this.ruleForm.NightShiftEndTime as Date).getMinutes(), 
							(this.ruleForm.NightShiftEndTime as Date).getSeconds());
							if(this.ruleForm.NightShiftOvernightEndTime){
								endValue.setDate(endValue.getDate() + 1);
							}
							const start = (this.ruleForm.NightShiftStartTime as Date) 
								? Math.trunc(startValue.getTime() / 1000) : 0;
							const end = (this.ruleForm.NightShiftEndTime as Date) 
								? Math.trunc(endValue.getTime() / 1000) : 0;
							if (start > 0 && end > 0 && start > end) {
								callback(new Error(this.$t('StartTimeCannotLargerThanEndTime').toString()));
							}
						}
						callback();
					},
				},
			],
		};
	}

	// @Watch("ruleForm", {deep: true})
	// handleChangeForm(){
	// 	(this.$refs.taRulesGlobalForm as any).validate();
	// }

	// async getData() {
	// 	await taRulesGlobalApi.GetRulesGlobal().then((res) => {
	// 		// console.log(res)
	// 		if(res.status == 200 && res.data){
	// 			this.ruleForm = res.data;
	// 			this.ruleForm.LockAttendanceTime = this.ruleForm.LockAttendanceTime > 0 ? this.ruleForm.LockAttendanceTime : null;
	// 			this.ruleForm.MaximumAnnualLeaveRegisterByMonth = this.ruleForm.MaximumAnnualLeaveRegisterByMonth || 0;
	// 			this.ruleForm.OverTimeNormalDay = this.ruleForm.OverTimeNormalDay || 0;
	// 			this.ruleForm.NightOverTimeNormalDay = this.ruleForm.NightOverTimeNormalDay || 0;
	// 			this.ruleForm.OverTimeLeaveDay = this.ruleForm.OverTimeLeaveDay || 0;
	// 			this.ruleForm.NightOverTimeLeaveDay = this.ruleForm.NightOverTimeLeaveDay || 0;
	// 			this.ruleForm.OverTimeHoliday = this.ruleForm.OverTimeHoliday || 0;
	// 			this.ruleForm.NightOverTimeHoliday = this.ruleForm.NightOverTimeHoliday || 0;
	// 			if(this.ruleForm.NightShiftStartTime){
	// 				this.ruleForm.NightShiftStartTime = new Date(this.ruleForm.NightShiftStartTime);
	// 			}
	// 			if(this.ruleForm.NightShiftEndTime){
	// 				this.ruleForm.NightShiftEndTime = new Date(this.ruleForm.NightShiftEndTime);
	// 			}
	// 		}
	// 	});
	// }

	// reset() {
	// 	const obj: TA_RulesGlobalDTO = {
	// 		MaximumAnnualLeaveRegisterByMonth: 0,
	// 		OverTimeNormalDay: 0,
	// 		NightOverTimeNormalDay: 0,
	// 		OverTimeLeaveDay: 0,
	// 		NightOverTimeLeaveDay: 0,
	// 		OverTimeHoliday: 0,
	// 		NightOverTimeHoliday: 0,
	// 		};
	// 	this.ruleForm = obj;
	// }

	// async Submit() {
	// 	(this.$refs.taRulesGlobalForm as any).validate(async (valid) => {
	// 		// console.log('ruleForm', this.ruleForm);
	// 		if (!valid) return;
	// 		else {
	// 			if(this.ruleForm.NightShiftStartTime){
	// 				this.ruleForm.NightShiftStartTimeString = moment(this.ruleForm.NightShiftStartTime).format("YYYY-MM-DD HH:mm:ss");
	// 			}
	// 			if(this.ruleForm.NightShiftEndTime){
	// 				this.ruleForm.NightShiftEndTimeString = moment(this.ruleForm.NightShiftEndTime).format("YYYY-MM-DD HH:mm:ss");
	// 			}
	// 			await taRulesGlobalApi.UpdateRulesGlobal(this.ruleForm).then((res) => {
	// 				// console.log(res);
	// 				if(res.status && res.status == 200 && res.data){
	// 					this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
	// 				}else{
	// 					this.$alert(this.$t('UpdateFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
	// 				}
	// 			}).finally(() => {
	// 				this.getData();
	// 			})
	// 		}
	// 	});
	// }

	async getAllCustomer() {
        await hrCustomerInfoApi.GetNewestActiveCustomerInfo().then((res: any) => {
            if (res.status == 200) {
                const data = res.data;
				if(data && data.length > 0){
					data.forEach(element => {
						element.ContactDepartmentName = (element.ContactDepartmentName && element.ContactDepartmentName != '') 
						? this.$t(element.ContactDepartmentName).toString() : '';
					});
				}
                this.listAllCustomer = data;
                this.listAllCustomerFilter = data.map(x => ({
                    Index: x.EmployeeATID,
                    FullName: (x.FullName && x.FullName != '') ? (x.FullName + " - " + x.NRIC) : (x.EmployeeATID + " - " + x.NRIC),
				}));
                this.listAllCustomerForm = this.listAllCustomerFilter;
				if(this.customerID){
					this.changeCustomer();
				}
				if(this.oldCardNumber && this.oldCardNumber != ''){
					this.customerInfo.CardNumber = Misc.cloneData(this.oldCardNumber);
					this.oldCardNumber = null;
				}
            }
        });
    }

	visibleCustomerSelect = false;
	changeVisible(value){
		if(value){
			this.tryFocus();
		}else{
			setTimeout(() => {
				const focusNode = (document.getElementsByClassName("customer-select")[0] as HTMLElement).childNodes[1].childNodes[1];
				(focusNode as any).readonly = true;
				(focusNode as HTMLElement).blur();
				// // (this.$refs.customerInfoCardNumberHidden as HTMLElement).focus();  
				const node = (document.getElementsByClassName('customer-info__card-number__hidden')[0] as HTMLElement).childNodes[1];
				(node as HTMLElement).focus();
				(node as HTMLElement).click();
				this.$forceUpdate();
				this.visibleCustomerSelect = false;
			}, 500);
		}
	}

	tryFocus(){
		this.visibleCustomerSelect = true;
		(this.$refs.customerInfoCardNumberHidden as any).blur();
		const focusNode = (document.getElementsByClassName("customer-select")[0] as HTMLElement).childNodes[1].childNodes[1];
		(focusNode as HTMLElement).focus();
		(focusNode as any).select();
		this.$forceUpdate();
	}

	changeCustomer(){
		this.isNotReturnOrDelete = false;
		this.customerInfo = {};
		if(this.customerID){
			this.customerInfo = Misc.cloneData(this.listAllCustomer.find(x => x.EmployeeATID == this.customerID));
			// console.log(this.customerInfo)
			this.customerInfo.CardUserIndex = this.customerInfo.CardUserIndex;
			this.customerInfo.FromTimeDayString = moment(this.customerInfo.FromTime).format("YYYY-MM-DD");
			this.customerInfo.ToTimeDayString = moment(this.customerInfo.ToTime).format("YYYY-MM-DD");
			this.customerInfo.FromTimeHourString = moment(this.customerInfo.FromTime).format("HH:mm:ss");
			this.customerInfo.ToTimeHourString = moment(this.customerInfo.ToTime).format("HH:mm:ss");
			if(this.customerInfo.CardNumber && this.customerInfo.CardNumber != ''){
				this.customerInfo.CardIssuanceFor = this.customerInfo.EmployeeATID;
				this.customerInfo.CardIssuanceForName = this.customerInfo.FullName;
				this.isNotReturnOrDelete = true;
			}
			// console.log(this.customerInfo)
		}else{
			this.customerInfo = {};
		}
	}

	async ConfirmClick() {
		if(!this.customerID || this.customerID == ''){
			this.$alert(
                this.$t("PleaseSelectCustomer").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
			return;
		}
		if(!this.customerInfo.CardNumber || this.customerInfo.CardNumber == ''){
			this.$alert(
                this.$t("PleaseWipeCard").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
			return;
		}
		await hrCustomerInfoApi.UpdateCustomerCardNumber(this.customerID, this.customerInfo.CardNumber).then((res: any) => {
			// console.log(res);
			if(res.status == 200){
				this.$saveSuccess();
				this.customerInfo.CardUserIndex = this.customerInfo.CardUserIndex;
				this.customerInfo.CardIssuanceFor = this.customerID;
				this.customerInfo.CardIssuanceForName = this.listAllCustomer.find(x =>
					x.EmployeeATID == this.customerID
				)?.FullName ?? '';
				this.customerInfo.CardUserIndex = this.listAllCustomer.find(x =>
					x.EmployeeATID == this.customerID
				)?.CardUserIndex ?? 0;
				// console.log(this.customerInfo)
				this.isNotReturnOrDelete = true;
			}
			this.getAllCustomer();
		}).finally(() => {
			// this.getAllCustomer();
		});
    }

	CancelClick(){
		this.customerID = null;
		this.customerInfo = {};
		this.isNotReturnOrDelete = false;
	}
}
