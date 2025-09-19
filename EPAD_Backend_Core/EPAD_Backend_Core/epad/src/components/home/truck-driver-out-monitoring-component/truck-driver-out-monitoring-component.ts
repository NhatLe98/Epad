import { Component, Mixins, Watch } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { truckerDriverLogApi, InOutMode } from '@/$api/gc-trucker-driver-log-api';
import { lineApi } from '@/$api/gc-lines-api';
import { configApi } from "@/$api/config-api";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { realtimeApi } from '@/$api/realtime-api';

type TruckDriverOutInfo = {
    Index?: number;
	EmployeeATID?: string;
    CardNumber?: string;
	LogInCardNumber?: string;
	LogOutCheckTime?: Date;
	LogOutCheckTimeString?: string;
	MachineSerial?: string;
	IsActive?: boolean;
	TripCode?: string;
	FullName?: string;
	VehiclePlate?: string;
	CompanyName?: string;
	Supplier?: string;
	OrderCode?: string;
	NRIC?: string;
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
	IsException?: boolean;
	ReasonException?: string;
}

@Component({
	name: 'TruckDriverOutMonitoring',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class TruckDriverOutMonitoring extends Mixins(ComponentBase) {
	truckDriverInfo: TruckDriverOutInfo = {};
	connection;
    isConnect = false;

	extraDriverData = [];

	beforeMount() {

	}

	setInterval: ReturnType<typeof setInterval> = setInterval(() => {
		if (this.$route.path == "/truck-driver-out-monitoring") {
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

	hiddenTripCodeOut = '';
	setTripCode(){
		if((!this.hiddenTripCodeOut || this.hiddenTripCodeOut == '') && this.truckDriverInfo.TripCode != ''){
			this.truckDriverInfo.TripCode = this.truckDriverInfo.TripCode;
		}else{
			this.truckDriverInfo.TripCode = this.hiddenTripCodeOut;
		}
		this.getTruckDriverInfo();
		this.hiddenTripCodeOut = '';
	}

	isTripCodeFocus = false;
	focusTripCodeField() {
		(this.$refs["hiddenTripCodeOut"] as any).focus();
		(this.$refs["hiddenTruckDriverOutCardNumber"] as any).blur();
		this.isCardNumberFocus = false;
		this.isTripCodeFocus = true;
		this.$forceUpdate();
	}

	unfocusTripCodeField() {
		(this.$refs["hiddenTripCodeOut"] as any).blur();
		this.isTripCodeFocus = false;
		this.$forceUpdate();
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
			if(listData && listData.length > 0 && this.$router.currentRoute.path == '/truck-driver-out-monitoring'){
				await lineApi.GetLineBySerialNumber(listData[0].SerialNumber).then(async (res: any) => {
					if(res.data && res.data.LineForDriver && res.data.LineForDriverIssuanceReturnCard){
						await truckerDriverLogApi.GetCardNumberById(listData[0].EmployeeATID).then((cardRes: any) => {
							// console.log(cardRes)
							if(cardRes.data){
								if(this.extraDriverData && this.extraDriverData.length > 0 
									&& this.extraDriverData.some(y => y.LogInCardNumber == cardRes.data.CardNumber)
								){
									let extraDriver = this.extraDriverData.find(y => y.LogInCardNumber == cardRes.data.CardNumber);
									extraDriver.CardNumber = cardRes.data.CardNumber;
								}else if(cardRes.data.CardNumber != this.truckDriverInfo.LogInCardNumber){
									// this.truckDriverInfo.CardNumber = this.$t("CardNumberNotMatch").toString();
									this.$alert(
										this.$t("CardNumberNotMatch").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
									this.truckDriverInfo.CardNumber = null;
									this.truckDriverInfo.LogOutCheckTime = null;
									this.truckDriverInfo.LogOutCheckTimeString = null;
									this.truckDriverInfo.MachineSerial = null;
								}else if(cardRes.data.CardNumber == this.truckDriverInfo.LogInCardNumber 
									&& !this.truckDriverInfo.IsActive){
									// this.truckDriverInfo.CardNumber = this.$t("CardReturned").toString();
									this.$alert(
										this.$t("CardReturned").toString(),
										this.$t("Warning").toString(),
										{ type: "warning" }
									);
									this.truckDriverInfo.CardNumber = null;
									this.truckDriverInfo.LogOutCheckTime = null;
									this.truckDriverInfo.LogOutCheckTimeString = null;
									this.truckDriverInfo.MachineSerial = null;
								}else{
									this.truckDriverInfo.CardNumber = cardRes.data.CardNumber;
									this.truckDriverInfo.LogOutCheckTime = new Date(listData[0].CheckTime.toString());
									this.truckDriverInfo.LogOutCheckTimeString = moment(this.truckDriverInfo.LogOutCheckTime)
									.format("YYYY/MM/DD HH:mm:ss");
									this.truckDriverInfo.MachineSerial = listData[0].SerialNumber;
								}
							}else{
								// this.truckDriverInfo.CardNumber = this.$t("CardNumberNotExist").toString();
								this.$alert(
									this.$t("CardNumberNotExist").toString(),
									this.$t("Warning").toString(),
									{ type: "warning" }
								);
								this.truckDriverInfo.CardNumber = null;
								this.truckDriverInfo.LogOutCheckTime = null;
								this.truckDriverInfo.LogOutCheckTimeString = null;
								this.truckDriverInfo.MachineSerial = null;
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
                    .invoke("AddUserToGroup", 2, self.localStorage.getItem("user") + "_truckDriverOutMonitoring")
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

	async getTruckDriverInfo(){
		if(!this.truckDriverInfo.TripCode || this.truckDriverInfo.TripCode == ""){
			this.$alert(
                this.$t("PleaseInputTripCode").toString(),
                this.$t("Warning").toString(),
                { type: "warning" }
            );
			return;
		}
		this.isSavedLog = false;
		this.isException = false;
		const tripCode = Misc.cloneData(this.truckDriverInfo.TripCode);
		// this.truckDriverInfo = {TripCode: tripCode, IsException: false};
		await truckerDriverLogApi.IsTruckDriverIn(tripCode).then(async (checkExistIn: any) => {
			// console.log(checkExistIn)
			if(checkExistIn && checkExistIn.data){
				await truckerDriverLogApi.IsTruckDriverOut(tripCode).then(async (checkExistOut: any) => {
					// console.log(checkExistOut)
					if(checkExistOut && !checkExistOut.data){
						await truckerDriverLogApi.GetTruckDriverInfoByTripCode(tripCode).then((res: any) => {
							// console.log(res)
							if(res.data){
								this.truckDriverInfo = {};
								this.truckDriverInfo.TripCode = res.data.TripId;
								this.truckDriverInfo.FullName = res.data.DriverName;
								this.truckDriverInfo.VehiclePlate = res.data.TrailerNumber;
								this.truckDriverInfo.CompanyName = res.data.CompanyName;
								this.truckDriverInfo.Supplier = res.data.Supplier;
								this.truckDriverInfo.OrderCode = res.data.OrderCode;
								this.truckDriverInfo.NRIC = res.data.DriverCode;
								this.truckDriverInfo.IsActive = res.data.IsActive;
								this.truckDriverInfo.DeliveryPoint = res.data.LocationFrom;
								this.truckDriverInfo.Phone = res.data.DriverPhone;
								this.truckDriverInfo.PassingVehicle = res.data.Vc;
								this.truckDriverInfo.PassingVehicleName = res.data.Vc ? "VC" : "";
								this.truckDriverInfo.LogInCardNumber = res.data.CardNumber;
								if(res.data.Eta){
									this.truckDriverInfo.DeliveryTime = res.data.Eta;
									this.truckDriverInfo.DeliveryTimeString = moment(this.truckDriverInfo.DeliveryTime)
									.format("DD/MM/YYYY HH:mm:ss");
									this.truckDriverInfo.DeliveryTimeDayString = moment(this.truckDriverInfo.DeliveryTime)
									.format("DD/MM/YYYY");
									this.truckDriverInfo.DeliveryTimeHourString = moment(this.truckDriverInfo.DeliveryTime)
									.format("HH:mm");
								}
								if(res.data.TimeIn){
									this.truckDriverInfo.GateEntryTime = res.data.TimeIn;
									this.truckDriverInfo.GateEntryTimeString = moment(this.truckDriverInfo.GateEntryTime)
									.format("DD/MM/YYYY HH:mm:ss");
									this.truckDriverInfo.GateEntryTimeDateString = moment(this.truckDriverInfo.GateEntryTime)
									.format("DD/MM/YYYY");
									this.truckDriverInfo.GateEntryTimeHourString = moment(this.truckDriverInfo.GateEntryTime)
									.format("HH:mm");
								}
								if(res.data.TimeOut){
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
									this.extraDriverData.forEach(element => {
										element.LogInCardNumber = Misc.cloneData(element.CardNumber);
										element.CardNumber = null;
									});
								}
								// console.log(this.truckDriverInfo)
								this.$forceUpdate();
							}
						});
					}else{
						this.$alert(
							this.$t("TripCodeExpired").toString(),
							this.$t("Warning").toString(),
							{ type: "warning" }
						);
						this.truckDriverInfo = {};
						this.extraDriverData = [];
					}
				});
			}else{
				this.$alert(
					this.$t("LogInNotExist").toString(),
					this.$t("Warning").toString(),
					{ type: "warning" }
				);
				this.truckDriverInfo = {};
				this.extraDriverData = [];
			}
		});
	}

	hiddenCardNumber = '';
	isCardNumberFocus = false;
	focusCardNumberField(){
		setTimeout(() => {
			(this.$refs["hiddenTripCodeOut"] as any).blur();
			(this.$refs["hiddenTruckDriverOutCardNumber"] as any).focus();
			this.isCardNumberFocus = true;
			this.$forceUpdate();
		}, 500);
	}
	unfocusCardNumberField(){
		(this.$refs["hiddenTruckDriverOutCardNumber"] as any).blur();
		this.isCardNumberFocus = false;
		this.$forceUpdate();
	}
	async setCardNumber(){
		if(this.hiddenCardNumber && this.hiddenCardNumber != ''){
			const cloneCardNumber = Misc.cloneData(this.hiddenCardNumber);
			this.hiddenCardNumber = null;
			await truckerDriverLogApi.GetCardNumberByNumber(cloneCardNumber).then((cardRes: any) => {
				// console.log(cardRes)
				if(cardRes.data){
					if(this.extraDriverData && this.extraDriverData.length > 0 
						&& this.extraDriverData.some(y => y.LogInCardNumber == cardRes.data.CardNumber)
					){
						let extraDriver = this.extraDriverData.find(y => y.LogInCardNumber == cardRes.data.CardNumber);
						extraDriver.CardNumber = cardRes.data.CardNumber;
					}else if(cardRes.data.CardNumber != this.truckDriverInfo.LogInCardNumber){
						// this.truckDriverInfo.CardNumber = this.$t("CardNumberNotMatch").toString();
						this.$alert(
							this.$t("CardNumberNotMatch").toString(),
							this.$t("Warning").toString(),
							{ type: "warning" }
						);
						this.truckDriverInfo.CardNumber = null;
						this.truckDriverInfo.LogOutCheckTime = null;
						this.truckDriverInfo.LogOutCheckTimeString = null;
						this.truckDriverInfo.MachineSerial = null;
					}else if(cardRes.data.CardNumber == this.truckDriverInfo.LogInCardNumber 
						&& !this.truckDriverInfo.IsActive){
						// this.truckDriverInfo.CardNumber = this.$t("CardReturned").toString();
						this.$alert(
							this.$t("CardReturned").toString(),
							this.$t("Warning").toString(),
							{ type: "warning" }
						);
						this.truckDriverInfo.CardNumber = null;
						this.truckDriverInfo.LogOutCheckTime = null;
						this.truckDriverInfo.LogOutCheckTimeString = null;
						this.truckDriverInfo.MachineSerial = null;
					}else{
						this.truckDriverInfo.CardNumber = cardRes.data.CardNumber;
						// this.truckDriverInfo.LogOutCheckTime = new Date(listData[0].CheckTime.toString());
						// this.truckDriverInfo.LogOutCheckTimeString = moment(this.truckDriverInfo.LogOutCheckTime)
						// .format("YYYY/MM/DD HH:mm:ss");
						// this.truckDriverInfo.MachineSerial = listData[0].SerialNumber;
						this.truckDriverInfo.LogOutCheckTime = new Date(new Date());
						this.truckDriverInfo.LogOutCheckTimeString = moment(new Date())
						.format("YYYY/MM/DD HH:mm:ss");
						this.truckDriverInfo.MachineSerial = cloneCardNumber;
					}
				}else{
					// this.truckDriverInfo.CardNumber = this.$t("CardNumberNotExist").toString();
					this.$alert(
						this.$t("CardNumberNotExist").toString(),
						this.$t("Warning").toString(),
						{ type: "warning" }
					);
					this.truckDriverInfo.CardNumber = null;
					this.truckDriverInfo.LogOutCheckTime = null;
					this.truckDriverInfo.LogOutCheckTimeString = null;
					this.truckDriverInfo.MachineSerial = null;
				}
				this.$forceUpdate();
				// console.log(this.truckDriverInfo)
			})
		}
	}

	isSavedLog = false;
	isException = false;
	async saveLog(){
		// console.log(this.truckDriverInfo.TripCode)
		// console.log(this.truckDriverInfo.CardNumber)
		if(!this.truckDriverInfo.TripCode || this.truckDriverInfo.TripCode == ""){
			this.$alert(
                this.$t("PleaseInputTripCode").toString(),
                this.$t("Warning").toString(),
                { type: "warning" }
            );
			return;
		}
		if(!this.isException){
			if(!this.truckDriverInfo.CardNumber || this.truckDriverInfo.CardNumber == ""){
				this.$alert(
					this.$t("PleaseScanCard").toString(),
					this.$t("Warning").toString(),
					{ type: "warning" }
				);
				return;
			}
			if(this.truckDriverInfo.CardNumber == this.$t("CardNumberNotMatch").toString()
			|| this.truckDriverInfo.CardNumber == this.$t("CardNumberNotExist").toString()
			|| this.truckDriverInfo.CardNumber == this.$t("CardReturned").toString()){
				this.$alert(
					this.$t(this.truckDriverInfo.CardNumber).toString(),
					this.$t("Warning").toString(),
					{ type: "warning" }
				);
				return;
			}
			if(this.extraDriverData && this.extraDriverData.length > 0 && this.extraDriverData.some(x => 
				x.CardNumber != x.LogInCardNumber
			)){
				this.$alert(
					this.$t('PleaseScanCardForExtraDriver').toString(),
					this.$t("Warning").toString(),
					{ type: "warning" }
				);
				return;
			}
		}else if(this.isException 
			&& (!this.truckDriverInfo.ReasonException || this.truckDriverInfo.ReasonException == '' 
				|| !this.truckDriverInfo.ReasonException.trim().length
			)){
				this.$alert(
					this.$t('PleaseInputExceptionReason').toString(),
					this.$t("Warning").toString(),
					{ type: "warning" }
				);
				return;
		}
		
		var param = {
			TripCode: this.truckDriverInfo.TripCode,
			CardNumber: this.truckDriverInfo.CardNumber,
			InOutMode: InOutMode.Output,
			TimeString: this.truckDriverInfo?.LogOutCheckTimeString ?? null,
			MachineSerial: this.truckDriverInfo.MachineSerial,
			IsException: this.isException,
			ReasonException: this.truckDriverInfo.ReasonException
		};
		await truckerDriverLogApi.IsTruckDriverIn(this.truckDriverInfo.TripCode).then(async (checkExistRes: any) => {
			// console.log(checkExistRes)
			if(checkExistRes && checkExistRes.data){
				await truckerDriverLogApi.SaveTruckDriverLog(param).then((res: any) => {
					// console.log(res)
					if(res.data){
						this.isSavedLog = true;
						this.$saveSuccess();
						this.truckDriverInfo.GateExitTime = new Date(res.data.toString());
						this.truckDriverInfo.GateExitTimeString = moment(this.truckDriverInfo.GateExitTime)
						.format("DD/MM/YYYY HH:mm:ss");
						this.truckDriverInfo.GateExitTimeDateString = moment(this.truckDriverInfo.GateExitTime)
						.format("DD/MM/YYYY");
						this.truckDriverInfo.GateExitTimeHourString = moment(this.truckDriverInfo.GateExitTime)
						.format("HH:mm");
					}else{
						this.$saveError();
					}
					this.$forceUpdate();
				});
			}else{
				this.$alert(
					this.$t("LogInNotExist").toString(),
					this.$t("Warning").toString(),
					{ type: "warning" }
				);
			}
		}).finally(() => {
			// this.truckDriverInfo = {};
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

	cancel(){
		this.isSavedLog = false;
		this.isException = false;
		this.truckDriverInfo = {};
		this.extraDriverData = [];
		this.focusTripCodeField();
	}
}
