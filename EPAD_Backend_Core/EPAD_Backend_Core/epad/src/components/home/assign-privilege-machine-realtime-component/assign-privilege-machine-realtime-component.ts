import { Component, Ref, Mixins, Watch } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { deviceApi, IC_Device } from '@/$api/device-api';
import { userAccountApi } from '@/$api/user-account-api';
import { privilegeMachineRealtimeApi, IC_PrivilegeMachineRealtimeDTO } from '@/$api/privilege-machine-realtime-api';
import { licenseApi } from '@/$api/license-api';
import { Form as ElForm } from 'element-ui';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import * as XLSX from 'xlsx';
import { isNullOrUndefined } from 'util';
import { serviceApi } from '@/$api/service-api';
import { groupDeviceApi } from '@/$api/group-device-api';

@Component({
    name: 'machine',
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class MachineComponent extends Mixins(ComponentBase) {
    page = 1;
    showDialog = false;
    checked = false;
    columns = [];
    rowsObj = [];
    devices = [];
    isEdit = false;
    isFormOpening = false;
    usingBasicMenu: boolean = true;
    maxdeviceNumber: number = 0;

    dataDevice = [];
    listAllGroupDevice = [];
    listAllUserAccount = [];
    listDevice = [];
    listFullDevice = [];

    ruleForm: IC_PrivilegeMachineRealtimeDTO = {
        UserName: '',
        ListUserName: [],
        PrivilegeGroup: 0,
        PrivilegeGroupName: '',
        GroupDeviceIndex: '',
        GroupDeviceName: '',
        ListGroupDeviceIndex: [],
        ListGroupDeviceName: [],
        DeviceModule: '',
        DeviceModuleName: '',
        ListDeviceModule: [],
        ListDeviceModuleName: [],
        DeviceSerial: '',
        DeviceName: '',
        ListDeviceSerial: [],
        ListDeviceName: [],
    };
    checkList = [];

    rules: any = {};

    deviceModules = [
        {
            index: "TA",
            value: 'Chấm công',
        },
        {
            index: "GCS",
            value: 'Quản lý cổng',
        },
        {
            index: "AC",
            value: 'Kiểm soát truy cập',
        },
        {
            index: "PA",
            value: 'Bãi xe',
        },
        {
            index: "ICMS",
            value: 'Nhà ăn',
        },
    ];

    privilegeGroup = [
        {
            index: 0,
            value: this.$t('OnDevice'),
        },
        {
            index: 1,
            value: this.$t('OnGroupDevice'),
        },
        {
            index: 2,
            value: this.$t('OnDeviceModule'),
        }
    ];

    beforeMount() {
        this.initColumns();
        this.initRule();
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.usingBasicMenu = x.UsingBasicMenu;
            this.maxdeviceNumber = x.MaxDeviceNumber;
        });

        this.getGroupDevice();
        this.getUserAccount();
        this.getDevice();
    }
    initRule() {
        this.rules = {
            ListUserName: [
                {
                    required: true,
                    message: this.$t('PleaseSelectAccount'),
                    trigger: 'blur',
                },
                {
                    validator: (rule, value, callback) => {
                        if (!value || value.length == 0) {
                            callback(new Error(this.$t('PleaseSelectAccount').toString()));
                        }
                        else {
                            callback();
                        }
                    },
                    trigger: 'blur'
                }
            ],
            ListDeviceSerial: [
                {
                    required: true,
                    message: this.$t('PleaseSelectMachine'),
                    trigger: 'blur',
                },
                {
                    validator: (rule, value, callback) => {
                        if (!value || value.length == 0) {
                            callback(new Error(this.$t('PleaseSelectMachine').toString()));
                        }
                        else {
                            callback();
                        }
                    },
                    trigger: 'blur'
                }
            ],
        };
    }
    mounted() { 
        
    }

    initColumns(){
        this.columns = [
            {
                prop: 'UserName',
                label: 'UserName',
                minWidth: 60,
                sortable: true,
                fixed: true,
                display: true
            },
            {
                prop: 'DeviceName',
                label: 'Device',
                minWidth: 200,
                sortable: true,
                fixed: false,
                display: true
            },
        ];
    }

    displayPopupInsert() {
        this.showDialog = false;
    }

    Reset() {
        const obj: IC_PrivilegeMachineRealtimeDTO = {
            UserName: '',
            ListUserName: [],
            PrivilegeGroup: 0,
            PrivilegeGroupName: '',
            GroupDeviceIndex: '',
            GroupDeviceName: '',
            ListGroupDeviceIndex: [],
            ListGroupDeviceName: [],
            DeviceModule: '',
            DeviceModuleName: '',
            ListDeviceModule: [],
            ListDeviceModuleName: [],
            DeviceSerial: '',
            DeviceName: '',
            ListDeviceSerial: [],
            ListDeviceName: [],
        };
        this.ruleForm = obj;
    }

    async getData({ page, filter, sortParams, pageSize}) {
        this.page = page;
        return await privilegeMachineRealtimeApi.GetAllPrivilegeMachineRealtime(page, filter, pageSize).then((res) => {
            const { data } = res as any;

            return {
                data: data.data,
                total: data.total,
            };
        });
    }

    Insert() {
        this.Reset();
        this.filterDevice();
        this.showDialog = true;
        this.isEdit = false;
        this.isFormOpening = true;
    }

    async Submit() {
        (this.$refs.ruleForm as any).validate(async (valid) => {
            if (!valid) return;
            else {
                this.isLoading = true;
                if (this.isEdit == true) {
                    // deviceApi.UpdateDevice(this.ruleForm).then((res) => {
                    //     (this.$refs.table as any).getTableData(this.page, null, null);
                    //     this.showDialog = false;
                    //     this.isLoading = false;
                    //     this.Reset();
                    //     if (!isNullOrUndefined(res.status) && res.status === 200) {
                    //         this.$saveSuccess();
                    //     }
                    // });
                    privilegeMachineRealtimeApi.UpdatePrivilegeMachineRealtime(this.ruleForm).then((res) => {
                        if(res.data){
                            let message = "";
                            (res.data as any).forEach(element => {
                                message += this.$t(element.split(":/:")[0], {
                                    userName: element.split(":/:")[1]
                                }).toString();
                            });
                            // message = `<p class="notify-content">${message}</p>`;
                            // this.$notify({
                            //     type: 'warning',
                            //     title: this.$t('Warning').toString(),
                            //     dangerouslyUseHTMLString: true,
                            //     message: message,
                            //     customClass: 'notify-content',
                            //     duration: 8000
                            // });
                            this.$alert(
                                message,
                                this.$t("Error").toString(),
                                { type: "error" }
                            );
                            return;
                        }
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.isFormOpening = false;
                        this.isLoading = false;
                        this.Reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                        }
                    });
                } else {
                    // deviceApi.AddDevice(this.ruleForm).then((res) => {
                    //     (this.$refs.table as any).getTableData(this.page, null, null);
                    //     this.showDialog = false;
                    //     this.isLoading = false;
                    //     this.Reset();
                    //     if (!isNullOrUndefined(res.status) && res.status === 200) {
                    //         this.$saveSuccess();
                    //     }
                    // });
                    privilegeMachineRealtimeApi.AddPrivilegeMachineRealtime(this.ruleForm).then((res) => {
                        if(res.data){
                            let message = "";
                            (res.data as any).forEach(element => {
                                message += this.$t(element.split(":/:")[0], {
                                    userName: element.split(":/:")[1]
                                }).toString();
                            });
                            // message = `<p class="notify-content">${message}</p>`;
                            // this.$notify({
                            //     type: 'warning',
                            //     title: this.$t('Warning').toString(),
                            //     dangerouslyUseHTMLString: true,
                            //     message: message,
                            //     customClass: 'notify-content',
                            //     duration: 8000
                            // });
                            this.$alert(
                                message,
                                this.$t("Error").toString(),
                                { type: "error" }
                            );
                            return;
                        }
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.isFormOpening = false;
                        this.isLoading = false;
                        this.Reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                        }
                    });
                }
            }
        });
    }

    async getGroupDevice() {
        return await groupDeviceApi.GetGroupDevice().then((res: any) => {
            if (res.status == 200) {
                const arrGroupDevice = res.data;
                for (let i = 0; i < arrGroupDevice.length; i++) {
                    this.listAllGroupDevice.push({
                        index: parseInt(arrGroupDevice[i].value),
                        device: arrGroupDevice[i].label
                    });
                }
            }
        });
    }

    async getUserAccount() {
        return await userAccountApi.GetAllUserAccount().then((res: any) => {
            if (res.status == 200) {
                const arrGroupDevice = res.data;
                for (let i = 0; i < arrGroupDevice.length; i++) {
                    this.listAllUserAccount.push({
                        index: arrGroupDevice[i].UserName,
                        name: arrGroupDevice[i].UserName
                    });
                }
            }
        });
    }

    async getDevice() {
        return await deviceApi.GetListDeviceInfo().then((res: any) => {
            if (res.status == 200 && res.data && res.data.data) {
                const data = res.data.data;
                data.forEach(element => {
                    element.AliasName = element.AliasName + ((element.IPAddress && element.IPAddress != "") 
                    ? (" (" + element.IPAddress + ")") : "")
                });
                this.listFullDevice = data;
                this.listDevice = data;
            }
        });
    }

    get allDevice() {
        if (
            this.listDevice.length > 0 && 
            this.ruleForm.ListDeviceSerial.length ===
            [...this.listDevice].map((item) => item.SerialNumber).length
        ) {
            return "DeselectAll";
        } else if(this.listDevice.length == 0) {
            return "SelectAll";
        } else {
            return "SelectAll";
        }

    }

    @Watch("ruleForm.ListDeviceSerial")
    onChangeSelectDevice(){
        if (this.ruleForm.ListDeviceSerial.indexOf("SelectAll") !== -1) {
            this.ruleForm.ListDeviceSerial = [...this.listDevice].map(
                (item) => item.SerialNumber
            );
        }
        if (this.ruleForm.ListDeviceSerial.indexOf("DeselectAll") !== -1) {
            this.ruleForm.ListDeviceSerial = [];
        }
    }

    @Watch("ruleForm.ListGroupDeviceIndex")
    @Watch("ruleForm.ListDeviceModule")
    @Watch("ruleForm.PrivilegeGroup")
    filterDevice() {
        if(this.isFormOpening){
            this.listDevice = this.listFullDevice;
            if (this.ruleForm.PrivilegeGroup == 1 && this.ruleForm.ListGroupDeviceIndex 
                && this.ruleForm.ListGroupDeviceIndex.length > 0) {
                this.ruleForm.ListDeviceSerial = [];
                this.listDevice = this.listDevice.filter(x => this.ruleForm.ListGroupDeviceIndex.includes(x.GroupDeviceId));
            }
            if (this.ruleForm.PrivilegeGroup == 2 && this.ruleForm.ListDeviceModule 
                && this.ruleForm.ListDeviceModule.length > 0) {
                this.ruleForm.ListDeviceSerial = [];
                this.listDevice = this.listDevice.filter(x => this.ruleForm.ListDeviceModule.includes(x.DeviceModule));
            }
        }
    }

    Edit() {
        this.isEdit = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (obj.length == 1) {
            this.showDialog = true;
            this.ruleForm = obj[0];
            setTimeout(() => {
                this.isFormOpening = true;
                const backupDeviceSerial = this.ruleForm.ListDeviceSerial;
                this.filterDevice();
                this.ruleForm.ListDeviceSerial = backupDeviceSerial;
            }, 200);
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }

    async Delete() {
        const obj: IC_PrivilegeMachineRealtimeDTO[] = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            this.$confirmDelete().then(async () => {
                await privilegeMachineRealtimeApi
                    .DeletePrivilegeMachineRealtime(obj)
                    .then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$deleteSuccess();
                        }
                    })
                    .catch(() => { });
            });
        }
    }

    Cancel() {        
        var ref = <ElForm>this.$refs.ruleForm;
        ref.resetFields();
        this.showDialog = false;
        this.filterDevice();
        this.Reset();
        this.isFormOpening = false;
    }

    focus(x) {
        (this.$refs[x] as any).focus();
    }
}
