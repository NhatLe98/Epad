import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { userPrivilegeApi, IC_UserPrivilege } from "@/$api/user-privilege-api";
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from "@/components/home/data-table-component/data-table-function-component.vue";

@Component({
    name: "group-account",
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent }
})
export default class GroupAccountComponent extends Mixins(ComponentBase) {
    page = 1;
    showDialog = false;
    isEdit = false;
    listExcelFunction = [];
    rowsObj = [];
    columns = [];
    dataTable = [];
    rowIndex = -1;
    groupAccountForm: IC_UserPrivilege = {
        Index: null,
        IsAdmin: false,
        IsAdminName: "",
        UseForDefault: false,
        IsUseForDefault: "",
        Name: "",
        Note: "",
        UpdatedDate: "",
        UpdatedUser: ""
    };

    rules: any = {};
    beforeMount() {
        this.setColumns();
        this.initRule();
    }
    initRule() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputGroupAccountName'),
                    trigger: "blur"
                }
            ]
        };
    }
    mounted() {
        
    }

    setColumns() {
        this.columns = [
            {
                prop: "Name",
                label: "GroupName",
                minWidth: "150",
                display: true
            },
            {
                prop: "UseForDefaultName",
                label: "UseForDefault",
                minWidth: "170",
                display: true
            },
            {
                prop: "IsAdminName",
                label: "IsAdmin",
                minWidth: "100",
                display: true
            },
            {
                prop: "Note",
                label: "Note",
                minWidth: "200",
                display: true
            },
            {
                prop: "UpdatedDate",
                label: "UpdatedDate",
                minWidth: "150",
                display: true
            },
            {
                prop: "UpdatedUser",
                label: "UpdatedUser",
                minWidth: "150",
                display: true
            }
        ];
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page
        return await userPrivilegeApi.GetUserPrivilegeAtPage(page, filter, pageSize).then(res => {
            const { data } = res as any;
            this.dataTable = data.data
            return {
                data: data.data,
                total: data.total
            };
        });
    }

    reset() {
        const obj: IC_UserPrivilege = {};
        this.groupAccountForm = obj;
    }

    Insert() {       
        this.showDialog = true;
        this.isEdit = false;
    }

    async Submit() {
        (this.$refs.groupAccountForm as any).validate(async valid => {
            var arr = Array.from(this.dataTable, item => item.Name)
            var index = this.dataTable.indexOf(this.$refs.groupAccountForm)
            if (!valid) return;

            else if (this.isEdit === true && arr.indexOf(this.groupAccountForm.Name) !== -1 && arr.indexOf(this.groupAccountForm.Name) !== this.rowIndex) {
                this.$alertSaveError(null, null, null, this.$t("UserPrivilegeExists").toString());
                return
            }

            else {
                if (this.isEdit == true) {
                    userPrivilegeApi
                        .UpdateUserPrivilege(this.groupAccountForm)
                        .then((res) => {
                            (this.$refs.table as any).getTableData(this.page, null, null);
                            this.showDialog = false;
                            this.rowIndex = -1
                            this.reset();
                            if (!isNullOrUndefined(res.status) && res.status === 200) {
                                this.$saveSuccess();
                            }
                        })
                        .catch(err => {
                            this.$alertSaveError(null, err);
                        });
                } else {
                    userPrivilegeApi
                        .AddUserPrivilege(this.groupAccountForm)
                        .then((res) => {
                            (this.$refs.table as any).getTableData(this.page, null, null);
                            this.showDialog = false;
                            this.rowIndex = -1
                            this.reset();
                            if (!isNullOrUndefined(res.status) && res.status === 200) {
                                this.$saveSuccess();
                            }
                        })
                        .catch(err => {
                            this.$alertSaveError(null, err);
                        });
                }
            }
        });
    }

    Edit() {
        this.isEdit = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t("MSG_SelectOnlyOneRow").toString());
        } else if (obj.length == 1) {
            this.showDialog = true;
            this.groupAccountForm = obj[0];
            let Name = this.groupAccountForm.Name
            let arr = Array.from(this.dataTable, item => item.Name)
            this.rowIndex = arr.indexOf(Name)
        } else {
            this.$alertSaveError(null, null, null, this.$t("ChooseUpdate").toString());
        }
    }

    async Delete() {
        const obj: IC_UserPrivilege[] = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t("ChooseRowForDelete").toString());
        }
        else {
            this.$confirmDelete()
                .then(() => {   
                    userPrivilegeApi.DeleteUserPrivilege(obj)
                        .then(() => {
                            (this.$refs.table as any).getTableData(this.page, null, null);
                            this.$deleteSuccess();
                        })
                        .catch(() => {

                        })
                })
        }

    }
    focus(x) {
        var theField = eval('this.$refs.' + x)
        theField.focus()
    }
    Cancel() {
        // var ref = <ElForm>this.$refs.groupAccountForm;
        // ref.resetFields();
        this.groupAccountForm.IsAdmin = false;
        this.groupAccountForm.UseForDefault = false;
        this.showDialog = false;
        this.rowIndex = -1;
    }
    closeForm(){;
        // var ref = <ElForm>this.$refs.groupAccountForm;
        // ref.resetFields();
        this.groupAccountForm.Name = "";
        this.groupAccountForm.Note = "";
        this.groupAccountForm.IsAdmin = false
        this.groupAccountForm.UseForDefault = false
        this.showDialog = false;
        this.rowIndex = -1;
    }
}
