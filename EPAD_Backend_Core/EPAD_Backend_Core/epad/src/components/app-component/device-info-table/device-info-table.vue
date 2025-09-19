<template>
    <div>
        <div class="device-info__toolbar">
            <el-input
                style="width:238px;"
                :placeholder="$t('SearchData')"
                v-model="searchValue"
                @keyup.enter.native="viewData()"
                class="filter-input">
                <i slot="suffix" class="el-icon-search" @click="viewData()"></i>
            </el-input>
            <slot name="toolbarExtend" />
        </div>

        <div style="display: flex; justify-content: end; margin-top: 10px;">
            <TableToolbar 
            :gridColumnConfig.sync="columnDefs" />
        </div>

        <VisualizeTable style="margin-top: 24px; height: calc(100vh - 242px)"
            :columnDefs="columnDefs.filter(x => x.display)"
            :loading="loading"
            :rowData="devices"
            :rowHeight="50"
            :maxHeight="maxHeight"
            :rowBuffer="1000"
            @onSelectionChange="onSelectionChange"
            :isKeepIndexAscending="true"
            :shouldResetColumnSortState="shouldResetColumnSortState"
            ref="visualizeTable"
        />
        <AppPagination 
            :page.sync="page"
            :pageSize.sync="pageSize"
            :total="totalDevice"
            :getData="getData"
            :offlineDevices="offlineDevices"
            :onlineDevices="onlineDevices"
            :isShowTotalOnOffDevice="true"/>
    </div>
</template>
<script lang="ts">
import { deviceApi, IC_Device } from '@/$api/device-api';
import AppPagination from '@/components/app-component/app-pagination/app-pagination.vue';
import VisualizeTable from '@/components/app-component/visualize-table/visualize-table.vue';
import { SignalRConnection } from '@/startup/hubConnection';
import { HubConnection } from '@microsoft/signalr';
import Vue from 'vue';
import DeviceLicenseRenderer from './device-license-renderer.vue';
import DeviceProcessingStatusComponent from './device-processing-status-component.vue';
import TableToolbar from '@/components/home/printer-control/table-toolbar.vue';

