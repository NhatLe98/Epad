import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from "@/components/home/data-table-component/data-table-function-component.vue";
import ApprovePopupComponent from "@/components/app-component/approve-popup-component/approve-popup-component.vue";
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';

import moment from "moment";
import { isNullOrUndefined, isBuffer } from "util";
import { employeeInfoApi } from "@/$api/employee-info-api";
import { employeeTransferApi, IC_EmployeeTransfer, ExportEmployeeTransferRequest, AddedParam } from "@/$api/employee-transfer-api";
import { departmentApi } from "@/$api/department-api";
import { Form as ElForm } from "element-ui";
import * as XLSX from "xlsx";
import { workingInfoApi } from '../../../$api/working-info-api';
import * as mime from 'mime-types';

@Component({
    name: "change-department",
    components: {
        HeaderComponent,
        DataTableComponent,
        DataTableFunctionComponent,
        ApprovePopupComponent,
        SelectTreeComponent,
        SelectDepartmentTreeComponent
    },
})
export default class ChangeDepartmentComponent extends Mixins(ComponentBase) {
    fromDate = moment(new Date()).format("YYYY-MM-DD 00:00:00");
    toDate = moment(new Date()).format("YYYY-MM-DD 23:59:59");
    page = 1;
    expandedKey = [-1];
    resultAddExcel = "";
    formExcel = {};
    showDialog = false;
    showDialogExcel = false;
    dataAddExcel = [];
    isEdit = false;
    fileName = "";
    rowsObj = [];
    treeData: any = [];
    comboDepartment: any = [];
    columns = [];
    key: any = [];
    filterTree = "";
    loadingTree = false;
    ArrEmployeeATID = [];
    listExcelFunction = ["AddExcel", "ExportExcel"];
    filter = "";
    addedParams: Array<AddedParam> = [];
    ruleForm: IC_EmployeeTransfer = {
        EmployeeATID: "",
        NewDepartment: null,
        FromTime: null,
        ToTime: null,
        IsFromTime: "",
        IsToTime: "",
        OldDepartment: null,
        RemoveFromOldDepartment: false,
        AddOnNewDepartment: false,
        IsSync: null,
        Description: "",
        TemporaryTransfer: false,
    };
    ruleForm_0: IC_EmployeeTransfer = {
        EmployeeATID: "",
        NewDepartment: null,
        FromTime: null,
        ToTime: null,
        IsFromTime: "",
        IsToTime: "",
        OldDepartment: null,
        RemoveFromOldDepartment: false,
        AddOnNewDepartment: false,
        IsSync: null,
        Description: "",
        TemporaryTransfer: false,
    };
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
    fromDatePickerOptions = {
        disabledDate(date) {
            return date < moment(new Date(), "YYYY-MM-DD").add(-1, 'd');
        }
    };

    arrData: any;
    defaultChecked = [];
    masterEmployeeFilter = [];

    rules: any = {};
    setColumns() {
        this.columns = [
            {
                prop: "EmployeeATID",
                label: "EmployeeATID",
                minWidth: "80",
                fixed: true,
                display: true
            },
            {
                prop: "FullName",
                label: "FullName",
                minWidth: "180",
                fixed: true,
                display: true
            },
            {
                prop: "IsFromTime",
                label: "FromDate",
                minWidth: "220",
                display: true
            },
            {
                prop: "IsToTime",
                label: "ToDate",
                minWidth: "220",
                display: true
            },
            //{
            //    prop: "OldDepartmentName",
            //    label: "OldDepartmentName",
            //    width: "180",
            //},
            {
                prop: "NewDepartmentName",
                label: "NewDepartmentName",
                minWidth: "180",
                display: true
            },
            {
                prop: "TypeTemporaryTransfer",
                label: "TypeTemporaryTransfer",
                minWidth: "180",
                display: true
            },
            {
                prop: "TransferApprovedDate",
                label: "TransferApprovedDate",
                minWidth: "180",
                display: true
            },
            {
                prop: "TransferApprovedUser",
                label: "TransferApprovedUser",
                minWidth: "180",
                display: true
            },
            {
                prop: "TransferApproveStatus",
                label: "TableHeaderTransferApproveStatus",
                minWidth: "180",
                display: true
            },
            {
                prop: "AddOnNewDepartmentName",
                label: "AddOnNewDepartment",
                width: "350",
                display: true
            },
            {
                prop: "RemoveFromOldDepartmentName",
                label: "RemoveFromOldDepartment",
                width: "350",
                display: true
            },
            {
                prop: "Description",
                label: "Description",
                width: "300",
                display: true
            },
        ];
    }

