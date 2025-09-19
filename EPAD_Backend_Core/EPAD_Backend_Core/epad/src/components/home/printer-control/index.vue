<template>
  <AppLayout formName="PrinterControl">
        <PrinterTable />
  </AppLayout>
</template>
<script lang="ts">
import Vue from 'vue';
import AppLayout from '@/components/app-component/app-layout/app-layout.vue';
import PrinterTable from './printer-table.vue';

export default Vue.extend({
    name: 'PrinterControl',
    components: {
        AppLayout,
        PrinterTable,
    }
})
</script>