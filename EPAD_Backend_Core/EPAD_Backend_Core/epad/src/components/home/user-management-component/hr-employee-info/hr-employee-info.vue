<template>
  <div class="tab" style="height: calc(100vh - 185px);">
    <alert-component :show.sync="showWaitScanDialog" :title="titleScanDialog" :message="messageScanDialog" 
      @Cancel="cancelScanDialog" :closeSec="closeSecScanDialog" :type="typeScanDialog" 
      :enableHiddenScanQR="true" @ScanQR="changeScanQR">
    </alert-component>
    <div class="tab-filter">
      <el-row>
          <span style="line-height: 28px; float: left; margin-right: 10px;">Lọc danh sách</span>
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
            v-model="filterModel.SelectedDepartment"
            style="padding: 0 10px; width: 100%"
          ></select-department-tree-component>
          <!-- <select-department v-else v-model="filterModel.SelectedDepartment"
                        clearable
                        multiple
                        collapse-tags
                        style="padding: 0 5px; width: 100%"></select-department> -->
                </el-col>
                <el-col :span="5">
                    <working-info-select-vue style="width: 100%; padding-right: 10px;" @onWorkingInfoChange="handleWorkingInfoChange" />
                </el-col>
                <el-col :span="3">
                    <el-date-picker style="padding-right: 10px;" v-model="filterModel.FromDate" format="dd/MM/yyyy" type="date" :placeholder="$t('WorkingFromDate')">
                    </el-date-picker>
                </el-col>
                <el-col :span="3">
                    <el-date-picker style="padding-right: 10px;" v-model="filterModel.ToDate" format="dd/MM/yyyy" type="date" :placeholder="$t('WorkingToDate')">
                    </el-date-picker>
                </el-col>
                <el-col :span="3">
                    <el-input style="padding-bottom: 3px; width: calc(100% - 10px)"
                          :placeholder="$t('SearchData')"
                          suffix-icon="el-icon-search"
                          v-model="filterModel.TextboxSearch"
                          @keyup.enter.native="onViewClick"
                          class="filter-input"></el-input>
                </el-col>
                <el-col :span="3">
                    <el-button type="primary"
                           class="smallbutton"
                           size="small"
                           @click="onViewClick">
                    {{ $t("View") }}
                    </el-button>
                    <el-dropdown style="margin-left: 10px; margin-top: 5px"
                        v-if="showMore"
                                @command="handleCommand"
                                trigger="click">
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
          <TableToolbar ref="customerDataTableFunction"
            :gridColumnConfig.sync="columnDefs" />
        </el-col>
      </el-row>
    </div>

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
        :columnDefs="columnDefs.filter(x => x.display)"
        :rowData="dataSource"
        :rowHeight="40"
        @onSelectionChange="onSelectionChange"
        :heightScale="260"
        :isKeepIndexAscending="true"
        :shouldResetColumnSortState="shouldResetColumnSortState"
        style="height: calc(100% - 45px);"
      />
      <AppPagination
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
        <div v-if="!isEdit" style="float: right; z-index: 1; position: relative; width: 100%;">
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
            </el-col>

            <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
              <el-form-item
                :label="clientName == 'MAY' ? $t('MCA') : $t('EmployeeATID')"
                prop="EmployeeATID"
              >
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
              <el-form-item :label="$t('Gender')">
                <el-radio-group v-model="formModel.Gender">
                  <el-radio :label="1">{{ $t("Male") }}</el-radio>
                  <el-radio :label="0">{{ $t("Female") }}</el-radio>
                  <el-radio :label="2">{{ $t("Other") }}</el-radio>
                </el-radio-group>
              </el-form-item>
              
              <el-form-item :label="$t('MobilePhone')" style="padding-top: 20px">
                <el-input ref="Phone" v-model="formModel.Phone"></el-input>
              </el-form-item>
              
              <el-form-item :label="$t('Address')" prop="Address">
                <el-input ref="Address" v-model="formModel.Address"></el-input>
              </el-form-item>

              <el-form-item
              :label="$t('Department')"
              prop="DepartmentIndex"
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

                          <el-form-item :label="$t('EmployeeType')" prop="EmployeeTypeIndex" >
                            <el-select v-model="formModel.EmployeeTypeIndex" class="hr-employee-info__employee-type-select">
                                <el-option v-for="item in listEmployeeType"
                                           :key="item.Index"
                                           :label="item.Name"
                                           :value="item.Index">
                                </el-option>
                            </el-select>
                        </el-form-item>
                        <el-form-item :label="$t('JoinedDate')" prop="FromDate">
                          <el-date-picker
                            :disabled="isEdit"
                            ref="FromDate"
                            v-model="formModel.FromDate"
                            type="date"
                          ></el-date-picker>
                        </el-form-item>

            </el-col>

            <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
              <el-form-item  :label="clientName == 'MAY' ? $t('MSKH') : $t('EmployeeCode')">
                <el-input v-model="formModel.EmployeeCode"></el-input>
              </el-form-item>
              <el-form-item :label="$t('NameOnMachine')">
                <el-input v-model="formModel.NameOnMachine"></el-input>
              </el-form-item>

              <el-form-item :label="$t('BirthDay')" prop="BirthDay">
                <el-date-picker ref="BirthDay"
                                v-model="formModel.BirthDay"
                                type="date"></el-date-picker>
            </el-form-item>

            <el-form-item :label="$t('CMND/CCCD/Passport')" prop="Nric">
              <el-input ref="Nric" v-model="formModel.Nric"></el-input>
            </el-form-item>
            
            <el-form-item :label="$t('Email')">
              <el-input ref="Email" v-model="formModel.Email"></el-input>
            </el-form-item>
            <el-form-item
            :label="
              clientName == 'MAY' ? $t('CustomerObject') : $t('Position')
            "
            prop="PositionIndex"
            :placeholder="$t('SelectPosition')"
            v-if="clientName == 'MAY'"
            
          >
            <el-select v-model="formModel.PositionIndex" :disabled="isEdit">
              <el-option
                v-for="item in listAllPosition"
                :key="item.UserTypeId"
                :label="item.Name"
                :value="item.UserTypeId"
                :disabled="isEdit"
              >
              </el-option>
            </el-select>
          </el-form-item>

          <el-form-item
            :label="
              clientName == 'MAY' ? $t('CustomerObject') : $t('Position')
            "
            v-else
            prop="PositionIndex"
            :placeholder="$t('SelectPosition')"
            
          >
            <el-select v-model="formModel.PositionIndex" :disabled="isEdit" class="hr-employee-info__employee-type-select">
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
            <el-form-item :label="$t('PhoneUseIsAllow')" style="padding-top: 5px">
              <el-checkbox v-model="formModel.IsAllowPhone"></el-checkbox>
            </el-form-item>

          <el-form-item :label="$t('LeaveDate')" prop="ToDate" style="padding-top: 15px">
            <el-date-picker
              ref="FromDate"
              v-model="formModel.ToDate"
              type="date"
            ></el-date-picker>
          </el-form-item>
                            <!-- <el-form-item :label="$t('MobilePhone')">
                                <el-input ref="Phone" v-model="formModel.Phone"></el-input>
                            </el-form-item>
                            <el-form-item :label="$t('Address')" prop="Address">
                                <el-input ref="Address" v-model="formModel.Address"></el-input>
                            </el-form-item> -->
                           
                        </el-col>
                    </el-row>
                </el-form>