    beforeMount() {
        this.setColumns();
        this.LoadDepartmentTreeEmployee();
        this.loadDepertmentTree();
        this.loadComboDepartment();
        this.initRule();
    }
    initRule() {

        this.rules = {
            NewDepartment: [
                {
                    required: true,
                    message: this.$t(
                        "PleaseSelectDepartmentTransfer"
                    ),
                    trigger: "blur",
                },
            ],
            FromTime: [
                {
                    required: true,
                    message: this.$t("PleaseSelectDayTransfer"),
                    trigger: "blur",

                },
            ],
            ToTime: [
                {
                    required: true,
                    message: this.$t("PleaseSelectDayTransfer"),
                    trigger: "change",
                },
                {
                    trigger: 'change',
                    message: this.$t('PleaseSelectDayTransfer'),
                    validator: (rule, value: string, callback) => {
                        if (/^\d+$/.test(value) === true && isNullOrUndefined(value) === true && this.ruleForm.TemporaryTransfer === true) {
                            callback(new Error());
                        }
                        callback();
                    },
                },
            ],
        };
    }

    mounted() {
        this.updateFunctionBarCSS();
    }

    updateFunctionBarCSS() {
        //// Get all child in custom function bar
        const component1 = document.querySelector('.change-department__custom-function-bar');
        // console.log(component1.childNodes)
        const component2 = document.querySelector('.change-department__data-table');
        let childNodes = Array.from(component1.childNodes);
        // const component3 = document.querySelector('.history-user__data-table-function');
        // childNodes.push(component3);
        const component4 = document.querySelector('.change-department__data-table-function .groupfunction');
        childNodes = childNodes.concat(Array.from(component4.childNodes));
        const component5 = document.querySelector('.change-department__data-table-function .group-btn');
        (component5 as HTMLElement).style.width = "100%";
        (component5 as HTMLElement).style.display = "flex";
        (component5 as HTMLElement).style.justifyContent = "flex-end";
        (component5 as HTMLElement).style.float = "right";
        childNodes.push(component5);
        //// Insert all child in custom function bar to after filter bar of table
        childNodes.forEach((element, index) => {
            component2.insertBefore(element, component2.childNodes[index + 1]);
        });
        (document.querySelector('.change-department__data-table-function') as HTMLElement).style.height = "0";
    }

    filterNode(value, data) {
        if (!value) return true;
        return (data.Name.indexOf(value) !== -1 || (!isNullOrUndefined(data.EmployeeATID) && data.EmployeeATID.indexOf(value) !== -1));

    }
    async filterTreeData() {
        (this.$refs.tree as any).filter(this.filterTree);
    }
    // TODO
    async loadNode(node, resolve) {
        //console.log(node)
        //console.log(this.treeData[0])
        if (node.level === 1) {
            return resolve(this.treeData[0].ListChildrent);
        }
        //let data = this.treeData.filter(e => e.Level === node.level);
        //return resolve(data);
        //resolve(this.treeData[0]);
        //if (node.level > 10) return resolve([]);
        //console.log(this.treeData[0].ListChildrent);
        //setTimeout(() => {
        //    //const data = [{
        //    //    name: 'leaf',
        //    //    leaf: true
        //    //}, {
        //    //    name: 'zone'
        //    //}];
        //    const index = node.level;

        //    if (this.treeData[index].ListChildrent.length > 0) {
        //       // resolve(this.treeData[0].ListChildrent);
        //    }
        //}, 500);
    }

