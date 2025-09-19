<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("AssignPrivilegeUserGroup") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div class="assign">
        <MatrixCheckboxMenuComponent :listData.sync="listMenu" :columns="groupUser" :listState="listState" :isLoading="isLoaded"/>
        </div>
          
        <el-row class="mt-10">
          <el-col :span="24" class="left">
            <el-button
              type="primary"
              @click="SaveData"
            >{{ $t("Save") }}</el-button>
          </el-col>
        </el-row>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./assign-privilege-user-group-component.ts"></script>
<style scoped>
.mt-10 {
  margin-top: 10px;
}

</style>
