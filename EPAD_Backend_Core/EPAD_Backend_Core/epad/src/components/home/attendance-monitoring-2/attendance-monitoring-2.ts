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
import { gatesApi } from '@/$api/gc-gates-api';
import { listViolationApi } from '@/$api/gc-list-violation-api';
import { deviceApi } from '@/$api/device-api';
import { privilegeMachineRealtimeApi } from '@/$api/privilege-machine-realtime-api';
import { groupDeviceApi } from '@/$api/group-device-api';

import moment from "moment";
import { rootLink } from "../../../constant/variable";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';


type TableDataItem = {
    Index: number;
    Employees: EmployeeLog[];
    Area: string;
    AreaDescription: string;
    MachineList: any[];
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
    MachineSerial: string;
    EmployeeType?: number;
}

@Component({
    name: 'attendance-monitoring-2',
    components: {HeaderComponent, DataTableComponent, DataTableFunctionComponent},
})
export default class AttendanceMonitoring extends Vue {
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
    employeeGate: any[];

    selectUserTypeOption = [
        { value: 1, label: 'Employee' },
        { value: 2, label: 'Customer' },
        { value: 3, label: 'Student' },
        { value: 4, label: 'Parent' },
        { value: 5, label: 'Nanny' },
        { value: 6, label: 'Contractor' },
        { value: 7, label: 'Teacher' },
    ];

    employeeCount = 0;
    customerCount = 0;
    contractorCount = 0;

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

