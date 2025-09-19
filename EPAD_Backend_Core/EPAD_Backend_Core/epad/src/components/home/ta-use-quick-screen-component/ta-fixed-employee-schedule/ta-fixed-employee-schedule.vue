<template>
  <div class="tab" style="height: calc(100vh - 185px)">
    <div>
      <el-row>
        <el-col :span="5">
          <select-department-tree-component
            :defaultExpandAll="tree.defaultExpandAll"
            :multiple="tree.multiple"
            :placeholder="$t('SelectDepartment')"
            :disabled="tree.isEdit"
            :data="tree.treeData"
            :props="tree.treeProps"
            :isSelectParent="true"
            :checkStrictly="tree.checkStrictly"
            :clearable="tree.clearable"
            :popoverWidth="tree.popoverWidth"
            v-model="selectedDepartment"
            @change="onChangeDepartmentFilter"
            style="padding-right: 10px; width: 100%"
          ></select-department-tree-component>
        </el-col>
        <el-col :span="5" >
          <app-select-new class="ta-fixed-employee-schedule__filter-employee"
          :dataSource="employeeFullLookupFilter"
          displayMember="NameInFilter"
          valueMember="Index"
          :allowNull="true"
          v-model="selectedEmployee"
          :multiple="true"
          style="width: 100%;"
          :placeholder="$t('SelectEmployee')"
          @getValueSelectedAll="selectAllEmployeeFilter"
          ref="employeeList"
        >
        </app-select-new>
        </el-col>
        <el-col :span="4" style="margin-left: 10px">
          <el-date-picker
            v-model="fromDateFilter"
            type="date"
            format="dd/MM/yyyy"
            :placeholder="$t('AppliedDate')"
            style="width: 100%"
            :clearable="false"
            :editable="false"
          >
          </el-date-picker>
        </el-col>
        <!-- <el-col :span="4" style="margin-left: 10px">
          <el-date-picker
            v-model="toDateFilter"
            type="date"
            format="MM/dd/yyyy"
            :placeholder="$t('ToDateString')"
            style="width: 100%"
            :clearable="true"
            :editable="true"
          >
          </el-date-picker>
        </el-col> -->
        <el-col :span="4" style="margin-left: 10px">
          <el-button
            type="primary"
            class="smallbutton"
            size="small"
            @click="view"
          >
            {{ $t("View") }}
          </el-button>
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
        </el-col>
        <el-col :span="1" style="float: right;">
          <TableToolbar 
          ref="customerDataTableFunction"
            :gridColumnConfig.sync="columnDefs" />
        </el-col>
      </el-row>
    </div>
    <div class="tab-grid" style="height: calc(100% - 35px) !important">
      <VisualizeTable
        v-loading="isLoading"
        :columnDefs="columnDefs.filter((x) => x.display)"
        :rowData="dataSource"
        :rowHeight="40"
        @onSelectionChange="onSelectionChange"
        :heightScale="260"
        :isKeepIndexAscending="true"
        :shouldResetColumnSortState="shouldResetColumnSortState"
        style="height: calc(100% - 45px); border-bottom: 1px solid lightgray;"
        
      />
      <AppPagination
        :page.sync="page"
        :pageSize.sync="pageSize"
        :getData="loadData"
        :total="total"
        ref="scheduleEmployee"
      />
    </div>
    <div class="tab-modal">
      <el-dialog
        custom-class="customdialog"
        width="1000px"
        :title="
          isEdit
            ? $t('EditFixedEmployeeSchedule')
            : $t('InsertFixedEmployeeSchedule')
        "
        :visible.sync="showDialog"
        :close-on-click-modal="false"
        :before-close="onCancelClick"
      >
        <el-form
          class="h-600"
          :model="formModel"
          :rules="rules"
          ref="fixedScheduleEmployee"
          label-width="168px"
          label-position="top"
        >
          <el-row>
            <el-col :span="11">
              <el-form-item
                :label="$t('Department')"
                prop="DepartmentList"
                :placeholder="$t('SelectDepartment')"
              >
                <!-- <select-department v-model="formModel.DepartmentIndex"
                                     :multiple="false"
                                     :disabled="isEdit"></select-department> -->
                <select-department-tree-component
                  :defaultExpandAll="tree.defaultExpandAll"
                  :multiple="true"
                  :placeholder="$t('SelectDepartment')"
                  :disabled="isEdit"
                  :data="tree.treeData"
                  :props="tree.treeProps"
                  :isSelectParent="true"
                  :clearable="tree.clearable"
                  :popoverWidth="tree.popoverWidth"
                  :check-strictly="tree.checkStrictly"
                  v-model="formModel.DepartmentList"
                  @change="onChangeDepartmentForm"
                  style="width: 100%"
                  class="hr-employee-info__select-department-tree"
                ></select-department-tree-component>
              </el-form-item>
            </el-col>
            <el-col :span="11" :offset="1">
              <el-form-item prop="EmployeeATIDs" :label="$t('Employee')">
                <app-select-new
                  :disabled="isEdit"
                  :dataSource="employeeFullLookup"
                  displayMember="NameInFilter"
                  valueMember="Index"
                  :allowNull="true"
                  v-model="formModel.EmployeeATIDs"
                  :multiple="true"
                  :placeholder="$t('SelectEmployee')"
                  @getValueSelectedAll="selectAllEmployeeForm"
                  ref="employeeList"
                >
                </app-select-new>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row>
            <el-col :span="11">
              <el-form-item
                :label="$t('AppliedDate')"
                @click.native="focus('FromDate')"
                prop="FromDate"
              >
                <el-date-picker
                  v-model="formModel.FromDate"
                  type="date"
                  :placeholder="$t('AppliedDate')"
                  style="width: 100% ;margin-bottom: 5px;"
                  :clearable="false"
                  :editable="false"
                >
                </el-date-picker>
              </el-form-item>
            </el-col>
            <el-col :span="11" :offset="1">
              <el-form-item
                :label="$t('ToDateString')"
                @click.native="focus('ToDate')"
                prop="ToDate"
              >
                <el-date-picker
                  v-model="formModel.ToDate"
                  type="date"
                  :placeholder="$t('ToDateString')"
                  style="width: 100%"
                  :clearable="true"
                  :editable="false"
                >
                </el-date-picker>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row>
            <el-col :span="24">
              <el-form-item>
                <label class="el-form-item__label hozitalClass">{{
                  $t("Calendar")
                }}</label>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row>
            <el-col :span="11">
              <el-form-item
                :label="$t('Monday')"
                @click.native="focus('Monday')"
                prop="Monday"
              >
                <el-select
                  filterable
                  :placeholder="$t('SelectShift')"
                  v-model="formModel.Monday"
                  style="padding: 0 5px"
                  :clearable="true"
                >
                  <el-option
                    v-for="item in listShift"
                    :key="item.value"
                    :label="$t(item.Code)"
                    :value="item.Index"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="11" :offset="1">
              <el-form-item
                :label="$t('Tuesday')"
                @click.native="focus('Tuesday')"
                prop="Tuesday"
              >
                <el-select
                  filterable
                  :placeholder="$t('SelectShift')"
                  v-model="formModel.Tuesday"
                  style="padding: 0 5px"
                  :clearable="true"
                >
                  <el-option
                    v-for="item in listShift"
                    :key="item.value"
                    :label="$t(item.Code)"
                    :value="item.Index"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row>
            <el-col :span="11">
              <el-form-item
                :label="$t('Wednesday')"
                @click.native="focus('Wednesday')"
                prop="Wednesday"
              >
                <el-select
                  filterable
                  :placeholder="$t('SelectShift')"
                  v-model="formModel.Wednesday"
                  style="padding: 0 5px"
                  :clearable="true"
                >
                  <el-option
                    v-for="item in listShift"
                    :key="item.value"
                    :label="$t(item.Code)"
                    :value="item.Index"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="11" :offset="1">
              <el-form-item
                :label="$t('Thursday')"
                @click.native="focus('Thursday')"
                prop="Thursday"
              >
                <el-select
                  filterable
                  :placeholder="$t('SelectShift')"
                  v-model="formModel.Thursday"
                  style="padding: 0 5px"
                  :clearable="true"
                >
                  <el-option
                    v-for="item in listShift"
                    :key="item.value"
                    :label="$t(item.Code)"
                    :value="item.Index"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row>
            <el-col :span="11">
              <el-form-item
                :label="$t('Friday')"
                @click.native="focus('Friday')"
                prop="Friday"
              >
                <el-select
                  filterable
                  :placeholder="$t('SelectShift')"
                  v-model="formModel.Friday"
                  style="padding: 0 5px"
                  :clearable="true"
                >
                  <el-option
                    v-for="item in listShift"
                    :key="item.value"
                    :label="$t(item.Code)"
                    :value="item.Index"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="11" :offset="1">
              <el-form-item
                :label="$t('Saturday')"
                @click.native="focus('Saturday')"
                prop="Saturday"
              >
                <el-select
                  filterable
                  :placeholder="$t('SelectShift')"
                  v-model="formModel.Saturday"
                  style="padding: 0 5px"
                  :clearable="true"
                >
                  <el-option
                    v-for="item in listShift"
                    :key="item.value"
                    :label="$t(item.Code)"
                    :value="item.Index"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row>
            <el-col :span="11">
              <el-form-item
                :label="$t('Sunday')"
                @click.native="focus('Sunday')"
                prop="Sunday"
              >
                <el-select
                  filterable
                  :placeholder="$t('SelectShift')"
                  v-model="formModel.Sunday"
                  style="padding: 0 5px"
                  :clearable="true"
                >
                  <el-option
                    v-for="item in listShift"
                    :key="item.value"
                    :label="$t(item.Code)"
                    :value="item.Index"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>

        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="onCancelClick">
            {{ $t("Cancel") }}
          </el-button>
          <el-button class="btnOK" type="primary" @click="onSubmitClick">
            {{ $t("OK") }}
          </el-button>
        </span>
      </el-dialog>
    </div>

    <div class="tab-modal-delete">
      <el-dialog
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
          <!-- <el-form-item>
            <el-checkbox v-model="isDeleteOnDevice">
              {{ $t("MSG_ConfirmDelete") }}
            </el-checkbox>
          </el-form-item> -->
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
    <div class="tab-modal-excel">
      <!--Dialog chosse excel-->
      <el-dialog
        :title="isAddFromExcel ? $t('AddFromExcel') : $t('DeleteFromExcel')"
        custom-class="customdialog"
        :visible.sync="showDialogExcel"
        @close="AddOrDeleteFromExcel('close')"
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
                accept=".xls, .xlsx"
                type="file"
                name="file-3[]"
                id="fileUploadScheduleEmployee"
                class="inputfile inputfile-3"
                @change="processFile($event)"
              />
              <label for="fileUploadScheduleEmployee">
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
                <!-- <span>Choose a file&hellip;</span> -->
                <span>{{ $t("ChooseAExcelFile") }}</span>
              </label>
              <span v-if="fileName === ''" class="fileName">
                {{ $t("NoFileChoosen") }}
              </span>
              <span v-else class="fileName">{{ fileName }}</span>
            </div>
          </el-form-item>
          <div>
            <el-form-item :label="$t('DownloadTemplate')">
              <a
                class="fileTemplate-lbl"
                href="/Template_ScheduleEmployee.xlsx"
                download
                >{{ $t("Download") }}</a
              >
            </el-form-item>
          </div>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="AddOrDeleteFromExcel('close')">
            {{ $t("Cancel") }}
          </el-button>
          <el-button
            v-if="isAddFromExcel"
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
              href="/Files/Template_ScheduleEmployee_Error.xlsx"
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
  </div>
</template>
<script src="./ta-fixed-employee-schedule.ts"></script>
<style lang="scss">
.hozitalClass {
  display: flex !important;
  flex-direction: row;
}
.hozitalClass:after {
  content: "";
  flex: 1 1;
  border-bottom: 1px solid #000;
  margin: auto;
}

.ta-fixed-employee-schedule__filter-employee .el-select__tags:not(:has(span span:nth-child(2))) {
  top: 50% !important;
  max-width: 100% !important;

  span span:first-child {
    width: max-content;
    max-width: calc(100% - 20%);
    margin-left: 1px;

    .el-select__tags-text {
      vertical-align: top;
      width: max-content;
      max-width: 90%;
      display: inline-block;
      text-overflow: ellipsis;
      overflow: hidden;
    }
  }
}

.ta-fixed-employee-schedule__filter-employee .el-select__tags:has(span span:nth-child(2)) {
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
#fileUploadScheduleEmployee {
	opacity: 0;
	position: absolute;
	z-index: -1;
}
</style>