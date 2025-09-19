<template>
  <div class="tab" style="height: calc(100vh - 185px)">
    <div class="tab-filter">
      <el-row>
        <span style="line-height: 28px; float: left; margin-right: 10px"
          >Lọc danh sách</span
        >
        <el-col :span="3">
          <el-input
            style="padding-bottom: 3px; width: calc(100% - 10px)"
            :placeholder="$t('VehiclePlate')"
            v-model="filterModel.VehiclePlate"
            @keyup.enter.native="onViewClick"
            class="filter-input"
          ></el-input>
        </el-col>
        <el-col :span="3">
          <el-select
            class="hr-driver-info__driver-status-select"
            props="Status"
            v-model="filterModel.Status"
            clearable
            collapse-tags
            multiple
            style="padding-right: 10px"
            ref="Status"
            :placeholder="$t('SelectDriverStatus')"
          >
            <el-option
              v-for="item in lstStatus"
              :key="item.Value"
              :label="$t(item.Label)"
              :value="item.Value"
            ></el-option>
          </el-select>
        </el-col>
        <el-col :span="3">
        <select-department-tree-component
            :defaultExpandAll="tree.defaultExpandAll"
            :multiple="tree.multiple"
            :placeholder="$t('SelectSupplier')"
            :disabled="tree.isEdit"
            :data="tree.treeData"
            :props="tree.treeProps"
            :isSelectParent="true"
            :checkStrictly="tree.checkStrictly"
            :clearable="tree.clearable"
            :popoverWidth="tree.popoverWidth"
            v-model="filterModel.SelectedDepartment"
            style="padding: 0 10px; width: 100%"
          ></select-department-tree-component>
        </el-col>
        <el-col :span="3">
          <el-date-picker
            class="hr-driver-info__date-picker"
            style="padding-right: 10px"
            v-model="filterModel.FromDate"
            format="dd/MM/yyyy"
            type="date"
            :placeholder="$t('WorkingFromDate')"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="3">
          <el-date-picker
            class="hr-driver-info__date-picker"
            style="padding-right: 10px"
            v-model="filterModel.ToDate"
            format="dd/MM/yyyy"
            type="date"
            :placeholder="$t('WorkingToDate')"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="3">
          <el-input
            style="padding-bottom: 3px; width: calc(100% - 10px)"
            :placeholder="$t('SearchData')"
            suffix-icon="el-icon-search"
            v-model="filterModel.TextboxSearch"
            @keyup.enter.native="onViewClick"
            class="filter-input hr-driver-info__filter-input"
          ></el-input>
        </el-col>
        <el-col :span="3">
          <el-button
            type="primary"
            class="smallbutton"
            size="small"
            @click="onViewClick"
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
          <TableToolbar
            ref="customerDataTableFunction"
            :gridColumnConfig.sync="columnDefs"
          />
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
        style="height: calc(100% - 45px)"
      />
      <AppPagination
        ref="taDriverInfoPagination"
        :page.sync="page"
        :pageSize.sync="pageSize"
        :getData="loadData"
        :total="total"
      />
    </div>

    <div class="tab-modal">
      <el-dialog
        custom-class="customdialog"
        width="1000px"
        :title="isEdit ? $t('EditEmployee') : $t('InsertDriver')"
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
            <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
              <el-form-item :label="$t('UserImage')">
                <el-upload
                  class="avatar-uploader"
                  action=""
                  accept="image/png, image/jpeg"
                  :multiple="false"
                  :file-list="fileList"
                  :auto-upload="false"
                  :on-change="onChangeAvatar"
                  :on-remove="onRemoveAvatar"
                >
                  <img
                    class="avatar"
                    v-if="formModel.Avatar && !errorUpload"
                    :src="'data:image/jpeg;base64, ' + formModel.Avatar"
                  />
                  <i
                    v-else
                    class="el-icon-plus avatar-uploader-icon"
                    style="width: 100%"
                  ></i>
                </el-upload>
              </el-form-item>
            </el-col>

            <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
              <el-form-item :label="$t('TripCode')" prop="TripId">
                <el-input
                  v-model="formModel.TripId"
                  :disabled="isEdit"
                ></el-input>
              </el-form-item>

              <el-form-item :label="$t('DeliveryPoint')" prop="LocationFrom">
                <el-input v-model="formModel.LocationFrom"></el-input>
              </el-form-item>

              <el-form-item :label="$t('VehiclePlate')" prop="TrailerNumber">
                <el-input v-model="formModel.TrailerNumber"></el-input>
              </el-form-item>

              <el-form-item :label="$t('CCCDNumber')" prop="DriverCode">
                <el-input v-model="formModel.DriverCode"></el-input>
              </el-form-item>
              <el-form-item label="" style="padding-top: 50px" prop="Vc">
                <el-checkbox v-model="formModel.Vc">
                  {{ $t("IsPassingVehicle") }}
                </el-checkbox>
              </el-form-item>

              <el-form-item
                :label="$t('VehicleStatus')"
                prop="VehicleStatus"
                :placeholder="$t('ChooseVehicleStatus')"
              >
                <el-select v-model="formModel.StatusDock">
                  <el-option
                    v-for="item in lstStatus"
                    :key="item.Value"
                    :label="item.Label"
                    :value="item.Value"
                  >
                  </el-option>
                </el-select>
              </el-form-item>
              <el-form-item :label="$t('Supplier')">
                <select-department-tree-component
                  :defaultExpandAll="tree.defaultExpandAll"
                  :multiple="false"
                  :placeholder="$t('SelectSupplier')"
                  :data="tree.treeData"
                  :props="tree.treeProps"
                  :checkStrictly="true"
                  :clearable="tree.clearable"
                  :popoverWidth="tree.popoverWidth"
                  v-model="formModel.SupplierId"
                  style="width: 100%; margin-bottom: 20px"
                ></select-department-tree-component>
              </el-form-item>
              <el-form-item :label="$t('Activity')">
                <el-input v-model="formModel.Operation"></el-input>
              </el-form-item>
            </el-col>

            <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
              <el-form-item :label="$t('OrderCode')" prop="OrderCode">
                <el-input v-model="formModel.OrderCode"></el-input>
              </el-form-item>

              <el-form-item :label="$t('FullName')" prop="DriverName">
                <el-input v-model="formModel.DriverName"></el-input>
              </el-form-item>
              <el-form-item :label="$t('MobilePhone')" prop="DriverPhone">
                <el-input v-model="formModel.DriverPhone"></el-input>
              </el-form-item>
              <el-form-item :label="$t('BirthDay')" prop="BirthDay">
                <el-date-picker
                  ref="BirthDay"
                  v-model="formModel.BirthDay"
                  type="date"
                ></el-date-picker>
              </el-form-item>
              <el-form-item :label="$t('ETA')" prop="Eta">
                <el-date-picker
                  v-model="formModel.Eta"
                  type="datetime"
                  :placeholder="$t('ChooseEta')"
                >
                </el-date-picker>
              </el-form-item>

              <el-form-item :label="$t('PlanDockTime')">
                <el-date-picker
                  v-model="formModel.TimesDock"
                  type="datetime"
                  :placeholder="$t('ChoosePlanDockTime')"
                >
                </el-date-picker>
              </el-form-item>
              <el-form-item :label="$t('Type')">
                <el-input v-model="formModel.Type"></el-input>
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

    <div class="tab-modal-finger">
      <el-dialog
        :title="$t('DialogOption')"
        custom-class="customDialogEmployee"
        :visible.sync="showFingerDialog"
        :before-close="cancelRegisterFingerDialog"
      >
        <el-form label-position="top" label-width="168px">
          <el-row>
            <el-col
              :span="3"
              style="margin-right: 25px; margin-left: 25px"
              v-for="item in listFinger"
              :key="item.ID"
            >
              <el-form-item :label="$t('Finger' + item.ID)" v-if="item.ID < 6">
                <el-card
                  style="cursor: pointer"
                  v-bind:class="{ 'has-focus': item.FocusFinger }"
                >
                  <img
                    style="height: 130px; width: 100%"
                    @click="onFocusFinger(item.ID)"
                    v-bind:src="
                      item.ImageFinger ||
                      getImgUrl('base_fpVerify_clearImage.png')
                    "
                    class="image"
                  />
                </el-card>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row>
            <el-col
              :span="3"
              style="margin-right: 25px; margin-left: 25px"
              v-for="item in listFinger"
              :key="item.ID"
            >
              <el-form-item :label="$t('Finger' + item.ID)" v-if="item.ID > 5">
                <el-card
                  style="cursor: pointer"
                  v-bind:class="{ 'has-focus': item.FocusFinger }"
                >
                  <img
                    style="height: 130px; width: 100%"
                    @click="onFocusFinger(item.ID)"
                    :src="
                      item.ImageFinger ||
                      getImgUrl('base_fpVerify_clearImage.png')
                    "
                    class="image"
                  />
                </el-card>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <h4>
            <span
              style="float: left"
              v-bind:class="[
                { connected: ConnectedDevice == 2 },
                { connecting: ConnectedDevice == 1 },
                { 'not-connect': ConnectedDevice == 0 },
              ]"
            >
              {{
                ConnectedDevice == 2
                  ? $t("ConnectedFingerDevice")
                  : ConnectedDevice == 1
                  ? $t("ConnectingFingerDevice")
                  : $t("NotConnectedDevice")
              }}
            </span>
          </h4>
          <el-button class="" @click="reconnect">
            {{ $t("ReConnectDevice") }}
          </el-button>
          <el-button class="btnCancel" @click="cancelRegisterFingerDialog">
            {{ $t("Cancel") }}
          </el-button>
          <el-button class="btnOK" type="primary" @click="submitRegisterFinger">
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
          <div>
            <el-form-item
              :label="$t('DownloadTemplate')"
              v-if="isDeleteFromExcel === true"
            >
              <a
                class="fileTemplate-lbl"
                href="/Template_Driver.xlsx"
                download
                >{{ $t("Download") }}</a
              >
            </el-form-item>
            <el-form-item :label="$t('DownloadTemplate')" v-else>
              <a
                class="fileTemplate-lbl"
                href="/Template_Driver.xlsx"
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
              href="/Files/DriverInfoError.xlsx"
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
  <script src="./hr-driver-info.ts"></script>
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

.el-input__icon {
  height: 115%;
}

.hr-driver-info__date-picker {
  .el-input__suffix .el-input__suffix-inner {
    padding: 0 5px 0 0 !important;
  }
}

.hr-driver-info__driver-status-select {
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

.hr-driver-info__filter-input {
  .el-input__icon.el-icon-search {
    height: 32px !important;
  }
}
</style>