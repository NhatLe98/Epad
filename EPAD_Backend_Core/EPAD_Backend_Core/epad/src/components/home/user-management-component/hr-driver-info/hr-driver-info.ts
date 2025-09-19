import { Component, Vue, Mixins, Model, Watch, Prop } from 'vue-property-decorator';
import { departmentApi } from '@/$api/department-api';
import TabBase from '@/mixins/application/tab-mixins';
import { employeeInfoApi, Finger } from '@/$api/employee-info-api';
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import { hrPositionInfoApi } from '@/$api/hr-position-info-api';
import { employeeTypesApi } from '@/$api/ic-employee-type-api';
import { isNullOrUndefined } from "util";
import * as XLSX from 'xlsx';
import { store } from '@/store';
import VisualizeTable from '@/components/app-component/visualize-table/visualize-table.vue';
import AppPagination from '@/components/app-component/app-pagination/app-pagination.vue';
import ImageCellRendererVisualizeTable from '@/components/app-component/visualize-table/image-cell-renderer-visualize-table.vue';
import { commandApi } from '@/$api/command-api';
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { userTypeApi } from '@/$api/user-type-api';
import TableToolbar from '@/components/home/printer-control/table-toolbar.vue';
import { hrDriverInfoApi } from '@/$api/hr-driver-info';
import { truckerDriverLogApi } from '@/$api/gc-trucker-driver-log-api';

@Component({
    name: 'hr-driver-info',
    components: {
        VisualizeTable,
        AppPagination,
        ImageCellRendererVisualizeTable,
        SelectTreeComponent,
        SelectDepartmentTreeComponent,
        TableToolbar
    },
})
export default class HRDriverInfo extends Mixins(TabBase) {
    //===== for WS Register finger
    showFingerDialog = false;
    wsUri = "ws://127.0.0.1:22003";
    websocket;
    currentIndex: number = 0;
    DeviceInfo: string = this.$t('NotConnectedDevice').toString();
    ConnectedDevice: number = 0;
    template1: string = "";
    template2: string = "";
    registerCount: number = 0;
    listFinger: Array<Finger> = [];
    listAllPosition = [];
    listAllPositionLookup = {};
    columns = [];
    ruleForm: any = {}
    ListFingerRequest: any;
    clientName: string;
    @Prop({ default: () => false }) showMore: boolean;
    value = '';
    shouldResetColumnSortState = false;
    //=====================
    fileImageName = '';
    errorUpload = false;
    fileList = [];
    lstStatus = [
        { Value: '0001', Label: 'Đăng tài' },
        { Value: '0002', Label: 'Xe vào cổng' },
        { Value: '0003', Label: 'Xe ra cổng' },
    ]
    filterModel = { Status: [], TextboxSearch: '', FromDate: null, ToDate: null, VehiclePlate: '', Filter: '', SelectedDepartment: [] };
    dockStatus = []
    newInputContactInfo = {
        Name: '',
        Email: '',
        Phone: ''
    };
    listEmployeeType = [];
    currentSelectEmployeeATID = '';
    listTemplateFinger = [] as any;
    listWorkingStatus = [0];
    listContactInfoFormApi = [];
    isLoading = false;
    fromDate = new Date();
    toDate = new Date();
    SelectedDepartment = [];
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
    checkboxRule: any;
    columnDefs: any[] = [
        {
            field: 'index1',
            sortable: true,
            pinned: true,
            headerName: '#',
            width: 80,
            checkboxSelection: true,
            headerCheckboxSelection: true,
            headerCheckboxSelectionFilteredOnly: true,
            display: true
        },
        {
            field: 'Avatar',
            headerName: this.$t('Avatar'),
            pinned: true,
            sortable: true,
            width: 150,
            cellRenderer: 'ImageCellRendererVisualizeTable',
            display: true
        },
        {
            field: 'TripId',
            headerName: this.$t('TripCode'),
            pinned: true,
            width: 150,
            sortable: true,
            display: true
        },
        {
            field: 'OrderCode',
            headerName: this.$t('OrderCode'),
            pinned: true,
            width: 150,
            sortable: true,
            display: true
        },
        {
            field: 'LocationOperator',
            headerName: this.$t('DeliveryPoint'),
            pinned: true,
            width: 150,
            sortable: true,
            display: true
        },
        {
            field: 'StatusString',
            headerName: this.$t('Status'),
            pinned: true,
            width: 150,
            sortable: true,
            display: true
        },
        {
            field: 'DriverName',
            headerName: this.$t('FullName'),
            pinned: true,
            width: 170,
            sortable: true,
            display: true
        },
        {
            field: 'TrailerNumber',
            headerName: this.$t('VehiclePlate'),
            pinned: false,
            width: 170,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('CCCDNumber'),
            field: 'DriverCode',
            pinned: false,
            width: 170,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('BirthDay'),
            field: 'BirthDay',
            pinned: false,
            width: 150,
            sortable: true,
            cellRenderer: params => `${params.value != null ? moment(params.value).format('DD/MM/YYYY') : ""}`,
            display: true
        },
        {
            field: 'DriverPhone',
            headerName: this.$t('MobilePhone'),
            pinned: false,
            width: 170,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('PassingVehicle'),
            field: 'Vc',
            pinned: false,
            width: 150,
            sortable: true,
            display: true,
            cellDataType: 'boolean',

            cellRenderer: params => {
                return `<input type='checkbox'  ${params.value ? 'checked' : ''} disabled/>`;
            }
        },
        {
            field: 'StatusDockString',
            headerName: this.$t('VehicleStatus'),
            pinned: false,
            width: 170,
            sortable: true,
            display: true
        },
        {
            field: 'Eta',
            headerName: this.$t('ETA'),
            pinned: false,
            width: 250,
            sortable: true,
            cellRenderer: params => `${params.value != null ? moment(params.value).format('DD/MM/YYYY HH:mm:ss') : ""}`,
            display: true
        },
        {
            field: 'TimesDockString',
            headerName: this.$t('PlanDockTime'),
            pinned: false,
            width: 170,
            sortable: true,
            display: true
        },
        {
            field: 'FromDate',
            headerName: this.$t('GateEntryTime'),
            pinned: false,
            width: 170,
            sortable: true,
            display: true
        },

        {
            field: 'ToDate',
            headerName: this.$t('GateExitTime'),
            pinned: false,
            width: 170,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('Type'),
            field: 'Type',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('Supplier'),
            field: 'Supplier',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('Activity'),
            field: 'Operation',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        }
    ];

