import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { parkingLotsApi, ParkingLotModel } from '@/$api/gc-parking-lot-api';
import { isNullOrUndefined } from 'util';

@Component({
    name: "parking-lot",
    components: { HeaderComponent, DataTableFunctionComponent, DataTableComponent }
})
export default class ParkingLot extends Mixins(ComponentBase) {
    columns = [];
    rowsObj = [];
    isEdit = false;
    showDialog = false;
    rules = null;
    parkingLotModel: ParkingLotModel = null;
    selectedGroupDevice = [];
    listGroupDevice = [];
    page = 1;

    async beforeMount() {
        this.Reset();
        this.CreateRules();
        this.CreateColumns();
    }
    CreateRules() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputParkingLotName'),
                    trigger: 'change',
                },
            ],
            Code: [
                {
                    required: true,
                    message: this.$t('PleaseInputParkingLotCode'),
                    trigger: 'change',
                },
            ],
            Description: [
                {
                    required: true,
                    message: this.$t('PleaseInputDescription'),
                    trigger: 'change',
                },
            ],
        }
    }
    CreateColumns() {
        this.columns = [
            {
                prop: 'Code',
                label: 'ParkingLotCode',
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
                prop: 'Capacity',
                label: 'CapacityNumber',
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
            this.parkingLotModel = obj[0];
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
                parkingLotsApi.DeleteParkingLots(listIndex).then((res: any) => {
                    (this.$refs.parkingLotTable as any).getTableData(this.page, null, null);
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$deleteSuccess();
                    }
                })
                .catch(() => { });
            });
        }
    }
    async ConfirmClick() {
        (this.$refs.parkingLotModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            if (this.isEdit == false) {
                await parkingLotsApi.AddParkingLot(this.parkingLotModel).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            else {
                //delete UpdatedDate cause backend cannot retrieve date as string, update it in backend later
                if((this.parkingLotModel as any).UpdatedDate){
                    delete (this.parkingLotModel as any).UpdatedDate;
                }
                await parkingLotsApi.UpdateParkingLot(this.parkingLotModel).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            (this.$refs.parkingLotTable as any).getTableData(this.page, null, null);
        });
    }
    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        return await parkingLotsApi.GetParkingLots(page, filter, pageSize).then((res) => {
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
        this.parkingLotModel = {
            Index: 0,
            Capacity: 0,
            Name: '',
            NameInEng: '',
            Code: '',
            Description: '',
        };
        this.selectedGroupDevice = [];
    }
}