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
        <el-dropdown
          style="margin-left: 10px; margin-top: 5px"
          @command="handleCommand"
          trigger="click"
        >
          <span class="el-dropdown-link" style="font-weight: bold">
            . . .<span class="more-text">{{ $t("More") }}</span>
          </span>

          <el-dropdown-menu slot="dropdown">
            <el-dropdown-item
              v-for="(item, index) in listExcelFunction"
              :key="index"
              :command="item"
            >
              {{ $t(item) }}
            </el-dropdown-item>
          </el-dropdown-menu>
        </el-dropdown>
        <div class="tab-modal-excel">
          <el-dialog
            :title="isAddFromExcel ? $t('AddFromExcel') : $t('DeleteFromExcel')"
            custom-class="customdialog"
            :visible.sync="showDialogExcel"
            @close="AddOrDeleteFromExcel('close')"
            :close-on-click-modal="false"
          >
            <el-form
              :model="formExcel"
              ref="formExcel"
              label-width="168px"
              label-position="top"
            >
              <el-form-item :label="$t('SelectFile')">
                <div class="box">
                  <input
                    ref="fileInput"
                    accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
                    type="file"
                    name="file-3[]"
                    id="fileUpload"
                    class="inputfile inputfile-3"
                    @change="processFile($event)"
                  />
                  <label for="fileUpload">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="20"
                      height="17"
                      viewBox="0 0 20 17"
                    >
                      <path
                        d="M10 0l-5.2 4.9h3.3v5.1h3.8v-5.1h3.3l-5.2-4.9zm9.3 11.5l-3.2-2.1h-2l3.4 2.6h-3.5c-.1 0-.2.1-.2.1l-.8 2.3h-6l-.8-2.2c-.1-.1-.1-.2-.2-.2h-3.6l3.4-2.6h-2l-3.2 2.1c-.4.3-.7 1-.6 1.5l.6 3.1c.1.5.7.9 1.2.9h16.3c.6 0 1.1-.4 1.3-.9l.6-3.1c.1-.5-.2-1.2-.7-1.5z"
                      />
                    </svg>
                    <span>{{ $t("ChooseAExcelFile") }}</span>
                  </label>
                  <span v-if="fileName === ''" class="fileName">{{
                    $t("NoFileChoosen")
                  }}</span>
                  <span v-else class="fileName">{{ fileName }}</span>
                </div>
              </el-form-item>
              <el-form-item :label="$t('DownloadTemplate')">
                <a
                  class="fileTemplate-lbl"
                  href="/Template_TA_LocationByEmployee.xlsx"
                  download
                  >{{ $t("Download") }}</a
                >
              </el-form-item>
            </el-form>

            <span slot="footer" class="dialog-footer">
              <el-button
                class="btnCancel"
                @click="AddOrDeleteFromExcel('close')"
              >
                {{ $t("Cancel") }}
              </el-button>
              <el-button
                class="btnOK"
                type="primary"
                @click="UploadDataFromExcel"
              >
                {{ $t("OK") }}
              </el-button>
            </span>
          </el-dialog>
        </div>
        <div>
          <el-dialog
            :title="$t('DialogHeaderTitle')"
            custom-class="customdialog"
            :visible.sync="showDialogImportError"
            @close="showOrHideImportError(false)"
            :close-on-click-modal="false"
          >
            <el-form label-width="168px" label-position="top">
              <el-form-item>
                <div class="box">
                  <label>
                    <span>{{ importErrorMessage }}</span>
                  </label>
                </div>
              </el-form-item>
              <el-form-item>
                <a
                  class="fileTemplate-lbl"
                  href="/Files/Template_LocationByEmployee_Error.xlsx"
                  download
                  >{{ $t("Download") }}</a
                >
              </el-form-item>
            </el-form>

            <span slot="footer" class="dialog-footer">
              <el-button
                class="btnOK"
                type="primary"
                @click="showOrHideImportError(false)"
              >
                OK
              </el-button>
            </span>
          </el-dialog>
        </div>
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
              ref="departmentList"
            >
            </select-department-tree-component>
          </el-form-item>

          <el-form-item
            :label="$t('Employee')"
            prop="EmployeeATIDs"
            @click.native="focus('Employee')"
          >
            <app-select-new
              :disabled="isEdit"
              :dataSource="employeeFullLookupFilter"
              displayMember="NameInFilter"
              valueMember="Index"
              :allowNull="true"
              v-model="ruleForm.EmployeeATIDs"
              :multiple="true"
              @getValueSelectedAll="selectAllEmployeeFilter"
              :placeholder="$t('SelectEmployee')"
              ref="employeeList"
              style="width: 100%"
            >
            </app-select-new>
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

<script src="./ta-location-by-employee-component.ts"></script>
