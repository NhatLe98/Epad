<template>
    <div class="datatable">
        <!-- old attribute: style="padding-bottom:3px; float:left; width:238px;" -->
        <el-input v-if="showSearch"
                style="padding-bottom:3px; float:left; width:238px;"
                  :placeholder="filterPlaceHolder && filterPlaceHolder != '' ? filterPlaceHolder : $t('SearchData')"
                  v-model="filter"
                  @keyup.enter.native="getTableData(1)"
                  class="filter-input">
                  <i slot="suffix" class="el-icon-search" @click="getTableData(1)"></i>
        </el-input>
        <el-table v-loading="loading"
                  :data="getDataFilter"
                  style="width: 100%"
                  :height="maxHeight"
                  @selection-change="handleSelectionChange"
                  :row-class-name="tableRowClassName"
                  ref="multipleTable">
            <slot name="columns">
                <el-table-column type="selection" v-if="showCheckbox" :selectedRows="selectObj" width="27"></el-table-column>

                <el-table-column type="index"
                                 :index="index"
                                 label="#"
                                 :fixed="true"></el-table-column>

                <el-table-column :sortable="true"
                                 v-for="column in columns.filter(x => x.display)"
                                 :key="column.prop"
                                 v-bind="column"
                                 :label="$t(column.label)"
                                 :fixed="column.fixed || false"
                                 ref="tableRef">
                    <template slot-scope="{ row }">
                        <slot v-if="column.prop != 'HardWareLicense' && column.prop != 'Status' && column.prop != 'JobName' 
                          && column.prop != 'DeviceName' && column.prop != 'IsUsing' && column.dataType !='date' 
                          && column.dataType != 'translate' && column.dataType != 'lookup' && column.dataType != 'yesno'
                          && column.dataType != 'listString' && column.dataType != 'cardStatus' && column.prop != 'ExtraDriver'"
                              :name="column.prop || column.type || column.label"
                              :row="row">{{ row[column.prop] }}</slot>
                              <span v-if="column.dataType == 'translate'" 
                              :name="column.prop || column.type || column.label"
                              :row="row">{{$t(row[column.prop])  }}</span>
                              <span v-if="column.dataType == 'date'" 
                              :name="column.prop || column.type || column.label"
                              :row="row">  {{ getDate(column.format,row[column.prop]) }} </span> 

                        <span v-if="row[column.prop]=='NotLicense'" class="red"
                              :name="column.prop || column.type || column.label"
                              :row="row">{{$t(row[column.prop])  }}</span>
                        <span v-if="row[column.prop]=='HwLicense'" class="green"
                              :name="column.prop || column.type || column.label"
                              :row="row">{{$t(row[column.prop])  }}</span>
                              <span v-if="column.prop == 'JobName'" 
                              >{{$t(row[column.prop])  }}</span>
                        
                              <span v-if="column.dataType === 'yesno'" :name="column.prop || column.type || column.label" :row="row"
                                style="font-size: 12px !important;">
                                  {{ row[column.prop] ? $t('Yes') : $t('No')}}
                              </span>
                              <span v-if="column.dataType === 'cardStatus'" :name="column.prop || column.type || column.label" :row="row"
                                style="font-size: 12px !important;">
                                  {{ row[column.prop] ? $t('true') : $t('false')}}
                              </span>
                              <span v-if="column.dataType === 'listString'" :name="column.prop || column.type || column.label" :row="row"
                                style="font-size: 12px !important;">
                                  {{ (row[column.prop] && row[column.prop].length > 0) ? row[column.prop].join(", ") : '' }}
                              </span>
                              <span v-if="column.dataType === 'lookup'" :name="column.prop || column.type || column.label" :row="row">
                                {{ getLookup(column.lookup, row[column.prop]) }}
                            </span>
                        <slot v-if="column.prop == 'Status' && column.dataType != 'cardStatus'"
                              :name="column.prop || column.type || column.label"
                              :row="row">
                            <el-tooltip v-if="row[column.prop] != 'Đang xử lý'" effect="light" placement="right">
                                <div slot="content">
                                    <span v-for="(command, cix) in row['ListRunningCommand']" :key="`cmd-${cix}`">
                                        {{$t(command)}}<br />
                                    </span>
                                </div>
                                <span>{{ row[column.prop] }}</span>
                            </el-tooltip>
                            <el-progress v-if="row[column.prop] == 'Đang xử lý'" type="circle" :percentage="row['TimeRemain']" :width="40"></el-progress>
                        </slot>
                        <slot v-if="column.prop == 'DeviceName'"
                              :name="column.prop || column.type || column.label"
                              :row="row">
                              <div>
                                <el-tooltip v-if="column.prop == 'DeviceName'"
                                  class="box-item"
                                  :content="row[column.prop]"
                                  placement="top-start"
                                >
                                  <p class="textOverflow">{{row[column.prop]}}</p>
                                </el-tooltip>
                              </div>
                        </slot>
                        <slot v-if="column.prop == 'ExtraDriver'"
                              :name="column.prop || column.type || column.label"
                              :row="row">
                              <div>
                                <span v-show="row.ExtraDriver && row.ExtraDriver.length > 0" @click="showExtraDriver(row)" 
                                style="cursor: pointer; color: blue; font-weight: bold;">{{ $t('SeeDetail...') }}</span>
                                <!-- <el-button type="primary" size="mini" @click="showExtraDriver(row)">{{ $t('SeeDetail...') }}</el-button> -->
                              </div>
                        </slot>
                        <slot v-if="column.prop == 'IsUsing'"
                              :name="column.prop || column.type || column.label"
                              :row="row">
                              <div>
                                <el-checkbox
                          class="checkbox-item"
                          :checked="row[column.prop]"
                          :disabled="true"
                        >
                        </el-checkbox>
                              </div>
                        </slot>
                    </template>
                </el-table-column>
            </slot>

        </el-table>
        <el-col class="page-container">
            <slot name="pagination" v-if="!isHiddenPaging">
                <div class="page-number" v-if="isShowPageSize">
                    <small>{{$t("Display")}}</small>
                    <el-input v-model="pageSize"
                               @change="onChangePageSize"
                               filterable
                               default-first-option
                               style=" margin-left:10px;width:80px ">
                        <!-- <el-option :key="3" :label="50" :value="50"></el-option>
                        <el-option :key="4" :label="100" :value="100"></el-option>
                        <el-option :key="5" :label="150" :value="150"></el-option>
                        <el-option :key="6" :label="200" :value="200"></el-option> -->
                    </el-input>
                </div>
                <el-pagination class="custom-pagination" :total="total"
                               :page-size="parseInt(pageSize)"
                               :current-page="page"
                               @current-change="getTableData"
                               layout="prev, pager, next"></el-pagination>
                <div class="total-record" v-if="isShowPageSize">
                    <small>Tổng số: <b>{{total}}</b></small>
                </div>
            </slot>
        </el-col>

    </div>
