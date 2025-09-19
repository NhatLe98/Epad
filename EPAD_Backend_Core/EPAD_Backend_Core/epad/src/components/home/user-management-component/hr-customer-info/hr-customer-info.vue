<template>
  <div class="tab" style="height: calc(100vh - 185px)">
    <alert-component :show.sync="showWaitScanDialog" :title="titleScanDialog" :message="messageScanDialog" 
      @Cancel="cancelScanDialog" :closeSec="closeSecScanDialog" :type="typeScanDialog" 
      :enableHiddenScanQR="true" @ScanQR="changeScanQR">
    </alert-component>
    <div class="tab-filter">
      <el-row>
        <!-- <el-col :span="6" v-if="this.idEnum == 2" style="margin-right: 10px;">
          <el-select class="hr-customer-info__student-of-parent-select"
            v-model="filterModel.SelectedEmployee"
            filterable
            multiple
            clearable
            collapse-tags
            :placeholder="$t('ContactPerson')"
            style="width: 100%; margin-right: 10px"
          >
            <el-option
              v-for="item in employeeFullLookupFilter"
              :key="item.Index"
              :label="item.NameInFilter"
              :value="item.Index"
            ></el-option>
          </el-select>
        </el-col> -->
        <el-col :span="4" v-if="this.idEnum == 4" style="margin-right: 10px;">
          <el-select class="hr-customer-info__student-of-parent-select"
            v-model="filterStudentOfParent"
            filterable
            multiple
            clearable
            collapse-tags
            :placeholder="$t('Student')"
            style="width: 100%; margin-right: 10px"
          >
            <el-option
              v-for="item in listEmployeeAndDepartment"
              :key="item.EmployeeATID"
              :label="item.EmployeeATID + (item.FullName != '' ? (' - ' + item.FullName) : '')"
              :value="item.EmployeeATID"
            ></el-option>
          </el-select>
        </el-col>
        <el-col :span="4" v-if="this.idEnum == 6">
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
            v-model="filterDepartments"
            style="padding: 0 10px; width: 100%"
          ></select-department-tree-component>
        </el-col>
        <el-col :span="4">
          <el-input
            style="padding-bottom: 3px; width: calc(100% - 10px)"
            :placeholder="$t('SearchData')"
            suffix-icon="el-icon-search"
            v-model="filterModel.TextboxSearch"
            @keyup.enter.native="onViewClick"
            class="filter-input"
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
          <TableToolbar ref="customerDataTableFunction" :gridColumnConfig.sync="columnDefs" />
        </el-col>
      </el-row>
    </div>

    <!-- <div class="tab-grid customer-t-grid" style="height: calc(100% - 35px) !important">
      <t-grid
        @onPageChange="onPageChange"
        @onPageSizeChange="onPageSizeChange"
        :gridColumns="gridColumns.filter((x) => x.display)"
        :dataSource="dataSource1"
        :selectedRows.sync="selectedRows"
        :total="total"
        v-loading="isLoading"
        :page.sync="page"
        :pageSize.sync="pageSize"
      ></t-grid>
    </div> -->
    <div class="tab-grid" style="height: calc(100% - 35px) !important">
      <!-- <t-grid @onPageChange="onPageChange"
                    @onPageSizeChange="onPageSizeChange"
                    :gridColumns="gridColumns"
                    :dataSource="dataSource"
                    :selectedRows.sync="selectedRows"
                    :total="total"
                    :page.sync="page"
                    :pageSize.sync="pageSize"></t-grid> -->
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
        ref="customerInfoPagination"
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
        :title="isEdit ? $t(editFormLabel) : $t(insertFormLabel)"
        :visible.sync="showDialog"
        :before-close="onCancelClick"
        :close-on-click-modal="false"
      >
        <el-form
          class="h-600"
          :model="formModel"
          :rules="formRules"
          ref="customerFormModel"
          label-width="168px"
          label-position="top"
        >
       
        <div v-if="(idEnum == 6 || idEnum == 2) && !isEdit" style="float: right; z-index: 1; position: relative; width: 100%;">
          <el-button type="primary" style="float: right;" 
          @click="openReadInfoFromQRCCCD">
            {{ $t('ReadInfoFromQRCCCD') }}
          </el-button>
        </div>
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
              <el-form-item :label="$t('IdentityImage')" v-if="idEnum == 2 &&  clientName !== 'Mondelez'">
                <el-upload
                  class="avatar-uploader"
                  action=""
                  accept="image/png, image/jpeg"
                  :multiple="false"
                  :file-list="fileListIdentityImage"
                  :auto-upload="false"
                  :on-change="onChangeIdentityImage"
                  :on-remove="onRemoveIdentityImage"
                >
                  <img
                    class="avatar"
                    v-if="formModel.IdentityImage && !errorUpload"
                    :src="'data:image/jpeg;base64, ' + formModel.IdentityImage"
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
              <el-form-item :label="$t('UserCode')" prop="EmployeeATID">
                <el-input
                  v-model="formModel.EmployeeATID"
                  :disabled="isEdit"
                ></el-input>
              </el-form-item>
              <el-form-item :label="$t('FullName')" prop="FullName">
                <el-input
                  ref="FullName"
                  v-model="formModel.FullName"
                ></el-input>
              </el-form-item>
              <el-form-item :label="$t('Gender')" style="margin-bottom: 40px">
                <el-radio-group v-model="formModel.Gender">
                  <el-radio :label="1">{{ $t("Male") }}</el-radio>
                  <el-radio :label="0">{{ $t("Female") }}</el-radio>
                  <el-radio :label="2">{{ $t("Other") }}</el-radio>
                </el-radio-group>
              </el-form-item>
              <el-form-item :label="$t('MobilePhone')" prop="Phone">
                <el-input ref="Phone" v-model="formModel.Phone"></el-input>
              </el-form-item>
              <el-form-item :label="$t('Address')" prop="Address">
                <el-input ref="Address" v-model="formModel.Address"></el-input>
              </el-form-item>

              <el-form-item
                :label="$t('Department')"
                prop="DepartmentIndex"
                v-if="idEnum == 6"
              >
                <!-- <select-department v-model="formModel.DepartmentIndex"
                                                :multiple="false"
                                                ></select-department> -->
                <select-department-tree-component
                  :defaultExpandAll="tree.defaultExpandAll"
                  :multiple="false"
                  :placeholder="$t('SelectDepartment')"
                  :disabled="isEdit"
                  :data="tree.treeData"
                  :props="tree.treeProps"
                  :checkStrictly="true"
                  :clearable="tree.clearable"
                  :popoverWidth="tree.popoverWidth"
                  v-model="formModel.DepartmentIndex"
                  @change="onChangeDepartmentForm"
                  style="width: 100%; margin-bottom: 20px"
                ></select-department-tree-component>
              </el-form-item>

              <!-- Fake block for contractor -->
              <div v-if="idEnum == 6" style="height: 99px;"></div>

              <!-- <el-form-item :label="$t('Password')">
                                <el-input v-model="formModel.Password" type="password"></el-input>
                            </el-form-item> -->
              <el-form-item :label="$t('CompanyName')" v-if="idEnum == 2">
                <el-input ref="Company" v-model="formModel.Company"></el-input>
              </el-form-item>
              <el-form-item
                :label="$t('ContactDepartment')"
                prop="ContactDepartment"
                class="contact_lable_color"
                v-if="idEnum == 2"
              >
                <!-- <el-select
                  v-model="formModel.ContactDepartment"
                  filterable
                  :placeholder="$t('ContactDepartment')"
                >
                  <el-option
                    v-for="item in listEmployeeAndDepartment"
                    :key="item.EmployeeATID"
                    :label="item.DisplayName"
                    :value="item.EmployeeATID"
                  ></el-option>
                </el-select> -->
                <select-department-tree-component
                  :defaultExpandAll="tree.defaultExpandAll"
                  :multiple="false"
                  :placeholder="$t('SelectDepartment')"
                  :disabled="isEdit"
                  :data="tree.treeData"
                  :props="tree.treeProps"
                  :checkStrictly="true"
                  :clearable="tree.clearable"
                  :popoverWidth="tree.popoverWidth"
                  v-model="formModel.ContactDepartment"
                  @value="setValueContactDepartment"
                  style="width: 100%;margin-bottom: 20px;"
                  ref="ContactDepartment"
                  @change="changeContactDepartment"
                ></select-department-tree-component>
                <!--<el-input ref="ContactPerson"
                                          v-model="formModel.ContactPerson"></el-input>-->
              </el-form-item>
              <div
                class="sub-title"
                v-if="idEnum == 2"
                style="padding-top: 19px"
              >
                <span class="no-filter"
                  style="color: blue; font-size: inherit; font-weight: 600"
                  >{{ $t("Thời gian làm việc") }}</span
                >
                <span style="color: red" class="no-filter">* </span>
              </div>
              <!-- <el-form-item :label="$t('Thời gian làm việc')" class="contact_lable_color" prop="FromTime" v-if="idEnum == 2">
                              
                            </el-form-item> -->

              <el-form-item
                :label="$t('FromDateString')"
                prop="FromTime"
                v-if="idEnum == 2"
                style="padding-top: 15px"
              >
                <el-date-picker
                  ref="FromTime"
                  v-model="formModel.FromTime"
                  type="date"
                  :clearable="true"
                  :editable="true"
                ></el-date-picker>
              </el-form-item>
              <el-form-item
                :label="$t('StartTime')"
                prop="StartTime"
                v-if="idEnum == 2"
                style="padding-top: 15px"
              >
                <el-time-picker
                  ref="StartTime"
                  v-model="formModel.StartTime"
                  format="HH:mm:ss"
                  :clearable="true"
                  ::editable="true"
                ></el-time-picker>
              </el-form-item>
              <el-form-item
                :label="$t('StartDate')"
                prop="StartDate"
                v-if="idEnum == 6"
                style="padding-top: 6px"
              >
                <el-date-picker
                  ref="FromTime"
                  v-model="formModel.FromTime"
                  type="date"
                ></el-date-picker>
              </el-form-item>
            </el-col>

            <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
              <el-form-item
                :label="$t('CardNumber')"
                prop="CardNumber"
                v-if="idEnum != 6 && idEnum != 2"
              >
                <el-input v-model="formModel.CardNumber"></el-input>
              </el-form-item>
              <!-- <el-form-item
                :label="$t('Position')"
                prop="PositionIndex"
                v-if="idEnum == 6"
              >
                <el-select v-model="formModel.PositionIndex">
                  <el-option
                    v-for="item in listAllPosition"
                    :key="item.Index"
                    :label="item.Name"
                    :value="item.Index"
                  >
                  </el-option>
                </el-select>
              </el-form-item> -->
              <el-form-item
                :label="$t('NameOnMachine')"
                style="padding-top: 99px;"
                v-if="idEnum == 6 || idEnum == 2"
              >
                <el-input v-model="formModel.NameOnMachine"></el-input>
              </el-form-item>
              <el-form-item :label="$t('NameOnMachine')" v-else>
                <el-input v-model="formModel.NameOnMachine"></el-input>
              </el-form-item>

              <el-form-item :label="$t('BirthDay')" prop="BirthDay" v-if="idEnum == 6 || idEnum == 2">
                <el-date-picker
                  ref="BirthDay"
                  v-model="formModel.BirthDay"
                  type="date"
                ></el-date-picker>
              </el-form-item>
              <el-form-item :label="$t('CMND/CCCD/Passport')" prop="NRIC" v-if="idEnum == 6 || idEnum == 2">
                <el-input ref="NRIC" v-model="formModel.NRIC"></el-input>
              </el-form-item>
              <el-form-item :label="$t('Email')" style="padding-top: 3px">
                <el-input ref="Email" v-model="formModel.Email"></el-input>
              </el-form-item>
              <el-form-item
              :label="$t('Password')"
              v-if="idEnum == 4"
            >
              <el-input
                v-model="formModel.Password"
                type="password"
              ></el-input>
            </el-form-item>
            <el-form-item
                prop="StudentOfParent"
                v-if="idEnum == 4"
                :label="$t('Student')"
                style="padding-top: 3px"
              >
                <!-- <el-input ref="StudentOfParent" v-model="formModel.StudentOfParent"></el-input> -->
                <el-select class="hr-customer-info__student-of-parent-select"
                  v-model="formModel.StudentOfParent"
                  filterable
                  multiple
                  collapse-tags
                  clearable
                  :placeholder="$t('Student')"
                  style="width: 100%; margin-right: 10px"
                >
                  <el-option
                    v-for="item in listEmployeeAndDepartment"
                    :key="item.EmployeeATID"
                    :label="item.EmployeeATID + (item.FullName != '' ? (' - ' + item.FullName) : '')"
                    :value="item.EmployeeATID"
                  ></el-option>
                </el-select>
              </el-form-item>
              <!--                       
                            <el-form-item :label="$t('CardNumber')" prop="CardNumber">
                                <el-input v-model="formModel.CardNumber"></el-input>
                            </el-form-item> -->

              <!-- <el-form-item
                :label="$t('Fingerprint')"
                v-if="idEnum == 2 && clientName !== 'Mondelez'"
              >
                <el-button
                  class="register-biometrics"
                  @click="showFingerDialog = true"
                >
                  <i class="el-icon-thumb"></i> {{ $t("Register") }}
                </el-button>
              </el-form-item> -->

              <!-- <el-form-item
                style="margin-top: 64px"
                v-if="idEnum == 2 && clientName !== 'Mondelez'"
              >
                <el-checkbox v-model="formModel.IsVip">
                  {{ $t("IsVip") }}
                </el-checkbox>
              </el-form-item> -->
              <el-form-item
                :label="$t('PhoneUseIsAllow')"
                v-if="idEnum == 2"
                style="padding-top: 5px"
              >
                <el-checkbox
                  class="custom-checkbox"
                  v-model="formModel.IsAllowPhone"
                ></el-checkbox>
              </el-form-item>
              <el-form-item
                :label="$t('Position')"
                prop="PositionIndex"
                v-if="idEnum == 6"
              >
                <el-select 
                clearable
                v-model="formModel.PositionIndex" 
                class="customer-info__position-select">
                  <el-option
                    v-for="item in listAllPosition"
                    :key="item.Index"
                    :label="item.Name"
                    :value="item.Index"
                  >
                  </el-option>
                </el-select>
              </el-form-item>
              <el-form-item :label="$t('PhoneUseIsAllow')" v-if="idEnum == 6">
                <el-checkbox style="height: 50px;"
                  class="custom-checkbox"
                  v-model="formModel.IsAllowPhone"
                ></el-checkbox>
              </el-form-item>
              <el-form-item
                :label="$t('ContactPerson')"
                prop="ContactPerson"
                class="contact_lable_color"
                style="padding-top: 14px"
                v-if="idEnum == 2"
              >
              <app-select-new :disabled="isEdit" :dataSource="employeeFullLookupForm" displayMember="NameInFilter" valueMember="Index"
                  :allowNull="true" v-model="formModel.ContactPerson" :multiple="false" style="width: 100%"
                  :placeholder="$t('ContactPerson')" ref="ContactPerson" @onChange="changeContactPerson">
                  </app-select-new>
                <!--<el-input ref="ContactPerson"
                                          v-model="formModel.ContactPerson"></el-input>-->
              </el-form-item>
              <el-form-item
                :label="$t('EndDate')"
                prop="EndDate"
                v-if="idEnum == 6"
                style="padding-top: 10px"
              >
                <el-date-picker
                  ref="ToTime"
                  v-model="formModel.ToTime"
                  type="date"
                ></el-date-picker>
              </el-form-item>
              <el-form-item
                :label="$t('ToDateString')"
                prop="ToTime"
                v-if="idEnum == 2"
                style="padding-top: 55px"
              >
                <el-date-picker
                  ref="ToTime"
                  v-model="formModel.ToTime"
                  type="date"
                  :clearable="true"
                  :editable="true"
                ></el-date-picker>
              </el-form-item>
              <el-form-item
                :label="$t('EndTime')"
                prop="EndTime"
                v-if="idEnum == 2"
                style="padding-top: 14px"
              >
                <el-time-picker
                  ref="StartTime"
                  v-model="formModel.EndTime"
                  format="HH:mm:ss"
                  :clearable="true"
                  :editable="true"
                ></el-time-picker>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
        <el-form>
          <div style="width: 98%; padding-left: 340px" v-if="idEnum == 2">
            <el-form-item
              prop="WorkingContent"
              :label="$t('WorkingContent')"
              class="contact_lable_color"
            >
              <el-input
                ref="WorkingContent"
                type="textarea"
                :autosize="{ minRows: 3, maxRows: 6 }"
                v-model="formModel.WorkingContent"
              ></el-input>
            </el-form-item>
            <el-form-item
              prop="Note"
              :label="$t('Note')"
              class="contact_lable_color"
            >
              <el-input
                ref="Note"
                type="textarea"
                :autosize="{ minRows: 3, maxRows: 6 }"
                v-model="formModel.Note"
              ></el-input>
            </el-form-item>
          </div>
          <div>
            <div class="accuracy__info" v-if="idEnum == 6">
              <el-form-item
                :label="$t('Password')"
                style="width: 28%"
              >
                <el-input
                  v-model="formModel.Password"
                  type="password"
                ></el-input>
              </el-form-item>
              <el-form-item
              :label="$t('CardNumber')"
              prop="CardNumber"
              style="width: 30.5%; margin-left: -14px"
            >
              <el-input v-model="formModel.CardNumber"></el-input>
            </el-form-item>
              <el-form-item
                :label="$t('Fingerprint')"
                style="margin-left: -10px"
              >
                <el-button
                  class="register-biometrics"
                  @click="showOrHideRegisterFingerDialog"
                >
                  <i class="el-icon-thumb"></i> {{ $t("Register") }}
                </el-button>
              </el-form-item>
            </div>
          </div>
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
                :id="'fileUpload' + idEnum"
                class="inputfile inputfile-3"
                @change="processFile($event)"
              />
              <label :for="'fileUpload' + idEnum">
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
          <el-form-item :label="$t('DownloadTemplate')">
            <a class="fileTemplate-lbl" :href="href" download>{{
              $t("Download")
            }}</a>
          </el-form-item>
          <el-form-item v-if="isDeleteFromExcel === true">
            <el-checkbox v-model="isDeleteOnDevice">
              {{ $t("DeleteEmployeeOnDeviceHint") }}
            </el-checkbox>
          </el-form-item>
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
              href="/Files/CustomerImportError.xlsx"
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
<script src="./hr-customer-info.ts"></script>
<style lang="scss">
.customer-t-grid {
  .t-grid {
    .el-table {
      margin-top: 35px;
      height: calc(100vh - 302px) !important;
      .el-table__body-wrapper {
        height: calc(100% - 35px) !important;
      }
    }
  }
}
#fileUpload1 {
  opacity: 0;
  position: absolute;
  z-index: -1;
}
#fileUpload2 {
  opacity: 0;
  position: absolute;
  z-index: -1;
}
#fileUpload3 {
  opacity: 0;
  position: absolute;
  z-index: -1;
}
#fileUpload4 {
  opacity: 0;
  position: absolute;
  z-index: -1;
}
#fileUpload5 {
  opacity: 0;
  position: absolute;
  z-index: -1;
}
#fileUpload6 {
  opacity: 0;
  position: absolute;
  z-index: -1;
}
#fileUpload7 {
  opacity: 0;
  position: absolute;
  z-index: -1;
}
.custom-checkbox {
  margin-left: 7px;
}
.custom-checkbox .el-checkbox__input {
  transform: scale(1.5); /* Adjust the scale as needed */
}

.custom-checkbox .el-checkbox__label {
  font-size: 20px; /* Adjust the font size as needed */
}

.contact_lable_color .el-form-item__label {
  color: blue;
}

.hr-customer-select,
.customer-info__position-select {
  .el-select__caret.el-input__icon.el-icon-arrow-up {
    height: 100% !important;
  }
  .el-select__caret.el-input__icon.el-icon-circle-close {
    height: 100% !important;
  }
}

.hr-customer-info__student-of-parent-select {
  .el-select__caret.el-input__icon.el-icon-arrow-up {
    height: 100% !important;
  }
  .el-select__caret.el-input__icon.el-icon-circle-close {
    height: 100% !important;
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