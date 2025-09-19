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
import { deviceApi } from '@/$api/device-api';
import { privilegeMachineRealtimeApi } from '@/$api/privilege-machine-realtime-api';
import { groupDeviceApi } from '@/$api/group-device-api';

import moment from "moment";
import { rootLink } from "../../../constant/variable";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';


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

type EmployeeLog = {
    EmployeeATID: string;
    DepartmentIndex: number;
    DepartmentName: string;
    FullName: string;
    SerialNumber: string;
    PositionName: string;
    Time: any;
    TimeString: string;
}

@Component({
    name: 'total-employee-present',
    components: {HeaderComponent, DataTableComponent, DataTableFunctionComponent},
})
export default class TotalEmployeePresent extends Vue {
    strNow = "";
    differentNumber: number;
    tblDepartmentLogData: DepartmentLogDataItem[] = [];
    tblAllDepartmentLogData: DepartmentLogDataItem[] = [];
    tblGroupDeviceLogData: GroupDeviceLogDataItem[] = [];

    tblDataDetail = [];

    groupDeviceData: any;
    departmentData: any;
    remainInLogData: any;

    connection;
    isConnect = false;
    dialogEmployeeInfo = false;
    dialogLoading = false;
    selectedDepartmentName = "";
    selectedNumberOfEmployee = 0;
    isShowOnlyRoot = false;

    maxHeight = (window.innerHeight / 100 * 55);
    activeTab = "area";
    get totalEmployee(){
        if(this.activeTab == "department"){
            return (this.tblDepartmentLogData as any).flatMap(x => x.Employees).length;
        }else{
            return (this.tblGroupDeviceLogData as any).flatMap(x => x.Employees).length;
        }
    }

    isFullscreenOn: boolean = false;   

    toggleFullScreen(): void {
        // // console.log("fullChange")
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

        await groupDeviceApi.GetGroupDeviceResult().then((res: any) => {
            // // console.log("Area", res)
            if(res && res.data){
                this.groupDeviceData = res.data;
            }
        });

        await departmentApi.GetActiveDepartmentAndDeviceByPermission().then((res: any) => {
            // // console.log("Department", res)
            if(res && res.data){
                this.departmentData = res.data;
                // console.log(this.departmentData)
            }
        });

        attendanceLogApi.GetRemainInLogs().then((res: any) => {
            // // console.log("RemainInLogs", res)
            if(res && res.data){
                this.remainInLogData = res.data;
                let index = 1; 

                this.groupDeviceData.forEach((element, index) => {
                    if(!this.tblGroupDeviceLogData.some(x => x.GroupDeviceIndex == element.GroupDeviceIndex)){
                        this.tblGroupDeviceLogData.push({GroupDeviceIndex: element.GroupDeviceIndex, 
                            GroupDeviceName: element.GroupDeviceName,
                            SerialNumber: [],
                            Employees: []});
                    }
                });
                this.tblGroupDeviceLogData.forEach((element, index) => {
                    element.SerialNumber = this.groupDeviceData.filter(x => x.GroupDeviceIndex == element.GroupDeviceIndex).
                        map(x => x.SerialNumber);
                });
                this.tblGroupDeviceLogData.forEach((element, index) => {
                    element.Employees = this.remainInLogData.filter(x => element.SerialNumber.includes(x.SerialNumber));
                });
                // // // console.log(this.tblGroupDeviceLogData)

                this.departmentData.forEach((element, index) => {
                    if(!this.tblDepartmentLogData.some(x => x.DepartmentIndex == element.DepartmentIndex)){
                        this.tblDepartmentLogData.push({DepartmentIndex: element.DepartmentIndex, 
                            Department: element.DepartmentName,
                            SerialNumber: [],
                            Employees: []});
                    }
                });

                this.tblDepartmentLogData.forEach((element, index) => {
                    element.SerialNumber = this.departmentData.filter(x => x.DepartmentIndex == element.DepartmentIndex).
                        map(x => x.SerialNumber);
                });
                this.tblDepartmentLogData.forEach((element, index) => {
                    // element.Employees = this.remainInLogData.filter(x => element.SerialNumber.includes(x.SerialNumber));
                    element.Employees = this.remainInLogData.filter(x => element.DepartmentIndex == x.DepartmentIndex);
                    // console.log(element.Department, element.Employees)
                });

                this.tblAllDepartmentLogData = Misc.cloneData(this.tblDepartmentLogData);

                // const grouped = this.remainInLogData.reduce((groups, item) => {
                //     const { DepartmentIndex } = item;

                //     let group = groups.find(g => g.DepartmentIndex === DepartmentIndex);

                //     if (!group) {
                //         group = {
                //             DepartmentIndex,
                //             Employees: []
                //         };
                //         groups.push(group);
                //     }

                //     group.Employees.push(item);

                //     if(group.Employees.length > 0){
                //         (group as any).Department = this.$t(group.Employees[0]?.DepartmentName) ?? "";
                //     }

                //     return groups;
                // }, []);
                // this.tblDepartmentLogData = grouped;
                // this.tblAllDepartmentLogData = grouped;

                // // // console.log(this.tblDepartmentLogData)
            }
        });
    }

