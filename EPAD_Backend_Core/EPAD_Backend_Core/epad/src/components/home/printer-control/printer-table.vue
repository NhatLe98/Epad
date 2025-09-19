<template>
    <div>
        <PrinterTableToolbar 
            @refreshData="fetchData" 
            :selectedPrinters="selectedPrinters"
            :gridColumnConfig.sync="columnDefs"
            @onSearchEnter="onSearchEnter" />
        <VisualizeTable class="printer-table"
            :columnDefs="columnDefs.filter(x => x.display)"
            :rowData="printers"
            :heightScale="176"
            @onSelectionChange="onSelectionChange"
            :isKeepIndexAscending="true"
            :shouldResetColumnSortState="shouldResetColumnSortState"
            />
        <AppPagination 
            :page.sync="page"
            :pageSize.sync="pageSize"
            :getData="fetchData"
            :total="total" />
    </div>
</template>
<script lang="ts">
import Vue from 'vue';
import VisualizeTable from '@/components/app-component/visualize-table/visualize-table.vue';
import SearchField from '@/components/app-component/search-field/search-field.vue';
import PrinterTableToolbar from './printer-table-toolbar.vue';
import AppPagination from '@/components/app-component/app-pagination/app-pagination.vue';
import { IC_Printer } from '@/models/ic-printer';
import {printerApi} from '@/$api/printer-api';

export default Vue.extend({
    name: 'PrinterTable',
    components: {
        VisualizeTable,
        SearchField,
        PrinterTableToolbar,
        AppPagination
    },
    watch: {
        columnDefs() {
            // console.log(this.columnDefs)
        },
    },
    data() {
        const printers: (IC_Printer & {
            index: number,
        })[] = [];

        const selectedPrinters: IC_Printer[] = [];
       return {
            columnDefs: [
                {
                    field: 'index',
                    sortable: true,
                    pinned: true,
                    headerName: '#',
                    width: 80,
                    checkboxSelection: true, 
                    headerCheckboxSelection: true,
                    headerCheckboxSelectionFilteredOnly: true,
                    display: true
                },
                {
                    field: "Name", 
                    pinned: 'left', 
                    sortable: true, 
                    width: 150,
                    headerName: this.$t('Name'),
                    display: true
                },
                {
                    field: "SerialNumber", 
                    sortable: true, 
                    width: 150,
                    headerName: this.$t('SerialNumber'), 
                    display: true
                },
                {
                    field: "IPAddress", 
                    sortable: true, 
                    width: 150,
                    headerName: this.$t('IPAddress'), 
                    display: true
                },
                {
                    field: "Port", 
                    sortable: true, 
                    width: 150,
                    headerName: this.$t('Port'), 
                    display: true
                },
                {
                    field: "PrinterModel", 
                    sortable: true, 
                    width: 150,
                    headerName: this.$t('Model'), 
                    display: true
                },
                {
                    field: "CreatedDate", 
                    sortable: true, 
                    width: 150,
                    headerName: this.$t('CreatedDate'), 
                    display: true
                },
                // {
                //     field: '',
                //     minWidth: 0,
                //     headerName: '',
                //     flex: 1,
                //     display: true
                // }
            ],
            printers,
            selectedPrinters,
            page: 1,
            pageSize: 1000,
            searchValue: '',
            total: 0,
            shouldResetColumnSortState: false,
        }
    },
    methods: {
        fetchData() {
            printerApi.getPrintersAsync({
                page: this.page,
                pageSize: this.pageSize,
                searchValue: this.searchValue,
            }).then((result) => {
                this.printers = result.Data.map((p, idx) => ({
                    ...p,
                    index: idx + 1,
                    CreatedDate: moment(p.CreatedDate).format('DD/MM/YYYY HH:mm:ss'),
                }));
                this.total = result.Total;
                this.shouldResetColumnSortState = !this.shouldResetColumnSortState;
            });
        },
        onSelectionChange(selectedRows: IC_Printer[]) {
            this.selectedPrinters = selectedRows;
        },
        onSearchEnter(value: string) {
            this.searchValue = value.trim();
            this.page = 1;
            this.fetchData();
        },
    },
    mounted() {
        this.fetchData();
    }
})
</script>