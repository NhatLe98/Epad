import { Component, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { configApi } from "@/$api/config-api";
import { attendanceLogApi } from "@/$api/attendance-log-api";
import moment from "moment";
import {HubConnectionBuilder, LogLevel} from '@microsoft/signalr'
@Component({
    name: "live-machine",
    components: { HeaderComponent, DataTableComponent }
})
export default class LiveMachineComponent extends Mixins(ComponentBase) {
    EmployeeATID = "";
    EmployeeCode = "";
    FullName = "";
    selectItem = [];
    dateTime = "";
    tableData = [];
    connection;
    isConnect = false;

    mounted() {
        this.getRealTimeServer();
        this.getDataForTable();
        const date = new Date();
        this.dateTime = moment(date.toString()).format("YYYY-MM-DD HH:mm:ss");
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

        // receive data from server
        this.connection.on("ReceiveAttendanceLog", listData => {
            for (let i = 0; i < listData.length; i++) {
                const table = {
                    atid: listData[i].EmployeeATID,
                    fullname: listData[i].FullName,
                    time: moment(listData[i].CheckTime).format("h:mm a"),
                    date: moment(listData[i].CheckTime).format("DD/MM/YYYY"),
                    machine: listData[i].SerialNumber,
                    inout: this.$t(listData[i].InOutMode).toString(),
                    verify: this.$t(listData[i].VerifyMode).toString()
                };
                this.tableData.unshift(table);
            }
        });
        this.connection
            .start()
            .then(() => {
                this.isConnect = true;
                this.connection
                    .invoke("AddUserToGroup", 2, self.localStorage.getItem("user"))
                    .catch(err => {
                        console.log(err.toString());
                    });
            })
            .catch(err => {
                this.isConnect = false;
                console.log(err.toString());
            });
        this.connection.onclose(() => {
            this.isConnect = false;
        });
    }

    getDataForTable() {
        attendanceLogApi
            .GetLastedAttendanceLog()
            .then((res: any) => {
                if (res.status == 200) {
                    let arrLogs = res.data;
                    for (let i = 0; i < arrLogs.length; i++) {
                        this.tableData.push({
                            atid: arrLogs[i].EmployeeATID,
                            fullname: arrLogs[i].FullName,
                            time: moment(new Date(arrLogs[i].CheckTime)).format("h:mm a"),
                            date: moment(new Date(arrLogs[i].CheckTime)).format("DD/MM/YYYY"),
                            machine: arrLogs[i].SerialNumber,
                            inout: this.$t(arrLogs[i].InOutMode).toString(),
                            verify: this.$t(arrLogs[i].VerifyMode).toString()
                        });
                    }
                }
            })
            .catch(error => {
                this.$alertSaveError(null, null, this.$t('Notify').toString(), this.$t(error.response.data.Message).toString())
            });
    }
}
