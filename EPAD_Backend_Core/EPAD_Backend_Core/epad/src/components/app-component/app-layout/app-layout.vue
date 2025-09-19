<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t(formName) }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent :showMasterEmployeeFilter="showMasterEmployeeFilter"/>
          </el-col>
        </el-row>
      </el-header>
       <el-main class="bgHome">
           <slot name="default" />
       </el-main>
    </el-container>
  </div>
</template>
<script lang="ts">
import Vue from 'vue';
import HeaderComponent from "@/components/home/header-component/header-component.vue";

export default Vue.extend({
    name: 'AppLayout',
    props: {
        formName: {
            type: String,
            required: true,
        },
        showMasterEmployeeFilter: {
            type: Boolean,
            default: false,
        },
    },
    components: {
        HeaderComponent,
    }
})
</script>