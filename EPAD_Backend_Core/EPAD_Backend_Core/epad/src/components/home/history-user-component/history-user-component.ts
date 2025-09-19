import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import { throws } from "assert";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from "@/components/home/data-table-component/data-table-function-component.vue";
import axios from "axios";
import { isNullOrUndefined } from 'util';
import { userAccountLogApi } from "@/$api/user-account-log-api";
import { auditApi, AddedParam, IC_Audit } from "@/$api/audit-api";
import moment from "moment";
import { addHours, getEndTimeOfDate, getStartTimeOfDate } from "@/utils/datetime-utils";
@Component({
    name: "history-user",
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent }
})
export default class HistoryUserComponent extends Vue {
    columns = [];
    fromTime = moment(getStartTimeOfDate(new Date())).format("YYYY-MM-DD HH:mm:ss");
    toTime = moment(getEndTimeOfDate(new Date())).format("YYYY-MM-DD HH:mm:ss");
    data: any = [];
    rowsObj: any = [];
    loading = false;
    addedParams: Array<AddedParam> = [];
    //filter = "";
    pageIndex = 1;
    statusOptions = ['Unexecuted', 'Processing', 'Completed', 'Error'];
    filteredStatus: string[] = [];
    setColumn() {
        this.columns = [
            {
                prop: "DateTime",
                label: "Time",
                minWidth: "100",
                display: true
            },
            {
                // prop: 'Name',
                prop: 'UserName',
                label: 'Account',
                minWidth: '100',
                display: true,
            },
            {
                prop: "UserName",
                label: "Email",
                minWidth: "150",
                display: true
            },
            {
                prop: "StateString",
                label: "Action",
                minWidth: "120",
                display: true
            },
            {
                prop: "PageName",
                label: "Screen",
                minWidth: "120",
                display: true
            },
            {
                prop: 'StatusString',
                label: 'Status',
                minWidth: '120',
                display: true,
            },
            {
                prop: 'NumSuccess',
                label: 'NumberOfSuccess',
                minWidth: '120',
                display: true,
            },
            {
                prop: 'NumFailure',
                label: 'NumberOfFailure',
                minWidth: '120',
                display: true,
            },
            {
                prop: "Description",
                label: "Detail",
                minWidth: "120",
                display: true
            }
        ];
    }
    beforeMount(){
        this.setColumn();
    }
    mounted() {
        this.updateFunctionBarCSS();
    }

    updateFunctionBarCSS() {
        //// Get all child in custom function bar
        const component1 = document.querySelector('.history-user__custom-function-bar');  
        // console.log(component1.childNodes)
        const component2 = document.querySelector('.history-user__data-table'); 
        const childNodes = Array.from(component1.childNodes);
        const component3 = document.querySelector('.history-user__data-table-function');
        childNodes.push(component3);
        //// Insert all child in custom function bar to after filter bar of table
        childNodes.forEach((element, index) => {
            component2.insertBefore(element, component2.childNodes[index + 1]);
        }); 

    }

    onChangeFilteredStatus(status: string[]) {
        this.filteredStatus = status;
        this.View();
    }

    async getData({ page, filter, query, sortParams, pageSize }) {
        const GMT = 7;
        this.addedParams = [];
        if (!isNullOrUndefined(filter) && filter != '') {
            this.addedParams.push({ Key: "Filter", Value: filter });
        }
        this.addedParams.push({ Key: "PageIndex", Value: page });
        this.addedParams.push({ Key: "FromDate", Value: addHours(new Date(this.fromTime), GMT) });
        this.addedParams.push({ Key: "ToDate", Value: addHours(new Date(this.toTime), GMT) });
        this.addedParams.push({ Key: 'Status', Value: this.filteredStatus.join(',')});
        var filters = JSON.stringify(this.addedParams);
        return await auditApi
            .GetPage(filters, pageSize)
            .then(res => {
                const { data } = res as any;
                data.data.forEach((item) => {
                    item.DateTime = moment(item.DateTime).format('DD-MM-YYYY HH:mm:ss');
                });
                data.data = data.data.map(e => ({
                    ...e, StateString: this.$t(e.StateString),
                    TableName: this.$t(e.TableName),
                    StatusString: this.$t(e.StatusString),
                    PageName: this.$t(e.PageName ?? e.TableName).toString(),
                }));
                data.data.forEach(element => {
                    if(element.Description != null && element.Description != undefined 
                        && (element.Description as string).includes(':/:')){
                        const detail = this.$t(element.Description.split(':/:')[0],{
                            param: this.$t(element.Description.split(':/:')[1].toString()).toString()
                        })
                        element.Description = detail;
                    }else{
                        element.Description = this.$t(element.Description);
                    }
                });
                return {
                    data: data.data,
                    total: data.total
                };
            });
    }

    async Search({ page, filter, sortParams }) {
        this.loading = true;
        this.addedParams = [];
        if (!isNullOrUndefined(filter) && filter != '') {
            this.addedParams.push({ Key: "Filter", Value: filter });
        }
        this.addedParams.push({ Key: "PageIndex", Value: this.pageIndex });
        this.addedParams.push({ Key: "FromDate", Value: moment(this.fromTime).format("YYYY/MM/DD HH:mm:ss") });
        this.addedParams.push({ Key: "ToDate", Value: moment(this.toTime).format("YYYY/MM/DD HH:mm:ss") });
        return await auditApi
            .PostPage(this.addedParams)
            .then(res => {
                this.loading = false;
                const { data } = res as any;
                return {
                    data: data.data.map(e => ({ ...e, StateString: this.$t(e.StateString), TableName: this.$t(e.TableName), })),
                    total: data.total
                };
            })
            .then(() => {
                (this.$refs.table as any).getTableData(this.pageIndex, null, null);
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
            this.pageIndex = 1;
            (this.$refs.table as any).getTableData(this.pageIndex, null, null);
        }

    }

    async Delete() {
        const obj: Array<IC_Audit> = [...this.rowsObj];// = JSON.parse(JSON.stringify(this.rowsObj));

        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            for (let i = 0; i < obj.length; i++) {
                obj[i].DateTime = null;
            }
            this.$confirmDelete().then(async () => {
                await auditApi.DeleteList(obj)
                    .then((res) => {
                        (this.$refs.table as any).getTableData(this.pageIndex, null, null);

                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$deleteSuccess();
                        } else {
                            this.$alertSaveError(null, null, null, this.$t('SystemCommandNotFound').toString());
                        }
                    })
                    .catch(() => {
                        this.$alert(this.$t('SystemCommandNotFound').toString(), null, null);
                    });
            }).catch();
        }
    }
}
