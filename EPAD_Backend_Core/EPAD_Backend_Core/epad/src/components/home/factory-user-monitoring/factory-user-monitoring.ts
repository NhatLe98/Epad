import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import axios from "axios";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';

import { configApi } from "@/$api/config-api";
import { attendanceLogApi } from "@/$api/attendance-log-api";

import moment from "moment";
import { rootLink } from "../../../constant/variable";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import XLSX from 'xlsx';

import EmployeePresentByUserTypeBarChart2 from 
"@/components/home/dashboard-component/custom-chart/employee-present-by-user-type-bar-chart-component-2.vue";
import EmergencyLogPieChart from 
"@/components/home/dashboard-component/custom-chart/emergency-log-pie-chart-component.vue";
import EmergencyLogBarChart from 
"@/components/home/dashboard-component/custom-chart/emergency-log-bar-chart-component.vue";
import VehicleInOutBarChart from 
"@/components/home/dashboard-component/custom-chart/vehicle-in-out-bar-chart-component.vue";
import { realtimeApi } from "@/$api/realtime-api";

@Component({
    name: 'factory-user-monitoring',
    components: {HeaderComponent, DataTableComponent, DataTableFunctionComponent,
        EmployeePresentByUserTypeBarChart2,
        EmergencyLogPieChart,
        EmergencyLogBarChart,
        VehicleInOutBarChart
    },
})
export default class FactoryUserMonitoring extends Vue {
    strDateNow = "";
    strTimeNow = "";

    truckDriverLogs = {Item1: [],Item2:[],Item3:[],Item4:[]};

    totalEmployeePresent = 0;
    totalEmergencyPresent = 0;
    totalVehicleInNotOut = 0;
    totalVehicleInNotOutToday = 0;
    dataEmployeePresent;
    dataEmergencyPresent;
    dataVehicleInNotOut;
    emergencyPresentDataValues = [];
    emergencyPresentDataChunks = [];
    totalVehicle = 0;

    realtimeConnection: signalR.HubConnection;
    realtimeConnected = false;
    realtimeServer = "";

    selectUserTypeOption = [
        { value: 1, label: 'Employee' },
        { value: 2, label: 'Customer' },
        { value: 3, label: 'Student' },
        { value: 4, label: 'Parent' },
        { value: 5, label: 'Nanny' },
        { value: 6, label: 'Contractor' },
        { value: 7, label: 'Teacher' },
        { value: 8, label: 'Driver' },
    ];

    titleExplode = '';
    isShowExplode = false;
    explodeColumns = [];
    explodeData = [];
    isErrorExplodeData = false;

    employeeCount = 0;
    customerCount = 0;
    contractorCount = 0;

    differentNumber: number;

    maxHeight = (window.innerHeight / 100 * 55);

    isFullscreenOn: boolean = false; 
    
    reload = false;
    loadTruckDriverLog = false;

    excludeVehicle = false;

    reloadData(){
        this.reload = true;
        this.$nextTick(() => {
            this.reload = false;
        });
        this.getTruckDriverLogData();
    }
    
