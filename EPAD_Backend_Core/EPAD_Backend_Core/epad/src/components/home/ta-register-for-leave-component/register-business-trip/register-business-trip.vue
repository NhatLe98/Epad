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
            v-model="filterDepartment"
            style="width: calc(100% - 10px); margin-right: 10px;"
            @change="onChangeDepartmentFilter"
          ></select-department-tree-component>
        </el-col>
        <el-col :span="4">
          <app-select-new class="register-business-trip__filter-employee"
          :dataSource="employeeFullLookupFilter"
          displayMember="NameInFilter"
          valueMember="Index"
          :allowNull="true"
          v-model="filterModel.ListEmployeeATID"
          :multiple="true"
          style="margin-right: 10px; width: calc(100% - 10px)"
          :placeholder="$t('SelectEmployee')"
          @getValueSelectedAll="selectAllEmployeeFilter"
          ref="employeeList"
        >
        </app-select-new>
        </el-col>
        <el-col :span="3">
          <el-date-picker
            style="padding-right: 10px"
            v-model="filterModel.FromDate"
            format="dd/MM/yyyy"
            type="date"
            :clearable="false"
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
            :placeholder="$t('WorkingToDate')"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="5">
          <el-input
            style="padding-bottom: 3px; width: 100%;"
            :placeholder="$t('InputMCCMNVName')"
            suffix-icon="el-icon-search"
            v-model="filterModel.TextboxSearch"
            @keyup.enter.native="onViewClick"
            class="filter-input"
          ></el-input>
        </el-col>
        <el-col :span="4">
          <el-button
            type="primary"
            class="smallbutton"
            size="small"
            @click="onViewClick"
            style="margin-left: 10px;"
          >
            {{ $t("View") }}
          </el-button>
          <el-dropdown
            style="margin-left: 10px; margin-top: 5px"
            v-if="showMore"
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
        <el-col :span="1" style="float: right">
          <TableToolbar ref="customerDataTableFunction" :gridColumnConfig.sync="columnDefs" />
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
      ref="registerBusinessPagination"
        :page.sync="page"
        :pageSize.sync="pageSize"
        :getData="loadData"
        :total="total"
      />
    </div>

    <div class="tab-modal">
      <el-dialog
        custom-class="customdialog"
        width="800px"
        :title="isEdit ? $t('EditBusinessRegistration') : $t('InsertBusinessRegistration')"
        :visible.sync="showDialog"
        :close-on-click-modal="false"
        :before-close="onCancelClick"
      >
        <el-form
          class="h-600"
          :model="formModel"
          :rules="formRules"
          ref="registerBusinessModel"
          label-width="168px"
          label-position="top"
        >
          <el-row :gutter="10">
            <el-col :span="12">
              <el-form-item
                :label="$t('Department')"
                :placeholder="$t('SelectDepartment')"
              >
                <select-department-tree-component v-if="!isEdit"
                  :defaultExpandAll="tree.defaultExpandAll"
                  :multiple="true"
                  :placeholder="$t('SelectDepartment')"
                  :disabled="isEdit"
                  :data="tree.treeData"
                  :props="tree.treeProps"
                  :isSelectParent="true"
                  :checkStrictly="tree.checkStrictly"
                  :clearable="tree.clearable"
                  :popoverWidth="tree.popoverWidth"
                  v-model="formDepartment"
                  style="width: 100%"
                  class="hr-employee-info__select-department-tree"
                  @change="onChangeDepartmentForm"
                ></select-department-tree-component>
                <select-department-tree-component v-else
                  :defaultExpandAll="tree.defaultExpandAll"
                  :multiple="false"
                  :placeholder="$t('SelectDepartment')"
                  :disabled="isEdit"
                  :data="tree.treeData"
                  :props="tree.treeProps"
                  :isSelectParent="true"
                  :checkStrictly="tree.checkStrictly"
                  :clearable="tree.clearable"
                  :popoverWidth="tree.popoverWidth"
                  v-model="formDepartment[0]"
                  style="width: 100%"
                  class="hr-employee-info__select-department-tree"
                  @change="onChangeDepartmentForm"
                ></select-department-tree-component>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item
                prop="ListEmployeeATID"
                :label="$t('Employee')"
              >
                <app-select-new
                  :disabled="isEdit"
                  :dataSource="employeeFullLookupForm"
                  displayMember="NameInFilter"
                  valueMember="Index"
                  :allowNull="true"
                  v-model="formModel.ListEmployeeATID"
                  :multiple="true"
                  :placeholder="$t('SelectEmployee')"
                  @getValueSelectedAll="selectAllEmployeeForm"
                  ref="employeeList"
                >
                </app-select-new>
              </el-form-item>
            </el-col>
            <el-col :span="12" v-if="!isEdit">
              <el-form-item :label="$t('FromDateString')" prop="FromDate">
                <el-date-picker
                  ref="FromDate"
                  v-model="formModel.FromDate"
                  type="date"
                  :clearable="false"
                ></el-date-picker>
              </el-form-item>
            </el-col>
            <el-col :span="12" v-if="!isEdit">
              <el-form-item :label="$t('ToDateString')" prop="ToDate">
                <el-date-picker
                  ref="ToDate"
                  v-model="formModel.ToDate"
                  type="date"
                  :clearable="false"
                ></el-date-picker>
              </el-form-item>
            </el-col>
            <el-col :span="12" v-if="isEdit">
              <el-form-item :label="$t('BusinessDate')" prop="BusinessDate">
                <el-date-picker
                  ref="BusinessDate"
                  v-model="formModel.BusinessDate"
                  type="date"
                  :clearable="false"
                ></el-date-picker>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item
                :label="$t('TypeBusinessTrip')"
                prop="BusinessType"
                :placeholder="$t('SelectTypeBusinessTrip')"
              >
                <el-select v-model="formModel.BusinessType">
                  <el-option
                    v-for="item in TypeBusinessTrip"
                    :key="item.Index"
                    :label="item.Name"
                    :value="item.Index"
                  >
                  </el-option>
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="isEdit ? 24 : 12">
              <el-form-item :label="$t('WorkPlace')" prop="WorkPlace">
                <el-input
                  ref="WorkPlace"
                  v-model="formModel.WorkPlace"
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="12" v-if="formModel.BusinessType == 2">
              <el-form-item :label="$t('StartTime')" prop="FromTime">
                <el-time-picker
                      format="HH:mm"
                        v-model="formModel.FromTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
              </el-form-item>
            </el-col>
            <el-col :span="12" v-if="formModel.BusinessType == 2">
              <el-form-item :label="$t('EndTime')" prop="ToTime">
                <el-time-picker
                      format="HH:mm"
                        v-model="formModel.ToTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
              </el-form-item>
            </el-col>
            
            <!-- <el-form-item
            :label="$t('TypeBusinessTrip')"
            prop="TypeBusinessTrip"
            :placeholder="$t('SelectTypeBusinessTrip')"
            v-if="formModel.TypeBusinessTrip == 2"
          >
            <el-select v-model="formModel.TypeBusinessTripDetailIndex" :disabled="isEdit">
              <el-option
                v-for="item in TypeBusinessTripDetail"
                :key="item.Index"
                :label="item.Name"
                :value="item.Index"
                :disabled="isEdit"
              >
              </el-option>
            </el-select>
          </el-form-item> -->
            <el-col :span="24">
              <el-form-item :label="$t('Reason')" prop="Note">
                <el-input
                  ref="Note"
                  v-model="formModel.Description"
                ></el-input>
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
                id="fileUploadBusiness"
                class="inputfile inputfile-3"
                @change="processFile($event)"
              />
              <label for="fileUploadBusiness">
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
                href="/Mission.xlsx"
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
          <el-form-item>
            <a
              class="fileTemplate-lbl"
              href="/Files/MissionError.xlsx"
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
<script src="./register-business-trip.ts"></script>
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

.el-message-box__message{
  max-height: 50vh;
  overflow-y: scroll;
}

#fileUploadBusiness {
	opacity: 0;
	position: absolute;
	z-index: -1;
}

.register-business-trip__filter-employee .el-select__tags:not(:has(span span:nth-child(2))) {
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

.register-business-trip__filter-employee .el-select__tags:has(span span:nth-child(2)) {
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