    mounted() {
        this.getRealTimeServer();
    }

    created() {
        
    }

    reGetRemainInLogs(){
        attendanceLogApi.GetRemainInLogs().then((res: any) => {
            // // console.log("RemainInLogs", res)
            if(res && res.data){
                this.remainInLogData = res.data;
                let index = 1; 

                this.groupDeviceData.forEach((element, index) => {
                    if(!this.tblGroupDeviceLogData.some(x => x.GroupDeviceIndex == element.GroupDeviceIndex)){
                        this.tblGroupDeviceLogData.push({GroupDeviceIndex: element.GroupDeviceIndex, 
                            GroupDeviceName: element.GroupDeviceName,
                            SerialNumber: [],
                            Employees: []});
                    }
                });
                this.tblGroupDeviceLogData.forEach((element, index) => {
                    element.SerialNumber = this.groupDeviceData.filter(x => x.GroupDeviceIndex == element.GroupDeviceIndex).
                        map(x => x.SerialNumber);
                });
                this.tblGroupDeviceLogData.forEach((element, index) => {
                    element.Employees = this.remainInLogData.filter(x => element.SerialNumber.includes(x.SerialNumber));
                });
                // // // console.log(this.tblGroupDeviceLogData)

                this.departmentData.forEach((element, index) => {
                    if(!this.tblDepartmentLogData.some(x => x.DepartmentIndex == element.DepartmentIndex)){
                        this.tblDepartmentLogData.push({DepartmentIndex: element.DepartmentIndex, 
                            Department: element.DepartmentName,
                            SerialNumber: [],
                            Employees: []});
                    }
                });

                this.tblDepartmentLogData.forEach((element, index) => {
                    element.SerialNumber = this.departmentData.filter(x => x.DepartmentIndex == element.DepartmentIndex).
                        map(x => x.SerialNumber);
                });
                this.tblDepartmentLogData.forEach((element, index) => {
                    // element.Employees = this.remainInLogData.filter(x => element.SerialNumber.includes(x.SerialNumber));
                    element.Employees = this.remainInLogData.filter(x => element.DepartmentIndex == x.DepartmentIndex);
                });

                this.tblAllDepartmentLogData = Misc.cloneData(this.tblDepartmentLogData);
            }
        });
    }

    getRealTimeServer() {
        configApi.GetRealTimeServerLink().then((res: any) => {
            // // // console.log(res)
            this.connectToRealTimeServer(res.data);
            const startOfDay = new Date();
            startOfDay.setHours(0,0,0);
            setInterval(() => {
                const clientTimeSpan = new Date().getTime();
                const now = new Date(clientTimeSpan + this.differentNumber);
                this.strNow = moment(now).format("DD/MM/YYYY - HH:mm:ss");
                // // console.log(now.getHours(), now.getMinutes(), now.getSeconds())
                // // console.log(startOfDay.getHours(), startOfDay.getMinutes(), startOfDay.getSeconds())
                if(now.getHours() == startOfDay.getHours() && now.getMinutes() == startOfDay.getMinutes() 
                    && now.getSeconds() == startOfDay.getSeconds()){
                    this.reGetRemainInLogs();
                }
                //this.strNow=now.getTime().toString();
            }, 500);
        });
    }

