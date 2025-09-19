import { Component, Mixins, Watch } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { hrCustomerInfoApi } from '@/$api/hr-customer-info-api';
import { truckerDriverLogApi, InOutMode } from '@/$api/gc-trucker-driver-log-api';
import { lineApi } from '@/$api/gc-lines-api';
import { configApi } from "@/$api/config-api";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { gatesApi } from '@/$api/gc-gates-api';
import { realtimeApi } from '@/$api/realtime-api';

type TruckDriverInInfo = {
	Index?: number;
	EmployeeATID?: string;
	CardNumber?: string;
	MachineSerial?: string;
	TripCode?: string;
	FullName?: string;
	VehiclePlate?: string;
	CompanyName?: string;
	Supplier?: string;
	OrderCode?: string;
	NRIC?: string;
	CardUserIndex?: number;
	DeliveryPoint?: string;
	Phone?: string;
	DeliveryTime?: Date;
	DeliveryTimeString?: string;
	DeliveryTimeDayString?: string;
	DeliveryTimeHourString?: string;
	PassingVehicle?: boolean;
	PassingVehicleName?: string;
	VehicleStatus?: string;
	GateEntryTime?: Date;
	GateExitTime?: Date;
	GateEntryTimeString?: string;
	GateExitTimeString?: string;
	GateEntryTimeDateString?: string;
	GateExitTimeDateString?: string;
	GateEntryTimeHourString?: string;
	GateExitTimeHourString?: string;
}

