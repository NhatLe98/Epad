import { Component, Vue,Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from "@/components/home/data-table-component/data-table-function-component.vue";
import { employeeTransferApi, IC_EmployeeTransfer } from '../../../$api/employee-transfer-api';
import moment from "moment";
import { isNullOrUndefined, isBuffer } from "util";
import { Form as ElForm } from "element-ui";

@Component({
    name: "approve-change-department",
    components: {
        HeaderComponent, DataTableComponent, DataTableFunctionComponent
    }
})
export default class ApproveChangeDepartmentComponent extends Mixins(ComponentBase) {
    // Component variables and objects
    fromDate = moment(new Date()).format("YYYY-MM-DD");
    toDate = moment(new Date()).format("YYYY-MM-DD");
    nowDate = moment(new Date()).format("YYYY-MM-DD");
    loading = false;
    page = 1;
    expandedKey = ["1"];
    showDialog = false;
    isApproveAction = false;
    rowsObj = [];
    columns = [];
    ArrEmployeeATID = [];
    transferNow = false;
    pageSize=50;
    //ruleForm: IC_EmployeeTransfer = {
    //    EmployeeATID: "",
    //    NewDepartment: null,
    //    FromTime: null,
    //    ToTime: null,
    //    IsFromTime: "",
    //    IsToTime: "",
    //    OldDepartment: null,
    //    RemoveFromOldDepartment: false,
    //    AddOnNewDepartment: false,
    //    IsSync: null,
    //    Description: "",
    //    TemporaryTransfer: false,
    //    TransferNow : false
    //};

    // Default Component Function from VueJS
    beforeCreate() { }
    created() { }
    beforeMount() {
        this.setColumns()    
    }
    mounted() {
        this.updateFunctionBarCSS();
    };

    updateFunctionBarCSS() {
        //// Get all child in custom function bar
        const component1 = document.querySelector('.approve-change-department__custom-function-bar');  
        // console.log(component1.childNodes)
        const component2 = document.querySelector('.approve-change-department__data-table'); 
        console.log(component2.childNodes)
        let childNodes = Array.from(component1.childNodes);
        // const component3 = document.querySelector('.history-user__data-table-function');
        // childNodes.push(component3);
        const component5 = document.querySelector('.approve-change-department__data-table-function');
        (component5 as HTMLElement).style.width = "100%";
        (component5 as HTMLElement).style.display = "flex";
        (component5 as HTMLElement).style.justifyContent = "flex-end";
        (component5 as HTMLElement).style.float = "right";
        childNodes.push(component5);
        //// Insert all child in custom function bar to after filter bar of table
        childNodes.forEach((element, index) => {
            component2.insertBefore(element, component2.childNodes[index + 1]);
        }); 
        (document.querySelector('.approve-change-department__data-table-function') as HTMLElement).style.height = "0";
    }

    beforeUpdate() { }
    updated() { };
    beforeDestroy() { }
    destroyed() { }

    // Functions are defired by Dev
    setColumns(){
        this.columns = [
            {
                prop: "EmployeeATID",
                label: "EmployeeATID",
                minWidth: "80",
                fixed: true,
                display:true
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
                prop: "Description",
                label: "Description",
                width: "300",
                display: true
            }
        ];
    }
    
    async searchData() {
        if (Date.parse(this.fromDate) > Date.parse(this.toDate)) {
            this.$alert(
                this.$t("PleaseCheckTheCondition").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
        } else {
            this.page = 1;
            return await employeeTransferApi
                .EmployeeTransfer(
                    1,
                    "",
                    moment(this.fromDate).format("YYYY-MM-DD"),
                    moment(this.toDate).format("YYYY-MM-DD"),
                    true,
                    this.pageSize 
                )
                .then((res) => {
                    let { data } = res;
                    return {
                        data: data.data,
                        total: data.total,
                    };
                })
                .then(() => {
                    (this.$refs.table as any).getTableData(
                        this.page,
                        null,
                        null
                    );
                });
        }
    }

    initTableData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        return employeeTransferApi
            .EmployeeTransfer(
                page,
                filter,
                moment(this.fromDate).format("YYYY-MM-DD"),
                moment(this.toDate).format("YYYY-MM-DD"),
                true,
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
    btnshowDialog(x) {
        const objApproves: IC_EmployeeTransfer[] = JSON.parse(
            JSON.stringify(this.rowsObj)
        );
        if (objApproves.length < 1) {
            if (x === 'approve') {
                this.$alert(
                    this.$t("ButtonChooseRowForApprove").toString(),
                    this.$t("Notify").toString(),
                    { type: "warning" }
                );
            } else {
                this.$alert(
                    this.$t("ButtonChooseRowForReject").toString(),
                    this.$t("Notify").toString(),
                    { type: "warning" }
                );
            }
        } else {
            if (x === 'approve')
                this.isApproveAction = true;
            else
                this.isApproveAction = false;
            this.showDialog = true;
        }
    }

    cancelDialog() {
        (this.$refs.table as any).getTableData(this.page, null, null);
        this.showDialog = false;
        this.transferNow = false;
    }

    approveDepartmentTransfer() {
        const objApproves: IC_EmployeeTransfer[] = JSON.parse(
            JSON.stringify(this.rowsObj)
        );
        if (objApproves.length < 1) {
            this.$alert(
                this.$t("ButtonChooseRowForApprove").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
        } else {
            let valid = true;
            objApproves.forEach(item => {
                if (this.transferNow == true) {
                    if (Date.parse(moment(item.FromTime).format("YYYY-MM-DD")) != Date.parse(this.nowDate)) {
                        valid = false;
                        return;
                    }
                }
                // update item value 
                item.Status = 1; // 1 is approve transfer to new deparment
                item.TransferNow = this.transferNow;
            });

            if (!valid) {
                this.$alert(
                    this.$t("PleaseCheckTheCondition").toString(),
                    this.$t("Notify").toString(),
                    { type: "warning" });
            } else {
                employeeTransferApi.ApproveOrRejectEmployeeTransfer(objApproves)
                    .then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                        } else {
                            this.$alertSaveError(null, null, null, this.$t('MSG_SystemError').toString());
                        }
                    })
                    .catch((error) => {
                        this.$alertSaveError(null, null, null, this.$t('MSG_SystemError').toString());
                    });
            }
        }
    }

    rejectDepartmentTransfer() {
        const objRejects: IC_EmployeeTransfer[] = JSON.parse(
            JSON.stringify(this.rowsObj)
        );
        if (objRejects.length < 1) {
            this.$alert(
                this.$t("ButtonChooseRowForReject").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
        } else {
            objRejects.forEach(item => {
                item.Status = 2; // 2 is reject transfer to new department
            });
            employeeTransferApi.ApproveOrRejectEmployeeTransfer(objRejects)
                .then((res) => {
                    (this.$refs.table as any).getTableData(this.page, null, null);
                    this.showDialog = false;
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$saveSuccess();
                    } else {
                        this.$alertSaveError(null, null, null, this.$t('MSG_SystemError').toString());
                    }
                })
                .catch((error) => {
                    this.$alertSaveError(null, null, null, this.$t('MSG_SystemError').toString());
                });

        }
    }
}