<template>
     <el-input
        style="width:238px;"
        :placeholder="$t('SearchData')"
        v-model="searchValue"
        @keyup.enter.native="onSearchEnter">
        <i slot="suffix" class="el-input__icon el-icon-search" @click="onSearchEnter"></i>
     </el-input>
</template>
<script lang="ts">
import Vue from 'vue'
export default Vue.extend({
    methods: {
        onSearchEnter() {
            this.$emit('onSearchEnter', this.searchValue);
        }
    },
    data() {
        return {
            searchValue: '',
        }
    },
    watch: {
        refreshData() {
           console.log('đâs');
        },
    }
})
</script>