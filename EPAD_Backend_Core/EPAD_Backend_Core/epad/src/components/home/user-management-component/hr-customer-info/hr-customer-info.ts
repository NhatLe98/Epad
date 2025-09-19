import { Component, Vue, Mixins, Prop, Watch } from 'vue-property-decorator';
import { departmentApi } from '@/$api/department-api';
import TGrid from '@/components/app-component/t-grid/t-grid.vue';
import TabBase from '@/mixins/application/tab-mixins';
import { hrCustomerInfoApi } from '@/$api/hr-customer-info-api';
import { hrUserApi } from '@/$api/hr-user-api';
import { employeeInfoApi, Finger } from '@/$api/employee-info-api';
import { isNullOrUndefined } from "util";
import * as XLSX from 'xlsx';
import { isEmpty } from '@/$core/misc';

import VisualizeTable from '@/components/app-component/visualize-table/visualize-table.vue';
import AppPagination from '@/components/app-component/app-pagination/app-pagination.vue';
import ImageCellRendererVisualizeTable from '@/components/app-component/visualize-table/image-cell-renderer-visualize-table.vue';
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import TableToolbar from '@/components/home/printer-control/table-toolbar.vue';
import { userTypeApi } from '@/$api/user-type-api';
import { hrPositionInfoApi } from '@/$api/hr-position-info-api';
import AlertComponent from '@/components/app-component/alert-component/alert-component.vue';

@Component({
    name: 'hr-customer-info',
    components: {
        VisualizeTable,
        AppPagination,
        ImageCellRendererVisualizeTable, 
        SelectTreeComponent, SelectDepartmentTreeComponent,TableToolbar,
        AlertComponent
    },
})
export default class HRCustomerInfo extends Mixins(TabBase) {
    //===== for WS Register finger
    showFingerDialog = false;
    wsUri = "ws://127.0.0.1:22003";
    websocket;
    currentIndex: number = 0;
    DeviceInfo: string = this.$t('NotConnectedDevice').toString();
    ConnectedDevice: boolean = false;
    template1: string = "";
    template2: string = "";
    registerCount: number = 0;
    listFinger: Array<Finger> = [];
    //=====================
    isLoading = false;
    importErrorMessage = "";
    showDialogImportError = false;
    fileImageName = '';
    fileIdentityImageName = '';
    errorUpload = false;
    fileList = [];
    fileListIdentityImage = [];
    filterModel = { SelectedEmployee: [], TextboxSearch: '' }
    listEmployeeAndDepartment = [];
    dataSource = [];
    insertFormLabel = "";
    editFormLabel = "";
    employeeFullLookupForm = {};
    allEmployeeFullLookupForm = [];
    employeeFullLookupFilter = {};
    listAllPosition = [];
    listAllPositionLookup = {};
    href = "/CustomerImport_Guest.xlsx";
    clientName = "";
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
    @Prop({ default: () => false }) showMore: boolean;
    columnDefs = [];
    formModel: any = { FromTime: new Date(), ToTime: new Date(), PositionIndex: null,ContactDepartment: null };
    @Prop({ default: () => 2 }) idEnum: number;
    @Prop({ default: () => null }) tabName: any;
    @Prop({ default: () => [] }) isActive: number;
    shouldResetColumnSortState = false;
    filterStudentOfParent = [];
    filterDepartments = [];

    listAllCustomer = [];
    listAllCustomerFilter = [];
    filterCustomerID = null;

    onSelectionChange(selectedRows: any[]) {
        // // console.log(selectedRows);
        this.selectedRows = selectedRows;
    }

    onInsertClick(){
        this.formModel = {
            NRIC: null,
            FullName: null,
            BirthDay: null,
            Gender: null,
            Address: null
        };
        this.isEdit = false;
        this.showDialog = true;
    }

    onEditClick() {
        this.formModel = this.selectedRows[0];
        this.filterCustomerID = this.formModel.EmployeeATID;
        this.formModel.PositionIndex = (!this.formModel.PositionIndex || this.formModel.PositionIndex == 0) 
            ? null : this.formModel.PositionIndex;
        if (this.clientName == 'MAY') {
            this.formModel['PositionIndex'] = this.formModel['EmployeeType']
        }
        if(this.formModel.Avatar){
            this.fileList = [{name: this.formModel.FullName}];
        }
        else
        {
            this.fileList = [];
        }
        this.isEdit = true;
        this.showDialog = true;
    }

