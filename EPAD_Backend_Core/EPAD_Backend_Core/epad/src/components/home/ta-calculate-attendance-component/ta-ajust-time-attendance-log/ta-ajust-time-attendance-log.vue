<template>
  <div class="tab" style="height: calc(100vh - 185px)">
    <div class="tab-filter">
      <el-row>
        <el-col :span="4">
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
            @change="onChangeDepartmentFilter"
            v-model="filterModel.DepartmentIds"
            style="width: 100%"
          ></select-department-tree-component>
        </el-col>
        <el-col :span="4" style="margin-left: 10px">
          <app-select-new class="ta-adjust-time-attendance-log__filter-employee"
            :disabled="isEdit"
            :dataSource="employeeFullLookup"
            displayMember="NameInFilter"
            valueMember="Index"
            :allowNull="true"
            v-model="filterModel.EmployeeATIDs"
            :multiple="true"
            style="width: 100%"
            :placeholder="$t('SelectEmployee')"
            @getValueSelectedAll="selectAllEmployeeFilter"
            ref="employeeList"
          >
          </app-select-new>
        </el-col>
        <el-col :span="3"  style="margin-left: 10px">
          <el-date-picker
            v-model="filterModel.FromDate"
            type="date"
            format="dd/MM/yyyy"
            :placeholder="$t('FromDateString')"
            style="width: 100%"
            :clearable="false"
            :editable="false"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="3" style="margin-left: 10px">
          <el-date-picker
            v-model="filterModel.ToDate"
            format="dd/MM/yyyy"
            type="date"
            :placeholder="$t('WorkingToDate')"
            :clearable="false"
            :editable="false"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="4" style="margin-left: 10px">
          <el-input
            :placeholder="$t('InputMCCMNVName')"
            v-model="filter"
            class="filter-input"
            @keyup.enter.native="onViewClick"
          >
            <i slot="suffix" class="el-icon-search" @click="onViewClick"></i>
          </el-input>
        </el-col>
        <el-col :span="1" style="margin-left: 10px">
          <el-button
            type="primary"
            class="smallbutton"
            size="small"
            @click="onViewClick"
            :disabled="isLoading"
          >
            {{ $t("View") }}
          </el-button>
        </el-col>
        <el-col :span="1" style="margin-left: 10px">
          <el-tooltip
            class="item"
            effect="dark"
            :content="$t('ChooseUpdate')"
            placement="bottom-end"
            v-if="!rowsObj || rowsObj.length == 0"
          >
            <el-button
              type="primary"
              class="smallbutton"
              size="small"
              disabled
              @click="saveData"
            >
              {{ $t("Save") }}
            </el-button>
          </el-tooltip>

          <el-button
            type="primary"
            class="smallbutton"
            size="small"
            v-if="rowsObj.length > 0"
            @click="saveData"
          >
            {{ $t("Save") }}
          </el-button>
        </el-col>
      </el-row>
    </div>

    <div
      class="tab-grid"
      style="height: calc(100% - 10px) !important; margin-top: 10px"
    >
      <el-table
        :data="listData"
        style="width: 100%"
        border
        :height="700"
        @selection-change="handleSelectionChange"
        @row-click="handleRowClick"
        :selected-rows.sync="rowsObj"
        row-key="EmployeeATID"
        v-loading="isLoading"
        class="ajust-time-attendance__table"
        ref="taAjustTimeAttendanceLogPagination"
      >
        <el-table-column type="selection" width="55" v-if="listColumn.length >= 0"></el-table-column>
        <template v-for="column in listColumn">
          <el-table-column
            :prop="column.dataField"
            :label="column.name"
            :key="column.dataField"
            align="center"
            :min-width="column.index <= 4 || column.index >= 50 ? 200 : 80"
            v-if="column.index <= 4"
            fixed
          >
            <template slot-scope="scope">
              <span v-if="column.index <= 4 || column.index >= 50">{{
                scope.row[column.dataField]
              }}</span>
              <app-select
                class="ajust-time-attendance__table__cell-app-select"
                v-else
                :dataSource="listShift"
                displayMember="label"
                valueMember="value"
                v-model="scope.row[column.dataField]"
                style="width: 100%; text-align-last: center"
              ></app-select>
            </template>
          </el-table-column>
          <el-table-column
            :prop="column.dataField"
            :label="column.name"
            :key="column.dataField"
            align="center"
            :min-width="column.index <= 4 || column.index >= 50 ? 150 : 80"
            v-else
          >
            <template slot-scope="scope">
              <span v-if="column.index <= 4 || column.index >= 50">{{
                scope.row[column.dataField]
              }}</span>
              <app-select
                v-else class="ajust-time-attendance__table__cell-app-select"
                :dataSource="listShift"
                displayMember="label"
                valueMember="value"
                v-model="scope.row[column.dataField]"
                style="width: 100%; text-align-last: center"
              ></app-select>
            </template>
          </el-table-column>
        </template>
      </el-table>
      <!-- <AppPagination
        :page.sync="page"
        :pageSize.sync="pageSize"
        :getData="loadData"
        :total="total"
      /> -->
    </div>

    <div class="tab-modal">
      <el-dialog
        custom-class="customdialog"
        width="800px"
        :title="isEdit ? $t('EditEmployee') : $t('InsertEmployee')"
        :visible.sync="showDialog"
        :close-on-click-modal="false"
        :before-close="onCancelClick"
      >
        <el-form
          class="h-600"
          :model="formModel"
          :rules="formRules"
          ref="employeeFormModel"
          label-width="168px"
          label-position="top"
        >
          <el-row>
            <el-col :span="12" style="padding-right: 30px">
              <el-form-item
                :label="$t('Department')"
                :placeholder="$t('SelectDepartment')"
              >
                <!-- <select-department v-model="formModel.DepartmentIndex"
                                                 :multiple="false"
                                                 :disabled="isEdit"></select-department> -->
                <select-department-tree-component
                  :defaultExpandAll="tree.defaultExpandAll"
                  :multiple="false"
                  :placeholder="$t('SelectDepartment')"
                  :disabled="isEdit"
                  :data="tree.treeData"
                  :props="tree.treeProps"
                  :isSelectParent="true"
                  :checkStrictly="true"
                  :clearable="tree.clearable"
                  :popoverWidth="tree.popoverWidth"
                  v-model="formModel.DepartmentIndex"
                  style="width: 100%"
                  class="hr-employee-info__select-department-tree"
                ></select-department-tree-component>
              </el-form-item>

              <el-form-item :label="$t('FromDateString')" prop="FromDate">
                <el-date-picker
                  :disabled="isEdit"
                  ref="FromDate"
                  v-model="formModel.FromDate"
                  type="date"
                ></el-date-picker>
              </el-form-item>

              <el-form-item
                :label="$t('TypeOfLeaveDate')"
                prop="TypeOfLeaveDate"
                :placeholder="$t('SelectTypeOfLeaveDate')"
              >
                <el-select v-model="formModel.PositionIndex" :disabled="isEdit">
                  <el-option
                    v-for="item in listAllPosition"
                    :key="item.Index"
                    :label="item.Name"
                    :value="item.Index"
                    :disabled="isEdit"
                  >
                  </el-option>
                </el-select>
              </el-form-item>
            </el-col>

            <el-col :span="12">
              <el-form-item
                :label="$t('Employee')"
                prop="EmployeeATID"
                :placeholder="$t('SelectEmployeeATID')"
              >
                <el-select v-model="formModel.PositionIndex" :disabled="isEdit">
                  <el-option
                    v-for="item in listAllPosition"
                    :key="item.Index"
                    :label="item.Name"
                    :value="item.Index"
                    :disabled="isEdit"
                  >
                  </el-option>
                </el-select>
              </el-form-item>

              <el-form-item :label="$t('ToDateString')" prop="ToDate">
                <el-date-picker
                  ref="ToDate"
                  v-model="formModel.ToDate"
                  type="date"
                ></el-date-picker>
              </el-form-item>
              <el-form-item
                :label="$t('DurationType')"
                prop="DurationType"
                :placeholder="$t('SelectPosition')"
              >
                <el-select v-model="formModel.PositionIndex" :disabled="isEdit">
                  <el-option
                    v-for="item in listAllPosition"
                    :key="item.Index"
                    :label="item.Name"
                    :value="item.Index"
                    :disabled="isEdit"
                  >
                  </el-option>
                </el-select>
              </el-form-item>
            </el-col>
            <el-form-item :label="$t('Note')" prop="Note">
              <el-input ref="Note" v-model="formModel.FullName"></el-input>
            </el-form-item>
          </el-row>
        </el-form>

        <el-form-item :label="$t('BirthDay')" prop="BirthDay">
          <el-date-picker
            ref="BirthDay"
            v-model="formModel.BirthDay"
            type="date"
          ></el-date-picker>
        </el-form-item>
        <el-form-item :label="$t('MobilePhone')">
          <el-input ref="Phone" v-model="formModel.Phone"></el-input>
        </el-form-item>
        <el-form-item :label="$t('Address')" prop="Address">
          <el-input ref="Address" v-model="formModel.Address"></el-input>
        </el-form-item>
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
              {{ $t("DeleteEmployeeCofirm") }}
            </span>
          </div>
          <el-form-item>
            <el-checkbox v-model="isDeleteOnDevice">
              {{ $t("DeleteEmployeeOnDeviceHint") }}
            </el-checkbox>
          </el-form-item>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="showDialogDeleteUser = false">
            {{ $t("Cancel") }}
          </el-button>
          <el-button type="primary" @click="doDelete">
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
                <!-- <span>Choose a file&hellip;</span> -->
                <span>{{ $t("ChooseAExcelFile") }}</span>
              </label>
              <span v-if="fileName === ''" class="fileName">
                {{ $t("NoFileChoosen") }}
              </span>
              <span v-else class="fileName">{{ fileName }}</span>
            </div>
          </el-form-item>
          <div v-if="value == 'MAY'">
            <el-form-item
              :label="$t('DownloadTemplate')"
              v-if="isDeleteFromExcel === true"
            >
              <a
                class="fileTemplate-lbl"
                href="/Template_Delete_IC_Employee_MAY.xlsx"
                download
                >{{ $t("Download") }}</a
              >
            </el-form-item>
            <el-form-item :label="$t('DownloadTemplate')" v-else>
              <a
                class="fileTemplate-lbl"
                href="/Template_IC_Employee_MAY.xlsx"
                download
                >{{ $t("Download") }}</a
              >
            </el-form-item>
          </div>
          <div v-else>
            <el-form-item
              :label="$t('DownloadTemplate')"
              v-if="isDeleteFromExcel === true"
            >
              <a
                class="fileTemplate-lbl"
                href="/Template_IC_Employee.xlsx"
                download
                >{{ $t("Download") }}</a
              >
            </el-form-item>
            <el-form-item :label="$t('DownloadTemplate')" v-else>
              <a
                class="fileTemplate-lbl"
                href="/Template_IC_Employee.xlsx"
                download
                >{{ $t("Download") }}</a
              >
            </el-form-item>
          </div>
          <el-form-item v-if="isDeleteFromExcel === true">
            <el-checkbox v-model="isDeleteOnDevice">
              {{ $t("DeleteEmployeeOnDeviceHint") }}
            </el-checkbox>
          </el-form-item>
        </el-form>
        <span slot="footer" class="dialog-footer" v-if="value == 'MAY'">
          <el-button class="btnCancel" @click="AddOrDeleteFromExcel('close')">
            {{ $t("Cancel") }}
          </el-button>
          <el-button
            v-if="isAddFromExcel"
            class="btnOK"
            type="primary"
            @click="UploadDataFromExcel_MAY"
          >
            {{ $t("OK") }}
          </el-button>
          <el-button
            v-else
            class="btnOK"
            type="primary"
            @click="DeleteDataFromExcel_MAY"
          >
            {{ $t("OK") }}
          </el-button>
        </span>
        <span slot="footer" class="dialog-footer" v-else>
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
          <el-button
            v-else
            class="btnOK"
            type="primary"
            @click="DeleteDataFromExcel"
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
          <el-form-item v-if="value == 'MAY'">
            <a
              class="fileTemplate-lbl"
              href="/Files/EmployeesImportError_MAY.xlsx"
              download
              >{{ $t("Download") }}</a
            >
          </el-form-item>
          <el-form-item v-else>
            <a
              class="fileTemplate-lbl"
              href="/Files/EmployeesImportError.xlsx"
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
<script src="./ta-ajust-time-attendance-log.ts"></script>
<style lang="scss" scoped>
.btn-close {
  background-color: red;
}

