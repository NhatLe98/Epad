import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import TabBase from '@/mixins/application/tab-mixins';
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { groupDeviceApi } from '@/$api/group-device-api';
import { isNullOrUndefined } from 'util';
import * as XLSX from 'xlsx';
import { AccessedGroupModel, accessedGroupApi } from "@/$api/gc-accessed-group-api";
import { departmentApi } from "@/$api/department-api";
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { hrUserApi } from "@/$api/hr-user-api";
import { EmployeeAccessedGroupModel, employeeAccessedGroupApi } from "@/$api/gc-employee-accessed-group-api";
@Component({
    name: "employee-accessed-group",
    components: { HeaderComponent, DataTableFunctionComponent, DataTableComponent, SelectDepartmentTreeComponent }
})
export default class EmployeeAccessedGroup extends Mixins(ComponentBase) {
    columns = [];
    rowsObj = [];
    isEdit = false;
    showDialog = false;
    rules = null;
    accessedGroupModel: EmployeeAccessedGroupModel = {
        Index: 0,
        EmployeeATID: '',
        DepartmentIDs: [],
        DepartmentIndex: 0,
        EmployeeATIDs: [],
        FromDate: new Date,
        ToDate: null
    };
    filterDepartment: any = [];
    filterModel: any = {
        ListEmployeeATID: [],
        FromDate: null,
        ToDate: null
    };
    listGeneralAccessRules = [];
    listParkingLotRules = [];
    page = 1;
    options = [];
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
    employeeFullLookup = {};
    employeeFullLookupFilter = {};
    EmployeeIDs = [];
    employeeFullLookupTemp = {};
    accessedGroupList = [];
    listAllEmployee: any = [];
    listExcelFunction = ['AddExcel','AutoSelectExcel'];
    showDialogImportError = false;
    isAddFromExcel = false;
    isDeleteFromExcel = false;
    showDialogExcel = false;
    showDialogAutoSelectExcel = false;
    fileName = '';
    dataAddExcel = [];
    dataAutoSelectExcel = [];
    importErrorMessage = '';

    masterEmployeeFilter = [];

