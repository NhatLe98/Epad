import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { trackingSystemLogApi } from "@/$api/tracking-system-log-api";
import moment from "moment";
import { trackingIntegrateApi } from "@/$api/tracking-integrate-api";
@Component({
    name: "tracking-integrate",
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent }
})
export default class TrackingIntegrateComponent extends Vue {
    columns = [];
    fromTime = moment(new Date()).format("YYYY-MM-DD 00:00:00");
    toTime = moment(new Date()).format("YYYY-MM-DD 23:59:59");
    data: any = [];
    loading = false;
    page = 1;
    pageSize = 50;
    setColumn() {
        this.columns = [
            {
                label: "JobName",
                prop: "JobName",
                minWidth: "100",
                sortable: true,
                display: true
            },
            {
                label: "LogTime",
                prop: "RunTimeString",
                minWidth: "40",
                display: true
            },
           
            {
                label: "Total",
                prop: "Total",
                minWidth: "40",
                sortable: true,
                display: true
            },
            {
                label: "DataNew",
                prop: "DataNew",
                minWidth: "40",
                display: true
            },
            {
                label: "DataUpdate",
                prop: "DataUpdate",
                minWidth: "40",
                display: true
            },
            {
                label: "Status",
                prop: "IsSuccessString",
                minWidth: "40",
                display: true
            },
            {
                label: "FailReason",
                prop: "Reason",
                minWidth: "70",
                display: true
            }
        ];
    }
    mounted() {
        this.setColumn();
        this.updateFunctionBarCSS();
    }

    updateFunctionBarCSS() {
        //// Get all child in custom function bar
        const component1 = document.querySelector('.tracking-integrate__custom-function-bar');  
        // console.log(component1.childNodes)
        const component2 = document.querySelector('.tracking-integrate__data-table'); 
        const childNodes = Array.from(component1.childNodes);
        // const component3 = document.querySelector('.tracking-integrate__data-table-function');
        // childNodes.push(component3);
        //// Insert all child in custom function bar to after filter bar of table
        childNodes.forEach((element, index) => {
            component2.insertBefore(element, component2.childNodes[index + 1]);
        }); 

    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        this.pageSize = pageSize;
        return await trackingIntegrateApi
            .GetHistoryTrackingIntegrate(
                page,
                filter,
                moment(this.fromTime).format("DD/MM/YYYY HH:mm:ss"),
                moment(this.toTime).format("DD/MM/YYYY HH:mm:ss"),
                pageSize
            )
            .then(res => {
                const { data } = res as any;
                return {
                    data: data.data,
                    total: data.total
                };
            });
    }

    async Search() {
        this.loading = true;
        return await trackingIntegrateApi
            .GetHistoryTrackingIntegrate(
                1,
                "",
                moment(this.fromTime).format("DD/MM/YYYY HH:mm:ss"),
                moment(this.toTime).format("DD/MM/YYYY HH:mm:ss"),
                this.pageSize
            )
            .then(res => {
                this.loading = false;
                const { data } = res as any;
                return {
                    data: data.data,
                    total: data.total
                };
            })
            .then(() => {
                (this.$refs.table as any).getTableData(this.page, null, null);
            });
    }
}
