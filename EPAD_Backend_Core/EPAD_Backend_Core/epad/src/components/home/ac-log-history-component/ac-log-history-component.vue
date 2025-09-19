<template>
  <div id="bgHome" v-loading="loading">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ACLogHistory") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
      
        <div class="ac-log-history__custom-function-bar">
            <app-select-new
              :dataSource="allAreaLst"
              displayMember="label"
              valueMember="value"
              :allowNull="true"
              v-model="selectedArea"
              :multiple="true"
              style="width: 15%; padding-right: 10px;"
              :placeholder="$t('SelectArea')"
              @change="onAreaChange"
              ref="parkingLotFilter"
              class="ac-log-history__app-select"
            >
            </app-select-new>

            <app-select-new
            :dataSource="allDoorLst"
            displayMember="label"
            valueMember="value"
            :allowNull="true"
            v-model="selectedDoor"
            :multiple="true"
            style="width: 15%;"
            :placeholder="$t('SelectDoor')"
            class="ac-log-history__app-select"
          >
          </app-select-new>

          <!-- <el-select calss="ac-log-history__select" v-model="selectedArea" collapse-tags filterable :placeholder="$t('SelectArea')" multiple clearable @change="onAreaChange"
             >
              <el-option v-for="item in allAreaLst" :key="item.value" :label="$t(item.label)"
                :value="item.value"></el-option>
            </el-select> -->
    
            <!-- <el-select class="ac-log-history__select" filterable collapse-tags :placeholder="$t('SelectDoor')" multiple v-model="selectedDoor" clearable
            style="margin-left: 10px;"
             >
              <el-option v-for="item in allDoorLst" :key="item.value" :label="$t(item.label)"
                :value="item.value"></el-option>
            </el-select> -->

            <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll" :multiple="tree.multiple"
                        :placeholder="$t('SelectDepartment')" :disabled="tree.isEdit" :data="tree.treeData"
                        :props="tree.treeProps" :isSelectParent="true" :checkStrictly="tree.checkStrictly"
                        :clearable="tree.clearable" :popoverWidth="tree.popoverWidth" v-model="departmentIds"
                        style="padding: 0 10px; width: 20%; display: inline-block;position: relative;"></select-department-tree-component>
        
            <el-date-picker v-model="fromTime" style="width:200px;" type="datetime" class="ac-log-history__date-picker"
              :placeholder="$t('SelectDateTime')"></el-date-picker>
        
            <el-date-picker v-model="toTime" style="width:200px;margin-left: 10px;" type="datetime" class="ac-log-history__date-picker"
              :placeholder="$t('SelectDateTime')"></el-date-picker>
              <el-input 
                style="padding-bottom:3px; width:238px;padding: 0 0px 0 0; margin-left: 10px; margin-right: 10px"
                  :placeholder="$t('SearchData')"
                  v-model="filter"
                  @keyup.enter.native="Search()"
                  class="filter-input">
                  <i slot="suffix" class="el-icon-search"></i>
        </el-input>
      
            <el-button type="primary" id="btnFunction" round @click="Search" style="background-color: #122658; color: white; height: 33px !important;">
              {{ $t("View") }}
            </el-button>
        </div>
       
        <div>
          <data-table-function-component class="ac-log-history__data-table-function"
                        :showButtonInsert="false"
                        :isHiddenEdit="true" :isHiddenDelete="true"
                        :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
                        style="height: fit-content; display: flex; position: relative; top: 0; width: 100%;
                        margin-right: 0 !important;"
                      ></data-table-function-component>
          <data-table-component class="ac-log-history__data-table" ref="table" :showSearch="false" :get-data="getData" :columns="columns"  :isShowPageSize="true"
            :showCheckbox="false"></data-table-component>
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
<script src="./ac-log-history-component.ts"></script>
<style lang="scss">
.ac-log-history__data-table .el-table{
  height: calc(100vh - 183px) !important;
  margin-top: 10px;
}

.ac-log-history__date-picker{
  .el-input__icon.el-icon-time {
    height: 32px !important;
  }
  .el-input__icon.el-icon-circle-close {
    height: 32px !important;
  }
}

.ac-log-history__app-select,
.ac-log-history__select {
  .el-select__caret.el-input__icon.el-icon-arrow-up {
    height: 100% !important;
  }
  .el-select__caret.el-input__icon.el-icon-circle-close {
    height: 100% !important;
  }
  .el-select__tags:has(span span:nth-child(1)) {
    top: 50% !important;
    max-width: 80% !important;
    span span:first-child {
      width: max-content;
      max-width: calc(100% - 20%);
      margin-left: 1px;
      .el-select__tags-text {
        vertical-align: top;
        width: max-content;
        max-width: 80%;
        display: inline-block;
        text-overflow: ellipsis;
        overflow: hidden;
      }
    }
  }
  .el-select__tags:has(span span:nth-child(2)) {
    top: 50% !important;
    max-width: 80% !important;
    span span:first-child {
      width: max-content;
      max-width: calc(100% - 30%);
      margin-left: 1px;
      .el-select__tags-text {
        vertical-align: top;
        width: max-content;
        max-width: 80%;
        display: inline-block;
        text-overflow: ellipsis;
        overflow: hidden;
      }
    }
    span span:nth-child(2) {
      max-width: 30%;
      margin-left: 1px;
      .el-select__tags-text {
        width: 100%;
        max-width: 100% !important;
      }
    }
  }
}
</style>
