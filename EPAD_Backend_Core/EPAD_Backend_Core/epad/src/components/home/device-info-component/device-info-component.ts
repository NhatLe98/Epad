import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import axios from "axios";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { apilink_GetDeviceInfo } from "../../../constant/variable";
import { deviceApi } from "@/$api/device-api";
import { userAccountApi } from "@/$api/user-account-api";
import { commandApi, CommandRequest } from "@/$api/command-api";
import { Form as ElForm } from 'element-ui'
import moment from "moment";
import { isNullOrUndefined } from 'util';
import DeviceInfoTable from '@/components/app-component/device-info-table/device-info-table.vue';
import { CommandResult } from "@/models/command-result";

@Component({
    name: "device-info",
    components: { HeaderComponent, DataTableComponent, DeviceInfoTable }
})
export default class DeviceInfoComponent extends Mixins(ComponentBase) {
    a = 0;
    page = 1;
    pageSize = 50;
    timer = null;
    loading = false;
    loading_button = false;
    columns = [];
    rowsObj = [];
    arraySerialNumber = [];
    data: any = [];
    isShowAttendance = false;
    isDeleteAttendance = false;
    usingBasicMenu: boolean = true;
    listIPAddress = null;
    isWaitingDownloadAttendanceResponse = false;
    openingConfirmDeletePopup = false;
    confirmDeletePassword = '';
    commandRequest: CommandRequest = {
        Action: "",
        ListSerial: null,
        ListUser: null,
        FromTime: null,
        ToTime: null,
        IsUsingTimeZone: false,
        TimeZone: null,
        Group: 0
    };
    TimeForm = {
        FromTime:moment(new Date().setHours(0, 0, 0)).format('YYYY-MM-DD HH:mm:ss'),
        ToTime: moment(new Date().setHours(23, 59, 59)).format('YYYY-MM-DD HH:mm:ss'),
        IsDownloadFull:false,
    };
    DeleteAttendaceTimeForm = {
        FromTime:moment(new Date().setHours(0, 0, 0)).format('YYYY-MM-DD HH:mm:ss'),
        ToTime: moment(new Date().setHours(23, 59, 59)).format('YYYY-MM-DD HH:mm:ss'),
        IsDeleteAll:false,
    };
    rule = {
        FromTime: [            
            {
                trigger: 'blur',
                message: this.$t('PleaseSelectTime'),
                validator: (rule, value: string, callback) => {
                    if (!this.TimeForm.IsDownloadFull && Misc.isEmpty(value) === true) {
                        callback(new Error());
                    }else if(!this.DeleteAttendaceTimeForm.IsDeleteAll && Misc.isEmpty(value) === true) {
                        callback(new Error());
                    }
                    callback();
                },
            },
        ],
        ToTime: [
          
            {
                trigger: 'blur',
                message: this.$t('PleaseSelectTime'),
                validator: (rule, value: string, callback) => {
                    if (!this.TimeForm.IsDownloadFull && Misc.isEmpty(value) === true) {
                        callback(new Error());
                    }else if(!this.DeleteAttendaceTimeForm.IsDeleteAll && Misc.isEmpty(value) === true) {
                        callback(new Error());
                    }
                    callback();
                },
            },
        ],
        IsDownloadFull: [
            {
                trigger: 'change',
                validator: (rule, value: string, callback) => {
                    callback();
                },
            },
        ]
    }
    setColumn() {
        this.columns = [
            {
                prop: "SerialNumber",
                label: "SerialNumber",
                minWidth: 120,
                fixed: true,
                display: true
            },
            {
                prop: "AliasName",
                label: "AliasName",
                minWidth: 160,
                fixed: true,
                display: true
            },
            {
                prop: "IPAddress",
                label: "IPAddress",
                minWidth: 130,
                fixed: true,
                display: true
            },
            {
                prop: "LastConnection",
                label: "LastConnection",
                minWidth: 200,
                display: true
            },
            {
                prop: "Status",
                label: "Status",
                minWidth: 200,
                display: true
            },
            {
                prop: "HardWareLicense",
                label: "HardWareLicense",
                minWidth: 200,
                display: true
            },
            {
                prop: "UserCount",
                label: "UserCount",
                minWidth: 200,
                display: true
            },
            {
                prop: "FingerCount",
                label: "FingerCount",
                minWidth: 200,
                display: true
            },
            {
                prop: "AttendanceLogCount",
                label: "AttendanceLogCount",
                minWidth: 220,
                display: true
            },
            {
                prop: "FaceCount",
                label: "FaceCount",
                minWidth: 200,
                display: true
            },
            {
                prop: "AdminCount",
                label: "AdminCount",
                minWidth: 200,
                display: true
            }
        ];
    }

