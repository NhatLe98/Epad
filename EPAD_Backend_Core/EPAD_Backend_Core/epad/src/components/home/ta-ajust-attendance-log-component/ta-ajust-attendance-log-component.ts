import { Component, Mixins, Watch } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { AC_AreaDTO, areaApi } from '@/$api/ac-area-api';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { departmentApi } from '@/$api/department-api';
import { hrUserApi } from '@/$api/hr-user-api';
import TabBase from '@/mixins/application/tab-mixins';
import VisualizeTable from '@/components/app-component/visualize-table/visualize-table.vue';
import AppPagination from '@/components/app-component/app-pagination/app-pagination.vue';
import timePicker from '@/components/app-component/app-time-picker-component/app-time-picker-component.vue'
import { TA_AjustAttendanceLogParam, taAjustAttendanceLog } from '@/$api/ta-ajust-attendance-log-api';
import { deviceApi } from '@/$api/device-api';
import * as XLSX from 'xlsx';

@Component({
    name: 'ta-ajust-attendance-log',
    components: {
        HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectDepartmentTreeComponent, VisualizeTable,
        AppPagination, timePicker
    },
})


export default class AjustAttendanceLogComponent extends Mixins(TabBase) {
    page = 1;
    showDialog = false;
    showMessage = false;
    checked = false;
    rowsObj = [];
    isLoading = false;
    EmployeeATIDs = [];
    EmployeeATIDsFilter = [];
    isEdit = false;
    filter = '';
    listExcelFunction = ['AddExcel'];
    ruleForm: any = {AttendanceDate: new Date(), AttendanceTime: new Date()};
    attendanceVerifyModeLst = [{ value: 1, label: 'Vân tay' },  { value: 3, label: 'Mật khẩu' }, { value: 4, label: 'Thẻ' }, { value: 15, label: 'Khuôn mặt' }]
    allInOutMode = [{ value: 0, label: 'Vào' }, { value: 1, label: 'Ra' }]
    deviceLst = [];
    showbtnSave = false;
    ruleObject: any = {};
    SelectedDepartment = [];
    rules: any = {};
    employeeFullLookupTemp = {};
    employeeFullLookup = {};
    DepartmentIDss = [];
    listAllEmployee = [];
    selectAllEmployeeFilter = [];
    shouldResetColumnSortState = false;
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


    columns = [
        {
            field: 'indexStt',
            sortable: true,
            pinned: true,
            headerName: '#',
            width: 80,
            checkboxSelection: true,
            headerCheckboxSelection: true,
            headerCheckboxSelectionFilteredOnly: true,
            display: true,
            type: 'editableColumn'
        },
        
        {
            field: 'EmployeeATID',
            headerName: this.$t('MCC'),
            pinned: true,
            width: 150,
            sortable: true,
            display: true,
            type: 'editableColumn'
        },
        {
            field: 'EmployeeCode',
            headerName: this.$t('EmployeeCode'),
            pinned: true,
            width: 150,
            sortable: true,
            display: true,
            type: 'editableColumn'
        },
        {
            headerName: this.$t('FullName'),
            field: 'FullName',
            pinned: true,
            width: 150,
            sortable: true,
            display: true,
            type: 'editableColumn'
        },
        {
            field: 'DepartmentName',
            headerName: this.$t('Department'),
            pinned: true,
            width: 150,
            sortable: true,
            display: true,
            type: 'editableColumn'
        },
        {
            headerName: this.$t('Day'),
            field: 'Day',
            pinned: false,
            width: 150,
            sortable: true,
            display: true,
            type: 'editableColumn'
        },
        {
            headerName: this.$t('Hour'),
            field: 'ProcessedCheckTime',
            pinned: false,
            width: 150,
            sortable: true,
            display: true,
            editable: true,
            type: 'editableColumn',
            cellEditorSelector: (params) => {
                return {

                    component: 'timePicker',
                };
            },
            cellRenderer: params => params.value ? `${moment(params.value).format('HH:mm:ss')}` : ''
        },
        {
            headerName: this.$t('AttendanceVerifyMode'),
            field: 'VerifyModeString',
            pinned: false,
            width: 200,
            sortable: true,
            display: true,
            type: 'editableColumn',
            cellRenderer: params => params.value ? this.$t(params.value) : ''
        },
        {
            headerName: this.$t('InOutModeString'),
            field: 'InOutModeString',
            pinned: false,
            width: 150,
            sortable: true,
            display: true,
            type: 'editableColumn'
        },
        {
            headerName: this.$t('Device'),
            field: 'DeviceName',
            pinned: false,
            width: 150,
            sortable: true,
            display: true,
            type: 'editableColumn'
        },
        {
            headerName: this.$t('Note'),
            field: 'Note',
            pinned: false,
            width: 150,
            sortable: true,
            display: true,
            
        }
    ];

