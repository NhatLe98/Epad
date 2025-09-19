import { Component, Mixins } from 'vue-property-decorator';
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
import { taShiftApi } from '@/$api/ta-shift-api';
import { ScheduleFixedDepartmentRequest, TA_ScheduleFixedDepartmentDTO, taScheduleFixedByDepartment } from '@/$api/ta-schedule-fixed-department-api';
import TableToolbar from '@/components/home/printer-control/table-toolbar.vue';
import * as XLSX from 'xlsx';
@Component({
	name: 'ta-fixed-department-schedule',
	components: {
		HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectDepartmentTreeComponent, VisualizeTable,
		AppPagination,
		TableToolbar
	},
})


export default class FixedDeparmentSchedule extends Mixins(TabBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isLoading = false;
	EmployeeATIDs = [];
	isEdit = false;
	formModel: TA_ScheduleFixedDepartmentDTO = {
		DepartmentList: [],
		FromDate: new Date,
		ToDate: null,
		Monday: null,
		Tuesday: null,
		Wednesday: null,
		Thursday: null,
		Friday: null,
		Saturday: null,
		Sunday: null,
	};
	ruleObject = {};
	selectedDepartment = [];
	rules: any = {};
	employeeFullLookupTemp = {};
	employeeFullLookup = {};
	listAllEmployee = [];
	shouldResetColumnSortState = false;
	tree = {
		employeeList: [],
		clearable: true,
		defaultExpandAll: false,
		multiple: true,
		placeholder: "",
		disabled: false,
		checkStrictly: true,
		popoverWidth: 400,
		treeData: [],
		treeProps: {
			value: 'ID',
			children: 'ListChildrent',
			label: 'Name',
		},
	}

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
			field: 'DepartmentName',
			headerName: this.$t('Department'),
			pinned: true,
			minWidth: 150,
			sortable: true,
			display: true
		},
		{
			field: 'FromDateFormat',
			headerName: this.$t('AppliedDate'),
			pinned: true,
			minWidth: 150,
			sortable: true,
			display: true
		},
		{
			field: 'ToDateFormat',
			headerName: this.$t('ToDateString'),
			pinned: true,
			minWidth: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('Monday'),
			field: 'MondayShift',
			pinned: false,
			minWidth: 120,
			sortable: true,
			display: true,
		},
		{
			headerName: this.$t('Tuesday'),
			field: 'TuesdayShift',
			pinned: false,
			minWidth: 120,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('Wednesday'),
			field: 'WednesdayShift',
			pinned: false,
			minWidth: 120,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('Thursday'),
			field: 'ThursdayShift',
			pinned: false,
			minWidth: 120,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('Friday'),
			field: 'FridayShift',
			pinned: false,
			minWidth: 120,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('Saturday'),
			field: 'SaturdayShift',
			pinned: false,
			minWidth: 120,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('Sunday'),
			field: 'SundayShift',
			pinned: false,
			minWidth: 120,
			sortable: true,
			display: true
		}
	];
	listShift = [];
	listShiftLookup = {};
	fromDateFilter = new Date;
	toDateFilter = new Date;
	async beforeMount() {
		this.initColumns();
		this.initRule();
		this.LoadDepartmentTree();
		this.getShiftList();
	}
	initRule() {
		this.rules = {
			DepartmentList: [
				{
					required: true,
					message: this.$t('PleaseInputDepartment'),
					trigger: 'change',
				},
			],
			FromDate: [
				{
					required: true,
					message: this.$t('PleaseSelectAppliedDate'),
					trigger: 'blur',
				},
			]
		};
	}

	mounted() {

	}

	initColumns() {


	}


