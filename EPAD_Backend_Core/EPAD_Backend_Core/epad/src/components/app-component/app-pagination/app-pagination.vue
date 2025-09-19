<template>
<div class="app-paging">
    <small>{{$t("Display")}}</small>
    <el-input v-model="lPageSize"
        @keyup.enter.native="handleChangePageSize"
        filterable
        default-first-option
        style=" margin-left:10px;width:80px">
         </el-input>
    <el-pagination class="custom-pagination" :total="total"
        :page-size="+lPageSize"
        :current-page="lPage"
        @current-change="handleCurrentPageChange"
        layout="prev, pager, next"></el-pagination>
    <div class="total-record">
        <small>{{$t('Total')}}: <b>{{total}}</b></small>
    </div>
     <div class="total-record" v-if="isShowTotalOnOffDevice">
        <small>Online/Offline: <b>{{onlineDevices}}/{{offlineDevices}}</b></small>
    </div>
</div>
</template>
<script lang="ts">
import { getDigit } from '@/utils/number-utils';
import Vue from 'vue'
export default Vue.extend({
    props: {
        page: {
            type: Number,
            default: 1,
        },
        pageSize: {
            default: 100000
        },
        total: {
            type: Number,
            default: 0,
        },
        getData: {
            type: Function,
        },
        offlineDevices: {
            type: Number,
            default: 0,
        },
        onlineDevices: {
            type: Number,
            default: 0,
        },
        isShowTotalOnOffDevice: {
            type: Boolean,
            default: false,
        }
    },
    // watch: {
    //     lPage() {
    //         this.$emit('update:page', this.lPage);
    //         this.getData && setTimeout(() => {
    //             this.getData();
    //         }, 50);
    //     },
    // },
    data() {
        const lPage = this.page;
        const lPageSize = this.pageSize;
        return {
            lPage,
            lPageSize,
        }
    },
    methods: {
        handleCurrentPageChange(page: number) {
            if (this.isValidPageSize()) {
                this.lPage = page;
                this.$emit('update:page', this.lPage);
                this.getData && setTimeout(() => {
                    this.getData();
                }, 50);
            }
            
        },
        handleChangePageSize() {
            if (!this.isValidPageSize()) return;
            this.lPage = 1;
            if (typeof this.pageSize === 'string') {
                this.lPageSize = getDigit(this.lPageSize as any);
            }
            this.$emit('update:pageSize', this.lPageSize);
            this.getData && setTimeout(() => {
                this.getData();
            }, 50);
        },
        isValidPageSize() {
            const invalidPageSize = this.lPageSize == 0 || this.lPageSize == null || this.lPageSize.toString() == "";
            if (invalidPageSize) {
                this.$alertSaveError(null, null, null, this.$t("PleaseEnterPageSize").toString())
                return false;
            }
            return true;
        }
    }
})
</script>
<style scoped>
.app-paging {
    display: flex;
    justify-content: center;
    align-items: center;
    margin-top: 4px;
}
</style>
