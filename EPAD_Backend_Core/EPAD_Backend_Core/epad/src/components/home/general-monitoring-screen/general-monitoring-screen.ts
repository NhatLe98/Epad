import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import axios from "axios";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';

import { configApi } from "@/$api/config-api";
import { attendanceLogApi } from "@/$api/attendance-log-api";
import { departmentApi, departmentAndDeviceApi } from "@/$api/department-api";
import { employeeInfoApi } from "@/$api/employee-info-api";
import { hrCustomerInfoApi } from "@/$api/hr-customer-info-api";
import { timeLogApi } from '@/$api/gc_timelog-api';
import { listViolationApi } from '@/$api/gc-list-violation-api';
import { deviceApi } from '@/$api/device-api';
import { privilegeMachineRealtimeApi } from '@/$api/privilege-machine-realtime-api';
import { groupDeviceApi } from '@/$api/group-device-api';

import moment from "moment";
import { rootLink } from "../../../constant/variable";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { realtimeApi } from "@/$api/realtime-api";


type DepartmentLogDataItem = {
    Employees: EmployeeLog[];
    SerialNumber: any[];
    DepartmentIndex: any;
    Department: string;
}

type GroupDeviceLogDataItem = {
    Employees: EmployeeLog[];
    SerialNumber: any[];
    GroupDeviceIndex: any;
    GroupDeviceName: string;
}

type TableDataItem = {
    Index: number;
    Employees: EmployeeLog[];
    Department: string;
}

type EmployeeLog = {
    EmployeeATID: string;
    DepartmentIndex: number;
    Department: string;
    FullName: string;
    Position: string;
    Time: string;
    Error: string;
    IsCustomer?: boolean;

}

@Component({
    name: 'general-monitoring-screen',
    components: {HeaderComponent, DataTableComponent, DataTableFunctionComponent},
})
export default class GeneralMonitoringScreen extends Vue {
    strNow = "";
    tblData: TableDataItem[] = [];
    get totalEmployee() {
        return this.tblData.flatMap(x => x.Employees).filter(x => !x.IsCustomer).length;
    }
    tblDataDetail = [];
    differentNumber: number;
    dialogEmployeeInfo = false;
    dialogLoading = false;

    realtimeConnection: signalR.HubConnection;
    realtimeConnected = false;
    realtimeServer = "";

    selectedDepartmentName = "";
    selectedNumberOfEmployee = 0;
    visitorDeptIndex = 0;
    maxAttendanceTime = 0;
    listFullCustomerData = {};
    listAllowShowRealtime = [
        "EmployeeInLeaveTime",
        "EmployeeInMissionTime",
        "EmployeeInStoppedWorkingTime",
        "AreaNotAllowed",
        "ExceedNumberOfAccess",
        "EmployeeNotInAccessGroup"
    ];

    employeeFullLookup: any[];
    employeeDepartment: any[];

    maxHeight = (window.innerHeight / 100 * 55);

    isFullscreenOn: boolean = false;   

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

    async beforeMount() {
        window.addEventListener("resize", () => {
            this.MaxHeightTable();
        });

        await this.getDiffFromServerTimeAndClientTime();
    }