    showEmployeePresentData(parentElement){
        const s = document.getElementById('employeePresentWrapper');
        this.titleExplode = this.$t('InfoEmployeePresentInFactory').toString();
        this.explodeColumns = [
            {
                prop: 'EmployeeATID',
                label: this.$t('ATIDOrUserIDOrTripCode'),
            },
            // {
            //     prop: 'EmployeeCode',
            //     label: this.$t('EmployeeCode'),
            // },
            {
                prop: 'FullName',
                label: this.$t('FullName'),
            },
            {
                prop: 'DepartmentName',
                label: this.$t('DepartmentOrPart'),
            },
            {
                prop: 'GateName',
                label: this.$t('Gate'),
            },
            {
                prop: 'UserTypeName',
                label: this.$t('Object'),
            }
            // {
            //     prop: 'DeviceName',
            //     label: this.$t('DeviceName'),
            // },
            // {
            //     prop: 'TimeString',
            //     label: this.$t('TimeString'),
            // },
            // {
            //   prop: 'InOutMode',
            //   label: this.$t('InOutMode'),
            // },
            // {
            //   prop: 'VerifyMode',
            //   label: this.$t('VerifyMode'),
            // },
        ];
        let explodeData = [];
        if(parentElement.includes(s)){
            explodeData = explodeData.concat(this.dataEmployeePresent.Item2);
            explodeData = explodeData.concat(this.dataEmployeePresent.Item3);
            explodeData = explodeData.concat(this.dataEmployeePresent.Item4);
            explodeData = explodeData.concat(this.dataEmployeePresent.Item5);
            explodeData = explodeData.concat(this.dataEmployeePresent.Item6);
            if(this.dataEmployeePresent.Item7){
                explodeData = explodeData.concat(this.dataEmployeePresent.Item7);
            }
        }else{
            explodeData = explodeData.concat(this.dataEmployeePresent.Item1);
            explodeData = explodeData.concat(this.dataEmployeePresent.Item2);
            explodeData = explodeData.concat(this.dataEmployeePresent.Item3);
            explodeData = explodeData.concat(this.dataEmployeePresent.Item4);
            explodeData = explodeData.concat(this.dataEmployeePresent.Item5);
        }
       
        this.explodeData = Misc.cloneData(explodeData);
        this.explodeData = this.explodeData.filter(x => !Misc.isNullOrUndefined(x));
        if(this.explodeData && this.explodeData.length > 0){
            this.explodeData.forEach((element, index) => {
                
                element.DepartmentName = element.DepartmentName ? this.$t(element.DepartmentName) : '';
                element.GateName = element.GateName ? element.GateName.join(', ') : '';
            });
        }
        this.handleOpenDialog();
    }

    showInEmergencyPresentData(){
        this.titleExplode = this.$t('InfoEmployeePresent').toString();
        this.explodeColumns = [
            {
                prop: 'EmployeeATID',
                label: this.$t('ATIDOrUserIDOrTripCode'),
            },
            // {
            //     prop: 'EmployeeCode',
            //     label: this.$t('EmployeeCode'),
            // },
            {
                prop: 'FullName',
                label: this.$t('FullName'),
            },
            {
                prop: 'DepartmentName',
                label: this.$t('DepartmentOrPart'),
            },
            {
                prop: 'UserTypeName',
                label: this.$t('Object'),
            },
            {
                prop: 'DeviceName',
                label: this.$t('FocusPoint'),
            }
            // {
            //     prop: 'DeviceName',
            //     label: this.$t('DeviceName'),
            // },
            // {
            //     prop: 'TimeString',
            //     label: this.$t('TimeString'),
            // },
            // {
            //   prop: 'InOutMode',
            //   label: this.$t('InOutMode'),
            // },
            // {
            //   prop: 'VerifyMode',
            //   label: this.$t('VerifyMode'),
            // },
        ];
        let explodeData = [];
        explodeData = explodeData.concat(this.dataEmergencyPresent);
        this.explodeData = Misc.cloneData(explodeData);
        this.explodeData = this.explodeData.filter(x => !Misc.isNullOrUndefined(x));
        if(this.explodeData && this.explodeData.length > 0){
            this.explodeData.forEach((element, index) => {
                element.DepartmentName = element.DepartmentName ? this.$t(element.DepartmentName) : '';
                element.GateName = element.GateName ? element.GateName.join(', ') : '';
            });
        }
        this.handleOpenDialog();
    }

