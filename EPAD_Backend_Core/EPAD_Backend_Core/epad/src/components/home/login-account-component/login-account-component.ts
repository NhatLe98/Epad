import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { loginAccountApi, IC_UserAccount } from '@/$api/login-account-api';
import { userPrivilegeApi } from '@/$api/user-privilege-api';
import { isNullOrUndefined } from 'util';
import { Form as ElForm } from 'element-ui';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';

@Component({
    name: 'login-account',
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class LoginAccountComponent extends Mixins(ComponentBase) {
    page = 1;
    showDialog = false;
    columns = [];
    rowsObj = [];
    isEdit = false;
    listExcelFunction = [];
    accountPrivilege: any = [];
    // validEmail = true
    ruleForm: IC_UserAccount = {
        UserName: '',
        Password: '',
        Name: '',
        ResetPasswordCode: '',
        Disabled: false,
        LockTo: null,
        CreatedDate: null,
        UpdatedDate: null,
        UpdatedUser: null,
        AccountPrivilege: null,
        IsAccountPrivilege: '',
        IsLockTo: '',
        IsUpdatedDate: '',
    };

    rules: any = {};

    async beforeMount() {
        this.initColumns();
        this.initRule();
    }
    initRule() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputNameAccount'),
                    trigger: 'blur',
                },
            ],
            UserName: [
                {
                    required: true,
                    message: this.$t('PleaseInputAccount'),
                    trigger: 'blur',
                },
                {
                    message: this.$t('AccountMustBeInEmailFormat'),
                    validator: (rule, value: string, callback) => {
                        var regex = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
                        if (isNullOrUndefined(value) === false && regex.test(value) === false) {
                            callback(new Error());
                        } else {
                            callback();
                        }
                    },
                },
            ],
            Password: [
                {
                    required: true,
                    message: this.$t('PleaseInputPassword'),
                    trigger: 'blur',
                },
                {
                    message: this.$t('PasswordBinding'),
                    validator: (rule, value: string, callback) => {
                        var regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{8,}$/;
                        if (isNullOrUndefined(value) === false && (value.length < 8 || regex.test(value) === false)) {
                            callback(new Error());
                        } else {
                            callback();
                        }
                    },
                },
            ],
        };
    }
    mounted() {
        this.LoadUserPrivilege();
    }

    initColumns(){
        this.columns = [
            {
                prop: 'Name',
                label: 'Name',
                minWidth: '120',
                display: true
            },
            {
                prop: 'UserName',
                label: 'UserName',
                minWidth: '120',
                display: true
            },
            {
                prop: 'IsAccountPrivilege',
                label: 'GroupAccount',
                minWidth: '120',
                display: true
            },
            /*{
        prop: "IsLockTo",
        label: "LockTo",
        minWidth: "120"
      },
      {
        prop: "Disabled",
        label: "Disabled",
        minWidth: "80",
        sortable: true
      },*/
            {
                prop: 'UpdatedUser',
                label: 'UpdatedUser',
                minWidth: '120',
            },
            {
                prop: 'IsUpdatedDate',
                label: 'UpdatedDate',
                minWidth: '120',
            },
        ];
    }

    reset() {
        const obj: IC_UserAccount = {};
        this.ruleForm = obj;
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        return await loginAccountApi.GetUserAccountAtPage(page, filter, pageSize).then((res) => {
            const { data } = res as any;
            return {
                data: data.data,
                total: data.total,
            };
        });
    }

    Insert() {
        this.reset();
        this.showDialog = true;
        this.isEdit = false;
    }

    async Submit() {
        (this.$refs.ruleForm as any).validate(async (valid) => {
            if (!valid) return;
            else {
                if (this.isEdit == true) {
                    loginAccountApi.UpdateUserAccount(this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                        }
                    });
                } else {
                    loginAccountApi.AddUserAccount(this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
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
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }

    async Delete() {
        const obj: IC_UserAccount[] = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            this.$confirmDelete().then(() => {
                loginAccountApi
                    .DeleteUserAccount(obj)
                    .then(() => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.$deleteSuccess();
                    })
                    .catch(() => { });
            });
        }
    }

    async LoadUserPrivilege() {
        return await userPrivilegeApi.GetUserPrivilege().then((res) => {
            //this.accountPrivilege = [...JSON.parse(JSON.stringify(res.data))];
            var arr = [...JSON.parse(JSON.stringify(res.data))];
            var arr_1 = [];
            for (let i = 0; i < arr.length; i++) {
                arr_1.push({ value: parseInt(arr[i].value), label: arr[i].label });
            }
            this.accountPrivilege = arr_1;
        });
    }
    focus(x) {
        var theField = eval('this.$refs.' + x);
        theField.focus();
    }
    Cancel() {
        var ref = <ElForm>this.$refs.ruleForm;
        ref.resetFields();
        this.showDialog = false;
    }
}
