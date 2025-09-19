<template>
  <div>
    <el-row>
      <el-col :span="24" class="ac-synch-user-component">
        <div>
          <el-container>
            <el-header>
              <el-row>
                <el-col :span="12" class="left">
                  <span id="FormName">{{ $t("ACUserManagement") }}</span>
                </el-col>
                <el-col :span="12">
                  <HeaderComponent />
                </el-col>
              </el-row>
            </el-header>

            <div>
              <el-tabs
                type="card"
                v-model="activeTab"
                @tab-click="handleClick"
                style="margin-top: 10px"
              >
                <el-tab-pane :label="$t('ACUserManagement')" name="sync">
                  <el-form :inline="true" class="form">
                    <el-row style="text-align: left">
                      <el-col :span="24">
                        <el-form-item style="margin-bottom: 10px">
                          <el-select
                            v-model="selectUserType"
                            :multiple="true"
                            filterable
                            collapse-tags
                            :placeholder="$t('UserType')"
                            style="width: 300px"
                            @change="onChangeUserTypeSelect"
                          >
                            <el-option
                              :key="
                                selectUserType.length ==
                                selectUserTypeOption.length
                                  ? -2
                                  : -1
                              "
                              :label="
                                selectUserType.length ==
                                selectUserTypeOption.length
                                  ? $t('DeselectAll')
                                  : $t('SelectAll')
                              "
                              :value="
                                selectUserType.length ==
                                selectUserTypeOption.length
                                  ? -2
                                  : -1
                              "
                            ></el-option>
                            <el-option
                              v-for="item in selectUserTypeOption"
                              :key="item.value"
                              :label="$t(item.label)"
                              :value="item.value"
                            ></el-option>
                          </el-select>
                        </el-form-item>
                        <el-form-item
                          style="margin-bottom: 10px"
                          v-if="
                            selectUserType.includes(1) ||
                            selectUserType.includes(2) || selectUserType.includes(6)
                          "
                        >
                          <select-department-tree-component
                            :defaultExpandAll="tree.defaultExpandAll"
                            :multiple="tree.multiple"
                            :placeholder="$t('SelectDepartment')"
                            :data="tree.treeData"
                            :props="tree.treeProps"
                            :isSelectParent="true"
                            :checkStrictly="tree.checkStrictly"
                            :clearable="tree.clearable"
                            :popoverWidth="tree.popoverWidth"
                            v-model="selectDepartment"
                            :disabled="loadListTree"
                            style="width: 300px"
                          ></select-department-tree-component>
                        </el-form-item>
                        <el-form-item
                          style="margin-bottom: 10px"
                          v-if="selectUserType.includes(1)"
                        >
                          <working-info-select-vue
                            style="width: 200px"
                            @onWorkingInfoChange="handleWorkingInfoChange"
                          />
                        </el-form-item>
                        <!-- <el-form-item
                          style="margin-bottom: 10px"
                          v-if="selectUserType.includes(2)"
                        >
                          <el-select
                            v-model="selectDepartment"
                            multiple
                            filterable
                            collapse-tags
                            reserve-keyword
                            default-first-option
                            style="width: 300px"
                            :placeholder="$t('SelectContactDepartment')"
                          >
                            <el-option
                              :key="allDepartments"
                              :label="$t(allDepartments)"
                              :value="allDepartments"
                            ></el-option>
                            <el-option
                              v-for="item in listAllDepartment"
                              :key="item.value"
                              :label="item.label"
                              :value="item.value"
                            ></el-option>
                          </el-select>
                        </el-form-item> -->
                        <el-form-item
                          style="margin-bottom: 10px"
                          v-if="selectUserType.includes(4)"
                        >
                          <el-select
                            v-model="selectClass"
                            multiple
                            filterable
                            default-first-option
                            :placeholder="$t('SelectClass')"
                          >
                            <el-option
                              :key="allClass"
                              :label="$t(allClass)"
                              :value="allClass"
                            ></el-option>
                            <el-option
                              v-for="item in listAllClass"
                              :key="item.Index"
                              :label="item.Name"
                              :value="item.Index"
                            ></el-option>
                          </el-select>
                        </el-form-item>
                        <el-form-item
                          style="float: right; margin-right: 0 !important"
                        >
                          <data-table-function-component
                            :showButtonInsert="false"
                            :isHiddenEdit="true"
                            ref="syncFunction"
                            :isHiddenDelete="true"
                            :showButtonColumConfig="true"
                            :gridColumnConfig.sync="columns"
                            style="
                              height: fit-content;
                              display: flex;
                              position: relative;
                              top: 0;
                              width: 100%;
                              margin-right: 0 !important;
                            "
                          ></data-table-function-component>
                        </el-form-item>
                        <el-form-item>
                          <el-input
                            style="width: 300px; float: left"
                            :placeholder="$t('SearchData')"
                            v-model="filter"
                            @keyup.enter.native="Filter()"
                          >
                            <i
                              slot="suffix"
                              class="el-icon-search"
                              @click="Filter()"
                            ></i>
                          </el-input>
                          <el-button
                            type="primary"
                            @click="Tab1View"
                            class="smallbutton"
                            size="small"
                            style="margin-left: 10px"
                          >
                            <span class="add">{{ $t("View") }}</span>
                          </el-button>
                          <el-dropdown
                            style="margin-left: 10px"
                            @command="handleCommand"
                            trigger="click"
                          >
                            <span
                              class="el-dropdown-link"
                              style="font-weight: bold"
                            >
                              . . .<span class="more-text">{{
                                $t("More")
                              }}</span>
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
                        </el-form-item>
                      </el-col>
                    </el-row>
                  </el-form>
                  <el-row>
                    <el-col :span="24">
                      <el-table
                        class="ac-synch-user__table"
                        v-loading="tab1Loading"
                        ref="multipleTable"
                        @select-all="SelectAllTable"
                        :data="dataDBCurrent"
                        @selection-change="handleSelectionChange"
                        type="selection"
                        :max-height="maxHeight"
                      >
                        <el-table-column
                          type="selection"
                          width="30"
                          :fixed="true"
                        ></el-table-column>
                        <el-table-column
                          v-for="column in columns.filter((x) => x.display)"
                          :key="column.prop"
                          :fixed="column.fixed || false"
                          v-bind="column"
                          :label="$t(column.label)"
                        ></el-table-column>

                        <template slot="append">
                          <div class="unique-ids"></div>
                        </template>
                      </el-table>
                    </el-col>
                    <el-col class="page-container">
                      <slot name="pagination">
                        <div class="page-number">
                          <small>{{ $t("Display") }}</small>
                          <el-input
                            v-model="pageSizeTab1"
                            @change="onChangePageSizeTab1"
                            filterable
                            default-first-option
                            style="margin-left: 10px; width: 80px"
                          >
                          </el-input>
                        </div>
                        <el-pagination
                          class="custom-pagination"
                          :total="totalTab1"
                          :page-size="pageSizeTab1"
                          :current-page="pageTab1"
                          @current-change="Tab1ViewChangePage"
                          layout="prev, pager, next"
                        ></el-pagination>
                        <div class="total-record">
                          <small
                            >Tổng số: <b>{{ totalTab1 }}</b></small
                          >
                        </div>
                      </slot>
                    </el-col>
                  </el-row>
                  <el-row style="margin-top: 10px">
                    <el-col :span="6" class="left">
                      <el-button
                        class="classLeft"
                        id="btnFunction"
                        type="primary"
                        round
                        @click="showOrHideDialogAuthenMode(true)"
                        >{{ $t("InsertToDevice") }}</el-button
                      >
                    </el-col>
                    <el-col :span="6">
                      <el-badge
                        :value="DBLength"
                        type="success"
                        style="float: left; margin-right: 30px"
                      >
                        <span style="font-size: 14px">
                          {{ $t("NumOfSelectedItem") }}
                        </span>
                      </el-badge>
                    </el-col>
                    <el-col :span="12">
                      <DeleteUserOnMachineButton
                        :listSerialNumber="selectMachine"
                        :listArea="listAllArea"
                        :listDoor="listAllDoor"
                        :listEmployeeATID="selectedEmployeeATIDs"
                      />
                    </el-col>
                  </el-row>
                </el-tab-pane>
                <el-tab-pane
                  :label="$t('ACUserMasterLog')"
                  name="syncHistory"
                  style="height: 100%"
                >
                  <el-select
                    filterable
                    :placeholder="$t('SelectViewMode')"
                    collapse-tags
                    v-model="selectedViewMode"
                    style="padding: 0 10px 0 10px"
                  >
                    <el-option
                      v-for="item in syncHistoryViewMode"
                      :key="item.Index"
                      :label="$t(item.Name)"
                      :value="item.Index"
                    ></el-option>
                  </el-select>
                  <el-select
                    v-model="selectedArea"
                    collapse-tags
                    @change="onAreaChange"
                    filterable
                    :placeholder="$t('SelectArea')"
                    multiple
                    clearable
                  >
                    <el-option
                      v-for="item in allAreaLst"
                      :key="item.value"
                      :label="$t(item.label)"
                      :value="item.value"
                    ></el-option>
                  </el-select>

                  <el-select
                    filterable
                    :placeholder="$t('SelectDoor')"
                    multiple
                    collapse-tags
                    v-model="selectedDoor"
                    style="padding: 0 10px 0 10px"
                    clearable
                  >
                    <el-option
                      v-for="item in allDoorLst"
                      :key="item.value"
                      :label="$t(item.label)"
                      :value="item.value"
                    ></el-option>
                  </el-select>

                  <select-department-tree-component
                    :defaultExpandAll="treeSyncHistory.defaultExpandAll"
                    :multiple="treeSyncHistory.multiple"
                    :placeholder="$t('SelectDepartment')"
                    :data="treeSyncHistory.treeData"
                    :props="treeSyncHistory.treeProps"
                    :isSelectParent="true"
                    :checkStrictly="treeSyncHistory.checkStrictly"
                    :clearable="treeSyncHistory.clearable"
                    :popoverWidth="treeSyncHistory.popoverWidth"
                    v-model="departmentIds"
                    style="
                      padding: 0 0 0 0;
                      display: inline-block;
                      position: relative;
                    "
                  ></select-department-tree-component>

                  <el-select
                    filterable
                    :placeholder="$t('SelectOperation')"
                    multiple
                    collapse-tags
                    v-model="selectedOperation"
                    style="padding: 0 10px 0 10px"
                    clearable
                  >
                    <el-option
                      v-for="item in syncHistoryOperation"
                      :key="item.Index"
                      :label="$t(item.Name)"
                      :value="item.Index"
                    ></el-option>
                  </el-select>

                  <!-- <el-date-picker v-model="fromTime" style="width:200px;" type="datetime" 
                      :placeholder="$t('SelectDateTime')"></el-date-picker>
                
                    <el-date-picker v-model="toTime" style="width:200px; padding: 0 5px 0 5px" type="datetime"
                      :placeholder="$t('SelectDateTime')"></el-date-picker> -->
                  <el-input
                    style="
                      padding-bottom: 3px;
                      width: 200px;
                      margin-right: 10px;
                    "
                    :placeholder="$t('SearchData')"
                    v-model="filter"
                    @keyup.enter.native="Search()"
                    class="filter-input"
                  >
                    <i slot="suffix" class="el-icon-search"></i>
                  </el-input>

                  <el-button
                    id="btnFunction"
                    round
                    @click="Search"
                    style="background-color: #122658; color: white"
                  >
                    {{ $t("View") }}
                  </el-button>

                  <div>
                    <data-table-function-component
                      :showButtonInsert="false"
                      :isHiddenEdit="true"
                      :isHiddenDelete="true"
                      :showButtonColumConfig="true"
                      :gridColumnConfig.sync="columnsSyncHistory"
                      ref="syncHistoryFunction"
                      style="
                        height: fit-content;
                        display: flex;
                        top: 0;
                        width: fit-content;
                        margin-right: 0 !important;
                      "
                    ></data-table-function-component>
                    <data-table-component
                      class="ac-usermaster__data-table"
                      ref="table"
                      :showSearch="false"
                      :get-data="getSyncHistoryData"
                      :columns="columnsSyncHistory"
                      :isShowPageSize="true"
                      :showCheckbox="false"
                    ></data-table-component>
                  </div>
                </el-tab-pane>
              </el-tabs>
            </div>
          </el-container>
        </div>
      </el-col>
    </el-row>

    <div>
      <el-dialog
        :title="isAddFromExcel ? $t('AddFromExcel') : $t('DeleteFromExcel')"
        custom-class="customdialog"
        :visible="showDialogExcel && !isChoose"
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
                <!-- <span>Choose a file&hellip;</span> -->
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
              href="/Template_EmployeeSync.xlsx"
              download
              >{{ $t("Download") }}</a
            >
          </el-form-item>
        </el-form>

        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="AddOrDeleteFromExcel('close')">
            {{ $t("Cancel") }}
          </el-button>
          <el-button class="btnOK" type="primary" @click="UploadDataFromExcel">
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
              href="/Files/DepartmentImportError.xlsx"
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

    <div>
      <!--Dialog notification-->
      <el-dialog
        :title="$t('Notify')"
        :visible.sync="showMessage"
        width="20%"
        height="20%"
        center
        :close-on-click-modal="false"
      >
        <span>{{ $t("ProcessingRequest") }}</span>
      </el-dialog>
    </div>
    <div>
      <!--Dialog chosse excel-->
      <el-dialog
        :title="$t('AutoSelectExcel')"
        custom-class="customdialog"
        :visible="showDialogExcel && isChoose"
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
                <span>{{ $t("ChooseAExcelFile") }}</span>
              </label>
              <span v-if="fileName === ''" class="fileName">
                {{ $t("NoFileChoosen") }}
              </span>
              <span v-else class="fileName">{{ fileName }}</span>
            </div>
          </el-form-item>
          <el-form-item :label="$t('DownloadTemplate')">
            <a
              class="fileTemplate-lbl"
              href="/Template_ChooseEmployeeSync.xlsx"
              download
              >{{ $t("Download") }}</a
            >
          </el-form-item>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="ShowOrHideDialogExcel('close')">
            {{ $t("Cancel") }}
          </el-button>
          <el-button class="btnOK" type="primary" @click="AutoSelectFromExcel">
            {{ $t("OK") }}
          </el-button>
        </span>
      </el-dialog>
    </div>
    <div>
      <el-dialog
        :title="$t('SynchUserOnDevice')"
        custom-class="customdialog"
        :visible.sync="showDialogDownloadUser"
        :before-close="cancelDialog"
        :close-on-click-modal="false"
      >
        <el-form label-width="168px" label-position="top">
          <div v-if="isOverwriteUserMaster" style="margin-bottom: 20px">
            <i
              style="font-weight: bold; font-size: larger; color: orange"
              class="el-icon-warning-outline"
            />
            <span style="font-weight: bold">
              {{ $t("SynchUserMasterHint") }}
            </span>
          </div>
          <el-form-item>
            <el-radio-group v-model="isOverwriteUserMaster">
              <el-radio :label="false">
                {{ $t("SyncNotOverwriteUserMaster") }}
              </el-radio>
              <el-radio :label="true">
                {{ $t("SyncOverwriteUserMaster") }}
              </el-radio>
            </el-radio-group>
          </el-form-item>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button
            class="btnCancel"
            @click="showOrHideDialogDownloadUser(false)"
          >
            {{ $t("Cancel") }}
          </el-button>
          <el-button type="primary" @click="DownloadUserMaster">
            {{ $t("DownloadUser") }}
          </el-button>
        </span>
      </el-dialog>
    </div>
    <div>
      <el-dialog
        :title="$t('Sync')"
        custom-class="customdialog"
        :visible.sync="showDialogAuthenMode"
        :before-close="cancelDialog"
        :close-on-click-modal="false"
      >
        <el-form label-width="168px" label-position="top">
          <el-form-item :label="$t('Timezone')">
            <el-select
              props="selectTimezone"
              v-model="selectedTimeZone"
              clearable
              :placeholder="$t('SelectTimezone')"
            >
              <el-option
                v-for="item in listAllTimeZone"
                :key="item.value"
                :label="$t(item.label)"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item :label="$t('IntegrateBy')">
            <el-radio-group v-model="isUsingArea">
              <el-radio :label="true">
                {{ $t("AccessArea") }}
              </el-radio>
              <el-radio :label="false">
                {{ $t("AccessDoor") }}
              </el-radio>
            </el-radio-group>
          </el-form-item>
          <el-form-item v-if="isUsingArea">
            <el-select
              props="selectArea"
              v-model="selectArea"
              clearable
              multiple
              :placeholder="$t('SelectArea')"
            >
              <el-option
                v-for="item in listAllArea"
                :key="item.value"
                :label="$t(item.label)"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item v-else>
            <el-select
              props="selectDoor"
              v-model="selectDoor"
              clearable
              multiple
              :placeholder="$t('SelectDoor')"
            >
              <el-option
                v-for="item in listAllDoor"
                :key="item.value"
                :label="$t(item.label)"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item>
            <el-checkbox v-model="syncUser">{{
              $t("UploadUsersToMachine")
            }}</el-checkbox>
          </el-form-item>
          <el-form-item v-if="syncUser">
            <el-select
              props="selectAuthenMode"
              v-model="selectAuthenMode"
              clearable
              multiple
              :placeholder="$t('SelectAuthenMode')"
            >
              <el-option
                :key="allAuthenModes"
                :label="$t(allAuthenModes)"
                :value="allAuthenModes"
              ></el-option>
              <el-option
                v-for="item in listAuthenMode"
                :key="item.value"
                :label="$t(item.label)"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button
            class="btnCancel"
            @click="showOrHideDialogAuthenMode(false)"
          >
            {{ $t("Cancel") }}
          </el-button>
          <el-button type="primary" @click="InsertToMachine">
            {{ $t("InsertToDevice") }}
          </el-button>
        </span>
      </el-dialog>
    </div>

    <div>
      <el-dialog
        :visible.sync="showAuthorizeModal"
        :before-close="cancelDialogAuthorize"
        :close-on-click-modal="false"
      >
        <el-row>
          <el-form label-position="top" style="text-align: left">
            <el-col :span="8">
              <el-form-item>
                <el-select
                  props="selectedTimezone"
                  v-model="selectedTimeZone"
                  filterable
                  clearable
                  :placeholder="$t('SelectTimezone')"
                >
                  <el-option
                    v-for="item in listAllTimeZone"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item>
                <el-select
                  props="selectUserPrivilege"
                  v-model="selectUserPrivilege"
                  clearable
                  filterable
                  :placeholder="$t('SelectPrivilege')"
                >
                  <el-option
                    v-for="item in listPrivileges"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="30" style="float: right">
              <el-form-item style="margin-top: 0px">
                <el-button
                  type="primary"
                  style="margin-right: 0px; float: right"
                  class="smallbutton"
                  size="small"
                  @click="UpdateUserPrivilege"
                  >{{ $t("UpdateUserPrivilege") }}</el-button
                >
              </el-form-item>
            </el-col>
          </el-form>
        </el-row>
        <el-row>
          <el-col :span="24" class="transferCompnent">
            <el-transfer
              filterable
              v-model="selectedDevice"
              :style="'height:300px'"
              :titles="[$t('ListDevicesUnSelected'), $t('ListDevicesSelected')]"
              :data="dataListDevice"
            >
            </el-transfer>
          </el-col>
        </el-row>
        <el-row>
          <el-col :span="24">
            <el-table
              ref="multipleTable"
              @select-all="SelectAllTable"
              :data="dataDBCurrent"
              style="width: 100%"
              @selection-change="handleSelectionChange"
              type="selection"
              :max-height="maxHeight"
            >
              <el-table-column
                type="selection"
                width="30"
                :fixed="true"
              ></el-table-column>
              <el-table-column
                v-for="column in columnsPermissions.filter((x) => x.display)"
                :key="column.prop"
                :fixed="column.fixed || false"
                v-bind="column"
                :label="$t(column.label)"
              ></el-table-column>

              <template slot="append">
                <div
                  class="unique-ids"
                  v-infinite-scroll="onInfinite"
                  :infinite-scroll-disabled="loadingLazy"
                  :infinite-scroll-distance="30"
                ></div>
              </template>
            </el-table>
            <div style="margin-top: 16px; margin-left: 64px">
              <el-badge
                :value="selectedEmployeeATIDs.length"
                type="success"
                style="float: left; margin-right: 30px"
              >
                <span style="font-size: 14px">
                  {{ $t("NumOfSelectedItem") }}
                </span>
              </el-badge>
            </div>
          </el-col>
        </el-row>
      </el-dialog>
    </div>
  </div>