    async getDiffFromServerTimeAndClientTime() {
        attendanceLogApi.GetSystemDateTime().then((res: any) => {
            // // // console.log(res)
            const systemDateTime: Date = new Date(res.data);
            const clientDateTime = new Date();
            this.differentNumber = systemDateTime.getTime() - clientDateTime.getTime();
        });
    }

    connectToRealTimeServer(link) {
        // // console.log(link)
        this.connection = new HubConnectionBuilder()
            .withUrl(link + "/attendanceHub")
            .configureLogging(LogLevel.Information)
            .build();
        const data = this.connection;
        // const notify = this.$notify;

        // receive data from server
        const thisVue = this;
        this.connection.on("ReceiveAttendanceLog", listData => {
            // // console.log("logData", listData)
            // // console.log(this.tblDepartmentLogData)
            // // console.log(this.tblGroupDeviceLogData)
            // // console.log(this.departmentData)
            // // console.log(this.groupDeviceData)

            this.reLoadAllDepartmentData(false);

            listData.forEach((element, index) => {
                if(element.InOutModeString == "In" || element.InOutModeString == "BreakIn" 
                    || element.InOutMode == 0 || element.InOutMode == 3){
                    // const departmentEmployeeData = this.tblDepartmentLogData.find(x => x.SerialNumber.includes(element.SerialNumber));
                    const departmentEmployeeData = this.tblDepartmentLogData.find(x => x.DepartmentIndex == 
                        element.DepartmentIndex);
                    // const department = this.departmentData.find(x => x.SerialNumber == element.SerialNumber);
                    const department = this.departmentData.find(x => x.DepartmentIndex == element.DepartmentIndex);
                    // // console.log(departmentEmployeeData)
                    if(!departmentEmployeeData){
                        // // console.log("a")
                        const departmentData = {DepartmentIndex: department.DepartmentIndex, 
                            Department: department.DeparmentName, 
                            SerialNumber: [element.SerialNumber],
                            Employees: [{
                                EmployeeATID: element.EmployeeATID,
                                FullName: element?.FullName ?? "",
                                DepartmentIndex: element?.DepartmentIndex ?? 0,
                                DepartmentName: element?.DepartmentName ?? "",
                                SerialNumber: element?.SerialNumber ?? "",
                                PositionName: element?.PositionName ?? "",
                                Time: element.CheckTime,
                                TimeString: moment(element.CheckTime).format('DD-MM-YYYY HH:mm:ss'),
                            }]};
                            thisVue.tblDepartmentLogData.unshift(departmentData);
                    }else{
                        // // console.log("b");
                        if(!(departmentEmployeeData as any).SerialNumber.includes(element.SerialNumber)){
                            (departmentEmployeeData as any).SerialNumber.unshift(element.SerialNumber);
                        } 
                        const employeeLog = (departmentEmployeeData as any).Employees.find(x => 
                            x.EmployeeATID == element.EmployeeATID);
                        if(employeeLog == null){
                            (departmentEmployeeData as any).Employees.unshift({
                                EmployeeATID: element.EmployeeATID,
                                FullName: element?.FullName ?? "",
                                DepartmentIndex: element?.DepartmentIndex ?? 0,
                                DepartmentName: element?.DepartmentName ?? "",
                                SerialNumber: element?.SerialNumber ?? "",
                                PositionName: element?.PositionName ?? "",
                                Time: element.CheckTime,
                                TimeString: moment(element.CheckTime).format('DD-MM-YYYY HH:mm:ss'),
                            });
                        }else{
                            employeeLog.Time = element.CheckTime;
                            employeeLog.TimeString = moment(element.CheckTime).format('DD-MM-YYYY HH:mm:ss');
                        }
                    }
                    // // console.log(this.tblDepartmentLogData)

                    this.tblGroupDeviceLogData.forEach(group => {
                        if(group != null && group.Employees && group.Employees.length > 0){
                            // // console.log("g")
                            group.Employees.forEach((data, idx) => {
                                if (element.EmployeeATID == data.EmployeeATID) {
                                    // // console.log("h")
                                    group.Employees.splice(idx, 1);
                                }
                            });
                        }
                    });

                    const groupDeviceEmployeeData = this.tblGroupDeviceLogData.filter(x => 
                        x.SerialNumber.includes(element.SerialNumber));
                    // // console.log(groupDeviceEmployeeData)
                    if(!groupDeviceEmployeeData || (groupDeviceEmployeeData && groupDeviceEmployeeData.length == 0)){
                        // // console.log("c")
                        const groupDevice = this.groupDeviceData.find(x => x.SerialNumber == element.SerialNumber);
                        const groupDeviceData = {GroupDeviceIndex: groupDevice.GroupDeviceIndex, 
                            GroupDeviceName: groupDevice.GroupDeviceName,
                            SerialNumber: [element.SerialNumber],
                            Employees: [{
                                EmployeeATID: element.EmployeeATID,
                                FullName: element?.FullName ?? "",
                                DepartmentIndex: element?.DepartmentIndex ?? 0,
                                DepartmentName: element?.DepartmentName ?? "",
                                SerialNumber: element?.SerialNumber ?? "",
                                PositionName: element?.PositionName ?? "",
                                Time: element.CheckTime,
                                TimeString: moment(element.CheckTime).format('DD-MM-YYYY HH:mm:ss'),
                            }]};
                            thisVue.tblGroupDeviceLogData.unshift(groupDeviceData);
                    }else{
                        // // console.log("d");
                        groupDeviceEmployeeData.forEach(group => {
                            if(!(group as any).SerialNumber.includes(element.SerialNumber)){
                                (group as any).SerialNumber.unshift(element.SerialNumber);
                            }
                            const employeeLog = (group as any).Employees.find(x => 
                                x.EmployeeATID == element.EmployeeATID);
                            if(employeeLog == null){
                                (group as any).Employees.unshift({
                                    EmployeeATID: element.EmployeeATID,
                                    FullName: element?.FullName ?? "",
                                    DepartmentIndex: element?.DepartmentIndex ?? 0,
                                    DepartmentName: element?.DepartmentName ?? "",
                                    SerialNumber: element?.SerialNumber ?? "",
                                    PositionName: element?.PositionName ?? "",
                                    Time: element.CheckTime,
                                    TimeString: moment(element.CheckTime).format('DD-MM-YYYY HH:mm:ss'),
                                });
                            }else{
                                employeeLog.Time = element.CheckTime;
                                employeeLog.TimeString = moment(element.CheckTime).format('DD-MM-YYYY HH:mm:ss');
                            }
                        });                    
                    }
                }else{
                    // const departmentEmployeeData = this.tblDepartmentLogData.find(x => 
                    //     x.SerialNumber.includes(element.SerialNumber));
                    const departmentEmployeeData = this.tblDepartmentLogData.find(x => 
                        x.DepartmentIndex == element.DepartmentIndex);
                    // // console.log(departmentEmployeeData)
                    if(departmentEmployeeData != null 
                        && departmentEmployeeData.Employees && departmentEmployeeData.Employees.length > 0){
                            // // console.log("e")
                            departmentEmployeeData.Employees.forEach((data, idx) => {
                                if (element.EmployeeATID == data.EmployeeATID) {
                                    // // console.log("f")
                                    departmentEmployeeData.Employees.splice(idx, 1);
                                }
                            });
                    }
                    // const groupDeviceEmployeeData = this.tblGroupDeviceLogData.filter(x => 
                    //     x.SerialNumber.includes(element.SerialNumber));
                    const groupDeviceEmployeeData = this.tblGroupDeviceLogData;
                    // // console.log(groupDeviceEmployeeData)

                    groupDeviceEmployeeData.forEach(group => {
                        if(group != null && group.Employees && group.Employees.length > 0){
                            // // console.log("g")
                            group.Employees.forEach((data, idx) => {
                                if (element.EmployeeATID == data.EmployeeATID) {
                                    // // console.log("h")
                                    group.Employees.splice(idx, 1);
                                }
                            });
                        }
                    });
                }                
            });

            this.tblAllDepartmentLogData = Misc.cloneData(this.tblDepartmentLogData);

            this.reLoadDepartmentData();
        });
        this.connection
            .start()
            .then(() => {
                // // // console.log("connection started")
                thisVue.isConnect = true;
                data
                    .invoke("AddUserToGroup", 2, self.localStorage.getItem("user"))
                    .catch(err => {
                        // // console.log(err.toString());
                    });
            })
            .catch(err => {
                thisVue.isConnect = false;
                // // console.log(err.toString());
            });
        this.connection.onclose(() => {
            thisVue.isConnect = false;
        });
    }