    setInterval: ReturnType<typeof setInterval> = setInterval(() => {
		if (this.$route.path == "/general-monitoring-screen") {
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

    async mounted() {
        await departmentApi.GetAll().then((res: any) => {
            const { data } = res as any;
            let arr = JSON.parse(JSON.stringify(data));
            for (let i = 0; i < arr.Value.length; i++) {
                arr.Value[i].value = parseInt(arr.Value[i].value);
            }
            this.employeeDepartment = arr.Value;
            this.generateDepartmentData();
        });

        await employeeInfoApi.GetEmployeeLookup().then((res: any) => {
            if(res && res.data){
                this.employeeFullLookup = res.data;
            }
        });


        this.getFullListCustomerData().then(() => {
            this.getListData();
        });

        this.getRealTimeServer();
    }

    created() {
        
    }

    async getListData() {
        const data = await timeLogApi.beginGetLogInGateMandatory();
        const arrTemp: EmployeeLog[] = [];
        data.forEach(element => {
            if (element.CustomerIndex > 0) {
                const customer = this.listFullCustomerData[element.CustomerIndex] || {};
                arrTemp.push({
                    EmployeeATID: customer.ATID,
                    DepartmentIndex: this.visitorDeptIndex,
                    Department: "Visitor",
                    Position: "",
                    FullName: customer.Name,
                    Time: moment(element.Time).format('DD-MM-YYYY - HH:mm:ss'),
                    Error: element.Error,
                    IsCustomer: true,
                });
            } else {
                const employee = this.employeeFullLookup.find(x => x.EmployeeATID == element.EmployeeATID) || {};
                arrTemp.push({
                    DepartmentIndex: employee.DepartmentIndex,
                    Department: employee.Department,
                    Position: employee.PositionName,
                    FullName: employee.FullName,
                    Time: moment(element.Time).format('DD-MM-YYYY - HH:mm:ss'),
                    Error: element.Error,
                    EmployeeATID: element.EmployeeATID,
                });

            }

        });
        this.tblData.map(x => x.Employees).flat().filter(x => x.Position)
        this.tblData.forEach(left => {
            left.Employees = [];
            arrTemp.forEach(element => {
                if (left.Index == element.DepartmentIndex) {
                    const existedEmpIdx = left.Employees.findIndex(x => x.EmployeeATID == element.EmployeeATID);
                    if (existedEmpIdx > -1) {
                        left.Employees[existedEmpIdx] = element;
                    } else {
                        left.Employees.push(element);
                    }
                }
            });
        });
    }

    async getFullListCustomerData() {
        const res: any = await hrCustomerInfoApi.GetAllCustomer()
        const data = res.data;
        const dictData = {};
        data.forEach((e: any) => {
            dictData[e.Index] = {
                Index: e.Index,
                Name: e.CustomerName,
                ATID: e.CustomerID
            };
        });
        this.listFullCustomerData = dictData;
    }

    generateDepartmentData() {
        // // console.log(this.employeeDepartment)
        for (let i = 0; i < this.employeeDepartment.length; i++) {
            const element = this.employeeDepartment[i];
            if (element.Code == "Visitor") {
                this.visitorDeptIndex = element.Index;
            }
            this.tblData.push({
                Index: element.value,
                Department: element.label,
                Employees: [],
            });
            // // console.log(this.tblData)
        }
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
                this.strNow = moment(now).format("DD/MM/YYYY HH:mm:ss");

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
            // console.log(data)
            this.handlerDataReceive(data);
        });

        this.realtimeConnection.on("ReceiveUpdateLog", async err => {
            await this.getListData();
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
        // // console.log(this.listFullCustomerData)
        // // console.log(this.employeeFullLookup)
        if (data.Value.CustomerIndex != 0) {
            const customer = this.listFullCustomerData[data.Value.CustomerIndex] || {};
            if (data.Key == true) { //In
                this.tblData.forEach(item => {
                    if (item.Index == this.visitorDeptIndex) {
                        const objIndex = item.Employees.findIndex((obj => obj.EmployeeATID == customer.ATID));
                        if (objIndex == -1) {
                            const employeeLog: EmployeeLog = {
                                EmployeeATID: customer.ATID,
                                FullName: customer.Name,
                                Position: "",
                                Time: moment(data.Value.Time).format('DD-MM-YYYY - HH:mm:ss'),
                                Department: "",
                                DepartmentIndex: 0,
                                Error: data.Value.Error,
                                IsCustomer: true,
                            }
                            item.Employees.push(employeeLog);
                        } else {
                            item.Employees[objIndex].Time = moment(data.Value.Time).format('DD-MM-YYYY - HH:mm:ss');
                        }
                    }
                });
            } else { // Out
                this.tblData.forEach(item => {
                    if (item.Index == this.visitorDeptIndex) {
                        item.Employees.forEach((element, index) => {
                            if (element.EmployeeATID == data.Value.CustomerIndex) {
                                item.Employees.splice(index, 1);
                            }
                        });
                    }
                });
            }
        } else {
            // const employee = this.employeeFullLookup[data.Value.EmployeeATID] || {};
            const employee = this.employeeFullLookup.find(x => x.EmployeeATID == data.Value.EmployeeATID) || {};
            if (data.Key == true) { //In
                this.tblData.forEach(left => {
                    if (left.Index == employee.DepartmentIndex) {
                        const objIndex = left.Employees.findIndex((obj => obj.EmployeeATID == employee.EmployeeATID));
                        if (objIndex == -1) {
                            const employeeLog: EmployeeLog = {
                                EmployeeATID: employee.EmployeeATID,
                                FullName: employee.FullName,
                                Position: employee.PositionName,
                                Time: moment(data.Value.Time).format('DD-MM-YYYY - HH:mm:ss'),
                                Error: data.Value.Error,
                                Department: "",
                                DepartmentIndex: 0,
                            }
                            left.Employees.push(employeeLog);
                        } else {
                            left.Employees[objIndex].Time = moment(data.Value.Time).format('DD-MM-YYYY - HH:mm:ss');
                        }
                    }
                });
            } else { // Out
                this.tblData.forEach(item => {
                    if (item.Index == employee.DepartmentIndex) {
                        item.Employees.forEach((element, index) => {
                            if (element.EmployeeATID == data.Value.EmployeeATID) {
                                item.Employees.splice(index, 1);
                            }
                        });
                    }
                });
            }
        }
    }

    async rowClicked(row: TableDataItem, column, event) {
        this.dialogEmployeeInfo = true;
        this.tblDataDetail.length = 0; //reset data table.
        /**
         * Danh sách lỗi vi phạm của người được chọn
         */
        let violationShift = [];
        let index = 1;
        this.selectedNumberOfEmployee = row.Employees.length;
        this.selectedDepartmentName = row.Department;
        this.dialogLoading = true;

        if (row.Employees.length > 0) {
            const isCustomer = this.visitorDeptIndex == row.Index;

            const emps: Array<string> = row.Employees.map(({ EmployeeATID }) => (EmployeeATID));
            await listViolationApi.GetViolationByShift(emps, isCustomer).then((res: any) => {
                if(res && res.data && res.data != ''){
                    violationShift = res.data;
                    violationShift.forEach((element, index) => {
                        if (element.Error != "ExceedCheckOutTime") {
                            const employee = row.Employees[index];
                            if (employee) {
                                const errorAllowShow = this.listAllowShowRealtime.find(e => e == employee.Error);
                                if (errorAllowShow) {
                                    element.Error = errorAllowShow;
                                } else {
                                    element.Error = "NoError";
                                }
                            }
                        }
                    });
                }
            });


            row.Employees.forEach(element => {
                let vioError = element.Error;
                const vioShift = violationShift.find(e => e.EmployeeATID == element.EmployeeATID);
                if (vioShift) {
                    vioError = vioShift.Error;
                    // if (vioError == "ExceedCheckOutTime" || vioError == "OutOfWorkingTime") {
                    //     vioError = "OutWokingTime"
                    // }
                }
                const data = {
                    Index: index,
                    EmployeeATID: element.EmployeeATID,
                    FullName: element.FullName,
                    Position: element.Position,
                    TimeIn: element.Time,
                    ViolationInfo: this.$t(vioError),
                    Error: element.Error
                }
                this.tblDataDetail.push(data);
                index++;
            });
        } else {
            this.tblDataDetail = [];
        }
        this.doLayout();
        this.dialogLoading = false;
    }

    doLayout() {
        const dlRefs = this.$refs.dataTableRef as any;

        if (dlRefs != undefined || dlRefs != '') {
            try {
                dlRefs.doLayout && dlRefs.doLayout();
            } catch {
                // // // console.log('');
            }
        }
    }

    tableRowClassName = ({
        row,
        rowIndex,
    }: {
        row: TableDataItem;
        rowIndex: number;
    }) => {
        if (row.Employees.length > 0) {
            const employeeArray = Misc.cloneData(row.Employees);
            const violentingRule = employeeArray.filter(x => x.Error == "WorkingOverTime").length;
            if (violentingRule > 0) {
                return 'warning-row';
            }
        }
        return '';
    }

    tableRowDetailClassName({ row }) {
        if (row != null && row.Error == "WorkingOverTime") {
            return 'warning-row';
        }
        return '';
    }
}