</template>

<script lang="ts" src="./ac-synch-user-component.ts"></script>

<style lang="scss" scoped>
.ac-synch-user-component {
  .page-container {
    margin-top: 5px;
    display: flex;
    justify-content: center;

    small {
      line-height: 28px;
    }
  }
}

.auto-synch-user__el-tabs {
  height: 100%;
  .el-tabs__content {
    padding: 15px 15px 0 15px;
  }
  .page-container {
    margin-top: 5px;
    display: flex;
    justify-content: center;
    small {
      line-height: 28px;
    }
  }
}

.el-table {
  margin-top: 0;
}

.el-table__body-wrapper {
  overflow: auto;
}

.transferCompnent .el-transfer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.transferCompnent .el-transfer .el-transfer-panel {
  flex-grow: 1;
  width: 100%;
  height: 100%;
}

.transferCompnent .el-transfer .el-transfer__buttons {
  text-align: center;
  padding: 0 5px;
}

.transferCompnent .el-transfer-panel__body {
  height: calc(100% - 40px);
}

.transferCompnent .el-checkbox-group.el-transfer-panel__list {
  height: 100%;
}

.transferCompnent .el-button {
  margin-left: 0;
}

.more-text {
  color: #606266;
  margin-left: 10px;
}

.sync-user__department_dropdown .el-select__tags span span:first-child {
  max-width: 80%;
  overflow: hidden;
}

.ac-synch-user__table {
  width: 100%;
}

.ac-usermaster__data-table {
  /deep/ .el-table {
    height: calc(100vh - 234px) !important;
  }
}
</style>