    onReadGoogleSheetClick(){
        hrCustomerInfoApi.ImportDataFromGoogleSheet().then((res: any) => {
            if (res.status == 200) {
                this.loadData();
                this.getAllCustomer();
                this.$saveSuccess();
            }
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

    async initLookup() {
        await this.getEmployeesData();
        await this.initPositionInfoLookup();
        // await this.initEmployeeAndDepartmentLookup();
        await this.LoadDepartmentTree();
    }


    async LoadDepartmentTree() {
        await departmentApi.GetDepartmentTreeEmployeeScreen(this.idEnum.toString()).then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });
    }

    async initEmployeeAndDepartmentLookup() {
        let formatData = [];

        await hrUserApi.GetEmployeeAndDepartmentLookup().then(response => {
            formatData = response?.data ?? [];
        })

        if (!isEmpty(formatData)) {
            formatData.forEach(obj => {
                if (!isEmpty(obj)) {
                    this.listEmployeeAndDepartment.push(obj);
                }
            });
        }
    }
    
    async getEmployeesData() {
		await hrUserApi.GetAllEmployeeCompactInfo().then((res: any) => {
			if (res.status == 200) {
				const data = res.data;
				const dictData = {};
				data.forEach((e: any) => {
					dictData[e.EmployeeATID] = {
						Index: e.EmployeeATID,
						Name: `${e.FullName}`,
						NameInEng: `${e.FullName}`,
						NameInFilter: `${e.EmployeeATID} - ${e.FullName}`,
						Code: e.EmployeeATID,
						Department: e.Department,
						Position: e.Position,
						DepartmentIndex: e.DepartmentIndex,
					};
				});
				this.employeeFullLookupForm = dictData;
				this.allEmployeeFullLookupForm = data;
				this.employeeFullLookupFilter = dictData;
			}
		});
	}

    async getAllCustomer() {
        await hrCustomerInfoApi.GetNewestCustomerInfo().then((res: any) => {
            if (res.status == 200) {
                const data = res.data;
				if(data && data.length > 0){
					data.forEach(element => {
						element.ContactDepartmentName = (element.ContactDepartmentName && element.ContactDepartmentName != '') 
						? this.$t(element.ContactDepartmentName).toString() : '';
					});
				}
                this.listAllCustomer = data;
                this.listAllCustomerFilter = data.map(x => ({
                    Index: x.EmployeeATID,
                    FullName: (x.FullName && x.FullName != '') ? (x.FullName + " - " + x.NRIC) : (x.EmployeeATID + " - " + x.NRIC),
				}));
            }
        });
    }

    async changeCustomer(value){
        if(value != this.filterCustomerID){
            // console.log(value);
            this.filterCustomerID = value;
            if(!this.filterCustomerID){
                this.isEdit = false;
                this.formModel = {ContactDepartment: null};
                return;
            }
            if(this.idEnum == 4 || this.idEnum == 6){
                await hrCustomerInfoApi.GetCustomerAtPageAdvanceByEmployeeATID([this.filterCustomerID], 
                    '', this.page, this.pageSize, this.idEnum, [],
                        []).then(response => {
                    const { data, total } = response.data;
                    if(data && data.length > 0){
                        this.formModel = data[0];
                        this.isEdit = true;
                    }
                    // console.log(data, total)

                }).catch(err => {
                    console.log(err)
                })
            }else{
                await hrCustomerInfoApi.GetCustomerAtPageByEmployeeATID([this.filterCustomerID], '', this.page, this.pageSize, this.idEnum).then(response => {
                    const { data, total } = response.data;
                    if(data && data.length > 0){
                        this.formModel = data[0];
                        this.isEdit = true;
                    }
                    // console.log(data, total)
                }).catch(err => {
                    console.log(err)
                })
            }
        }
    }

    setValueContactDepartment(value){
        this.formModel.ContactPerson = null;
        this.formModel.ContactDepartment = value;
        this.changeContactDepartment();
    }

    changeContactDepartment(){
        let filterContactPersonForm = [];
        this.employeeFullLookupForm = {};
        if(this.formModel.ContactDepartment){
            if(this.formModel.ContactDepartment > 0){
                filterContactPersonForm = Misc.cloneData(this.allEmployeeFullLookupForm.filter(x => 
                    x.DepartmentIndex == this.formModel.ContactDepartment));
            }else{
                filterContactPersonForm = Misc.cloneData(this.allEmployeeFullLookupForm.filter(x => 
                    !x.DepartmentIndex || x.DepartmentIndex == 0));
            }
        }else{
            filterContactPersonForm = Misc.cloneData(this.allEmployeeFullLookupForm);
        }

        if(filterContactPersonForm && filterContactPersonForm.length > 0){
            filterContactPersonForm.forEach((e: any) => {
                this.employeeFullLookupForm[e.EmployeeATID] = {
                    Index: e.EmployeeATID,
                    Name: `${e.FullName}`,
                    NameInEng: `${e.FullName}`,
                    NameInFilter: `${e.EmployeeATID} - ${e.FullName}`,
                    Code: e.EmployeeATID,
                    Department: e.Department,
                    Position: e.Position,
                    DepartmentIndex: e.DepartmentIndex,
                };
            });
        }
    }

    changeContactPerson(){
        if(this.formModel.ContactPerson && this.formModel.ContactPerson > 0){
            const contactPerson = this.allEmployeeFullLookupForm.find(x => x.EmployeeATID == this.formModel.ContactPerson);
            if(contactPerson){
                if(contactPerson.DepartmentIndex && contactPerson.DepartmentIndex > 0){
                    this.formModel.ContactDepartment = contactPerson.DepartmentIndex;
                }else{
                    this.formModel.ContactDepartment = 0;
                }
            }
        }
        this.changeContactDepartment();
    }

    mounted() {
    }

    async beforeMount() {
        if(this.idEnum == 4){
            this.listExcelFunction = ['AddExcel'];
        }
        await Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.clientName = x.ClientName;

            if (this.idEnum == 2) {
                if (this.clientName == "Mondelez") {
                    this.href = "/CustomerImport_Guest_Mdl.xlsx"
                }
                this.initFormRules_Guest();
                // this.initFormRuless();
                this.initGridColumnsCustomer();
            }
            else if (this.idEnum == 4) {
                this.href = "/CustomerImport_Parent.xlsx"
                this.initGridColumnsNotCustomer();
                this.initFormRulesNotCustomer();
            }
            else if (this.idEnum == 6) {
                if (this.clientName == "Mondelez") {
                    this.href = "/CustomerImport_Contractor_Mdl.xlsx"
                }
                else {
                    this.href = "/CustomerImport_Contractor.xlsx"
                }
                this.initGridColumnsContractor();
                this.initFormRuless();

            }
            else {
                this.initGridColumnsNotCustomer();
                this.initFormRulesNotCustomer();
            }
        });

        if (this.tabName == "CustomerManagement") {
            this.insertFormLabel = "InsertCustomer";
            this.editFormLabel = "EditCustomer";
        } else if (this.tabName == "ContractorManagement") {
            this.insertFormLabel = "InsertContractor";
            this.editFormLabel = "EditContractor";
        } else if (this.tabName == "StudentManagement") {
            this.insertFormLabel = "InsertStudent";
            this.editFormLabel = "EditStudent";
        } else if (this.tabName == "ParentManagement") {
            this.insertFormLabel = "InsertParent";
            this.editFormLabel = "EditParent";
        } else if (this.tabName == "TeacherManagement") {
            this.insertFormLabel = "InsertTeacher";
            this.editFormLabel = "EditTeacher";
        } else if (this.tabName == "NannyManagement") {
            this.insertFormLabel = "InsertNanny";
            this.editFormLabel = "EditNanny";
        }

