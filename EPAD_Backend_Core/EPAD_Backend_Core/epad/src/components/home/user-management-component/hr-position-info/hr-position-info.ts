import { Component, Ref, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { hrPositionInfoApi, HR_PositionInfo } from '@/$api/hr-position-info-api';
import { Form as ElForm } from 'element-ui';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import * as XLSX from 'xlsx';
import { isNullOrUndefined } from 'util';

@Component({
    name: 'hr-position-info',
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class HRPositionInfoComponent extends Mixins(ComponentBase) {
    page = 1;
    pageSize = 40;
    showDialog = false;
    checked = false;
    columns = [];
    rowsObj = [];
    devices = [];
    isEdit = false;
    usingBasicMenu: boolean = true;
    listExcelFunction = [];
    dataPositionInfo = [];
    ruleForm: HR_PositionInfo = {
        Name: '',
        Code: '',
        NameInEng: '',
        Description: '',
    };
    checkList = [];

    rules: any = {};

    beforeMount() {
        this.initColumns();
        this.initRule();
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.usingBasicMenu = x.UsingBasicMenu;
        })
    }
    initColumns(){
        this.columns = [
            {
                prop: 'Name',
                label: 'Name',
                minWidth: 120,
                sortable: true,
                fixed: true,
                display: true
            },
            {
                prop: 'Code',
                label: 'PositionCode',
                minWidth: 200,
                sortable: true,
                fixed: true,
                display: true
            },
            {
                prop: 'Description',
                label: 'Description',
                minWidth: 130,
                sortable: true,
                display: true
            }
        ];
    }
    initRule() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputPositionName'),
                    trigger: 'blur',
                },
            ],
            Code: [
                {
                    required: true,
                    message: this.$t('PleaseInputPositionCode'),
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

    Reset() {
        const obj: HR_PositionInfo = {};
        this.ruleForm = obj;
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        
        return await hrPositionInfoApi.GetHRPositionInfoAtPage(page, filter, pageSize).then((res) => {
            const { data } = res as any;
            this.dataPositionInfo = data.data;
            return {
                data: this.dataPositionInfo,
                total: data.total,
            };
        });
    }

    Insert() {
        this.Reset();
        this.showDialog = true;
        this.isEdit = false;
    }

    async Submit() {
        (this.$refs.ruleForm as any).validate(async (valid) => {
            if (!valid) return;
            else {
                const classIndex = (this.ruleForm as any).Index;
                this.isLoading = true;
                if (this.isEdit == true) {
                    await hrPositionInfoApi.UpdateHRPositionInfo(classIndex, this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.isLoading = false;
                        this.Reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                        }
                    });
                } else {
                    await hrPositionInfoApi.AddHRPositionInfo(this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.isLoading = false;
                        this.Reset();
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
        const obj = this.rowsObj.map(e => e.Index);
        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            this.$confirmDelete().then(async () => {
                await hrPositionInfoApi.DeleteHRPositionMulti(obj)
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

    Cancel() {
        var ref = <ElForm>this.$refs.ruleForm;
        //ref.resetFields();
        this.showDialog = false;
    }

    focus(x) {
        (this.$refs[x] as any).focus();
    }
}
