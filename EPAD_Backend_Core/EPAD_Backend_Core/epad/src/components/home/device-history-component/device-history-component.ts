import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { trackingSystemLogApi } from "@/$api/tracking-system-log-api";
import moment from "moment";
import { trackingIntegrateApi } from "@/$api/tracking-integrate-api";
import { deviceHistoryApi } from "@/$api/device-history-api";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
@Component({
    name: "device-history",
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent }
})
export default class DeviceHistoryComponent extends Vue {
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
                label: "SerialNumber",
                prop: "SerialNumber",
                minWidth: "50",
                sortable: true,
                display: true
            },
            {
                label: "IP",
                prop: "IP",
                minWidth: "40",
                display: true
            },
           
            {
                label: "Date",
                prop: "Date",
                minWidth: "40",
                sortable: true,
                display: true
            },
            {
                label: "Status",
                prop: "Status",
                minWidth: "40",
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
        const component1 = document.querySelector('.device-history__custom-function-bar');  
        // console.log(component1.childNodes)
        const component2 = document.querySelector('.device-history__data-table'); 
        const childNodes = Array.from(component1.childNodes);
        // const component3 = document.querySelector('.device-history__data-table-function');
        // childNodes.push(component3);
        //// Insert all child in custom function bar to after filter bar of table
        childNodes.forEach((element, index) => {
            component2.insertBefore(element, component2.childNodes[index + 1]);
        }); 

    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        this.pageSize = pageSize;
        return await deviceHistoryApi
            .GetDeviceHistory(
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
        return await deviceHistoryApi
            .GetDeviceHistory(
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
