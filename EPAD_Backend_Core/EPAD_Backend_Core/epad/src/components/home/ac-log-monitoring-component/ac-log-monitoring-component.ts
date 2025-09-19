import { Component, Vue, Mixins } from "vue-property-decorator";
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

import moment from "moment";
import { rootLink } from "../../../constant/variable";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { areaApi } from "@/$api/ac-area-api";
import { doorApi } from "@/$api/ac-door-api";
@Component({
    name: "log-monitoring",
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectTreeComponent, SelectDepartmentTreeComponent }
})
export default class ACLogMonitoringComponent extends Mixins(ComponentBase) {
    EmployeeATID = "";
    EmployeeCode = "";
    FullName = "";
    Gender = "";
    Genderoptions = [];
    columns = [];
    selectItem = [];
    dateTime = "";
    tableData = [];
    connection;
    isConnect = false;
    lstDepartment = [];
    accessedDepartment = {};
    filter = { Department: [], Machine: [], Area: [], Door: [] }
    departmentAndDeviceLookup = [];
    deviceFilter = [];
    maxHeight = window.innerHeight - 210;
    areaLst = [];
    doorLst = [];

    strConnectSuccessfully;
    strConnectFailed;

    tree = {
        employeeList: [],
        clearable: true,
        defaultExpandAll: false,
        multiple: true,
        placeholder: "",
        disabled: false,
        checkStrictly: false,
        popoverWidth: 400,
        treeData: [],
        treeProps: {
            value: 'ID',
            children: 'ListChildrent',
            label: 'Name',
        },
    }

    apiGetAttendaceLog = rootLink + "api/AttendanceLog/GetLastedAttendanceLog";

    dialogVisible: boolean = true;
    isFullscreenOn: boolean = false;

