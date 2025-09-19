<template>
  <div id="bgHome">
    <el-row>
      <el-col :span="24" class="DepartmentTreeComponent">
        <div>
          <el-container>
            <el-header>
              <el-row>
                <el-col :span="12" class="left">
                  <span id="FormName">{{ $t("UserMasterInfo") }}</span>
                </el-col>
                <el-col :span="12">
                  <HeaderComponent />
                </el-col>
              </el-row>
            </el-header>

            <el-tabs class="auto-synch-user__el-tabs"
              type="border-card"
              style="margin-top: 10px;"
              @tab-click="handleTabClick"
              v-model="activeTab"
            >
              <el-tab-pane name="sync">
                <span slot="label">
                  <i class="el-icon-s-data"></i>
                  {{ $t("UserMasterInfo") }}
                </span>
                <el-form :inline="true" class="form">
                  <el-row style="text-align: left">
                    <el-col :span="16">
                      <el-form-item style="margin-bottom: 10px;">
                        <el-select
                          v-model="selectUserType"
                          :multiple="multiple"
                          filterable
                          collapse-tags
                          :placeholder="$t('UserType')"
                          style="width: 300px"
                          @change="onChangeUserTypeSelect"
                        >
                          <el-option
                            :key="selectUserType.length == selectUserTypeOption.length ? -2 : -1"
                            :label="selectUserType.length == selectUserTypeOption.length ? $t('DeselectAll') : $t('SelectAll')"
                            :value="selectUserType.length == selectUserTypeOption.length ? -2 : -1"
                          ></el-option>
                          <el-option
                            v-for="item in selectUserTypeOption"
                            :key="item.value"
                            :label="$t(item.label)"
                            :value="item.value"
                          ></el-option>
                        </el-select>
                      </el-form-item>
                      <el-form-item style="margin-bottom: 10px;" 
                        v-if="selectUserType.includes(1) || selectUserType.includes(2) || selectUserType.includes(6)">
                       
                        <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll"
                                    :multiple="tree.multiple"
                                    :placeholder="$t('SelectDepartment')"
                                    :data="tree.treeData"
                                    :props="tree.treeProps"
                                    :isSelectParent="true"
                                    :checkStrictly="tree.checkStrictly"
                                    :clearable="tree.clearable"
                                    :popoverWidth="tree.popoverWidth"
                                    v-model="selectDepartment"
                                    style="width: 300px"
                                    :disabled="loadListTree"
                                    ></select-department-tree-component>
                      </el-form-item>
                      <el-form-item style="margin-bottom: 10px;" v-if="selectUserType.includes(1) || selectUserType.includes(2) || selectUserType.includes(6)">
                        <working-info-select-vue style="width: 300px;" @onWorkingInfoChange="handleWorkingInfoChange" />
                      </el-form-item>
                      <!-- <el-form-item style="margin-bottom: 10px;" v-if="selectUserType == 2">
                        <el-select
                          v-model="selectDepartment"
                          multiple
                          filterable
                          collapse-tags
                          reserve-keyword
                          default-first-option
                          style="width: 350px"
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
                      <el-form-item style="margin-bottom: 10px;" v-if="selectUserType.includes(4)">
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
                      <el-form-item style="margin-bottom: 10px;">
                        <el-input
                          style="width: 300px; float: left"
                          :placeholder="$t('SearchData')"
                          v-model="filter"
                          @keyup.enter.native="Filter()">
                          <i slot="suffix" class="el-icon-search" @click="Filter()"></i>
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
                      </el-form-item>
                    </el-col>
                   
                    <el-col :span="8" style="text-align: right;">
                      <el-row>
                        <el-col :span="11">
                          <el-form-item class="user-master-info__right-function-bar-item" style="margin-bottom: 10px; margin-right: 10px;">
                            <el-checkbox style="width: 100%; text-align: right !important;" 
                            v-model="isOnline" @change="onChangeOnline">Chỉ hiển thị máy online</el-checkbox>
                          </el-form-item>
                        </el-col>
                        <el-col :span="13">
                          <el-form-item class="user-master-info__right-function-bar-item" style="margin-bottom: 0; margin-right: 0;">
                            <el-select
                            reserve-keyword 
                            v-model="selectMachine"
                            multiple
                            filterable
                            collapse-tags
                            clearable
                            default-first-option
                            :placeholder="$t('SelectMachine')"
                            style="width: 100%;"
                            >                        
                              <el-option
                                :key="allMachines"
                                :label="$t(allMachines)"
                                :value="allMachines"
                              ></el-option>
                              <el-option
                                v-for="item in selectMachineOption"
                                :key="item.value"
                                :label="item.label"
                                :value="item.value"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                      <el-row>
                        <el-col :span="11">
                          <p></p>
                        </el-col>
                        <el-col :span="13">
                          <el-form-item class="user-master-info__right-function-bar-item" style="margin-right: 0; margin-bottom: 0;">
                            <DownloadUserMasterButton class="user-master-info__right-function-bar-item__download-user-master__btn"
                              style="width: 100%;" 
                              :listEmployeeATID="selectedEmployeeATIDs"
                              :listSelectedMachineSerialNumber="selectMachine" />
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                </el-form>
                <el-row>
                  <el-col :span="24">
                    <!-- {{ columns }} -->
                    <data-table-function-component
                      :showButtonInsert="false"
                      :isHiddenEdit="true" :isHiddenDelete="true"
                      :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
                      style="height: fit-content; display: flex; position: relative; top: 0; width: 100%;"
                      ref="syncFunction"
                    ></data-table-function-component>
                    <!--<data-table-component :get-data="getData" ref="table" :columns="columns" :selectedRows.sync="rowsObj"></data-table-component>-->
                    <el-table class="sync-user__table"
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
                        v-for="column in columns.filter(x => x.display)"
                        :key="column.prop"
                        :fixed="column.fixed || false"
                        v-bind="column"
                        :label="$t(column.label)"
                      ></el-table-column>

                      <template slot="append">
                        <!-- <div
                          class="unique-ids"
                          v-infinite-scroll="onInfinite"
                          :infinite-scroll-disabled="loadingLazy"
                          :infinite-scroll-distance="30"
                        ></div> -->
                        <div
                          class="unique-ids"
                        ></div>
                      </template>
                    </el-table>
                  </el-col>
                  <el-col class="page-container">
                       <slot name="pagination">
                          <div class="page-number">
                              <small>{{$t("Display")}}</small>
                              <el-input v-model="pageSizeTab1"
                                        @change="onChangePageSizeTab1"
                                        filterable
                                        default-first-option
                                        style=" margin-left:10px;width:80px ">
                                  <!-- <el-option :key="3" :label="50" :value="50"></el-option>
                                  <el-option :key="4" :label="100" :value="100"></el-option>
                                  <el-option :key="5" :label="150" :value="150"></el-option>
                                  <el-option :key="6" :label="200" :value="200"></el-option> -->
                              </el-input>
                          </div>
                          <el-pagination class="custom-pagination" :total="totalTab1"
                                        :page-size="parseInt(pageSizeTab1)"
                                        :current-page="pageTab1"
                                        @current-change="Tab1ViewChangePage"
                                        layout="prev, pager, next"></el-pagination>
                          <div class="total-record">
                              <small>Tổng số: <b>{{totalTab1}}</b></small>
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
                      >{{ $t("InsertToMachine") }}</el-button
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
                        :listEmployeeATID="selectedEmployeeATIDs" />       
                  </el-col>
                </el-row>
              </el-tab-pane>

              <el-tab-pane v-if="usingBasicMenu === false" name="privilege">
                <span slot="label"
                  ><i class="el-icon-s-custom"></i> {{ $t("Privilege") }}</span
                >
                <el-row>
                  <el-form label-position="top">
                    <el-col :span="5">
                      <el-form-item>
                        <el-select
                          v-model="selectUserType"
                          :multiple="multiple"
                          filterable
                          collapse-tags
                          :placeholder="$t('UserType')"
                          @change="onChangeUserTypeSelect"
                          style="width: 100%; padding-right: 10px;"
                        >
                        <el-option
                            :key="selectUserType.length == selectUserTypeOption.length ? -2 : -1"
                            :label="selectUserType.length == selectUserTypeOption.length ? $t('DeselectAll') : $t('SelectAll')"
                            :value="selectUserType.length == selectUserTypeOption.length ? -2 : -1"
                          ></el-option>
                          <el-option
                            v-for="item in selectUserTypeOption"
                            :key="item.value"
                            :label="$t(item.label)"
                            :value="item.value"
                          ></el-option>
                        </el-select>
                      </el-form-item>
                    </el-col>
                    <el-col :span="5">
                      <el-form-item v-if="selectUserType.includes(1) || selectUserType.includes(2) ||selectUserType.includes(6)">
                        <!-- <el-select
                          v-model="selectDepartment"
                          multiple
                          filterable
                          reserve-keyword
                          collapse-tags
                          default-first-option
                          style="width: 200px"
                          :placeholder="$t('SelectDeparment')"
                        >
                          <el-option
                            :key="allDepartments"
                            :label="$t(allDepartments)"
                            :value="allDepartments"
                          ></el-option>
                          <el-option
                            v-for="item in listAllDepartment"
                            :key="item.value"
                            :label="item.parent ? item.parent + ' - ' + item.label : item.label"
                            :value="item.value"
                          ></el-option>
                        </el-select> -->
                        <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll"
                                    :multiple="tree.multiple"
                                    :placeholder="$t('SelectDepartment')"
                                    :data="tree.treeData"
                                    :props="tree.treeProps"
                                    :isSelectParent="true"
                                    :checkStrictly="tree.checkStrictly"
                                    :clearable="tree.clearable"
                                    :popoverWidth="tree.popoverWidth"
                                    v-model="selectDepartment"
                                    style="padding-right: 10px; width: 100%"
                                    :disabled="loadListTree"
                                    ></select-department-tree-component>
                      </el-form-item>
                      <!-- <el-form-item v-if="selectUserType == 2">
                        <el-select
                          v-model="selectDepartment"
                          multiple
                          filterable
                          collapse-tags
                          reserve-keyword
                          default-first-option
                          style="width: 100%; padding-right: 10px;"
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
                      <el-form-item v-if="selectUserType.includes(4)">
                        <el-select
                          v-model="selectClass"
                          multiple
                          collapse-tags
                          filterable
                          default-first-option
                          style="width: 100%; padding-right: 10px;"
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
                    </el-col>
                    <el-col :span="8">
                      <el-form-item>
                        <el-input
                          :placeholder="$t('SearchData')"
                          v-model="filter"
                          @keyup.enter.native="Filter()">
                          <i slot="suffix" class="el-icon-search" @click="Filter()"></i>
                        </el-input>
                      </el-form-item>
                    </el-col>
                    <el-col :span="30">
                      <el-form-item>
                        <el-button
                          type="primary"
                          @click="Tab1View"
                          class="smallbutton"
                          size="small"
                          style="margin-left: 10px"
                        >
                          <span class="add">{{ $t("View") }}</span>
                        </el-button>
                      </el-form-item>
                    </el-col>

                    <el-col :span="30" style="float: right">
                      <el-form-item style="margin-top: 0px">
                        <el-button
                          type="primary"
                          style="margin-right: 0px; float: right"
                          class="smallbutton"
                          size="small"
                          @click="showDialogAuthorize"
                          >{{ $t("UpdateUserPrivilege") }}</el-button
                        >
                      </el-form-item>
                    </el-col>
                  </el-form>
                </el-row>
                <el-row>
                  <el-col :span="24">
                    <data-table-function-component
                      :showButtonInsert="false"
                      :isHiddenEdit="true" :isHiddenDelete="true"
                      :showButtonColumConfig="true" :gridColumnConfig.sync="columnsPermissions"
                      style="height: fit-content; display: flex; position: relative; top: 0; width: 100%;"
                      ref="privilegeFunction"
                    ></data-table-function-component>
                    <el-table class="privilege-user__table"
                      ref="multipleTable"
                      @select-all="SelectAllTable"
                      :data="dataDBCurrent"
                      style="width: 100%"
                      @selection-change="handleSelectionChange"
                      type="selection"
                      :max-height="maxHeightPrivilege"
                    >
                      <el-table-column
                        type="selection"
                        width="30"
                        :fixed="true"
                      ></el-table-column>
                      <el-table-column
                        v-for="column in columnsPermissions.filter(x => x.display)"
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
                  </el-col>
                  <el-col class="page-container">
                       <slot name="pagination">
                          <div class="page-number">
                              <small>{{$t("Display")}}</small>
                              <el-input v-model="pageSizeTab1"
                                        @change="onChangePageSizeTab1"
                                        filterable
                                        default-first-option
                                        style=" margin-left:10px;width:80px ">
                                  <!-- <el-option :key="3" :label="50" :value="50"></el-option>
                                  <el-option :key="4" :label="100" :value="100"></el-option>
                                  <el-option :key="5" :label="150" :value="150"></el-option>
                                  <el-option :key="6" :label="200" :value="200"></el-option> -->
                              </el-input>
                          </div>
                          <el-pagination class="custom-pagination" :total="totalTab1"
                                        :page-size="parseInt(pageSizeTab1)"
                                        :current-page="pageTab1"
                                        @current-change="Tab1ViewChangePage"
                                        layout="prev, pager, next"></el-pagination>
                          <div class="total-record">
                              <small>Tổng số: <b>{{totalTab1}}</b></small>
                          </div>
                      </slot>
                  </el-col>
                </el-row>
              </el-tab-pane>

              <el-tab-pane name="compare"
                v-if="usingBasicMenu === false"
                style="min-height: 550px"
              >
                <span slot="label">
                  <i class="el-icon-s-data"></i>
                  {{ $t("CompareWithUserMasterInfo") }}
                </span>
                <el-row>
                  <el-form :inline="true" class="form">
                    <el-col :span="3">
                      <el-form-item style="margin-bottom: 10px;">
                          <el-select
                            v-model="selectUserType"
                            :multiple="multiple"
                            filterable
                            collapse-tags
                            :placeholder="$t('UserType')"
                            @change="onChangeUserTypeSelect"
                          >
                            <el-option
                              :key="selectUserType.length == selectUserTypeOption.length ? -2 : -1"
                              :label="selectUserType.length == selectUserTypeOption.length ? $t('DeselectAll') : $t('SelectAll')"
                              :value="selectUserType.length == selectUserTypeOption.length ? -2 : -1"
                            ></el-option>
                            <el-option
                              v-for="item in selectUserTypeOption"
                              :key="item.value"
                              :label="$t(item.label)"
                              :value="item.value"
                            ></el-option>
                          </el-select>
                        </el-form-item>
                         <el-form-item style="margin-bottom: 10px;" v-if="selectUserType.includes(1) || selectUserType.includes(2) ||selectUserType.includes(6)">
                        <!-- <el-select
                          v-model="selectDepartment"
                          multiple
                          filterable
                          clearable
                          reserve-keyword
                          collapse-tags
                          default-first-option
                          style="width: 350px"
                          :placeholder="$t('SelectDeparment')"
                        >
                          <el-option
                            :key="allDepartments"
                            :label="$t(allDepartments)"
                            :value="allDepartments"
                          ></el-option>
                          <el-option
                            v-for="item in listAllDepartment"
                            :key="item.value"
                            :label="item.parent ? item.parent + ' - ' + item.label : item.label"
                            :value="item.value"
                          ></el-option>
                        </el-select> -->
                        <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll"
                                    :multiple="tree.multiple"
                                    :placeholder="$t('SelectDepartment')"
                                    :data="tree.treeData"
                                    :props="tree.treeProps"
                                    :isSelectParent="true"
                                    :checkStrictly="tree.checkStrictly"
                                    :clearable="tree.clearable"
                                    :popoverWidth="tree.popoverWidth"
                                    v-model="selectDepartment"
                                    style="width: 100%"
                                    :disabled="loadListTree"
                                    ></select-department-tree-component>
                      </el-form-item>
                      <el-form-item style="margin-bottom: 10px;" v-if="selectUserType.includes(4)">
                        <el-select
                          v-model="selectClass"
                          multiple
                          filterable
                          collapse-tags
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
                    </el-col>
                    <el-col :span="3">
                      <el-form-item style="margin-bottom: 10px;">
                          <el-input
                            :placeholder="$t('SearchData')"
                            v-model="filterCompare"
                            @keyup.enter.native="FilterUserCompare()">
                            <i slot="suffix" class="el-icon-search" @click="FilterUserCompare()"></i>
                          </el-input>
                        </el-form-item>
                    </el-col>
                    <el-col :span="2">
                      <el-button
                          style="float: left;"
                          type="primary"
                          @click="FilterUserCompare"
                          class="smallbutton"
                          size="small"
                        >
                          <span class="add">{{ $t("View") }}</span>
                        </el-button>
                    </el-col>
                    <el-col :span="12">

                      <el-form-item style="margin-bottom: 10px;">
                          <el-select
                            v-model="baseOnCompare"
                            filterable
                            default-first-option
                            @change="selectBaseOnCompareChange"
                          >
                            <el-option
                              :key="1"
                              :label="$t('BaseOnDatabase')"
                              value="BaseOnDatabase"
                            ></el-option>
                            <el-option
                              :key="2"
                              :label="$t('BaseOnDevice')"
                              value="BaseOnDevice"
                            ></el-option>
                          </el-select>
                        </el-form-item>
                        <el-form-item style="margin-bottom: 10px;">
                          <el-select
                            v-model="selectMachineCompare"
                            multiple
                            filterable
                            reserve-keyword
                            collapse-tags
                            default-first-option
                            @change="selectMachineCompareChange"
                            :placeholder="$t('SelectDevice')"
                          >
                            <el-option
                              :key="allMachineCompares"
                              :label="$t(allMachineCompares)"
                              :value="allMachineCompares"
                            ></el-option>
                            <el-option
                              v-for="item in selectMachineCompareOption"
                              :key="item.value"
                              :label="item.label"
                              :value="item.value"
                            ></el-option>
                          </el-select>
                        </el-form-item>
                    </el-col>
                    <el-col :span="2">
                          <el-button style="width: 100%;"
                            type="primary"
                            class="smallbutton"
                            size="small"
                            @click="DownloadUserInfo"
                            >{{ $t("DownloadUser") }}</el-button
                          >
                    </el-col>
                    <el-col :span="2">
                        <el-button
                          style="width: 100%; margin-left: 10px;"
                          type="primary"
                          class="smallbutton"
                          size="small"
                          @click="ExportDeviceToExcel"
                          >{{ $t("ExportToExcel") }}</el-button>
                        <data-table-function-component
                          :showButtonInsert="false"
                          :isHiddenEdit="true" :isHiddenDelete="true"
                          :showButtonColumConfig="true" :gridColumnConfig.sync="columCompare"
                          style="height: fit-content; display: flex; position: relative; top: 0; width: 100%;
                          text-align: left;margin-top: 10px; margin-right: 0 !important;"
                          ref="compareFunction"
                        ></data-table-function-component>
                    </el-col>
                  </el-form>
                </el-row>
                <el-row>
                  <el-col :span="11">
                    <span style="font-weight: bold">
                      {{ $t("UserInDatabase") }}
                    </span>
                  </el-col>
                  <el-col :span="2">
                    <p></p>
                  </el-col>
                  <el-col :span="11">
                    <span style="font-weight: bold">
                      {{ $t("UserOnDevice") }}
                    </span>
                  </el-col>
                 
                </el-row>
                <el-row>
                  <el-col :span="11" style="padding: 10px; padding-top: 0px">
                    <el-table
                      class="compare-user-master-info__user-master-table"
                      ref="userMasterTable"
                      @select-all="SelectAllUserMasterTable"
                      :data="dataDBCurrentUserMaster"
                      style="width: 100%; height: calc(100vh - 270px);"
                      @selection-change="handleSelectionUserMasterChange"
                      type="selection"
                      :max-height="maxHeighttransfer"
                    >
                      <!--<el-table-column type="selection"
                                                                  width="30"
                                                                  :fixed="true"></el-table-column>-->
                      <el-table-column
                        v-for="column in columCompare.filter(x => x.display)"
                        :key="column.prop"
                        :fixed="column.fixed || false"
                        v-bind="column"
                        :label="$t(column.label)"
                      ></el-table-column>
                      <template slot="append">
                        <div
                          class="unique-ids"
                          v-infinite-scroll="onInfiniteLazyLoadUserMaster"
                          :infinite-scroll-disabled="loadingLazyUserMaster"
                          :infinite-scroll-distance="50"
                        ></div>
                      </template>
                    </el-table>
                  </el-col>
                  <el-col
                    :span="2"
                    style="
                      text-align: center;
                      vertical-align: middle;
                      padding-top: 15%;
                    "
                  >
                    <!--<el-button type="primary" disable="true"><i class="el-icon-d-arrow-left"></i><i class="el-icon-d-arrow-right"></i></el-button>-->
                  </el-col>
                  <el-col :span="11" style="padding: 10px; padding-top: 0px">
                    <el-table
                      class="compare-user-master-info__user-info-table"
                      ref="userInfoTable"
                      @select-all="SelectAllUserInfoTable"
                      :data="dataDBCurrentUserInfo"
                      style="width: 100%; height: calc(100vh - 270px);"
                      @selection-change="handleSelectionUserInfoChange"
                      type="selection"
                      :max-height="maxHeighttransfer"
                    >
                      <el-table-column
                        v-for="column in columCompare.filter(x => x.display)"
                        :key="column.prop"
                        :fixed="column.fixed || false"
                        v-bind="column"
                        :label="$t(column.label)"
                      ></el-table-column>
                      <template slot="append">
                        <div
                          class="unique-ids"
                          v-infinite-scroll="onInfiniteLazyLoadUserInfo"
                          :infinite-scroll-disabled="loadingLazyUserInfo"
                          :infinite-scroll-distance="50"
                        ></div>
                      </template>
                    </el-table>
                  </el-col>
                </el-row>
                <el-row>
                  <el-col :span="24">
                    <el-col :span="3">
                      <div class="total-record">
                        <small>
                          {{ $t("Employee") }}:
                          <b>{{ countUserMaster }}</b>
                        </small>
                      </div>
                    </el-col>
                    <el-col :span="2">
                      <div class="total-record">
                        <small>
                          {{ $t("Card") }}:
                          <b>{{ countUserMasterCard }}</b>
                        </small>
                      </div>
                    </el-col>
                    <el-col :span="2">
                      <div class="total-record">
                        <small>
                          {{ $t("Pass") }}:
                          <b>{{ countUserMasterPass }}</b>
                        </small>
                      </div>
                    </el-col>
                    <el-col :span="2">
                      <div class="total-record">
                        <small>
                          {{ $t("Finger") }}:
                          <b>{{ countUserMasterFinger }}</b>
                        </small>
                      </div>
                    </el-col>
                    <el-col :span="3">
                      <div class="total-record">
                        <small>
                          {{ $t("Face") }}:
                          <b>{{ countUserMasterFace }}</b>
                        </small>
                      </div>
                    </el-col>

                    <el-col :span="3" style="margin-left: 30px">
                      <div class="total-record">
                        <small
                          >{{ $t("Employee") }}: <b>{{ countUser }}</b></small
                        >
                      </div>
                    </el-col>
                    <el-col :span="2">
                      <div class="total-record">
                        <small
                          >{{ $t("Card") }}: <b>{{ countUserCard }}</b></small
                        >
                      </div>
                    </el-col>
                    <el-col :span="2">
                      <div class="total-record">
                        <small
                          >{{ $t("Pass") }}: <b>{{ countUserPass }}</b></small
                        >
                      </div>
                    </el-col>
                    <el-col :span="2">
                      <div class="total-record">
                        <small>
                          {{ $t("Finger") }}:
                          <b>{{ countUserFinger }}</b>
                        </small>
                      </div>
                    </el-col>
                    <el-col :span="2">
                      <div class="total-record">
                        <small
                          >{{ $t("Face") }}: <b>{{ countUserFace }}</b></small
                        >
                      </div>
                    </el-col>
                  </el-col>
                </el-row>
              </el-tab-pane>
            </el-tabs>
          </el-container>
        </div>
      </el-col>
    </el-row>

    <div>
      <!--Dialog notification-->
      <el-dialog
        :title="$t('Notify')"
        :visible.sync="showMessage"
        width="20%"
        height="20%"
        center
      >
        <span>{{ $t("ProcessingRequest") }}</span>
      </el-dialog>
    </div>
    <div>
      <!--Dialog chosse excel-->
      <el-dialog
        :title="$t('AutoSelectExcel')"
        custom-class="customdialog"
        :visible.sync="showDialogExcel"
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
        <el-form
          label-width="168px"
          label-position="top"
          @keyup.enter.native="Submit"
        >
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
        :title="$t('AuthenMode')"
        custom-class="customdialog"
        :visible.sync="showDialogAuthenMode"
        :before-close="cancelDialog"
        :close-on-click-modal="false"
      >
        <el-form
          label-width="168px"
          label-position="top"
          @keyup.enter.native="Submit"
        >
          <el-form-item>
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
            {{ $t("InsertToMachine") }}
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
                  props="selectedGroup"
                  v-model="selectedGroup"
                  filterable
                  clearable
                  @change="handleGroupDeviceChange"
                  :placeholder="$t('SelectGroupDevice')"
                >
                  <el-option
                    v-for="item in listGroupDevice"
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
                v-for="column in columnsPermissions.filter(x => x.display)"
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

