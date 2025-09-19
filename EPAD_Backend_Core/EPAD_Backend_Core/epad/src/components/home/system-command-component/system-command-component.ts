import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import { throws } from "assert";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from "@/components/home/data-table-component/data-table-function-component.vue";
import axios from "axios";
import { systemCommandApi, AddedParam, IC_SystemCommandDTO } from "@/$api/system-command-api";
import moment from "moment";
import { isNullOrUndefined } from 'util';
import { fHMS } from "@/utils/datetime-utils";
import CommandDeleteConfirmDialog from './command-delete-confirm-dialog.vue';
@Component({
    name: "system-command",
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent, CommandDeleteConfirmDialog }
})
export default class SystemCommandComponent extends Vue {
    columns = [];
    fromTime = moment(new Date()).format("YYYY-MM-DD 00:00:00");
    toTime = moment(new Date()).format("YYYY-MM-DD 23:59:59");
    data: any = [];
    rowsObj: any = [];
    loading = false;
    addedParams: Array<AddedParam> = [];
    page = 1;

    setColumn() {
        this.columns = [
            {
                prop: "UpdatedUser",
                label: "UpdatedUser",
                minWidth: "120",
                display: true
            },
            {
                prop: "CommandName",
                label: "CommandName",
                minWidth: "160",
                display: true
            },
            {
                prop: "DeviceName",
                label: "AliasName",
                minWidth: "120",
                display: true
            },
            {
                prop: "SerialNumber",
                label: "SerialNumber",
                minWidth: "120",
                display: true
            },
            {
                prop: "SystemCommandStatus",
                label: "SystemCommandStatus",
                minWidth: "110",
                display: true
            },
            {
                prop: "CreatedDate",
                label: "CreatedDate",
                minWidth: "120",
                display: true
            },
            {
                prop: "ExcutedTime",
                label: "ExcutedTime",
                minWidth: "120",
                display: true
            },
            {
                prop: 'TotalExecutingTime',
                label: 'ExecutingTime',
                width: "100",
                display: true,
            },
            {
                prop: 'NumSuccess',
                label: 'Success',
                minWidth: '120',
                display: true,
            },
            {
                prop: 'NumFailure',
                label: 'Failure',
                minWidth: '120',
                display: true,
            },
            {
                prop: 'Error',
                label: 'Error',
                minWidth: '120',
                display: true,
            }
        ];
    }
    mounted() {
        this.setColumn();
        this.updateFunctionBarCSS();
    }

    updateFunctionBarCSS() {
        //// Get all child in custom function bar
        const component1 = document.querySelector('.system-command__custom-function-bar');  
        // console.log(component1.childNodes)
        const component2 = document.querySelector('.system-command__data-table'); 
        const childNodes = Array.from(component1.childNodes);
        const component4 = document.querySelector('.system-command__custom-button-bar');
        // console.log(component4.childNodes)
        const component3 = document.querySelector('.system-command__data-table-function');
        Array.from(component4.childNodes).forEach((element, index) => {
            // component3.append(element);
            component3.insertBefore(element, component3.childNodes[index + 1])
        }); 
        childNodes.push(component3);
        //// Insert all child in custom function bar to after filter bar of table
        childNodes.forEach((element, index) => {
            component2.insertBefore(element, component2.childNodes[index + 1]);
        }); 

    }

    fetchAndSetData() {
        (this.$refs.table as any).getTableData(1);
    }

