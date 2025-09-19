import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import axios from "axios";
@Component({
    name: "device",
    components: { HeaderComponent, DataTableComponent }
})
export default class DeviceComponent extends Mixins(ComponentBase) {
    input = "";
    data = [];
    datafilter = [];
    columns = [
        {
            prop: "name",
            label: "Name",
            minWidth: "100",
            display: true
        },
        {
            prop: "email",
            label: "Email",
            minWidth: "100",
            display: true
        },
        {
            prop: "salary",
            label: "Salary",
            minWidth: "100",
            display: true
        },
        {
            prop: "gender",
            label: "Gender",
            minWidth: "80",
            display: true
        },
        {
            fixed: "right",
            label: "Operations",
            width: "150",
            display: true
        }
    ];

    getData({ page, query, sortParams }) {
        return axios
            .get("https://vuetable.ratiw.net/api/users", {
                params: {
                    page,
                    sort: sortParams
                }
            })
            .then(response => {
                let { data } = response;
                this.datafilter = data.data;
                return {
                    data: data.data,
                    total: data.total
                };
            });
    }
    handleEdit(row) {
        this.$confirm(`Editing ${row.name}. This won't do anything`).catch(_ => { });
    }
}