    showNotInEmergencyPresentData(){
        this.titleExplode = this.$t('InfoEmployeeNotPresent').toString();
        this.explodeColumns = [
            {
                prop: 'EmployeeATID',
                label: this.$t('ATIDOrUserIDOrTripCode'),
            },
            // {
            //     prop: 'EmployeeCode',
            //     label: this.$t('EmployeeCode'),
            // },
            {
                prop: 'FullName',
                label: this.$t('FullName'),
            },
            {
                prop: 'DepartmentName',
                label: this.$t('DepartmentOrPart'),
            },
            {
                prop: 'UserTypeName',
                label: this.$t('Object'),
            },
            {
                prop: 'GateName',
                label: this.$t('LastPosition'),
            }
            // {
            //     prop: 'DeviceName',
            //     label: this.$t('DeviceName'),
            // },
            // {
            //     prop: 'TimeString',
            //     label: this.$t('TimeString'),
            // },
            // {
            //   prop: 'InOutMode',
            //   label: this.$t('InOutMode'),
            // },
            // {
            //   prop: 'VerifyMode',
            //   label: this.$t('VerifyMode'),
            // },
        ];
        let explodeData = [];
        explodeData = explodeData.concat(this.dataEmployeePresent.Item1);
        explodeData = explodeData.concat(this.dataEmployeePresent.Item2);
        explodeData = explodeData.concat(this.dataEmployeePresent.Item3);
        explodeData = explodeData.concat(this.dataEmployeePresent.Item4);
        explodeData = explodeData.concat(this.dataEmployeePresent.Item5);
        this.explodeData = Misc.cloneData(explodeData);
        this.explodeData = this.explodeData.filter(x => !Misc.isNullOrUndefined(x));
        if(this.explodeData && this.explodeData.length > 0){
            this.explodeData.forEach((element, index) => {
                element.DepartmentName = element.DepartmentName ? this.$t(element.DepartmentName) : '';
                element.GateName = element.GateName ? element.GateName.join(', ') : '';
            });
            this.explodeData = this.explodeData.filter(x => !this.dataEmergencyPresent 
                || this.dataEmergencyPresent.length == 0 
                || !this.dataEmergencyPresent.some(y => y.EmployeeATID == x.EmployeeATID 
                    && y.CardNumber == x.CardNumber));
        }
        this.isErrorExplodeData = true;
        this.handleOpenDialog();
    }

    showTruckPresentData(){
        this.titleExplode = this.$t('InfoTruck').toString();
        this.explodeColumns = [
            {
                prop: 'EmployeeATID',
                label: this.$t('TripCode'),
            },
            // {
            //     prop: 'EmployeeCode',
            //     label: this.$t('EmployeeCode'),
            // },
            {
                prop: 'FullName',
                label: this.$t('DriverFullName'),
            },
            {
                prop: 'DepartmentName',
                label: this.$t('UnitOrCompany'),
            },
            {
                prop: 'Plate',
                label: this.$t('Plate'),
            },
            {
                prop: 'TimeString',
                label: this.$t('TimeIn'),
            }
            // {
            //     prop: 'DeviceName',
            //     label: this.$t('DeviceName'),
            // },
            // {
            //     prop: 'TimeString',
            //     label: this.$t('TimeString'),
            // },
            // {
            //   prop: 'InOutMode',
            //   label: this.$t('InOutMode'),
            // },
            // {
            //   prop: 'VerifyMode',
            //   label: this.$t('VerifyMode'),
            // },
        ];
        let explodeData = [];
        explodeData = explodeData.concat(this.truckDriverLogs.Item4);
        this.explodeData = Misc.cloneData(explodeData);
        this.explodeData = this.explodeData.filter(x => !Misc.isNullOrUndefined(x));
        if(this.explodeData && this.explodeData.length > 0){
            this.explodeData.forEach((element, index) => {
                element.DepartmentName = element.DepartmentName ? this.$t(element.DepartmentName) : '';
                element.GateName = element.GateName ? element.GateName.join(', ') : '';
            });
        }
        this.handleOpenDialog();
    }

