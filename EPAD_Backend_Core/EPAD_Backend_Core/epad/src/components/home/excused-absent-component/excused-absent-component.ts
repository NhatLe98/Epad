import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { departmentApi, IC_Department } from '@/$api/department-api';
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import * as XLSX from 'xlsx';
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { excusedAbsentApi, ExcusedAbsentRequest, HR_ExcusedAbsent } from '@/$api/excused-absent-api';
import { hrUserApi } from '@/$api/hr-user-api';

@Component({
	name: 'excused-absent',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectTreeComponent, SelectDepartmentTreeComponent },
})
export default class ExcusedAbsentComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	disabled = false;
	listExcelFunction = ['AddExcel'];
	isAddFromExcel = false;
    isDeleteFromExcel = false;
    showDialogImportError = false;
    importErrorMessage = "";
	showDialogExcel = false;
	fileName = '';
	dataAddExcel = [];
	ruleForm: HR_ExcusedAbsent = {
		Index: 0,
		EmployeeATID: null,
		EmployeeATIDs: [],
		AbsentDate: new Date(),
		AbsentDateString: null,
		ExcusedAbsentReasonIndex : null,
		Description: '',
	};
	rules: any = {};
	fromDate = moment(new Date()).format("YYYY-MM-DD");
    toDate = moment(new Date()).format("YYYY-MM-DD");
    nowDate = moment(new Date()).format("YYYY-MM-DD");
	selectDepartment = [];
	formDepartment = [];
	listAllEmployee = [];
	employeeFullLookupTemp = {};
	employeeFullLookup = {};
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
	listExcusedAbsentReason = [];
	masterEmployeeFilter = [];

	initRule() {
		this.rules = {
			EmployeeATIDs: [
				{
					required: true,
					message: this.$t('PleaseSelectStudent'),
					trigger: 'blur',
				},
				{
					trigger: 'change',
                    message: this.$t('PleaseSelectStudent'),
                    validator: (rule, value: string, callback) => {
                        if (!value || (value && value.length < 1)) {
                            // console.log("")
                            callback(new Error());
                        }
                        callback();
                    },
				}
			],
			AbsentDate: [
				{
					required: true,
					message: this.$t('PleaseSelectDate'),
					trigger: 'blur',
				},
			],
			ExcusedAbsentReasonIndex: [
				{
					required: true,
					message: this.$t('PleaseSelectReason'),
					trigger: 'blur',
				},
			],
		};
	}

	selectAllEmployeeFilter(value) {
		this.ruleForm.EmployeeATIDs = [...value];
	}

	async beforeMount() {
		this.setColumns();
		this.initRule();
		await this.getExcusedAbsentReasonData();
		await excusedAbsentApi.ExportTemplateExcusedAbsentReason().then((res: any) => {
            // console.log(res);
        })
		await this.LoadDepartmentTree();
		await this.getEmployeesData();
		const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
	}
	mounted() {
		this.updateFunctionBarCSS();
	}

	updateFunctionBarCSS() {
        //// Get all child in custom function bar
        const component1 = document.querySelector('.excused-absent__custom-function-bar');  
        // console.log(component1.childNodes)
        const component2 = document.querySelector('.excused-absent__data-table'); 
        let childNodes = Array.from(component1.childNodes);
        const component5 = document.querySelector('.excused-absent__data-table-function');
        (component5 as HTMLElement).style.width = "100%";
        (component5 as HTMLElement).style.display = "flex";
        (component5 as HTMLElement).style.margin = "0 0 15px 0";
        (component5 as HTMLElement).style.paddingLeft = "24px";
        (component5 as HTMLElement).style.position = "relative";
        (component5 as HTMLElement).style.top = "0";
        // childNodes.push(component5);
        //// Insert all child in custom function bar to after filter bar of table
        childNodes.forEach((element, index) => {
            component2.insertBefore(element, component2.childNodes[index + 1]);
        }); 
		component2.insertBefore(component5, component2.childNodes[0]);
        // (document.querySelector('.excused-absent__data-table-function') as HTMLElement).style.height = "0";
    }

	async getEmployeesData() {
		await hrUserApi.GetAllEmployeeCompactInfoByPermissionImprovePerformance().then((res: any) => {
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

	async getExcusedAbsentReasonData(){
		await excusedAbsentApi.GetExcusedAbsentReason().then((res: any) => {
			if(res && res.data){
				this.listExcusedAbsentReason = res.data;
				// console.log(this.listExcusedAbsentReason)
			}
		});
	}

	onChangeDepartmentFilter(departments) {
		// delete this.ruleForm.EmployeeATIDs; 
		this.ruleForm.EmployeeATIDs = [];
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

	async LoadDepartmentTree() {
        await departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });

    }

	setColumns() {
		this.columns = [
			{
				prop: 'EmployeeATID',
				label: 'ID',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'FullName',
				label: 'FullName',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'DepartmentName',
				label: 'Class',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'AbsentDateString',
				label: 'Day',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'ExcusedAbsentReason',
				label: 'Reason',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'Description',
				label: 'Description',
				minWidth: '180',
				sortable: true,
				display: true
			},
		];
	}

	reset() {
		const obj: HR_ExcusedAbsent = {
			Index: 0,
			EmployeeATID: null,
			EmployeeATIDs: [],
			AbsentDate: new Date(),
			ExcusedAbsentReasonIndex : null,
			Description: '',
		};
		this.ruleForm = obj;
		this.formDepartment = [];
		this.onChangeDepartmentFilter(this.formDepartment);
	}

	handleCommand(command) {
        if (command === 'AddExcel') {
            this.AddOrDeleteFromExcel('add');
        }

    }

	async AddOrDeleteFromExcel(x){
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
		// console.log(e.target.files[0]['type']);
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

	UploadDataFromExcel() {
        this.importErrorMessage = "";
        var arrData = [];
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});
         if (this.dataAddExcel[0][i].hasOwnProperty('Mã ID (*)')) {
                a.EmployeeATID = this.dataAddExcel[0][i]['Mã ID (*)'] + '';
            } else {
                a.EmployeeATID = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Họ tên')) {
                a.FullName = this.dataAddExcel[0][i]['Họ tên'] + '';
            } else {
                a.FullName = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Lớp')) {
                a.Class = this.dataAddExcel[0][i]['Lớp'] + '';
            } else {
                a.Class = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Ngày (*)')) {
                a.AbsentDate = this.dataAddExcel[0][i]['Ngày (*)'] + '';
            } else {
                a.AbsentDate = '';
            }
			if (this.dataAddExcel[0][i].hasOwnProperty('Lý do (*)')) {
                a.ExcusedAbsentReason = this.dataAddExcel[0][i]['Lý do (*)'] + '';
            } else {
                a.ExcusedAbsentReason = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Ghi chú')) {
                a.Description = this.dataAddExcel[0][i]['Ghi chú'] + '';
            } else {
                a.Description = '';
            }
            arrData.push(a);
        }

        excusedAbsentApi.AddExcusedAbsentFromExcel(arrData).then((res) => {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
            }
            (this.$refs.table as any).getTableData(this.page, null, null);
            this.showDialogExcel = false;
            this.fileName = '';
            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
                this.$saveSuccess();
            } else {
                this.importErrorMessage = this.$t('ImportAbsentErrorMessage') + res.data.toString() + " " + this.$t('AbsentInfo');
                this.showOrHideImportError(true);
            }
        });
    }

	showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }

	searchData()
	{
		if (Date.parse(this.fromDate) > Date.parse(this.toDate)) {
            this.$alert(
                this.$t("FromDateCannotLargerThanToDate").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
        } else {
            this.page = 1;
			(this.$refs.table as any).getTableData(this.page);
        }
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		const requestParam: ExcusedAbsentRequest = {
			page: page, filter: filter, limit: pageSize, 
			from: moment(this.fromDate).format("YYYY-MM-DD"), 
			to: moment(this.toDate).format("YYYY-MM-DD"), departments: this.selectDepartment
		};
		return await excusedAbsentApi.GetExcusedAbsentAtPage(requestParam).then((res) => {
			const { data } = res as any;
			data.data.forEach(element => {
				element.AbsentDateString = moment(element.AbsentDate).format("DD-MM-YYYY");
				if(element.ExcusedAbsentReasonIndex && element.ExcusedAbsentReasonIndex > 0){
					element.ExcusedAbsentReason = this.listExcusedAbsentReason
						.find(x => x.Index == element.ExcusedAbsentReasonIndex)?.Name ?? "";
				}
			});
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	Insert() {
		this.reset();
		if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.ruleForm.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => (this.employeeFullLookup as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
		}
		this.showDialog = true;
		this.isEdit = false;
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				this.ruleForm.AbsentDateString = moment(this.ruleForm.AbsentDate).format("YYYY-MM-DD");
				if (this.isEdit == true) {
					excusedAbsentApi.UpdateExcusedAbsent(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
						}
					});
				} else {
					excusedAbsentApi.AddExcusedAbsent(this.ruleForm).then((res) => {
						// console.log(res);
						if(res && res.data && !(res.data as any).Item1){
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							this.reset();
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
							}
						}else if((res.data as any).Item1 && (res.data as any).Item1 == "ExcusedAbsentIsExist"){
							var message = this.$t("StudentAlreadyHaveExcusedAbsent").toString();
							if((res.data as any).Item2 && (res.data as any).Item2.length > 0){
								message += "<br/>";
								(res.data as any).Item2.forEach(element => {
									// console.log(this.employeeFullLookupTemp[element.EmployeeATID])
									message += "- " + element.EmployeeATID + " " 
									+ (this.employeeFullLookupTemp[element.EmployeeATID]?.Name ?? "") + "<br/>";
								});
							}
							const fullMessage = `<p class="notify-content">${message}</p>`
							// this.$notify({
							// 	type: 'warning',
							// 	title: this.$t('Warning').toString(),
							// 	dangerouslyUseHTMLString: true,
							// 	message: message,
							// 	customClass: 'notify-content',
							// 	duration: 8000
							// });
							this.$alert(fullMessage, this.$t('Warning').toString(), {
								confirmButtonText: 'OK',
								dangerouslyUseHTMLString: true
							});
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
			// console.log(this.ruleForm)
			this.ruleForm.EmployeeATIDs = [obj[0].EmployeeATID];
			this.formDepartment = [obj[0].DepartmentIndex];
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}

	async Delete() {
		const obj: number[] = JSON.parse(JSON.stringify(this.rowsObj.map(x => x.Index)));
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(() => {
				excusedAbsentApi
					.DeleteExcusedAbsent(obj)
					.then((res) => {
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$deleteSuccess();
						}
						(this.$refs.table as any).getTableData(this.page, null, null);
					})
					.catch((er) => {
						console.log(er.response.data);
					});
			});
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}
	Cancel() {
		this.reset();
		var ref = <ElForm>this.$refs.ruleForm;
		ref.resetFields();
		this.showDialog = false;
	}
}
