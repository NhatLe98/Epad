
import { Component, Vue, Mixins, Model, Watch, Prop } from 'vue-property-decorator';
import { departmentApi } from '@/$api/department-api';
import TabBase from '@/mixins/application/tab-mixins';
import { employeeInfoApi, Finger } from '@/$api/employee-info-api';
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import { hrPositionInfoApi } from '@/$api/hr-position-info-api';
import { employeeTypesApi } from '@/$api/ic-employee-type-api';
import { isNullOrUndefined } from "util";
import * as XLSX from 'xlsx';
import EXCEL from 'exceljs';
import { store } from '@/store';
import VisualizeTable from '@/components/app-component/visualize-table/visualize-table.vue';
import AppPagination from '@/components/app-component/app-pagination/app-pagination.vue';
import { commandApi } from '@/$api/command-api';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { userTypeApi } from '@/$api/user-type-api';
import TableToolbar from '@/components/home/printer-control/table-toolbar.vue';
import { hrUserApi } from '@/$api/hr-user-api';
import { EmployeeShiftModel, EmployeeShiftRequest, taEmployeeShiftApi } from '@/$api/ta-employee-shift-api';
import { taShiftApi } from '@/$api/ta-shift-api';
import comboBox from '@/components/app-component/app-select-table-component/app-select-table-component.vue'
import { taScheduleFixedByDepartment } from '@/$api/ta-schedule-fixed-department-api';
@Component({
    name: 'register-employee-shift',
    components: {
        VisualizeTable,
        AppPagination,
        SelectDepartmentTreeComponent,
        TableToolbar,
        comboBox
    },
})
export default class TARegisterEmployeeShift extends Mixins(TabBase) {
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
    activeObj: any = [];
    ruleForm: any = {}
    ListFingerRequest: any;
    clientName: string;
    @Prop({ default: () => false }) showMore: boolean;
    value = '';
    shouldResetColumnSortState = false;
    //=====================
    fileImageName = '';
    errorUpload = false;
    filterModel = { DepartmentIds: [], EmployeeATIDs: [], TextboxSearch: '', FromDate: new Date(), ToDate: new Date() };
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
    employeeFullLookupTemp = {};
    employeeFullLookup = {};
    DepartmentIDss = [];
    listAllEmployee = [];
    employeeFullLookupForm = {};

    dataSource = [];
    listShift: any[];
    EmployeeATIDs = [];
    columnDefs = [];
    listColumn = [];
    listData = [];
    rowsObj = [];

    saveLoading = false;
    isReading = false;
    isComplete = false;
    fileList: any[] = [];
    dateFormat = 'DD/MM/YYYY';
    errors: any = {};
    importType = false;
    @Prop({ default: false }) allowDatFile;
    @Prop({ default: 0 }) sheetIndex: number;
    @Prop({ default: 0 }) titleRow;
    @Prop({ default: () => [] }) fieldNotNull: [];
    @Prop({ default: 1 }) fromRow;
    @Prop({ default: false }) customDataChange: boolean;
    @Prop({ default: () => [] }) dateFormatPos: string[];
    @Prop() invalidTemplateOverride: (exc: any, comp: any) => any;
    @Prop({ default: () => [] }) mapOption: Record<string, Array<{ from: string; to: any }>>;
    @Prop({ default: () => ({}) }) defaultRowValue;

    masterEmployeeFilter = [];