    showIntegratedVehiclePresentData(){
        this.titleExplode = this.$t('InfoBikeBicycle').toString();
        this.explodeColumns = [
            {
                prop: 'EmployeeATID',
                label: this.$t('ATIDOrUserID'),
            },
            // {
            //     prop: 'EmployeeCode',
            //     label: this.$t('EmployeeCode'),
            // },
            {
                prop: 'FullName',
                label: this.$t('FullName'),
            },
            {
                prop: 'DepartmentName',
                label: this.$t('UnitOrCompany'),
            },
            {
                prop: 'UserTypeName',
                label: this.$t('Object'),
            },
            {
                prop: 'Plate',
                label: this.$t('Plate'),
            },
            {
                prop: 'TimeString',
                label: this.$t('TimeIn'),
            }
            // {
            //     prop: 'DeviceName',
            //     label: this.$t('DeviceName'),
            // },
            // {
            //     prop: 'TimeString',
            //     label: this.$t('TimeString'),
            // },
            // {
            //   prop: 'InOutMode',
            //   label: this.$t('InOutMode'),
            // },
            // {
            //   prop: 'VerifyMode',
            //   label: this.$t('VerifyMode'),
            // },
        ];
        let explodeData = [];
        explodeData = explodeData.concat(this.dataVehicleInNotOut.Item4);
        this.explodeData = Misc.cloneData(explodeData);
        this.explodeData = this.explodeData?.filter(x => !Misc.isNullOrUndefined(x)) ?? [];
        if(this.explodeData && this.explodeData.length > 0){
            this.explodeData.forEach((element, index) => {
                element.DepartmentName = element.DepartmentName ? this.$t(element.DepartmentName) : '';
                element.GateName = element.GateName ? element.GateName.join(', ') : '';
            });
        }
        this.handleOpenDialog();
    }

    handleCloseDialog(){
        this.isShowExplode = false;
        this.titleExplode = '';
        this.explodeColumns = [];
        this.explodeData = [];
        this.isErrorExplodeData = false;
    }

    handleOpenDialog(){
        this.isShowExplode = true;
    }

    exportToExcel() {
        const data = this.explodeData;
        let formatData = [];
        if (data && data.length) {
            for (let i = 0; i < data.length; i++) {
                let obj = {};
                this.explodeColumns.forEach(element => {
                    const key = this.$t(element.prop).toString();
                    if (!obj[key]) {
                        obj[key] = []
                    }
                    obj[key] = data[i][element.prop];
                });
                formatData.push(obj);
            }
        }
        // console.log(data)
        // export json to Worksheet of Excel
        // only array possible
        var dataWs = XLSX.utils.json_to_sheet(formatData)

        let cellsWidth = [];
        this.explodeColumns.forEach(element => {
            cellsWidth.push({ width: 30 });
        });
        dataWs['!cols'] = cellsWidth;

        // A workbook is the name given to an Excel file
        var wb = XLSX.utils.book_new() // make Workbook of Excel

        // add Worksheet to Workbook
        // Workbook contains one or more worksheets
        XLSX.utils.book_append_sheet(wb, dataWs, "ExportData") // sheetAName is name of Worksheet

        // export Excel file
        XLSX.writeFile(wb, 'ExportChartData.xlsx') // name of the file is 'book.xlsx'
    }

    toggleFullScreen(): void {
        // // // console.log("fullChange")
        if (this.isFullScreen()) {
            this.exitFullScreen();
            this.isFullscreenOn = false;
        } else {
            this.enterFullScreen();
            this.isFullscreenOn = true;
        }
    }

    isFullScreen(): boolean {
        return !!(
            document.fullscreenElement ||
            document.webkitFullscreenElement ||
            document.mozFullScreenElement ||
            document.msFullscreenElement
        );
    }

    enterFullScreen(): void {
        const el = document.documentElement as any;
        if (el.requestFullscreen) {
            el.requestFullscreen();
        } else if (el.webkitRequestFullscreen) {
            el.webkitRequestFullscreen();
        } else if (el.mozRequestFullScreen) {
            el.mozRequestFullScreen();
        } else if (el.msRequestFullscreen) {
            el.msRequestFullscreen();
        }
    }