@Component({
	name: 'TruckDriverInMonitoring',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class TruckDriverInMonitoring extends Mixins(ComponentBase) {
	truckDriverInfo: TruckDriverInInfo = {};
	listCardNumber = [];
	connection;
	isConnect = false;

	isSavedLog = false;

	showReturnCardForm = false;

	listGate = [];
	gateIndex = null;

	async beforeMount() {
		await this.getAllCardNumber();
		await this.GetGatesAll();
	}

	setInterval: ReturnType<typeof setInterval> = setInterval(() => {
		if (this.$route.path == "/truck-driver-in-monitoring") {
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
		this.focusTripCodeField();
		this.getRealTimeServer();
	}

	async GetGatesAll(){
        await gatesApi.GetGatesAll().then((res: any) => {
            this.listGate = [];
            if (res.status == 200 && res.data && res.data.data && res.data.data.length > 0) {
                this.listGate = res.data.data;
                // this.viewingGate = this.listGate[0];
            }
        });
    }

	getRealTimeServer() {
		configApi.GetRealTimeServerLink().then((res: any) => {
			this.connectToRealTimeServer(res.data);
		});
	}

	connectToRealTimeServer(link) {
		this.connection = null;
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
			if(listData && listData.length > 0 && this.$router.currentRoute.path == '/truck-driver-in-monitoring'){
				await lineApi.GetLineBySerialNumber(listData[0].SerialNumber).then(async (res: any) => {
					if (res.data && this.gateIndex && this.gateIndex > 0
						&& res.data.GateIndexes && res.data.GateIndexes.length > 0 
						&& res.data.GateIndexes.includes(this.gateIndex)
						&& res.data.LineForDriver && res.data.LineForDriverIssuanceReturnCard) {
						await truckerDriverLogApi.GetCardNumberById(listData[0].EmployeeATID).then(async (cardRes: any) => {
							// console.log(cardRes)
							if (cardRes.data) {
								if(this.showReturnCardForm){
									if(!this.returnCardForm.IsLostCard){
										this.returnCardForm.CardNumber = cardRes.data.CardNumber;
										await truckerDriverLogApi.GetDriverByCardNumber(cardRes.data.CardNumber)
										.then((driverByCardRes: any) => {
											if(driverByCardRes.status == 200 && driverByCardRes.data){
												this.returnCardForm.NRIC = driverByCardRes.data.Nric;
												this.returnCardForm.TripCode = driverByCardRes.data.EmployeeATID;
												this.returnCardForm.FullName = driverByCardRes.data.FullName;
												this.returnCardForm.ListLogDriver = driverByCardRes.data.ListLogDriver;
												this.returnCardForm.IsFoundData = true;
											}else{
												this.$alert(
													this.$t("CardNotRegister").toString(),
													this.$t("Warning").toString(),
													{ type: "warning" }
												);
												this.returnCardForm.IsFoundData = false;
											}
										});
									}
								}else{
									// if(cardRes.data.UserCode && cardRes.data.UserCode != ""){
									// 	if (!this.showExtraDriverDialog 
									// 		&& cardRes.data.UserCode != this.truckDriverInfo.NRIC
									// 		&& !this.isSavedLog) {
									// 		this.truckDriverInfo.CardNumber = 
									// 			this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									// 			+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
									// 	} else if (this.showExtraDriverDialog 
									// 		&& cardRes.data.UserCode != this.extraDriverForm.ExtraDriverCode
									// 	) {
									// 		this.extraDriverForm.CardNumber = 
									// 			this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									// 			+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
									// 	}
									// }
									if(cardRes.data.CardUserIndex && cardRes.data.CardUserIndex > 0){
										// console.log(this.truckDriverInfo, cardRes.data)
										if (!this.showExtraDriverDialog 
											&& this.truckDriverInfo.Index && this.truckDriverInfo.Index > 0
											&& cardRes.data.CardUserIndex != this.truckDriverInfo.Index
											&& !this.isSavedLog) {
											this.truckDriverInfo.CardNumber = 
												this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
												+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
										} else if (this.showExtraDriverDialog 
											&& cardRes.data.CardUserIndex != this.extraDriverForm.CardUserIndex
										) {
											this.extraDriverForm.CardNumber = 
												this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
												+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
										}
									}
									else{
										if (!this.showExtraDriverDialog) {
											if(this.extraDriverData && this.extraDriverData.length > 0 && 
												this.extraDriverData.some(y => y.CardNumber == cardRes.data.CardNumber)
											){
												this.truckDriverInfo.CardNumber = 
													this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
													+ " " 
													+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber)?.ExtraDriverCode ?? '') 
													+ " - " 
													+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber)?.ExtraDriverName ?? '');
												this.truckDriverInfo.MachineSerial = null;
											}else if(this.isSavedLog && cardRes.data.CardNumber != this.truckDriverInfo.CardNumber){
												this.$alert(
													this.$t("TripIsProvidedCard").toString(),
													this.$t("Warning").toString(),
													{ type: "warning" }
												);
											}else{
												this.truckDriverInfo.CardNumber = cardRes.data.CardNumber;
												this.truckDriverInfo.MachineSerial = listData[0].SerialNumber;
											}
										} else if (this.showExtraDriverDialog) {
											if(this.truckDriverInfo.CardNumber && this.truckDriverInfo.CardNumber != '' 
												&& this.truckDriverInfo.CardNumber.length > 0
												&&this.truckDriverInfo.CardNumber != this.$t("CardNumberNotExist").toString() 
												&& !this.truckDriverInfo.CardNumber.includes(this.$t('CardIsAssigned').toString())
												&& this.truckDriverInfo.CardNumber == cardRes.data.CardNumber){
												this.extraDriverForm.CardNumber = 
												this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
												+ " " 
												+ this.truckDriverInfo.NRIC 
												+ " - " 
												+ this.truckDriverInfo.FullName ;
											}else if(this.extraDriverData && this.extraDriverData.length > 0 && 
												this.extraDriverData.some(y => y.CardNumber == cardRes.data.CardNumber
													&& y.TempIndex != this.extraDriverForm.TempIndex
												)
											){
												this.extraDriverForm.CardNumber = 
													this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
													+ " " 
													+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber 
														&& y.TempIndex != this.extraDriverForm.TempIndex
													)?.ExtraDriverCode ?? '') 
													+ " - " 
													+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber 
														&& y.TempIndex != this.extraDriverForm.TempIndex
													)?.ExtraDriverName ?? '');
											}else if(this.isSavedLog && this.isEdit 
												&& cardRes.data.CardNumber != this.extraDriverForm.CardNumber){
												this.$alert(
													this.$t("TripIsProvidedCard").toString(),
													this.$t("Warning").toString(),
													{ type: "warning" }
												);
											}else{
												this.extraDriverForm.CardNumber = cardRes.data.CardNumber;
											}
										}
									}
								}								
							} else {
								if (!this.showExtraDriverDialog && !this.showReturnCardForm) {
									this.truckDriverInfo.CardNumber = this.$t("CardNumberNotExist").toString();
									this.truckDriverInfo.MachineSerial = null;
								}else if(this.showExtraDriverDialog){
									this.extraDriverForm.CardNumber = this.$t("CardNumberNotExist").toString();
								}else if(this.showReturnCardForm){
									this.$alert(
										this.$t("CardNumberNotExist").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
								}
							}
							this.$forceUpdate();
							// console.log(this.truckDriverInfo)
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
                    .invoke("AddUserToGroup", 2, self.localStorage.getItem("user") + "_truckDriverInMonitoring")
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

	beforeDestroy(){
		// console.log("beforeDestroy")
		this.connection = null;
	}

	destroyed(){
		// console.log("destroy")
		this.connection = null;
	}

	beforeUnmount(){
		// console.log("beforeUnmount")
		this.connection = null;
	}

	unmounted(){
		// console.log("unmounted")
		this.connection = null;
	}

	hiddenTripCodeIn = '';
	setTripCode() {
		if ((!this.hiddenTripCodeIn || this.hiddenTripCodeIn == '') && this.truckDriverInfo.TripCode != '') {
			this.truckDriverInfo.TripCode = this.truckDriverInfo.TripCode;
		} else {
			this.truckDriverInfo.TripCode = this.hiddenTripCodeIn;
		}
		this.getTruckDriverInfo();
		this.hiddenTripCodeIn = '';
	}

	hiddenCardNumber = '';
	isCardNumberFocus = false;
	focusCardNumberField(){
		(this.$refs["hiddenTripCodeIn"] as any).blur();
		(this.$refs["vehiclePlate"] as any).blur();
		setTimeout(() => {
			(this.$refs["hiddenTruckDriverInCardNumber"] as any).focus();
		}, 500);
		this.isCardNumberFocus = true;
		this.$forceUpdate();
	}
	unfocusCardNumberField(){
		(this.$refs["hiddenTruckDriverInCardNumber"] as any).blur();
		this.isCardNumberFocus = false;
		this.$forceUpdate();
	}
	async setCardNumber(){
		if(this.hiddenCardNumber && this.hiddenCardNumber != ''){
			const cloneCardNumber = Misc.cloneData(this.hiddenCardNumber);
			this.hiddenCardNumber = null;
			await truckerDriverLogApi.GetCardNumberByNumber(cloneCardNumber).then(async (cardRes: any) => {
				// console.log(cardRes)
				if (cardRes.data) {
					if(this.showReturnCardForm){
						if(!this.returnCardForm.IsLostCard){
							this.returnCardForm.CardNumber = cardRes.data.CardNumber;
							await truckerDriverLogApi.GetDriverByCardNumber(cardRes.data.CardNumber)
							.then((driverByCardRes: any) => {
								if(driverByCardRes.status == 200 && driverByCardRes.data){
									this.returnCardForm.NRIC = driverByCardRes.data.Nric;
									this.returnCardForm.TripCode = driverByCardRes.data.EmployeeATID;
									this.returnCardForm.FullName = driverByCardRes.data.FullName;
									this.returnCardForm.ListLogDriver = driverByCardRes.data.ListLogDriver;
									this.returnCardForm.IsFoundData = true;
								}else{
									this.$alert(
										this.$t("CardNotRegister").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
									this.returnCardForm.IsFoundData = false;
								}
							});
						}
					}else{
						// if(cardRes.data.UserCode && cardRes.data.UserCode != ""){
						// 	if (!this.showExtraDriverDialog 
						// 		&& cardRes.data.UserCode != this.truckDriverInfo.NRIC
						// 		&& !this.isSavedLog) {
						// 		this.truckDriverInfo.CardNumber = 
						// 			this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
						// 			+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
						// 	} else if (this.showExtraDriverDialog 
						// 		&& cardRes.data.UserCode != this.extraDriverForm.ExtraDriverCode
						// 	) {
						// 		this.extraDriverForm.CardNumber = 
						// 			this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
						// 			+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
						// 	}
						// }
						if(cardRes.data.CardUserIndex && cardRes.data.CardUserIndex > 0){
							// console.log(this.truckDriverInfo, cardRes.data)
							if (!this.showExtraDriverDialog 
								&& this.truckDriverInfo.Index && this.truckDriverInfo.Index > 0
								&& cardRes.data.CardUserIndex != this.truckDriverInfo.Index
								&& !this.isSavedLog) {
								this.truckDriverInfo.CardNumber = 
									this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
							} else if (this.showExtraDriverDialog 
								&& cardRes.data.CardUserIndex != this.extraDriverForm.CardUserIndex
							) {
								this.extraDriverForm.CardNumber = 
									this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
							}
						}
						else{
							if (!this.showExtraDriverDialog) {
								if(this.extraDriverData && this.extraDriverData.length > 0 && 
									this.extraDriverData.some(y => y.CardNumber == cardRes.data.CardNumber)
								){
									this.truckDriverInfo.CardNumber = 
										this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
										+ " " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber)?.ExtraDriverCode ?? '') 
										+ " - " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber)?.ExtraDriverName ?? '');
									this.truckDriverInfo.MachineSerial = null;
								}else if(this.isSavedLog && cardRes.data.CardNumber != this.truckDriverInfo.CardNumber){
									this.$alert(
										this.$t("TripIsProvidedCard").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
								}else{
									this.truckDriverInfo.CardNumber = cardRes.data.CardNumber;
									// this.truckDriverInfo.MachineSerial = listData[0].SerialNumber;
									this.truckDriverInfo.MachineSerial = cloneCardNumber;
								}
							} else if (this.showExtraDriverDialog) {
								if(this.truckDriverInfo.CardNumber && this.truckDriverInfo.CardNumber != '' 
									&& this.truckDriverInfo.CardNumber.length > 0
									&&this.truckDriverInfo.CardNumber != this.$t("CardNumberNotExist").toString() 
									&& !this.truckDriverInfo.CardNumber.includes(this.$t('CardIsAssigned').toString())
									&& this.truckDriverInfo.CardNumber == cardRes.data.CardNumber){
									this.extraDriverForm.CardNumber = 
									this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									+ " " 
									+ this.truckDriverInfo.NRIC 
									+ " - " 
									+ this.truckDriverInfo.FullName ;
								}else if(this.extraDriverData && this.extraDriverData.length > 0 && 
									this.extraDriverData.some(y => y.CardNumber == cardRes.data.CardNumber
										&& y.TempIndex != this.extraDriverForm.TempIndex
									)
								){
									this.extraDriverForm.CardNumber = 
										this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
										+ " " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber 
											&& y.TempIndex != this.extraDriverForm.TempIndex
										)?.ExtraDriverCode ?? '') 
										+ " - " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber 
											&& y.TempIndex != this.extraDriverForm.TempIndex
										)?.ExtraDriverName ?? '');
								}else if(this.isSavedLog && this.isEdit 
									&& cardRes.data.CardNumber != this.extraDriverForm.CardNumber){
									this.$alert(
										this.$t("TripIsProvidedCard").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
								}else{
									this.extraDriverForm.CardNumber = cardRes.data.CardNumber;
								}
							}
						}
					}								
				} else {
					if (!this.showExtraDriverDialog && !this.showReturnCardForm) {
						this.truckDriverInfo.CardNumber = this.$t("CardNumberNotExist").toString();
						this.truckDriverInfo.MachineSerial = null;
					}else if(this.showExtraDriverDialog){
						this.extraDriverForm.CardNumber = this.$t("CardNumberNotExist").toString();
					}else if(this.showReturnCardForm){
						this.$alert(
							this.$t("CardNumberNotExist").toString(),
							this.$t("Warning").toString(),
							{ type: "warning" }
						);
					}
				}
				this.$forceUpdate();
				// console.log(this.truckDriverInfo)
			})
		}
	}

	hiddenExtraCardNumber = '';
	isExtraCardNumberFocus = false;
	focusExtraCardNumberField(){
		(this.$refs["hiddenTripCodeIn"] as any).blur();
		(this.$refs["vehiclePlate"] as any).blur();
		(this.$refs["hiddenExtraCardNumber"] as any).focus();
		this.isExtraCardNumberFocus = true;
		this.$forceUpdate();
	}
	unfocusExtraCardNumberField(){
		(this.$refs["hiddenExtraCardNumber"] as any).blur();
		this.isExtraCardNumberFocus = false;
		this.$forceUpdate();
	}
	async setExtraCardNumber(){
		if(this.hiddenExtraCardNumber && this.hiddenExtraCardNumber != ''){
			const cloneCardNumber = Misc.cloneData(this.hiddenExtraCardNumber);
			this.hiddenExtraCardNumber = null;
			await truckerDriverLogApi.GetCardNumberByNumber(cloneCardNumber).then(async (cardRes: any) => {
				// console.log(cardRes)
				if (cardRes.data) {
					if(this.showReturnCardForm){
						if(!this.returnCardForm.IsLostCard){
							this.returnCardForm.CardNumber = cardRes.data.CardNumber;
							await truckerDriverLogApi.GetDriverByCardNumber(cardRes.data.CardNumber)
							.then((driverByCardRes: any) => {
								if(driverByCardRes.status == 200 && driverByCardRes.data){
									this.returnCardForm.NRIC = driverByCardRes.data.Nric;
									this.returnCardForm.TripCode = driverByCardRes.data.EmployeeATID;
									this.returnCardForm.FullName = driverByCardRes.data.FullName;
									this.returnCardForm.ListLogDriver = driverByCardRes.data.ListLogDriver;
									this.returnCardForm.IsFoundData = true;
								}else{
									this.$alert(
										this.$t("CardNotRegister").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
									this.returnCardForm.IsFoundData = false;
								}
							});
						}
					}else{
						// if(cardRes.data.UserCode && cardRes.data.UserCode != ""){
						// 	if (!this.showExtraDriverDialog 
						// 		&& cardRes.data.UserCode != this.truckDriverInfo.NRIC
						// 		&& !this.isSavedLog) {
						// 		this.truckDriverInfo.CardNumber = 
						// 			this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
						// 			+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
						// 	} else if (this.showExtraDriverDialog 
						// 		&& cardRes.data.UserCode != this.extraDriverForm.ExtraDriverCode
						// 	) {
						// 		this.extraDriverForm.CardNumber = 
						// 			this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
						// 			+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
						// 	}
						// }
						if(cardRes.data.CardUserIndex && cardRes.data.CardUserIndex > 0){
							console.log(this.truckDriverInfo, cardRes.data)
							if (!this.showExtraDriverDialog 
								&& this.truckDriverInfo.Index && this.truckDriverInfo.Index > 0
								&& cardRes.data.CardUserIndex != this.truckDriverInfo.Index
								&& !this.isSavedLog) {
								this.truckDriverInfo.CardNumber = 
									this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
							} else if (this.showExtraDriverDialog 
								&& cardRes.data.CardUserIndex != this.extraDriverForm.CardUserIndex
							) {
								this.extraDriverForm.CardNumber = 
									this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
							}
						}
						else{
							if (!this.showExtraDriverDialog) {
								if(this.extraDriverData && this.extraDriverData.length > 0 && 
									this.extraDriverData.some(y => y.CardNumber == cardRes.data.CardNumber)
								){
									this.truckDriverInfo.CardNumber = 
										this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
										+ " " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber)?.ExtraDriverCode ?? '') 
										+ " - " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber)?.ExtraDriverName ?? '');
									this.truckDriverInfo.MachineSerial = null;
								}else if(this.isSavedLog && cardRes.data.CardNumber != this.truckDriverInfo.CardNumber){
									this.$alert(
										this.$t("TripIsProvidedCard").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
								}else{
									this.truckDriverInfo.CardNumber = cardRes.data.CardNumber;
									// this.truckDriverInfo.MachineSerial = listData[0].SerialNumber;
									this.truckDriverInfo.MachineSerial = cloneCardNumber;
								}
							} else if (this.showExtraDriverDialog) {
								if(this.truckDriverInfo.CardNumber && this.truckDriverInfo.CardNumber != '' 
									&& this.truckDriverInfo.CardNumber.length > 0
									&&this.truckDriverInfo.CardNumber != this.$t("CardNumberNotExist").toString() 
									&& !this.truckDriverInfo.CardNumber.includes(this.$t('CardIsAssigned').toString())
									&& this.truckDriverInfo.CardNumber == cardRes.data.CardNumber){
									this.extraDriverForm.CardNumber = 
									this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									+ " " 
									+ this.truckDriverInfo.NRIC 
									+ " - " 
									+ this.truckDriverInfo.FullName ;
								}else if(this.extraDriverData && this.extraDriverData.length > 0 && 
									this.extraDriverData.some(y => y.CardNumber == cardRes.data.CardNumber
										&& y.TempIndex != this.extraDriverForm.TempIndex
									)
								){
									this.extraDriverForm.CardNumber = 
										this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
										+ " " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber 
											&& y.TempIndex != this.extraDriverForm.TempIndex
										)?.ExtraDriverCode ?? '') 
										+ " - " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber 
											&& y.TempIndex != this.extraDriverForm.TempIndex
										)?.ExtraDriverName ?? '');
								}else{
									this.extraDriverForm.CardNumber = cardRes.data.CardNumber;
								}
							}
						}
					}								
				} else {
					if (!this.showExtraDriverDialog && !this.showReturnCardForm) {
						this.truckDriverInfo.CardNumber = this.$t("CardNumberNotExist").toString();
						this.truckDriverInfo.MachineSerial = null;
					}else if(this.showExtraDriverDialog){
						this.extraDriverForm.CardNumber = this.$t("CardNumberNotExist").toString();
					}else if(this.showReturnCardForm){
						this.$alert(
							this.$t("CardNumberNotExist").toString(),
							this.$t("Warning").toString(),
							{ type: "warning" }
						);
					}
				}
				this.$forceUpdate();
				// console.log(this.truckDriverInfo)
			})
		}
	}

	hiddenReturnCardNumber = '';
	isReturnCardNumberFocus = false;
	focusReturnCardNumberField(){
		(this.$refs["hiddenReturnCardNumber"] as any).focus();
		this.isReturnCardNumberFocus = true;
		this.$forceUpdate();
	}
	unfocusReturnCardNumberField(){
		(this.$refs["hiddenReturnCardNumber"] as any).blur();
		this.isReturnCardNumberFocus = false;
		this.$forceUpdate();
	}
	async setReturnCardNumber(){
		if(this.hiddenReturnCardNumber && this.hiddenReturnCardNumber != ''){
			const cloneCardNumber = Misc.cloneData(this.hiddenReturnCardNumber);
			this.hiddenReturnCardNumber = null;
			await truckerDriverLogApi.GetCardNumberByNumber(cloneCardNumber).then(async (cardRes: any) => {
				// console.log(cardRes)
				if (cardRes.data) {
					if(this.showReturnCardForm){
						if(!this.returnCardForm.IsLostCard){
							this.returnCardForm.CardNumber = cardRes.data.CardNumber;
							await truckerDriverLogApi.GetDriverByCardNumber(cardRes.data.CardNumber)
							.then((driverByCardRes: any) => {
								if(driverByCardRes.status == 200 && driverByCardRes.data){
									this.returnCardForm.NRIC = driverByCardRes.data.Nric;
									this.returnCardForm.TripCode = driverByCardRes.data.EmployeeATID;
									this.returnCardForm.FullName = driverByCardRes.data.FullName;
									this.returnCardForm.ListLogDriver = driverByCardRes.data.ListLogDriver;
									this.returnCardForm.IsFoundData = true;
								}else{
									this.$alert(
										this.$t("CardNotRegister").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
									this.returnCardForm.IsFoundData = false;
								}
							});
						}
					}else{
						// if(cardRes.data.UserCode && cardRes.data.UserCode != ""){
						// 	if (!this.showExtraDriverDialog 
						// 		&& cardRes.data.UserCode != this.truckDriverInfo.NRIC
						// 		&& !this.isSavedLog) {
						// 		this.truckDriverInfo.CardNumber = 
						// 			this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
						// 			+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
						// 	} else if (this.showExtraDriverDialog 
						// 		&& cardRes.data.UserCode != this.extraDriverForm.ExtraDriverCode
						// 	) {
						// 		this.extraDriverForm.CardNumber = 
						// 			this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
						// 			+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
						// 	}
						// }
						if(cardRes.data.CardUserIndex && cardRes.data.CardUserIndex > 0){
							console.log(this.truckDriverInfo, cardRes.data)
							if (!this.showExtraDriverDialog 
								&& this.truckDriverInfo.Index && this.truckDriverInfo.Index > 0
								&& cardRes.data.CardUserIndex != this.truckDriverInfo.Index
								&& !this.isSavedLog) {
								this.truckDriverInfo.CardNumber = 
									this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
							} else if (this.showExtraDriverDialog 
								&& cardRes.data.CardUserIndex != this.extraDriverForm.CardUserIndex
							) {
								this.extraDriverForm.CardNumber = 
									this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									+ " " + cardRes.data.UserCode + " - " + cardRes.data.UserName;
							}
						}
						else{
							if (!this.showExtraDriverDialog) {
								if(this.extraDriverData && this.extraDriverData.length > 0 && 
									this.extraDriverData.some(y => y.CardNumber == cardRes.data.CardNumber)
								){
									this.truckDriverInfo.CardNumber = 
										this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
										+ " " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber)?.ExtraDriverCode ?? '') 
										+ " - " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber)?.ExtraDriverName ?? '');
									this.truckDriverInfo.MachineSerial = null;
								}else if(this.isSavedLog && cardRes.data.CardNumber != this.truckDriverInfo.CardNumber){
									this.$alert(
										this.$t("TripIsProvidedCard").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
								}else{
									this.truckDriverInfo.CardNumber = cardRes.data.CardNumber;
									// this.truckDriverInfo.MachineSerial = listData[0].SerialNumber;
									this.truckDriverInfo.MachineSerial = cloneCardNumber;
								}
							} else if (this.showExtraDriverDialog) {
								if(this.truckDriverInfo.CardNumber && this.truckDriverInfo.CardNumber != '' 
									&& this.truckDriverInfo.CardNumber.length > 0
									&&this.truckDriverInfo.CardNumber != this.$t("CardNumberNotExist").toString() 
									&& !this.truckDriverInfo.CardNumber.includes(this.$t('CardIsAssigned').toString())
									&& this.truckDriverInfo.CardNumber == cardRes.data.CardNumber){
									this.extraDriverForm.CardNumber = 
									this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
									+ " " 
									+ this.truckDriverInfo.NRIC 
									+ " - " 
									+ this.truckDriverInfo.FullName ;
								}else if(this.extraDriverData && this.extraDriverData.length > 0 && 
									this.extraDriverData.some(y => y.CardNumber == cardRes.data.CardNumber
										&& y.TempIndex != this.extraDriverForm.TempIndex
									)
								){
									this.extraDriverForm.CardNumber = 
										this.$t('CardIsAssigned').toString() + " " + this.$t('for').toString() 
										+ " " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber 
											&& y.TempIndex != this.extraDriverForm.TempIndex
										)?.ExtraDriverCode ?? '') 
										+ " - " 
										+ (this.extraDriverData.find(y => y.CardNumber == cardRes.data.CardNumber 
											&& y.TempIndex != this.extraDriverForm.TempIndex
										)?.ExtraDriverName ?? '');
								}else if(this.isSavedLog && this.isEdit 
									&& cardRes.data.CardNumber != this.extraDriverForm.CardNumber){
									this.$alert(
										this.$t("TripIsProvidedCard").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
								}else{
									this.extraDriverForm.CardNumber = cardRes.data.CardNumber;
								}
							}
						}
					}								
				} else {
					if (!this.showExtraDriverDialog && !this.showReturnCardForm) {
						this.truckDriverInfo.CardNumber = this.$t("CardNumberNotExist").toString();
						this.truckDriverInfo.MachineSerial = null;
					}else if(this.showExtraDriverDialog){
						this.extraDriverForm.CardNumber = this.$t("CardNumberNotExist").toString();
					}else if(this.showReturnCardForm){
						this.$alert(
							this.$t("CardNumberNotExist").toString(),
							this.$t("Warning").toString(),
							{ type: "warning" }
						);
					}
				}
				this.$forceUpdate();
				// console.log(this.truckDriverInfo)
			})
		}
	}

	isTripCodeFocus = false;
	focusTripCodeField() {
		(this.$refs["hiddenTripCodeIn"] as any).focus();
		(this.$refs["vehiclePlate"] as any).blur();
		(this.$refs["hiddenTruckDriverInCardNumber"] as any).blur();
		this.isTripCodeFocus = true;
		this.isCardNumberFocus = false;
		this.$forceUpdate();
	}

	unfocusTripCodeField() {
		(this.$refs["hiddenTripCodeIn"] as any).blur();
		this.isTripCodeFocus = false;
		this.$forceUpdate();
	}

	focusVehiclePlateField() {
		(this.$refs["hiddenTripCodeIn"] as any).blur();
		(this.$refs["vehiclePlate"] as any).focus();
		(this.$refs["hiddenTruckDriverInCardNumber"] as any).blur();
		this.isTripCodeFocus = false;
		this.isCardNumberFocus = false;
		this.$forceUpdate();
	}

	unfocusVehiclePlateField() {
		(this.$refs["vehiclePlate"] as any).blur();
		this.$forceUpdate();
	}

	async getAllCardNumber() {
		await truckerDriverLogApi.GetAllCardNumber().then((res: any) => {
			if (res.data) {
				this.listCardNumber = res.data;
			}
		});
	}

	async getTransitTruckDriverInfoByVehiclePlate(){
		if(!this.truckDriverInfo.VehiclePlate 
			|| (this.truckDriverInfo.VehiclePlate && this.truckDriverInfo.VehiclePlate.trim().length == 0)){
			this.$alert(
				this.$t("PleaseInputTransitVehiclePlate").toString(),
				this.$t("Warning").toString(),
				{ type: "warning" }
			);
			return;
		}

		this.isSavedLog = false;
		this.extraDriverData = [];
		const vehiclePlate = Misc.cloneData(this.truckDriverInfo.VehiclePlate);
		this.truckDriverInfo = {VehiclePlate: vehiclePlate};

		await truckerDriverLogApi.GetTransitTruckDriverInfoByVehiclePlate(vehiclePlate).then(async (res: any) => {
			// console.log(res)
			if (res.status == 200 && res.data) {
				if(!res.data.Vc){
					// this.$notify.warning({
                    //     title: this.$t("Notify").toString(),
                    //     message: this.$t("NotFoundTransitVehicle").toString(),
                    //     position: "top-right"
                    // });
					this.$alert(
						this.$t("NotFoundTransitVehicle").toString(),
						this.$t("Warning").toString(),
						{ type: "warning" }
					);
					return;
				}
				if(!res.data.IsRegisDri){
					// this.$notify.warning({
                    //     title: this.$t("Notify").toString(),
                    //     message: this.$t("VehicleNotRegisDri").toString(),
                    //     position: "top-right"
                    // });
					
					this.$alert(
						this.$t("VehicleNotRegisDri").toString(),
						this.$t("Warning").toString(),
						{ type: "warning" }
					);
					return;
				}
				await truckerDriverLogApi.IsTruckDriverInNotOut(res.data.TripId).then(async (checkExistIn: any) => {
					console.log(checkExistIn)
					if (checkExistIn.data === true) {
						this.isSavedLog = true;
					} else if (checkExistIn.data === false) {
						this.$alert(
							this.$t("TripCodeExpired").toString(),
							this.$t("Warning").toString(),
							{ type: "warning" }
						);
						this.truckDriverInfo = {};
						this.extraDriverData = [];
						return;
					}
				});
				this.truckDriverInfo.TripCode = res.data.TripId;
				this.truckDriverInfo.FullName = res.data.DriverName;
				this.truckDriverInfo.VehiclePlate = res.data.TrailerNumber;
				this.truckDriverInfo.CompanyName = res.data.CompanyName;
				this.truckDriverInfo.Supplier = res.data.Supplier;
				this.truckDriverInfo.OrderCode = res.data.OrderCode;
				this.truckDriverInfo.NRIC = res.data.DriverCode;
				this.truckDriverInfo.Index = res.data.Index;
				this.truckDriverInfo.DeliveryPoint = res.data.LocationFrom;
				this.truckDriverInfo.Phone = res.data.DriverPhone;
				this.truckDriverInfo.PassingVehicle = res.data.Vc;
				this.truckDriverInfo.PassingVehicleName = res.data.Vc ? "VC" : "";
				// this.truckDriverInfo.DeliveryTime = res.data.TimesDock;
				this.truckDriverInfo.CardNumber = res.data.CardNumber;
				if (res.data.Eta) {
					this.truckDriverInfo.DeliveryTime = res.data.Eta;
					this.truckDriverInfo.DeliveryTimeString = moment(this.truckDriverInfo.DeliveryTime)
						.format("DD/MM/YYYY HH:mm:ss");
					this.truckDriverInfo.DeliveryTimeDayString = moment(this.truckDriverInfo.DeliveryTime)
						.format("DD/MM/YYYY");
					this.truckDriverInfo.DeliveryTimeHourString = moment(this.truckDriverInfo.DeliveryTime)
						.format("HH:mm");
				}
				if (res.data.TimeIn) {
					this.truckDriverInfo.GateEntryTime = res.data.TimeIn;
					this.truckDriverInfo.GateEntryTimeString = moment(this.truckDriverInfo.GateEntryTime)
						.format("DD/MM/YYYY HH:mm:ss");
					this.truckDriverInfo.GateEntryTimeDateString = moment(this.truckDriverInfo.GateEntryTime)
						.format("DD/MM/YYYY");
					this.truckDriverInfo.GateEntryTimeHourString = moment(this.truckDriverInfo.GateEntryTime)
						.format("HH:mm");
				}
				if (res.data.TimeOut) {
					this.truckDriverInfo.GateExitTime = res.data.TimeOut;
					this.truckDriverInfo.GateExitTimeString = moment(this.truckDriverInfo.GateExitTime)
						.format("DD/MM/YYYY HH:mm:ss");
					this.truckDriverInfo.GateExitTimeDateString = moment(this.truckDriverInfo.GateExitTime)
						.format("DD/MM/YYYY");
					this.truckDriverInfo.GateExitTimeHourString = moment(this.truckDriverInfo.GateExitTime)
						.format("HH:mm");
				}
				this.truckDriverInfo.VehicleStatus = res.data.StatusDockName;
				this.extraDriverData = res.data?.ExtraDriver ?? [];
				if(this.extraDriverData && this.extraDriverData.length > 0){
					for(let i = 0; i < this.extraDriverData.length; i++){
						(this.extraDriverData[i] as any).TempIndex = i + 1;
					}
				}
				// console.log(this.truckDriverInfo)
				this.$forceUpdate();
				if(res.data.Status == "D"){
					// this.$notify.warning({
                    //     title: this.$t("Notify").toString(),
                    //     message: this.$t("TripHasBeenDeleted").toString(),
                    //     position: "top-right"
                    // });
					this.$alert(
						this.$t("TripHasBeenDeleted").toString(),
						this.$t("Warning").toString(),
						{ type: "warning" }
					);
					return;
				}
			}
		});
	}

	async getTruckDriverInfo() {
		// console.log(this.truckDriverInfo.TripCode)
		if (!this.truckDriverInfo.TripCode || this.truckDriverInfo.TripCode == "") {
			this.$alert(
				this.$t("PleaseInputTripCode").toString(),
				this.$t("Warning").toString(),
				{ type: "warning" }
			);
			return;
		}
		this.isSavedLog = false;
		this.extraDriverData = [];
		const tripCode = Misc.cloneData(this.truckDriverInfo.TripCode);
		this.truckDriverInfo = { TripCode: tripCode };
		// console.log(tripCode)
		await truckerDriverLogApi.IsTruckDriverInNotOut(tripCode).then(async (checkExistIn: any) => {
			// console.log(checkExistIn)
			if (checkExistIn.data === true) {
				this.isSavedLog = true;
			} else if (checkExistIn.data === false) {
				this.$alert(
					this.$t("TripCodeExpired").toString(),
					this.$t("Warning").toString(),
					{ type: "warning" }
				);
				this.truckDriverInfo = {};
				this.extraDriverData = [];
				return;
			}
			await truckerDriverLogApi.GetTruckDriverInfoByTripCode(tripCode).then((res: any) => {
				// console.log(res)
				if (res.data) {
					if(!res.data.IsRegisDri && ( res.data.StatusDockName == "" ||  res.data.StatusDockName == null)){
						// this.$notify.warning({
						//     title: this.$t("Notify").toString(),
						//     message: this.$t("VehicleNotRegisDri").toString(),
						//     position: "top-right"
						// });
						
						this.$alert(
							this.$t("VehicleNotRegisDri").toString(),
							this.$t("Warning").toString(),
							{ type: "warning" }
						);
						return;
					}
					this.truckDriverInfo.TripCode = res.data.TripId;
					this.truckDriverInfo.FullName = res.data.DriverName;
					this.truckDriverInfo.VehiclePlate = res.data.TrailerNumber;
					this.truckDriverInfo.CompanyName = res.data.CompanyName;
					this.truckDriverInfo.Supplier = res.data.Supplier;
					this.truckDriverInfo.OrderCode = res.data.OrderCode;
					this.truckDriverInfo.NRIC = res.data.DriverCode;
					this.truckDriverInfo.Index = res.data.Index;
					this.truckDriverInfo.DeliveryPoint = res.data.LocationFrom;
					this.truckDriverInfo.Phone = res.data.DriverPhone;
					this.truckDriverInfo.PassingVehicle = res.data.Vc;
					this.truckDriverInfo.PassingVehicleName = res.data.Vc ? "VC" : "";
					// this.truckDriverInfo.DeliveryTime = res.data.TimesDock;
					this.truckDriverInfo.CardNumber = res.data.CardNumber;
					if (res.data.Eta) {
						this.truckDriverInfo.DeliveryTime = res.data.Eta;
						this.truckDriverInfo.DeliveryTimeString = moment(this.truckDriverInfo.DeliveryTime)
							.format("DD/MM/YYYY HH:mm:ss");
						this.truckDriverInfo.DeliveryTimeDayString = moment(this.truckDriverInfo.DeliveryTime)
							.format("DD/MM/YYYY");
						this.truckDriverInfo.DeliveryTimeHourString = moment(this.truckDriverInfo.DeliveryTime)
							.format("HH:mm");
					}
					if (res.data.TimeIn) {
						this.truckDriverInfo.GateEntryTime = res.data.TimeIn;
						this.truckDriverInfo.GateEntryTimeString = moment(this.truckDriverInfo.GateEntryTime)
							.format("DD/MM/YYYY HH:mm:ss");
						this.truckDriverInfo.GateEntryTimeDateString = moment(this.truckDriverInfo.GateEntryTime)
							.format("DD/MM/YYYY");
						this.truckDriverInfo.GateEntryTimeHourString = moment(this.truckDriverInfo.GateEntryTime)
							.format("HH:mm");
					}
					if (res.data.TimeOut) {
						this.truckDriverInfo.GateExitTime = res.data.TimeOut;
						this.truckDriverInfo.GateExitTimeString = moment(this.truckDriverInfo.GateExitTime)
							.format("DD/MM/YYYY HH:mm:ss");
						this.truckDriverInfo.GateExitTimeDateString = moment(this.truckDriverInfo.GateExitTime)
							.format("DD/MM/YYYY");
						this.truckDriverInfo.GateExitTimeHourString = moment(this.truckDriverInfo.GateExitTime)
							.format("HH:mm");
					}
					this.truckDriverInfo.VehicleStatus = res.data.StatusDockName;
					this.extraDriverData = res.data?.ExtraDriver ?? [];
					if(this.extraDriverData && this.extraDriverData.length > 0){
						for(let i = 0; i < this.extraDriverData.length; i++){
							(this.extraDriverData[i] as any).TempIndex = i + 1;
						}
					}
					// console.log(this.truckDriverInfo)
					this.$forceUpdate();
					if(res.data.Status == "D"){
						// this.$notify.warning({
						// 	title: this.$t("Notify").toString(),
						// 	message: this.$t("TripHasBeenDeleted").toString(),
						// 	position: "top-right"
						// });
						this.$alert(
							this.$t("TripHasBeenDeleted").toString(),
							this.$t("Warning").toString(),
							{ type: "warning" }
						);
						return;
					}
				}
			});
		}).catch((ex: any) => {
			console.log(ex);
			this.truckDriverInfo.TripCode = null;
		});
	}

	async saveLog() {
		// console.log(this.truckDriverInfo.TripCode)
		if (!this.truckDriverInfo.TripCode || this.truckDriverInfo.TripCode == "") {
			this.$alert(
				this.$t("PleaseInputTripCode").toString(),
				this.$t("Warning").toString(),
				{ type: "warning" }
			);
			return;
		}
		// console.log(this.truckDriverInfo.CardNumber)
		if (!this.truckDriverInfo.CardNumber || this.truckDriverInfo.CardNumber == "") {
			this.$alert(
				this.$t("PleaseScanCard").toString(),
				this.$t("Warning").toString(),
				{ type: "warning" }
			);
			return;
		}
		let extraDriverData = Misc.cloneData(this.extraDriverData);
		if (!extraDriverData || extraDriverData.length == 0) {
			extraDriverData = [];
		} else {
			extraDriverData.forEach(element => {
				element.ExtraDriverName = element.ExtraDriverName.trim();
				element.ExtraDriverCode = element.ExtraDriverCode.trim();
				element.BirthDayString = moment(element.BirthDay).format("YYYY/MM/DD HH:mm:ss");
				element.TripCode = this.truckDriverInfo.TripCode;
			});
		}

		var param = {
			TripCode: this.truckDriverInfo.TripCode,
			CardNumber: this.truckDriverInfo.CardNumber,
			InOutMode: InOutMode.Input,
			ExtraDriver: extraDriverData,
			MachineSerial: this.truckDriverInfo.MachineSerial
		};
		await truckerDriverLogApi.IsTruckDriverIn(this.truckDriverInfo.TripCode).then(async (checkExistRes: any) => {
			// console.log(checkExistRes)
			if (checkExistRes && !checkExistRes.data) {
				await truckerDriverLogApi.SaveTruckDriverLog(param).then((res: any) => {
					if (res.data != true && res.status == 200) {
						this.$alert(this.$t("DriverExtraInBlackList").toString() + ": " + `${res.data}` + this.$t("PleaseCheckAgain").toString(), this.$t("Notify").toString(), { dangerouslyUseHTMLString: true });
					}
					else 
					{
						this.truckDriverInfo = {};
						this.extraDriverData = [];
						this.isSavedLog = false;
						this.$saveSuccess();
					}
				});
			} else {
				this.$alert(
					this.$t("TripCodeExpired").toString(),
					this.$t("Warning").toString(),
					{ type: "warning" }
				);
			}
		})
	}

	cancel() {
		this.truckDriverInfo = {};
		this.extraDriverData = [];
		this.isSavedLog = false;
		this.focusTripCodeField();
	}



	returnCardForm = {
		SerialNumber: null,
		CardNumber: null,
		IsLostCard: false,
		NRIC: null,
		TripCode: null,
		FullName: null,
		Description: null,
		IsFoundData: false,
		ListLogDriver: [],
	};

	returnCardFormRules: any;

	initReturnCardForm(){
		this.returnCardForm = {
			SerialNumber: null,
			CardNumber: null,
			IsLostCard: false,
			NRIC: null,
			TripCode: null,
			FullName: null,
			Description: null,
			IsFoundData: false,
			ListLogDriver: [],
		};
	}

	openReturnCardDialog(){
		this.unfocusTripCodeField();
		this.unfocusVehiclePlateField();
		this.unfocusCardNumberField();
		this.initReturnCardFormRules();
		this.showReturnCardForm = true;
		setTimeout(() => {
			this.focusReturnCardNumberField();
			// (this.$refs.returnCardForm as any).validate();
		}, 500);
	}

	cancelReturnCardDialog(){
		// this.unfocusReturnCardNumberField();
		this.focusTripCodeField();
		this.initReturnCardForm();
		this.showReturnCardForm = false;
	}

	clearReturnCardForm(){
		this.returnCardForm.SerialNumber = null;
		this.returnCardForm.CardNumber = null;
		this.returnCardForm.NRIC = null;
		this.returnCardForm.TripCode = null;
		this.returnCardForm.FullName = null;
		this.returnCardForm.Description = null;
		this.returnCardForm.IsFoundData = false;
	}

	initReturnCardFormRules(){
		this.returnCardFormRules = {
			CardNumber: [
				{
					required: true,
					message: this.$t('PleaseScanCard'),
					trigger: 'blur',
				},
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if (!value) {
							callback(new Error(this.$t('PleaseScanCard').toString()))
						}
						if (value && (value == '' || !value.trim().length)) {
							callback(new Error(this.$t('PleaseScanCard').toString()))
						}
						callback();
					},
				},
			],
		}
		if(this.returnCardForm.IsLostCard){
			this.returnCardFormRules = {
				Description: [
					{
						required: true,
						message: this.$t('PleaseEnterDescription'),
						trigger: 'blur',
					},
					{
						trigger: 'change',
						validator: (rule, value: string, callback) => {
							if (!value) {
								callback(new Error(this.$t('PleaseEnterDescription').toString()))
							}
							if (value && (value == '' || !value.trim().length)) {
								callback(new Error(this.$t('PleaseEnterDescription').toString()))
							}
							callback();
						},
					},
				],
				NRIC: [
					{
						required: true,
						message: this.$t('PleaseInputCCCD'),
						trigger: 'blur',
					},
					{
						trigger: 'change',
						validator: (rule, value: string, callback) => {
							if (!value) {
								callback(new Error(this.$t('PleaseInputCCCD').toString()))
							}
							if (value && (value == '' || !value.trim().length)) {
								callback(new Error(this.$t('PleaseInputCCCD').toString()))
							}
							callback();
						},
					},
				],
			}
		}
	}

	changeIsLostCard(){
		this.initReturnCardFormRules();
		(this.$refs.returnCardForm as any).clearValidate();
		this.returnCardForm.SerialNumber = null;
		this.returnCardForm.CardNumber = null;
		this.returnCardForm.NRIC = null;
		this.returnCardForm.TripCode = null;
		this.returnCardForm.FullName = null;
		this.returnCardForm.Description = null;
		this.returnCardForm.IsFoundData = false;
	}

	async searchDriverByCCCD(){
		if(!this.returnCardForm.IsLostCard){
			return;
		}
		if(!this.returnCardForm.NRIC || (this.returnCardForm.NRIC 
			&& (this.returnCardForm.NRIC == '' || !this.returnCardForm.NRIC.trim().length))){
				this.$alert(
					this.$t("PleaseInputCCCD").toString(),
					this.$t("Warning").toString(),
					{ type: "warning" }
				);
				return;
			}
		this.returnCardForm.SerialNumber = null;
		this.returnCardForm.CardNumber = null;
		this.returnCardForm.TripCode = null;
		this.returnCardForm.FullName = null;
		this.returnCardForm.IsFoundData = false;
		await truckerDriverLogApi.GetDriverByCCCD(this.returnCardForm.NRIC).then((res: any) => {
			if(res.status == 200 && res.data){
				if(res.data.IsExpired == null){
					this.$alert(
						this.$t("TripCodeNotActivateYet").toString(),
						this.$t("Warning").toString(),
						{ type: "warning" }
					);
					this.returnCardForm.IsFoundData = false;
				}else if(res.data.IsExpired === false){
					this.returnCardForm.CardNumber = res.data.CardNumber;
					this.returnCardForm.TripCode = res.data.EmployeeATID;
					this.returnCardForm.FullName = res.data.FullName;
					this.returnCardForm.ListLogDriver = res.data.ListLogDriver;
					this.returnCardForm.IsFoundData = true;	
				}else if(res.data.IsExpired === true){
					this.$alert(
						this.$t("TripCodeExpired").toString(),
						this.$t("Warning").toString(),
						{ type: "warning" }
					);
					this.returnCardForm.IsFoundData = false;
				}
			}else{
				this.$alert(
					this.$t("NotFoundRegisterInfo").toString(),
					this.$t("Warning").toString(),
					{ type: "warning" }
				);
				this.returnCardForm.IsFoundData = false;
			}
		});
	}

	async returnCard(isContinue){
		(this.$refs.returnCardForm as any).validate(async (valid: any) => {
			// console.log(valid, "valid")
			if(!valid){
				return;
			}
			if(!this.returnCardForm.IsFoundData){
				await truckerDriverLogApi.GetDriverByCCCD(this.returnCardForm.NRIC).then(async (res: any) => {
					if(res.status == 200 && res.data){
						if(res.data.IsExpired == null){
							this.$alert(
								this.$t("TripCodeNotActivateYet").toString(),
								this.$t("Warning").toString(),
								{ type: "warning" }
							);
							this.returnCardForm.IsFoundData = false;
							return;
						}else if(res.data.IsExpired === false){
							this.returnCardForm.CardNumber = res.data.CardNumber;
							this.returnCardForm.TripCode = res.data.EmployeeATID;
							this.returnCardForm.FullName = res.data.FullName;
							this.returnCardForm.ListLogDriver = res.data.ListLogDriver;
							this.returnCardForm.IsFoundData = true;	

							const data = {
								TripCode: this.returnCardForm.TripCode,
								CardNumber: this.returnCardForm.CardNumber,
								Description: this.returnCardForm.Description,
								SerialNumber: this.returnCardForm.SerialNumber
							}
							// console.log(data)
							await truckerDriverLogApi.ReturnCard(data).then((res: any) => {
								// console.log(res)
								if(res.status == 200){
									this.$saveSuccess();
								}
							})
							.finally(() => {
								if(isContinue){
									this.returnCardForm.CardNumber = null;
									this.returnCardForm.NRIC = null;
									this.returnCardForm.TripCode = null;
									this.returnCardForm.FullName = null;
									this.returnCardForm.Description = null;
									this.returnCardForm.IsFoundData = false;
									this.returnCardForm.ListLogDriver = [];
								}else{
									this.cancelReturnCardDialog();
								}
							});
						}else if(res.data.IsExpired === true){
							this.$alert(
								this.$t("TripCodeExpired").toString(),
								this.$t("Warning").toString(),
								{ type: "warning" }
							);
							this.returnCardForm.IsFoundData = false;
							return;
						}
					}else{
						this.$alert(
							this.$t("NotFoundRegisterInfo").toString(),
							this.$t("Warning").toString(),
							{ type: "warning" }
						);
						this.returnCardForm.IsFoundData = false;
						return;
					}
				});
			}else{
				const data = {
					TripCode: this.returnCardForm.TripCode,
					CardNumber: this.returnCardForm.CardNumber,
					Description: this.returnCardForm.Description,
					SerialNumber: this.returnCardForm.SerialNumber
				}
				// console.log(data)
				await truckerDriverLogApi.ReturnCard(data).then((res: any) => {
					// console.log(res)
					if(res.status == 200){
						this.$saveSuccess();
					}
				})
				.finally(() => {
					if(isContinue){
						this.returnCardForm.CardNumber = null;
						this.returnCardForm.NRIC = null;
						this.returnCardForm.TripCode = null;
						this.returnCardForm.FullName = null;
						this.returnCardForm.Description = null;
						this.returnCardForm.IsFoundData = false;
						this.returnCardForm.ListLogDriver = [];
					}else{
						this.cancelReturnCardDialog();
					}
				});
			}
		});
	}



	isEdit = false;
	extraDriverData = [];
	extraDriverForm = {
		Index: 0,
		TempIndex: 0,
		ExtraDriverName: '',
		ExtraDriverCode: '',
		BirthDay: null,
		CardNumber: '',
		CardUserIndex: 0
	};
	extraDriverRules = {
		ExtraDriverName: [
			{
				required: true,
				message: this.$t('PleaseInputExtraDriverName'),
				trigger: 'blur',
			},
			{
				trigger: 'change',
				validator: (rule, value: string, callback) => {
					if (!value) {
						callback(new Error(this.$t('PleaseInputExtraDriverName').toString()))
					}
					if (value && (value == '' || !value.trim().length)) {
						callback(new Error(this.$t('PleaseInputExtraDriverName').toString()))
					}
					callback();
				},
			},
		],
		ExtraDriverCode: [
			{
				required: true,
				message: this.$t('PleaseInputCCCD'),
				trigger: 'blur',
			},
			{
				trigger: 'change',
				validator: (rule, value: string, callback) => {
					if (!value) {
						callback(new Error(this.$t('PleaseInputCCCD').toString()))
					}
					if (value && (value == '' || !value.trim().length)) {
						callback(new Error(this.$t('PleaseInputCCCD').toString()))
					}
					callback();
				},
			},
		],
		BirthDay: [
			{
				required: true,
				message: this.$t('PleaseSelectBirthDay'),
				trigger: 'blur',
			},
			{
				trigger: 'change',
				validator: (rule, value: Date, callback) => {
					if (!value) {
						callback(new Error(this.$t('PleaseSelectBirthDay').toString()))
					}
					const currentDate = new Date();

					const date18YearsAgo = new Date(
						currentDate.getFullYear() - 18,
						currentDate.getMonth(),
						currentDate.getDate()
					);
					if (value > date18YearsAgo) {
						callback(new Error(this.$t('ExtraDriverAgeMustBeEqualOrLargerThan18').toString()));
					}
					callback();
				},
			},
		],
		CardNumber: [
			{
				required: true,
				message: this.$t('PleaseScanCard'),
				trigger: 'blur',
			},
			{
				trigger: 'change',
				validator: (rule, value: string, callback) => {
					if(!value){
						callback(new Error(this.$t('PleaseScanCard').toString()))
					}
					if(value && (value == '' || !value.trim().length 
						|| value == this.$t("CardNumberNotExist").toString()
						|| value.includes(this.$t("CardIsAssigned").toString()))){
						callback(new Error(this.$t('PleaseScanCard').toString()))
					}
					callback();
				},
			},
		],
	};
	showExtraDriverDialog = false;

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	openExtraDriverDialog() {
		this.unfocusTripCodeField();
		this.unfocusVehiclePlateField();
		this.unfocusCardNumberField();
		this.showExtraDriverDialog = true;
		setTimeout(() => {
			this.focusExtraCardNumberField();
		}, 500);
	}

	cancelExtraDriverDialog() {
		this.isEdit = false;
		this.extraDriverForm = {
			Index: 0,
			TempIndex: 0,
			ExtraDriverName: '',
			ExtraDriverCode: '',
			BirthDay: null,
			CardNumber: '',
			CardUserIndex: 0
		};
		// this.cancelExtraDriverDialog();
		this.focusTripCodeField();
		this.showExtraDriverDialog = false;
	}

	editExtraDriver(row) {
		// console.log(row);
		if(this.extraDriverData && this.extraDriverData.length > 0){
			for(let i = 0; i < this.extraDriverData.length; i++){
				(this.extraDriverData[i] as any).TempIndex = i + 1;
			}
		}
		this.isEdit = true;
		this.extraDriverForm = Misc.cloneData(this.extraDriverData.find(x => x.Index == row.Index));
		this.openExtraDriverDialog();
	}

	async deleteExtraDriver(row) {
		// console.log(row)
		this.extraDriverData = this.extraDriverData.filter(x => x != row);
		if (this.isSavedLog && row.Index > 0) {
			await truckerDriverLogApi.DeleteExtraTruckDriverLog(row.Index).then((res: any) => {
				if (res.data && res.status == 200) {
					this.$saveSuccess();
					this.getTruckDriverInfo();
				}
			});
		}
	}

	async submitExtraDriver(isContinue) {
		// console.log(this.extraDriverForm);
		(this.$refs.extraDriverForm as any).validate(async (valid) => {
			if (!valid) return;

			const oldExtraDriverData = Misc.cloneData(this.extraDriverData);
			if (!this.isEdit) {
				this.extraDriverForm.CardUserIndex = this.truckDriverInfo.Index;
				this.extraDriverForm.ExtraDriverName = this.extraDriverForm.ExtraDriverName.trim();
				this.extraDriverForm.ExtraDriverCode = this.extraDriverForm.ExtraDriverCode.trim();
				this.extraDriverForm.BirthDay = this.extraDriverForm.BirthDay;
				(this.extraDriverForm as any).BirthDayString = moment(this.extraDriverForm.BirthDay).format("DD-MM-YYYY");
				this.extraDriverData.push(this.extraDriverForm);
			} else {
				let existedExtraDriver = this.extraDriverData.find(x => x.Index == this.extraDriverForm.Index 
					|| (x.Index == 0 && x.TempIndex == this.extraDriverForm.TempIndex)
				);
				existedExtraDriver.CardUserIndex = this.truckDriverInfo.Index;
				existedExtraDriver.CardNumber = this.extraDriverForm.CardNumber;
				existedExtraDriver.ExtraDriverName = this.extraDriverForm.ExtraDriverName.trim();
				existedExtraDriver.ExtraDriverCode = this.extraDriverForm.ExtraDriverCode.trim();
				existedExtraDriver.BirthDay = this.extraDriverForm.BirthDay;
				existedExtraDriver.BirthDayString = moment(this.extraDriverForm.BirthDay).format("DD-MM-YYYY");
			}

			if(this.extraDriverData && this.extraDriverData.length > 0){
				for(let i = 0; i < this.extraDriverData.length; i++){
					(this.extraDriverData[i] as any).TempIndex = i + 1;
				}
			}

			if (this.isSavedLog) {
				const extraDriverData = Misc.cloneData(this.extraDriverData);
				extraDriverData.forEach(element => {
					element.BirthDayString = moment(element.BirthDay).format("DD-MM-YYYY");
					element.TripCode = this.truckDriverInfo.TripCode;
				});
				await truckerDriverLogApi.SaveExtraTruckDriverLog(extraDriverData).then((res: any) => {
					if (res.data != true && res.status == 200) {
						this.$alert(this.$t("DriverExtraInBlackList").toString() + ": " + `${res.data}` + this.$t("PleaseCheckAgain").toString(), this.$t("Notify").toString(), { dangerouslyUseHTMLString: true });
						this.extraDriverData = oldExtraDriverData;
					}
					else {
						this.$saveSuccess();

						if (!isContinue) {
							this.cancelExtraDriverDialog();
						} else {
							this.extraDriverForm = {
								Index: 0,
								TempIndex: 0,
								ExtraDriverName: '',
								ExtraDriverCode: '',
								BirthDay: null,
								CardNumber: '',
								CardUserIndex: 0
							};
						}

						this.getTruckDriverInfo();
					}
					// if (res.data && res.status == 200) {
					// 	this.$saveSuccess();
					// 	this.getTruckDriverInfo();
					// }
				});
			}else{
				if (!isContinue) {
					this.cancelExtraDriverDialog();
				} else {
					this.extraDriverForm = {
						Index: 0,
						TempIndex: 0,
						ExtraDriverName: '',
						ExtraDriverCode: '',
						BirthDay: null,
						CardNumber: '',
						CardUserIndex: 0
					};
				}
			}
		});
	}
}
