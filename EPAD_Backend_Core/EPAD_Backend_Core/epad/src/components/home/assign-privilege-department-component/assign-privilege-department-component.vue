<template>
   <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("DevicesByDepartment") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome devicebysomething">
        <el-row class="rowdeviceby" :gutter="20">
          <el-col :span="10" class="table-device-by">
            <el-table :data="listUserPrivilege"
              :height="maxHeight"
              highlight-current-row @current-change="handleCurrentChange" 
              style="cursor:pointer;"
              class="deviceby not-table-text-color" border
            >
              <el-table-column prop="name" :label="$t('GroupAccount')"></el-table-column>
            </el-table>
        </el-col>
        <el-col :span="14" class="transfer-device-by" v-bind:style="{height: maxHeight+'px'}">
            <div class="department-tree" >
                <div class="department-title">{{$t('DepartmentList')}}</div>
                <department-tree-component :showEmployee="false" ref="departmentTree"></department-tree-component>
            </div>
        </el-col>
        </el-row>
        <el-row style="margin-top:10px;">
          
          
            <el-button type="success" style="float: right" @click="updateDepartmentByUserPrivilege">
              {{$t("Save")}}
            </el-button>
          
        </el-row>
      </el-main>
    </el-container>
  </div>
</template>

<script src="./assign-privilege-department-component.ts"></script>
<style scoped>


.devicebysomething .el-table {
  margin-top: 0;
}
.devicebysomething .el-table.deviceby {
  border: 1px solid #ebeef5;
  
}
.devicebysomething .el-table__header-wrapper th {
  background-color: #F5F7FA;
}
.devicebysomething .el-transfer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.devicebysomething .el-transfer .el-transfer-panel {
  flex-grow: 1;
  width: 100%;
  height: 100%;
}
.devicebysomething .el-transfer .el-transfer__buttons {
  text-align: center;
  padding: 0 5px;
}
.devicebysomething .el-transfer-panel__body {
  height: calc(100% - 40px);
}
.devicebysomething .el-checkbox-group.el-transfer-panel__list {
  height: 100%;
}
.devicebysomething .el-button {
  margin-left: 0;
}
.department-tree{
    border: 1px solid #e3e3e3;
    width: 100%;
    height: 100%;
    overflow-y: auto;
}
.department-title{
    font-weight: 600;
    font-size: 12px;
    text-transform: uppercase;
    padding: 10px;
    border-bottom: 1px solid #e3e3e3;
}
.el-tree{
    background-color: unset;
}
</style>