export default Vue.extend({
    name: "DeviceInfoTable",
    components: {
        VisualizeTable,
        DeviceProcessingStatusComponent,
        AppPagination,
        DeviceLicenseRenderer,
        TableToolbar
    },
    props: {
        showCheckbox: {
            type: Boolean,
            default: true,
        },
        selectedRows: {
            type: Array,
            required: false,
        },
        refreshData: {
            type: Number,
            required: false,
        },
    },
    watch: {
        refreshData() {
            this.refreshDataTableAsync();
        },
    },
    data() {
        const hubConnection: HubConnection = null;
        const devices: IC_Device[] = [];
        const fullColumnDefs = [
            {
                field: 'index',
                sortable: true,
                pinned: true,
                headerName: '#',
                width: 80,
                checkboxSelection: true, 
                headerCheckboxSelection: true,
                headerCheckboxSelectionFilteredOnly: true,
                display:true
            },
            {
                field: "SerialNumber",
                headerName: this.$t("SerialNumber"),
                width: 120,
                pinned: true,
                sortable: true,
                display:true
            },
            {
                field: "AliasName",
                headerName: this.$t("AliasName"),
                width: 160,
                pinned: true,
                sortable: true,
                display:true
            },
            {
                field: "IPAddress",
                headerName: this.$t("IPAddress"),
                width: 130,
                pinned: true,
                sortable: true, 
                display:true
            },
            {
                field: "LastConnection",
                headerName: this.$t("LastConnection"),
                minWidth: 200,
                sortable: true,
                display:true
            },
            {
                field: "Status",
                headerName: this.$t("Status"),
                minWidth: 200,
                cellRenderer: 'DeviceProcessingStatusComponent',
                sortable: true,
                display:true
            },
            {
                field: "HardWareLicense",
                headerName: this.$t("HardWareLicense"),
                minWidth: 200,
                sortable: true,
                cellRenderer: 'DeviceLicenseRenderer',
                display:true
            },
            {
                field: "UserCount",
                headerName: this.$t("UserCount"),
                minWidth: 200,
                sortable: true,
                display:true
            },
            {
                field: "FingerCount",
                headerName: this.$t("FingerCount"),
                minWidth: 200,
                sortable: true, 
                display:true
            },
            {
                field: "AttendanceLogCount",
                headerName: this.$t("AttendanceLogCount"),
                minWidth: 220,
                sortable: true,
                display:true
            },
            {
                field: "FaceCount",
                headerName: this.$t("FaceCount"),
                minWidth: 200,
                sortable: true,
                display:true
            },
            {
                field: "AdminCount",
                headerName: this.$t("AdminCount"),
                minWidth: 200,  
                sortable: true,
                display:true
            },
        ];
        const columnDefs = [...fullColumnDefs];
        return {
            fullColumnDefs,
            columnDefs,
            selectedDevices: [],
            shouldResetColumnSortState: false,
            maxHeight: 500,
            devices,
            loading: false,
            searchValue: '',
            
            page: 1,
            pageSize: 50,
            totalDevice: 0,

            hubConnection,
            autoRefreshInterval: null,
            autoRefreshTotalOnOffInterval: null,
            offlineDevices: 0,
            onlineDevices: 0,
            refreshDataTimeout: null,

        }
    },
    methods: {
        fetchProcessingStatus() {
            this.devices.filter(d => d.Status !== 'Offline').forEach((device) => {
                SignalRConnection.connection.invoke('SendAll', `currentStatus_${device.SerialNumber}`, '');
            });
        },
        handleSelectionChange(rows: any) {
            this.selectedDevices = rows;
            this.$emit("update:selectedRows", this.selectedDevices);
        },
        tableRowClassName({ row, rowIndex }) {
            if (row.Status == "Offline") {
                return 'offline7';
            }
            else if (row.Status == "Online" || row.Status == "Đang xử lý") {
                return 'online7';
            }
            else {
                return '';
            }
        },
        setMaxHeightTable() {
            this.maxHeight = window.innerHeight - 195;
        },
        viewData(){
            this.page = 1;
            this.getData();
        },
        async getData(options?: { showLoading: boolean }) {
            options?.showLoading !== false && (this.loading = true); 
            const res = await deviceApi.GetDeviceAtPage(this.page, this.searchValue, this.pageSize);
            this.devices = res.data.data.map((d,idx) => ({
                index: idx + 1,
                ...d,
            }));
            this.totalDevice = res.data.total;
            //nếu điều kiện trước (options?.showLoading !== false) đúng thì thực hiện vế sau dấu '&&' (this.loading = false), không thì thôi
            options?.showLoading !== false && (this.loading = false);
            this.shouldResetColumnSortState = !this.shouldResetColumnSortState;
        },
        onSelectionChange(selectedRows: any[]) {
            this.$emit('update:selectedRows', selectedRows);
        },
        async refreshDataTableAsync() {
            const res = await deviceApi.GetDeviceAtPage(this.page, this.searchValue, this.pageSize);
            res.data.data.forEach((d,idx) => {
                Object.assign(this.devices[idx], d);
            });
            (this.$refs.visualizeTable as any)?.gridApi?.refreshCells({
                force: false,
                suppressFlash: true,
            });
        },
        async countOnOffDevice() {
            const res = await deviceApi.GetDeviceAtPage(1, "", 100000);
            const allDevices = res.data.data.map((d,idx) => ({
                index: idx + 1,
                ...d,
            }));
            if(this.$router.currentRoute.path == '/device-info')
            { 
                this.onlineDevices = allDevices.length - allDevices.filter(x => x.Status == "Offline").length;
            }
            if(this.$router.currentRoute.path == '/device-info')
            {
                this.offlineDevices = allDevices.filter(x => x.Status == "Offline").length;
            }
        }
    },
    beforeMount() {
        this.setMaxHeightTable();
        window.addEventListener("resize", () => {
            this.setMaxHeightTable();
        });
    },
    mounted() {
        this.getData().then(() => {
            this.fetchProcessingStatus();
        });
        this.countOnOffDevice();
        this.autoRefreshInterval = setInterval(async () => {
            this.refreshDataTableAsync();
        }, 20000);

        this.autoRefreshTotalOnOffInterval = setInterval(async () => {
            this.countOnOffDevice();
        }, 20000);
        
        SignalRConnection.connection.on("ReceiveNotification", (lsMessage: any[]) => {
            if (lsMessage.find(x => x.result === "GetDeviceInfoSuccess" 
                || x.result.includes('DeleteAllUsers') 
                || x.result.includes('DeleteUserById'))) {
                clearTimeout(this.refreshDataTimeout);
                this.refreshDataTimeout = setTimeout(() => {
                    this.refreshDataTableAsync();                    
                }, 100);
            }
        });
    },
    beforeDestroy() {
        clearInterval(this.autoRefreshInterval);
        clearInterval(this.autoRefreshTotalOnOffInterval);
    }
})
</script>
<style scoped>
.device-info__toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 8px;
    flex-wrap: wrap;

}
</style>