import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { trackingSystemLogApi } from "@/$api/tracking-system-log-api";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import moment from "moment";
@Component({
    name: "tracking-system",
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent }
})
export default class TrackingSystemComponent extends Vue {
    columns = [];
    fromTime = moment(new Date()).format("YYYY-MM-DD 00:00:00");
    toTime = moment(new Date()).format("YYYY-MM-DD 23:59:59");
    data: any = [];
    loading = false;
    page = 1;
    setColumn() {
        this.columns = [
            {
                label: "LogTime",
                prop: "TimeString",
                minWidth: "40",
                display: true
            },
            {
                label: "SDKFunction",
                prop: "SDKFunction",
                minWidth: "40",
                sortable: true,
                display: true
            },
            {
                label: "Result",
                prop: "Result",
                minWidth: "50",
                sortable: true,
                display: true
            },
            {
                label: "Data",
                prop: "Data",
                minWidth: "100",
                sortable: true,
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
        const component1 = document.querySelector('.tracking-system__custom-function-bar');  
        // console.log(component1.childNodes)
        const component2 = document.querySelector('.tracking-system__data-table'); 
        const childNodes = Array.from(component1.childNodes);
        // const component3 = document.querySelector('.tracking-system__data-table-function');
        // childNodes.push(component3);
        //// Insert all child in custom function bar to after filter bar of table
        childNodes.forEach((element, index) => {
            component2.insertBefore(element, component2.childNodes[index + 1]);
        }); 

    }

    async getData({ page, filter, sortParams }) {
        this.page = page;
        return await trackingSystemLogApi
            .GetSystemLog(
                page,
                filter,
                moment(this.fromTime).format("DD/MM/YYYY HH:mm:ss"),
                moment(this.toTime).format("DD/MM/YYYY HH:mm:ss")
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
        return await trackingSystemLogApi
            .GetSystemLog(
                1,
                "",
                moment(this.fromTime).format("DD/MM/YYYY HH:mm:ss"),
                moment(this.toTime).format("DD/MM/YYYY HH:mm:ss")
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
