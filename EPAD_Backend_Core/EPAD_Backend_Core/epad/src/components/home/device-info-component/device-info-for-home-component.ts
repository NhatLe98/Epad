import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import axios from "axios";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { apilink_GetDeviceInfo } from "../../../constant/variable";
import { deviceApi, IC_Device } from "@/$api/device-api";
import { commandApi, CommandRequest } from "@/$api/command-api";
import { Form as ElForm } from 'element-ui'
import moment from "moment";
import { isNullOrUndefined } from 'util';
import DeviceProcessingStatusComponent from '@/components/app-component/device-info-table/device-processing-status-component.vue';
import DeviceLicenseRenderer from '@/components/app-component/device-info-table/device-license-renderer.vue';
import { SignalRConnection } from "@/startup/hubConnection";

@Component({
    name: "device-info-for-home",
    components: { HeaderComponent, DataTableComponent, DeviceProcessingStatusComponent, DeviceLicenseRenderer }
})
export default class DeviceInfoForHomeComponent extends Mixins(ComponentBase) {
    page = 1;
    pageSize = 100000;
    loading = false;
    devices: IC_Device[] = [];
    intervalRefreshData = null;

    columnDefs = [
        {
            field: 'index',
            sortable: true,
            headerName: '#',
            width: 50,
        },
        {
            field: "SerialNumber",
            headerName: this.$t("SerialNumber"),
            minWidth: 120,
            sortable: true,
        },
        {
            field: "AliasName",
            headerName: this.$t("AliasName"),
            minWidth: 160,
            sortable: true,
        },
        {
            field: "IPAddress",
            headerName: this.$t("IPAddress"),
            minWidth: 130,
            sortable: true, 
        },
        {
            field: "HardWareLicense",
            headerName: this.$t("HardWareLicense"),
            minWidth: 200,
            sortable: true,
            cellRenderer: 'DeviceLicenseRenderer',
        },
        {
            field: "Status",
            headerName: this.$t("Status"),
            minWidth: 200,
            cellRenderer: 'DeviceProcessingStatusComponent',
            sortable: true,
            flex: 1
        },
    ];

    mounted() {
        this.fetchData().then(() => {
            this.fetchProcessingStatus();
        });
        this.intervalRefreshData = setInterval(() => {
            this.refreshDataTableAsync();
        }, 20000);
    }

    destroyed() {
        clearInterval(this.intervalRefreshData);
    }

    fetchProcessingStatus() {
        this.devices.filter(d => d.Status !== 'Offline').forEach((device) => {
            SignalRConnection.connection.invoke('SendAll', `currentStatus_${device.SerialNumber}`, '');
        });
    }

    async refreshDataTableAsync() {
        const res = await deviceApi.GetDeviceAtPage(this.page, undefined, this.pageSize);
        res.data.data.forEach((d,idx) => {
            Object.assign(this.devices[idx], d);
        });
        (this.$refs.visualizeTable as any)?.gridApi?.refreshCells({
            force: false,
            suppressFlash: true,
        });
    }

    async fetchData() {
        this.loading = true;
        try {
            const res = await deviceApi.GetDeviceAtPage(this.page, undefined, this.pageSize);
            const { data } = res as any;
            this.devices = data.data.map((item, idx) => ({
                ...item,
                index: idx + 1,
            }));
        } finally {
            this.loading = false;
        }
    }
}