    masterEmployeeFilter = [];

    async beforeMount() {
        this.initColumns();
        this.initRule();
        this.LoadDepartmentTree();
        await this.getEmployeesData();
        await this.getAllDevices();
        const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.EmployeeATIDsFilter = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookup.hasOwnProperty(x.toString()))?.map(x => x.toString()) ?? [];
		}
    }

    async getAllDevices() {
        await deviceApi.GetDeviceAll()
            .then(res => {
                this.deviceLst = (res.data as any)
            })
    }

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

    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }

    initRule() {
        this.rules = {
            EmployeeATID: [
                {
                    required: true,
                    message: this.$t('PleaseSelectEmployee'),
                    trigger: 'blur',
                }

            ],
            AttendanceDate: [
                {
                    required: true,
                    message: this.$t('PleaseSelectDate'),
                    trigger: 'blur',
                },
            ],
            AttendanceTime: [
                {
                    required: true,
                    message: this.$t('PleaseChooseAttendanceTime'),
                    trigger: 'blur',
                },
            ]
        };
    }

    mounted() {

    }

    @Watch('selectedRows')
    selectedRowsChange(value) {
        if (value && value.length > 0) {
            this.showbtnSave = true;
        } else {
            this.showbtnSave = false;
        }
    }

    initColumns() {
        this.ruleObject = {
            FromDate: new Date(),
            ToDate: new Date()
        }

    }

    selectAllEmployeeFilterGet(value){
        this.EmployeeATIDsFilter = [...value];
        this.$forceUpdate();
    }



    async loadData() {
        this.isLoading = true;

        if (this.ruleObject.FromDate != null) {
            this.ruleObject.FromDate = new Date(moment(this.ruleObject.FromDate).format('YYYY-MM-DD'));
        }
        if (this.ruleObject.ToDate != null) {
            this.ruleObject.ToDate = new Date(moment(this.ruleObject.ToDate).format('YYYY-MM-DD'));
        }
        if (this.ruleObject.FromDate != null && this.ruleObject.ToDate != null
            && this.ruleObject.FromDate > this.ruleObject.ToDate) {
            this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
            this.isLoading = false;
            return;
        }

        const fromDate = moment(this.ruleObject.FromDate).format('YYYY-MM-DD');
        const toDate = moment(this.ruleObject.ToDate).format('YYYY-MM-DD');
        const filterModel: TA_AjustAttendanceLogParam = {
            Departments: this.SelectedDepartment,
            EmployeeATIDs: this.EmployeeATIDsFilter,
            Filter: this.filter,
            FromDate: fromDate,
            ToDate: toDate,
            Limit: this.pageSize,
            Page: this.page
        }


        await taAjustAttendanceLog.GetAjustAttendanceLogAtPage(filterModel).then((response): any => {
            const { data, total }: { data: any[], total: number } = response.data;
            this.dataSource = data.map((emp, idx) => ({
                ...emp,
                indexStt: idx + 1 + (this.page - 1) * this.pageSize,
            }));
            this.total = total;
            this.shouldResetColumnSortState = !this.shouldResetColumnSortState;
        });
        // console.log(this.dataSource)
        this.isLoading = false;
    }

    async Savebtn() {
        const value = this.selectedRows.map(x =>( {...x, ProcessedCheckTimeString :  moment(x.ProcessedCheckTime).format('YYYY-MM-DD HH:mm:ss')}));
        await taAjustAttendanceLog.UpdateAjustAttendanceLogLst(value).then((res) => {
            this.showDialog = false;
            this.reset();
            if (res.status === 200 && res.data) {
                const msg: any = res.data;
                this.$alert(msg, this.$t('Warning').toString(), { 
                    type: "warning", 
                    dangerouslyUseHTMLString: true 
                });
            }
            else if (res.status === 200) {
                this.$saveSuccess();
                this.loadData();
            }
            
        });
    }
    viewData(){
        this.page = 1;
        (this.$refs.taAjustAttendacenLogPagination as any).page = this.page;
		(this.$refs.taAjustAttendacenLogPagination as any).lPage = this.page;
		this.loadData();
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
                        if (sheet == "Data") {
                            var range = XLSX.utils.decode_range(workbook.Sheets[sheet]['!ref']);
                            range.s.r = 1; // <-- zero-indexed, so setting to 1 will skip row 0
                            workbook.Sheets[sheet]['!ref'] = XLSX.utils.encode_range(range);
                        }
                        var rowObject = XLSX.utils.sheet_to_json(workbook.Sheets[sheet]);
                        // console.log(rowObject)
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



    handleCommand(command) {
        if (command === 'AddExcel') {
            this.AddOrDeleteFromExcel('add');
        }
        // else if (command === 'ExportExcel') {
        // 	// this.ExportToExcel();
        // }
        // else if (command === 'DeleteExcel') {
        // 	this.AddOrDeleteFromExcel('delete');
        // }
    }

    UploadDataFromExcel() {

        this.importErrorMessage = "";
        var arrData = [];
        var regex = /^\d+$/;
        console.log(this.dataAddExcel);
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});

            if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm công (*)')) {
                a.EmployeeATID = this.dataAddExcel[0][i]['Mã chấm công (*)'] + '';
            } 
            // else {
            //     console.log(this.dataAddExcel[0][i]);
            //     this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
            //     return;
            // }
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
            if (this.dataAddExcel[0][i].hasOwnProperty('Ngày (*)')) {
                a.Date = this.dataAddExcel[0][i]['Ngày (*)'] + '';
            }
            //  else {
            //     this.$alertSaveError(null, null, null, this.$t('DateMayNotBeBlank').toString()).toString();
            //     return;
            // }
            if (this.dataAddExcel[0][i].hasOwnProperty('Vào')) {
                a.In = this.dataAddExcel[0][i]['Vào'] + '';
            } else {
                a.In = '';
            }

            if (this.dataAddExcel[0][i].hasOwnProperty('Ra')) {
                a.Out = this.dataAddExcel[0][i]['Ra'] + '';
            } else {
                a.Out = '';
            }

            // if (a.In == '' && a.Out == '') {
            //     this.$alertSaveError(null, null, null, this.$t('InAndOutMayNotBeBlank').toString()).toString();
            //     return;
            // }

            if (this.dataAddExcel[0][i].hasOwnProperty('Chế độ điểm danh')) {
                a.VerifyMode = this.dataAddExcel[0][i]['Chế độ điểm danh'] + '';
            } else {
                a.VerifyMode = '';
            }

            if (this.dataAddExcel[0][i].hasOwnProperty('Thiết bị')) {
                a.SerialNumber = this.dataAddExcel[0][i]['Thiết bị'] + '';
            } else {
                a.SerialNumber = '';
            }

            if (this.dataAddExcel[0][i].hasOwnProperty('Ghi chú')) {
                a.Note = this.dataAddExcel[0][i]['Ghi chú'] + '';
            } else {
                a.Note = '';
            }
            arrData.push(a);
        }

        taAjustAttendanceLog.AddAjustAttendanceLogFromExcel(arrData).then((res) => {
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
            console.log(res)
            if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
                console.log("Import success")
                this.$saveSuccess();
                this.loadData();
            } else {
                this.importErrorMessage = this.$t('AddAttendanceLogFromExcelError') + res.data.toString() + " " + this.$t('Row');
                console.log("Import error, show popup import error file download")
                this.showOrHideImportError(true);
            }
        });

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
        }
    }



    displayPopupInsert() {
        this.showDialog = false;
    }


    onSelectionChange(selectedRows: any[]) {
        // // console.log(selectedRows);
        this.selectedRows = selectedRows;
        this.rowsObj = selectedRows;
    }

    onChangeDepartmentFilter(departments) {
        console.log(this.listAllEmployee, departments);
        // delete this.ruleForm.EmployeeATIDs; 
        this.EmployeeATIDs = [];
        if (departments && departments.length > 0) {
            this.employeeFullLookup = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)))?.map(x => ({
                Index: x.EmployeeATID,
                NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            }));
        } else {
            this.employeeFullLookup = Misc.cloneData(this.listAllEmployee)?.map(x => ({
                Index: x.EmployeeATID,
                NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            }));
        }
    }

    getValueSelectedAll(value){
        (this.formModel as any).EmployeeATID = value;
        this.$forceUpdate();
        (this.$refs.ajustAttendanceLogRef as any).validate();
    }


    onChangeDepartmentFilterSearch(departments) {

        this.selectAllEmployeeFilter = [];
        if (departments && departments.length > 0) {
            this.employeeFullLookup = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)))?.map(x => ({
                Index: x.EmployeeATID,
                NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            }));
        } else {
            this.employeeFullLookup = Misc.cloneData(this.listAllEmployee)?.map(x => ({
                Index: x.EmployeeATID,
                NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            }));
        }
    }

    LoadDepartmentTree() {
        departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
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
                this.employeeFullLookupTemp = dictData;
            }
        });
    }


    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        return await areaApi.GetAreaAtPage(page, filter, pageSize).then((res) => {
            const { data } = res as any;
            return {
                data: data.data,
                total: data.total,
            };
        });
    }

    reset() {
        const obj: any = {AttendanceDate: new Date(), AttendanceTime: new Date()};
        this.ruleForm = obj;
    }

    Insert() {
        this.showDialog = true;
        this.isEdit = false;
        this.reset();
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.ruleForm.EmployeeATID = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookup.hasOwnProperty(x.toString()))?.map(x => x.toString())[0] ?? null;
		}
    }

    async Submit() {
        (this.$refs.ajustAttendanceLogRef as any).validate(async (valid) => {
            if (!valid) return;
            else {
                if (this.isEdit == true) {
                    this.ruleForm.AttendanceDate = moment(this.ruleForm.AttendanceDate).format("YYYY-MM-DD HH:mm:ss");
                    this.ruleForm.AttendanceTime = moment(this.ruleForm.AttendanceTime).format("YYYY-MM-DD HH:mm:ss");
                    return await taAjustAttendanceLog.UpdateAjustAttendanceLog(this.ruleForm).then((res) => {

                        this.showDialog = false;
                        this.reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                            this.loadData();
                        }
                    });
                } else {
                    this.ruleForm.AttendanceDate = moment(this.ruleForm.AttendanceDate).format("YYYY-MM-DD HH:mm:ss");
                    this.ruleForm.AttendanceTime = moment(this.ruleForm.AttendanceTime).format("YYYY-MM-DD HH:mm:ss");
                    return await taAjustAttendanceLog
                        .AddAjustAttendanceLog(this.ruleForm)
                        .then((res) => {

                            this.showDialog = false;
                            this.reset();
                            if (!isNullOrUndefined(res.status) && res.status === 200) {
                                this.$saveSuccess();
                                this.loadData();
                            }
                        })
                        .catch(() => { });
                }
            }
        });
    }

    Edit() {
        this.isEdit = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));

        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (obj.length === 1) {
            this.showDialog = true;
            console.log(obj[0].ProcessedCheckTime);
            obj[0].AttendanceDate =new Date( obj[0].ProcessedCheckTime);
            obj[0].AttendanceTime = obj[0].ProcessedCheckTime;
            this.ruleForm = {...obj[0]};
            this.ruleForm.DepartmentIDs = [this.ruleForm.DepartmentIndex]
            console.log(this.ruleForm);
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }


    async Delete() {
        const obj = JSON.parse(JSON.stringify(this.selectedRows));
        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            this.$confirmDelete().then(async () => {
                await taAjustAttendanceLog
                    .DeleteAjustAttendanceLog(obj)
                    .then((res) => {

                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$deleteSuccess();
                            this.loadData();
                        }
                    })
                    .catch(() => { });
            });
        }
    }

    focus(x) {
        var theField = eval('this.$refs.' + x);
        theField.focus();
    }

    Cancel() {
        (this.$refs.ajustAttendanceLogRef as any).resetFields();
        (this.$refs.ajustAttendanceLogRef as any).clearValidate();
        this.showDialog = false;
        this.selectedRows = [];
		this.reset();
    }
}