<script lang="ts" src="./auto-synch-user-component.ts"></script>
<style lang="scss" scoped>
.more-text {
  color: #0C66E4; ;
  margin-left: 10px;
}
</style>
<style lang="scss">
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

// .transferCompnent .el-checkbox-group.el-transfer-panel__list {
//   height: 100%;
// }

.transferCompnent .el-button {
  margin-left: 0;
}

.sync-user__department_dropdown .el-select__tags span span:first-child {
  max-width: 80%;
  overflow: hidden;
}

.user-master-info__right-function-bar-item {
  display: flex !important;
  .el-form-item__content{
    flex: 1;
  }
}

.compare-user-master-info__user-master-table,
.compare-user-master-info__user-info-table {
  .el-table__body-wrapper:has(.el-table__empty-block){
    display: none;
  }
  .el-table__fixed-body-wrapper:has(.el-table__empty-block){
    display: none;
  }
}
.sync-user__table{
  width: 100%; 
  height: calc(100vh - 333px);
  .el-table__body-wrapper{
    max-height: calc(100vh - 366px) !important;
  }
  .el-table__fixed::before{
    height: 0 !important;
  }
  .el-table__fixed{
    max-height: calc(100vh - 333px) !important;
    .el-table__fixed-body-wrapper{
      top: 35px !important;
      max-height: calc(100vh - 372px) !important;
    }
  }
}

// .privilege-user__table {
//   max-height: calc(100vh - 208px) !important;
//   overflow-y: auto;
// }
</style>
