import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { departmentApi } from '@/$api/department-api';
import { hrUserApi } from '@/$api/hr-user-api';
import { hrCustomerInfoApi } from '@/$api/hr-customer-info-api';
import { isNullOrUndefined } from 'util';
import * as XLSX from 'xlsx';
import { parkingLotsApi } from "@/$api/gc-parking-lot-api";
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { EmployeeVehicleRequest, employeeVehicleApi } from "@/$api/gc-employee-vehicle-api";
import { CustomerVehicleRequest, customerVehicleApi } from "@/$api/gc-customer-vehicle-api";
import { commandApi } from "@/$api/command-api";

@Component({
    name: "employee-vehicle",
    components: { HeaderComponent, DataTableFunctionComponent, DataTableComponent, SelectTreeComponent, SelectDepartmentTreeComponent }
})
export default class EmployeeVehicle extends Mixins(ComponentBase) {
    columns = [];
    rowsObj = [];
    isEdit = false;
    showDialog = false;
    rules = null;

    filterAccessType = [1];
    filterParkingLotIndex: any = [];
    filterFromDate: any = new Date();
    filterToDate: any = null;
    filterFormDepartmentIndex: any = [];
    filterFormEmployeeATID: any = [];

    filterModel = { ListEmployeeATID: [], TextboxSearch: '', FromDate: new Date(), ToDate: new Date() };
    filterDepartment = [];

    listAccessType = [
        {Index: 1, Name: this.$t('Employee')},
        {Index: 2, Name: this.$t('Customer')}
    ]
    vehicleType = [
		{ Name: this.$t('MotorBike'), Index: 0 },
		{ Name: this.$t('Bicycle'), Index: 1 },
		{ Name: this.$t('ElectricBicycle'), Index: 2 },
		{ Name: this.$t('Car'), Index: 3 },
	];
    vehicleStatusType = [
		{ Name: this.$t('FixedVehicle'), Index: 0 },
		{ Name: this.$t('NotFixedVehicle'), Index: 1 },
	];
    listAllEmployee: any = [];
    listAllEmployeeFilter: any = [];
    listAllEmployeeForm: any = [];

    listAllCustomer: any = [];
    listAllCustomerFilter: any = [];
    listAllCustomerForm: any = [];

    formExcel = {};
    fileName = '';
    importErrorMessage = '';
    dataProcessedExcel = [];
    dataAddExcel = [];
    listExcelFunction = ['AddExcel'];
    showDialogExcel = false;

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

    showDialogImportError = false;
    isAddFromExcel = false;
    isDeleteFromExcel = false;

    employeeVehicleModel: EmployeeVehicleRequest = {
        EmployeeATID: "",
        EmployeeATIDs: [],
        Type: 0,
        StatusType: 0,
        Plate: "",
        Branch: "",
        Color: "",
        Description: '',
    };
    customerVehicleModel: CustomerVehicleRequest = {
        EmployeeATID: "",
        EmployeeATIDs: [],
        Type: 0,
        StatusType: 0,
        Plate: "",
        Branch: "",
        Color: "",
        Description: '',
    };
    listParkingLot = [];

    page = 1;
    filter = '';

    activeTab = "employeeVehicle";

    masterEmployeeFilter = [];

