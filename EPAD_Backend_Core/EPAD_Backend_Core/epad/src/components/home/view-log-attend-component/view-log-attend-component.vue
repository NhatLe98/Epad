<template>
    <div id="bgHome" class="view-log-attend">
        <el-row>
            <el-col :span="5" class="Tree">
                <el-input class="view-log-attend__input-tree"
                style="padding: 5px; position:absolute; width:20%; height:30px;z-index:1000"
                          :placeholder="$t('SearchData')"
                          v-model="filterTree"
                          @keyup.enter.native="filterTreeData()">
                          <i slot="suffix" class="el-icon-search" @click="filterTreeData()"></i>
                </el-input>
                <el-tree style="margin-top:50px" :data="treeData" :props="{ label: 'Name', children: 'ListChildrent' }"
                         :filter-node-method="filterNode"
                         :default-expanded-keys="expandedKey"
                         :default-checked-keys="defaultChecked"
                         @check="nodeCheck"
                         ref="tree"
                         show-checkbox highlight-current node-key="ID" v-loading="loadingTree">
                    <template slot-scope="scoped">
                        <div>
                            <i :class="getIconClass(scoped.data.Type,scoped.data.Gender)" />
                            <span class="ml-5">{{ scoped.data.Name }}</span>
                        </div>
                    </template>
                </el-tree>
            </el-col>
            <el-col :span="19">
                <el-container>
                    <el-header>
                        <el-row>
                            <el-col :span="10" class="left">
                                <span id="FormName">{{$t("ViewLogAttend")}}</span>
                            </el-col>
                            <el-col :span="14">
                                <HeaderComponent :showMasterEmployeeFilter="true"/>
                            </el-col>
                        </el-row>
                    </el-header>

                    <el-main class="change-department">

                        <div style="width:100%;float:left;">
                            <el-form :inline="true">
                                <el-row>
                                    <el-col :span="4">
                                        <el-input style="float:left; width:100%;heigth:32px;padding-right:10px;"
                                                  :placeholder="$t('Search')" v-model="filter"
                                                  class="filter-input">
                                        </el-input>
                                    </el-col>
                                    <el-col :span="4">
                                        <el-form-item class="inputFromDate">
                                            <!-- <el-date-picker  type="date" style="width:160px" ></el-date-picker> -->
                                            <el-date-picker id="inputFromDate" class="inputDateTime"
                                            style="width: 100%;"
                                                            v-model="fromDate" type="datetime" default-time="00:00:00">
                                            </el-date-picker>
                                        </el-form-item>
                                    </el-col>
                                    <el-col :span="4">
                                        <el-form-item class="inputToDate">
                                            <!-- <el-date-picker v-model="toDate" type="date" style="width:180px" id="inputToDate"></el-date-picker> -->
                                            <el-date-picker id="inputToDate" class="inputDateTime"
                                            style="width: 100%;"
                                                            v-model="toDate" type="datetime" default-time="23:59:59">
                                            </el-date-picker>
                                        </el-form-item>
                                    </el-col>
                                    <el-col :span="1">
                                        <el-form-item class="button">
                                            <el-button @click="View" 
                                            type="primary">
                                                {{$t("View")}}
                                            </el-button>
                                        </el-form-item>
                                    </el-col>
                                    <el-col :span="10" style="text-align:right">
                                        <el-form-item class="button" style="margin-right: 0">
                                            <el-button type="primary" @click="Import">
                                                {{ $t("AddFromFile") }}
                                            </el-button>
                                            <el-button style="margin-left:10px;" type="primary" @click="Export">
                                                {{ $t("ExportToExcel") }}
                                            </el-button>
                                            <el-button style="margin-left:10px;" type="primary" @click="RunIntegrateLog" v-if="isFull">
                                                {{ $t("RunIntegrateLog") }}
                                            </el-button>
                                        </el-form-item>
                                    </el-col>
                                    <el-col :span="1">
                                        <el-form-item style="float:right;">
                                            <data-table-function-component style="top:0px" :gridColumnConfig.sync="columns"
                                                                           :isHiddenEdit="true"
                                                                           :isHiddenDelete="true"
                                                                           :isHiddenSearch="true"
                                                                           :showButtonColumConfig="true"
                                                                           :showButtonInsert="false"></data-table-function-component>
                                        </el-form-item>
                                    </el-col>
                                </el-row>
                            </el-form>
                        </div>
                        <div class="table">
                            <data-table-component class="view-log-attend__data-table"
                                :get-data="getData" :columns="columns" :showSearch="false"
                                :selectedRows.sync="rowsObj" ref="table" :isShowPageSize="true">
                            </data-table-component>
                        </div>
                        <import-popup-component :title="titlePopupImport" :classProcess="'Log'" :numberOfFiles="4"
                                                ref="popupImport">

                        </import-popup-component>
                        <el-dialog :title="$t('ChooseTimePeriod')" :visible.sync="isShowAttendance" custom-class="customdialog" :before-close="Cancel" >
            <el-form
              :model="TimeForm"
              :rules="rule"
              ref="TimeForm"
              label-width="168px"
              label-position="top"
              @keyup.enter.native="DownloadAttendanceData"
            >
           
             
              <el-form-item :label="$t('DownloadFromPreviousDay')" prop="ToTime">
                <el-input-number
                      ref="DownloadFromPreviousDay"
                      v-model="PreviousDays"
                      style="width: 100%"
                    ></el-input-number>
                 <span class="day-in-config">{{ $t(dayToCurrentDay) }}</span>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">{{ $t("Cancel") }}</el-button>
              <el-button class="btnOK" type="primary" @click="DownloadAttendanceData">{{ $t("OK") }}</el-button>
            </span>
          </el-dialog>
                    </el-main>
                </el-container>
            </el-col>
        </el-row>
    </div>
</template>
<script src="./view-log-attend-component.ts"></script>

<style lang="scss">
    .view-log-attend .inputDateTime {
        width: 220px;
    }

    .filter-input.el-input--small input.el-input__inner {
        height: 32px;
    }
    .view-log-attend__data-table .el-table {
        height: calc(100vh - 208px) !important;

        .el-table__empty-text {
            width: auto;
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
        }
    }

    @media all and (min-device-width: 960px) and (max-device-width: 1360px) {
        .view-log-attend .inputDateTime {
            width: 170px;
        }
    }
    .view-log-attend__input-tree {
        .el-input__suffix{
            top: unset !important;
            margin-right: 5px;
        }
    }
</style>
