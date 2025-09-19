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
import { privilegeMachineRealtimeApi } from '@/$api/privilege-machine-realtime-api';

import moment from "moment";
import { rootLink } from "../../../constant/variable";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
@Component({
    name: "log-realtime-monitoring",
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectTreeComponent, SelectDepartmentTreeComponent }
})
export default class LogMonitoringComponent extends Mixins(ComponentBase) {
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
    filter = { Department: [], Machine: [] }
    departmentAndDeviceLookup = [];
    deviceFilter = [];
    maxHeight = (window.innerHeight / 100 * 55);

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

    listDeviceSerial = [];
    apiGetAttendaceLog = rootLink + "api/AttendanceLog/GetLastedRealtimeAttendanceLog";

    dialogVisible: boolean = true;
    isFullscreenOn: boolean = false;

    avatar="";
    employeeATID = "";
    fullName = "";
    departmentName = "";
    logTime = "";    

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

    async GetDeviceSerial(){
        await privilegeMachineRealtimeApi.GetPrivilegeMachineRealtimeByUser().then((res: any) => {
            if (res.status == 200) {
                this.listDeviceSerial = res.data;
            }
        });
    }

    async LoadDepartmentTree() {
        await departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });
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
        await this.GetDeviceSerial();
        window.addEventListener("resize", () => {
            this.MaxHeightTable();
        });
        this.avatar = this.avatarDefault;
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
            {
                prop: "facemask",
                label: "FaceMask",
                minWidth: "80",
                sortable: true,
                display: true
            },
            {
                prop: "bodytemperature",
                label: "BodyTemperature",
                minWidth: "80",
                sortable: true,
                display: true
            }
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
                if (!(listData[i].DepartmentIndex in this.accessedDepartment) && listData[i].DepartmentIndex !== 0) continue;
                if((this.listDeviceSerial && this.listDeviceSerial.length > 0 
                    && this.listDeviceSerial.includes(listData[i].SerialNumber)) 
                    || !this.listDeviceSerial || this.listDeviceSerial.length == 0){
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
                            departmentid: listData[i].DepartmentIndex
                        };
                        this.employeeATID = listData[i].EmployeeATID;
                        this.fullName = listData[i].FullName;
                        this.departmentName = listData[i].Department;
                        this.logTime = moment(listData[i].CheckTime).format('YYYY-MM-DD HH:mm:ss');
                        this.avatar = (listData[i].Avatar ? ("data:image/jpeg;base64," + listData[i].Avatar) : this.avatarDefault);
                        thisVue.tableData.unshift(table);
                }
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
            .GetLastedRealtimeAttendanceLog()
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
                            departmentid: arrLogs[i].DepartmentIndex
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
        if (this.filter.Machine.length != 0 || this.filter.Department.length != 0) {
            if (this.filter.Department.length != 0) {
                result = result.filter(e => this.filter.Department.some(m => m == e.departmentid));
            }
            else {
                result = result.filter(e => e.departmentid in this.accessedDepartment);
            }

            if (this.filter.Machine.length != 0) {
                result = result.filter(e => this.filter.Machine.some(m => m == e.machineserial));
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

    avatarDefault="data:image/png;base64,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";
}