    async getData({ page, filter, sortParams, pageSize }) {
        if (this.fromTime != null) {
            this.fromTime = moment(new Date(moment(this.fromTime).format('YYYY-MM-DD'))).format("YYYY-MM-DD 00:00:00");
        }
        if (this.toTime != null) {
            this.toTime = moment(new Date(moment(this.toTime).format('YYYY-MM-DD'))).format("YYYY-MM-DD 23:59:59");
        }

        this.addedParams = [];
        if (!isNullOrUndefined(filter) && filter != '') {
            this.addedParams.push({ Key: "Filter", Value: filter });
        }
        this.addedParams.push({ Key: "PageIndex", Value: page });
        this.addedParams.push({ Key: "FromDate", Value: this.fromTime });
        this.addedParams.push({ Key: "ToDate", Value: this.toTime });
        this.addedParams.push({ Key: "PageSize", Value: pageSize });
        //var filter = JSON.parse(JSON.stringify(this.addedParams));
        var filters = JSON.stringify(this.addedParams);
        return await systemCommandApi
            .GetPage(filters)
            .then((res: any) => {
                let listData = res.data;
                listData.data.forEach((systemCommand) => {
                    let numSuccess = systemCommand.IC_Audits[0]?.NumSuccess;
                    let numFailure = systemCommand.IC_Audits[0]?.NumFailure;
                    /**
                     * Một số SystemCommand không có IC_Audit để lưu trữ dữ liệu thành công/ thất bại 
                     * thì lấy trong systemCommand.ParamBody để xem đó là dữ liệu gì.
                     */
                    const users = systemCommand.ParamBody?.ListUsers ?? [];
                    const noDataAboutSuccessFailureOnAudit = !systemCommand.IC_Audits.length
                        || (!systemCommand.IC_Audits[0].NumSuccess && !systemCommand.IC_Audits[0].NumFailure);
                    if (noDataAboutSuccessFailureOnAudit && users.length > 0) {
                        if (systemCommand.Status === 'Completed') {
                            numSuccess = users.length;
                        } else if (systemCommand.Status === 'Error') {
                            numFailure = users.length;
                        }
                    }
                    systemCommand.NumSuccess = numSuccess;
                    systemCommand.NumFailure = numFailure;
                    if (systemCommand.Excuted && systemCommand.ExcutedTime) {
                        systemCommand.TotalExecutingTime = fHMS(new Date(systemCommand.ExcutedTime), new Date(systemCommand.UpdatedDate));
                    }
                    systemCommand.CreatedDate = (isNullOrUndefined(systemCommand.CreatedDate) == false) ? moment(systemCommand.CreatedDate).format('DD-MM-YYYY HH:mm:ss') : '';
                    if(systemCommand.ExcutedTime){
                        systemCommand.ExcutedTime = (isNullOrUndefined(systemCommand.ExcutedTime) == false) ? moment(systemCommand.ExcutedTime).format('DD-MM-YYYY HH:mm:ss') : '';
                    }
                });
                listData.data = listData.data.map(e => ({ ...e, CommandName: this.$t(e.CommandName) }));
                return {
                    data: listData.data,
                    total: listData.total,
                };
            });

    }

    async View() {

        if (Date.parse(this.fromTime) > Date.parse(this.toTime)) {
            this.$alert(
                this.$t("PleaseCheckTheCondition").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
            return;
        } else {
            this.page = 1;
            (this.$refs.table as any).getTableData(this.page, null, null);
        }

    }

    async ResetCaching() {

        return await systemCommandApi.ReloadCaching()
            .then((res: any) => {
                this.$alert(this.$t('ResetCachingSuccess').toString(), null, null);
            }).catch(() => {
                this.$alert(this.$t('SystemCommandNotFound').toString(), null, null);
            });

    }

    showDeleteConfirmDialog = false;
    async Delete() {
        const obj: Array<IC_SystemCommandDTO> = [...this.rowsObj];// = JSON.parse(JSON.stringify(this.rowsObj));

        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            this.showDeleteConfirmDialog = true;
        }
    }

    async renewCommands() {
        const obj: any[] = [...this.rowsObj];// = JSON.parse(JSON.stringify(this.selectedSystemCommands));

        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('Chọn 1 lệnh cần tạo lại').toString());
        } else if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('Chưa hỗ trợ tạo lại lệnh trên nhiều dòng').toString());
        } else if (!obj.every(cmd => cmd.Status == 'Unexecuted' || cmd.Status == 'Processing')) {
            this.$alertSaveError(null, null, null, this.$t('Chỉ tạo lại được cho những lệnh chưa hoàn thành').toString());
        } else {

            this.$confirm("Bạn có muốn tạo lại lệnh đã chọn không?", "Xác nhận tạo lại lệnh").then(async () => {
                this.$notify({
                    type: 'info',
                    title: 'Thông báo từ thiết bị',
                    dangerouslyUseHTMLString: true,
                    message: "Đang tạo lại lệnh, vui lòng chờ.",
                    customClass: 'notify-content',
                    duration: 3000
                });
                await systemCommandApi.renewCommandAsync(obj[0].Index).then(() => {
                    this.$notify({
                        type: 'success',
                        title: 'Thông báo từ thiết bị',
                        dangerouslyUseHTMLString: true,
                        message: "Tạo lại lệnh thành công",
                        customClass: 'notify-content',
                        duration: 3000
                    });
                    this.fetchAndSetData();
                }).catch(() => {
                    this.$notify({
                        type: 'error',
                        title: 'Thông báo từ thiết bị',
                        dangerouslyUseHTMLString: true,
                        message: "Tạo lại lệnh không thành công",
                        customClass: 'notify-content',
                        duration: 8000
                    });
                });
            }).catch();
        }
    }
}
