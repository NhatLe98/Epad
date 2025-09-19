import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { groupDeviceApi } from '@/$api/group-device-api';
import { isNullOrUndefined } from 'util';
import { AccessedGroupModel, accessedGroupApi } from "@/$api/gc-accessed-group-api";

@Component({
    name: "accessed-group",
    components: { HeaderComponent, DataTableFunctionComponent, DataTableComponent }
})
export default class AccessedGroup extends Mixins(ComponentBase) {
    columns = [];
    rowsObj = [];
    isEdit = false;
    showDialog = false;
    rules = null;
    accessedGroupModel: AccessedGroupModel = {
        Index: 0,
        Name: '',
        NameInEng: '',
        GeneralAccessRuleIndex: '',
        // ParkingLotRuleIndex: '',
        Description: '',
        // AreaGroupParentIndex: null
    };
    listGeneralAccessRules = [];
    listParkingLotRules = [];
    page = 1;

    async beforeMount() {
        this.CreateColumns();
        await this.getGeneralAccessRules();
        await this.getParkingLotRules();
        this.Reset();
        this.CreateRules();
    }
    CreateRules() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputAccessedGroupName'),
                    trigger: 'blur',
                },
            ],
            // ParkingLotRuleIndex: [
            //     {
            //         required: true,
            //         message: this.$t('PleaseInputGeneralAccessRule'),
            //         trigger: 'blur',
            //     },
            // ],
            GeneralAccessRuleIndex: [
                {
                    required: true,
                    message: this.$t('PleaseInputGeneralAccessRule'),
                    trigger: 'blur',
                },
            ],
        }
    }
    CreateColumns() {
        this.columns = [
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
                prop: 'GeneralAccessRuleName',
                label: 'GeneralAccessRules',
                minWidth: 150,
                display: true
            },
            {
                prop: 'GeneralAccessRuleLineNameList',
                label: 'LineAllowAccess',
                minWidth: 150,
                display: true,
                dataType: "listString"
            },
            {
                prop: 'IsDriverDefaultGroupName',
                label: 'DriverDefaultGroup',
                minWidth: 150,
                display: true,
               
            },
            {
                prop: 'IsGuestDefaultGroupName',
                label: 'GuestDefaultGroup',
                minWidth: 150,
                display: true,
               
            },
            {
                prop: 'Description',
                label: 'Description',
                minWidth: 150,
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
            this.accessedGroupModel = obj[0];
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
                accessedGroupApi.DeleteAccessedGroup(listIndex).then((res: any) => {
                    (this.$refs.accessedGroupTable as any).getTableData(this.page, null, null);
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$deleteSuccess();
                    }
                })
                .catch(() => { });
            });
        }
    }
    async ConfirmClick() {
        (this.$refs.accessedGroupModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            if (this.isEdit == false) {
                await accessedGroupApi.AddAccessedGroup(this.accessedGroupModel).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            else {
                await accessedGroupApi.UpdateAccessedGroup(this.accessedGroupModel).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            (this.$refs.accessedGroupTable as any).getTableData(this.page, null, null);
        });
    }

    async getGeneralAccessRules() {
        await accessedGroupApi.GetDataRulesGeneralAccess().then((res: any) => {
            if (res.status == 200) {
                this.listGeneralAccessRules = res.data;
            }
        });
    }

    async getParkingLotRules() {
        await accessedGroupApi.GetDataRulesParkingLot().then((res: any) => {
            if (res.status == 200) {
                this.listParkingLotRules = res.data;
            }
        });
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        return await accessedGroupApi.GetAccessedGroup(page, filter, pageSize).then((res) => {
            return {
                data: (res.data as any).data,
                total: (res.data as any).total,
            };
        });
    }
    Cancel() {
        this.showDialog = false;
        this.Reset();
    }
    Reset() {
        this.accessedGroupModel = {
            Index: 0,
            Name: '',
            NameInEng: '',
            GeneralAccessRuleIndex: '',
            Description: '',
            // AreaGroupParentIndex: null
        };
    }
}