.contact__title {
  color: black;
  font-weight: bold;
  padding: 20px 0;
  text-transform: uppercase;
  position: relative;
  margin-bottom: 36px;
}

.contact__wrapper {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.contact__container {
  display: flex;
}

.contact__list {
  margin-bottom: 20px;
}

.contact__item {
  display: flex;
  column-gap: 55px;
  row-gap: 20px;
  justify-content: center;
  align-items: center;
  margin-bottom: 20px;
}

.contact__button {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  width: 107px;
}

.contact__formInput {
  margin-bottom: 10px;
}

.el-card__body {
  padding: 0px !important;
}

.has-focus {
  border: cornflowerblue solid 3px;
}

.connecting {
  color: blue;
}

.connected {
  color: limegreen;
}

.not-connect {
  color: red;
}

.hr-employee-info__select-department-tree {
  height: 50px !important;
}

.ta-adjust-time-attendance-log__filter-employee .el-select__tags:not(:has(span span:nth-child(2))) {
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

.ta-adjust-time-attendance-log__filter-employee .el-select__tags:has(span span:nth-child(2)) {
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
/deep/ .ajust-time-attendance__table{
  width: 100% !important; 
  height: calc(100vh - 260) !important;
  .el-table__body-wrapper{
    max-height: calc(100vh - 305px) !important;
    .el-table__empty-text{
      position: fixed;
      top: calc(50vh + 51px);
      left: calc(50vw - 260px);
      z-index: 1;
    }
  }
  .el-table__fixed{
    max-height: calc(100vh - 270px) !important;
    .el-table__fixed-body-wrapper{
      top: 49px !important;
      max-height: calc(100vh - 270px) !important;
    }
  }
  .ajust-time-attendance__table__cell-app-select{
    .el-input__inner {
      border: none !important;
    }
  }
}
</style>