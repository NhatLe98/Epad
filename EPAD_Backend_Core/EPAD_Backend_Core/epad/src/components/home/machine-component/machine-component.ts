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
    listExcelFunction = ['AddExcel', 'UpdateLicense'];
    sheetIndex = 0;
    fileName = '';
    formExcel = {};
    dataAddExcel = [];

    dataDevice = [];
    listAllService = [];
    listAllDevice = [];

    listAllProducer = [];

    listLicenseFile = [];
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
        Note: '',
        IsUsingOffline: false
    };
    checkList = [];

    rules: any = {};

    importColumns = [
        {
            name: 'Số Serial (*)',
            prop: 'SerialNumber',
            dataType: 'string',
            display: true
        },
        {
            name: 'Tên máy',
            prop: 'AliasName',
            dataType: 'string',
            display: true
        },
        {
            name: 'Địa chỉ IP',
            prop: 'IPAddress',
            dataType: 'string',
            display: true
        },
        {
            name: 'Cổng',
            prop: 'Port',
            dataType: 'number',
            display: true
        },
        {
            name: 'Loại thiết bị (*)',
            prop: 'DeviceType',
            dataType: 'lookup',
            display: true,
            lookup: {
                dataSource: [
                    {
                        key: 'Thẻ', value: 0
                    },
                    {
                        key: 'Vân tay', value: 1
                    },
                    {
                        key: 'Khuôn mặt', value: 2,
                    },
                    {
                        key: 'Thẻ + vân tay', value: 3
                    },
                    {
                        key: 'Thẻ + vân tay + khuôn mặt', value: 4
                    }
                ],
                displayMember: 'key',
                valueMember: 'value'
            }
        }
    ]

    machineType = [
        {
            index: 0,
            value: 'Card',
        },
        {
            index: 1,
            value: 'Finger',
        },
        {
            index: 2,
            value: 'FaceTemplate',
        },
        {
            index: 3,
            value: 'Card+Finger',
        },
        {
            index: 4,
            value: 'Card+Finger+FaceTemplate',
        },
    ];

    deviceStatus = [
        {
            index: 0,
            value: 'Không xác định',
        },
        {
            index: 1,
            value: 'Vào',
        },
        {
            index: 2,
            value: 'Ra',
        },
        {
            index: 3,
            value: 'Ra giữa giờ',
        },
        {
            index: 4,
            value: 'Vào giữa giờ',
        },
    ];

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

    parkingList = [
        {
            index: 0,
            value: 'Chuẩn',
        },
        {
            index: 1,
            value: 'Vinparking',
        },
        {
            index: 2,
            value: 'Lovad',
        }
    ]

    beforeMount() {
        this.initColumns();
        this.initRule();
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.usingBasicMenu = x.UsingBasicMenu;
            this.maxdeviceNumber = x.MaxDeviceNumber;
        });
        this.getAllDataService();
        this.getGroupDevice();
        this.GetProducerEnumList();
    }
    initRule() {
        this.rules = {
            SerialNumber: [
                {
                    required: true,
                    message: this.$t('PleaseInputSerialNumber'),
                    trigger: 'blur',
                },
            ],
            DeviceType: [
                {
                    required: true,
                    message: this.$t('PleaseSelectMachine'),
                    trigger: 'blur',
                },
                // {
                //   validator: (rule, value, callback) => {
                //     if (isNullOrUndefined(value) === true || value === 0) {
                //       callback(new Error("Vui lòng chọn máy chấm công"));
                //     }
                //     else {
                //       callback();
                //     }
                //   }
                // }
            ],
            Port: [
                {
                    message: this.$t('PortOnlyAcceptsIntegers'),
                    validator: (rule, value, callback) => {
                        var regex = /^[0-9]*$/;
                        if (isNullOrUndefined(value) === false && regex.test(value) === false) {
                            callback(new Error());
                        } else {
                            callback();
                        }
                    },
                },
            ],
        };
    }
    mounted() {
        
    }

    initColumns() {
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
                fixed: true,
                display: true
            },
            {
                prop: 'IPAddress',
                label: 'IPAddress',
                minWidth: 130,
                sortable: true,
                fixed: true,
                display: true
            },
            {
                prop: 'HardWareLicense',
                label: 'HardWareLicense',
                minWidth: 120,
            },
            {
                prop: 'Port',
                label: 'Port',
                minWidth: 90,
                sortable: true,
                display: true
            },
            {
                prop: 'DeviceName',
                label: 'DeviceType',
                minWidth: 200,
                sortable: true,
                display: true
            },
            {
                prop: 'DeviceModuleName',
                label: 'DeviceModule',
                minWidth: 200,
                sortable: true,
                display: true
            },
            {
                prop: 'Note',
                label: 'Note',
                minWidth: 120,
                sortable: true,
                display: true
            }
        ];
    }

    displayPopupInsert() {
        this.showDialog = false;
    }

    Reset() {
        const obj: IC_Device = {};
        this.ruleForm = obj;
    }

    async getData({ page, filter, sortParams, pageSize}) {
        this.page = page;
        return await deviceApi.GetDeviceAtPage(page, filter, pageSize).then((res) => {
            const { data } = res as any;
            this.dataDevice = data.data;
            data.data.forEach((element) => {
                switch (element.DeviceType) {
                    case 0:
                        element.DeviceName = this.$t('Card');
                        break;
                    case 1:
                        element.DeviceName = this.$t('Finger');
                        break;
                    case 2:
                        element.DeviceName = this.$t('FaceTemplate');
                        break;
                    case 3:
                        element.DeviceName = this.$t('Card+Finger');
                        break;
                    case 4:
                        element.DeviceName = this.$t('Card+Finger+FaceTemplate');
                        break;
                    default:
                        element.DeviceName = '';
                        break;
                };
                element.DeviceModuleName = this.$t(element.DeviceModuleName).toString();
            });

            if (this.usingBasicMenu == true) {
                return {
                    data: data.data.slice(0, this.maxdeviceNumber),
                    total: data.total,
                };
            }
            else {
                return {
                    data: data.data,
                    total: data.total,
                };
            }
        });
    }

    Insert() {
        this.Reset();
        this.showDialog = true;
        this.isEdit = false;
    }

    async Submit() {
        (this.$refs.ruleForm as any).validate(async (valid) => {
            if (!valid) return;
            else {
                this.isLoading = true;
                if (this.isEdit == true) {
                    deviceApi.UpdateDevice(this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.isLoading = false;
                        this.Reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                        }
                    });
                } else {
                    if (this.usingBasicMenu == true && this.dataDevice.length >= this.maxdeviceNumber) {
                        this.$alertSaveError(null, null, null, this.$t('OverMaxDeviceNumber').toString());
                        return;
                    }
                    deviceApi.AddDevice(this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
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

    async getAllDataService() {
        return await serviceApi.GelAllService().then((res: any) => {
            if (res.status == 200) {
                const arrGroupService = res.data;
                for (let i = 0; i < arrGroupService.length; i++) {
                    this.listAllService.push({
                        index: arrGroupService[i]?.Index ?? 0,
                        service: arrGroupService[i]?.Name ?? ''
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
                    this.listAllDevice.push({
                        index: parseInt(arrGroupDevice[i].value),
                        device: arrGroupDevice[i].label
                    });
                }
            }
        });
    }

    async GetProducerEnumList(){
        return await deviceApi.GetProducerEnumList().then((res: any) => {
            if(res.status == 200){
                this.listAllProducer = res.data;
            }
        })
    }

    async SubmitAndNext() {
        (this.$refs.ruleForm as any).validate(async (valid) => {
            if (!valid) return;
            else {
                this.isLoading = true;
                if (this.isEdit == true) {
                    deviceApi.UpdateDevice(this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.isLoading = false;
                        this.Reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                        }
                    });
                } else {
                    if (this.usingBasicMenu == true && this.dataDevice.length >= this.maxdeviceNumber) {
                        this.$alertSaveError(null, null, null, this.$t('OverMaxDeviceNumber').toString());
                        return;
                    }
                    deviceApi.AddDevice(this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = true;
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

    Edit() {
        this.isEdit = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (obj.length == 1) {
            this.showDialog = true;
            this.ruleForm = obj[0];
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }

    async Delete() {
        const obj: IC_Device[] = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            this.$confirmDelete().then(async () => {
                await deviceApi
                    .DeleteDevice(obj)
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
    }

    focus(x) {
        (this.$refs[x] as any).focus();
    }

    async SubmitUploadLicense() {
        await licenseApi
            .AddHardwareLicense(this.listLicenseFile)
            .then(() => {
                this.$saveSuccess();
                (this.$refs.table as any).getTableData(this.page, null, null);
                this.closePopupUpload();
            })
            .catch(() => { });
    }

    uploadLicenseFile(file, fileList) {
        this.listLicenseFile = [];
        fileList.forEach((file) => {
            const reader = new FileReader();
            reader.readAsText(file.raw, 'UTF-8');
            reader.onload = (evt) => {
                const LicenseData = evt.target.result;
                this.listLicenseFile.push({ FileName: file.name, Key: LicenseData });
            };
        });
    }

    closePopupUpload() {
        this.uploadLicense.clearFiles;
        this.showDialogImportLicense = false;
    }

    closeDialogImportMachine() {
        (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
        this.showDialogImportMachine = false;
        this.fileName = '';
    }

    processFile(files: any[]) {
        if (files.length > 0) {
            const file = files[0];
            this.fileName = file.name;
            if (!isNullOrUndefined(file)) {
                const fileReader = new FileReader();

                fileReader.onload = (m: any) => {
                    const bStr = m.target.result;
                    this.readExcelFile(bStr);
                }
                fileReader.readAsArrayBuffer(file);
            }
        }
    }

    readExcelFile(bStr) {
        const wb = XLSX.read(bStr, {
            type: 'buffer',
            raw: false,
        });
        const sheet = wb.Sheets[wb.SheetNames[this.sheetIndex]];
        const rawArr = XLSX.utils.sheet_to_json(sheet, {
            header: 1,
            blankrows: false,
            defval: null,
            raw: false,
        });

        this.dataAddExcel = rawArr;
        console.log('rawArr', rawArr)
    }

    async UploadDataFromExcel() {
        if (this.dataAddExcel.length == 0) return;
        const arrData = [];
        const regex = /^\d+$/;
        const header = this.dataAddExcel[0];
        const rows = this.dataAddExcel.slice(1);
        let hasError = false;

        //console.log('header', header)
        //console.log('rows', rows);



        rows.forEach((row, rowIndex) => {
            const machineModel = { ...this.ruleForm };
            this.importColumns.forEach((col, colIndex) => {
                if (col.dataType === 'number' && regex.test(row[colIndex]) == false) {
                    this.$alertSaveError(null, null, null, `${col.name}: ${this.$t('OnlyNumeric')}`).toString();
                    hasError = true;
                    return;
                }

                if (col.name.indexOf('(*)') >= 0 && Misc.isEmpty(row[colIndex])) {
                    this.$alertSaveError(null, null, null, `${col.name}: ${this.$t('NotBeBlank')}`).toString();
                    hasError = true;
                    return;
                }

                if (col.dataType === 'lookup') {
                    const key = row[colIndex];
                    if (key !== null && key !== undefined && key !== '') {
                        const lookup = col.lookup.dataSource.find(x => x.key.toUpperCase() === key.toUpperCase());
                        if (Misc.isEmpty(lookup)) {
                            this.$alertSaveError(null, null, null, `${this.$t('LineNumber')}: ${rowIndex + 1} - ${col.name}: ${this.$t('InvalidValue')}`).toString();
                            hasError = true;
                            return;
                        }
                        else {
                            machineModel[col.prop] = lookup.value;
                        }
                    }
                }
                else {
                    machineModel[col.prop] = row[colIndex];
                }
            });
            arrData.push(machineModel);
        });

        if (this.usingBasicMenu == true && (arrData.length >= this.dataDevice.length || arrData.length >= this.maxdeviceNumber || this.dataDevice.length >= this.maxdeviceNumber)) {
            this.$alertSaveError(null, null, null, this.$t('OverMaxDeviceNumber').toString());
            return;
        }
        if (hasError) return;

        await deviceApi.ImportDevice(arrData).then(res => {
            this.closeDialogImportMachine();
            (this.$refs.table as any).getTableData(this.page, null, null);
            if (!isNullOrUndefined(res.status) && res.status === 200) {
                this.$saveSuccess();
            }
        });
    }
}
