import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { serviceApi, IC_Service } from '@/$api/service-api';
import { commandApi } from '@/$api/command-api';
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import RestartServiceDialog from './restart-service-dialog.vue';
@Component({
    name: 'machine',
    components: {
        HeaderComponent,
        DataTableComponent,
        DataTableFunctionComponent,
        RestartServiceDialog,
    },
})
export default class ServiceComponent extends Mixins(ComponentBase) {
    page = 1;
    showDialog = false;
    showMessage = false;
    checked = false;
    columns = [];
    rowsObj = [];
    isEdit = false;
    listExcelFunction = [];
    ruleForm: IC_Service = {
        Name: '',
        ServiceType: '',
        Description: '',
        ListDeviceSerial: null,
    };
    rules: any = {};

    beforeMount() {
        this.initColumns();
        this.initRule();
    }
    initRule() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputName'),
                    trigger: 'blur',
                },
            ],
            ServiceType: [
                {
                    required: true,
                    message: this.$t('PleaseDoNotLeaveServiceTypeBlank'),
                    trigger: 'blur',
                },
            ],
        };
    }
    mounted() {
       
    }

    initColumns(){
        this.columns = [
            {
                prop: 'Name',
                label: 'ServiceName',
                minWidth: 100,
                display: true
            },
            {
                prop: 'ServiceType',
                label: 'ServiceType',
                minWidth: 300,
                display: true
            },
            {
                prop: 'Description',
                label: 'Description',
                minWidth: 300,
                display: true
            },
        ];
    }

    displayPopupInsert() {
        this.showDialog = false;
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        return await serviceApi.GetServiceAtPage(page, filter, pageSize).then((res) => {
            const { data } = res as any;
            return {
                data: data.data,
                total: data.total,
            };
        });
    }

    reset() {
        const obj: IC_Service = {};
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
                    return await serviceApi.UpdateService(this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                        }
                    });
                } else {
                    return await serviceApi
                        .AddService(this.ruleForm)
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
        const obj: IC_Service[] = JSON.parse(JSON.stringify(this.rowsObj));

        const arrId: Array<Number> = new Array<Number>();
        obj.forEach((element) => {
            arrId.push(element.Index);
        });
        this.showMessage = true;

        commandApi.RestartService(arrId).then((res: any) => { });
    }

    async Delete() {
        const obj: IC_Service[] = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            this.$confirmDelete().then(async () => {
                await serviceApi
                    .DeleteService(obj)
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
