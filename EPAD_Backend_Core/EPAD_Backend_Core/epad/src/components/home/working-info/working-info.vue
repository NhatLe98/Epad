<template>
  <div id="bgHome">
    <el-row>
        <el-col :span="5" class="Tree">
            <el-input class="working-info__input-tree" 
            style="padding: 5px; position: absolute; width: 20%; height: 30px; z-index: 1000"
                      :placeholder="$t('SearchData')"
                      v-model="filterTree"
                      @keyup.enter.native="filterTreeData()">
                      <i slot="suffix" class="el-icon-search" @click="filterTreeData()"></i>
            </el-input>
            <el-tree style="margin-top:50px" :data="treeData" class="table-text-color"
                     node-key="ID"
                     :props="{ label: 'Name', children: 'ListChildrent' }"
                     :filter-node-method="filterNode"
                     :default-expanded-keys="expandedKey" 
                     :default-checked-keys="defaultChecked"
                     @check="nodeCheck"
                     ref="tree"
                     show-checkbox
                     highlight-current
                     v-loading="loadingTree">
                <template slot-scope="scoped">
                    <div>
                        <i :class="getIconClass(scoped.data.Type, scoped.data.Gender)" />
                        <span class="ml-5">{{ scoped.data.Name }}</span>
                    </div>
                </template>
            </el-tree>
        </el-col>
      <el-col :span="19">
        <el-container>
          <el-header>
            <el-row>
              <el-col :span="10" class="left">
                <span id="FormName">
                  {{ $t('WorkingInfo') }}
                </span>
              </el-col>
              <el-col :span="14">
                <HeaderComponent :showMasterEmployeeFilter="true"/>
              </el-col>
            </el-row>
          </el-header>
          <el-main class="bgHomeHasView">
            <el-row class="working-info__custom-function-bar">
              <el-col :span="4" style="margin-right: 10px;">
                <el-date-picker
                :placeholder="$t('FromDateString')"
                  v-model="fromDate"
                  :clearable="false"
                  type="date"
                  id="inputFromDate"
                ></el-date-picker>
              </el-col>
              <el-col :span="4" style="margin-right: 10px;">
                <el-date-picker
                :placeholder="$t('ToDateString')"
                  v-model="toDate"
                  :clearable="false"
                  type="date"
                  id="inputToDate"
                ></el-date-picker>
              </el-col>
              <el-col :span="1">
                <el-button
                  type="primary"
                  size="small"
                  class="smallbutton"
                  @click="View"
                  >{{ $t('View') }}</el-button
                >
              </el-col>
            </el-row>
            <div class="table">
              <data-table-function-component class="working-info__data-table-function"
                v-bind:isHiddenEdit="true"
                v-bind:isHiddenDelete="true"
                v-bind:showButtonCustom="false"
                v-bind:showButtonInsert="false"
              ></data-table-function-component>
              <data-table-component class="working-info__data-table"
                :get-data="getPage"
                ref="table"
                :columns="columns"
                :selectedRows.sync="rowsObj"
                v-bind:isFromHasView="true"
                :isShowPageSize="true"
              ></data-table-component>
            </div>
          </el-main>
        </el-container>
      </el-col>
    </el-row>
  </div>
</template>
<script src="./working-info"></script>
<style lang="scss">
.working-info__data-table {
  .filter-input {
    margin-right: 10px;
  }
  .el-table {
    height: calc(100vh - 174px) !important;
    .el-table__body-wrapper {
      height: 100% !important;
    }
  }
}
.working-info__input-tree {
        .el-input__suffix{
            top: unset !important;
            margin-right: 5px;
        }
    }
</style>