    // beforeMount() {
    //   console.clear();
    // }

    created() {
        this.setTimer();
    }

    mounted() {
        this.setColumn();
    }

    beforeMount() {
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.usingBasicMenu = x.UsingBasicMenu;

        })
    }

    setTimer() {
        this.timer = setInterval(() => {
            return deviceApi
                .GetDeviceAtPage(this.page, "", this.pageSize)
                .then(res => {
                    const { data } = res as any;
                    return {
                        data: data.data,
                        total: data.total
                    };
                })
                .then(() => {
                    (this.$refs.table as any).getTableData(this.page, null, null);
                });
        }, 10000);
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page
        return await deviceApi.GetDeviceAtPage(page, filter, pageSize).then(res => {
            const { data } = res as any;
            //  data.data = data.data.map(e => ({...e, HardWareLicense: this.$t(e.License)}));
            return {
                data: data.data,
                total: data.total
            };
        });
    }

    CheckManualConnect(){
        var self = this;
        var message = '';
        var noIPArr = this.rowsObj.some(item => item.IPAddress === null)
        this.rowsObj.forEach(e => this.arraySerialNumber.push(e.SerialNumber));
        if (this.rowsObj.length == 0) {
            this.$alertSaveError(null, null, null, this.$t("PleaseSelectMachine").toString())
        }
        else if (noIPArr === true) {
            this.$alertSaveError(null, null, null, this.$t('PleaseAddIpAddressForMachine').toString())
        }
        else {
            var req = this.rowsObj.map(item => item.SerialNumber);
            commandApi.RestartServiceByDevice(req).then((res: any) => {

                message += self.$t("SendRequestSuccess").toString() + "<br/>";
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    message = `<p class="notify-content">${message}</p>`;
                    this.$notify({
                        type: 'success',
                        title: self.$t('NotificationFromDevice').toString(),
                        dangerouslyUseHTMLString: true,
                        message: message,
                        customClass: 'notify-content',
                        duration: 8000
                    });
                }
             });
        }
    }

    DownloadDeviceInfo() {
        var self = this;
        var message = '';
        var noIPArr = this.rowsObj.some(item => item.IPAddress === null)
        this.rowsObj.forEach(e => this.arraySerialNumber.push(e.SerialNumber));
        if (this.rowsObj.length == 0) {
            this.$alertSaveError(null, null, null, this.$t("PleaseSelectMachine").toString())
        }
        else if (noIPArr === true) {
            this.$alertSaveError(null, null, null, this.$t('PleaseAddIpAddressForMachine').toString())
        }
        else {
            var req = this.rowsObj.map(item => item.SerialNumber);
            this.commandRequest.ListSerial = req
            this.isWaitingDownloadAttendanceResponse = true;
            commandApi.GetDeviceInfo(this.commandRequest)
                .then((res) => {
                    const commandResults = res.data as CommandResult[] ?? [];
                    this.commandRequest.ListSerial.forEach((serialOnRequest => {
                        const commandRequest = this.rowsObj.find(x => x.SerialNumber == serialOnRequest);
                        if (commandResults && commandResults.find(x => x.SerialNumber == serialOnRequest)) {
                            message += commandRequest.IPAddress + ": " + self.$t("SendRequestSuccess").toString() + "<br/>";
                        } else {
                            message += commandRequest.IPAddress + ": " + this.$t('DeviceIsNotAvailable') + "<br/>";
                        }
                    }));

                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        message = `<p class="notify-content">${message}</p>`;
                        this.$notify({
                            type: 'success',
                            title: self.$t('NotificationFromDevice').toString(),
                            dangerouslyUseHTMLString: true,
                            message: message,
                            customClass: 'notify-content',
                            duration: 8000
                        });
                    }
                })
                .catch((err) => {
                })
                .finally(() => {
                    this.isWaitingDownloadAttendanceResponse = false;
                })
        }
    }
    RestartDevice() {
        var self = this;
        var message = '';
        this.rowsObj.forEach(e => this.arraySerialNumber.push(e.SerialNumber));
        if (this.rowsObj.length == 0) {
            this.$alertSaveError(null, null, null, this.$t("PleaseSelectMachine").toString())
        }
        else {
            commandApi.RestartDevice({
                ListSerial: this.rowsObj.map(x => x.SerialNumber),
                Action: 'Restart Device',
                ListUser: [],
                IsUsingTimeZone: false,
                TimeZone: []
            }).then((res) => {
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    this.listIPAddress = this.rowsObj.map(item => item.IPAddress);
                    this.listIPAddress.forEach(element => {
                        message += element.toString() + ": " + self.$t("SendRequestSuccess").toString() + "<br/>";
                    });
                    message = `<p class="notify-content">${message}</p>`;
                    this.$notify({
                        type: 'success',
                        title: self.$t('NotificationFromDevice').toString(),
                        dangerouslyUseHTMLString: true,
                        message: message,
                        customClass: 'notify-content',
                        duration: 8000
                    });
                }
            }).catch((err) => {
                console.log(err);
            })
        }
    }

    SetDeviceTime() {
        var self = this;
        var message = '';
        this.rowsObj.forEach(e => this.arraySerialNumber.push(e.SerialNumber));
        if (this.rowsObj.length == 0) {
            this.$alertSaveError(null, null, null, this.$t("PleaseSelectMachine").toString())
        }
        else {
            var req = this.rowsObj.map(item => item.SerialNumber);
            this.commandRequest.ListSerial = req;
            commandApi.SetDeviceTime(this.commandRequest)
                .then((res) => {
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.listIPAddress = this.rowsObj.map(item => item.IPAddress);
                        this.listIPAddress.forEach(element => {
                            message += element.toString() + ": " + self.$t("SendRequestSuccess").toString() + "<br/>";
                        });
                        message = `<p class="notify-content">${message}</p>`;
                        this.$notify({
                            type: 'success',
                            title: self.$t('NotificationFromDevice').toString(),
                            dangerouslyUseHTMLString: true,
                            message: message,
                            customClass: 'notify-content',
                            duration: 8000
                        });
                    }
                });
        }
    }

    FormAttendanceData() {
        //this.arraySerialNumber = []
        //this.rowsObj.forEach(e => this.arraySerialNumber.push(e.SerialNumber));
        var noIPArr = this.rowsObj.some(item => item.IPAddress === null)
        if (this.rowsObj.length == 0) {
            this.$alertSaveError(null, null, null, this.$t("PleaseSelectMachine").toString())
        }
        else if (noIPArr === true) {
            this.$alertSaveError(null, null, null, this.$t('PleaseAddIpAddressForMachine').toString())
        }
        else {
            this.isShowAttendance = true
        }
    }

    FormDeleteAttendanceData() {
        //this.arraySerialNumber = []
        //this.rowsObj.forEach(e => this.arraySerialNumber.push(e.SerialNumber));
        var noIPArr = this.rowsObj.some(item => item.IPAddress === null)
        if (this.rowsObj.length == 0) {
            this.$alertSaveError(null, null, null, this.$t("PleaseSelectMachine").toString())
        }
        else if (noIPArr === true) {
            this.$alertSaveError(null, null, null, this.$t('PleaseAddIpAddressForMachine').toString())
        }
        else {
            this.isDeleteAttendance = true
        }
    }
    DownloadAttendanceData() {
        var self = this;
        var message = '';
        (this.$refs.TimeForm as any).validate(async valid => {
            if (!valid) return
            else if (this.TimeForm.FromTime > this.TimeForm.ToTime) {
                this.$alertSaveError(null, null, this.$t('SaveDataFaild').toString(), this.$t('PleaseCheckCondition').toString())
            }
            else {
                var req = this.rowsObj.map(item => item.SerialNumber)
                this.commandRequest.ListSerial = req
                //console.log(moment(this.TimeForm.FromTime));
                let fromTime = moment(this.TimeForm.FromTime).format("YYYY-MM-DD hh:mm:ss a")
                let toTime = moment(this.TimeForm.ToTime).format("YYYY-MM-DD hh:mm:ss a")
                this.commandRequest.FromTime = fromTime;
                this.commandRequest.ToTime = toTime;
                this.commandRequest.IsDownloadFull = this.TimeForm.IsDownloadFull;
                console.log(this.commandRequest);
                commandApi.GetAttendanceData(this.commandRequest)
                    .then((res) => {
                        var ref = <ElForm>this.$refs.TimeForm
                        ref.resetFields()
                        this.isShowAttendance = false
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            if (!isNullOrUndefined(res.status) && res.status === 200) {
                                this.listIPAddress = this.rowsObj.map(item => item.IPAddress);
                                this.listIPAddress.forEach(element => {
                                    message += element.toString() + ": " + self.$t("SendRequestSuccess").toString() + "<br/>";
                                });
                                message = `<p class="notify-content">${message}</p>`;
                                this.$notify({
                                    type: 'success',
                                    title: self.$t('NotificationFromDevice').toString(),
                                    dangerouslyUseHTMLString: true,
                                    message: message,
                                    customClass: 'notify-content',
                                    duration: 8000
                                });
                            }
                        }
                    })
                    .catch((err) => {
                    })
            }
        })
    }

    DeleteAttendanceData() {
        this.openingConfirmDeletePopup = false;
        var self = this;
        var message = '';
        (this.$refs.DeleteAttendaceTimeForm as any).validate(async valid => {
            const fromTime = new Date(this.DeleteAttendaceTimeForm.FromTime);
            const toTime = new Date(this.DeleteAttendaceTimeForm.ToTime);
            if (!valid) return;
            else if (fromTime > toTime) {
                this.$alertSaveError(null, null, this.$t('SaveDataFaild').toString(), this.$t('PleaseCheckCondition').toString());
                return;
            }
            else {
                var req = this.rowsObj.map(item => item.SerialNumber)
                this.commandRequest.ListSerial = req;
                let fromTime = moment(this.DeleteAttendaceTimeForm.FromTime).format("YYYY-MM-DD hh:mm:ss a")
                let toTime = moment(this.DeleteAttendaceTimeForm.ToTime).format("YYYY-MM-DD hh:mm:ss a")
                this.commandRequest.FromTime = fromTime;
                this.commandRequest.ToTime = toTime;
                this.commandRequest.IsDeleteAll = this.DeleteAttendaceTimeForm.IsDeleteAll;
                userAccountApi.CheckValidPassword(localStorage.getItem('user'), this.confirmDeletePassword).then((res) => {
                    this.confirmDeletePassword = '';
                    if(!res.data)
                    {
                        this.$alertSaveError(null, null, this.$t('Warning').toString(), this.$t('WrongPassword').toString());
                        return;
                    }
                    commandApi.DeleteAttendanceData(this.commandRequest)
                    .then((res) => {
                        var ref = <ElForm>this.$refs.DeleteAttendaceTimeForm
                        ref.resetFields()
                        this.isShowAttendance = false
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            if (!isNullOrUndefined(res.status) && res.status === 200) {
                                this.listIPAddress = this.rowsObj.map(item => item.IPAddress);
                                this.listIPAddress.forEach(element => {
                                    message += element.toString() + ": " + self.$t("SendRequestSuccess").toString() + "<br/>";
                                });
                                message = `<p class="notify-content">${message}</p>`;
                                this.$notify({
                                    type: 'success',
                                    title: self.$t('NotificationFromDevice').toString(),
                                    dangerouslyUseHTMLString: true,
                                    message: message,
                                    customClass: 'notify-content',
                                    duration: 8000
                                });
                            }
                        }
                    })
                    .catch((err) => {
                    })
                    .finally(() => {
                        this.isDeleteAttendance = false;
                    })
                });                
            }
        })
    }

    CloseDeleteAttendanceLogForm(){
        this.openingConfirmDeletePopup = false; 
        this.isDeleteAttendance = false; 
        this.confirmDeletePassword = '';
    }

    Cancel() {
        var ref = <ElForm>this.$refs.TimeForm
        ref.resetFields()
        this.isShowAttendance = false
    }

    CancelDeleteAttendance() {
        var ref = <ElForm>this.$refs.DeleteAttendaceTimeForm
        ref.resetFields()
        this.isDeleteAttendance = false
    }

    beforeDestroy() {
        clearInterval(this.timer);
    }
}