    async beforeMount() {
        this.CreateColumns();
        this.CreateRules();
        this.LoadDepartmentTree();
        await this.getEmployeesData();
        await this.getAccessedGroupAll();
        this.Reset();
        const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
        if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.filterModel.ListEmployeeATID = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookupFilter.hasOwnProperty(x.toString()))?.map(x => x.toString()) ?? [];
		}
    }

    mounted(){
        this.updateFunctionBarCSS();
    }

    updateFunctionBarCSS() {
        const component1 = document.querySelector('.approve-change-department__custom-function-bar');
        const component2 = document.querySelector('.employee-accessed-group__data-table');

        component2.insertBefore(component1, component2.childNodes[1]);

        const component3 = document.querySelector('.filter-input');
        const component4 = document.querySelector('.approve-change-department__custom-function-bar');

        component4.insertBefore(component3, component4.childNodes[component4.childNodes.length - 1]);
        
        const component5 = document.getElementsByClassName('datatable-function employee-accessed-group__data-table-function');
        (component5[0] as HTMLElement).style.width = "100%";
        (component5[0] as HTMLElement).style.position = "unset";
    }

    CreateRules() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputAreaName'),
                    trigger: 'blur',
                },
            ],
            AccessedGroupIndex: [
                {
                    required: true,
                    message: this.$t('PleaseInputAccessedGroup'),
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
            FromDate: [
                {
                    required: true,
                    message: this.$t('PleaseSelectTime'),
                    trigger: 'blur',
                },
            ],
            ToDate: [
                {
                    trigger: 'change',
                    message: this.$t('FromDateCannotLargerToDate'),
                    validator: (rule, value: string, callback) => {

                        if (this.accessedGroupModel.FromDate != null) {
                            this.accessedGroupModel.FromDate = new Date(moment(this.accessedGroupModel.FromDate).format('YYYY-MM-DD'));
                        }
                        if (this.accessedGroupModel.ToDate != null) {
                            this.accessedGroupModel.ToDate = new Date(moment(this.accessedGroupModel.ToDate).format('YYYY-MM-DD'));
                        }
                        if (this.accessedGroupModel.FromDate != null && this.accessedGroupModel.ToDate != null
                            && this.accessedGroupModel.FromDate > this.accessedGroupModel.ToDate) {
                                callback(new Error());
                        }
                        else {
                            callback();
                        }
                    },
                }
            ],
        }
    }

    CreateColumns() {
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
                prop: 'EmployeeName',
                label: 'Employee',
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
                prop: 'FromDateFormat',
                label: 'FromTime',
                minWidth: 150,
                display: true
            },
            {
                prop: 'ToDateFormat',
                label: 'ToTime',
                minWidth: 150,
                display: true
            },
            {
                prop: 'AccessedGroupName',
                label: 'AccessedGroup',
                minWidth: 150,
                display: true
            }
        ];
    }

    Insert() {
        this.showDialog = true;
        if (this.isEdit == true) {
            this.Reset();
        }
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.accessedGroupModel.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookup.hasOwnProperty(x.toString()))?.map(x => x.toString()) ?? [];
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
            this.accessedGroupModel = obj[0];
            this.accessedGroupModel.EmployeeATIDs = [this.accessedGroupModel.EmployeeATID];
            this.accessedGroupModel.DepartmentIDs = [this.accessedGroupModel.DepartmentIndex];
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
       // (this.$refs.employeeAccessedGroupTable as any).getTableData(this.page);
    }

    Delete() {
        const listIndex: Array<number> = this.rowsObj.map((item: any) => {
            return item.Index;
        });

        if (listIndex.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            this.$confirmDelete().then(() => {
                employeeAccessedGroupApi.DeleteEmployeeAccessedGroup(listIndex).then((res: any) => {
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$deleteSuccess();
                    }
                })
                    .catch(() => { })
                    .finally(() => {
                        (this.$refs.employeeAccessedGroupTable as any).getTableData(this.page);
                    });
            });
        }
        // (this.$refs.employeeAccessedGroupTable as any).getTableData(this.page);
    }

    async ConfirmClick() {
        (this.$refs.accessedGroupModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            const submitData = Misc.cloneData(this.accessedGroupModel);
            
            submitData.FromDate = new Date(
                moment(submitData.FromDate).format("YYYY-MM-DD")
            );
            submitData.ToDate = new Date(
                moment(submitData.ToDate).format("YYYY-MM-DD")
            );
            if (this.isEdit == false) {
                await employeeAccessedGroupApi.AddEmployeeAccessedGroup(submitData).then((res: any) => {
                    if (res.status === 200 && res.data) {
                        const msg = res.data;
                        this.$alert(
                            this.$t("MSG_DataRegisterExistedFromTo", { data: msg }).toString(),
                            this.$t("Notify").toString(), { dangerouslyUseHTMLString: true }
                        );
                    }
                    else if (res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            else {
                await employeeAccessedGroupApi.UpdateEmployeeAccessedGroup(submitData).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            (this.$refs.employeeAccessedGroupTable as any).getTableData(this.page);
        });
    }


    async getAccessedGroupAll() {
        await accessedGroupApi.GetAccessedGroupNormal().then((res: any) => {
            if (res.status == 200) {
                this.accessedGroupList = res.data;
            }
        });
    }

    onViewClick(){
        (this.$refs.employeeAccessedGroupTable as any).getTableData(this.page);
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        var request = {
            DepartmentIDs: this.filterDepartment,
            EmployeeATIDs: this.filterModel.ListEmployeeATID,
            FromDateString: this.filterModel.FromDate ? moment(this.filterModel.FromDate).format("YYYY-MM-DD") : "",
            ToDateString: this.filterModel.ToDate ? moment(this.filterModel.ToDate).format("YYYY-MM-DD") : "",
            pageIndex: this.page,
            pageSize: pageSize,
            filter: filter
        };
        // return await employeeAccessedGroupApi.GetEmployeeAccessedGroup(filter, page, pageSize).then((res) => {
        return await employeeAccessedGroupApi.GetEmployeeAccessedGroupByFilter(request).then((res) => {
            // console.log((res.data as any).data)
            return {
                data: (res.data as any).data,
                total: (res.data as any).total,
            };
        });
    }

    LoadDepartmentTree() {
        departmentApi.GetDepartmentTreeEmployeeScreen("10").then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });

    }

    async getEmployeesData() {
        await hrUserApi.GetAllEmployeeCompactInfo().then((res: any) => {
            if (res.status == 200) {
                const data = res.data;
                const dictData = {};
                this.listAllEmployee = data;
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
                this.employeeFullLookup = dictData;
                this.employeeFullLookupFilter = dictData;
                this.employeeFullLookupTemp = dictData;
            }
        });
    }

    onChangeDepartmentForm(departments) {
        this.accessedGroupModel.EmployeeATIDs = [];
        if (departments && departments.length > 0) {
            this.employeeFullLookup = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)))?.map(x => ({
                Index: x.EmployeeATID,
                NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            })) ?? [];
        } else {
            this.employeeFullLookup = Misc.cloneData(this.listAllEmployee).map(x => ({
                Index: x.EmployeeATID,
                NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            }));
        }
    }

    onChangeDepartmentFilter(departments) {
        // console.log(departments)
        this.filterModel.ListEmployeeATID = [];
        if (departments && departments.length > 0) {
            this.employeeFullLookupFilter = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)))?.map(x => ({
                Index: x.EmployeeATID,
                NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            })) ?? [];
        } else {
            this.employeeFullLookupFilter = Misc.cloneData(this.listAllEmployee).map(x => ({
                Index: x.EmployeeATID,
                NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            }));
        }
    }

    selectAllEmployeeFilter(value) {
        // console.log(value)
        this.EmployeeIDs = [...value];
		this.accessedGroupModel.EmployeeATIDs = [...value];
    }

    Cancel() {
        this.Reset();
        this.showDialog = false;
    }

    Reset() {
        this.accessedGroupModel = {
            Index: 0,
            EmployeeATID: '',
            DepartmentIDs: [],
            DepartmentIndex: 0,
            EmployeeATIDs: [],
            FromDate: new Date,
            ToDate: null
        };
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

    async AutoSelectFromExcel(x) {
        if (x === 'open') {
            this.showDialogAutoSelectExcel = true;
            this.fileName = '';
        }
        else if (x === 'close') {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileAutoSelect')))) {
                (<HTMLInputElement>document.getElementById('fileAutoSelect')).value = '';
            }

            this.dataAutoSelectExcel = [];
            this.showDialogAutoSelectExcel = false;
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

    processAutoSelectFile(e) {
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
                this.dataAutoSelectExcel = arrData;
                fileReader.readAsBinaryString(file);
            }
        }
        // console.log(this.dataAutoSelectExcel)
    }

    async ProcessAutoSelectFromExcel() {
        let dataAutoSelect = [];
        var regex = /^\d+$/;
        for (let i = 0; i < this.dataAutoSelectExcel[0].length; i++) {
            let a = Object.assign({});
            if (regex.test(this.dataAutoSelectExcel[0][i]['Mã chấm công (*)']) === false) {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDOnlyAcceptNumericCharacters').toString()).toString();
                return;
            } else if (this.dataAutoSelectExcel[0][i].hasOwnProperty('Mã chấm công (*)')) {
                a.EmployeeATID = this.dataAutoSelectExcel[0][i]['Mã chấm công (*)'] + '';
            } else {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                return;
            }
            if (this.dataAutoSelectExcel[0][i].hasOwnProperty('Mã nhân viên')) {
                a.EmployeeCode = this.dataAutoSelectExcel[0][i]['Mã nhân viên'] + '';
            } else {
                a.EmployeeCode = '';
            }
            if (this.dataAutoSelectExcel[0][i].hasOwnProperty('Tên nhân viên')) {
                a.FullName = this.dataAutoSelectExcel[0][i]['Tên nhân viên'] + '';
            } else {
                a.FullName = '';
            }

            dataAutoSelect.push(a);
        }

        var listEmployeeATID = dataAutoSelect.map((e) => e.EmployeeATID);
        this.filterModel.ListEmployeeATID = listEmployeeATID;
        (this.$refs.employeeAccessedGroupTable as any).getTableData(1).then(() => {
            this.filterModel.ListEmployeeATID = [];
            this.showDialogAutoSelectExcel = false;
        });
    }

    async UploadDataFromExcel() {
        this.importErrorMessage = "";
        var arrData = [];
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});

            if (this.dataAddExcel[0][i].hasOwnProperty('MCC (*)')) {
                a.EmployeeATID = this.dataAddExcel[0][i]['MCC (*)'] + '';
            } else {
                a.EmployeeATID = '';
            }

            if (this.dataAddExcel[0][i].hasOwnProperty('Nhân viên')) {
                a.EmployeeName = this.dataAddExcel[0][i]['Nhân viên'] + '';
            } else {
                a.FullName = '';
            }
            
            if (this.dataAddExcel[0][i].hasOwnProperty('Từ ngày (*)')) {
                a.FromDateFormat = this.dataAddExcel[0][i]['Từ ngày (*)'] + '';
            } else {
                a.FromDateFormat = '';
            }

            if (this.dataAddExcel[0][i].hasOwnProperty('Đến ngày')) {
                a.ToDateFormat = this.dataAddExcel[0][i]['Đến ngày'] + '';
            } else {
                a.ToDateFormat = '';
            }

            if (this.dataAddExcel[0][i].hasOwnProperty('Nhóm truy cập (*)')) {
                a.AccessedGroupName = this.dataAddExcel[0][i]['Nhóm truy cập (*)'] + '';
            } else {
                a.AccessedGroupName = '';
            }
            
            arrData.push(a);
        }

        await employeeAccessedGroupApi.ImportEmployeeAccessedGroup(arrData).then((res: any) => {
            if (res.status && res.status == 200) {
                if(res.data){
                    this.$saveSuccess();
                    (this.$refs.employeeAccessedGroupTable as any).getTableData(this.page);

                }else{
                    this.importErrorMessage = this.$t('ImportEmployeeAccessedGroupFailed').toString();
                    this.showOrHideImportError(true);
                }
            }
        })
        .finally(() => {
            this.showDialogExcel = false;
        });
    }

}