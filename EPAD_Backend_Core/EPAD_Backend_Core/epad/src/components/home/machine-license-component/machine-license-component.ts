import { Component, Ref, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { deviceApi, IC_Device } from '@/$api/device-api';
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
    @Ref('uploadLicense') uploadLicense;
    page = 1;
    showDialog = false;
    checked = false;
    columns = [];
    rowsObj = [];
    devices = [];
    isEdit = false;
    usingBasicMenu: boolean = true;
    maxdeviceNumber: number = 0;
    showDialogImportLicense = false;
    showDialogImportMachine = false;

    dataDevice = [];
    ruleForm: IC_Device = {
        AliasName: '',
        SerialNumber: '',
        IPAddress: '',
        Port: 0,
        DeviceType: null,
        UseSDK: false,
        UsePush: false,
        DeviceName: '',
        IsSDK: '',
        IsPush: '',
        UserCount: 0,
        FingerCount: 0,
        AttendanceLogCount: 0,
        UserCapacity: null,
        AttendanceLogCapacity: null,
        FingerCapacity: null,
        FaceCapacity: null,
        LastConnection: '',
        Status: '',
        DeviceStatus: null,
        ConnectionCode: '',
        DeviceId: '',
        DeviceModel: '',
        DeviceModule: '',
        Note: ''
    };

    beforeMount() {
        this.columns = [
            {
                prop: 'SerialNumber',
                label: 'SerialNumber',
                minWidth: 120,
                sortable: true,
                fixed: true,
                display: true
            },
            {
                prop: 'AliasName',
                label: 'AliasName',
                minWidth: 200,
                sortable: true,
                display: true
            },
            {
                prop: 'IPAddress',
                label: 'IPAddress',
                minWidth: 130,
                sortable: true,
                display: true
            },
            {
                prop: 'HardWareLicenseExpireDate',
                label: 'ExpireDate',
                minWidth: 120,
                sortable: true,
                display: true
            }
        ];

        this.initRule();
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.usingBasicMenu = x.UsingBasicMenu;
            this.maxdeviceNumber = x.MaxDeviceNumber;
        });
    }
    initRule() {
    }

    mounted() {
        
    }

    async getData({ page, filter, sortParams, pageSize}) {
        this.page = page;
        return await deviceApi.GetDeviceAtPage(page, filter, pageSize).then((res) => {
            const { data } = res as any;
            this.dataDevice = data.data;
            this.dataDevice.forEach(element => {
                if(element.HardWareLicenseExpireDate && element.HardWareLicenseExpireDate != ""
                    && parseInt(element.HardWareLicenseExpireDate.split("/")[2]) >= 2099){
                        element.HardWareLicenseExpireDate = this.$t('ForeverLicense').toString();
                }
            });

            if (this.usingBasicMenu == true) {
                return {
                    data: this.dataDevice.slice(0, this.maxdeviceNumber),
                    total: data.total,
                };
            }
            else {
                return {
                    data: this.dataDevice,
                    total: data.total,
                };
            }
        });
    }
}
