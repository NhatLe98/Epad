import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { employeeTypesApi, EmployeeTypeModel } from '@/$api/ic-employee-type-api';
import { isNullOrUndefined } from 'util';

@Component({
    name: "employee-type",
    components: { HeaderComponent, DataTableFunctionComponent, DataTableComponent }
})
export default class EmployeeType extends Mixins(ComponentBase) {
    columns = [];
    rowsObj = [];
    isEdit = false;
    showDialog = false;
    rules = null;
    employeeTypeModel: EmployeeTypeModel = null;
    selectedGroupDevice = [];
    listGroupDevice = [];
    page = 1;
    clientName: string = "";
    showTable = true;

    async beforeMount() {
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.clientName = x.ClientName;
        });

        this.Reset();
        this.CreateRules();
        this.CreateColumns();
    }
    CreateRules() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputEmployeeTypeName'),
                    trigger: 'change',
                },
            ],
            Code: [
                {
                    required: true,
                    message: this.$t('PleaseInputEmployeeTypeCode'),
                    trigger: 'change',
                },
            ]
        }
    }
    CreateColumns() {
        this.columns = [
            {
                prop: 'Code',
                label: 'EmployeeTypeCode',
                minWidth: 150,
                display: true
            },
            {
                prop: 'Name',
                label: 'Name',
                minWidth: 150,
                display: true
            },
            {
                prop: 'NameInEng',
                label: 'NameInEng',
                minWidth: 150,
                display: true
            },
            {
                prop: 'IsUsing',
                label: 'IsUsing',
                minWidth: 150,
                display: true
            },
            {
                prop: 'Description',
                label: 'Description',
                minWidth: 150,
                display: true
            },
            {
                prop: 'UpdatedDate',
                label: 'UpdatedDate',
                minWidth: 220,
                display: true
            },
            {
                prop: 'UpdatedUser',
                label: 'UpdatedUser',
                minWidth: 200,
                display: true
            }
        ];
    }

    Insert() {
        this.showDialog = true;
        if (this.isEdit == true) {
            this.Reset();
        }
        this.isEdit = false;

    }
    Edit() {
        this.isEdit = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (obj.length == 1) {
            this.showDialog = true;
            this.employeeTypeModel = obj[0];
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }
    async Delete() {
        const listIndex: Array<number> = this.rowsObj.map((item: any) => {
            return item.Index;
        });

        if (listIndex.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            await this.$confirmDelete().then(() => {
                employeeTypesApi.DeleteEmployeeTypes(listIndex).then((res: any) => {
                    (this.$refs.employeeTypeTable as any).getTableData(this.page, null, null);
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$deleteSuccess();
                    }
                })
                .catch(() => { })
                .finally(() => {
                    this.showTable = false;
                    setTimeout(() => {
                        this.showTable = true;
                    }, 500);
                });
            });
        }
    }
    async ConfirmClick() {
        (this.$refs.employeeTypeModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            if (this.isEdit == false) {
                await employeeTypesApi.AddEmployeeType(this.employeeTypeModel).then((res: any) => {
                    if (res.status && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            else {
                //delete UpdatedDate cause backend cannot retrieve date as string, update it in backend later
                if((this.employeeTypeModel as any).UpdatedDate){
                    delete (this.employeeTypeModel as any).UpdatedDate;
                }
                await employeeTypesApi.UpdateEmployeeType(this.employeeTypeModel).then((res: any) => {
                    if (res.status && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            this.showTable = false;
            setTimeout(() => {
                this.showTable = true;
            }, 500);
        });
    }
    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        return await employeeTypesApi.GetEmployeeTypes(page, filter, pageSize).then((res) => {
            let data = Misc.cloneData((res.data as any).data);
            if(data){
                data.forEach(element => {
                    element.UpdatedDate = moment(element.UpdatedDate).format("DD/MM/YYYY HH:mm:ss");
                });
            }else{
                data = [];
            }
            return {
                data: data,
                total: (res.data as any).total,
            };
        });
    }
    Cancel() {
        this.Reset();
        this.showDialog = false;
    }
    Reset() {
        this.employeeTypeModel = {
            Index: 0,
            Name: '',
            NameInEng: '',
            Code: '',
            IsUsing: false,
            Description: '',
        };
        this.selectedGroupDevice = [];
    }
}