        // await departmentApi.GetActiveDepartmentAndDeviceByPermission().then((res: any) => {
        //     if(res && res.data){
        //         this.employeeDepartment = res.data;
        //     }
        // });
    }

    async mounted() {
        // this.generateDepartmentData();

        await gatesApi.GetGateByDevice().then((res: any) => {
            if(res && res.data){
                this.employeeGate = res.data;
            }
            this.generateAreaData();
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
            if (element.CustomerIndex <= 0) {
                const employee = this.employeeFullLookup.find(x => x.EmployeeATID == element.EmployeeATID) || {};
                arrTemp.push({
                    DepartmentIndex: employee.DepartmentIndex,
                    Department: employee.Department,
                    Position: employee.PositionName,
                    FullName: employee.FullName,
                    Time: moment(element.Time).format('DD-MM-YYYY - HH:mm:ss'),
                    Error: element.Error,
                    EmployeeATID: element.EmployeeATID,
                    MachineSerial: element.MachineSerial
                });
            }
        });
        this.tblData.map(x => x.Employees).flat().filter(x => x.Position)
        this.tblData.forEach(left => {
            left.Employees = [];
            arrTemp.forEach(element => {
                if (left.MachineList.includes(element.MachineSerial)) {
                    const existedEmpIdx = left.Employees.findIndex(x => x.EmployeeATID == element.EmployeeATID);
                    if (existedEmpIdx > -1) {
                        left.Employees[existedEmpIdx] = element;
                    } else {
                        left.Employees.push(element);
                    }
                }
            });
        });
        this.calculateEmployeeTypeCount();
    }

    calculateEmployeeTypeCount(){
        if(this.tblData && this.tblData.length > 0){
            if(this.listFullCustomerData){
                this.employeeCount = 0;
                this.customerCount = 0;
                this.contractorCount = 0;
                this.tblData.forEach(element => {
                    if (element.Employees && element.Employees.length > 0) {
                        element.Employees.forEach(childElement => {
                            const customer = this.listFullCustomerData[childElement.EmployeeATID] || {};
                            const employeeType = customer.EmployeeType || this.selectUserTypeOption.find(x => x.label === "Employee").value;
                            switch (employeeType) {
                                case this.selectUserTypeOption.find(x => x.label === "Customer").value:
                                    this.customerCount += 1;
                                    break;
                                case this.selectUserTypeOption.find(x => x.label === "Contractor").value:
                                    this.contractorCount += 1;
                                    break;
                                default:
                                    this.employeeCount += 1;
                            }
                        });
                    }
                });
            }else{
                this.employeeCount = this.tblData.reduce((sum, element) => {
                    return (element.Employees.length + sum);
                }, 0);
            }
        }
    }

    async getFullListCustomerData() {
        const res: any = await hrCustomerInfoApi.GetCustomerAndContractorInfo()
        const data = res.data;
        const dictData = {};
        data.forEach((e: any) => {
            dictData[e.EmployeeATID] = {
                // Index: e.Index,
                // Name: e.CustomerName,
                // ATID: e.CustomerID
                Index: e.EmployeeATID,
                Name: e.FullName,
                EmployeeATID: e.EmployeeATID,
                EmployeeType: e.EmployeeType
            };
        });
        this.listFullCustomerData = dictData;
    }

    // generateDepartmentData() {
    //     for (let i = 0; i < this.employeeDepartment.length; i++) {
    //         const element = this.employeeDepartment[i];
    //         if (element.Code == "Visitor") {
    //             this.visitorDeptIndex = element.Index;
    //         }
    //         this.tblData.push({
    //             Index: element.Index,
    //             Department: element.Name,
    //             Employees: [],
    //         });
    //     }
    // }

    generateAreaData() {
        for (let i = 0; i < this.employeeGate.length; i++) {
            const element = this.employeeGate[i];
            this.tblData.push({
                Index: element.Index,
                Area: element.Name,
                AreaDescription: element.Description,
                MachineList: element.MachineList,
                Employees: []
            })
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
                this.strNow = moment(now).format("DD/MM/YYYY") + "\xa0\xa0\xa0\xa0\xa0\xa0\xa0" + moment(now).format("hh:mm A");

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
        console.log(data)
        // // console.log(this.listFullCustomerData)
        // // console.log(this.employeeFullLookup)
        let dataTime = Misc.cloneData(data.Value.Time);
        if(!this.isUTC(dataTime)){
            dataTime = moment.utc(dataTime).format();
        }
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
                                Time: moment.utc(new Date(dataTime)).format('DD-MM-YYYY - HH:mm:ss'),
                                Department: "",
                                DepartmentIndex: 0,
                                Error: data.Value.Error,
                                IsCustomer: true,
                                MachineSerial: "",
                                EmployeeType: customer.EmployeeType
                            }
                            item.Employees.push(employeeLog);
                        } else {
                            item.Employees[objIndex].Time = moment.utc(new Date(dataTime)).format('DD-MM-YYYY - HH:mm:ss');
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
                    if (left.MachineList.includes(data.Value.MachineSerial)) {
                        const objIndex = left.Employees.findIndex((obj => obj.EmployeeATID == employee.EmployeeATID));
                        if (objIndex == -1) {
                            const employeeLog: EmployeeLog = {
                                EmployeeATID: employee.EmployeeATID,
                                FullName: employee.FullName,
                                Position: employee.PositionName,
                                Time: moment.utc(new Date(dataTime)).format('DD-MM-YYYY - HH:mm:ss'),
                                Error: data.Value.Error,
                                Department: "",
                                DepartmentIndex: 0,
                                MachineSerial: ""
                            }
                            left.Employees.push(employeeLog);
                        } else {
                            left.Employees[objIndex].Time = moment.utc(new Date(dataTime)).format('DD-MM-YYYY - HH:mm:ss');
                            left.Employees[objIndex].Error = data.Value.Error;
                        }
                    }
                });
               
            } else { // Out
                this.tblData.forEach(item => {
                    if (item.MachineList.includes(data.Value.MachineSerial)) {
                        item.Employees.forEach((element, index) => {
                            if (element.EmployeeATID == data.Value.EmployeeATID) {
                                item.Employees.splice(index, 1);
                            }
                        });
                    }
                });
            }
        }
        this.calculateEmployeeTypeCount();
    }

    isUTC(date) {
        return date.endsWith('Z');
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
        this.selectedDepartmentName = row.Area.replace("26px", "16px");
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
        }else{
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
        }else if(row != null && row.Employees && row.Employees.some(y => y.Error && y.Error != "")){
            return 'warning-row';
        }else if(row.Error && row.Error != ""){
            return 'warning-row';
        }
        return '';
    }

    itemClassName(row){
        if (row != null && row.Error == "WorkingOverTime") {
            return 'warning-class';
        }else if(row != null && row.Employees && row.Employees.some(y => y.Error && y.Error != "")){
            return 'warning-class';
        }else if(row.Error && row.Error != ""){
            return 'warning-class';
        }
        return '';
    }
}
