import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { userTypeApi, HR_UserType } from '@/$api/user-type-api';
import { commandApi } from '@/$api/command-api';
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { IC_LocationOperator, locationOperatorApi } from '@/$api/ic-location-operator-api';
@Component({
    name: 'LocationOperator',
    components: {
        HeaderComponent,
        DataTableComponent,
        DataTableFunctionComponent
    },
})
export default class LocationOperatorComponent extends Mixins(ComponentBase) {
    page = 1;
    showDialog = false;
    showMessage = false;
    checked = false;
    columns = [];
    rowsObj = [];
    isEdit = false;
    listExcelFunction = [];
    ruleForm: IC_LocationOperator = {
        Name: '',
        Department: ''
    };
    rules: any = {};

    beforeMount() {
        this.initRule();
        this.columns = [
            {
                prop: 'Name',
                label: 'ActivityName',
                minWidth: 100,
                display: true
            },
            {
                prop: 'Department',
                label: 'Part',
                minWidth: 60,
                display: true
            }
        ];
    }

    initRule() {
        this.rules = {
            Name: [
            {
                    required: true,
                    message: this.$t('PleaseInputActivity'),
                    trigger: 'blur',
                },
            ],
            Department: [
                {
                    required: true,
                    message: this.$t('PleaseInputPart'),
                    trigger: 'blur',
                },
            ]
          
        };
    }
    mounted() {

    }

    displayPopupInsert() {
        this.showDialog = false;
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        return await locationOperatorApi.GetLocationOperatorAtPage(page, filter, pageSize).then((res) => {
            const { data } = res as any;
            return {
                data: data.data,
                total: data.total,
            };
        });
    }

    reset() {
        const obj: IC_LocationOperator = {};
        this.ruleForm = obj;
    }

    Insert() {
        this.showDialog = true;
        this.isEdit = false;
        this.reset();
    }

    async Submit() {
        (this.$refs.ruleForm as any).validate(async (valid) => {
            if (!valid) return;
            else {
                if (this.isEdit == true) {
                    return await locationOperatorApi.UpdateLocationOperator(this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                        }
                    });
                } else {
                    return await locationOperatorApi
                        .AddLocationOperator(this.ruleForm)
                        .then((res) => {
                            (this.$refs.table as any).getTableData(this.page, null, null);
                            this.showDialog = false;
                            this.reset();
                            if (!isNullOrUndefined(res.status) && res.status === 200) {
                                this.$saveSuccess();
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
            this.ruleForm = obj[0];
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }
    Restart() {
        const obj: HR_UserType[] = JSON.parse(JSON.stringify(this.rowsObj));

        const arrId: Array<Number> = new Array<Number>();
        obj.forEach((element) => {
            arrId.push(element.Index);
        });
        this.showMessage = true;

        //commandApi.RestartUserType(arrId).then((res: any) => { });
    }

    async Delete() {
        const obj: HR_UserType[] = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            let msg = obj.map(function (element) {
                return element.Name + ',';
            });

            this.$confirmDeleteNew(msg.join(' ')).then(async () => {
                await locationOperatorApi
                    .DeleteLocationOperator(obj)
                    .then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$deleteSuccess();
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
        var ref = <ElForm>this.$refs.ruleForm;
        ref.resetFields();
        this.showDialog = false;
    }
}
