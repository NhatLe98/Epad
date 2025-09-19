<template>
   <el-select v-model="ListWorkingStatus" :placeholder="$t('WorkStatus')" reserve-keyword collapse-tags clearable filterable multiple default-first-option
      style="width:180px">
      <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value">
      </el-option>
   </el-select>
</template>
<script lang="ts">
import Vue from 'vue';
export default Vue.extend({
   name: 'WorkingInfoSelect',
   watch: {
      ListWorkingStatus(isWorking) {
         this.$emit('onWorkingInfoChange', isWorking);
      }
   },
   data() {
      return {
         options: [{
            value: 0,
            label: this.$t('IsWorking').toString()
         }, {
            value: 1,
            label: this.$t('StoppedWork').toString()
         }],
         ListWorkingStatus: [0],
      }
   }
});
</script>