<!-- 
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
           -->
           <el-form>
            <div>
              <div class="accuracy__info">
                <el-form-item :label="$t('CardNumber')" prop="CardNumber" style="width: 28%;">
                  <el-input v-model="formModel.CardNumber"></el-input>
                </el-form-item>

                
               <el-form-item :label="$t('Password')" style="width: 30.5%;margin-left:-14px">
              <el-input
                v-model="formModel.Password"
                type="password"
              ></el-input>
            </el-form-item>

            <el-form-item :label="$t('Fingerprint')" style="margin-left:-10px">
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

           <div class="contact">
            <el-form class="h-600">
              <div
                class="contact__list"
                v-for="item in listContactInfoFormApi"
                :key="item.id"
              >
                <div class="contact__item">
                  <el-form-item :label="$t('FullName')">
                    <el-input v-model="item.Name"></el-input>
                  </el-form-item>
  
                  <el-form-item :label="$t('Email')">
                    <el-input name="email" v-model="item.Email"> </el-input>
                  </el-form-item>
  
                  <el-form-item :label="$t('MobilePhone')">
                    <el-input v-model="item.Phone"></el-input>
                  </el-form-item>
  
                  <div class="contact__button">
                    <el-button
                      type="danger"
                      icon="el-icon-delete"
                      circle
                      @click="deleteContactInfo(item.Name)"
                    >
                    </el-button>
                  </div>
                </div>
              </div>
            </el-form>
          </div>
  
        <div class="contact">
          <div class="contact__title">{{ $t("ContactInformation") }}</div>

          <el-form class="h-600 contact__formInput">
            <div class="contact__list">
              <div class="contact__item">
                <el-form-item :label="$t('FullName')">
                  <el-input v-model="newInputContactInfo.Name"></el-input>
                </el-form-item>

                <el-form-item :label="$t('Email')">
                  <el-input v-model="newInputContactInfo.Email"></el-input>
                </el-form-item>

                <el-form-item :label="$t('MobilePhone')">
                  <el-input v-model="newInputContactInfo.Phone"></el-input>
                </el-form-item>

                <el-button
                  class="contact__btn-add btnOK"
                  type="primary"
                  @click="addContactInfo()"
                >
                  {{ $t("Add") }}
                </el-button>
              </div>
            </div>
          </el-form>

          <el-form class="h-600">
            <div
              class="contact__list"
              v-for="item in listContactInfoFormApi"
              :key="item.id"
            >
              <div class="contact__item">
                <el-form-item :label="$t('FullName')">
                  <el-input v-model="item.Name"></el-input>
                </el-form-item>

                <el-form-item :label="$t('Email')">
                  <el-input name="email" v-model="item.Email"> </el-input>
                </el-form-item>

                <el-form-item :label="$t('MobilePhone')">
                  <el-input v-model="item.Phone"></el-input>
                </el-form-item>

                <div class="contact__button">
                  <el-button
                    type="danger"
                    icon="el-icon-delete"
                    circle
                    @click="deleteContactInfo(item.Name)"
                  >
                  </el-button>
                </div>
              </div>
            </div>
          </el-form>
        </div>

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
          <div v-if="clientName == 'Mondelez'">
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
                href="/Template_IC_Employee_Mdl.xlsx"
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
<script src="./hr-employee-info.ts"></script>
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

.accuracy__info {
  display: flex;
  column-gap: 55px;
  row-gap: 20px;
  justify-content: flex-start;
  align-items: flex-start;
  margin-bottom: 20px;
  margin-left: 40px;
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
.hr-employee-info__employee-type-select {
  .el-select__caret.el-input__icon.el-icon-arrow-up {
    height: 100% !important;
  }
  .el-select__caret.el-input__icon.el-icon-circle-close {
    height: 100% !important;
  }
}
</style>