    toggleFullScreen(): void {
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

    async LoadDepartmentTree() {
        await departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });
    }

    async LoadAreaLst(){
        await areaApi.GetAllArea().then((res: any) => {
            this.areaLst = res.data;
        })
    }

    async LoadDoorLst(){
        await doorApi.GetAllDoor().then((res: any) => {
            this.doorLst = res.data;
        })
    }

    getData({ page, query, sortParams }) {
        return axios
            .get("https://vuetable.ratiw.net/api/users", {
                params: {
                    page,
                    sort: sortParams
                }
            })
            .then(response => {
                let { data } = response;
                return {
                    data: data.data,
                    total: data.total
                };
            });
    }

    handleClose(done) {
        this.selectItem = [1];
        return;
    }

    async beforeMount() {
        await this.getDepartment();
        await this.LoadDepartmentTree();
        await this.getAllDevices();
        await this.LoadAreaLst();
        await this.LoadDoorLst();
        window.addEventListener("resize", () => {
            this.MaxHeightTable();
        });
        // await this.getDepartmentAndDeviceLookup();
    }
    MaxHeightTable() {
        this.maxHeight = window.innerHeight - 210;
    }
    created() {
        this.initColumn();
    }

    mounted() {
        this.getRealTimeServer();
        this.getDataForTable();

        const date = new Date();
        this.dateTime = moment(date.toString()).format("YYYY-MM-DD HH:mm:ss");
    }

    initColumn() {
        this.columns = [
            {
                prop: "atid",
                label: "EmployeeATID",
                minWidth: "100",
                sortable: true,
                display: true
            },
            {
                prop: "fullname",
                label: "FullName",
                minWidth: "100",
                sortable: true,
                display: true
            },
            {
                prop: "department",
                label: "Department",
                minWidth: "100",
                sortable: true,
                display: true
            },
            {
                prop: "time",
                label: "DateTime",
                minWidth: "80",
                sortable: true,
                display: true
            },
            {
                prop: "areaname",
                label: "Area",
                minWidth: "100",
                sortable: true,
                display: true
            },
            {
                prop: "doorname",
                label: "Door",
                minWidth: "100",
                sortable: true,
                display: true
            },
            {
                prop: "machine",
                label: "Device",
                minWidth: "80",
                sortable: true,
                display: true
            },
            {
                prop: "inout",
                label: "InOut",
                minWidth: "80",
                sortable: true,
                display: true
            },
            {
                prop: "verify",
                label: "Mode",
                minWidth: "80",
                sortable: true,
                display: true
            },
            
            //},
            //{
            //    fixed: "right",
            //    label: "Operations",
            //    sortable: true,
            //    width: "150",
            //    display: true
            //}
        ];
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
        this.connection.on("ReceiveAttendanceLog", listData => {
            for (let i = 0; i < listData.length; i++) {
                console.log(listData);
                if (!(listData[i].DepartmentIndex in this.accessedDepartment) && listData[i].DepartmentIndex == 0) continue;
                if (!(listData[i].AreaIndex in this.filter.Area) && listData[i].AreaIndex === 0 && this.filter.Area.length != 0) continue;
                if (!(listData[i].DoorIndex in this.filter.Door) && listData[i].DoorIndex === 0 && this.filter.Door.length != 0) continue;
                const table = {
                    atid: listData[i].EmployeeATID,
                    fullname: listData[i].FullName,
                    time: moment(listData[i].CheckTime).format('YYYY-MM-DD HH:mm:ss'),
                    machine: listData[i].DeviceName,
                    machineserial: listData[i].SerialNumber,
                    inout: this.$t(listData[i].InOutModeString).toString(),
                    verify: this.$t(listData[i].VerifyMode).toString(),
                    facemask: this.$t(listData[i].FaceMask).toString(),
                    bodytemperature: this.$t(listData[i].BodyTemperature).toString(),
                    department: listData[i].Department,
                    departmentid: listData[i].DepartmentIndex,
                    areaname: listData[i].AreaName,
                    doorname: listData[i].DoorName,
                    areaindex: listData[i].AreaIndex,
                    doorindex: listData[i].DoorIndex
                };
                console.log(table);
                thisVue.tableData.unshift(table);
            }
        });
        this.connection
            .start()
            .then(() => {
                thisVue.isConnect = true;
                data
                    .invoke("AddUserToGroup", 2, self.localStorage.getItem("user"))
                    .catch(err => {
                        console.log(err.toString());
                    });
            })
            .catch(err => {
                thisVue.isConnect = false;
                console.log(err.toString());
            });
        this.connection.onclose(() => {
            thisVue.isConnect = false;
        });
    }

    getDataForTable() {
        attendanceLogApi
            .GetLastedACAttendanceLog()
            .then((res: any) => {
                if (res.status == 200) {
                    let arrLogs = res.data;
                    for (let i = 0; i < arrLogs.length; i++) {
                        this.tableData.push({
                            atid: arrLogs[i].EmployeeATID,
                            fullname: arrLogs[i].FullName,
                            time: moment(arrLogs[i].CheckTime).format('YYYY-MM-DD HH:mm:ss'),
                            machine: arrLogs[i].DeviceName,
                            machineserial: arrLogs[i].SerialNumber,
                            inout: this.$t(arrLogs[i].InOutMode).toString(),
                            verify: this.$t(arrLogs[i].VerifyMode).toString(),
                            facemask: this.$t(arrLogs[i].FaceMask).toString(),
                            bodytemperature: this.$t(arrLogs[i].BodyTemperature).toString(),
                            isoverbodytemperature: this.$t(arrLogs[i].IsOverBodyTemperature),
                            department: arrLogs[i].Department,
                            departmentid: arrLogs[i].DepartmentIndex,
                            areaname: arrLogs[i].AreaName,
                            doorname: arrLogs[i].DoorName,
                            areaindex: arrLogs[i].AreaIndex,
                            doorindex: arrLogs[i].DoorIndex,
                        });

                    }
                }
            })
            .catch(error => {
                this.$alertSaveError(null, null, "Thông báo", this.$t(error.response.data.Message).toString())
            });
    }
    tableRowClassName({ row }) {

        if (row.isoverbodytemperature) {
            return "warningrow";
        }
        return "";
    }

    get filteredTableData() {
        let result = [];
        result = [...this.tableData];
        if (this.filter.Machine.length != 0 || this.filter.Department.length != 0 || this.filter.Area.length != 0 || this.filter.Door.length != 0) {
            if (this.filter.Department.length != 0) {
                result = result.filter(e => this.filter.Department.some(m => m == e.departmentid));
            }
            else {
                result = result.filter(e => e.departmentid in this.accessedDepartment);
            }

            if (this.filter.Machine.length != 0) {
                result = result.filter(e => this.filter.Machine.some(m => m == e.machineserial));
            }
            if (this.filter.Area.length != 0) {
                result = result.filter(e => this.filter.Area.some(m => m == e.areaindex));
            }
            if (this.filter.Door.length != 0) {
                result = result.filter(e => this.filter.Door.some(m => m == e.doorindex));
            }
        }
        return result;
    }

    async getDepartment() {
        await departmentApi.GetDepartment().then((res: any) => {
            this.lstDepartment = res.data;
            this.lstDepartment.forEach(d => {
                if (!(d.value in this.accessedDepartment)) {
                    this.accessedDepartment[d.value] = d;
                }
            });
        })
            .catch(() => { });
    }

    async getDepartmentAndDeviceLookup() {
        await departmentAndDeviceApi.GetDepartmentAndDeviceLookup().then((res: any) => {
            this.departmentAndDeviceLookup = res.data;
        })
            .catch(() => { })
    }

    async getAllDevices() {
        await deviceApi.GetDeviceAll()
            .then(res => {
                this.deviceFilter = (res.data as any)
            })
    }
}