leCommand(command) {
		// if (command === 'AddExcel') {
		// 	this.AddOrDeleteFromExcel('add');
		// }
		// else if (command === 'ExportExcel') {
		// 	// this.ExportToExcel();
		// }
		// else if (command === 'DeleteExcel') {
		// 	this.AddOrDeleteFromExcel('delete');
		// }
	}

	displayPopupInsert() {
		this.showDialog = false;
	}


	onSelectionChange(selectedRows: any[]) {
		// // console.log(selectedRows);
		this.selectedRows = selectedRows;
	}

	onChangeDepartmentFilter(departments) {
		// delete this.formModel.EmployeeATIDs; 
		this.EmployeeATIDs = [];
		if (departments && departments.length > 0) {
			this.employeeFullLookup = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex))).map(x => ({
				Index: x.EmployeeATID,
				NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
			}));
		} else {
			this.employeeFullLookup = Misc.cloneData(this.listAllEmployee).map(x => ({
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

	async getShiftList() {
		await taShiftApi.GetShiftByCompanyIndex().then((res) => {
			if (res.status && res.status == 200 && res.data && (res.data as any).length > 0) {
				this.listShift = Misc.cloneData((res.data as any));
				this.listShiftLookup = res.data;
			}
		});
	}

	exportInfoShift() {
	     taScheduleFixedByDepartment.ExportInfoShift().then(res => {
            console.log(res);
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

	async loadData() {
		if (this.fromDateFilter != null) {
            this.fromDateFilter = new Date(moment(this.fromDateFilter).format('YYYY-MM-DD'));
        }
        // if (this.toDateFilter != null) {
        //     this.toDateFilter = new Date(moment(this.toDateFilter).format('YYYY-MM-DD'));
        // }
        // if (this.fromDateFilter != null && this.toDateFilter != null
        //     && this.fromDateFilter > this.toDateFilter) {
        //     this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
        //     this.isLoading = false;
        //     return;
        // }

		const filter: ScheduleFixedDepartmentRequest = {
			departmentIndexes: this.selectedDepartment,
			fromDate: this.fromDateFilter,
			page: this.page,
			pageSize: this.pageSize
		}
		return taScheduleFixedByDepartment.GetScheduleFixedByDepartmentAtPage(filter).then((res) => {
			const { data } = res as any;
			if(res.status == 200 && data){
                this.dataSource = data.data.map((x, idx) => {
                    return {
                        ...x,
                        index: idx + 1 + (this.page - 1) * this.pageSize
                    }
                });
                this.total = data.total;
            }
		});
	}

	reset() {
		const obj: TA_ScheduleFixedDepartmentDTO = {
			DepartmentList: [],
			FromDate: new Date,
			ToDate: null,
			Monday: null,
			Tuesday: null,
			Wednesday: null,
			Thursday: null,
			Friday: null,
			Saturday: null,
			Sunday: null,
		};
		this.formModel = obj;
	}

	Insert() {
		this.showDialog = true;
		this.isEdit = false;
		this.reset();
	}

	async onSubmitClick() {
		const submitData = Misc.cloneData(this.formModel);
		submitData.FromDate = new Date(
			moment(submitData.FromDate).format("YYYY-MM-DD")
		);
		if(submitData.ToDate){
			submitData.ToDate = new Date(
				moment(submitData.ToDate).format("YYYY-MM-DD")
			);
		}
		(this.$refs.fixedScheduleDepartment as any).validate(async (valid) => {
			if (!valid) return;	
			if (this.isEdit == true) {
					return await taScheduleFixedByDepartment.UpdateScheduleFixedByDepartment(submitData).then((res) => {
						if(res.data != true){
							this.$alert(this.$t("FixedDepartmentScheduleExist").toString() + ": " + `${res.data}` + ". " + this.$t("PleaseCheckAgain").toString(), this.$t("Notify").toString(), { dangerouslyUseHTMLString: true }
							);
						}else{
							this.loadData();
							this.showDialog = false;
							this.reset();
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
							}
						}
					});
				} else {
					return await taScheduleFixedByDepartment
						.AddScheduleFixedByDepartment(submitData)
						.then((res) => {
							if(res.data != true){
								this.$alert(this.$t("FixedDepartmentScheduleExist").toString() + ": " + `${res.data}` + ". " + this.$t("PleaseCheckAgain").toString(), this.$t("Notify").toString(), { dangerouslyUseHTMLString: true }
								);
							}else{
								this.loadData();
								this.showDialog = false;
								this.reset();
								if (!isNullOrUndefined(res.status) && res.status === 200) {
									this.$saveSuccess();
								}
							}
						})
						.catch(() => { });
				}
		});
	}

	Edit() {
		console.log('uow');
		this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));

		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length === 1) {
			this.showDialog = true;
			this.formModel = obj[0];
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}

	view(){
		this.page = 1;
		(this.$refs.scheduleDepartment as any).lPage = this.page;
		this.loadData();
	}

	async Delete() {
		const listIndex = this.selectedRows.map(e => e.Id);
		console.log(listIndex);
		if (listIndex.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			await taScheduleFixedByDepartment
					.DeleteScheduleFixedByDepartment(listIndex)
					.then((res) => {
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$deleteSuccess();
							this.loadData();
							this.selectedRows = [];
							this.$deleteSuccess();
						}
					})
					.catch(() => { })
					.finally(() => { this.showDialogDeleteUser = false; })
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	Cancel() {
		var ref = <ElForm>this.$refs.formModel;
		ref.resetFields();
		this.showDialog = false;
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
    listExcelFunction = ['AddExcel'];
	employeeFullLookupForm = {};
	
    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }

    onCancelClick() {
        (this.$refs.fixedScheduleDepartment as any).resetFields();
        (this.$refs.fixedScheduleDepartment as any).clearValidate();
        this.showDialog = false;
        this.selectedRows = [];
		this.reset();
        (this.formModel as any).FromDate = new Date();
        (this.formModel as any).ToDate = new Date();
        
        this.loadData();
    }

    UploadDataFromExcel() {
        this.importErrorMessage = "";
        var arrData = [];
        var regex = /^\d+$/;
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});
            // if (regex.test(this.dataAddExcel[0][i]["Mã chấm công (*)"]) === false) {
            //     this.$alertSaveError(null, null, null, this.$t("EmployeeATIDOnlyAcceptNumericCharacters").toString()).toString();
            //     return;
            // } else 
            if (this.dataAddExcel[0][i].hasOwnProperty("Phòng ban (*)")) {
                a.DepartmentName = this.dataAddExcel[0][i]["Phòng ban (*)"] + "";
            } else {
                // this.$alertSaveError(null, null, null, this.$t("EmployeeATIDMayNotBeBlank").toString()).toString();
                // return;
                a.DepartmentName = "";
            }
            
            if (this.dataAddExcel[0][i].hasOwnProperty("Ngày áp dụng (*)")) {
                a.FromDateFormat = this.dataAddExcel[0][i]["Ngày áp dụng (*)"] + "";
            } else {
                a.FromDateFormat = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Đến ngày")) {
                a.ToDateFormat = this.dataAddExcel[0][i]["Đến ngày"] + "";
            } else {
                a.ToDateFormat = "";
            }
          
            if (this.dataAddExcel[0][i].hasOwnProperty("Thứ hai")) {
                a.MondayShift = this.dataAddExcel[0][i]["Thứ hai"] + "";
            } else {
                a.MondayShift = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Thứ ba")) {
                a.TuesdayShift = this.dataAddExcel[0][i]["Thứ ba"] + "";
            } else {
                a.TuesdayShift = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Thứ tư")) {
                a.WednesdayShift = this.dataAddExcel[0][i]["Thứ tư"] + "";
            } else {
                a.WednesdayShift = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Thứ năm")) {
                a.ThursdayShift = this.dataAddExcel[0][i]["Thứ năm"] + "";
            } else {
                a.ThursdayShift = "";
            }
			if (this.dataAddExcel[0][i].hasOwnProperty("Thứ sáu")) {
                a.FridayShift = this.dataAddExcel[0][i]["Thứ sáu"] + "";
            } else {
                a.FridayShift = "";
            }
			if (this.dataAddExcel[0][i].hasOwnProperty("Thứ bảy")) {
                a.SaturdayShift = this.dataAddExcel[0][i]["Thứ bảy"] + "";
            } else {
                a.SaturdayShift = "";
            }
			if (this.dataAddExcel[0][i].hasOwnProperty("Chủ nhật")) {
                a.SundayShift = this.dataAddExcel[0][i]["Chủ nhật"] + "";
            } else {
                a.SundayShift = "";
            }
            arrData.push(a);
        }
		console.log('arrDate', arrData)
        taScheduleFixedByDepartment.AddScheduleFixedByDepartmentFromExcel(arrData).then((res) => {
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
			this.exportInfoShift();
            this.AddOrDeleteFromExcel('add');
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
                        if(sheet == "Data"){
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
                fileReader.readAsBinaryString(file);
				(document.getElementById("fileUploadScheduleDepartment") as any).value = null;
            }
        }
    }

    //#endregion

}