    LoadDepartmentTreeEmployee() {
        departmentApi.GetDepartmentTreeEmployeeScreen("10").then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });

    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        this.filter = filter;
        return employeeTransferApi
            .EmployeeTransfer(
                page,
                filter,
                this.fromDate,
                this.toDate,
                false,
                pageSize
            )
            .then((res) => {
                let { data } = res;
                return {
                    data: data.data,
                    total: data.total,
                };
            });
    }

    async View() {
        if (Date.parse(this.fromDate) > Date.parse(this.toDate)) {
            this.$alert(
                this.$t("PleaseCheckTheCondition").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
        } else {
            this.page = 1;
            (this.$refs.table as any).getTableData(this.page);
            //return await employeeTransferApi
            //    .EmployeeTransfer(
            //        1,
            //        "",
            //        moment(this.fromDate).format("YYYY-MM-DD"),
            //        moment(this.toDate).format("YYYY-MM-DD"),
            //        false
            //    )
            //    .then((res) => {
            //        let { data } = res;
            //        return {
            //            data: data.data,
            //            total: data.total,
            //        };
            //    })
            //    .then(() => {
            //        (this.$refs.table as any).getTableData(this.page);
            //    });
        }
    }

    async Export() {
        if (Date.parse(this.fromDate) > Date.parse(this.toDate)) {
            this.$alert(
                this.$t("PleaseCheckTheCondition").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
        } else {
            const workingInfoIndexArr = [];
            this.rowsObj.forEach(x => {
                workingInfoIndexArr.push(x.WorkingInfoIndex);
            });
            this.addedParams = [];
            if (!isNullOrUndefined(this.filter) && this.filter != '') {
                this.addedParams.push({ Key: "Filter", Value: this.filter });
            }
            this.addedParams.push({ Key: "FromDate", Value: this.fromDate });
            this.addedParams.push({ Key: "ToDate", Value: this.toDate });
            this.addedParams.push({ Key: "IsPenddingApprove", Value: false });
            this.addedParams.push({ Key: "WorkingInfoIndexList", Value: workingInfoIndexArr });
            await employeeTransferApi
                .ExportEmployeeTransfer(this.addedParams)
                .then((res: any) => {
                    // //const filePath = "EmployeeTransfer.xlsx";
                    // //window.open(
                    // //    URL.createObjectURL(
                    // //        Misc.dataURLtoFile(
                    // //            'data:' + mime.lookup(filePath) + ';base64,' + res.data,
                    // //            filePath
                    // //        )
                    // //    ),
                    // //    '_blank'
                    // //);
                    // //window.open(res.data.toString(),'_blank');
                    // this.downloadFile(res.data.toString());

                    const fileURL = window.URL.createObjectURL(new Blob([res.data]));
                    const fileLink = document.createElement("a");

                    fileLink.href = fileURL;
                    fileLink.setAttribute("download", `EmployeeTransfer_${moment(new Date).format('YYYY-MM-DD HH:mm')}.xlsx`);

                    fileLink.click();
                })
        }
    }

    downloadFile(filePath) {
        var link = document.createElement('a');
        link.href = filePath;
        link.download = filePath.substr(filePath.lastIndexOf('/') + 1);
        link.click();
    }

    async loadDepertmentTree() {
        this.loadingTree = true;
        //Don't get info employee in department driver
        return await employeeInfoApi
            .GetEmployeeAsTree(8)
            .then((res) => {
                this.loadingTree = false;
                const data = res.data as any;
                this.treeData = data;
                if(this.treeData){
					this.arrData = this.flattenArray(this.treeData);
					const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
					if(jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0){
						this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
						this.key = this.masterEmployeeFilter;
						this.defaultChecked = this.arrData.filter(x => this.masterEmployeeFilter.includes(x.EmployeeATID))
							?.map(x => x.ID) ?? [];
                        if(this.defaultChecked && this.defaultChecked.length > 0){
                            const result = this.findParentID(this.defaultChecked);
                            if(result && result.length > 0){
                                setTimeout(() => {
                                    const tree = (this.$refs.tree as any);
                                    result.forEach(element => {
                                        // tree.store.nodesMap[element].checked = true;
                                        tree.store.nodesMap[element].expanded = true;
                                        tree.store.nodesMap[element].loaded = true;
                                    });
                                }, 500);
                            }
                        }
					}
				}
                // console.log(this.treeData)
                //this.treeData[0] = this.GetListChildrent(data[0]);
            })
            .catch(() => {
                this.loadingTree = false;
            });
    }

    flattenArray(data, parentId = null, result = []) {
        const cloneData = Misc.cloneData(data);
        cloneData.forEach(item => {
            // Create a copy of the item to avoid mutating the original data
            // const newItem = { ...item };
            const newItem = Misc.cloneData(item);

            // Set the parentIndex property
            newItem.ParentID = parentId;

            // Remove the children property from the new item
            delete newItem.ListChildrent;

            // Add the new item to the result array
            result.push(newItem);

            // Get the current item's index in the result array
            const currentIndex = item.ID;

            // If the item has a children array, recursively flatten it
            if (Array.isArray(item.ListChildrent) && item.ListChildrent.length > 0) {
                this.flattenArray(item.ListChildrent, currentIndex, result);
            }

            // delete item.ListChildrent;
        });

        return result;
    }

    findParentID(arrID){
		let result = [];
		const parentIDs = this.arrData.filter(x => arrID.includes(x.ID))?.map(x => x.ParentID) ?? [];
		if(parentIDs && parentIDs.length > 0){
			result = result.concat(parentIDs);
			if(this.arrData.some(x => parentIDs.includes(x.ID) && x.ParentID)){
				const nestParentIDs = this.findParentID(parentIDs);
				if(nestParentIDs && nestParentIDs.length > 0){
					result = result.concat(nestParentIDs);
				}
			}
		}
		result = [...new Set(result)];
		return result;
	}

    async loadComboDepartment() {
        return await departmentApi.GetAll().then((res) => {
            let a = JSON.parse(JSON.stringify(res.data));
            for (let i = 0; i < a.Value.length; i++) {
                a.Value[i].value = parseInt(a.Value[i].value);
            }
            this.comboDepartment = a.Value;
        });
    }

    displayPopupInsert() {
        this.showDialog = false;
    }

    Insert() {
        if ([...this.key].length === 0) {
            this.$alert(
                this.$t("PleaseChooseTreeEmployee").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
            return false;
        }
        /* Hiện tại cho thêm 1 lượt nhiều nhân viên từ các phòng ban khác nhau */
        // else if([...this.key].length > 1) {
        //   this.$alert(
        //     this.$t("PleaseChooseOnlyOneEmployee").toString(),
        //     this.$t("Notify").toString(),
        //     { type: "warning" }
        //   );
        //   return false;
        // }
        else {
            this.showDialog = true;
            this.isEdit = false;
            //this.reset();
        }
    }
    DisplayWaitingApprove() {
        const approvePopup = this.$refs.approvePopup as any;
        approvePopup.showHideDialog(true);
    }

    async nodeCheck(e) {
        this.loadingEffect(500);
        if (!this.filterTree) {
            this.key = (this.$refs.tree as any)
                .getCheckedNodes()
                .filter((e) => e.Type == 'Employee')
                .map((e) => e.EmployeeATID);
        }
        else {
            this.key = (this.$refs.tree as any)
                .getCheckedNodes()
                .filter((e) => e.Type == 'Employee' && (e.Name.indexOf(this.filterTree) !== -1) || (!isNullOrUndefined(e.EmployeeATID) && e.EmployeeATID.indexOf(this.filterTree) !== -1))
                .map((e) => e.EmployeeATID);
        }

    }

    async Submit() {
        (this.$refs.ruleForm as any).validate(async (valid) => {
            if (!valid) return;
            else {

                if (Date.parse(moment(this.ruleForm.FromTime).format("YYYY-MM-DD")) < Date.parse(moment(new Date()).format("YYYY-MM-DD"))) {
                    this.$alert(
                        this.$t("InvalidDatePleaseInputAgain").toString(),
                        this.$t("Notify").toString(),
                        { type: "warning" }
                    );
                }
                else if (this.ruleForm.TemporaryTransfer == true && this.ruleForm.FromTime > this.ruleForm.ToTime) {
                    this.$alert(
                        this.$t("InvalidDatePleaseInputAgain").toString(),
                        this.$t("Notify").toString(),
                        { type: "warning" }
                    );
                }
                // else if (this.isEdit === true) {
                //   employeeTransferApi
                //     .UpdateEmployeeTransfer(this.ruleForm)
                //     .then((res) => {
                //       (this.$refs.table as any).getTableData(this.page, null, null)
                //       var ref = <ElForm>this.$refs.ruleForm
                //       ref.resetFields()
                //       this.showDialog = false
                //       if(!isNullOrUndefined(res.status) && res.status === 200) {
                //         this.$saveSuccess()
                //       }
                //     })
                // }
                else if (this.isEdit === true) {
                    this.ruleForm.FromTime = new Date(
                        moment(this.ruleForm.FromTime).format("YYYY-MM-DD")
                    );
                    this.ruleForm.ToTime = new Date(
                        moment(this.ruleForm.ToTime).format("YYYY-MM-DD")
                    );
                    var a = Object.assign(
                        {},
                        {
                            EmployeeATID: this.ruleForm_0.EmployeeATID,
                            NewDepartment: this.ruleForm_0.NewDepartment,
                            FromTime: this.ruleForm_0.FromTime,
                            ToTime: this.ruleForm_0.ToTime,
                            IsFromTime: this.ruleForm_0.IsFromTime,
                            IsToTime: this.ruleForm_0.IsToTime,
                            OldDepartment: this.ruleForm_0.OldDepartment,
                            RemoveFromOldDepartment: this.ruleForm_0
                                .RemoveFromOldDepartment,
                            AddOnNewDepartment: this.ruleForm_0
                                .AddOnNewDepartment,
                            IsSync: this.ruleForm_0.IsSync,
                            Description: this.ruleForm_0.Description,
                            TemporaryTrasfer: this.ruleForm_0.TemporaryTransfer,
                        },
                        {}
                    );
                    var b = Object.assign(
                        {},
                        {
                            EmployeeATID: this.ruleForm.EmployeeATID,
                            NewDepartment: this.ruleForm.NewDepartment,
                            FromTime: this.ruleForm.FromTime,
                            ToTime: this.ruleForm.ToTime,
                            IsFromTime: this.ruleForm.IsFromTime,
                            IsToTime: this.ruleForm.IsToTime,
                            OldDepartment: this.ruleForm.OldDepartment,
                            RemoveFromOldDepartment: this.ruleForm
                                .RemoveFromOldDepartment,
                            AddOnNewDepartment: this.ruleForm
                                .AddOnNewDepartment,
                            IsSync: this.ruleForm.IsSync,
                            Description: this.ruleForm.Description,
                            TemporaryTrasfer: this.ruleForm.TemporaryTransfer,
                        },
                        {}
                    );
                    if (b.FromTime > b.ToTime) {
                        this.$alert(
                            this.$t("InvalidDatePleaseInputAgain").toString(),
                            this.$t("Notify").toString(),
                            { type: "warning" }
                        );
                        return;
                    }
                    employeeTransferApi
                        .UpdateEmployeeTransferNew([a, b] as Array<IC_EmployeeTransfer>)
                        .then((res) => {
                            (this.$refs.table as any).getTableData(this.page);
                            var ref = <ElForm>this.$refs.ruleForm;
                            ref.resetFields();
                            this.showDialog = false;
                            if (
                                !isNullOrUndefined(res.status) &&
                                res.status === 200
                            ) {
                                this.$saveSuccess();
                            }
                            this.ruleForm_0 = {};
                        });
                }
                else {
                    if (this.ruleForm.TemporaryTransfer) {

                        this.ruleForm.FromTime = new Date(
                            moment(this.ruleForm.FromTime).format("YYYY-MM-DD")
                        );
                        this.ruleForm.ToTime = new Date(
                            moment(this.ruleForm.ToTime).format("YYYY-MM-DD")
                        );
                    }
                    else {
                        this.ruleForm.FromTime = new Date(
                            moment(this.ruleForm.FromTime).format("YYYY-MM-DD")
                        );
                    }

                    this.ruleForm.EmployeeATID = `${[...this.key][0]}`;
                    this.ArrEmployeeATID = [...this.key];
                    if (this.ruleForm.IsSync === null) {
                        this.ruleForm.IsSync = false;
                    }
                    if (this.ArrEmployeeATID.length === 1) {
                        this.insertDepartmentTransfer();
                    }
                    else if (this.ArrEmployeeATID.length > 1) {
                        this.insertListDepartmentTransfer();
                    }
                }
            }
        });
    }

    insertDepartmentTransfer() {

        employeeTransferApi
            .AddEmployeeTransfer(this.ruleForm)
            .then((res) => {
                this.fileName = "";

                console.log("ChangeDepartmentComponent ~ .then ~ res:", res);
                if (res.status === 200 && res.data) {
                    const msg = res.data;
                    this.$alert(
                        this.$t("MSG_ThereHasBeenInformationAboutQuitting", { data: msg }).toString(),
                        this.$t("Notify").toString(), { dangerouslyUseHTMLString: true }
                    );
                }
                else if (!isNullOrUndefined(res.status) && res.status === 200) {
                    //this.$saveSuccess();
                    (this.$refs.table as any).getTableData(this.page);
                    var ref = <ElForm>this.$refs.ruleForm;
                    ref.resetFields();
                    this.showDialog = false;
                    this.notify(this.$t('Notify').toString(), this.$t('DepartmentTransferNotifySuccess').toString(), 's', 'tr');
                }
                else {
                    this.$alert(this.$t('InvalidDatePleaseInputAgain').toString(), null, null);
                }
            }).catch(() => {
                // this.$alert(this.$t('WorkingInfoIsExists').toString(), null, null);

            });
    }

    insertListDepartmentTransfer() {
        employeeTransferApi
            .AddEmployeesTransfer(
                this.ruleForm,
                this.ArrEmployeeATID
            )
            .then((res) => {

                this.fileName = "";
                if(res.status === 200 && res.data){
                    const msg = res.data;
                    this.$alert(
                        this.$t("MSG_ThereHasBeenInformationAboutQuitting", { data: msg }).toString(),
                        this.$t("Notify").toString(), { dangerouslyUseHTMLString: true }
                    );
                }
                else if (!isNullOrUndefined(res.status) && res.status === 200) {
                    //this.$saveSuccess();
                    (this.$refs.table as any).getTableData(this.page);
                    var ref = <ElForm>this.$refs.ruleForm;
                    ref.resetFields();
                    this.showDialog = false;
                    this.notify(this.$t('Notify').toString(), this.$t('DepartmentTransferNotifySuccess').toString(), 's', 'tr');
                }
                else {
                    this.$alert(this.$t('InvalidDatePleaseInputAgain').toString(), null, null);

                }
            }).catch((error) => {
                this.$alert(this.$t('WorkingInfoIsExists').toString(), null, null);
            });
    }


    Edit() {
        this.isEdit = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length > 1) {
            this.$alert(
                this.$t("MSG_SelectOnlyOneRow").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
        } else if (obj.length == 1) {
            this.showDialog = true;

            this.ruleForm = Object.assign({}, this.ruleForm, obj[0], {});
            this.ruleForm_0 = Object.assign({}, this.ruleForm_0, obj[0], {});
        } else {
            this.$alert(
                this.$t("ChooseUpdate").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
        }
    }

    Delete() {
        const obj: IC_EmployeeTransfer[] = JSON.parse(
            JSON.stringify(this.rowsObj)
        );
        if (obj.length < 1) {
            this.$alert(
                this.$t("ChooseRowForDelete").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
        } else {
            this.$confirmDelete()
                .then(async () => {
                    let allPromise = obj.map((item) => {
                        // return employeeTransferApi.DeleteEmployeeTransfer(item);
                        employeeTransferApi
                            .DeleteEmployeeTransfer(item)
                            .then((res) => {
                                (this.$refs.table as any).getTableData(this.page);
                                if (
                                    !isNullOrUndefined(res.status) &&
                                    res.status === 200
                                ) {
                                    this.$deleteSuccess();
                                }
                            })
                            .catch(() => { });
                    });
                    await Promise.all(allPromise).then((res) => {
                        (this.$refs.table as any).getTableData(this.page);
                        // this.$deleteSuccess();
                    });
                })
                .catch((error) => {
                    this.$deleteError(null, error);
                });
        }
    }

    reset() {
        const obj: IC_EmployeeTransfer = {};
        this.ruleForm = obj;
        // var ref = <ElForm>this.$refs.ruleForm
        // ref.resetFields()
    }

    getIconClass(type, gender) {
        switch (type) {
            case "Company":
                return "el-icon-office-building";
                break;
            case "Department":
                return "el-icon-s-home";
                break;
            case "Employee":
                if (isNullOrUndefined(gender) || gender === "Other") {
                    return "el-icon-s-custom employee-other";
                } else if (gender === "Male") {
                    return "el-icon-s-custom employee-male";
                } else {
                    return "el-icon-s-custom employee-female";
                }
        }
    }
    Cancel() {
        var ref = <ElForm>this.$refs.ruleForm;
        ref.resetFields();
        this.showDialog = false;
        this.fileName = "";
    }
    focus(x) {
        var theField = eval("this.$refs." + x);
        theField.focus();
    }
    loadingEffect(x) {
        const loading = this.$loading({
            lock: true,
            text: "Loading",
            spinner: "el-icon-loading",
            background: "rgba(0, 0, 0, 0.7)",
        });
        setTimeout(() => {
            loading.close();
        }, x);
    }

    handleChange() {
        this.ruleForm.RemoveFromOldDepartment = !this.ruleForm
            .RemoveFromOldDepartment;
    }

    AddOrDeleteFromExcel(x) {
        if (x === "close") {
            this.fileName = '';
            (<HTMLInputElement>document.getElementById("fileUpload")).value =
                "";
            (this.$refs.table as any).getTableData(this.page);

            this.dataAddExcel = [];
            // this.isAddFromExcel = false
            // this.isDeleteFromExcel = false
            this.showDialogExcel = false;
            this.resultAddExcel = "";
        } else if (x === "add") {
            // this.isAddFromExcel = true
            this.showDialogExcel = true;
        }
    }

    processFile(e) {
        console.log(e)
        if ((<HTMLInputElement>e.target).files.length > 0) {
            var file = (<HTMLInputElement>e.target).files[0];
            this.fileName = file.name;
            if (!isNullOrUndefined(file)) {
                var fileReader = new FileReader();
                var arrData = [];
                fileReader.onload = function (event) {
                    var data = event.target.result;
                    var workbook = XLSX.read(data, {
                        type: "binary",
                    });

                    workbook.SheetNames.forEach((sheet) => {
                        var rowObject = XLSX.utils.sheet_to_json(
                            workbook.Sheets[sheet]
                        );
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

    async AddEmployeeTransferFromExcel() {
        console.log(this.dataAddExcel)
        var arrData = [];
        var regex = /^\d+$/;
        var errorMessage = "";
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            console.log(this.dataAddExcel[0])
            let transferObject = Object.assign({});
            if (!this.dataAddExcel[0][i].hasOwnProperty("Mã chấm công (*)")) {
                errorMessage += this.$t("Row") + " " + (i + 2).toString() + ": " + this.$t("EmployeeATIDMayNotBeBlank").toString() + "\n";
            } else if (regex.test(this.dataAddExcel[0][i]["Mã chấm công (*)"]) === false) {
                errorMessage += this.$t("Row") + " " + (i + 2).toString() + ": " + this.$t("EmployeeATIDOnlyAcceptNumericCharacters").toString() + "\n";
            } else if (this.dataAddExcel[0][i].hasOwnProperty("Mã chấm công (*)")) {
                transferObject.EmployeeATID = this.dataAddExcel[0][i]["Mã chấm công (*)"] + "";
            }

            if (this.dataAddExcel[0][i].hasOwnProperty("Phòng ban chuyển đến (*)")) {
                let labelDepartment = this.comboDepartment.map(
                    (item) => item.label
                );
                let regex = /[/]/;
                if (regex.test(this.dataAddExcel[0][i]["Phòng ban chuyển đến (*)"] + "") === false &&
                    labelDepartment.indexOf(this.dataAddExcel[0][i]["Phòng ban chuyển đến (*)"] + "") === -1) {
                    errorMessage += this.$t("Row") + " " + (i + 2).toString() + ": " + this.$t("DepartmentIsNotValid").toString() + "\n";
                } else {
                    transferObject.NewDepartmentName =
                        this.dataAddExcel[0][i]["Phòng ban chuyển đến (*)"] + "";
                }
            } else {
                errorMessage += this.$t("Row") + " " + (i + 2).toString() + ": " + this.$t("DepartmentTransferIsNotEmpty").toString() + "\n";
            }

            if (this.dataAddExcel[0][i].hasOwnProperty("Ngày bắt đầu (*)")) {
                transferObject.FromTime = this.dataAddExcel[0][i]["Ngày bắt đầu (*)"] + "";
            } else {
                errorMessage += this.$t("Row") + " " + (i + 2).toString() + ": " + this.$t("TransferDateMustNotBeBlank").toString() + "\n";
            }

            if (!this.dataAddExcel[0][i].hasOwnProperty("Ngày kết thúc")
                && !this.dataAddExcel[0][i].hasOwnProperty("Điều chuyển luôn")) {
                errorMessage += this.$t("Row") + " " + (i + 2).toString() + ": " + this.$t("TemporaryTransferReturnDateMustNotBeBlank").toString() + "\n";
            } else if (this.dataAddExcel[0][i].hasOwnProperty("Ngày kết thúc")) {
                transferObject.ToTime = this.dataAddExcel[0][i]["Ngày kết thúc"] + "";
            }

            if (this.dataAddExcel[0][i].hasOwnProperty("Điều chuyển luôn")
                && this.dataAddExcel[0][i]["Điều chuyển luôn"] === "x") {
                transferObject.PernamentTransfer = true;
            } else {
                transferObject.PernamentTransfer = false;
            }

            if (this.dataAddExcel[0][i].hasOwnProperty("Xóa thông tin trên máy ở phòng ban cũ") &&
                this.dataAddExcel[0][i]["Xóa thông tin trên máy ở phòng ban cũ"] === "x") {
                transferObject.RemoveFromOldDepartment = true;
            } else {
                transferObject.RemoveFromOldDepartment = false;
            }

            if (this.dataAddExcel[0][i].hasOwnProperty("Ghi thông tin lên máy ở phòng ban mới") &&
                this.dataAddExcel[0][i]["Ghi thông tin lên máy ở phòng ban mới"] === "x") {
                transferObject.AddOnNewDepartment = true;
            } else {
                transferObject.AddOnNewDepartment = false;
            }

            if (this.dataAddExcel[0][i].hasOwnProperty("Diễn giải")) {
                transferObject.Description = this.dataAddExcel[0][i]["Diễn giải"] + "";
            } else {
                transferObject.Description = "";
            }
            arrData.push(transferObject);
        }

        if (errorMessage && errorMessage != "") {
            this.resultAddExcel = errorMessage;
            return;
        }

        await employeeTransferApi
            .AddEmployeesTransferFromExcel(arrData)
            .then((res) => {
                let data = res.data as any;
                if (data === "AddEmployeeTransferSuccess") {
                    this.$saveSuccess();
                } else {
                    if (data.includes(":*:")) {
                        let message = "";
                        const splitData = data.split(":*:");
                        splitData.forEach(x => {
                            if (x.includes(":/:")) {
                                message += this.$t("Row") + " " + (parseInt(x.split(":/:")[0]) + 2).toString() + ": " + this.$t(x.split(":/:")[1]) + "\n";
                            }
                        });
                        this.resultAddExcel = message;
                    } else {
                        this.resultAddExcel = data;
                    }
                }
                // this.isAddFromExcel = false;
            })
            .catch((err) => {
                console.log("err", err);
            });
    }
    GetListChildrent(object) {
        if (
            !Misc.isEmpty(object.ListChildrent) &&
            object.ListChildrent.length > 150
        ) {
            //var arrTemp = [...object.ListChildrent]
            //delete object['ListChildrent']
            var arrTemp = [];
            for (
                let i = 0;
                i < Math.ceil(object.ListChildrent.length / 100);
                i++
            ) {
                let calcFirstNumber = i * 100 + 1;
                let calcLastNumber =
                    (i + 1) * 100 < object.ListChildrent.length
                        ? (i + 1) * 100
                        : object.ListChildrent.length;
                arrTemp.push(
                    Object.assign(
                        {},
                        {
                            Name: calcFirstNumber + "-" + calcLastNumber,
                            ListChildrent: object.ListChildrent.slice(
                                calcFirstNumber - 1,
                                calcLastNumber
                            ),
                        },
                        {}
                    )
                );
            }
            object.ListChildrent = arrTemp;
        }
        if (!Misc.isEmpty(object.ListChildrent)) {
            object.ListChildrent.forEach((item) => {
                this.GetListChildrent(item);
            });
        }
        return object;
    }
}