</template>
<script src="./data-table-component.ts"></script>

<style lang="scss">
.datatable {
  .el-table {
    margin-top: 35px;
  }
}

/* .filter-input {
  margin-bottom: 15px;
} */

.filter-input.el-input--small input.el-input__inner {
  height: 32px;
}

.offline td:nth-last-child(1) .cell,
.offline7 td:nth-child(7) .cell {
  color: #5c6aff;
  font-weight: 600 !important;
}

.online td:nth-last-child(1) .cell,
.online7 td:nth-child(7) .cell {
  color: #3bd97f;
  font-weight: 600 !important;
}

.red {
  color: red;
  font-weight: 600 !important;
  font-size: 12px;
}

.green {
  color: #3bd97f;
  font-weight: 600 !important;
  font-size: 12px;
}

.page-container {
  margin-top: 5px;
  text-align: center;
}

.total-record {
  display: inline;
  margin-left: 10px;
}

.custom-pagination {
  display: inline-block;
  text-align: center;
}

.datatable {
  .page-number {
    display: inline;
    width: 70px;
    margin-top: 10px;
  }

  .page-number .el-input-number--small {
    width: 70px;
  }

  .page-number .el-input-number.is-controls-right .el-input__inner {
    padding-left: 10px;
    padding-right: 10px;
    height: 22px;
    width: 60px;
  }

  .page-number
    .el-input-number.is-controls-right[class*="small"]
    [class*="decrease"],
  .el-input-number.is-controls-right[class*="small"] [class*="increase"] {
    display: none;
  }

  .page-number
    .el-input-number.is-controls-right[class*="small"]
    [class*="decrease"],
  .el-input-number.is-controls-right[class*="small"] [class*="increase"] {
    display: none;
  } 
}

.el-table-column--selection .cell,
.el-table th > .cell,
.el-table .cell {
  padding: initial;
}
.warningrow {
  color: red;
}

.near-expire td{
  color: black !important;
  background-color: #ff0 !important;
}
.near-expire td div{
  font-weight: bold !important;
}

.expired td{
  color: black !important;
  background-color: #ff9a9a !important;
}
.expired td div{
  font-weight: bold !important;
}

.el-icon-search {
  line-height: 32px;
}

.textOverflow {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin: 0;
}
</style>