    onSelectionChange(selectedRows: any[]) {
        // // console.log(selectedRows);
        this.selectedRows = selectedRows;
    }
    async beforeMount() {
        this.columns = new Array(35).fill(null).map((x) => ({}));
        var date = new Date();
        this.filterModel.FromDate = new Date(date.getFullYear(), date.getMonth(), 1);
        this.filterModel.ToDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);
        await this.LoadDepartmentTree();
        await this.getEmployeesData();
        await this.getShiftList();
        this.initFormRules();
        const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.filterModel.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookupForm.hasOwnProperty(x.toString()))?.map(x => x.toString()) ?? [];
		}
    }

    LoadDepartmentTree() {
        departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });
    }

    async getShiftList() {
        await taShiftApi.GetShiftByCompanyIndex().then((res) => {
            if (res.status && res.status == 200 && res.data && (res.data as any).length > 0) {
                this.listShift = Misc.cloneData((res.data as any));
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
                this.employeeFullLookupForm = dictData;
                this.employeeFullLookup = dictData;
                this.employeeFullLookupTemp = dictData;
            }
        });
    }

    initFormRules() {
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
            FromDate: [
                {
                    required: true,
                    message: this.$t('PleaseSelectFromDate'),
                    trigger: 'change',
                },
            ],
            ToDate: [
                {
                    required: true,
                    message: this.$t('PleaseSelectToDate'),
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
            TypeOfLeaveDate: [
                {
                    required: true,
                    message: this.$t('PleaseInputTypeOfLeaveDate'),
                    trigger: 'change',
                },
            ],
            DurationType: [
                {
                    required: true,
                    message: this.$t('PleaseInputDurationType'),
                    trigger: 'change',
                },
            ],
        };
    }

    onEditClick() {
        this.formModel = this.selectedRows[0];
        if (this.clientName == 'MAY') {
            this.formModel['PositionIndex'] = this.formModel['EmployeeType']
        }
        this.isEdit = true;
        this.showDialog = true;
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

    page = 1; 
    pageSize = 20;
    total = 0;

    async loadData() {
        this.isLoading = true;
        this.listData = [];
        this.listColumn = [];
      
        if (this.filterModel.FromDate != null) {
			this.filterModel.FromDate = new Date(moment(this.filterModel.FromDate).format('YYYY-MM-DD'));
		}
		if (this.filterModel.ToDate != null) {
			this.filterModel.ToDate = new Date(moment(this.filterModel.ToDate).format('YYYY-MM-DD'));
		}
		let maxDate = new Date(this.filterModel.FromDate);

		maxDate.setMonth(maxDate.getMonth() + 1);
		if (maxDate.getDate() !== this.filterModel.FromDate.getDate()) {
			maxDate = new Date(this.filterModel.FromDate);
			const date = (maxDate.getFullYear().toString() + "-" + (maxDate.getMonth() + 3).toString() + "-1");
			maxDate = new Date(date);
			maxDate.setDate(maxDate.getDate() - 1);
		}

		if (this.filterModel.FromDate != null && this.filterModel.ToDate != null
			&& this.filterModel.FromDate > this.filterModel.ToDate) {
			this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
			this.isLoading = false;
			return;
		}

		if (this.filterModel.ToDate >= maxDate) {
			this.$alertSaveError(null, null, null, this.$t('PleaseChooseMaximum1Month').toString()).toString();
			this.isLoading = false;
			return;
		}

        const request: EmployeeShiftRequest = {
            FromDate: this.filterModel.FromDate,
            ToDate: this.filterModel.ToDate,
            EmployeeATIDs: this.filterModel.EmployeeATIDs,
            DepartmentIDs: this.filterModel.DepartmentIds,
            page: this.page,
            pageSize: this.pageSize
        }
        await taEmployeeShiftApi.GetEmployeeShiftByFilter(request).then((res: any) => {
            const Data = res.data;
            this.total = Data.total;
            Data.Columns.forEach(item => {
                const col = {
                    index: item.Index,
                    name: this.$t(item.Name),//"Ca 1"
                    dataField: item.Code,//"Ca1"
                };
                this.listColumn.push(col);
            });

            Data.Rows.forEach(element => {
                const col = {
                    index: element.Index,
                    EmployeeATID: element.EmployeeATID,
                    EmployeeCode: element.EmployeeCode,
                    EmployeeName: element.EmployeeName,
                    DepartmentName: element.DepartmentName,
                    DepartmentIndex: element.DepartmentIndex,
                    Name: element.ShiftName
                };
                this.listColumn.forEach(item => {
                    const dt = element.ListColumnObjectUsed.find(e => e.KeyMain == item.dataField);
                    if (dt != null) {
                        col[item.dataField] = dt.Index;
                    }
                });
                this.listData.push(col);
            });
        });
        this.isLoading = false;
    }

    async exportDataTemplateImport() {
      
        if (this.filterModel.FromDate != null) {
			this.filterModel.FromDate = new Date(moment(this.filterModel.FromDate).format('YYYY-MM-DD'));
		}
		if (this.filterModel.ToDate != null) {
			this.filterModel.ToDate = new Date(moment(this.filterModel.ToDate).format('YYYY-MM-DD'));
		}
		let maxDate = new Date(this.filterModel.FromDate);

		maxDate.setMonth(maxDate.getMonth() + 1);
		if (maxDate.getDate() !== this.filterModel.FromDate.getDate()) {
			maxDate = new Date(this.filterModel.FromDate);
			const date = (maxDate.getFullYear().toString() + "-" + (maxDate.getMonth() + 3).toString() + "-1");
			maxDate = new Date(date);
			maxDate.setDate(maxDate.getDate() - 1);
		}

		if (this.filterModel.FromDate != null && this.filterModel.ToDate != null
			&& this.filterModel.FromDate > this.filterModel.ToDate) {
			this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
			return;
		}

		if (this.filterModel.ToDate >= maxDate) {
			this.$alertSaveError(null, null, null, this.$t('PleaseChooseMaximum1Month').toString()).toString();
			return;
		}

        const request: EmployeeShiftRequest = {
            FromDate: this.filterModel.FromDate,
            ToDate: this.filterModel.ToDate,
            EmployeeATIDs: this.filterModel.EmployeeATIDs,
            DepartmentIDs: this.filterModel.DepartmentIds,
            page: this.page,
            pageSize: this.pageSize
        }
        await taEmployeeShiftApi.ExportDataIntoTemplateImport(request).then((res: any) => {
            const fileURL = window.URL.createObjectURL(new Blob([res.data]));
            const fileLink = document.createElement("a");

            fileLink.href = fileURL;
            fileLink.setAttribute("download", `Data_Employee_Shift.xlsx`);
            document.body.appendChild(fileLink);
            fileLink.click();
        });
    }

    async saveData() {
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        console.log("saveData ~ obj:", obj)
        if (obj.length > 0) {
            const request = this.getRequestData(obj);
            console.log('SaveData', request)
            taEmployeeShiftApi.AddEmployeeShift(request).then((res: any) => {
                console.log("taEmployeeShiftApi.AddEmployeeShift ~ res:", res)
                const { Status, MessageCode, MessageDetail } = res.data;
                if (Status === 'false' || Status === 'fail') {
                    this.$alert(this.$t('SaveFailed').toString(), this.$t('Warning').toString());
                }
                else {
                    this.selectedRows = [];
				this.$saveSuccess();
                }
            }).catch(err => {
                this.$alert(this.$t(err).toString(), this.$t('Notify').toString());
            })
        } 
        else 
        {
            console.log("ChooseUpdate")
            this.$alert(this.$t("ChooseUpdate").toString(), this.$t('Notify').toString());
        }
    }

    onChangeDepartmentForm(departments) {
        // delete this.ruleForm.ListEmployeeATID; 
        (this.formModel as any).ListEmployeeATID = [];
        if (departments && departments.length > 0) {
            const filterEmployees = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)));
            if (filterEmployees && filterEmployees.length > 0) {
                this.employeeFullLookupForm = filterEmployees.map(x => ({
                    Index: x.EmployeeATID,
                    NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
                }));
            } else {
                this.employeeFullLookupForm = {};
            }
        } else {
            this.employeeFullLookupForm = Misc.cloneData(this.listAllEmployee).map(x => ({
                Index: x.EmployeeATID,
                NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            }));
        }
    }

    getRequestData(data) {
        const request: Array<EmployeeShiftModel> = [];
        if (data.length > 0) {
            data.forEach(element => {
                this.listColumn.forEach((item, index) => {
                    if (index > 3) {
                        const dateParts = item.dataField.split("/");
                        const date = new Date(+dateParts[2], dateParts[1], +dateParts[0]);
                        const data: EmployeeShiftModel = {
                            ShiftIndex: this.getShiftIndex(element[item.dataField]),
                            DateStr: item.dataField,
                            EmployeeATID: element.EmployeeATID,
                            Date: date,
                            DepartmentIndex: element.DepartmentIndex
                        }
                        request.push(data);
                    }
                });
            });
        }

        return request;
    }

    getShiftIndex(value) {
        return (value != undefined && value != "") ? value : 0;
    }

    async doDelete() {
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
        // console.log('this.filterModel',this.filterModel)
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



    handleWorkingInfoChange(workingInfo: any) {
        this.listWorkingStatus = workingInfo;
        //this.getEmployees();
    }

    focus(x) {
        var theField = eval('this.$refs.' + x)
        theField.focus()
    }

    Edit() {
        this.isEdit = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (obj.length === 1) {
            this.showDialog = true;
            this.ruleForm = obj[0];
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }

    handleSelectionChange(obj) {
        this.rowsObj = obj
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
    listExcelFunction = ['AddExcel', 'ExportTemplateWithData'];

    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }


    UploadDataFromExcel() {
        this.importErrorMessage = "";
        const data = this.cast(
            Object.assign(
                {
                    Data: this.activeObj,
                },
            )
        );
        console.log("UploadDataFromExcel ~ data:", data)
        taEmployeeShiftApi.ImportExcelEmployeeShift(data).then((res) => {
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
                this.importErrorMessage = this.$t('ImportScheduleDepartmentErrorMessage') + res.data.toString() + " " + this.$t('Row');
                //// console.log("Import error, show popup import error file download")
                this.showOrHideImportError(true);
            }
        });

    }

    downloadFile(filePath) {
        var link = document.createElement('a');
        link.href = filePath;
        link.download = filePath.substr(filePath.lastIndexOf('/') + 1);
        link.click();
    }

    onChangeDepartmentFilter(departments) {
		this.filterModel.EmployeeATIDs = [];
		if (departments && departments.length > 0) {
            const filterEmployees = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)));
            if(filterEmployees && filterEmployees.length > 0){
                this.employeeFullLookupForm = filterEmployees.map(x => ({
                    Index: x.EmployeeATID,
                    NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
                }));
            }else{
                this.employeeFullLookupForm = {};
            }
		} else {
			this.employeeFullLookupForm = Misc.cloneData(this.listAllEmployee).map(x => ({
				Index: x.EmployeeATID,
				NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
			}));
		}
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
            // this.exportInfoShift();
            this.AddOrDeleteFromExcel('add');
        }
        if (command === 'ExportTemplateWithData') {
            // this.exportInfoShift();
            this.exportDataTemplateImport();
        }
    }
    processFile(e) {
        console.log('eeee', e)
        const wbBuffer = new EXCEL.Workbook();
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
                            range.s.r = 0; // <-- zero-indexed, so setting to 1 will skip row 0
                            workbook.Sheets[sheet]['!ref'] = XLSX.utils.encode_range(range);
                        }
                        var rowObject = XLSX.utils.sheet_to_json(workbook.Sheets[sheet]);
                        // var arr = Array.from(rowObject)
                        // arrData.push(arr)
                        arrData.push(Array.from(rowObject));
                    });
                };
                this.dataAddExcel = arrData;
                console.log("TARegisterEmployeeShift ~ processFile ~ dataAddExcel:", this.dataAddExcel)
                fileReader.readAsBinaryString(file);
                (document.getElementById("fileUploadScheduleDepartment") as any).value = null;
            }
        }
    }

    //#endregion
    // isReading = false;  isComplete = false;
    // async importFileV1(e: any) {  
    //     this.$emit('update:activeObj', []);

    //     this.fileList = [e];
    //     this.isReading = true;
    //     const files: File = e.raw;
    //     const reader = new FileReader();

    //     reader.onload = async (m: any) => {
    //       const data = m.target.result;

    //       const endFile = files.name.split('.')[1];

    //       if (!['xlsx', 'dat', 'xls'].includes(endFile)) {
    //         this.$alert(this.$t('resources.file-import-error') as string, {
    //           type: 'error',
    //         });
    //         this.isComplete = false;
    //         this.isReading = false;
    //       }
    //       await this.readExcelV1(data);


    //   }
    async importFile(e: any) {
        this.fileList = [e];
        const files: File = e.raw;
        console.log("importFileV1 ~ files:", files)
        const reader = new FileReader();
        console.log("importFileV1 ~ reader:", reader)

        reader.onload = async (m: any) => {
            console.log("reader.onload= ~ m:", m)
            const data = m.target.result;
            console.log("reader.onload= ~ data:", data)

            const endFile = files.name.split('.')[1];
            console.log('0000', e)
            if (!['xlsx', 'dat', 'xls'].includes(endFile)) {
                this.$alert(this.$t('resources.file-import-error') as string, {
                    type: 'error',
                });
            }
            console.log('1111', e)
            await this.readExcelV1(data);
        };
    }

    async importFileV1(e: any) {
        console.log('importFileV1', e)
        this.$emit('update:activeObj', []);

        this.fileList = [e];
        this.isReading = true;
        const files: File = e.raw;
        const reader = new FileReader();
        console.log("ImportComponentComponent ~ importFileV1 ~ reader:", reader)

        reader.onload = async (m: any) => {
            const data = m.target.result;
            console.log("ImportComponentComponent ~ reader.onload= ~ data:", data)

            const endFile = files.name.split('.')[1];

            if (!['xlsx', 'dat', 'xls'].includes(endFile)) {
                this.$alert(this.$t('resources.file-import-error') as string, {
                    type: 'error',
                });
                this.isComplete = false;
                this.isReading = false;
            }

            if (this.allowDatFile && files.name.endsWith('.dat')) {
                this.$emit('datDataChange', this, data);
                this.isReading = false;
            } else {
                await this.readExcelV1(data);
            }
        };

        reader.onloadend = () => {
        };

        if (!isNullOrUndefined(files)) {
            if (this.allowDatFile && files.name.endsWith('.dat')) {
                reader.readAsText(files);
            } else {
                reader.readAsArrayBuffer(files);
            }
        }
    }

    getImageRefBase64(
        lstImages: EXCEL.Media[],
        sheetImages: Array<{
            type: 'image';
            imageId: string;
            range: EXCEL.ImageRange;
        }>,
        row: number,
        col: number
    ) {
        const imageRef = sheetImages.find((img) => img.range.tl.nativeCol === col && img.range.tl.nativeRow === row - 1);

        if (!isNullOrUndefined(imageRef)) {
            const image = lstImages.find((x) => (x as any).index === imageRef.imageId);

            if (!isNullOrUndefined(image)) {
                return Misc.arrayBufferToBase64(image.buffer);
            }
        }

        return '';
    }

    async readExcelV1(bstr: any) {
        console.log("ImportComponentComponent ~ readExcelV1 ~ bstr:", bstr)
        const columnFile = [];
        const wbBuffer = new EXCEL.Workbook();

        const rawExcelData = [];
        const rawArr = await wbBuffer.xlsx.load(bstr).then((wb) => {
            const sheetName = wb.worksheets[this.sheetIndex];
            console.log("ImportComponentComponent ~ rawArr ~ sheetName:", sheetName)
            this.$emit('worksheetChange', wb.worksheets);
            const ws = wb.getWorksheet(sheetName.name);
            const rows = [];
            const lstImages = wb.model.media.filter((x) => x.type === 'image');
            const sheetImages = ws.getImages();
            ws.eachRow((row, rowIndex) => {
                if (this.fieldNotNull.length) {
                    let i = false;

                    this.fieldNotNull.forEach((index) => {
                        console.log("ImportComponentComponent ~ this.fieldNotNull.forEach ~ index:", index)
                        if (isNullOrUndefined(row.values[index]) && rowIndex > this.titleRow + 1) {
                            i = true;
                        }
                    });
                    if (i) {
                        return;
                    }
                }

                rawExcelData.push(row.values);
                if (rowIndex === this.titleRow + 1) {
                    row.eachCell((c) => {
                        columnFile.push(c.value);
                    });
                }
                console.log("rawExcelData", rawExcelData)
                console.log('this.columns', this.columns)
                // this.handleData(rawArr);
                const rowValue = this.columns.map((col, colIndex) => {
                    if (col.dataType === 'image' && rowIndex > this.fromRow) {
                        return this.getImageRefBase64(lstImages, sheetImages, rowIndex, colIndex);
                    } else {
                        const value = row.getCell(colIndex + 1);
                        if (
                            ['date', 'datetime'].includes(col.dataType) &&
                            typeof value.value === 'object' &&
                            col.type === 'object' &&
                            !isNullOrUndefined(value.value)
                        ) {
                            const d = moment(value.value as any);

                            const fArr = `${col.format || ''}`.split(' ').map((e, i) => (i === 0 ? e.toUpperCase() : e));
                            const fm = fArr.join(' ');

                            if (col.dataType === 'datetime') {
                                const t = d.utcOffset(0);
                                const is = t.toDate().toISOString().split('.')[0];

                                return moment(is).toDate();
                            }

                            return d.format(fm);
                        }

                        if (typeof value.result === 'object') {
                            const errResult: any = value.result;

                            return errResult.error;
                        }

                        if (!isNullOrUndefined(value.result)) {
                            return value.result;
                        }

                        return value.value || null;
                    }
                });

                rows.push(rowValue);
            });
            console.log("rawArr ~ rows:", rows)
            return rows;
        });

        console.log("ImportComponentComponent ~ readExcelV1 ~ rawArr:", rawArr)
        this.handleData(rawArr);
        if (this.customDataChange) {
            this.handleData(rawArr);
            this.isComplete = false;
            this.isReading = false;
            return;
        }


        this.dateFormat =
            this.dateFormatPos.length > 1
                ? ((rawArr[this.dateFormatPos[0]][this.dateFormatPos[1]] as string) || 'DD/MM/YYYY').toUpperCase()
                : 'DD/MM/YYYY';
        this.dateFormat = this.dateFormat.replace('DD', 'dd').replace('YYYY', 'yyyy');
        const err = !this.columns.every((e, i) => {
            const rt = (e.label || '').trim().unSign() === (rawArr[this.titleRow][i] || '').trim().unSign();

            if (!rt) {
                `column '${(rawArr[this.titleRow][i] || '').trim()}' does not match '${(e.label || '').trim()}'`;
            }

            return rt;
        });

        if (err) {
            columnFile.filter((x) => x).map((x) => ({ label: x, prop: '' }));

            if (this.invalidTemplateOverride) {
                this.invalidTemplateOverride(rawExcelData, this);
            } else {
                this.$alert(this.$t('resources.excel-format-import-error-bbbbbb') as string, {
                    type: 'error',
                });
                this.$emit('update:activeObj', []);
            }

            this.isComplete = false;
            this.isReading = false;
            return;
        }

        const raw = rawArr.splice(this.fromRow);
        const data = raw.map((e, x) => this.mapRow(e, x)).filter((e) => Object.keys(e).length !== 0);

        this.$emit('update:activeObj', data);
        this.isComplete = false;
        this.isReading = false;
        this.$emit('import-complete', rawExcelData);
    }

    validateDate(str: string, type?: any) {
        if (type === 'object') {
            return str;
        }

        if (!(str || '').toString().trim()) {
            return true;
        }

        if (typeof str === 'object') {
            return true;
        }

        let strArr = str.split('/');

        if (type === 'month') {
            strArr = ['01'].concat(strArr);
        }

        const arr = strArr.map((e) => +e);

        if (
            arr.length !== 3 ||
            strArr[0].length > 2 ||
            strArr[1].length > 2 ||
            strArr[2].length !== 4 ||
            arr.some((x) => isNaN(x)) ||
            arr[0] < 1 ||
            arr[0] > 31 ||
            arr[1] < 1 ||
            arr[1] > 12 ||
            arr[2] < 1900 ||
            arr[2] > 9999
        ) {
            return false;
        }

        const d = new Date(arr[2], arr[1] - 1, arr[0]);

        if (d.getDate() !== arr[0] || d.getMonth() + 1 !== arr[1] || d.getFullYear() !== arr[2]) {
            return false;
        }

        return d;
    }

    mapRow(e: any, idx: number) {
        const temp: any = {};

        this.columns.forEach((c, index) => {
            e[index] = (isNullOrUndefined(e[index]) ? '' : e[index]).toString().trim();
            switch (c.dataType) {
                case 'date': {
                    temp[c.prop] =
                        e[index] && this.validateDate(e[index], c.type)
                            ? this.validateDate(e[index], c.type)
                            : (() => {
                                if (e[index]) {
                                    temp[`raw${c.prop}`] = e[index];
                                }

                                return null;
                            })();
                    break;
                }

                case 'datetime': {
                    let valid = true;

                    if (c.type !== 'object') {
                        valid = moment(e[index], c.format).isValid();
                    }

                    temp[c.prop] = valid
                        ? (() => {
                            if (c.type === 'object') {
                                return moment(e[index]).toDate();
                            }

                            const dt = moment(e[index], c.format).toDate();

                            const rt = new Date();

                            rt.setDate(1);
                            rt.setMonth(0);
                            rt.setHours(dt.getHours());
                            rt.setMinutes(dt.getMinutes());
                            return rt;
                        })()
                        : (() => {
                            if (e[index]) {
                                temp[`raw${c.prop}`] = e[index];
                            }

                            return null;
                        })();
                    break;
                }

                case 'time': {
                    temp[c.prop] = moment(`1900-01-01T${e[index]}`).format('HH:mm:ss');
                    break;
                }

                case 'number': {
                    temp[c.prop] =
                        !isNaN(e[index]) && !isNullOrUndefined(e[index]) && e[index] !== '' ? +e[index] : e[index];
                    break;
                }

                default: {
                    temp[c.prop] = e[index];
                    break;
                }
            }

            if (this.mapOption[c.prop] && this.mapOption[c.prop].length > 0) {
                const old = temp[c.prop];

                this.mapOption[c.prop].forEach((mapObj) => {
                    //   if (('' + (temp[c.prop] || '')).toLowerCase().equal ('' + mapObj.from)) {
                    //     temp[c.prop] = mapObj.to;
                    //   }
                });
                if (typeof this.mapOption[c.prop][0].to === 'boolean' && old === temp[c.prop]) {
                    temp[c.prop] = false;
                }
            }
        });
        temp.ID = idx;
        temp.errors = [];
        Object.keys(this.defaultRowValue).forEach((key) => {
            temp[key] = temp[key] || this.defaultRowValue[key];
        });
        return temp;
    }


    handleData(raw) {
        console.log("TESTTTT", raw)
        this.columns = (raw[2] || []).map((e, i) => {
            if (i === 0) {
                return {
                    prop: 'EmployeeATID',
                    label: e,
                };
            }
            if (i === 1) {
                return {
                    prop: 'EmployeeCode',
                    label: e,
                };
            }
            if (i === 2) {
                return {
                    prop: 'Employee',
                    label: e,
                };
            }
            if (i === 3) {
                return {
                    prop: 'Department',
                    label: e,
                };
            }
            else {
                return {
                    prop: moment(`${raw[1][1]}-${raw[1][0]}-${raw[1][2]}`)
                        .add(i - 4, 'days')
                        .format('YYYY-MM-DD'),
                    label: moment(`${raw[1][1]}-${raw[1][0]}-${raw[1][2]}`)
                        .add(i - 4, 'days')
                        .format('DD-MM-YYYY'),
                };
            }
        });
        console.log("this.columns", this.columns)
        const err = this.columns.length !== 35 || this.columns.some((e) => e.prop === 'Invalid date');

        if (err) {
            this.$alert(this.$t('TheFileContentIsNotInTheCorrectFormat') as string, {
                type: 'error',
            });
            this.activeObj = [];
            this.columns = [];
            return;
        }
        const data = raw
            .splice(4)
            .map((e, x) => {
                const temp: any = {};

                this.columns.forEach((c, index) => {
                    temp[c.prop] = e[index];
                });

                temp.ID = x;
                temp.errors = [];
                return temp;
            })
            .filter((e) => Object.keys(e).length !== 0);
        console.log("hdata", data)
        this.activeObj = data;

    }

    cast(d) {
        console.log("cast ~ d:", d)
        const temp: any[] = [];
        d.Data.forEach((e) => {
            const tempData: any = {
                EmployeeATID: e.EmployeeATID,
                EmployeeCode: e.EmployeeCode,
                Employee: e.Employee,
                Department: e.Department,
                DailyShifts: [],
            };
            this.columns.slice(4).forEach((c) => {
                tempData.DailyShifts.push({
                    Date: new Date(c.prop),
                    ShiftValue: e[c.prop],
                });
            });
            if (tempData.DailyShifts.length > 0) {
                temp.push(tempData);
            }
        });
        console.log('temp', temp)
        return temp.length === 0
            ? null
            : {
                Data: temp,
            };

    }
}
