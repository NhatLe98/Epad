import { isLength } from 'lodash';
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
import AlertComponent from '@/components/app-component/alert-component/alert-component.vue';

@Component({
    name: 'hr-employee-info',
    components: {
        VisualizeTable,
        AppPagination,
        ImageCellRendererVisualizeTable,
        SelectTreeComponent,
        SelectDepartmentTreeComponent,
        TableToolbar,
        AlertComponent
    },
})
export default class HREmployeeInfo extends Mixins(TabBase) {
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
    filterModel = { SelectedDepartment: [], TextboxSearch: '', FromDate: null, ToDate: null };
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
    columnDefs = [
        {
            field: 'index',
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
            field: 'EmployeeATID',
            headerName: this.$t('MCC'),
            pinned: true,
            width: 150,
            sortable: true,
            display: true
        },
        {
            field: 'EmployeeCode',
            headerName: this.$t('EmployeeCode'),
            pinned: true,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('FullName'),
            field: 'FullName',
            pinned: true,
            width: 150,
            sortable: true,
            display: true
        },
        {
            field: 'DepartmentName',
            headerName: this.$t('Department'),
            pinned: true,
            width: 170,
            sortable: true,
            display: true
        },
        {
            field: 'EmployeeTypeName',
            headerName: this.$t('EmployeeTypeName'),
            pinned: false,
            width: 170,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('JoinedDate'),
            field: 'FromDate',
            pinned: false,
            width: 150,
            sortable: true,
            cellRenderer: params => `${moment(params.value).format('DD-MM-YYYY')}`,
            display: true
        },
        {
            headerName: this.$t('StoppedDate'),
            field: 'ToDate',
            pinned: false,
            width: 150,
            sortable: true,
            cellRenderer: params => `${params.value != null ? moment(params.value).format('DD-MM-YYYY') : ""}`,
            display: true
        },
        {
            field: 'WorkingStatus',
            headerName: this.$t('Status'),
            pinned: true,
            width: 170,
            sortable: true,
            cellRenderer: params => `${params.value === 'IsWorking' ? this.$t('IsWorking').toString() : this.$t('StoppedWork').toString()}`,
            display: true
        },
        {
            headerName: this.$t('CMND/CCCD/Passport'),
            field: 'Nric',
            pinned: false,
            width: 250,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('Address'),
            field: 'Address',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('Note'),
            field: 'Note',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('Position'),
            field: 'PositionName',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('Gender'),
            field: 'GenderTitle',
            // dataType: 'lookup',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('BirthDay'),
            field: 'BirthDay',
            pinned: false,
            width: 150,
            sortable: true,
            cellRenderer: params => `${moment(params.value).format('DD-MM-YYYY')}`,
            display: true
        },
        {
            headerName: this.$t('Email'),
            field: 'Email',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('Phone'),
            field: 'Phone',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('PhoneUseIsAllow'),
            field: 'IsAllowPhone',
            pinned: false,
            width: 270,
            sortable: true,
            display: true,
            cellRenderer: params => `${params.value === true ? this.$t('Yes').toString() : this.$t('No').toString()}`,
        },
        {
            headerName: this.$t('CardNumber'),
            field: 'CardNumber',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('NameOnMachine'),
            field: 'NameOnMachine',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('TotalFingerTemplate'),
            field: 'TotalFingerTemplate',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
    ];

    onSelectionChange(selectedRows: any[]) {
        // // console.log(selectedRows);
        this.selectedRows = selectedRows;
    }
    async beforeMount() {
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.clientName = x.ClientName;
            if (this.clientName == "MAY") {
                this.value = 'MAY';

                this.columnDefs = [
                    {
                        field: 'index',
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
                        field: 'EmployeeATID',
                        headerName: this.$t('MCA'),
                        pinned: true,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        field: 'EmployeeCode',
                        headerName: this.$t('MSKH'),
                        pinned: true,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('FullName'),
                        field: 'FullName',
                        pinned: true,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        field: 'DepartmentName',
                        headerName: this.$t('Department'),
                        pinned: true,
                        width: 170,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('JoinedDate'),
                        field: 'FromDate',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        cellRenderer: params => `${moment(params.value).format('DD-MM-YYYY')}`,
                        display: true
                    },
                    {
                        headerName: this.$t('StoppedDate'),
                        field: 'ToDate',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        cellRenderer: params => `${params.value != null ? moment(params.value).format('DD-MM-YYYY') : ""}`,
                        display: true
                    },
                    {
                        field: 'WorkingStatus',
                        headerName: this.$t('Status'),
                        pinned: true,
                        width: 170,
                        sortable: true,
                        cellRenderer: params => `${params.value === 'IsWorking' ? this.$t('IsWorking').toString() : this.$t('StoppedWork').toString()}`,
                        display: true
                    },
                    {
                        headerName: this.$t('Address'),
                        field: 'Address',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('Note'),
                        field: 'Note',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('Position'),
                        field: 'PositionName',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('Gender'),
                        field: 'GenderTitle',
                        // dataType: 'lookup',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('BirthDay'),
                        field: 'BirthDay',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        cellRenderer: params => `${moment(params.value).format('DD-MM-YYYY')}`,
                        display: true
                    },
                    {
                        headerName: this.$t('Email'),
                        field: 'Email',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('Phone'),
                        field: 'Phone',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('CardNumber'),
                        field: 'CardNumber',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('NameOnMachine'),
                        field: 'NameOnMachine',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('TotalFingerTemplate'),
                        field: 'TotalFingerTemplate',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                ];
                this.columnDefs.splice(9, 0, {
                    headerName: this.$t('CustomerObject'),
                    field: 'UserType',
                    pinned: false,
                    width: 150,
                    sortable: true,
                    display: true
                },);
            } else if (this.clientName == "Mondelez") {
                this.columnDefs = [
                    {
                        field: 'index',
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
                        field: 'EmployeeATID',
                        headerName: this.$t('MCC'),
                        pinned: true,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        field: 'EmployeeCode',
                        headerName: this.$t('EmployeeCode'),
                        pinned: true,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('FullName'),
                        field: 'FullName',
                        pinned: true,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        field: 'DepartmentName',
                        headerName: this.$t('Department'),
                        pinned: true,
                        width: 170,
                        sortable: true,
                        display: true
                    },
                    {
                        field: 'EmployeeTypeName',
                        headerName: this.$t('EmployeeTypeName'),
                        pinned: false,
                        width: 170,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('JoinedDate'),
                        field: 'FromDate',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        cellRenderer: params => `${moment(params.value).format('DD-MM-YYYY')}`,
                        display: true
                    },
                    {
                        headerName: this.$t('StoppedDate'),
                        field: 'ToDate',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        cellRenderer: params => `${params.value != null ? moment(params.value).format('DD-MM-YYYY') : ""}`,
                        display: true
                    },
                    {
                        field: 'WorkingStatus',
                        headerName: this.$t('Status'),
                        pinned: true,
                        width: 170,
                        sortable: true,
                        cellRenderer: params => `${params.value === 'IsWorking' ? this.$t('IsWorking').toString() : this.$t('StoppedWork').toString()}`,
                        display: true
                    },
                    {
                        headerName: this.$t('CMND/CCCD/Passport'),
                        field: 'Nric',
                        pinned: false,
                        width: 250,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('Address'),
                        field: 'Address',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('Position'),
                        field: 'PositionName',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('Gender'),
                        field: 'GenderTitle',
                        // dataType: 'lookup',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('BirthDay'),
                        field: 'BirthDay',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        cellRenderer: params => `${moment(params.value).format('DD-MM-YYYY')}`,
                        display: true
                    },
                    {
                        headerName: this.$t('Email'),
                        field: 'Email',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('Phone'),
                        field: 'Phone',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('PhoneUseIsAllow'),
                        field: 'IsAllowPhone',
                        pinned: false,
                        width: 270,
                        sortable: true,
                        display: true,
                        cellRenderer: params => `${params.value === true ? this.$t('Yes').toString() : this.$t('No').toString()}`,
                    },
                    {
                        headerName: this.$t('CardNumber'),
                        field: 'CardNumber',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('NameOnMachine'),
                        field: 'NameOnMachine',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                    {
                        headerName: this.$t('TotalFingerTemplate'),
                        field: 'TotalFingerTemplate',
                        pinned: false,
                        width: 150,
                        sortable: true,
                        display: true
                    },
                ];

            }
            else {
                this.value = '';
            }
        });

        employeeInfoApi.InfoEmployeeTemplateImport().then((res: any) => {
            // console.log(res);
        })
        employeeInfoApi.ExportTemplateICEmployee().then((res: any) => {
            // console.log(res);
        })
    }

    async initLookup() {
        await this.getDepartment();
        this.initPositionInfoLookup();
        await this.getApiContactInfo();
        await this.getEmployeeType();
        this.LoadDepartmentTree();
    }

    LoadDepartmentTree() {
        departmentApi.GetDepartmentTreeEmployeeScreen("1").then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });

    }

    initFormRules() {
        if (this.clientName == "MAY") {
            this.formRules = {
                EmployeeATID: [
                    {
                        required: true,
                        message: this.$t('PleaseInputEmployeeATID'),
                        trigger: 'change',
                    },
                    {
                        trigger: 'change',
                        message: this.$t('EmployeeATIDOnlyAcceptNumericCharacters'),
                        validator: (rule, value: string, callback) => {
                            if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
                                callback(new Error());
                            }
                            callback();
                        },
                    },
                ],
                CardNumber: [
                    {
                        required: true,
                        message: this.$t('PleaseInputCardNumber'),
                        trigger: 'change',
                    },
                    {
                        trigger: 'change',
                        message: this.$t('CardNumberOnlyAcceptNumericCharacters'),
                        validator: (rule, value: string, callback) => {
                            if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
                                callback(new Error());
                            }
                            callback();
                        },
                    },
                ],
                JoinedDate: [
                    {
                        required: true,
                        message: this.$t('PleaseSelectDayJoinedDate'),
                        trigger: 'change',
                    },
                ],
                DepartmentIndex: [
                    {
                        required: true,
                        message: this.$t('SelectDepartment'),
                        trigger: 'change'
                    },
                ],
                FullName: [
                    {
                        required: true,
                        message: this.$t('PleaseInputFullName'),
                        trigger: 'change',
                    },
                ],
            };
        }
        else if (this.clientName == "Mondelez") {
            this.formRules = {
                EmployeeATID: [
                    {
                        required: true,
                        message: this.$t('PleaseInputEmployeeATID'),
                        trigger: 'change',
                    },
                    {
                        trigger: 'change',
                        message: this.$t('EmployeeATIDOnlyAcceptNumericCharacters'),
                        validator: (rule, value: string, callback) => {
                            if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
                                callback(new Error());
                            }
                            callback();
                        },
                    },
                    {
                        trigger: 'change',
                        message: this.$t('EmployeeATIDEqual8Characters'),
                        validator: (rule, value: string, callback) => {
                            if(value.length != 8){
                                callback(new Error());
                            }
                            callback();
                        },
                    }
                ],
                CardNumber: [
                    {
                        trigger: 'change',
                        message: this.$t('CardNumberOnlyAcceptNumericCharacters'),
                        validator: (rule, value: string, callback) => {
                            if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
                                callback(new Error());
                            }
                            callback();
                        },
                    },
                ],
                JoinedDate: [
                    {
                        required: true,
                        message: this.$t('PleaseSelectDayJoinedDate'),
                        trigger: 'change',
                    },
                ],
                DepartmentIndex: [
                    {
                        required: true,
                        message: this.$t('SelectDepartment'),
                        trigger: 'change'
                    },
                ],
                FullName: [
                    {
                        required: true,
                        message: this.$t('PleaseInputFullName'),
                        trigger: 'change',
                    },
                ],
                Nric: [
                    {
                        required: true,
                        message: this.$t('PleaseInputNRIC'),
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
                BirthDay: [
                    {
                        required: true,
                        message: this.$t('PleaseInputBirthDay'),
                        trigger: 'change',
                    },
                ],
            };
        }
        else {
            this.formRules = {
                EmployeeATID: [
                    {
                        required: true,
                        message: this.$t('PleaseInputEmployeeATID'),
                        trigger: 'change',
                    },
                    {
                        trigger: 'change',
                        message: this.$t('EmployeeATIDOnlyAcceptNumericCharacters'),
                        validator: (rule, value: string, callback) => {
                            if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
                                callback(new Error());
                            }
                            callback();
                        },
                    },
                ],
                CardNumber: [
                    {
                        trigger: 'change',
                        message: this.$t('CardNumberOnlyAcceptNumericCharacters'),
                        validator: (rule, value: string, callback) => {
                            if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
                                callback(new Error());
                            }
                            callback();
                        },
                    },
                ],
                JoinedDate: [
                    {
                        required: true,
                        message: this.$t('PleaseSelectDayJoinedDate'),
                        trigger: 'change',
                    },
                ],
                DepartmentIndex: [
                    {
                        required: true,
                        message: this.$t('SelectDepartment'),
                        trigger: 'change'
                    },
                ],
                FullName: [
                    {
                        required: true,
                        message: this.$t('PleaseInputFullName'),
                        trigger: 'change',
                    },
                ]
            };
        }

        if (this.clientName == "PSV") {
            (this.formRules as any).EmployeeTypeIndex = [
                {
                    required: true,
                    message: this.$t('PleaseSelectEmployeeType'),
                    trigger: 'change',
                },
            ];

            this.checkboxRule = {
                isDeleteOnDevice: [
                    {
                        trigger: 'change',
                        message: this.$t('PleaseChooseDeleteAllMachine'),
                        validator: (rule, value: string, callback) => {
                            console.log(this.isDeleteOnDevice);
                            if (!this.isDeleteOnDevice) {
                                callback(new Error());
                            }
                            callback();
                        },
                    },
                ]
            }
        }
    }

    onEditClick() {
        this.formModel = this.selectedRows[0];
        if (this.clientName == 'MAY') {
            this.formModel['PositionIndex'] = this.formModel['EmployeeType']
        }
        if (this.formModel.Avatar) {
            this.fileList = [{ name: this.formModel.FullName }];
        }
        else {
            this.fileList = [];
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

        // await hrEmployeeInfoApi.GetEmployeeAtPage(this.filterModel.TextboxSearch?.trim(), this.filterModel.SelectedDepartment, this.page, this.pageSize, this.listWorkingStatus).then(response => {
        //     const { data, total }: { data: any[], total: number } = response.data;
        //     this.dataSource = data.map((emp, idx) => ({
        //         ...emp,
        //         index: idx + 1 + (this.page - 1) * this.pageSize,
        //         GenderTitle: this.$t(`${emp.Gender == 0 ? 'Female' : ''}${emp.Gender == 1 ? 'Male' : ''}`).toString(),
        //         PositionName: this.listAllPositionLookup[emp.PositionIndex]?.Name,
        //     }));
        //     this.total = total;
        // });

        if (this.filterModel.FromDate != null) {
            this.filterModel.FromDate = new Date(moment(this.filterModel.FromDate).format('YYYY-MM-DD'));
        }
        if (this.filterModel.ToDate != null) {
            this.filterModel.ToDate = new Date(moment(this.filterModel.ToDate).format('YYYY-MM-DD'));
        }
        if (this.filterModel.FromDate != null && this.filterModel.ToDate != null
            && this.filterModel.FromDate > this.filterModel.ToDate) {
            this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
            this.isLoading = false;
            return;
        }

        await hrEmployeeInfoApi.GetEmployeesAtPage(this.filterModel.TextboxSearch?.trim(), this.filterModel.SelectedDepartment,
            this.page, this.pageSize, this.listWorkingStatus, this.filterModel.FromDate, this.filterModel.ToDate).then(response => {
                const { data, total }: { data: any[], total: number } = response.data;
                this.dataSource = data.map((emp, idx) => ({
                    ...emp,
                    index: idx + 1 + (this.page - 1) * this.pageSize,
                    GenderTitle: this.$t(`${emp.Gender == 0 ? 'Female' : ''}${emp.Gender == 1 ? 'Male' : ''}${emp.Gender == 2 ? 'Other' : ''}`).toString(),
                    PositionName: this.listAllPositionLookup[emp.PositionIndex]?.Name,
                    EmployeeTypeIndex: (!emp.EmployeeTypeIndex || (emp.EmployeeTypeIndex && emp.EmployeeTypeIndex == 0)) ? null : emp.EmployeeTypeIndex,
                }));
                this.total = total;
                this.shouldResetColumnSortState = !this.shouldResetColumnSortState;
            });
        // console.log(this.dataSource)
        this.isLoading = false;
    }



    async doDelete() {
        if (this.clientName == 'PSV') {
            (this.$refs.isDeleteOnDevice as any).validateField('isDeleteOnDevice');
            if (!this.isDeleteOnDevice) return;
        }

        const selectedEmp = this.selectedRows.map(e => e.EmployeeATID);
        await hrEmployeeInfoApi
            .DeleteEmployeeMulti(selectedEmp)
            .then(async (res) => {

                if (this.isDeleteOnDevice) {
                    await commandApi.DeleteUserByIdFromUserId(selectedEmp).then((res) => {

                    })
                        .catch(() => { })

                }
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
        await this.loadData();
    }

    async getApiContactInfo() {

        await hrEmployeeInfoApi.GetAllEmployee().then((response) => {
            // this.listAllPosition = response.data;
            // // console.log(response.data)
            // this.listAllPosition.forEach(e => {
            //     this.listAllPositionLookup[e.Index] = e;
            // })
        });
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
        const a = (this.$refs.employeeFormModel as any);
        console.log(a);
        (this.$refs.employeeFormModel as any).validate(async (valid) => {
            if (!valid) return;
            (this.formModel as any).ListFinger = this.ListFingerRequest;
            (this.formModel as any).FromDate = new Date(moment((this.formModel as any).FromDate).format('YYYY-MM-DD'));
            (this.formModel as any).ToDate = (this.formModel as any).ToDate ? new Date(moment((this.formModel as any).ToDate).format('YYYY-MM-DD')) : null;
            if ((this.formModel as any).FromDate && (this.formModel as any).ToDate && (this.formModel as any).ToDate < (this.formModel as any).FromDate) {
                this.$alertSaveError(null, null, null, this.$t('JoinedDateCannotLargerThanToDate').toString()).toString();
                return;
            }
            const dateNow = new Date();
            if (this.filterModel.FromDate == null && this.filterModel.ToDate != null && this.filterModel.ToDate <= dateNow) {
                this.$alertSaveError(null, null, null, this.$t('ToDateCannotBeLessThanTheCurrentDate').toString()).toString();
                return;
            }

            (this.formModel as any).FromDate = new Date(moment((this.formModel as any).FromDate).format('YYYY-MM-DD'));
            (this.formModel as any).BirthDay = new Date(moment((this.formModel as any).BirthDay).format('YYYY-MM-DD'));
            const birthDay = new Date(moment((this.formModel as any).BirthDay).format('YYYY-MM-DD'));
            const ageDifferenceInMilliseconds = dateNow.getTime() - birthDay.getTime();
            const ageInYears = Math.floor(ageDifferenceInMilliseconds / (1000 * 60 * 60 * 24 * 365));
            if (this.clientName == "Mondelez" && ageInYears < 18) {
                this.$alertSaveError(null, null, null, this.$t('EmployeesAreUnder18YearsOld').toString()).toString();
                return;
            }

            // if (this.clientName == "Mondelez") {
            //     let firstChar =  (this.formModel as any).Nric.charAt(0);
            //     if (!isNaN(firstChar) && (this.formModel as any).Nric.length != 12) {
            //         this.$alertSaveError(null, null, null, this.$t('CCCDExactly12Characters').toString()).toString();
            //         return;
            //       }
            // }

            const atid = (this.formModel as any).EmployeeATID;
            if (this.isEdit) {
                Object.assign(this.formModel, { HR_ContactInfo: this.listContactInfoFormApi });
                await hrEmployeeInfoApi.UpdateEmployee(atid, this.formModel)
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
                    if (!Misc.isEmpty(
                        this.newInputContactInfo.Name &&
                        this.newInputContactInfo.Email &&
                        this.newInputContactInfo.Phone
                    )) {
                        this.listContactInfoFormApi.unshift({
                            Name: this.newInputContactInfo?.Name ?? '',
                            Email: this.newInputContactInfo?.Email ?? '',
                            Phone: this.newInputContactInfo?.Phone ?? '',
                        });
                    }

                    Object.assign(this.formModel, { HR_ContactInfo: this.listContactInfoFormApi });
                    await hrEmployeeInfoApi.AddEmployee(this.formModel).then(() => {
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
    listExcelFunction = ['AddExcel', 'DeleteExcel', 'ExportExcel'];

    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }

    onCancelClick() {
        (this.$refs.employeeFormModel as any).resetFields();
        this.formModel = {};
        this.fileList = [];
        this.showDialog = false;
        this.selectedRows = [];
        this.loadData();
    }

    async UploadDataFromExcel_MAY() {

        this.importErrorMessage = "";
        var arrData = [];
        let parentDataFromExcel = [];
        let subDataFromExcel = [];
        let arrFormatDataFromExcel = [];

        for (let element of this.dataAddExcel[0]) {
            if (
                element['Mã số khách hàng'] != 'Tên' ||
                element['Mã code thẻ'] != 'Email' ||
                element['Họ tên khách hàng'] != 'SDT'
            ) {
                if (element['Đối tượng khách hàng'] != undefined) {
                    if (element['Giới tính (Nam)'] === 'x' && element['Giới tính (Nam)'] === 'X') {
                        element['Giới tính (Nam)'] = 1;
                    }
                    else {
                        element['Giới tính (Nam)'] = 0;
                    }

                    parentDataFromExcel.push(element);
                }
            }
        }
        for (let i = 0; i < parentDataFromExcel.length; i++) {
            arrFormatDataFromExcel.push({
                "EmployeeCode": parentDataFromExcel[i]?.['Mã số khách hàng'] ?? '',
                "FullName": parentDataFromExcel[i]?.['Họ tên khách hàng'] ?? '',
                "Gender": parentDataFromExcel[i]?.['Giới tính (Nam)'] ?? '',
                "CardNumber": parentDataFromExcel[i]?.['Mã code thẻ'] ?? '',
                "DepartmentName": parentDataFromExcel[i]?.['Đơn vị khách hàng'] ?? '',
                "NameOnMachine": "",
                "Position": parentDataFromExcel[i]?.['Đối tượng khách hàng'] ?? '',
                "EmployeeATID": parentDataFromExcel[i]?.['Mã chấm ăn của khách hàng'] ?? '',
                "JoinedDate": parentDataFromExcel[i]?.['Ngày vào'] ?? '',
                "Email": parentDataFromExcel[i]?.['Email'] ?? '',
                "PhoneNumber": parentDataFromExcel[i]?.['SDT'] ?? '',
                "HR_ContactInfo": parentDataFromExcel[i]?.['HR_ContactInfo'] ?? [],
                "Address": parentDataFromExcel[i]?.['Địa chỉ'] ?? '',
                "Note": parentDataFromExcel[i]?.['Ghi chú'] ?? '',
                "ParentName1": parentDataFromExcel[i]?.['Tên phụ huynh 1'] ?? '',
                "ParentName2": parentDataFromExcel[i]?.['Tên phụ huynh 2'] ?? '',
                "ParentEmail1": parentDataFromExcel[i]?.['Email phụ huynh 1'] ?? '',
                "ParentEmail2": parentDataFromExcel[i]?.['Email phụ huynh 2'] ?? '',
                "ParentPhone1": parentDataFromExcel[i]?.['SDT phụ huynh 1'] ?? '',
                "ParentPhone2": parentDataFromExcel[i]?.['SDT phụ huynh 2'] ?? '',
            })
        }

        arrData = arrFormatDataFromExcel;

        // for (let i = 0; i < this.dataAddExcel[0].length; i++) {
        //     let a = Object.assign({});
        //     if (regex.test(this.dataAddExcel[0][i]['Mã chấm ăn của khách hàng']) === false) {
        //         this.$alertSaveError(null, null, null, this.$t('EmployeeATIDOnlyAcceptNumericCharacters').toString()).toString();
        //         return;
        //     } else if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm ăn của khách hàng')) {
        //         a.EmployeeATID = this.dataAddExcel[0][i]['Mã chấm ăn của khách hàng'] + '';
        //     } else {
        //         this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
        //         return;
        //     }
        //     if (this.dataAddExcel[0][i].hasOwnProperty('Mã số học sinh')) {
        //         a.EmployeeCode = this.dataAddExcel[0][i]['Mã số học sinh'] + '';
        //     } else {
        //         a.EmployeeCode = '';
        //     }
        //     if (this.dataAddExcel[0][i].hasOwnProperty('Họ tên khách hàng')) {
        //         a.FullName = this.dataAddExcel[0][i]['Họ tên khách hàng'] + '';
        //     } else {
        //         a.FullName = '';
        //     }
        //     if (this.dataAddExcel[0][i].hasOwnProperty('Mã code thẻ')) {
        //         a.CardNumber = this.dataAddExcel[0][i]['Mã code thẻ'] + '';
        //     } else {
        //         a.CardNumber = '';
        //     }
        //     if (this.dataAddExcel[0][i].hasOwnProperty('Giới tính (Nam)')) {
        //         a.Gender = 1;
        //     } else {
        //         a.Gender = 0;
        //     }
        //     if (this.dataAddExcel[0][i].hasOwnProperty('Đơn vị khách hàng')) {
        //         a.DepartmentName = this.dataAddExcel[0][i]['Đơn vị khách hàng'] + '';
        //     } else {
        //         a.DepartmentName = '';
        //     }
        //     if (this.dataAddExcel[0][i].hasOwnProperty('Đối tượng khách hàng')) {
        //         a.Position = this.dataAddExcel[0][i]['Đối tượng khách hàng'] + '';
        //     } else {
        //         a.Position = '';
        //     }

        //     // if (this.dataAddExcel[0][i]['Giới tính (Nam)'] == 'x') {
        //     //     a.Gender = 1;
        //     // } else {
        //     //     a.Gender = 0;
        //     // }
        //     // if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban')) {
        //     //     a.DepartmentName = this.dataAddExcel[0][i]['Phòng ban'] + '';
        //     // } else {
        //     //     a.DepartmentName = '';
        //     // }
        //     // if (this.dataAddExcel[0][i].hasOwnProperty('Chức vụ')) {
        //     //     a.Position = this.dataAddExcel[0][i]['Chức vụ'] + '';
        //     // } else {
        //     //     a.Position = '';
        //     // }
        //     if (this.dataAddExcel[0][i].hasOwnProperty('Ngày vào')) {
        //         a.JoinedDate = this.dataAddExcel[0][i]['Ngày vào'] + '';
        //     } else {
        //         a.JoinedDate = '';
        //     }
        //     // if (this.dataAddExcel[0][i].hasOwnProperty('Ngày sinh (ngày/tháng/năm)')) {
        //     //     a.DateOfBirth = this.dataAddExcel[0][i]['Ngày sinh (ngày/tháng/năm)'] + '';
        //     // } else {
        //     //     a.DateOfBirth = '';
        //     // }
        //     if (this.dataAddExcel[0][i].hasOwnProperty('Email')) {
        //         a.Email = this.dataAddExcel[0][i]['Email'] + '';
        //     } else {
        //         a.Email = '';
        //     }
        //     if (this.dataAddExcel[0][i].hasOwnProperty('SDT')) {
        //         a.PhoneNumber = this.dataAddExcel[0][i]['SDT'] + '';
        //     } else {
        //         a.PhoneNumber = '';
        //     }
        //     if (this.dataAddExcel[0][i].hasOwnProperty('Tên trên máy')) {
        //                 a.NameOnMachine = this.dataAddExcel[0][i]['Tên trên máy'] + '';
        //             } else {
        //                 a.NameOnMachine = '';
        //             }
        //     arrData.push(a);
        // }

        employeeInfoApi.AddEmployeeFromExcel(arrData).then((res) => {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
            }

            this.showDialogExcel = false;
            this.fileName = '';
            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
                this.$saveSuccess();
                this.loadData();
            } else {
                this.importErrorMessage = this.$t('ImportEmployeeErrorMessage') + res.data.toString() + " " + this.$t('Employee');
                this.showOrHideImportError(true);
            }
        });
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
            // if (regex.test(this.dataAddExcel[0][i]['Mã chấm công (*)']) === false) {
            //     this.$alertSaveError(null, null, null, this.$t('EmployeeATIDOnlyAcceptNumericCharacters').toString()).toString();
            //     return;
            // } else 
            if (this.clientName == "Mondelez") {
                if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm công (*)')) {
                    a.EmployeeATID = this.dataAddExcel[0][i]['Mã chấm công (*)'] + '';
                } else {
                    // this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                    // return;
                    a.EmployeeATID = '';
                }
                if (this.dataAddExcel[0][i].hasOwnProperty('Mã nhân viên')) {
                    a.EmployeeCode = this.dataAddExcel[0][i]['Mã nhân viên'] + '';
                } else {
                    a.EmployeeCode = '';
                }
                if (this.dataAddExcel[0][i].hasOwnProperty('Họ tên (*)')) {
                    a.FullName = this.dataAddExcel[0][i]['Họ tên (*)'] + '';
                } else {
                    a.FullName = '';
                }
                if (this.dataAddExcel[0][i].hasOwnProperty('Tên trên máy')) {
                    a.NameOnMachine = this.dataAddExcel[0][i]['Tên trên máy'] + '';
                } else {
                    a.NameOnMachine = '';
                }
                
                if (this.dataAddExcel[0][i]['Giới tính'] == 'Nam') {
                    a.Gender = 1;
                } else if(this.dataAddExcel[0][i]['Giới tính'] == 'Nữ') {
                    a.Gender = 0;
                }else{
                    a.Gender = 2;
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Ngày sinh (ngày/tháng/năm) (*)')) {
                    a.DateOfBirth = this.dataAddExcel[0][i]['Ngày sinh (ngày/tháng/năm) (*)'] + '';
                } else {
                    a.DateOfBirth = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Số điện thoại')) {
                    a.PhoneNumber = this.dataAddExcel[0][i]['Số điện thoại'] + '';
                } else {
                    a.PhoneNumber = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('CMND/CCCD/Passport (*)')) {
                    a.Nric = this.dataAddExcel[0][i]['CMND/CCCD/Passport (*)'] + '';
                } else {
                    a.Nric = '';
                }


                if (this.dataAddExcel[0][i].hasOwnProperty('Email')) {
                    a.Email = this.dataAddExcel[0][i]['Email'] + '';
                } else {
                    a.Email = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Địa chỉ')) {
                    a.Address = this.dataAddExcel[0][i]['Địa chỉ'] + '';
                } else {
                    a.Address = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban (*)')) {
                    a.DepartmentName = this.dataAddExcel[0][i]['Phòng ban (*)'] + '';
                } else {
                    a.DepartmentName = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Chức vụ')) {
                    a.Position = this.dataAddExcel[0][i]['Chức vụ'] + '';
                } else {
                    a.Position = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Loại nhân viên')) {
                    a.EmployeeTypeName = this.dataAddExcel[0][i]['Loại nhân viên'] + '';
                } else {
                    a.EmployeeTypeName = '';
                }

                if (this.dataAddExcel[0][i]['Sử dụng điện thoại'] == 'x') {
                    a.IsAllowPhone = 1;
                } else {
                    a.IsAllowPhone = 0;
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Ngày vào')) {
                    a.JoinedDate = this.dataAddExcel[0][i]['Ngày vào'] + '';
                } else {
                    a.JoinedDate = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Ngày nghỉ')) {
                    a.StoppedDate = this.dataAddExcel[0][i]['Ngày nghỉ'] + '';
                } else {
                    a.StoppedDate = '';
                }

                //---------------------------------------------------------
                if (this.dataAddExcel[0][i].hasOwnProperty('Mật khẩu')) {
                    a.Password = this.dataAddExcel[0][i]['Mật khẩu'] + '';
                } else {
                    a.Password = '';
                }
                if (this.dataAddExcel[0][i].hasOwnProperty('Mã thẻ')) {
                    a.CardNumber = this.dataAddExcel[0][i]['Mã thẻ'] + '';
                } else {
                    a.CardNumber = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Tổ')) {
                    a.TeamName = this.dataAddExcel[0][i]['Tổ'] + '';
                } else {
                    a.TeamName = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Ghi chú')) {
                    a.Note = this.dataAddExcel[0][i]['Ghi chú'] + '';
                } else {
                    a.Note = '';
                }
            } else {
                if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm công (*)')) {
                    a.EmployeeATID = this.dataAddExcel[0][i]['Mã chấm công (*)'] + '';
                } else {
                    // this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                    // return;
                    a.EmployeeATID = '';
                }
                if (this.dataAddExcel[0][i].hasOwnProperty('Mã nhân viên')) {
                    a.EmployeeCode = this.dataAddExcel[0][i]['Mã nhân viên'] + '';
                } else {
                    a.EmployeeCode = '';
                }
                if (this.dataAddExcel[0][i].hasOwnProperty('Họ tên (*)')) {
                    a.FullName = this.dataAddExcel[0][i]['Họ tên (*)'] + '';
                } else {
                    a.FullName = '';
                }
                if (this.dataAddExcel[0][i].hasOwnProperty('Tên trên máy')) {
                    a.NameOnMachine = this.dataAddExcel[0][i]['Tên trên máy'] + '';
                } else {
                    a.NameOnMachine = '';
                }
                
                if (this.dataAddExcel[0][i]['Giới tính'] == 'Nam') {
                    a.Gender = 1;
                } else if(this.dataAddExcel[0][i]['Giới tính'] == 'Nữ') {
                    a.Gender = 0;
                }else{
                    a.Gender = 2;
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Ngày sinh (ngày/tháng/năm)')) {
                    a.DateOfBirth = this.dataAddExcel[0][i]['Ngày sinh (ngày/tháng/năm)'] + '';
                } else {
                    a.DateOfBirth = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Số điện thoại')) {
                    a.PhoneNumber = this.dataAddExcel[0][i]['Số điện thoại'] + '';
                } else {
                    a.PhoneNumber = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('CMND/CCCD/Passport')) {
                    a.Nric = this.dataAddExcel[0][i]['CMND/CCCD/Passport'] + '';
                } else {
                    a.Nric = '';
                }


                if (this.dataAddExcel[0][i].hasOwnProperty('Email')) {
                    a.Email = this.dataAddExcel[0][i]['Email'] + '';
                } else {
                    a.Email = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Địa chỉ')) {
                    a.Address = this.dataAddExcel[0][i]['Địa chỉ'] + '';
                } else {
                    a.Address = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban (*)')) {
                    a.DepartmentName = this.dataAddExcel[0][i]['Phòng ban (*)'] + '';
                } else {
                    a.DepartmentName = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Chức vụ')) {
                    a.Position = this.dataAddExcel[0][i]['Chức vụ'] + '';
                } else {
                    a.Position = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Loại nhân viên')) {
                    a.EmployeeTypeName = this.dataAddExcel[0][i]['Loại nhân viên'] + '';
                } else {
                    a.EmployeeTypeName = '';
                }

                if (this.dataAddExcel[0][i]['Sử dụng điện thoại'] == 'x') {
                    a.IsAllowPhone = 1;
                } else {
                    a.IsAllowPhone = 0;
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Ngày vào')) {
                    a.JoinedDate = this.dataAddExcel[0][i]['Ngày vào'] + '';
                } else {
                    a.JoinedDate = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Ngày nghỉ')) {
                    a.StoppedDate = this.dataAddExcel[0][i]['Ngày nghỉ'] + '';
                } else {
                    a.StoppedDate = '';
                }

                //---------------------------------------------------------
                if (this.dataAddExcel[0][i].hasOwnProperty('Mật khẩu')) {
                    a.Password = this.dataAddExcel[0][i]['Mật khẩu'] + '';
                } else {
                    a.Password = '';
                }
                if (this.dataAddExcel[0][i].hasOwnProperty('Mã thẻ')) {
                    a.CardNumber = this.dataAddExcel[0][i]['Mã thẻ'] + '';
                } else {
                    a.CardNumber = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Tổ')) {
                    a.TeamName = this.dataAddExcel[0][i]['Tổ'] + '';
                } else {
                    a.TeamName = '';
                }

                if (this.dataAddExcel[0][i].hasOwnProperty('Ghi chú')) {
                    a.Note = this.dataAddExcel[0][i]['Ghi chú'] + '';
                } else {
                    a.Note = '';
                }
            }

            arrData.push(a);
        }
        employeeInfoApi.AddEmployeeFromExcel(arrData).then((res) => {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
            }

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
                this.importErrorMessage = this.$t('ImportEmployeeErrorMessage') + res.data.toString() + " " + this.$t('Employee');
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
        this.addedParams = [];
        this.addedParams.push({ Key: "ListDepartment", Value: this.filterModel.SelectedDepartment });
        this.addedParams.push({ Key: "Filter", Value: this.filterModel.TextboxSearch });
        if (this.selectedRows.length > 0) {
            this.addedParams.push({ Key: "ListEmployeeATID", Value: this.selectedRows.map(x => x.EmployeeATID) });
        }
        this.addedParams.push({ Key: "IsWorking", Value: this.listWorkingStatus });

        await employeeInfoApi.ExportToExcel(this.addedParams).then((res: any) => {
            const fileURL = window.URL.createObjectURL(new Blob([res.data]));
            const fileLink = document.createElement("a");

            fileLink.href = fileURL;
            fileLink.setAttribute("download", `Employee_${moment(this.fromDate).format('YYYY-MM-DD HH:mm')}.xlsx`);
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


    //Check QR CCCD
    showWaitScanDialog = false;
    titleScanDialog:any = '';
    messageScanDialog: any = '';
    typeScanDialog = null;
    closeSecScanDialog = 0;
    scanQR = null;

    openReadInfoFromQRCCCD(){
        this.titleScanDialog = this.$t('PleaseScanQRCCCD');
        this.messageScanDialog = this.$t('WaitingScan');
        this.typeScanDialog = null;
        this.closeSecScanDialog = 0;
        this.showWaitScanDialog = true;
        this.scanQR = null;
    }

    cancelScanDialog(){
        this.titleScanDialog = '';
        this.messageScanDialog = '';
        this.typeScanDialog = null;
        this.closeSecScanDialog = 0;
        this.showWaitScanDialog = false;
        this.scanQR = null;
    }

    changeScanQR(value, valid){
        if(valid){
            this.scanQR = value;
            this.typeScanDialog = true;
            this.messageScanDialog = null;
            this.closeSecScanDialog = 0.5;
            const qrData = value.split('|');
            if(qrData && qrData.length > 0){
                this.formModel.Nric = qrData[0].toString();
                this.formModel.FullName = qrData[2].toString();
                this.formModel.BirthDay = this.parseDDMMYYYY(qrData[3]);
                this.formModel.Gender = qrData[4].toString() == this.$t('Male').toString() ? 1 
                    : (qrData[4].toString() == this.$t('Female').toString() ? 0 : 2);
                this.formModel.Address = qrData[5].toString();
                this.$forceUpdate();
                // (this.$refs.customerFormModel as any).validate();
            }
        }else{
            this.typeScanDialog = false;
            this.messageScanDialog = this.$t('InvalidQR');
        }
    }

    parseDDMMYYYY(dateString) {
        // Check if the string matches the DDMMYYYY format
        if (!/^\d{8}$/.test(dateString)) {
          throw new Error("Invalid date format. Expected DDMMYYYY.");
        }
      
        // Extract day, month, and year
        const day = parseInt(dateString.slice(0, 2), 10);
        const month = parseInt(dateString.slice(2, 4), 10) - 1; // JS months are 0-indexed
        const year = parseInt(dateString.slice(4, 8), 10);
      
        // Create and return a new Date object
        const date = new Date(year, month, day);
      
        // Check if the date is valid
        if (isNaN(date.getTime())) {
          throw new Error("Invalid date.");
        }
      
        return date;
    }
}
