<template>
  <div class="bgHome" style="height: calc(100vh - 185px)">
    <div class="tab-modal">
      <el-row :span="4">
        <el-input
          style="
            padding-bottom: 3px;
            float: left;
            width: 200px;
            margin-top: 3px;
          "
          :placeholder="$t('SearchData')"
          v-model="filterModel.TextboxSearch"
          @keyup.enter.native="onViewClick"
          class="filter-input"
        >
          <i slot="suffix" class="el-icon-search" @click="onViewClick"></i>
        </el-input>
      </el-row>
      <el-dialog
        :title="isEdit ? $t('Edit') : $t('Insert')"
        custom-class="customdialog"
        :close-on-click-modal="false"
        :visible.sync="showDialog"
        :before-close="onCancelClick"
      >
        <el-form
          :model="ruleForm"
          :rules="rules"
          ref="ruleForm"
          label-width="168px"
          label-position="top"
          @key.enter.native="onSubmitClick"
        >
          <el-form-item :label="$t('Department')" prop="DepartmentList">
            <select-department-tree-component
              :defaultExpandAll="tree.defaultExpandAll"
              :disabled="isEdit"
              :multiple="true"
              :placeholder="$t('SelectDepartment')"
              :data="tree.treeData"
              :props="tree.treeProps"
              :isSelectParent="true"
              :clearable="tree.clearable"
              :popoverWidth="tree.popoverWidth"
              :check-strictly="tree.checkStrictly"
              v-model="ruleForm.DepartmentList"
              @change="onChangeDepartmentFilter"
              style="width: 100%; margin-bottom: 25px"
            >
            </select-department-tree-component>
          </el-form-item>
          <el-form-item :label="$t('Place')" prop="LocationIndex">
            <el-select
              v-model="ruleForm.LocationIndex"
              :multiple="false"
              clearable
              filterable
              :placeholder="$t('SelectedLocation')"
              style="width: 100%"
            >
              <el-option
                v-for="item in listLocation"
                :key="item.LocationIndex"
                :value="item.LocationIndex"
                :label="item.LocationName"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="onCancelClick">
            {{ $t("Cancel") }}
          </el-button>
          <el-button class="btnOK" type="primary" @click="onSubmitClick">{{
            $t("OK")
          }}</el-button>
        </span>
      </el-dialog>
    </div>
    <div class="tab-modal-delete">
      <el-dialog
        v-if="showDialogDeleteUser"
        custom-class="customdialog"
        :title="$t('DialogOption')"
        :visible.sync="showDialogDeleteUser"
        :before-close="cancelDeleteUser"
      >
        <el-form label-width="168px" label-position="top">
          <div style="margin-bottom: 20px">
            <i
              style="font-weight: bold; font-size: larger; color: orange"
              class="el-icon-warning-outline"
            />
            <span style="font-weight: bold">
              {{ $t("MSG_ConfirmDelete") }}
            </span>
          </div>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="showDialogDeleteUser = false">
            {{ $t("Cancel") }}
          </el-button>
          <el-button type="primary" @click="Delete">
            {{ $t("OK") }}
          </el-button>
        </span>
      </el-dialog>
    </div>
    <div class="tab-grid" style="height: 100% !important">
      <VisualizeTable
        v-loading="isLoading"
        :columnDefs="columnDefs.filter((x) => x.display)"
        :rowData="dataSource"
        :rowHeight="40"
        @onSelectionChange="onSelectionChange"
        :heightScale="260"
        :isKeepIndexAscending="true"
        :shouldResetColumnSortState="shouldResetColumnSortState"
        style="height: calc(100% - 83px); border-bottom: 1px solid lightgray"
      />
      <AppPagination
        :page.sync="page"
        :pageSize.sync="pageSize"
        :get-data="loadData"
        :total="total"
        ref="table"
      />
    </div>
  </div>
</template>

<script src="./ta-location-by-department-component.ts"></script>
