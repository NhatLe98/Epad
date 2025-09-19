import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import * as XLSX from 'xlsx';
import { hrCustomerCardApi, CustomerCardModel } from '@/$api/hr-customer-card-api';
import { isNullOrUndefined } from 'util';
import { deviceApi } from "@/$api/device-api";

@Component({
    name: "customer-card-component",
    components: { HeaderComponent, DataTableFunctionComponent, DataTableComponent }
})
export default class CustomerCard extends Mixins(ComponentBase) {
    columns = [];
    rowsObj = [];
    isEdit = false;
    showDialog = false;
    rules = null;
    customerCardModel: CustomerCardModel = { IsSyncToDevice: true };
    page = 1;
    listExcelFunction = ['AddExcel'];

    showDialogImportError = false;
    isAddFromExcel = false;
    showDialogExcel = false;
    fileName = '';
    dataAddExcel = [];
    importErrorMessage = '';

    prefixID;
    maxLength;

    async beforeMount() {
        this.Reset();
        this.CreateRules();
        this.CreateColumns();
        await hrCustomerCardApi.GetCustomerCardRequirement().then((res: any) => {
            this.prefixID = res.data.PrefixID;
            this.maxLength = res.data.MaxLength;
        });
    }
    CreateRules() {
        this.rules = {
            CardNumber: [
                {
                    required: true,
                    message: this.$t('PleaseInputCardNumber'),
                    trigger: 'change',
                },
                // {
				// 	required: true,
				// 	trigger: 'change',
				// 	validator: (rule, value: string, callback) => {
				// 		if (!this.customerCardModel.CardNumber.startsWith(this.prefixID)) {
				// 			callback(new Error(this.$t('CardNumberMustStartWithChar').toString() + ' ' + this.prefixID));
				// 		} else {
				// 			callback();
				// 		}
				// 	},
				// },
            ],
        }
    }
    CreateColumns() {
        this.columns = [
            {
                prop: 'CardNumber',
                label: 'CardNumber',
                minWidth: 150,
                display: true
            },
            {
                prop: 'CardID',
                label: 'IDOnMachine',
                minWidth: 150,
                display: true
            },
            {
                prop: 'Status',
                label: 'Status',
                minWidth: 150,
                display: true,
                dataType: "cardStatus"
            },
            {
                prop: 'UserCode',
                label: 'UserOrTripCode',
                minWidth: 220,
                display: true
            },
            {
                prop: 'ObjectString',
                label: 'Object',
                minWidth: 150,
                display: true
            },
            {
                prop: 'CardUpdatedDateString',
                label: 'UpdatedDate',
                minWidth: 220,
                display: true
            },
            {
                prop: 'CreatedDateString',
                label: 'InnitiatedDate',
                minWidth: 220,
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
            this.customerCardModel = obj[0];
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }
    async Delete() {
        //console.log(this.rowsObj)
        const listIndex: Array<number> = this.rowsObj.map((item: any) => {
            return item.Index;
        });

        if (listIndex.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            await this.$confirmDelete().then(() => {
                hrCustomerCardApi.DeleteCustomerCard(listIndex).then((res: any) => {
                    (this.$refs.customerCardTable as any).getTableData(this.page, null, null);
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$deleteSuccess();
                    }
                })
                .catch(() => { });
            });
        }
    }
    async ConfirmClick() {
        (this.$refs.customerCardModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            if (this.isEdit == false) {
                await hrCustomerCardApi.AddCustomerCard(this.customerCardModel).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            (this.$refs.customerCardTable as any).getTableData(this.page, null, null);
        });
    }
    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        return await hrCustomerCardApi.GetCustomerCardAtPage( filter, page, pageSize).then((res) => {
            let data = Misc.cloneData((res.data as any).data);
            if(data){
                // console.log(data);
                data.forEach(element => {
                    element.UpdatedDateString = element.UpdatedDate && element.UpdatedDateString != '' 
                    ? moment(element.UpdatedDate).format("DD/MM/YYYY HH:mm:ss") : '';
                    element.CardUpdatedDateString = element.CardUpdatedDate && element.CardUpdatedDateString != '' 
                    ? moment(element.CardUpdatedDate).format("DD/MM/YYYY HH:mm:ss") : '';
                    element.ObjectString = element.Object && element.Object != '' 
                    ? this.$t(element.Object) : '';
                    element.UserCode = (element.Object == 'Driver' || element.Object == 'ExtraDriver') ? element.TripCode : element.UserCode;
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
        this.customerCardModel = {
            IsSyncToDevice: true
        };
    }
    async AddOrDeleteFromExcel(x) {
        if (x === 'add') {
            this.isAddFromExcel = true;
            this.showDialogExcel = true;
            this.fileName = '';
        }else if (x === 'close') {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }

            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            this.showDialogExcel = false;
            this.fileName = '';
        }
    }
    processFile(e) {
        if ((<HTMLInputElement>e.target).files.length > 0) {
            var file = (<HTMLInputElement>e.target).files[0];
            this.fileName = file.name;
            if (!isNullOrUndefined(file)) {
                var fileReader = new FileReader();
                var arrData = [];
                fileReader.onload = function (event) {
                    var data = event.target.result;
                    var workbook = XLSX.read(data, {
                        type: 'binary',
                    });

                    workbook.SheetNames.forEach((sheet) => {
                        var rowObject = XLSX.utils.sheet_to_json(workbook.Sheets[sheet]);
                        // var arr = Array.from(rowObject)
                        // arrData.push(arr)
                        arrData.push(Array.from(rowObject));
                    });
                };
                this.dataAddExcel = arrData;
                fileReader.readAsBinaryString(file);
            }
        }
    }
    async UploadDataFromExcel() {
        this.importErrorMessage = "";
        var arrData = [];
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});

            if (this.dataAddExcel[0][i].hasOwnProperty('Mã thẻ (*)')) {
                a.CardNumber = this.dataAddExcel[0][i]['Mã thẻ (*)'] + '';
            } else {
                a.CardNumber = '';
            }

            if (this.dataAddExcel[0][i].hasOwnProperty('Đồng bộ lên máy')) {
                a.IsSyncToDevice = true;
            } else {
                a.IsSyncToDevice = false;
            }
            
            arrData.push(a);
        }

        await hrCustomerCardApi.AddCustomerCardFromExcel(arrData).then((res: any) => {
            console.log(res.status, res.data)
            if (res.status && res.status == 200) {
                if(res.data && res.data != ''){
                    this.importErrorMessage = this.$t('ImportCustomerCardFromExcelFailed').toString() + ': ' + res.data.toString() + " " + this.$t('card');;
                    this.showOrHideImportError(true);
                }else{
                    this.$saveSuccess();
                }
            }
        })
        .finally(() => {
            this.showDialogExcel = false;
            this.fileName = '';
			this.dataAddExcel = [];
			this.isAddFromExcel = false;
            (this.$refs.customerCardTable as any).getTableData(this.page);
        });
    }
    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }

    async syncDataToDevice(){
        await hrCustomerCardApi.SyncCustomerCardToDevice().then((res: any) => {
            if (!isNullOrUndefined(res.status) && res.status == 200 && res.data) {
                let message = '';
                this.getListIPAddress(res.data.toString()).then((deviceRes: any) => {
                    deviceRes.forEach(element => {
                        message += element.toString() + ": " + this.$t("SendRequestSuccess").toString() + "<br/>";
                    });
                    message = `<p class="notify-content">${message}</p>`;
                    this.$notify({
                        type: 'success',
                        title: 'Thông báo từ thiết bị',
                        dangerouslyUseHTMLString: true,
                        message: message,
                        customClass: 'notify-content',
                        duration: 8000
                    });
                })

            }
        });
    }

    async getListIPAddress(serialNumbers: string) {
        const response = await deviceApi.GetIPAddressBySerialNumbers(serialNumbers);
        return response.data;
    }
}