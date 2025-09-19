<template>
  <div id="bgHome" v-loading="loading">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ACUserMasterLog") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome" style="margin-top: 2px">
        <el-select v-model="selectedArea" collapse-tags @change="onAreaChange" filterable :placeholder="$t('SelectArea')" multiple clearable
             >
              <el-option v-for="item in allAreaLst" :key="item.value" :label="$t(item.label)"
                :value="item.value"></el-option>
            </el-select>
    
            <el-select filterable :placeholder="$t('SelectDoor')" multiple collapse-tags v-model="selectedDoor" style="padding: 0 10px 0 10px;"
            clearable
           
             >
          
              <el-option v-for="item in allDoorLst" :key="item.value" :label="$t(item.label)"
                :value="item.value"></el-option>
            </el-select>

            <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll" :multiple="tree.multiple"
                        :placeholder="$t('SelectDepartment')" :disabled="tree.isEdit" :data="tree.treeData"
                        :props="tree.treeProps" :isSelectParent="true" :checkStrictly="tree.checkStrictly"
                        :clearable="tree.clearable" :popoverWidth="tree.popoverWidth" v-model="departmentIds"
                        style="padding: 0 10px 0 0; width: 20%; display: inline-block;position: relative;"></select-department-tree-component>
        
            
       
            <!-- <el-date-picker v-model="fromTime" style="width:200px;" type="datetime" 
              :placeholder="$t('SelectDateTime')"></el-date-picker>
        
            <el-date-picker v-model="toTime" style="width:200px; padding: 0 5px 0 5px" type="datetime"
              :placeholder="$t('SelectDateTime')"></el-date-picker> -->
              <el-input 
              style="padding-bottom:3px; width:238px; margin-right: 10px;"
                  :placeholder="$t('SearchData')"
                  v-model="filter"
                  @keyup.enter.native="Search()"
                  class="filter-input">
                  <i slot="suffix" class="el-icon-search"></i>
        </el-input>
      
            <el-button id="btnFunction" round @click="Search" style="background-color: #122658; color: white;">
              {{ $t("View") }}
            </el-button>
       
        <div>
          <data-table-function-component
                        :showButtonInsert="false"
                        :isHiddenEdit="true" :isHiddenDelete="true"
                        :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
                        style="height: fit-content; display: flex; position: relative; top: 0; width: 100%;
                        margin-right: 0 !important;"
                      ></data-table-function-component>
          <data-table-component class="ac-usermaster__data-table" ref="table" :showSearch="false" :get-data="getData" :columns="columns"   :isShowPageSize="true"
            :showCheckbox="false">></data-table-component>
        </div>
        <!-- <div>
          <el-row>
            <el-col :span="12" class="left">
              <el-button class="classLeft" id="btnFunction" type="primary" round>{{ $t("Export") }}</el-button>
            </el-col>
            <el-col :span="12"></el-col>
          </el-row>
        </div> -->
      </el-main>
    </el-container>
  </div>
</template>
<script src="./ac-usermaster-component.ts"></script>
<style>
.ac-usermaster__data-table .el-table{
  height: calc(100vh - 240px) !important;
}
</style>