    handleClick() {
        this.page = 1;
        this.CreateRules();
        this.Reset();
        this.ResetCustomer();
        this.resetColumns();
        this.filterModel = { ListEmployeeATID: [], TextboxSearch: '', FromDate: new Date(), ToDate: new Date() };
        this.filterDepartment = [];
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
            if(this.activeTab == "employeeVehicle"){
                this.filterModel.ListEmployeeATID = Misc.cloneData(this.masterEmployeeFilter)
				    .filter(x => (this.listAllEmployeeFilter as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
            }else{
                this.filterModel.ListEmployeeATID = Misc.cloneData(this.masterEmployeeFilter)
                    .filter(x => (this.listAllCustomerFilter as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
            }
		}
    }

    async beforeMount() {
        this.CreateRules();
        this.CreateColumns();
        await this.getAllEmployee();
        await this.getAllCustomer();
        await this.getDepartmentTree();
        this.Reset();
        const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
            if(this.activeTab == "employeeVehicle"){
                this.filterModel.ListEmployeeATID = Misc.cloneData(this.masterEmployeeFilter)
				    .filter(x => (this.listAllEmployeeFilter as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
            }else{
                this.filterModel.ListEmployeeATID = Misc.cloneData(this.masterEmployeeFilter)
                    .filter(x => (this.listAllCustomerFilter as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
            }
		}
    }

    CreateRules() {
        if(this.activeTab == "employeeVehicle"){
            this.rules = {
                ParkingLotIndex: [
                    {
                        required: true,
                        message: this.$t('PleaseInputParkingLot'),
                        trigger: 'blur',
                    },
                ],
                Plate: [
                    // {
                    //     required: true,
                    //     message: this.$t('PleaseInputPlate'),
                    //     trigger: 'blur',
                    // },
                    {
                        trigger: 'change',
                        validator: (rule, value: string, callback) => {
                            if (this.employeeVehicleModel.StatusType == 0 && (!this.employeeVehicleModel.Plate 
                                || this.employeeVehicleModel.Plate == "")) {
                                callback(new Error(this.$t('PleaseInputPlate').toString()));
                            }
                            callback();
                        },
                    },
                ],
                Type: [
                    {
                        required: true,
                        message: this.$t('PleaseInputVehicleType'),
                        trigger: 'blur',
                    },
                ],
                EmployeeATIDs: [
                    {
                        required: true,
                        message: this.$t('PleaseSelectEmployee'),
                        trigger: 'blur',
                    },
                    {
                        trigger: 'blur',
                        message: this.$t('PleaseSelectEmployee'),
                        validator: (rule, value: string, callback) => {
                            if (!value || (value && value.length < 1)) {
                                // console.log("")
                                callback(new Error());
                            }
                            callback();
                        },
                    },
                ],
            }
        }else{
            this.rules = {
                ParkingLotIndex: [
                    {
                        required: true,
                        message: this.$t('PleaseInputParkingLot'),
                        trigger: 'blur',
                    },
                ],
                // Plate: [
                //     // {
                //     //     required: true,
                //     //     message: this.$t('PleaseInputPlate'),
                //     //     trigger: 'blur',
                //     // },
                //     {
                //         trigger: 'change',
                //         validator: (rule, value: string, callback) => {
                //             if (this.employeeVehicleModel.StatusType == 0 && (!this.employeeVehicleModel.Plate 
                //                 || this.employeeVehicleModel.Plate == "")) {
                //                 callback(new Error(this.$t('PleaseInputPlate').toString()));
                //             }
                //             callback();
                //         },
                //     },
                // ],
                Type: [
                    {
                        required: true,
                        message: this.$t('PleaseInputVehicleType'),
                        trigger: 'blur',
                    },
                ],
                EmployeeATIDs: [
                    {
                        required: true,
                        message: this.$t('PleaseSelectCustomer'),
                        trigger: 'blur',
                    },
                    {
                        trigger: 'blur',
                        message: this.$t('PleaseSelectCustomer'),
                        validator: (rule, value: string, callback) => {
                            if (!value || (value && value.length < 1)) {
                                // console.log("")
                                callback(new Error());
                            }
                            callback();
                        },
                    },
                ],
            }
        }
    }
    CreateColumns() {
        if(this.activeTab == "employeeVehicle"){
            this.columns = [
                {
                    prop: 'EmployeeATID',
                    label: 'MCC',
                    minWidth: 150,
                    display: true
                },
                {
                    prop: 'EmployeeCode',
                    label: 'EmployeeCode',
                    minWidth: 150,
                    display: true
                },
                {
                    prop: 'FullName',
                    label: 'FullName',
                    minWidth: 150,
                    display: true
                },
                {
                    prop: 'DepartmentName',
                    label: 'Department',
                    minWidth: 150,
                    display: true
                },
                {
                    prop: 'Type',
                    label: 'VehicleType',
                    minWidth: 150,
                    display: true,
                    dataType: 'lookup',
                    lookup: {
                        dataSource: this.vehicleType,
                        displayMember: 'Name',
                        valueMember: 'Index',
                    },
                },
                {
                    prop: 'StatusType',
                    label: 'VehicleStatusType',
                    minWidth: 150,
                    display: true,
                    dataType: 'lookup',
                    lookup: {
                        dataSource: this.vehicleStatusType,
                        displayMember: 'Name',
                        valueMember: 'Index',
                    },
                },
                {
                    prop: 'Plate',
                    label: 'Plate',
                    minWidth: 200,
                    display: true
                },
                {
                    prop: 'Branch',
                    label: 'Branch',
                    minWidth: 200,
                    display: true
                },
                {
                    prop: 'Color',
                    label: 'Color',
                    minWidth: 150,
                    display: true
                }
            ];
        }else{
            this.columns = [
                {
                    prop: 'EmployeeATID',
                    label: 'CustomerCode',
                    minWidth: 150,
                    display: true
                },
                {
                    prop: 'FullName',
                    label: 'FullName',
                    minWidth: 150,
                    display: true
                },
                {
                    prop: 'Type',
                    label: 'VehicleType',
                    minWidth: 150,
                    display: true,
                    dataType: 'lookup',
                    lookup: {
                        dataSource: this.vehicleType,
                        displayMember: 'Name',
                        valueMember: 'Index',
                    },
                },
                {
                    prop: 'Plate',
                    label: 'Plate',
                    minWidth: 200,
                    display: true
                },
                {
                    prop: 'Branch',
                    label: 'Branch',
                    minWidth: 200,
                    display: true
                },
                {
                    prop: 'Color',
                    label: 'Color',
                    minWidth: 150,
                    display: true
                }
            ];
        }
    }

    resetColumns(){
        this.CreateColumns();
        setTimeout(() => {
            (this.$refs.employeeVehicleDataTableFunction as any).reloadConfig(this.activeTab);
        }, 500);
    }

    mounted(){
        (this.$refs.employeeVehicleDataTableFunction as any).reloadConfig(this.activeTab);
    }

    Insert() {
        this.showDialog = true;
        if (this.isEdit == true) {
            if(this.activeTab == "employeeVehicle"){
                this.Reset();
            }else{
                this.ResetCustomer();
            }
        }
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
            if(this.activeTab == "employeeVehicle"){
                this.employeeVehicleModel.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
				    .filter(x => (this.listAllEmployeeForm as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
            }else{
                this.customerVehicleModel.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
                    .filter(x => (this.listAllCustomerForm as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
            }
		}
        this.isEdit = false;

    }
    Edit() {
        this.isEdit = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (obj.length == 1) {
            this.showDialog = true;
            if(this.activeTab == "employeeVehicle"){
                this.employeeVehicleModel = obj[0];
                this.employeeVehicleModel.EmployeeATIDs = [this.employeeVehicleModel.EmployeeATID];
            }else{
                this.customerVehicleModel = obj[0];
                this.customerVehicleModel.EmployeeATIDs = [this.customerVehicleModel.EmployeeATID];
            }
           
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }
    async Delete() {
        // console.log(this.rowsObj)
        const listIndex: Array<any> = this.rowsObj.map((item: any) => {
            return item.Index;
        });
        if (listIndex.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            if(this.activeTab == "employeeVehicle"){
                await this.$confirmDelete().then(() => {
                    employeeVehicleApi.DeleteEmployeeVehicle(listIndex).then((res: any) => {
                        (this.$refs.employeeVehicleTable as any).getTableData(this.page);
                        if (res.status && res.status == 200) {
                            this.$deleteSuccess();
                        }
                    })
                    .catch(() => { })
                    .finally(() => {
                        (this.$refs.employeeVehicleTable as any).getTableData(this.page);
                    });
                });
            }else{
                await this.$confirmDelete().then(() => {
                    customerVehicleApi.DeleteCustomerVehicle(listIndex).then((res: any) => {
                        (this.$refs.customerVehicleTable as any).getTableData(this.page);
                        if (res.status && res.status == 200) {
                            this.$deleteSuccess();
                        }
                    })
                    .catch(() => { })
                    .finally(() => {
                        (this.$refs.customerVehicleTable as any).getTableData(this.page);
                    });
                });
            }
        }
    }
    async ConfirmClick() {
        (this.$refs.employeeVehicleModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            this.employeeVehicleModel.EmployeeATID = this.employeeVehicleModel.EmployeeATIDs[0].toString();
            if (this.isEdit == false) {
                await employeeVehicleApi.AddEmployeeVehicle(this.employeeVehicleModel).then((res: any) => {
                    if (res.status && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            else {
                await employeeVehicleApi.UpdateEmployeeVehicle(this.employeeVehicleModel).then((res: any) => {
                    if (res.status && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            (this.$refs.employeeVehicleTable as any).getTableData(this.page);
        });
    }
    async ConfirmClickCustomer() {
        (this.$refs.customerVehicleModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            this.customerVehicleModel.EmployeeATID = this.customerVehicleModel.EmployeeATIDs[0].toString();
            if (this.isEdit == false) {
                await customerVehicleApi.AddCustomerVehicle(this.customerVehicleModel).then((res: any) => {
                    if (res.status && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.ResetCustomer();
                    this.showDialog = false;
                });
            }
            else {
                await customerVehicleApi.UpdateCustomerVehicle(this.customerVehicleModel).then((res: any) => {
                    if (res.status && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.ResetCustomer();
                    this.showDialog = false;
                });
            }
            (this.$refs.customerVehicleTable as any).getTableData(this.page);
        });        
    }
    async getParkingLot() {
        await parkingLotsApi.GetParkingLotsAll().then((res: any) => {
            if (res.status == 200) {
                const arrGroupDevice = res.data.data;
                for (let i = 0; i < arrGroupDevice.length; i++) {
                    this.listParkingLot.push({
                        Index: parseInt(arrGroupDevice[i].Index),
                        Name: arrGroupDevice[i].Name
                    });
                }
                // console.log(this.listParkingLot)
            }
        });
    }
    async getDepartmentTree() {
        await departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });
    }
    async getAllEmployee() {
        await hrUserApi.GetAllEmployeeTypeUserCompactInfo().then((res: any) => {
            if (res.status == 200) {
                const data = res.data;
                this.listAllEmployee = data;
                this.listAllEmployeeFilter = data.map(x => ({
                    Index: x.EmployeeATID,
                    FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : "" )),
                    DepartmentIndex: x.DepartmentIndex,
                }));
                this.listAllEmployeeForm = this.listAllEmployeeFilter;
            }
        });
    }

    async getAllCustomer() {
        // await hrCustomerInfoApi.GetAllCustomer().then((res: any) => {
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
                    FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : "" )),
                }));
                this.listAllCustomerForm = this.listAllCustomerFilter;
            }
        });
    }
   
    async getData({filter , page, pageSize }) {
        this.page = page;
        
        const filterParam: EmployeeVehicleRequest = {
            EmployeeATID: "",
            Type: 0,
            StatusType: 0,
            Plate: "",
            Branch: "",
            Color: "",
            Description: '',
            EmployeeATIDs: this.filterModel.ListEmployeeATID,
            DepartmentIndexes: this.filterDepartment,
            Filter: this.filter,
            PageIndex: page,
            PageSize: pageSize
        };

        return await employeeVehicleApi.GetEmployeeVehicleByFilter(filterParam).then((res) => {
            if(res.data && (res.data as any).data){
                (res.data as any).data.forEach(element => {
                    element.DepartmentName = this.$t(element.DepartmentName);
                });
            }
            return {
                data: (res.data as any).data,
                total: (res.data as any).total,
            };
        });
    }

    async getDataCustomer({filter , page, pageSize }) {
        this.page = page;
        
        const filterParam: CustomerVehicleRequest = {
            EmployeeATID: "",
            Type: 0,
            StatusType: 0,
            Plate: "",
            Branch: "",
            Color: "",
            Description: '',
            EmployeeATIDs: this.filterModel.ListEmployeeATID,
            DepartmentIndexes: this.filterDepartment,
            Filter: this.filter,
            PageIndex: page,
            PageSize: pageSize
        };

        return await customerVehicleApi.GetCustomerVehicleByFilter(filterParam).then((res) => {
            return {
                data: (res.data as any).data,
                total: (res.data as any).total,
            };
        });
    }

    Cancel() {
        this.showDialog = false;
        this.Reset();
        this.onChangeDepartmentForm(this.filterFormDepartmentIndex);
    }

    CancelCustomer() {
        this.showDialog = false;
        this.ResetCustomer();
    }

    Reset() {
        this.employeeVehicleModel = {
            EmployeeATID: "",
            EmployeeATIDs: [],
            Type: 0,
            StatusType: 0,
            Plate: "",
            Branch: "",
            Color: "",
            Description: '',
        };
        this.filterFormDepartmentIndex = [];
        setTimeout(() => {
            (this.$refs.employeeVehicleModel as any).clearValidate();
        }, 200);
    }

    ResetCustomer() {
        this.customerVehicleModel = {
            EmployeeATID: "",
            EmployeeATIDs: [],
            Type: 0,
            StatusType: 0,
            Plate: "",
            Branch: "",
            Color: "",
            Description: '',
        };
        setTimeout(() => {
            (this.$refs.customerVehicleModel as any).clearValidate();
        }, 200);
    }

    handleCommand(command) {
        if (command === 'AddExcel') {
            this.AddOrDeleteFromExcel('add');
        }
    }

    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }

    async AddOrDeleteFromExcel(x) {
        if (x === 'add') {
            this.isAddFromExcel = true;
            this.showDialogExcel = true;
            this.fileName = '';
        }
        else if (x === 'close') {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }

            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            this.isDeleteFromExcel = false;
            this.showDialogExcel = false;
            this.fileName = '';
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

    onChangeDepartmentForm(departments) {
		// console.log(departments);
		this.employeeVehicleModel.EmployeeATID = "";
		this.employeeVehicleModel.EmployeeATIDs = [];
        if(departments && departments.length > 0){
            this.listAllEmployeeForm = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)))?.map(x => ({
                Index: x.EmployeeATID,
                FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : "" ))
            })) ?? [];
        }else{
            this.listAllEmployeeForm = Misc.cloneData(this.listAllEmployee).map(x => ({
                Index: x.EmployeeATID,
                FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : "" ))
            }));
        }
	}

    onChangeDepartmentFilter(departments) {
		// console.log(departments);
		this.employeeVehicleModel.EmployeeATID = "";
		this.employeeVehicleModel.EmployeeATIDs = [];
        if(departments && departments.length > 0){
            this.listAllEmployeeFilter = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex))).map(x => ({
                Index: x.EmployeeATID,
                FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : "" ))
            }));
        }else{
            this.listAllEmployeeFilter = Misc.cloneData(this.listAllEmployee).map(x => ({
                Index: x.EmployeeATID,
                FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : "" ))
            }));
        }
	}

    selectAllAccessTypeFilter(value) {
        this.filterAccessType = value;
    }

    selectAllParkingLotFilter(value) {
        this.filterParkingLotIndex = value;
    }

    selectAllEmployeeFilter(value) {
        this.filterModel.ListEmployeeATID = value;
    }

    selectAllEmployeeForm(value) {
        this.employeeVehicleModel.EmployeeATIDs = value;
    }

    selectAllCustomerFilter(value) {
        this.filterModel.ListEmployeeATID = value;
    }

    selectAllCustomerForm(value) {
        this.customerVehicleModel.EmployeeATIDs = value;
    }

    async UploadDataFromExcel() {
        this.importErrorMessage = "";
        var arrData = [];
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});
            a.CustomerIndex = 'empty';
            if(this.activeTab == "employeeVehicle"){
                if (this.dataAddExcel[0][i].hasOwnProperty('MCC (*)')) {
                    a.EmployeeATID = this.dataAddExcel[0][i]['MCC (*)'] + '';
                } else {
                    // this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                    // return;
                    a.EmployeeATID = '';
                }
                if (this.dataAddExcel[0][i].hasOwnProperty('Nhân viên')) {
                    a.FullName = this.dataAddExcel[0][i]['Nhân viên'] + '';
                } else {
                    a.FullName = '';
                }
            }else{
                if (this.dataAddExcel[0][i].hasOwnProperty('Mã khách (*)')) {
                    a.EmployeeATID = this.dataAddExcel[0][i]['Mã khách (*)'] + '';
                } else {
                    // this.$alertSaveError(null, null, null, this.$t('CustomerATIDMayNotBeBlank').toString()).toString();
                    // return;
                    a.EmployeeATID = '';
                }
                if (this.dataAddExcel[0][i].hasOwnProperty('Họ tên')) {
                    a.FullName = this.dataAddExcel[0][i]['Họ tên'] + '';
                } else {
                    a.FullName = '';
                }
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Loại phương tiện')) {
                a.TypeName = this.dataAddExcel[0][i]['Loại phương tiện'] + '';
                a.Type = this.vehicleType.find(x => x.Name == a.TypeName)?.Index ?? 0;
            } else {
                a.TypeName = '';
                a.Type = 0;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Loại xe (*)')) {
                a.StatusTypeName = this.dataAddExcel[0][i]['Loại xe (*)'] + '';
                a.StatusType = this.vehicleStatusType.find(x => x.Name == a.StatusTypeName)?.Index ?? 0;
            } else {
                a.StatusTypeName = '';
                a.StatusType = 0;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Biển số (*)')) {
                a.Plate = this.dataAddExcel[0][i]['Biển số (*)'] + '';
            } else {
                // this.$alertSaveError(null, null, null, this.$t('PlateRequired').toString()).toString();
                // return;
                a.Plate = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Nhãn hiệu')) {
                a.Branch = this.dataAddExcel[0][i]['Nhãn hiệu'] + '';
            } else {
                a.Branch = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Màu sơn')) {
                a.Color = this.dataAddExcel[0][i]['Màu sơn'] + '';
            } else {
                a.Color = '';
            }
            arrData.push(a);
        }

        // console.log(arrData)

        if(this.activeTab == "employeeVehicle"){
            await employeeVehicleApi.ImportEmployeeVehicle(arrData).then((res: any) => {
                if (res.status === 200) {
                    if(res.data){
                        this.$saveSuccess();
                        (this.$refs.employeeVehicleTable as any).getTableData(this.page);
                    }else{
                        this.importErrorMessage = this.$t('ImportEmployeeVehicleFailed').toString();
                        this.showOrHideImportError(true);
                    }
                }
            }).finally(() => {
                this.showDialogExcel = false;
                (this.$refs.employeeVehicleTable as any).getTableData(this.page);
            });
        }else{
            await customerVehicleApi.ImportCustomerVehicle(arrData).then((res: any) => {
                if (res.status === 200) {
                    if(res.data){
                        this.$saveSuccess();
                        (this.$refs.customerVehicleTable as any).getTableData(this.page);
                    }else{
                        this.importErrorMessage = this.$t('ImportEmployeeVehicleFailed').toString();
                        this.showOrHideImportError(true);
                    }
                }
            }).finally(() => {
                this.showDialogExcel = false;
                (this.$refs.customerVehicleTable as any).getTableData(this.page);
            });
        }
    }

    reloadVehicleData(){
        if(this.activeTab == "employeeVehicle"){
            (this.$refs.employeeVehicleTable as any).getTableData(this.page);
        }else{
            (this.$refs.customerVehicleTable as any).getTableData(this.page);
        }
    }

    async onViewClick() {
        //  this.configModel.filterModel = this.filterModel;
        this.page = 1;
        (this.$refs.employeeVehicleTable as any).getTableData(this.page);
    }

    async onViewClickCustomer() {
        //  this.configModel.filterModel = this.filterModel;
        this.page = 1;
        (this.$refs.customerVehicleTable as any).getTableData(this.page);
    }
}