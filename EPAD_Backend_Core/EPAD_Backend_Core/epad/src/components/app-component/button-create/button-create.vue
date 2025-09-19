<template>
    <el-button type="primary" class="add-button" @click="click">
        <span class="add-icon"><img src="@/assets/icons/Button/circle-add.svg" alt="insert" /></span>
        <span class="add">{{ $t("Insert") }}</span>
    </el-button>
</template>
<script lang="ts">
import Vue from 'vue'
export default Vue.extend({
    inheritAttrs: true,
    props: {
        click: {
            type: Function
        }
    }
})
</script>
<style scoped>
    .add-button {
        padding: 8px 16px 8px 10px;
    }

    .add, .add-icon {
        float: left;
        display: block;
        height: 20px;
    }

    .add-icon {
        width: 20px;
    }

    .add {
        margin-left: 10px;
        height: 20px;
        line-height: 20px;
    }

</style>