        hrCustomerInfoApi.InfoCustomerTemplateImport().then((res: any) => {
            // console.log(res);
        })
        hrCustomerInfoApi.InfoContractorTemplateImport().then((res: any) => {
            // console.log(res);
        })
        await this.getAllCustomer();
    }

    initGridColumnsNotCustomer() {
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
                headerName: this.$t('UserCode'),
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
                headerName: this.$t('Address'),
                field: 'Address',
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
                display: true,
                // cellRenderer: params => `${params.value == 1 ? this.$t('Male').toString() : this.$t('Female').toString()}`,

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
                // name: 'CardNumber',
                // dataField: 'CardNumber',
                // fixed: false,
                // width: 150,
                // show: true,
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
        ];
        if(this.idEnum == 4){
            this.columnDefs.push(
                {
                    headerName: this.$t('Student'),
                    field: 'StudentOfParentString',
                    pinned: false,
                    width: 300,
                    sortable: true,
                    display: true
                },
            );
        }
    }

    initGridColumnsCustomer() {
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
            // {
            //     field: 'IdentityImage',
            //     headerName: this.$t('IdentityImage'),
            //     pinned: true,
            //     sortable: true,
            //     width: 150,
            //     cellRenderer: 'ImageCellRendererVisualizeTable',
            //     display: true
            // },
            {
                field: 'EmployeeATID',
                headerName: this.$t('UserCode'),
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
                headerName: this.$t('NameOnMachine'),
                field: 'NameOnMachine',
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
                headerName: this.$t('CMND/CCCD/Passport'),
                field: 'NRIC',
                pinned: false,
                width: 200,
                sortable: true,
                display: true
            },
            {
                headerName: this.$t('CompanyName'),
                field: 'Company',
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
                headerName: this.$t('Email'),
                field: 'Email',
                pinned: false,
                width: 150,
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
                headerName: this.$t('ContactDepartment'),
                field: 'ContactDepartmentGuest',
                pinned: false,
                width: 150,
                sortable: true,
                display: true
            },
            {
                headerName: this.$t('ContactPerson'),
                field: 'ContactPersonGuest',
                pinned: false,
                width: 150,
                sortable: true,
                display: true
            },
            {
                headerName: this.$t('FromDateString'),
                field: 'FromTime',
                pinned: false,
                width: 150,
                sortable: true,
                cellRenderer: params => `${moment(params.value).format('DD-MM-YYYY')}`,
                display: true
            },
            {
                headerName: this.$t('ToDateString'),
                field: 'ToTime',
                pinned: false,
                width: 150,
                sortable: true,
                cellRenderer: params => `${moment(params.value).format('DD-MM-YYYY')}`,
                display: true
            },
            {
                headerName: this.$t('StartTime'),
                field: 'StartTime',
                pinned: false,
                width: 150,
                sortable: true,
                cellRenderer: params => `${moment(params.value).format('HH:mm:ss')}`,
                display: true
            },
            {
                headerName: this.$t('EndTime'),
                field: 'ToTime',
                pinned: false,
                width: 150,
                sortable: true,
                cellRenderer: params => `${moment(params.value).format('HH:mm:ss')}`,
                display: true
            },
            {
                headerName: this.$t('PhoneUseIsAllow'),
                field: 'IsAllowPhone',
                pinned: false,
                width: 300,
                sortable: true,
                display: true,
                cellRenderer: params => `${params.value === true ? this.$t('Yes').toString() : this.$t('No').toString()}`,
            },
            {
                headerName: this.$t('Gender'),
                field: 'GenderTitle',
                // dataType: 'lookup',
                pinned: false,
                width: 150,
                sortable: true,
                display: true,
                // cellRenderer: params => `${params.value == 1 ? this.$t('Male').toString() : this.$t('Female').toString()}`,
            },
            // {
            //     headerName: this.$t('CardNumber'),
            //     field: 'CardNumber',
            //     pinned: false,
            //     width: 150,
            //     sortable: true,
            //     display: true
            // },
           
            {
                headerName: this.$t('WorkingContent'),
                field: 'WorkingContent',
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
        ];
    }

    initGridColumnsContractor() {
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
                headerName: this.$t('UserCode'),
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
                field: 'PositionName',
                headerName: this.$t('Position'),
                pinned: false,
                width: 170,
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
                headerName: this.$t('CMND/CCCD/Passport'),
                field: 'NRIC',
                pinned: false,
                width: 200,
                sortable: true,
                display: true
            },
            {
                headerName: this.$t('StartDate'),
                field: 'FromTime',
                pinned: false,
                width: 150,
                sortable: true,
                cellRenderer: params => `${moment(params.value).format('DD-MM-YYYY')}`,
                display: true
            },
            {
                headerName: this.$t('EndDate'),
                field: 'ToTime',
                pinned: false,
                width: 150,
                sortable: true,
                cellRenderer: params => `${params.value != null ? moment(params.value).format('DD-MM-YYYY') : ""}`,
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
                headerName: this.$t('Gender'),
                field: 'GenderTitle',
                // dataType: 'lookup',
                pinned: false,
                width: 150,
                sortable: true,
                display: true,
                // cellRenderer: params => `${params.value == 1 ? this.$t('Male').toString() : this.$t('Female').toString()}`,

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
                width: 300,
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


        ];
    }

    @Watch("isActive")
    loadCustomerData() {
        if (this.isActive === this.idEnum) {
            this.loadData();
        }
    }

    onPageChange() {
        this.loadData();
    }

    onPageSizeChange() {
        this.loadData();
    }

    initFormRuless() {
        if (this.clientName == "Mondelez") {
            this.formRules = {
                EmployeeATID: [
                    {
                        required: true,
                        message: this.$t('PleaseInputUserID'),
                        trigger: 'change',
                    },
                    {
                        trigger: 'change',
                        message: this.$t('UserIDOnlyAcceptNumericCharacters'),
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
                FullName: [
                    {
                        required: true,
                        message: this.$t('PleaseInputFullName'),
                        trigger: 'change',
                    },
                ],
                NRIC: [
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
                ToTime: [
                    {
                        required: true,
                        message: this.$t("PleaseChooseToDate"),
                        trigger: "change",
                    },
                    {
                        required: true,
                        trigger: 'change',
                        message: this.$t('ToDateCannotSmallerThanFromDate'),
                        validator: (rule, value: string, callback) => {
                            const item = Object.assign(this.formModel);
                            if (item.ToTime < item.FromTime) {
                                callback(new Error());
                            }
                            callback();
                        },
                    }
                ],
                DepartmentIndex: [
                    {
                        required: true,
                        message: this.$t('PleaseInputDepartment'),
                        trigger: 'change'
                    },
                ],
                BirthDay: [
                    {
                        required: true,
                        message: this.$t('PleaseInputBirthDay'),
                        trigger: 'change',
                    },
                ],
                FromTime: [
                    {
                        required: true,
                        message: this.$t('PleaseChooseFromDate'),
                        trigger: 'change',
                    },
                    {
                        required: true,
                        trigger: 'change',
                        message: this.$t('FromDateCannotLargerThanToDate'),
                        validator: (rule, value: string, callback) => {
                            const item = Object.assign(this.formModel);
                            if (item.ToTime < item.FromTime) {
                                callback(new Error());
                            }
                            callback();
                        },
                    }
                ],
                StartTime: [
                    {
                        required: true,
                        message: this.$t('PleaseSelectFromTime'),
                        trigger: 'change',
                    },
                    {
                        trigger: 'change',
                        message: this.$t('FromTimeCannotLargerThanToTime'),
                        validator: (rule, value: string, callback) => {
                            const startValue = new Date();
                            // const startTime = (this.formModel as any).StartTime;

                            const startTime = new Date(moment((this.formModel as any).StartTime).format('YYYY-MM-DD'));

                            
                            startValue.setHours(startTime.getHours(),startTime.getMinutes(), startTime.getSeconds());
                            console.log("HRCustomerInfo ~ initFormRuless ~ startTime:", startValue)
                            
                            const endValue = new Date();
                            // const endTime = (this.formModel as any).EndTime;

                            const endTime = new Date(moment((this.formModel as any).EndTime).format('YYYY-MM-DD'));

                            endValue.setHours(endTime.getHours(), endTime.getMinutes(), endTime.getSeconds());
                            console.log("HRCustomerInfo ~ initFormRuless ~ endTime:", endValue)

                            const start = startTime
                                ? Math.trunc(startValue.getTime() / 1000) : 0;
                            const end = endTime
                                ? Math.trunc(endValue.getTime() / 1000) : 0;
                            if (start > end) {
                                callback(new Error());
                            }else{
                                callback();
                            }
                        },
                    },
                ],
                EndTime: [
                    {
                        required: true,
                        message: this.$t('PleaseSelectToTime'),
                        trigger: 'change',
                    },
                    {
                        trigger: 'change',
                        message: this.$t('FromTimeCannotLargerThanToTime'),
                        validator: (rule, value: string, callback) => {
                            const startValue = new Date();
                            // const startTime = (this.formModel as any).StartTime;

                            const startTime = new Date(moment((this.formModel as any).StartTime).format('YYYY-MM-DD'));
                            startValue.setHours(startTime.getHours(),startTime.getMinutes(), startTime.getSeconds());
                            console.log("HRCustomerInfo ~ initFormRuless ~ startTime:", startValue)
                            
                            const endValue = new Date();
                            // const endTime = (this.formModel as any).EndTime;
                            const endTime = new Date(moment((this.formModel as any).EndTime).format('YYYY-MM-DD'));
                            
                            endValue.setHours(endTime.getHours(), endTime.getMinutes(), endTime.getSeconds());
                            console.log("HRCustomerInfo ~ initFormRuless ~ endTime:", endValue)

                            const start = startTime
                                ? Math.trunc(startValue.getTime() / 1000) : 0;
                            const end = endTime
                                ? Math.trunc(endValue.getTime() / 1000) : 0;
                            if (start > end) {
                                callback(new Error());
                            }else{
                                callback();
                            }
                        },
                    },
                ],
                ContactDepartment: [
                    {
                        required: true,
                        message: this.$t('PleaseInputContactDepartment'),
                        trigger: 'change'
                    },
                ],
                ContactPerson: [
                    {
                        required: true,
                        message: this.$t('PleaseInputContactPerson'),
                        trigger: 'change'
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
                FullName: [
                    {
                        required: true,
                        message: this.$t('PleaseInputFullName'),
                        trigger: 'change',
                    },
                ],
                ToTime: [
                    {
                        trigger: 'change',
                        message: this.$t('ToTimeMustBeGreaterThanFromTime'),
                        validator: (rule, value: string, callback) => {
                            const item = Object.assign(this.formModel);
                            if (item.ToTime < item.FromTime) {
                                callback(new Error());
                            }
                            callback();
                        },
                    }
                ],
                DepartmentIndex: [
                    {
                        required: true,
                        message: this.$t('PleaseInputDepartment'),
                        trigger: 'change'
                    },
                ]
            };
        }

    }

    initFormRules_Guest() {
        if (this.clientName == "Mondelez") {
            this.formRules = {
                EmployeeATID: [
                    {
                        trigger: 'change',
                        message: this.$t('UserIDOnlyAcceptNumericCharacters'),
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
                            if(Misc.isEmpty(value) === false && value.length != 8){
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
                FullName: [
                    {
                        required: true,
                        message: this.$t('PleaseInputFullName'),
                        trigger: 'change',
                    },
                ],
                NRIC: [
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
                ToTime: [
                    {
                        required: true,
                        message: this.$t("PleaseChooseToDate"),
                        trigger: "change",
                    },
                    {
                        required: true,
                        trigger: 'change',
                        message: this.$t('ToDateCannotSmallerThanFromDate'),
                        validator: (rule, value: string, callback) => {
                            const item = Object.assign(this.formModel);
                            if (item.ToTime < item.FromTime) {
                                callback(new Error());
                            }
                            callback();
                        },
                    }
                ],
                DepartmentIndex: [
                    {
                        required: true,
                        message: this.$t('PleaseInputDepartment'),
                        trigger: 'change'
                    },
                ],
                BirthDay: [
                    {
                        required: true,
                        message: this.$t('PleaseInputBirthDay'),
                        trigger: 'change',
                    },
                ],
                FromTime: [
                    {
                        required: true,
                        message: this.$t('PleaseChooseFromDate'),
                        trigger: 'change',
                    },
                    {
                        required: true,
                        trigger: 'change',
                        message: this.$t('FromDateCannotLargerThanToDate'),
                        validator: (rule, value: string, callback) => {
                            const item = Object.assign(this.formModel);
                            if (item.ToTime < item.FromTime) {
                                callback(new Error());
                            }
                            callback();
                        },
                    }
                ],
                StartTime: [
                    {
                        required: true,
                        message: this.$t('PleaseSelectFromTime'),
                        trigger: 'change',
                    },
                    {
                        trigger: 'change',
                        message: this.$t('FromTimeCannotLargerThanToTime'),
                        validator: (rule, value: string, callback) => {
                            const startValue = new Date();
                            // const startTime = (this.formModel as any).StartTime;

                            const startTime = new Date(moment((this.formModel as any).StartTime).format('YYYY-MM-DD'));

                            
                            startValue.setHours(startTime.getHours(),startTime.getMinutes(), startTime.getSeconds());
                            console.log("HRCustomerInfo ~ initFormRuless ~ startTime:", startValue)
                            
                            const endValue = new Date();
                            // const endTime = (this.formModel as any).EndTime;

                            const endTime = new Date(moment((this.formModel as any).EndTime).format('YYYY-MM-DD'));

                            endValue.setHours(endTime.getHours(), endTime.getMinutes(), endTime.getSeconds());
                            console.log("HRCustomerInfo ~ initFormRuless ~ endTime:", endValue)

                            const start = startTime
                                ? Math.trunc(startValue.getTime() / 1000) : 0;
                            const end = endTime
                                ? Math.trunc(endValue.getTime() / 1000) : 0;
                            if (start > end) {
                                callback(new Error());
                            }else{
                                callback();
                            }
                        },
                    },
                ],
                EndTime: [
                    {
                        required: true,
                        message: this.$t('PleaseSelectToTime'),
                        trigger: 'change',
                    },
                    {
                        trigger: 'change',
                        message: this.$t('FromTimeCannotLargerThanToTime'),
                        validator: (rule, value: string, callback) => {
                            const startValue = new Date();
                            // const startTime = (this.formModel as any).StartTime;

                            const startTime = new Date(moment((this.formModel as any).StartTime).format('YYYY-MM-DD'));
                            startValue.setHours(startTime.getHours(),startTime.getMinutes(), startTime.getSeconds());
                            console.log("HRCustomerInfo ~ initFormRuless ~ startTime:", startValue)
                            
                            const endValue = new Date();
                            // const endTime = (this.formModel as any).EndTime;
                            const endTime = new Date(moment((this.formModel as any).EndTime).format('YYYY-MM-DD'));
                            
                            endValue.setHours(endTime.getHours(), endTime.getMinutes(), endTime.getSeconds());
                            console.log("HRCustomerInfo ~ initFormRuless ~ endTime:", endValue)

                            const start = startTime
                                ? Math.trunc(startValue.getTime() / 1000) : 0;
                            const end = endTime
                                ? Math.trunc(endValue.getTime() / 1000) : 0;
                            if (start > end) {
                                callback(new Error());
                            }else{
                                callback();
                            }
                        },
                    },
                ],
                ContactDepartment: [
                    {
                        required: true,
                        message: this.$t('PleaseInputContactDepartment'),
                        trigger: 'change'
                    },
                ],
                ContactPerson: [
                    {
                        required: true,
                        message: this.$t('PleaseInputContactPerson'),
                        trigger: 'change'
                    },
                ],
            };
        }
        else {
            this.formRules = {
                // EmployeeATID: [
                //     {
                //         required: true,
                //         message: this.$t('PleaseInputEmployeeATID'),
                //         trigger: 'change',
                //     },
                //     {
                //         trigger: 'change',
                //         message: this.$t('EmployeeATIDOnlyAcceptNumericCharacters'),
                //         validator: (rule, value: string, callback) => {
                //             if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
                //                 callback(new Error());
                //             }
                //             callback();
                //         },
                //     },
                // ],
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
                FullName: [
                    {
                        required: true,
                        message: this.$t('PleaseInputFullName'),
                        trigger: 'change',
                    },
                ],
                ToTime: [
                    {
                        trigger: 'change',
                        message: this.$t('ToTimeMustBeGreaterThanFromTime'),
                        validator: (rule, value: string, callback) => {
                            const item = Object.assign(this.formModel);
                            if (item.ToTime < item.FromTime) {
                                callback(new Error());
                            }
                            callback();
                        },
                    }
                ],
                DepartmentIndex: [
                    {
                        required: true,
                        message: this.$t('PleaseInputDepartment'),
                        trigger: 'change'
                    },
                ]
            };
        }

    }

    initFormRulesNotCustomer() {
        if (this.clientName == "Mondelez") {
            this.formRules = {
                EmployeeATID: [
                    {
                        required: true,
                        message: this.$t('PleaseInputUserID'),
                        trigger: 'change',
                    },
                    {
                        trigger: 'change',
                        message: this.$t('UserIDOnlyAcceptNumericCharacters'),
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
                FullName: [
                    {
                        required: true,
                        message: this.$t('PleaseInputFullName'),
                        trigger: 'change',
                    },
                ],
                FromTime: [
                    {
                        required: true,
                        message: this.$t('PleaseInputFullName'),
                        trigger: 'change',
                    }
                ],
                ToTime: [
                    {
                        trigger: 'change',
                        message: this.$t('ToTimeMustBeGreaterThanFromTime'),
                        validator: (rule, value: string, callback) => {
                            const item = Object.assign(this.formModel);
                            if (item.ToTime < item.FromTime) {
                                callback(new Error());
                            }
                            callback();
                        },
                    }
                ],
                DepartmentIndex: [
                    {
                        required: true,
                        message: this.$t('PleaseInputDepartment'),
                        trigger: 'blur'
                    },
                ],
                BirthDay: [
                    {
                        required: true,
                        message: this.$t('PleaseInputBirthDay'),
                        trigger: 'change',
                    },
                ]
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
                FullName: [
                    {
                        required: true,
                        message: this.$t('PleaseInputFullName'),
                        trigger: 'change',
                    },
                ]
            };
        }

        if (this.idEnum == 4) {
            (this.formRules as any).StudentOfParent = [
                {
                    required: true,
                    message: this.$t('PleaseSelectStudent'),
                    trigger: 'change',
                },
                {
                    trigger: 'change',
                    message: this.$t('PleaseSelectStudent'),
                    validator: (rule, value: string, callback) => {
                        if (!this.formModel.StudentOfParent || this.formModel.StudentOfParent.length == 0) {
                            callback(new Error());
                        }
                        callback();
                    },
                },
            ]
        }
        else {
            delete (this.formRules as any).StudentOfParent;
        }
    }

    async loadData() {
        await this.initPositionInfoLookup();
        this.dataSource = [];
        this.isLoading = true;
        this.fileName = '';
        if(this.idEnum == 4 || this.idEnum == 6){
            await hrCustomerInfoApi.GetCustomerAtPageAdvance(this.filterModel.SelectedEmployee, 
                this.filterModel.TextboxSearch, this.page, this.pageSize, this.idEnum, this.filterStudentOfParent,
                    this.filterDepartments).then(response => {
                const { data, total } = response.data;
                this.dataSource = data.map((emp, idx) => ({
                    ...emp,
                    index: idx + 1 + (this.page - 1) * this.pageSize,
                    GenderTitle: this.$t(`${emp.Gender == 0 ? 'Female' : ''}${emp.Gender == 1 ? 'Male' : ''}${emp.Gender == 2 ? 'Other' : ''}`).toString(),
                    EmployeeTypeIndex: (!emp.EmployeeTypeIndex || (emp.EmployeeTypeIndex && emp.EmployeeTypeIndex == 0)) ? null : emp.EmployeeTypeIndex,
                    PositionIndex: (!emp.PositionIndex || emp.PositionIndex == 0) ? null : emp.PositionIndex
                }));
                this.isLoading = false;
                // this.dataSource = data;
                this.total = total;
            }).catch(err => {
                this.isLoading = false;
            })
        }else{
            await hrCustomerInfoApi.GetCustomerAtPage(this.filterModel.SelectedEmployee, this.filterModel.TextboxSearch, this.page, this.pageSize, this.idEnum).then(response => {
                const { data, total } = response.data;
                this.dataSource = data.map((emp, idx) => ({
                    ...emp,
                    index: idx + 1 + (this.page - 1) * this.pageSize,
                    GenderTitle: this.$t(`${emp.Gender == 0 ? 'Female' : ''}${emp.Gender == 1 ? 'Male' : ''}${emp.Gender == 2 ? 'Other' : ''}`).toString(),
                    EmployeeTypeIndex: (!emp.EmployeeTypeIndex || (emp.EmployeeTypeIndex && emp.EmployeeTypeIndex == 0)) ? null : emp.EmployeeTypeIndex,
                    PositionIndex: (!emp.PositionIndex || emp.PositionIndex == 0) ? null : emp.PositionIndex
                }));
                this.isLoading = false;
                // this.dataSource = data;
                this.total = total;
            }).catch(err => {
                this.isLoading = false;
            })
        }
    }

    async doDelete() {
        const selectedEmp = this.selectedRows.map(e => e.EmployeeATID);
        await hrCustomerInfoApi
            .DeleteCustomerMulti(selectedEmp)
            .then((res) => {
                this.loadData();
                this.getAllCustomer();
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
        (this.$refs.customerInfoPagination as any).page = this.page;
        (this.$refs.customerInfoPagination as any).lPage = this.page;
        await this.loadData();
    }

    onChangeDepartmentForm(){
        (this.$refs.customerFormModel as any).clearValidate('DepartmentIndex');
    }
    async onSubmitClick() {
        (this.$refs.customerFormModel as any).validate(async (valid) => {
            if (!valid) return;
            (this.formModel as any).FromTime = new Date(moment((this.formModel as any).FromTime).format('YYYY-MM-DD'));
            (this.formModel as any).ToTime = (this.formModel as any).ToTime ? new Date(moment((this.formModel as any).ToTime).format('YYYY-MM-DD')) : null;
            //console.log(`this.formModel`, this.formModel)
           
            if (this.clientName == "Mondelez") {
                const dateNow = new Date();
                (this.formModel as any).BirthDay = new Date(moment((this.formModel as any).BirthDay).format('YYYY-MM-DD'));
                const birthDay = new Date(moment((this.formModel as any).BirthDay).format('YYYY-MM-DD'));
                const ageDifferenceInMilliseconds = dateNow.getTime() - birthDay.getTime();
                const ageInYears = Math.floor(ageDifferenceInMilliseconds / (1000 * 60 * 60 * 24 * 365));
                if (ageInYears < 18) {
                    this.$alertSaveError(null, null, null, this.$t('EmployeesAreUnder18YearsOld').toString()).toString();
                    return;
                }
                // (this.formModel as any).StartTime = new Date(moment((this.formModel as any).StartTime).format("YYYY-MM-DD HH:mm:ss"));
                // (this.formModel as any).EndTime = new Date(moment((this.formModel as any).EndTime).format("YYYY-MM-DD HH:mm:ss"));
                // let firstChar =  (this.formModel as any).Nric.charAt(0);
                // if (!isNaN(firstChar) && (this.formModel as any).Nric.length != 12) {
                //     this.$alertSaveError(null, null, null, this.$t('CCCDExactly12Characters').toString()).toString();
                //     return;
                //   }
            }
            this.formModel.StartTimeStr = moment(this.formModel.StartTime).format("YYYY-MM-DD HH:mm:ss");
            this.formModel.EndTimeStr = moment(this.formModel.EndTime).format("YYYY-MM-DD HH:mm:ss");
            (this.formModel as any).ToTime = (this.formModel as any).ToTime ? new Date(moment((this.formModel as any).ToTime).format('YYYY-MM-DD')) : null;
            if ((this.formModel as any).FromTime && (this.formModel as any).ToTime && (this.formModel as any).ToTime < (this.formModel as any).FromTime) {
                this.$alertSaveError(null, null, null, this.$t('StartDateCannotLargerThanEndDate').toString()).toString();
                return;
            }

            const dateNow = new Date();
            if ((this.formModel as any).FromTime == null &&  (this.formModel as any).ToTime!= null &&  (this.formModel as any).ToTime <= dateNow) {
                this.$alertSaveError(null, null, null, this.$t('EndDateCannotBeLessThanTheCurrentDate').toString()).toString();
                return;
            }

            const atid = (this.formModel as any).EmployeeATID;
            const formData = Misc.cloneData(this.formModel);
            formData.PositionIndex = !(this.formModel as any).PositionIndex ? 0 : (this.formModel as any).PositionIndex;
            if (this.isEdit) {
                
                await hrCustomerInfoApi.UpdateCustomer(atid, formData, this.idEnum).then(() => {
                    this.loadData();
                    this.getAllCustomer();
                    this.$saveSuccess();
                    this.filterCustomerID = null;
                    this.showDialog = false;
                });
            }
            else {
                await hrCustomerInfoApi.AddCustomer(formData, this.idEnum).then(() => {
                    this.loadData();
                    this.getAllCustomer();
                    this.$saveSuccess();
                    this.filterCustomerID = null;
                    this.showDialog = false;
                });
            }
        })
    }


    onCancelClick() {
        (this.$refs.customerFormModel as any).resetFields();
        this.formModel = {ContactDepartment: null};
        this.fileList = [];
        this.showDialog = false;
        this.selectedRows = [];
        this.filterCustomerID = null;
        this.loadData();
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
            .catch(e => console.log(e));
    }

    onRemoveAvatar(file, fileList) {
        Object.assign(this.formModel, { ImageUpload: '', Avatar: null });
        this.errorUpload = false;
        this.$forceUpdate();
    }


    async onChangeIdentityImage(file, fileListIdentityImage) {
        const originCountFileList = fileListIdentityImage.length;
        if (fileListIdentityImage.length > 1) {
            fileListIdentityImage.splice(1, fileListIdentityImage.length);
        }
        const fileRaw = file.raw;
        const isJPG = fileRaw.type === 'image/jpeg';
        const isLt2M = fileRaw.size / 1024 / 1024 < 2;

        if (!isJPG) {
            this.$message.error('Identity image must be JPG format!');
            if (originCountFileList == 1) fileListIdentityImage.splice(0, 1);
            return;
        }
        if (!isLt2M) {
            this.$message.error('Identity image size can not exceed 2MB!');
            if (originCountFileList == 1) fileListIdentityImage.splice(0, 1);
            return;
        }

        fileListIdentityImage[0] = file;

        this.fileIdentityImageName = fileRaw.name;
        await this.getArrayBuffer(fileRaw).then(
            data => {
                Object.assign(this.formModel, { IdentityImage: Misc.arrayBufferToBase64(data) });
                this.errorUpload = false;
                this.$forceUpdate();
            }

        )
            .catch(e => console.log(e));
    }

    onRemoveIdentityImage(file, fileList) {
        Object.assign(this.formModel, { IdentityImage: null });
        this.errorUpload = false;
        this.$forceUpdate();
    }
    //#endregion

    //#region Register Finger print

    // Finger pRINT
    showOrHideRegisterFingerDialog() {
        this.showFingerDialog = true;
        this.resetFingerParam();
        this.websOpen();
    }

    cancelRegisterFingerDialog() {
        this.showFingerDialog = false;
        this.websClose();
        this.listFinger = [];
        this.setFingers();
    }

    submitRegisterFinger() {
        const lstFinger = this.listFinger.map(e => (e.Template));
        Object.assign(this.formModel, { ListFinger: lstFinger });
        this.showFingerDialog = false;
    }

    async getEmployeeFinger(employeeATID) {
        await employeeInfoApi.GetEmployeeFinger(employeeATID).then(res => {
            const { data } = res as any;
            for (var i = 0; i < data.length; i++) {
                if (!Misc.isEmpty(data[i]) && data[i].length > 0) {
                    this.listFinger[i].ImageFinger = this.getImgUrl('fingerprint.png');
                }
            }
        });
    }

    setFingers() {
        for (var i = 1; i <= 10; i++) {
            this.listFinger.push({ FocusFinger: false, ID: i, Template: "", ImageFinger: "" });
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

    onError(event) {
        console.log(event.data);
    }

    onMessage(event) {
        if (event.data != undefined) {
            var jsonData = JSON.parse(event.data);
            if (jsonData.datatype === "image") {
                var tempImg = jsonData.data.jpg_base64
                if (tempImg == undefined || tempImg == '') {
                    return false
                }
                var strImgData = "data:image/jpg;base64,";
                strImgData += tempImg;
                this.listFinger[this.currentIndex].ImageFinger = strImgData;
            }
            if (jsonData.datatype === "template") {

                this.listFinger[this.currentIndex].Template = jsonData.data.template;
            }
            else {
                if (jsonData.ret == 0 && jsonData.function == "open") {
                    this.ConnectedDevice = true;
                    this.DeviceInfo = this.$t('ConnectedFingerDevice').toString();
                } else if (jsonData.ret == -10007) {
                    this.ConnectedDevice = false;
                    this.DeviceInfo = this.$t('NotConnectedDevice').toString();
                } else if (jsonData.ret < 0) {
                    this.DeviceInfo = this.$t('NotConnectedDevice').toString();
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

        this.websocket.onmessage = (event) => {
            this.onMessage(event);
        }

        this.websocket.onerror = (event) => {
            this.onError(event);
        }
    }

    reconnect() {
        this.resetFingerParam();
        this.closeDev();
        this.openDev();
    }

    websClose() {
        if (!Misc.isEmpty(this.websocket))
            this.websocket.close();
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
        console.log(this.template1);
        console.log(this.template2);
        var str = "{\"module\":\"fingerprint\",\"function\":\"verify\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":{" + "\"template1\":\"" + this.template1 + "\",\"template2\":\"" + this.template2 + "\"}}"
        this.doSend(str);
    }

    openDev() {

        var str = "{\"module\":\"fingerprint\",\"function\":\"open\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}"
        this.doSend(str);
    }

    closeDev() {
        if (this.websocket.readyState != WebSocket.CLOSED && this.websocket.readyState != WebSocket.CLOSING) {
            this.doSend("{\"module\":\"fingerprint\",\"function\":\"close\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}");
        }
    }

    //#endregion


    //#region Import excel
    showImportExcel = true;
    isAddFromExcel = false;
    isDeleteFromExcel = false;
    addedParams = [];
    formExcel = {};
    dataAddExcel = [];
    fileName = '';
    showDialogExcel = false;
    dataProcessedExcel = [];
    listExcelFunction = ['AddExcel', 'ExportExcel'];
    UploadDataFromExcel() {
        var arrData = [];
        var regex = /^\d+$/;
        console.log(this.dataAddExcel[0]);
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});
            // if (regex.test(this.dataAddExcel[0][i]['Mã chấm công (*)']) === false) {
            //     this.$alertSaveError(null, null, null, this.$t('EmployeeATIDOnlyAcceptNumericCharacters').toString()).toString();
            //     return;
            // } else 
            if (this.clientName == "Mondelez") {
                if (this.idEnum == 6) {
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
                    } else if (this.dataAddExcel[0][i]['Giới tính'] == 'Nữ') {
                        a.Gender = 0;
                    } else {
                        a.Gender = 2;
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Ngày sinh (ngày/tháng/năm) (*)')) {
                        a.DateOfBirth = this.dataAddExcel[0][i]['Ngày sinh (ngày/tháng/năm) (*)'] + '';
                    } else {
                        a.DateOfBirth = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('CMND/CCCD/Passport (*)')) {
                        a.Nric = this.dataAddExcel[0][i]['CMND/CCCD/Passport (*)'] + '';
                    } else {
                        a.Nric = '';
                    }


                    if (this.dataAddExcel[0][i].hasOwnProperty('Tên công ty')) {
                        a.Company = this.dataAddExcel[0][i]['Tên công ty'] + '';
                    } else {
                        a.Company = '';
                    }


                    if (this.dataAddExcel[0][i].hasOwnProperty('Số điện thoại')) {
                        a.PhoneNumber = this.dataAddExcel[0][i]['Số điện thoại'] + '';
                    } else {
                        a.PhoneNumber = '';
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
                        a.Department = this.dataAddExcel[0][i]['Phòng ban (*)'] + '';
                    } else {
                        a.Department = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Chức vụ')) {
                        a.PositionName = this.dataAddExcel[0][i]['Chức vụ'] + '';
                    } else {
                        a.PositionName = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban liên hệ (*)')) {
                        a.ContactDepartment = this.dataAddExcel[0][i]['Phòng ban liên hệ (*)'] + '';
                    } else {
                        a.ContactDepartment = '';
                    }
                    if (this.dataAddExcel[0][i].hasOwnProperty('Người liên hệ (*)')) {
                        a.ContactPerson = this.dataAddExcel[0][i]['Người liên hệ (*)'] + '';
                    } else {
                        a.ContactPerson = '';
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

                    if (this.dataAddExcel[0][i].hasOwnProperty('Từ giờ (*)')) {
                        a.StartTime = this.dataAddExcel[0][i]['Từ giờ (*)'] + '';
                    } else {
                        a.StartTime = '';
                    }
                    if (this.dataAddExcel[0][i].hasOwnProperty('Đến giờ (*)')) {
                        a.EndTime = this.dataAddExcel[0][i]['Đến giờ (*)'] + '';
                    } else {
                        a.EndTime = '';
                    }

                    if (this.dataAddExcel[0][i]['Sử dụng điện thoại'] == 'x') {
                        a.IsAllowPhone = 1;
                    } else {
                        a.IsAllowPhone = 0;
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


                    if (this.dataAddExcel[0][i].hasOwnProperty('Nội dung làm việc')) {
                        a.WorkingContent = this.dataAddExcel[0][i]['Nội dung làm việc'] + '';
                    } else {
                        a.WorkingContent = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Ghi chú')) {
                        a.Note = this.dataAddExcel[0][i]['Ghi chú'] + '';
                    } else {
                        a.Note = '';
                    }
                }
                else {
                    if(this.idEnum == 2){
                        if (this.dataAddExcel[0][i].hasOwnProperty('Mã người dùng')) {
                            a.EmployeeATID = this.dataAddExcel[0][i]['Mã người dùng'] + '';
                        } else {
                            // this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                            // return;
                            a.EmployeeATID = '';
                        }
                    }else{
                        if (this.dataAddExcel[0][i].hasOwnProperty('Mã người dùng (*)')) {
                            a.EmployeeATID = this.dataAddExcel[0][i]['Mã người dùng (*)'] + '';
                        } else {
                            // this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                            // return;
                            a.EmployeeATID = '';
                        }
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
                    } else if (this.dataAddExcel[0][i]['Giới tính'] == 'Nữ') {
                        a.Gender = 0;
                    } else {
                        a.Gender = 2;
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Ngày sinh (ngày/tháng/năm) (*)')) {
                        a.DateOfBirth = this.dataAddExcel[0][i]['Ngày sinh (ngày/tháng/năm) (*)'] + '';
                    } else {
                        a.DateOfBirth = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('CMND/CCCD/Passport (*)')) {
                        a.Nric = this.dataAddExcel[0][i]['CMND/CCCD/Passport (*)'] + '';
                    } else {
                        a.Nric = '';
                    }


                    if (this.dataAddExcel[0][i].hasOwnProperty('Tên công ty')) {
                        a.Company = this.dataAddExcel[0][i]['Tên công ty'] + '';
                    } else {
                        a.Company = '';
                    }


                    if (this.dataAddExcel[0][i].hasOwnProperty('Số điện thoại')) {
                        a.PhoneNumber = this.dataAddExcel[0][i]['Số điện thoại'] + '';
                    } else {
                        a.PhoneNumber = '';
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
                        a.Department = this.dataAddExcel[0][i]['Phòng ban (*)'] + '';
                    } else {
                        a.Department = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Chức vụ')) {
                        a.PositionName = this.dataAddExcel[0][i]['Chức vụ'] + '';
                    } else {
                        a.PositionName = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban liên hệ (*)')) {
                        a.ContactDepartment = this.dataAddExcel[0][i]['Phòng ban liên hệ (*)'] + '';
                    } else {
                        a.ContactDepartment = '';
                    }
                    if (this.dataAddExcel[0][i].hasOwnProperty('Người liên hệ (*)')) {
                        a.ContactPerson = this.dataAddExcel[0][i]['Người liên hệ (*)'] + '';
                    } else {
                        a.ContactPerson = '';
                    }
                    if (this.idEnum == 2) {
                        if (this.dataAddExcel[0][i].hasOwnProperty('Từ ngày (*)')) {
                            a.JoinedDate = this.dataAddExcel[0][i]['Từ ngày (*)'] + '';
                        } else {
                            a.JoinedDate = '';
                        }
                        if (this.dataAddExcel[0][i].hasOwnProperty('Đến ngày (*)')) {
                            a.StoppedDate = this.dataAddExcel[0][i]['Đến ngày (*)'] + '';
                        } else {
                            a.StoppedDate = '';
                        }
                    }
                    else {
                        if (this.dataAddExcel[0][i].hasOwnProperty('Ngày bắt đầu')) {
                            a.JoinedDate = this.dataAddExcel[0][i]['Ngày bắt đầu'] + '';
                        } else {
                            a.JoinedDate = '';
                        }

                        if (this.dataAddExcel[0][i].hasOwnProperty('Ngày kết thúc')) {
                            a.StoppedDate = this.dataAddExcel[0][i]['Ngày kết thúc'] + '';
                        } else {
                            a.StoppedDate = '';
                        }
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Từ giờ (*)')) {
                        a.StartTime = this.dataAddExcel[0][i]['Từ giờ (*)'] + '';
                    } else {
                        a.StartTime = '';
                    }
                    if (this.dataAddExcel[0][i].hasOwnProperty('Đến giờ (*)')) {
                        a.EndTime = this.dataAddExcel[0][i]['Đến giờ (*)'] + '';
                    } else {
                        a.EndTime = '';
                    }

                    if (this.dataAddExcel[0][i]['Sử dụng điện thoại'] == 'x') {
                        a.IsAllowPhone = 1;
                    } else {
                        a.IsAllowPhone = 0;
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


                    if (this.dataAddExcel[0][i].hasOwnProperty('Nội dung làm việc')) {
                        a.WorkingContent = this.dataAddExcel[0][i]['Nội dung làm việc'] + '';
                    } else {
                        a.WorkingContent = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Ghi chú')) {
                        a.Note = this.dataAddExcel[0][i]['Ghi chú'] + '';
                    } else {
                        a.Note = '';
                    }
                }
            } else {
                if (this.idEnum == 6) {
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
                    } else if (this.dataAddExcel[0][i]['Giới tính'] == 'Nữ') {
                        a.Gender = 0;
                    } else {
                        a.Gender = 2;
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Ngày sinh (ngày/tháng/năm)')) {
                        a.DateOfBirth = this.dataAddExcel[0][i]['Ngày sinh (ngày/tháng/năm)'] + '';
                    } else {
                        a.DateOfBirth = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('CMND/CCCD/Passport')) {
                        a.Nric = this.dataAddExcel[0][i]['CMND/CCCD/Passport'] + '';
                    } else {
                        a.Nric = '';
                    }


                    if (this.dataAddExcel[0][i].hasOwnProperty('Tên công ty')) {
                        a.Company = this.dataAddExcel[0][i]['Tên công ty'] + '';
                    } else {
                        a.Company = '';
                    }


                    if (this.dataAddExcel[0][i].hasOwnProperty('Số điện thoại')) {
                        a.PhoneNumber = this.dataAddExcel[0][i]['Số điện thoại'] + '';
                    } else {
                        a.PhoneNumber = '';
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
                        a.Department = this.dataAddExcel[0][i]['Phòng ban (*)'] + '';
                    } else {
                        a.Department = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Chức vụ')) {
                        a.PositionName = this.dataAddExcel[0][i]['Chức vụ'] + '';
                    } else {
                        a.PositionName = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban liên hệ (*)')) {
                        a.ContactDepartment = this.dataAddExcel[0][i]['Phòng ban liên hệ (*)'] + '';
                    } else {
                        a.ContactDepartment = '';
                    }
                    if (this.dataAddExcel[0][i].hasOwnProperty('Người liên hệ (*)')) {
                        a.ContactPerson = this.dataAddExcel[0][i]['Người liên hệ (*)'] + '';
                    } else {
                        a.ContactPerson = '';
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

                    if (this.dataAddExcel[0][i].hasOwnProperty('Từ giờ (*)')) {
                        a.StartTime = this.dataAddExcel[0][i]['Từ giờ (*)'] + '';
                    } else {
                        a.StartTime = '';
                    }
                    if (this.dataAddExcel[0][i].hasOwnProperty('Đến giờ (*)')) {
                        a.EndTime = this.dataAddExcel[0][i]['Đến giờ (*)'] + '';
                    } else {
                        a.EndTime = '';
                    }

                    if (this.dataAddExcel[0][i]['Sử dụng điện thoại'] == 'x') {
                        a.IsAllowPhone = 1;
                    } else {
                        a.IsAllowPhone = 0;
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


                    if (this.dataAddExcel[0][i].hasOwnProperty('Nội dung làm việc')) {
                        a.WorkingContent = this.dataAddExcel[0][i]['Nội dung làm việc'] + '';
                    } else {
                        a.WorkingContent = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Ghi chú')) {
                        a.Note = this.dataAddExcel[0][i]['Ghi chú'] + '';
                    } else {
                        a.Note = '';
                    }
                }
                else {
                    if(this.idEnum == 2){
                        if (this.dataAddExcel[0][i].hasOwnProperty('Mã người dùng')) {
                            a.EmployeeATID = this.dataAddExcel[0][i]['Mã người dùng'] + '';
                        } else {
                            // this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                            // return;
                            a.EmployeeATID = '';
                        }
                    }else{
                        if (this.dataAddExcel[0][i].hasOwnProperty('Mã người dùng (*)')) {
                            a.EmployeeATID = this.dataAddExcel[0][i]['Mã người dùng (*)'] + '';
                        } else {
                            // this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                            // return;
                            a.EmployeeATID = '';
                        }
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
                    } else if (this.dataAddExcel[0][i]['Giới tính'] == 'Nữ') {
                        a.Gender = 0;
                    } else {
                        a.Gender = 2;
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Ngày sinh (ngày/tháng/năm)')) {
                        a.DateOfBirth = this.dataAddExcel[0][i]['Ngày sinh (ngày/tháng/năm)'] + '';
                    } else {
                        a.DateOfBirth = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('CMND/CCCD/Passport')) {
                        a.Nric = this.dataAddExcel[0][i]['CMND/CCCD/Passport'] + '';
                    } else {
                        a.Nric = '';
                    }


                    if (this.dataAddExcel[0][i].hasOwnProperty('Tên công ty')) {
                        a.Company = this.dataAddExcel[0][i]['Tên công ty'] + '';
                    } else {
                        a.Company = '';
                    }


                    if (this.dataAddExcel[0][i].hasOwnProperty('Số điện thoại')) {
                        a.PhoneNumber = this.dataAddExcel[0][i]['Số điện thoại'] + '';
                    } else {
                        a.PhoneNumber = '';
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
                        a.Department = this.dataAddExcel[0][i]['Phòng ban (*)'] + '';
                    } else {
                        a.Department = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Chức vụ')) {
                        a.PositionName = this.dataAddExcel[0][i]['Chức vụ'] + '';
                    } else {
                        a.PositionName = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban liên hệ (*)')) {
                        a.ContactDepartment = this.dataAddExcel[0][i]['Phòng ban liên hệ (*)'] + '';
                    } else {
                        a.ContactDepartment = '';
                    }
                    if (this.dataAddExcel[0][i].hasOwnProperty('Người liên hệ (*)')) {
                        a.ContactPerson = this.dataAddExcel[0][i]['Người liên hệ (*)'] + '';
                    } else {
                        a.ContactPerson = '';
                    }
                    if (this.idEnum == 2) {
                        if (this.dataAddExcel[0][i].hasOwnProperty('Từ ngày')) {
                            a.JoinedDate = this.dataAddExcel[0][i]['Từ ngày'] + '';
                        } else {
                            a.JoinedDate = '';
                        }
                        if (this.dataAddExcel[0][i].hasOwnProperty('Đến ngày')) {
                            a.StoppedDate = this.dataAddExcel[0][i]['Đến ngày'] + '';
                        } else {
                            a.StoppedDate = '';
                        }
                    }
                    else {
                        if (this.dataAddExcel[0][i].hasOwnProperty('Ngày bắt đầu')) {
                            a.JoinedDate = this.dataAddExcel[0][i]['Ngày bắt đầu'] + '';
                        } else {
                            a.JoinedDate = '';
                        }

                        if (this.dataAddExcel[0][i].hasOwnProperty('Ngày kết thúc')) {
                            a.StoppedDate = this.dataAddExcel[0][i]['Ngày kết thúc'] + '';
                        } else {
                            a.StoppedDate = '';
                        }
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Từ giờ')) {
                        a.StartTime = this.dataAddExcel[0][i]['Từ giờ'] + '';
                    } else {
                        a.StartTime = '';
                    }
                    if (this.dataAddExcel[0][i].hasOwnProperty('Đến giờ')) {
                        a.EndTime = this.dataAddExcel[0][i]['Đến giờ'] + '';
                    } else {
                        a.EndTime = '';
                    }

                    if (this.dataAddExcel[0][i]['Sử dụng điện thoại'] == 'x') {
                        a.IsAllowPhone = 1;
                    } else {
                        a.IsAllowPhone = 0;
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


                    if (this.dataAddExcel[0][i].hasOwnProperty('Nội dung làm việc')) {
                        a.WorkingContent = this.dataAddExcel[0][i]['Nội dung làm việc'] + '';
                    } else {
                        a.WorkingContent = '';
                    }

                    if (this.dataAddExcel[0][i].hasOwnProperty('Ghi chú')) {
                        a.Note = this.dataAddExcel[0][i]['Ghi chú'] + '';
                    } else {
                        a.Note = '';
                    }
                }
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Học sinh (*)')) {
                a.StudentOfParent = this.dataAddExcel[0][i]['Học sinh (*)'] + '';
            } else {
                a.StudentOfParent = '';
            }

            arrData.push(a);
        }
        hrCustomerInfoApi.AddCustomerFromExcel(arrData, this.idEnum).then((res) => {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload' + this.idEnum)))) {
                (<HTMLInputElement>document.getElementById('fileUpload' + this.idEnum)).value = '';
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
                this.getAllCustomer();
            } else {
                if(this.idEnum == 6){
                    this.importErrorMessage = this.$t('ImportContractorErrorMessage') + res.data.toString() + " " + this.$t('Contractor');
                } else if(this.idEnum == 2){
                    this.importErrorMessage = this.$t('ImportGuestErrorMessage') + res.data.toString() + " " + this.$t('Customer');
                }else{
                    this.importErrorMessage = this.$t('ImportEmployeeErrorMessage') + res.data.toString() + " " + this.$t('Employee');

                }
                this.showOrHideImportError(true);
            }
        });


    }
    

    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
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
        hrCustomerInfoApi.DeleteCustomerFromExcel(this.addedParams, this.idEnum).then((res) => {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload' + this.idEnum)))) {
                (<HTMLInputElement>document.getElementById('fileUpload' + this.idEnum)).value = '';
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
    async ExportToExcel() {
        this.addedParams = [];
        if (this.selectedRows.length > 0) {
            this.addedParams.push({ Key: "ListEmployeeATID", Value: this.selectedRows.map(x => x.EmployeeATID) });
        }
        // this.addedParams.push({ Key: "DepartmentIndex", Value: this.filterDepartmentId });
        this.addedParams.push({ Key: "Filter", Value: this.filterModel.TextboxSearch });
        await employeeInfoApi.ExportToExcel1(this.addedParams, this.idEnum).then((res: any) => {
            const fileURL = window.URL.createObjectURL(new Blob([res.data]));
            const fileLink = document.createElement("a");

            fileLink.href = fileURL;
            if(this.idEnum == 6){
                fileLink.setAttribute("download", `Contractor${moment(new Date).format('YYYY-MM-DD HH:mm')}.xlsx`);

            }else{
                fileLink.setAttribute("download", `Employee_${moment(new Date).format('YYYY-MM-DD HH:mm')}.xlsx`);

            }
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
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload' + this.idEnum)))) {
                (<HTMLInputElement>document.getElementById('fileUpload' + this.idEnum)).value = '';
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
            (<HTMLInputElement>document.getElementById('fileUpload' + this.idEnum)).value = '';
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
                    console.log(arrData);
                };
                console.log(arrData);
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
    //#endregion

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
                this.formModel.NRIC = qrData[0].toString();
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
