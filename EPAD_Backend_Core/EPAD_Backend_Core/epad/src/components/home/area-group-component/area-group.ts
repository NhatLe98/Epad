import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { areaGroupApi, AreaModel } from '@/$api/gc-area-group-api';
import { groupDeviceApi } from '@/$api/group-device-api';
import { isNullOrUndefined } from 'util';

@Component({
    name: "area-group",
    components: { HeaderComponent, DataTableFunctionComponent, DataTableComponent }
})
export default class AreaGroup extends Mixins(ComponentBase) {
    columns = [];
    rowsObj = [];
    isEdit = false;
    showDialog = false;
    rules = null;
    areaGroupModel: AreaModel = {
        Index: 0,
        Name: '',
        NameInEng: '',
        Code: '',
        Description: '',
        // AreaGroupParentIndex: null
    };
    selectedGroupDevice = [];
    listGroupDevice = [];
    page = 1;

    async beforeMount() {
        this.CreateColumns();
        await this.getGroupDevice();
        this.Reset();
        this.CreateRules();
    }
    CreateRules() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputAreaName'),
                    trigger: 'change',
                },
            ],
            Code: [
                {
                    required: true,
                    message: this.$t('AreaCodeIsRequired'),
                    trigger: 'change',
                },
                {
                    message: this.$t('AreaCodeIsDuplicate'),
                    validator: (rule, value: string, callback) => {
                        if (value && value != "") {
                            if (this.isEdit == false) {
                                areaGroupApi.GetAreaGroupByCode(value).then((res: any) => {
                                    if (res.status == 200 && res.data) {
                                        const area = res.data;
                                        if (area != null) {
                                            callback(new Error());
                                        } else {
                                            callback();
                                        }
                                    }else{
                                        callback();
                                    }
                                });                              
                            } else {
                                callback();
                            }
                        }else{
                            callback();
                        }
                    },
                    trigger: 'blur',
                },
            ],
        }
    }
    CreateColumns() {
        this.columns = [
            {
                prop: 'Code',
                label: 'AreaGroupCode',
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
                prop: 'GroupDeviceString',
                label: 'GroupDeviceString',
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
                prop: 'UpdatedDateString',
                label: 'UpdatedDateString',
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
            this.areaGroupModel = obj[0];
            this.selectedGroupDevice = (this.areaGroupModel as any).GroupDevice;
            // console.log(this.areaGroupModel)
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
                areaGroupApi.DeleteAreaGroups(listIndex).then((res: any) => {
                    (this.$refs.areaGroupTable as any).getTableData(this.page, null, null);
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$deleteSuccess();
                    }
                })
                .catch(() => { });
            });
        }
    }
    async ConfirmClick() {
        (this.$refs.areaGroupModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            if (this.isEdit == false) {
                await areaGroupApi.AddAreaGroup(this.areaGroupModel, this.selectedGroupDevice).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            else {
                await areaGroupApi.UpdateAreaGroup(this.areaGroupModel, this.selectedGroupDevice).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            (this.$refs.areaGroupTable as any).getTableData(this.page, null, null);
        });
    }
    async getGroupDevice() {
        await groupDeviceApi.GetGroupDevice().then((res: any) => {
            if (res.status == 200) {
                const arrGroupDevice = res.data;
                for (let i = 0; i < arrGroupDevice.length; i++) {
                    this.listGroupDevice.push({
                        Index: parseInt(arrGroupDevice[i].value),
                        Name: arrGroupDevice[i].label
                    });
                }
            }
        });
    }
    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        return await areaGroupApi.GetAreaGroups(page, filter, pageSize).then((res) => {
            return {
                data: (res.data as any).data,
                total: (res.data as any).total,
            };
        });
    }
    Cancel() {
        this.Reset();
        this.showDialog = false;
    }
    Reset() {
        this.areaGroupModel = {
            Index: 0,
            Name: '',
            NameInEng: '',
            Code: '',
            Description: '',
            // AreaGroupParentIndex: null
        };
        this.selectedGroupDevice = [];
    }
}