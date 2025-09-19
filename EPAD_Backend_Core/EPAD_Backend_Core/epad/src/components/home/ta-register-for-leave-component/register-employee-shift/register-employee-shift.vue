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
            v-model="filterModel.DepartmentIds"
            @change="onChangeDepartmentFilter"
            style="width: calc(100% - 10px); margin-right: 10px;"
          ></select-department-tree-component>
          <!-- <select-department v-else v-model="filterModel.SelectedDepartment"
                        clearable
                        multiple
                        collapse-tags
                        style="padding: 0 5px; width: 100%"></select-department> -->
        </el-col>
        <el-col :span="5">
          <app-select-new class="register-employee-shift__filter-employee"
            :dataSource="employeeFullLookupForm"
            displayMember="NameInFilter"
            valueMember="Index"
            :allowNull="true"
            v-model="filterModel.EmployeeATIDs"
            :multiple="true"
            style="width: 100%;padding-right: 10px"
            :placeholder="$t('SelectEmployee')"
            ref="employeeList"
             @getValueSelectedAll="selectAllEmployeeFilter"
          >
          </app-select-new>
          <!-- <select-department v-else v-model="filterModel.SelectedDepartment"
                        clearable
                        multiple
                        collapse-tags
                        style="padding: 0 5px; width: 100%"></select-department> -->
        </el-col>
        <el-col :span="3">
          <el-date-picker
            style="padding-right: 10px"
            v-model="filterModel.FromDate"
            format="dd/MM/yyyy"
            type="date"
            :clearable="false"
            :editable="false"
            :placeholder="$t('WorkingFromDate')"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="3">
          <el-date-picker
            style="padding-right: 10px"
            v-model="filterModel.ToDate"
            format="dd/MM/yyyy"
            type="date"
            :clearable="false"
            :editable="false"
            :placeholder="$t('WorkingToDate')"
          >
          </el-date-picker>
        </el-col>
        
        <el-col :span="3">
          <el-tooltip
          class="item"
          effect="dark"
          :content="$t('ChooseEmployeeNeedView')"
          placement="bottom-end"
          v-if="!filterModel.EmployeeATIDs.length || filterModel.EmployeeATIDs.length == 0"
        >
        <el-button
        type="primary"
        class="smallbutton"
        size="small"
        :disabled="isLoading"
        @click="onViewClick"
      >
        {{ $t("View") }}
      </el-button>
        </el-tooltip>

          <el-button
            type="primary"
            class="smallbutton"
            size="small"
            @click="onViewClick"
            v-if="filterModel.EmployeeATIDs.length > 0"
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
        <el-col :span="1" style="float:right;">
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

    <div class="tab-grid" style="height: calc(100% - 10px) !important; margin-top: 10px">
      <el-table
      :data="listData"
      style="width: 100%"
      border
      :height="800"
      @selection-change="handleSelectionChange"
      @row-click="handleRowClick"
      :selected-rows.sync="rowsObj"
      v-loading="isLoading"
      row-key="EmployeeATID"
      class="ajust-time-attendance__table"
    >
    <el-table-column type="selection" width="55" v-if="listColumn.length >= 0"></el-table-column>
        <template v-for="column in listColumn">
          <el-table-column
            :prop="column.dataField"
            :label="column.name"
            :key="column.dataField"
            align="center"
            v-if="column.index <= 3"
            fixed
            min-width="150"
          >
            <template slot-scope="scope">
              <span v-if="column.index <= 3 || column.index >= 50">{{
                scope.row[column.dataField]
              }}</span>
              <app-select
                v-else
                :dataSource="listShift"
                displayMember="Code"
                valueMember="Index"
                :allowNull="true"
                :editable="false"
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
            min-width="150"
            v-else
          >
            <template slot-scope="scope">
              <span v-if="column.index <= 3 || column.index >= 100">{{
                scope.row[column.dataField]
              }}</span>
              <app-select
                v-else
                :dataSource="listShift"
                displayMember="Code"
                valueMember="Index"
                :allowNull="true"
                :editable="false"
                v-model="scope.row[column.dataField]"
                style="width: 100%; text-align-last: center"
              ></app-select>
            </template>
          </el-table-column>
        </template>
      </el-table>
      <AppPagination
        :page.sync="page"
        :pageSize.sync="pageSize"
        :getData="loadData"
        :total="total"
      />
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
                id="fileUploadEmployeeShift"
                class="inputfile inputfile-3"
                @change="processFile($event)"
              />
              <el-upload
              accept="
                application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
              "
              action="/"
              :auto-upload="false"
              :file-list="fileList"
              :multiple="false"
              :on-change="importFileV1"
              :on-remove="
                () => {
                  $emit('update:activeObj', []);
                }
              "
              :show-file-list="true">
              <el-row slot="trigger" :gutter="10" type="flex">
                <el-col>
                  <el-button icon="el-icon-upload" plain size="small" type="danger">
                    {{ $t('SelectFile') }}
                  </el-button>
                </el-col>
              </el-row>
            </el-upload>
            </div>
          </el-form-item>
          <div>
            <el-form-item :label="$t('DownloadTemplate')">
              <a
                class="fileTemplate-lbl"
                href="/Template_EmployeeShift.xlsx"
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
              href="/Files/Template_EmployeeShift_Error.xlsx"
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
<script src="./register-employee-shift.ts"></script>
<style lang="scss">
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
#fileUploadEmployeeShift {
	opacity: 0;
	position: absolute;
	z-index: -1;
}
.ajust-time-attendance__table{
  width: 100% !important; 
  height: calc(100vh - 265px) !important;
  .el-table__body-wrapper{
    height: calc(100vh - 305px) !important;
  }
  .el-table__fixed::before{
    height: 0 !important;
  }
  .el-table__fixed{
    height: calc(100vh - 278px) !important;
    .el-table__fixed-body-wrapper{
      top: 35px !important;
      height: calc(100vh - 312px) !important;
    }
  }
}

.register-employee-shift__filter-employee .el-select__tags:not(:has(span span:nth-child(2))) {
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

.register-employee-shift__filter-employee .el-select__tags:has(span span:nth-child(2)) {
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
</style>