    exitFullScreen(): void {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        } else if (document.mozCancelFullScreen) {
            document.mozCancelFullScreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        }
    }

    MaxHeightTable() {
        this.maxHeight = window.innerHeight - 210;
    }

    handleClick(){

    }

    setTotalEmployeePresent(value){
        this.totalEmployeePresent = value;
    }

    setEmergencyDataValues(ids, labels, values, colors){
        // console.log(ids, labels, values, colors)
        this.totalEmergencyPresent = 0;
        this.emergencyPresentDataValues = [];
        this.emergencyPresentDataChunks = [];
        ids.forEach((element, index) => {
            this.emergencyPresentDataValues.push({
                id: element,
                label: labels[index],
                value: values[index],
                color: colors != null && colors.length > 0 ? colors[index] : null
            });
            
            this.totalEmergencyPresent += values[index];
        });
        console.log(this.totalEmergencyPresent)

        const chunkSize = 4;
        for (let i = 0; i < this.emergencyPresentDataValues.length; i += chunkSize) {
            this.emergencyPresentDataChunks.push(this.emergencyPresentDataValues.slice(i, i + chunkSize));
        }
    }

    setTotalVehicleInNotOut(value){
        this.totalVehicleInNotOut = value;
    }

    setTotalVehicleInNotOutToday(value){
        this.totalVehicleInNotOutToday = value;
    }

    setDataEmployeePresent(data){
        this.dataEmployeePresent = data;
    }

    setDataEmergencyPresent(data){
        this.dataEmergencyPresent = data;
    }

    setDataVehicleInNotOut(data){
        this.dataVehicleInNotOut = data;
    }

    formatNull(value){
        return null;
    }

    async beforeMount() {
        window.addEventListener("resize", () => {
            this.MaxHeightTable();
        });

        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.excludeVehicle = x.ExcludeVehicleFactoryUserMonitoring;

        })

        await this.getDiffFromServerTimeAndClientTime();
        await this.getTruckDriverLogData();
    }

    setInterval: ReturnType<typeof setInterval> = setInterval(() => {
		if (this.$route.path == "/factory-user-monitoring") {
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
      
    async getTruckDriverLogData(){
        this.loadTruckDriverLog = true;
        await attendanceLogApi.GetTruckDriverLog().then((res: any) => {
            if(res.data){
                this.truckDriverLogs = res.data;
            }
            this.$nextTick(() => {
                this.loadTruckDriverLog = false;
            });
        });
    }

    reloadInterval = null;
    mounted() {
        this.getRealTimeServer();
        this.reloadInterval = setInterval(() => {
            if(!this.loadTruckDriverLog){
                this.getTruckDriverLogData();
            }
        }, 60000); 

        const thisVue = this;
        setTimeout(() => {
            const parentElement = document.getElementById('employeePresentWrapper');
            const childButtons1 = document.querySelectorAll('.inEmergencyPresent');
            const childButtons2 = document.querySelectorAll('.notInEmergencyPresent');
            const parentElement1 = document.getElementById('truckPresentWrapper');
            const parentElement2 = document.getElementById('integratedVehiclePresentWrapper');
            const buttonElement = document.getElementById('clearEmergencyBtn');
            // console.log(document.getElementById('employeePresentWrapper'))
            document.addEventListener('click', function(event) {
                const path = event.composedPath();
                if(!path.includes(buttonElement)){
                    if (path.includes(parentElement)) {
                        thisVue.showEmployeePresentData(path);
                    }
                    childButtons1.forEach((button) => {
                        if (path.includes(button)) {
                            if(!thisVue.excludeVehicle){
                                thisVue.showInEmergencyPresentData();
                            }else{

                            }
                        }
                    })
                    childButtons2.forEach((button) => {
                        if (path.includes(button)) {
                            thisVue.showNotInEmergencyPresentData();
                        }
                    })
                    if (path.includes(parentElement1)) {
                        thisVue.showTruckPresentData();
                    }
                    if (path.includes(parentElement2)) {
                        thisVue.showIntegratedVehiclePresentData();
                    }
                }
            });
        }, 2000);
    }

    updated(){
        document.removeEventListener('click', null);
        const thisVue = this;
        setTimeout(() => {
            const parentElement = document.getElementById('employeePresentWrapper');
            const childButtons1 = document.querySelectorAll('.inEmergencyPresent');
            const childButtons2 = document.querySelectorAll('.notInEmergencyPresent');
            const parentElement1 = document.getElementById('truckPresentWrapper');
            const parentElement2 = document.getElementById('integratedVehiclePresentWrapper');
            // console.log(document.getElementById('employeePresentWrapper'))
            document.addEventListener('click', function(event) {
                const path = event.composedPath();
                if (path.includes(parentElement)) {
                    thisVue.showEmployeePresentData(path);
                }
                childButtons1.forEach((button) => {
                    if (path.includes(button)) {
                        thisVue.showInEmergencyPresentData();
                    }
                })
                childButtons2.forEach((button) => {
                    if (path.includes(button)) {
                        thisVue.showNotInEmergencyPresentData();
                    }
                })
                if (path.includes(parentElement1)) {
                    thisVue.showTruckPresentData();
                }
                if (path.includes(parentElement2)) {
                    thisVue.showIntegratedVehiclePresentData();
                }
            });
        }, 2000);
    }

    beforeUnmount(){
        clearInterval(this.reloadInterval);
    }

    created() {
        
    }

    format(value){
        return '';
    }

    async getListData() {
        
    }

    

    getRealTimeServer() {
        configApi.GetRealTimeServerLink().then((res: any) => {
            // // // // console.log(res)
            this.connectToRealTimeServer(res.data);
            // const startOfDay = new Date();
            // startOfDay.setHours(0,0,0);
            setInterval(() => {
                const clientTimeSpan = new Date().getTime();
                const now = new Date(clientTimeSpan + this.differentNumber);
                this.strDateNow = moment(now).format("DD/MM/YYYY");
                this.strTimeNow = moment(now).format("HH:mm:ss");

                // if(now.getHours() == startOfDay.getHours() && now.getMinutes() == startOfDay.getMinutes() 
                //     && now.getSeconds() == startOfDay.getSeconds()){
                //         // Reload log data
                // }
            }, 500);
        });
    }

    async getDiffFromServerTimeAndClientTime() {
        attendanceLogApi.GetSystemDateTime().then((res: any) => {
            // // // // console.log(res)
            const systemDateTime: Date = new Date(res.data);
            const clientDateTime = new Date();
            this.differentNumber = systemDateTime.getTime() - clientDateTime.getTime();
        });
    }

    connectToRealTimeServer(link) {
        // // // console.log(link)
        this.realtimeConnection = new HubConnectionBuilder()
            .withUrl(link + "/attendanceHub")
            .configureLogging(LogLevel.Information)
            .build();
        const data = this.realtimeConnection;
        // const notify = this.$notify;

        // receive data from server
        const thisVue = this;

        this.realtimeConnection.on("ReceiveTimeLog", data => {
            // this.handlerDataReceive(data);
        });

        this.realtimeConnection.on("ReceiveUpdateLog", async err => {
            // await this.getListData();
        });
        this.realtimeConnection
            .start()
            .then(() => {
                // // // // console.log("connection started")
                thisVue.realtimeConnected = true;
                data
                    .invoke("AddUserToGroup", 2, self.localStorage.getItem("user"))
                    .catch(err => {
                        // // // console.log(err.toString());
                    });
            })
            .catch(err => {
                thisVue.realtimeConnected = false;
                // // // console.log(err.toString());
            });
        this.realtimeConnection.onclose(() => {
            thisVue.realtimeConnected = false;
        });
    }

    handlerDataReceive(data) {
        
    }

    isUTC(date) {
        return date.endsWith('Z');
    }
}