    onSelectionChange(selectedRows: any[]) {
        this.selectedRows = selectedRows;
    }
    async beforeMount() {
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.clientName = x.ClientName;
                this.value = '';
        });

        truckerDriverLogApi.InfoTruckDriverTemplateImport().then((res: any) => {
            console.log(res);
        })

    }

    async initLookup() {
        //await this.getDepartment();
        this.initPositionInfoLookup();
        await this.getApiContactInfo();
        await this.getEmployeeType();
        this.LoadDepartmentTree();
    }

    LoadDepartmentTree() {
        departmentApi.GetDepartmentTreeEmployeeScreen("8").then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });

    }

    initFormRules() {
        this.formRules = {
            TripId: [
                {
                    required: true,
                    message: this.$t('PleaseInputTripCode'),
                    trigger: 'change',
                },
            ],
            OrderCode: [
                {
                    required: true,
                    message: this.$t('PleaseInputOrderCode'),
                    trigger: 'change',
                },
            ],
            DriverName: [
                {
                    required: true,
                    message: this.$t('PleaseInputDriverName'),
                    trigger: 'change',
                },
            ],
            DriverCode: [
                {
                    required: true,
                    message: this.$t('PleaseInputDriverCode'),
                    trigger: 'change',
                },
                {
                    trigger: 'change',
                    message: this.$t('CCCDExactly12Characters'),
                    validator: (rule, value: any, callback) => {
                        let firstChar =  value.charAt(0);
                        if (!isNaN(firstChar) && value.length != 12) {
                            callback(new Error());
                          }
                        callback();
                    },
                }
            ],
            LocationFrom: [
                {
                    required: true,
                    message: this.$t('PleaseInputLocationFrom'),
                    trigger: 'change',
                },
            ],
            TrailerNumber: [
                {
                    required: true,
                    message: this.$t('PleaseInputTrailerNumber'),
                    trigger: 'change',
                },
            ],
            DriverPhone: [
                {
                    required: true,
                    message: this.$t('PleaseInputDriverPhone'),
                    trigger: 'change',
                },
            ],
        };
    }

    onInsertClick() {
        this.formModel = {Vc: false};
        this.isEdit = false;
        this.showDialog = true;
    }


    onEditClick() {
        this.formModel = this.selectedRows[0];
        console.log(this.formModel);
        if (this.clientName == 'MAY') {
            this.formModel['PositionIndex'] = this.formModel['EmployeeType']
        }
        this.isEdit = true;
        this.showDialog = true;
    }

    async initCustomize() {
        this.setFingers();
    }

    async getDepartment() {
        return await departmentApi.GetAll().then((res) => {
            const { data } = res as any;
            let arr = JSON.parse(JSON.stringify(data));
            for (let i = 0; i < arr.Value.length; i++) {
                arr.Value[i].value = parseInt(arr.Value[i].value);
            }
            this.listDepartment = arr.Value;
        });
    }

    async initPositionInfoLookup() {
        if (this.listAllPosition.length > 0) {
            return Promise.resolve(this.listAllPosition);
        } else {
            await Misc.readFileAsync('static/variables/common-utils.json').then(async x => {
                if (x.ClientName == 'MAY') {
                    userTypeApi.GelAllUserType().then(res => {
                        this.listAllPosition = res.data.filter(x => x.Status == 'Active');

                    });
                } else {
                    console.log('đasa');
                    return await hrPositionInfoApi.GetAllHRPositionInfo().then((response) => {

                        this.listAllPosition = response.data;
                        // console.log(response.data)
                        this.listAllPosition.forEach(e => {
                            this.listAllPositionLookup[e.Index] = e;
                        })
                    });
                }
            });

        }
    }

    async loadData() {
        this.isLoading = true;
        await this.initPositionInfoLookup();
        let fromDate = '';
        let toDate = '';

        if (this.filterModel.FromDate != null) {
            this.filterModel.FromDate = new Date(moment(this.filterModel.FromDate).format('YYYY-MM-DD'));
            fromDate = moment(this.filterModel.FromDate).format('YYYY-MM-DD');
        }
        if (this.filterModel.ToDate != null) {
            this.filterModel.ToDate = new Date(moment(this.filterModel.ToDate).format('YYYY-MM-DD'));
            toDate = moment(this.filterModel.ToDate).format('YYYY-MM-DD');
        }
        if (this.filterModel.FromDate != null && this.filterModel.ToDate != null
            && this.filterModel.FromDate > this.filterModel.ToDate) {
            this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
            this.isLoading = false;
            return;
        }

        const param = {
            VehiclePlate: this.filterModel.VehiclePlate,
            Status: this.filterModel.Status,
            FromDate: fromDate,
            ToDate: toDate,
            Filter: this.filterModel.TextboxSearch,
            Page: this.page,
            PageSize: this.pageSize,
            DepartmentIDs: this.filterModel.SelectedDepartment
        }

        await hrDriverInfoApi.GetDriverAtPage(param).then(response => {
            const { data, total }: { data: any[], total: number } = response.data;
            this.dataSource = data.map((emp, idx) => ({
                ...emp,
                index: idx + 1 + (this.page - 1) * this.pageSize,
            }));
            this.total = total;
            this.shouldResetColumnSortState = !this.shouldResetColumnSortState;
        });
        this.isLoading = false;
    }



    async doDelete() {
        const selectedEmp = this.selectedRows.map(e => e.TripId);
        await hrDriverInfoApi
            .DeleteDriverInfo(selectedEmp)
            .then(async (res) => {
                this.loadData();
                this.selectedRows = [];
                this.$deleteSuccess();
            })
            .catch(() => { })
            .finally(() => { this.showDialogDeleteUser = false; })
    }

    async onViewClick() {
        //  this.configModel.filterModel = this.filterModel;
        this.$emit('filterModel', this.configModel);
        this.page = 1;
        (this.$refs.taDriverInfoPagination as any).page = this.page;
		(this.$refs.taDriverInfoPagination as any).lPage = this.page;
        await this.loadData();
    }

    async getApiContactInfo() {

        // await hrEmployeeInfoApi.GetAllEmployee().then((response) => {
        //     // this.listAllPosition = response.data;
        //     // // console.log(response.data)
        //     // this.listAllPosition.forEach(e => {
        //     //     this.listAllPositionLookup[e.Index] = e;
        //     // })
        // });
    }

    async getEmployeeType() {
        await employeeTypesApi.GetUsingEmployeeType().then((res: any) => {
            if (res.status && res.status == 200) {
                this.listEmployeeType = res.data;
                // // console.log(this.listEmployeeType)
            }
        });
    }

    async addContactInfo() {

        if (!Misc.isEmpty(
            this.newInputContactInfo.Email &&
            this.newInputContactInfo.Name &&
            this.newInputContactInfo.Phone
        )) {
            this.listContactInfoFormApi.unshift({
                Name: this.newInputContactInfo?.Name ?? '',
                Email: this.newInputContactInfo?.Email ?? '',
                Phone: this.newInputContactInfo?.Phone ?? '',
            });

            this.newInputContactInfo.Name = '';
            this.newInputContactInfo.Email = '';
            this.newInputContactInfo.Phone = '';
        }
    }

    deleteContactInfo(name) {
        if (this.listContactInfoFormApi.length > 0) {
            for (let index = 0; index < this.listContactInfoFormApi.length; index++) {
                let valueExistInArray = this.listContactInfoFormApi[index]?.Name.indexOf(name);

                if (valueExistInArray > -1) {
                    this.listContactInfoFormApi.splice(valueExistInArray, 1);
                }
            }
        }
    }

    async onSubmitClick() {
        delete this.formModel.index;
        const a = (this.$refs.employeeFormModel as any);
        (this.$refs.employeeFormModel as any).validate(async (valid) => {
            if (!valid) return;
            if (this.formModel.TimesDock) {
                this.formModel.TimesDock = moment(this.formModel.TimesDock).format('YYYY-MM-DD HH:mm:ss');
            }
            if (this.formModel.Eta) {
                this.formModel.Eta = moment(this.formModel.Eta).format('YYYY-MM-DD HH:mm:ss');
            }
            const dateNow = new Date();
            (this.formModel as any).BirthDay = new Date(moment((this.formModel as any).BirthDay).format('YYYY-MM-DD'));
            const birthDay = new Date(moment((this.formModel as any).BirthDay).format('YYYY-MM-DD'));
            const ageDifferenceInMilliseconds = dateNow.getTime() - birthDay.getTime();
            const ageInYears = Math.floor(ageDifferenceInMilliseconds / (1000 * 60 * 60 * 24 * 365));

            // if (this.clientName == "Mondelez") {
            //     let firstChar =  (this.formModel as any).DriverCode.charAt(0);
            //     if (!isNaN(firstChar) && (this.formModel as any).DriverCode.length != 12) {
            //         this.$alertSaveError(null, null, null, this.$t('CCCDExactly12Characters').toString()).toString();
            //         return;
            //       }
            // }
            if (this.isEdit) {
                await hrDriverInfoApi.Put_HR_DriverInfo(this.formModel)
                    .then(() => {
                        this.loadData();
                        this.$saveSuccess();
                        this.showDialog = false;
                    })
                    .catch((err) => {
                        this.listContactInfoFormApi = [];
                        console.warn(err)
                    })
            }
            else {
                try {

                    await hrDriverInfoApi.Post_HR_DriverInfo(this.formModel).then(() => {
                        this.loadData();
                        this.$saveSuccess();
                        this.showDialog = false;
                    });
                } catch (error) {
                    console.warn(error);
                }
            }

            store.dispatch('HumanResource/loadEmployeeLookup').catch((error) => { });
        })
        this.listFinger = [];
        this.selectedRows = [];
        this.setFingers();
    }

    //#region handle avatar
    getBase64(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsDataURL(file);
            reader.onload = () => resolve(reader.result);
            reader.onerror = error => reject(error);
        });
    }

    getArrayBuffer(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsArrayBuffer(file);
            reader.onload = () => resolve(reader.result);
            reader.onerror = error => reject(error);
        });
    }

    handleBeforeUploadAvatar(file) {

    }

    async onChangeAvatar(file, fileList) {
        const originCountFileList = fileList.length;
        if (fileList.length > 1) {
            fileList.splice(1, fileList.length);
        }
        const fileRaw = file.raw;
        const isJPG = fileRaw.type === 'image/jpeg';
        const isLt2M = fileRaw.size / 1024 / 1024 < 2;

        if (!isJPG) {
            this.$message.error('Avatar picture must be JPG format!');
            if (originCountFileList == 1) fileList.splice(0, 1);
            return;
        }
        if (!isLt2M) {
            this.$message.error('Avatar picture size can not exceed 2MB!');
            if (originCountFileList == 1) fileList.splice(0, 1);
            return;
        }

        fileList[0] = file;

        this.fileImageName = fileRaw.name;
        await this.getArrayBuffer(fileRaw).then(
            data => {
                Object.assign(this.formModel, { Avatar: Misc.arrayBufferToBase64(data) });
                this.errorUpload = false;
                this.$forceUpdate();
            }

        )
            .catch(e => {
                console.log(e)
            });
    }

    onRemoveAvatar(file, fileList) {
        Object.assign(this.formModel, { ImageUpload: '', Avatar: null });
        this.errorUpload = false;
        this.$forceUpdate();
    }
    //#endregion

    //#region Register Finger print

    // Finger pRINT
    showOrHideRegisterFingerDialog() {
        this.showFingerDialog = true;
        this.getEmployeeFinger(this.currentSelectEmployeeATID);
        this.resetFingerParam();
        this.websOpen();
    }

    cancelRegisterFingerDialog() {
        this.showFingerDialog = false;
        this.websClose();
        this.listFinger = [];
        this.listTemplateFinger = [];
        this.setFingers();
    }

    @Watch("isEdit")
    assignCurrentSelectedEmployee() {
        this.currentSelectEmployeeATID = (this.formModel as any).EmployeeATID;
    }

    async submitRegisterFinger() {
        var lowQualityFinger = false;
        this.listFinger.forEach(e => {
            if (e.Quality >= 0 && e.Quality < 70) {
                var message = 'Vân tay ' + e.ID.toString() + ' có chất lượng thấp, hãy thử lại!';
                this.$notify({
                    type: 'warning',
                    title: 'Thông báo từ thiết bị',
                    dangerouslyUseHTMLString: true,
                    message: message,
                    customClass: 'notify-content',
                    duration: 5000
                });
                lowQualityFinger = true;
            }
        });
        if (lowQualityFinger) {
            return;
        }

        this.ListFingerRequest = this.listFinger.map(e => (e.Template));
        this.showFingerDialog = false;
    }

    // submitRegisterFinger() {
    //     const lstFinger = this.listFinger.map(e => (e.Template));
    //     Object.assign(this.formModel, { ListFinger: lstFinger });
    //     this.showFingerDialog = false;
    // }

    async getEmployeeFinger(employeeATID) {
        await employeeInfoApi.GetEmployeeFinger(employeeATID).then(res => {
            const { data } = res as any;
            for (var i = 0; i < data.length; i++) {
                if (!Misc.isEmpty(data[i]) && data[i].length > 0) {
                    this.listFinger[i].ImageFinger = this.getImgUrl('fingerprint.png');
                    this.listFinger[i].Template = data[i];
                    this.listFinger[i].Quality = 71;
                }
            }
        });
    }

    setFingers() {
        for (var i = 1; i <= 10; i++) {
            this.listFinger.push({ FocusFinger: false, ID: i, Template: "", ImageFinger: "", Quality: -1 });
        }
    }

    getImgUrl(image) {
        return require('@/assets/images/' + image);
    }

    resetFingerParam() {
        this.registerCount = 0;
        this.template1 = "";
        this.template2 = "";
    }

    onFocusFinger(index) {

        if (this.currentIndex != 0) {
            this.closeDev();
        }
        this.listFinger.forEach(function (item) {
            item.FocusFinger = false;
        });
        this.listFinger[index - 1].FocusFinger = true;
        this.currentIndex = index - 1;
        this.openDev();
    }

    onOpen(event) {
        // console.log(event.data);
    }

    onClose(event) {
        this.listTemplateFinger = [];
        // console.log(event.data);
    }

    onError(event) {
        // console.log(event.data);
    }

    onMessage(event) {
        if (event.data != undefined) {
            var jsonData = JSON.parse(event.data);
            if (jsonData.datatype === "image") {
                var tempImg = jsonData.data.jpg_base64;
                if (tempImg == undefined || tempImg == '') {
                    return false
                }
                var strImgData = "data:image/jpg;base64,";
                strImgData += tempImg;
                var quality = jsonData.data.quality;
                this.listFinger[this.currentIndex].ImageFinger = strImgData;
                this.listFinger[this.currentIndex].Quality = quality;
                if (quality >= 0 && quality < 70) {
                    var message =
                        "Vân tay " +
                        (this.currentIndex + 1).toString() +
                        " có chất lượng thấp, hãy thử lại!";
                    this.$notify({
                        type: "warning",
                        title: "Thông báo từ thiết bị",
                        dangerouslyUseHTMLString: true,
                        message: message,
                        customClass: "notify-content",
                        duration: 5000,
                    });
                } else if (quality >= 70) {
                    var message =
                        "Vân tay " + (this.currentIndex + 1).toString() + " hợp lệ!";
                    this.$notify({
                        type: "success",
                        title: "Thông báo từ thiết bị",
                        dangerouslyUseHTMLString: true,
                        message: message,
                        customClass: "notify-content",
                        duration: 5000,
                    });
                }
            }
            if (jsonData.datatype === "template") {

                this.listFinger[this.currentIndex].Template = jsonData.data.template;
            }
            else {
                if (jsonData.ret == 0 && jsonData.function == "open") {
                    this.ConnectedDevice = 2;
                    this.DeviceInfo = this.$t("ConnectedFingerDevice").toString();
                } else if (jsonData.ret == -10007) {
                    this.ConnectedDevice = 0;
                    this.DeviceInfo = this.$t("NotConnectedDevice").toString();
                } else if (jsonData.ret < 0) {
                    this.DeviceInfo = this.$t("NotConnectedDevice").toString();
                } else if (jsonData.ret == 0 && jsonData.function == "close") {
                    this.openDev();
                }

            }
        }
        else {
            this.DeviceInfo = this.$t('NotConnectedDevice').toString();
        }
    }

    doSend(message) {
        this.websocket.send(message);
    }

    websOpen() {
        this.websocket = new WebSocket(this.wsUri);
        this.websocket.onopen = (event) => {
            this.onOpen(event);
        };
        this.websocket.onclose = (event) => {
            this.onClose(event);
        };

        this.websocket.onmessage = (event) => {
            this.onMessage(event);
        };

        this.websocket.onerror = (event) => {
            this.onError(event);
        };
        this.ConnectedDevice = 0;

    }

    reconnect() {
        this.resetFingerParam();
        this.closeDev();
        this.openDev();
    }

    websClose() {
        if (!Misc.isEmpty(this.websocket)) {
            this.websocket.close();
            this.ConnectedDevice = 0;
        }
    }

    getInfo() {
        this.doSend("{\"module\":\"common\",\"function\":\"info\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}");
    }

    register() {
        this.doSend("{\"module\":\"fingerprint\",\"function\":\"register\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}");
    }

    disRegister() {
        this.doSend("{\"module\":\"fingerprint\",\"function\":\"cancelregister\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}");
    }

    verity() {
        // console.log(this.template1);
        // console.log(this.template2);
        var str = "{\"module\":\"fingerprint\",\"function\":\"verify\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":{" + "\"template1\":\"" + this.template1 + "\",\"template2\":\"" + this.template2 + "\"}}"
        this.doSend(str);
    }

    openDev() {
        this.ConnectedDevice = 1;
        var str = "{\"module\":\"fingerprint\",\"function\":\"open\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}";
        this.doSend(str);
    }

    closeDev() {
        // console.log(this.websocket)
        if (this.websocket.readyState != WebSocket.CLOSED && this.websocket.readyState != WebSocket.CLOSING) {
            this.doSend("{\"module\":\"fingerprint\",\"function\":\"close\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}");
            this.ConnectedDevice = 0;
        }
    }

    //#endregion

    //#region Import excel
    importErrorMessage = "";
    showImportExcel = true;
    isAddFromExcel = false;
    isDeleteFromExcel = false;
    addedParams = [];
    formExcel = {};
    dataAddExcel = [];
    fileName = '';
    showDialogExcel = false;
    dataProcessedExcel = [];
    showDialogImportError = false;
    listExcelFunction = ['AddExcel', 'ExportExcel'];

    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }


    onCancelClick() {
        (this.$refs.employeeFormModel as any).resetFields();
        this.showDialog = false;
        this.selectedRows = [];
        this.formModel = {Vc : false};
        this.loadData();
    }


    DeleteDataFromExcel_MAY() {
        var arrData = [];
        var regex = /^\d+$/;
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a;
            if (regex.test(this.dataAddExcel[0][i]['Mã chấm ăn của khách hàng']) === false) {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDOnlyAcceptNumericCharacters').toString()).toString();
                return;
            } else if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm ăn của khách hàng')) {
                a = this.dataAddExcel[0][i]['Mã chấm ăn của khách hàng'] + '';
            }
            arrData.push(a);
        }
        this.addedParams = [];
        this.addedParams.push({ Key: "ListEmployeeATID", Value: arrData });
        this.addedParams.push({ Key: "IsDeleteOnDevice", Value: this.isDeleteOnDevice });
        employeeInfoApi.DeleteEmployeeFromExcel(this.addedParams).then((res) => {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
            }
            this.showDialogExcel = false;
            if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
                this.$deleteSuccess();
                this.loadData();
            }
        });
    }
    UploadDataFromExcel() {

        this.importErrorMessage = "";
        var arrData = [];
        var regex = /^\d+$/;
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});

            if (this.dataAddExcel[0][i].hasOwnProperty('Mã chuyến (*)')) {
                a.TripId = this.dataAddExcel[0][i]['Mã chuyến (*)'] + '';
            } else {
                // this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                // return;
                a.TripId = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Mã đơn hàng (*)')) {
                a.OrderCode = this.dataAddExcel[0][i]['Mã đơn hàng (*)'] + '';
            } else {
                a.OrderCode = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Điểm nhận hàng (*)')) {
                a.LocationFrom = this.dataAddExcel[0][i]['Điểm nhận hàng (*)'] + '';
            } else {
                a.LocationFrom = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Họ tên (*)')) {
                a.DriverName = this.dataAddExcel[0][i]['Họ tên (*)'] + '';
            } else {
                a.DriverName = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Biển số xe (*)')) {
                a.TrailerNumber = this.dataAddExcel[0][i]['Biển số xe (*)'] + '';
            } else {
                a.TrailerNumber = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Số CCCD (*)')) {
                a.DriverCode = this.dataAddExcel[0][i]['Số CCCD (*)'] + '';
            } else {
                a.DriverCode = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Ngày sinh')) {
                a.BirthDayStr = this.dataAddExcel[0][i]['Ngày sinh'] + '';
            } else {
                a.BirthDayStr = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Xe vãng lai')) {
                a.Vc = true;
            } else {
                a.Vc = false;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Thời gian dự kiến đến điểm lấy')) {
                a.EtaStr = this.dataAddExcel[0][i]['Thời gian dự kiến đến điểm lấy'] + '';
            } else {
                a.EtaStr = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Số điện thoại (*)')) {
                a.DriverPhone = this.dataAddExcel[0][i]['Số điện thoại (*)'] + '';
            } else {
                a.DriverPhone = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Thời gian vào Dock')) {
                a.TimesDockStr = this.dataAddExcel[0][i]['Thời gian vào Dock'] + '';
            } else {
                a.TimesDockStr = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Trạng thái xe')) {
                a.StatusDockString = this.dataAddExcel[0][i]['Trạng thái xe'] + '';
            } else {
                a.StatusDockString = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Loại')) {
                a.Type = this.dataAddExcel[0][i]['Loại'] + '';
            } else {
                a.Type = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Nhà cung cấp')) {
                a.Supplier = this.dataAddExcel[0][i]['Nhà cung cấp'] + '';
            } else {
                a.Supplier = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Tên hoạt động')) {
                a.Operation = this.dataAddExcel[0][i]['Tên hoạt động'] + '';
            } else {
                a.Operation = '';
            }
            arrData.push(a);
        }
        hrDriverInfoApi.AddDriveInfoFromExcel(arrData).then((res) => {
            this.showDialogExcel = false;
            this.fileName = '';
            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            //// console.log(res)
            if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
                //// console.log("Import success")
                this.$saveSuccess();
                this.loadData();
            } else {
                this.importErrorMessage = this.$t('ImportDriverErrorMessage') + res.data.toString() + " " + this.$t('Driver');
                //// console.log("Import error, show popup import error file download")
                this.showOrHideImportError(true);
            }
        });

    }
    DeleteDataFromExcel() {
        var arrData = [];
        var regex = /^\d+$/;
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a;
            if (regex.test(this.dataAddExcel[0][i]['Mã chấm công (*)']) === false) {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDOnlyAcceptNumericCharacters').toString()).toString();
                return;
            } else if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm công (*)')) {
                a = this.dataAddExcel[0][i]['Mã chấm công (*)'] + '';
            }
            arrData.push(a);
        }

        this.addedParams = [];
        this.addedParams.push({ Key: "ListEmployeeATID", Value: arrData });
        this.addedParams.push({ Key: "IsDeleteOnDevice", Value: this.isDeleteOnDevice });
        employeeInfoApi
            .DeleteEmployeeFromExcel(this.addedParams)
            .then((res) => {
                console.log(res);

                if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                    (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
                }
                if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                    (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
                }
                this.showDialogExcel = false;
                this.fileName = '';
                this.isDeleteFromExcel = false;
                this.dataAddExcel = [];
                console.log(res);
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    this.$deleteSuccess();

                }
            })
            .catch(() => { });

    }
    async ExportToExcel() {
        let fromDate = '';
        let toDate = '';

        if (this.filterModel.FromDate != null) {
            this.filterModel.FromDate = new Date(moment(this.filterModel.FromDate).format('YYYY-MM-DD'));
            fromDate = moment(this.filterModel.FromDate).format('YYYY-MM-DD');
        }
        if (this.filterModel.ToDate != null) {
            this.filterModel.ToDate = new Date(moment(this.filterModel.ToDate).format('YYYY-MM-DD'));
            toDate = moment(this.filterModel.ToDate).format('YYYY-MM-DD');
        }
        if (this.filterModel.FromDate != null && this.filterModel.ToDate != null
            && this.filterModel.FromDate > this.filterModel.ToDate) {
            this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
            this.isLoading = false;
            return;
        }

        const param = {
            VehiclePlate: this.filterModel.VehiclePlate,
            Status: this.filterModel.Status,
            FromDate: fromDate,
            ToDate: toDate,
            Filter: this.filterModel.Filter,
            Page: this.page,
            PageSize: this.pageSize
        }


        await hrDriverInfoApi.ExportToExcel(param).then((res: any) => {

            const fileURL = window.URL.createObjectURL(new Blob([res.data]));
            const fileLink = document.createElement("a");

            fileLink.href = fileURL;
            fileLink.setAttribute("download", `DriverInfo_${moment(this.fromDate).format('YYYY-MM-DD HH:mm')}.xlsx`);
            document.body.appendChild(fileLink);

            fileLink.click();
        });
    }
    downloadFile(filePath) {
        var link = document.createElement('a');
        link.href = filePath;
        link.download = filePath.substr(filePath.lastIndexOf('/') + 1);
        link.click();
    }
    AddOrDeleteFromExcel(x) {
        if (x === 'close') {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
            }
            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            this.isDeleteFromExcel = false;
            this.showDialogExcel = false;
            this.fileName = '';
        } else if (x === 'add') {
            this.isAddFromExcel = true;
            this.showDialogExcel = true;
            this.fileName = '';
        } else if (x === 'delete') {
            this.isDeleteFromExcel = true;
            this.showDialogExcel = true;
            this.fileName = '';
        }
    }

    ShowOrHideDialogExcel(x) {
        if (x == 'open') {
            this.dataAddExcel = [];
            this.fileName = '';
            this.showDialogExcel = true;
        }
        else {
            (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            this.showDialogExcel = false;
        }
    }
    handleCommand(command) {
        if (command === 'AddExcel') {
            this.AddOrDeleteFromExcel('add');
        }
        else if (command === 'ExportExcel') {
            this.ExportToExcel();
        }
        else if (command === 'DeleteExcel') {
            this.AddOrDeleteFromExcel('delete');
        }
    }
    processFile(e) {
        if ((<HTMLInputElement>e.target).files.length > 0) {
            var file = (<HTMLInputElement>e.target).files[0];
            this.fileName = file.name;
            if (!isNullOrUndefined(file)) {
                var fileReader = new FileReader();
                var arrData = [];
                fileReader.onload = function (event) {
                    var data = event.target.result;
                    var workbook = XLSX.read(data, {
                        type: 'binary',
                    });

                    workbook.SheetNames.forEach((sheet) => {
                        var rowObject = XLSX.utils.sheet_to_json(workbook.Sheets[sheet]);
                        // var arr = Array.from(rowObject)
                        // arrData.push(arr)
                        arrData.push(Array.from(rowObject));
                    });
                };
                this.dataAddExcel = arrData;
                fileReader.readAsBinaryString(file);
            }
        }
    }
    async AutoSelectFromExcel() {
        this.dataProcessedExcel = [];
        var regex = /^\d+$/;
        if (this.dataAddExcel.length == 0) {
            return this.$alertSaveError(null, null, null, this.$t('NoFileUpload').toString()).toString();

        }
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});
            if (regex.test(this.dataAddExcel[0][i]['Mã chấm công (*)']) === false) {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDOnlyAcceptNumericCharacters').toString()).toString();
                return;
            } else if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm công (*)')) {
                a.EmployeeATID = this.dataAddExcel[0][i]['Mã chấm công (*)'] + '';
            } else {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                return;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Mã nhân viên')) {
                a.EmployeeCode = this.dataAddExcel[0][i]['Mã nhân viên'] + '';
            } else {
                a.EmployeeCode = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Tên nhân viên')) {
                a.FullName = this.dataAddExcel[0][i]['Tên nhân viên'] + '';
            } else {
                a.FullName = '';
            }
            this.dataProcessedExcel.push(a);
        }
        // Handle after upload 
    }

    handleWorkingInfoChange(workingInfo: any) {
        this.listWorkingStatus = workingInfo;
        //this.getEmployees();
    }

    focus(x) {
        var theField = eval('this.$refs.' + x)
        theField.focus()
    }

    //#endregion

}