    async rowClicked(row: any, column, event) {
        // // // console.log(row)
        this.dialogEmployeeInfo = true;
        this.dialogLoading = true;

        const data = row.Employees;
        this.tblDataDetail = data;
        this.tblDataDetail.forEach((element, index) => {
            element.Index = (index + 1);
        });
        this.selectedNumberOfEmployee = data.length;

        if(this.activeTab == 'area'){
            const depName = row.GroupDeviceName;
            this.selectedDepartmentName = depName;
        }else{
            const depName = row.Department;
            this.selectedDepartmentName = depName;
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
                // // console.log('');
            }
        }
    }

    @Watch("isShowOnlyRoot")
    reLoadDepartmentData(){
        if(this.isShowOnlyRoot){
            const onlyRootDeparmentLogDataTbl = Misc.cloneData(this.tblAllDepartmentLogData.filter(x => 
                this.departmentData.some(y => y.RootDepartment && y.DepartmentIndex == x.DepartmentIndex)));
            onlyRootDeparmentLogDataTbl.forEach((element, index) => {
                const childRootDepartmentArray = Misc.cloneData(this.departmentData.filter(x => 
                    x.RootDepartmentIndex == element.DepartmentIndex));
                const childDepartmentArray = (childRootDepartmentArray && childRootDepartmentArray.length > 0) 
                    ? childRootDepartmentArray.map(x => x.DepartmentIndex) : [];
                const childDepartmentLogDataTbl = Misc.cloneData(this.tblAllDepartmentLogData.filter(x => 
                    childDepartmentArray.includes(x.DepartmentIndex)));
                if(childDepartmentLogDataTbl && childDepartmentLogDataTbl.length > 0){
                    childDepartmentLogDataTbl.forEach((childElement, childIndex) => {
                        if(childElement.Employees && childElement.Employees.length > 0){
                            element.Employees = element.Employees.concat(childElement.Employees);
                        }
                    });
                }
            });
            this.tblDepartmentLogData = onlyRootDeparmentLogDataTbl;
        }else{
            this.tblDepartmentLogData = Misc.cloneData(this.tblAllDepartmentLogData.filter(x => 
                this.departmentData.some(y => y.DepartmentIndex == x.DepartmentIndex)));
        }
        // // console.log(this.tblAllDepartmentLogData)
        if(!this.tblDepartmentLogData){
            this.tblDepartmentLogData = [];
        }
    }

    reLoadAllDepartmentData(onlyRoot: boolean){
        if(onlyRoot){
            const onlyRootDeparmentLogDataTbl = Misc.cloneData(this.tblAllDepartmentLogData.filter(x => 
                this.departmentData.some(y => y.RootDepartment && y.DepartmentIndex == x.DepartmentIndex)));
            onlyRootDeparmentLogDataTbl.forEach((element, index) => {
                const childRootDepartmentArray = Misc.cloneData(this.departmentData.filter(x => 
                    x.RootDepartmentIndex == element.DepartmentIndex));
                const childDepartmentArray = (childRootDepartmentArray && childRootDepartmentArray.length > 0) 
                    ? childRootDepartmentArray.map(x => x.DepartmentIndex) : [];
                const childDepartmentLogDataTbl = Misc.cloneData(this.tblAllDepartmentLogData.filter(x => 
                    childDepartmentArray.includes(x.DepartmentIndex)));
                if(childDepartmentLogDataTbl && childDepartmentLogDataTbl.length > 0){
                    childDepartmentLogDataTbl.forEach((childElement, childIndex) => {
                        if(childElement.Employees && childElement.Employees.length > 0){
                            element.Employees = element.Employees.concat(childElement.Employees);
                        }
                    });
                }
            });
            this.tblDepartmentLogData = onlyRootDeparmentLogDataTbl;
        }else{
            this.tblDepartmentLogData = Misc.cloneData(this.tblAllDepartmentLogData.filter(x => 
                this.departmentData.some(y => y.DepartmentIndex == x.DepartmentIndex)));
        }
        // // console.log(this.tblAllDepartmentLogData)
        if(!this.tblDepartmentLogData){
            this.tblDepartmentLogData = [];
        }
    }
}
