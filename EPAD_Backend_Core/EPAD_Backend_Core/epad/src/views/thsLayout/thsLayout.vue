<template>
  <div class="ths">
    <div class="thsLayout">
      <h3>Button basic</h3>
      <el-row
        class="mb-4"
        style="display: flex; justify-content: space-between;"
      >
        <el-button>Default</el-button>
        <el-button type="primary">Primary</el-button>
        <el-button type="success">Success</el-button>
        <el-button type="info">Info</el-button>
        <el-button type="warning">Warning</el-button>
        <el-button type="danger">Danger</el-button>
      </el-row>

      <el-row
        class="mb-4"
        style="display: flex; justify-content: space-between;"
      >
        <el-button plain>Plain</el-button>
        <el-button type="primary" plain>Primary</el-button>
        <el-button type="success" plain>Success</el-button>
        <el-button type="info" plain>Info</el-button>
        <el-button type="warning" plain>Warning</el-button>
        <el-button type="danger" plain>Danger</el-button>
      </el-row>

      <el-row
        class="mb-4"
        style="display: flex; justify-content: space-between;"
      >
        <el-button round>Round</el-button>
        <el-button type="primary" round>Primary</el-button>
        <el-button type="success" round>Success</el-button>
        <el-button type="info" round>Info</el-button>
        <el-button type="warning" round>Warning</el-button>
        <el-button type="danger" round>Danger</el-button>
      </el-row>
    </div>

    <div class="thsLayout">
      <h3>Disabled Button</h3>
      <el-row
        class="mb-4"
        style="display: flex; justify-content: space-between;"
      >
        <el-button disabled>Default</el-button>
        <el-button type="primary" disabled>Primary</el-button>
        <el-button type="success" disabled>Success</el-button>
        <el-button type="info" disabled>Info</el-button>
        <el-button type="warning" disabled>Warning</el-button>
        <el-button type="danger" disabled>Danger</el-button>
      </el-row>

      <el-row style="display: flex; justify-content: space-between;">
        <el-button plain disabled>Plain</el-button>
        <el-button type="primary" plain disabled>Primary</el-button>
        <el-button type="success" plain disabled>Success</el-button>
        <el-button type="info" plain disabled>Info</el-button>
        <el-button type="warning" plain disabled>Warning</el-button>
        <el-button type="danger" plain disabled>Danger</el-button>
      </el-row>
    </div>

    <div class="thsLayout">
      <h3>Button Sizes</h3>
      <el-row class="my-4" style="display: flex; justify-content: start; gap: 15px;">
        <el-button size="small" round>Small</el-button>
        <el-button size="large" round>Large</el-button>
      </el-row>
    </div>

    <div class="thsLayout">
      <h3>Input Sizes</h3>
      <div class="demo-input-size">
        <el-input
          v-model="inputA"
          class="w-50 m-2"
          size="small"
          placeholder="small"
        />
        <el-input
          v-model="inputB"
          class="w-50 m-2"
          size="large"
          placeholder="large"
        />
      </div>
    </div>

    <div class="thsLayout">
      <h3>Input Number</h3>
      <div class="demo-input-size">
        <el-input-number v-model="numA" size="large" />
        <el-input-number v-model="numB" size="small" />
      </div>
    </div>

    <div class="thsLayout">
      <h3>Color</h3>
      <div class="container">
        <div class="color" v-for="item in data" :key="item.id">
          <div class="color-wrappwer">
            <div class="color-item" :style="'background-color: #' + item"></div>
            <div class="color-title">{{ item }}</div>
          </div>
        </div>
      </div>
    </div>

    <div class="thsLayout">
      <h3>Radio</h3>
      <div class="mb-2 flex items-center text-sm">
        <el-radio-group v-model="radio1" class="ml-4">
          <el-radio label="1" size="large">Option 1</el-radio>
          <el-radio label="2" size="large">Option 2</el-radio>
        </el-radio-group>
      </div>
      <div class="mb-2 flex items-center text-sm">
        <el-radio-group v-model="radio3" disabled class="ml-4">
          <el-radio label="1" size="small">Option 1</el-radio>
        </el-radio-group>
      </div>
    </div>

    <div class="thsLayout">
      <h3>Select</h3>
      <div style="display: flex; justify-content: start; align-items: center;">
        <el-select
          v-model="value"
          class="m-2"
          placeholder="Select"
          size="small"
        >
          <el-option
            v-for="item in options"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
        <el-select
          v-model="value"
          class="m-2"
          placeholder="Select"
          size="large"
        >
          <el-option
            v-for="item in options"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
        <el-select v-model="value" disabled placeholder="Select">
          <el-option
            v-for="item in options"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>
    </div>

    <div class="thsLayout">
      <h3>Multiple select</h3>
      <ul>
        <li>Search multiple or Single employee</li>
        <li>Multiple choise employee</li>
        <li>Paste list employee code</li>
      </ul>
      <AppSelectNew
        :placeholder="$t('RationPlaceHolder')"
        :dataSource="dataTemplate"
        displayMember="Name"
        valueMember="Index"
        :isDisabled="isEdit"
        :allowNull="true"
        :multiple="true"
        v-model="RationIndex"
        style="width: 100%"
      >
      </AppSelectNew>
    </div>

    <div class="thsLayout">
      <h3>Switch</h3>
      <div>
        <el-switch
          v-model="valueTempate"
          disabled
          style="--el-switch-on-color: #13ce66; --el-switch-off-color: #ff4949"
        />

        <el-switch
          v-model="value"
          style="margin-left: 24px"
          inline-prompt
          :active-icon="Check"
          :inactive-icon="Close"
        />
      </div>
    </div>

    <div class="thsLayout">
      <h3>Tab with Table</h3>
      <el-tabs
        v-model="editableTabsValueA"
        type="card"
        editable
        @edit="handleTabsEditA"
      >
        <el-tab-pane
          v-for="item in editableTabs"
          :key="item.name"
          :label="item.title"
          :name="item.name"
        >
          <el-table :data="tableData" style="width: 100%">
            <el-table-column prop="date" :label="item.content" width="180">
            </el-table-column>
            <el-table-column prop="name" :label="item.name" width="180">
            </el-table-column>
            <el-table-column prop="address" :label="item.title">
            </el-table-column>
          </el-table>
        </el-tab-pane>
      </el-tabs>
    </div>

    <div class="thsLayout">
      <h3>Tab with Collapse and Table</h3>
      <el-tabs
        v-model="editableTabsValueB"
        type="card"
        editable
        @edit="handleTabsEditB"
      >
        <el-tab-pane
          v-for="item in editableTabs"
          :key="item.name"
          :label="item.title"
          :name="item.name"
        >
          <el-collapse v-model="activeNames" @change="handleChange">
            <el-collapse-item title="Consistency" :name="item.content">
              <el-table :data="tableData" style="width: 100%">
                <el-table-column prop="date" :label="item.content" width="180">
                </el-table-column>
                <el-table-column prop="name" :label="item.name" width="180">
                </el-table-column>
                <el-table-column prop="address" :label="item.title">
                </el-table-column>
              </el-table>
            </el-collapse-item>
          </el-collapse>
        </el-tab-pane>
      </el-tabs>
    </div>

    <div class="thsLayout">
      <h3>Multiple select</h3>
      <ul>
        <li>Filter the table to find desired data.</li>
        <li>Sort the data to find or compare data quickly.</li>
        <li>You can also select multiple rows.</li>
        <li>You can search data by Name.</li>
      </ul>

      <div>
        <div
          style="
            display: grid;
            grid-template-columns: repeat(4, 200px);
            border: 1px solid #f5f7fa;
            background: #f5f7fa;
            padding: 11px 0 0 10px;
          "
        >
          <div v-for="item in listTitle" :key="item">
            <p style="color: '#1f336c'">{{ item.title }}</p>
          </div>
        </div>
        <el-table
          ref="filterTable"
          :data="
            tableData.filter(
              data =>
                !search ||
                data.name.toLowerCase().includes(search.toLowerCase())
            )
          "
          style="width: 100%"
          @selection-change="handleSelectionChange"
        >
          <el-table-column type="selection" width="200px"> </el-table-column>
          <el-table-column
            prop="date"
            label="Filter"
            sortable
            width="180"
            column-key="date"
            :filters="[
              { text: '2016-05-01', value: '2016-05-01' },
              { text: '2016-05-02', value: '2016-05-02' },
              { text: '2016-05-03', value: '2016-05-03' },
              { text: '2016-05-04', value: '2016-05-04' },
            ]"
            :filter-method="filterHandler"
          >
          </el-table-column>

          <el-table-column align="right" min-width="21">
            <template slot="header" slot-scope="scope">
              <el-input
                :prop="scope"
                v-model="search"
                size="mini"
                style="width: 150px; display: flex"
                placeholder="Type to search"
              />
            </template>

            <template slot-scope="scope">
              <p style="text-align: start">{{ scope.row["name"] }}</p>
            </template>
          </el-table-column>
          <el-table-column
            prop="address"
            label="Address"
            :formatter="formatter"
          >
            <template slot="header">
              <el-select v-model="select" placeholder="Select" class="select">
                <el-option
                  v-for="item in options"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                >
                </el-option>
              </el-select>
            </template>

            <el-select v-model="selectA" placeholder="Select">
              <el-option
                v-for="item in options"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              >
              </el-option>
            </el-select>
          </el-table-column>
        </el-table>
      </div>
    </div>

    <div class="thsLayout">
      <h3>Time picker</h3>
      <el-time-select
        v-model="timeValue"
        :picker-options="{
          start: '08:30',
          step: '00:15',
          end: '18:30',
        }"
        placeholder="Select time"
      >
      </el-time-select>

      <h3>Arbitrary time picker</h3>
      <el-time-picker
        v-model="fromTime"
        :picker-options="{
          selectableRange: '18:30:00 - 20:30:00',
        }"
        placeholder="Arbitrary time"
      >
      </el-time-picker>
      <el-time-picker
        arrow-control
        v-model="toTime"
        :picker-options="{
          selectableRange: '18:30:00 - 20:30:00',
        }"
        placeholder="Arbitrary time"
      >
      </el-time-picker>
    </div>

    <div class="thsLayout">
      <h3>Button Export, Import, Edit, Delete</h3>
      <AppFunctionBar
        :gridColumnConfig.sync="listColumn"
        :showEdit="showEdit"
        :showDelete="showDelete"
        :showMore="showMore"
        :isAdd="true"
        :isUpdate="true"
        :isDelete="true"
        :isExport="true"
        :isImport="true"
        :isRead="true"
        @openDialog="openAddEditDialog"
        @deleteHandler="del"
      />
    </div>

    <div class="thsLayout">
      <h3>Pagination</h3>
      <!-- <slot name="pagination">
        <el-pagination
          :total="total"
          :page-sizes="[50, 100, 200, 300, 500, 1000, 1500, 2000]"
          :page-size="pagesize"
          :current-page="page"
          @current-change="page_click"
          @size-change="handleSizeChange_click"
          layout="sizes, prev, pager, next"
        ></el-pagination>
      </slot> -->
      <el-col class="page-container">
        <slot name="pagination">
          <div class="page-number">
            <small>{{ $t("Display") }}</small>
            <el-input
              v-model="pageSizeTab1"
              @change="onChangePageSizeTab1"
              filterable
              default-first-option
              style=" margin-left:10px;width:80px "
            >
            </el-input>
          </div>
          <el-pagination
            class="custom-pagination"
            :total="totalTab1"
            :page-size="parseInt(pageSizeTab1)"
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
    </div>

    <div class="thsLayout">
      <h3>Tab</h3>
      <el-row style="margin-top: 5px; background-color: white; padding: 10px">
        <el-tabs style="height: 100%">
          <el-tab-pane :label="$t('Danh sách thẻ')" :key="0">
            <mealcard-tabs-component :Type="0"> </mealcard-tabs-component>
          </el-tab-pane>
          <el-tab-pane :label="$t('Khai báo chức năng thẻ')" :key="1">
            <mealcard-tabs-component :Type="1"> </mealcard-tabs-component>
          </el-tab-pane>
        </el-tabs>
      </el-row>
    </div>

    <div class="thsLayout" style="background: #f6f8fa">
      <h3>Screen type 1</h3>

      <div class="main cm-canteen-info">
        <div class="body-content">
          <AppFunctionBar
            :gridColumnConfig.sync="listColumn"
            :showEdit="showEdit"
            :showDelete="showDelete"
            :showMore="showMore"
            :isAdd="true"
            :isUpdate="true"
            :isDelete="true"
            :isExport="true"
            :isImport="true"
            :isRead="true"
            @openDialog="openAddEditDialog"
            @deleteHandler="del"
          />

          <div class="tab-grid">
            <t-grid
              @onPageChange="onPageChange"
              @onPageSizeChange="onPageSizeChange"
              :gridColumns="columns"
              :dataSource="listData"
              :selectedRows.sync="selectedRows"
              :total="total"
              :page.sync="page"
              :pageSize.sync="pageSize"
            ></t-grid>
          </div>
        </div>
      </div>
    </div>

    <div class="thsLayout" style="background: #f6f8fa">
      <h3>Screen type 2</h3>

      <div class="main cm-lines">
        <AppFunctionBar
          :gridColumnConfig.sync="listColumn"
          :showEdit="showEdit"
          :showDelete="showDelete"
          :showMore="showMore"
          :isAdd="true"
          :isUpdate="true"
          :isDelete="true"
          :isExport="true"
          :isImport="true"
          :isRead="true"
          @openDialog="openAddEditDialog"
          @deleteHandler="del"
        />
        <el-row style="">
          <el-form
            :model="form"
            :rules="rule"
            ref="formref"
            label-position="top"
          >
            <el-form-item
              style="float: left; padding-left: 8px; padding-top: 7px"
            >
              <app-select
                :dataSource="listLine"
                displayMember="Name"
                valueMember="Index"
                :placeholder="$t('LineName')"
                :allowNull="true"
                :multiple="true"
                :collapse-tags="true"
                style="width: 100%"
              ></app-select>
            </el-form-item>

            <el-form-item
              style="float: left; padding-left: 8px; padding-top: 5px"
            >
              <app-select
                :dataSource="listCanteen"
                displayMember="Name"
                valueMember="Index"
                :allowNull="true"
                :placeholder="$t('CanteenIndex')"
                :multiple="true"
                :collapse-tags="true"
                style="width: 100 %"
              ></app-select>
            </el-form-item>
          </el-form>
        </el-row>
        <div class="tab-grid">
          <t-grid
            @onPageChange="onPageChange"
            @onPageSizeChange="onPageSizeChange"
            :gridColumns="columns"
            :dataSource="listData"
            :selectedRows.sync="selectedRows"
            :total="total"
            :page.sync="page"
            :pageSize.sync="pageSize"
          ></t-grid>
        </div>
      </div>
    </div>

    <div class="thsLayout" style="background: #f6f8fa">
      <h3>Screen type 3</h3>

      <div class="main cm-rules-global">
        <el-row style="height: 100%">
          <el-col class="colList" :span="6">
            <el-table
              ref="tblRules"
              :data="listRules"
              @current-change="currentRowChanged"
              highlight-current-row
              style="width: 100%"
              height="100%"
            >
              <el-table-column type="index" width="50"></el-table-column>
              <el-table-column
                property=""
                :label="$t('RulesList')"
                :formatter="getDate"
              ></el-table-column>
            </el-table>
          </el-col>

          <el-col class="colDetail" :span="18">
            <div class="divScrollCollapse">
              <el-collapse v-model="activeCollapse" style="margin-top: 6px">
                <el-form
                  ref="form"
                  :model="ruleObject"
                  :rules="validationObject"
                  label-width="260px"
                  label-position="left"
                >
                  <el-collapse-item :title="$t('BasicInfo')" name="a">
                    <el-form-item
                      prop="AppliedDate"
                      :label="$t('AppliedDate')"
                      class="hasValue"
                    >
                      <el-date-picker
                        size="mini"
                        type="date"
                        :placeholder="$t('SelectDate')"
                        style="width: 40%"
                      >
                      </el-date-picker>
                    </el-form-item>

                    <el-form-item
                      prop="CanteenIndexes"
                      :label="$t('SelectCanteen')"
                    >
                      <app-select
                        :dataSource="listCanteen"
                        :multiple="true"
                        :allowNull="false"
                        :collapse-tags="true"
                        displayMember="Name"
                        valueMember="Index"
                        style="width: 45%"
                      ></app-select>
                    </el-form-item>

                    <el-form-item prop="Description" :label="$t('Description')">
                      <el-input placeholder="" style="width: 60%"></el-input>
                    </el-form-item>

                    <el-form-item
                      prop="AutoOfflineProcess"
                      :label="$t('AutoOfflineProcess')"
                    >
                      <el-checkbox></el-checkbox>
                    </el-form-item>

                    <el-form-item
                      prop="IntegrateWorkingSchedule"
                      :label="$t('IntegrateWorkingSchedule')"
                    >
                      <el-checkbox></el-checkbox>
                    </el-form-item>

                    <el-form-item
                      prop="IntegrateShift"
                      :label="$t('IntegrateShift')"
                    >
                      <el-checkbox></el-checkbox>
                    </el-form-item>

                    <el-form-item
                      prop="CheckMaxMealInMonth"
                      :label="$t('CheckMaxMealInMonth')"
                    >
                      <el-checkbox></el-checkbox>
                    </el-form-item>

                    <el-form-item
                      prop="CheckEmployeePermission"
                      :label="$t('CheckEmployeePermission')"
                    >
                      <el-checkbox></el-checkbox>
                    </el-form-item>

                    <el-form-item
                      prop="StartDayOfMonth"
                      :label="$t('StartDayOfMonth')"
                    >
                      <el-input-number
                        :min="0"
                        style="width: 30%"
                        placeholder=""
                      ></el-input-number>
                    </el-form-item>

                    <el-form-item
                      prop="MonthOfCalculating"
                      :label="$t('MonthOfCalculating')"
                    >
                      <app-select
                        :dataSource="listMonthOfCalculating"
                        displayMember="Name"
                        valueMember="Index"
                        style="width: 45%"
                      ></app-select>
                    </el-form-item>

                    <el-form-item
                      prop="MaxMealInMonth"
                      :label="$t('MaxMealInMonth')"
                    >
                      <el-input-number
                        :min="0"
                        style="width: 30%"
                        placeholder=""
                      ></el-input-number>
                    </el-form-item>

                    <el-form-item
                      prop="GiveBackCateringByRation"
                      :label="$t('GiveBackCateringByRation')"
                    >
                      <el-checkbox></el-checkbox>
                    </el-form-item>

                    <el-row>
                      <el-col :span="24">
                        <el-form-item
                          prop1="GiveBackCateringByTime"
                          :label="$t('GiveBackCateringByTime')"
                        >
                          <el-checkbox></el-checkbox>
                        </el-form-item>
                      </el-col>

                      <el-col :span="24">
                        <el-form-item
                          prop="AllowTimeGiveBack"
                          :label="$t('AllowTimeGiveBack')"
                        >
                          <el-input-number
                            :min="0"
                            style="width: 70%"
                            placeholder=""
                          ></el-input-number>
                        </el-form-item>
                      </el-col>
                    </el-row>
                  </el-collapse-item>
                </el-form>
              </el-collapse>
            </div>
          </el-col>
        </el-row>

        <el-row style="height: 33px">
          <el-col class="btn-left" :span="6">
            <el-button
              type="primary"
              icon="el-icon-plus"
              @click="open"
              style="float: right; width: 80px; height: 33px;"
            ></el-button>
            <el-button
              type="danger"
              icon="el-icon-delete"
              :disabled="ruleSelected == 0"
              @click="deleteRule"
              style="float: right; width: 80px; margin-right: 5px"
            ></el-button>
          </el-col>

          <el-col class="colDetail btn-right" :span="18">
            <el-button
              type="success"
              icon="el-icon-circle-check"
              @click="updateResult"
              style="float: right; width: 100px; margin-right: 20px"
            ></el-button>
          </el-col>
        </el-row>
      </div>
    </div>

    <div class="thsLayout" style="background: #f6f8fa">
      <h3>Screen type 4</h3>

      <div class="main" style>
        <el-row>
          <el-col
            :span="6"
            prop="CanteenIndex"
            :label="$t('Canteen')"
            style="padding-right: 10px"
          >
            <div class="sub-title">
              <span style="color: red">* </span>{{ $t("Canteen") }}
            </div>
            <app-select
              :dataSource="listCanteen"
              displayMember="Name"
              valueMember="Index"
              :allowNull="false"
              style="width: 100%"
            ></app-select>
          </el-col>
          <el-col
            :span="6"
            prop="LineIndex"
            :label="$t('Line')"
            style="padding-right: 10px"
          >
            <div class="sub-title">
              <span style="color: red">* </span>{{ $t("Line") }}
            </div>
            <app-select
              :dataSource="getListLineFiltered"
              :allowNull="true"
              displayMember="Name"
              valueMember="Index"
              @onChange="lineChanged"
              filterable
              style="width: 100%"
            >
              <el-option value=""></el-option>
            </app-select>
          </el-col>
          <el-col :span="6">
            <el-button
              type="primary"
              @click="updateData"
              style="float: right; margin-top: 20px;"
            >
              {{ $t("Update") }}
            </el-button>
          </el-col>
        </el-row>
        <el-row style="margin-top: 5px; height: calc(100% - 38px)">
          <el-tabs type="border-card" :stretch="true" style="height: 100%">
            <el-tab-pane :label="$t('ListSerialSelect')" style="height: 100%">
              <el-table
                :data="listColumnRation"
                style="width: 100%"
                border
                :height="400"
              >
                <template v-for="column in listColumnRation">
                  <el-table-column
                    :prop="column.dataField"
                    :label="column.name"
                    :key="column.dataField"
                  >
                    <template slot-scope="scope">
                      <span v-if="column.dataField == ''">{{
                        scope.row.mainName
                      }}</span>
                      <el-checkbox
                        v-model="scope.row[column.dataField]"
                        v-else
                      ></el-checkbox>
                    </template>
                  </el-table-column>
                </template>
              </el-table>
            </el-tab-pane>
            <el-tab-pane :label="$t('ControllerList')" style="height: 100%">
              <el-table
                :data="listColumnRation"
                style="width: 100%"
                border
                :height="400"
              >
                <template v-for="column in listColumnRation">
                  <el-table-column
                    :prop="column.dataField"
                    :label="column.name"
                    :key="column.dataField"
                  >
                    <template slot-scope="scope">
                      <span v-if="column.dataField == ''">{{
                        scope.row.mainName
                      }}</span>
                      <el-checkbox
                        v-model="scope.row[column.dataField]"
                        v-else
                      ></el-checkbox>
                    </template>
                  </el-table-column>
                </template>
              </el-table>
            </el-tab-pane>
            <el-tab-pane :label="$t('PrinterList')" style="height: 100%">
              <el-table
                :data="listColumnRation"
                style="width: 100%"
                border
                :height="400"
              >
                <template v-for="column in listColumnRation">
                  <el-table-column
                    :prop="column.dataField"
                    :label="column.name"
                    :key="column.dataField"
                  >
                    <template slot-scope="scope">
                      <span v-if="column.dataField == ''">{{
                        scope.row.mainName
                      }}</span>
                      <el-checkbox
                        v-model="scope.row[column.dataField]"
                        v-else
                      ></el-checkbox>
                    </template>
                  </el-table-column>
                </template>
              </el-table>
            </el-tab-pane>
          </el-tabs>
        </el-row>
      </div>
    </div>

    <div class="thsLayout" style="background: #f6f8fa">
      <h3>Screen type 5</h3>

      <div class="main cm-ration-shift">
        <div class="body-content">
          <el-row style="margin-bottom: 20px" id="filter">
            <el-col :span="4" style="padding-right: 10px">
              <app-select
                :dataSource="listCanteen"
                displayMember="Name"
                valueMember="Index"
                :allowNull="true"
                :placeholder="$t('SelectCanteen')"
                style="width: 100%"
              ></app-select>
            </el-col>
            <el-button class="btn-save" round @click="save">
              {{ $t("Save") }}
            </el-button>
          </el-row>

          <el-table
            :data="listColumnRation"
            style="width: 100%"
            border
            :height="400"
          >
            <template v-for="column in listColumnRation">
              <el-table-column
                :prop="column.dataField"
                :label="column.name"
                :key="column.dataField"
              >
                <template slot-scope="scope">
                  <span v-if="column.dataField == ''">{{
                    scope.row.mainName
                  }}</span>
                  <el-checkbox
                    v-model="scope.row[column.dataField]"
                    v-else
                  ></el-checkbox>
                </template>
              </el-table-column>
            </template>
          </el-table>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import AppFunctionBar from "@/components/app-component/app-function-bar-component/app-function-bar-component.vue";
import AppSelectNew from "@/components/app-component/app-select-new-component/app-select-new-component.vue";

export default {
  created() {},
  name: "DigitalClock",
  props: {},
  components: { AppFunctionBar, AppSelectNew },
  data() {
    return {
      totalTab1 : 0,
      pageTab1 : 1,
      pageSizeTab1 : 50,
      listTitle: [
        { id: 0, title: "Checkbox" },
        { id: 1, title: "Date" },
        { id: 2, title: "Name" },
        { id: 3, title: "Category" },
      ],
      page: 1,
      selectObj: [],
      pagesize: 100,
      total: 100,
      data: [
        "f8fafc",
        "f1f5f9",
        "e2e8f0",
        "cbd5e1",
        "94a3b8",
        "64748b",
        "475569",
        "334155",
        "1e293b",
        "0f172a",
        "f9fafb",
        "f3f4f6",
        "e5e7eb",
        "d1d5db",
        "9ca3af",
        "6b7280",
        "4b5563",
        "374151",
        "1f2937",
        "111827",
        "fafafa",
        "f4f4f5",
        "e4e4e7",
        "d4d4d8",
        "a1a1aa",
        "71717a",
        "52525b",
        "3f3f46",
        "27272a",
        "18181b",
        "fafafa",
        "f5f5f5",
        "e5e5e5",
        "d4d4d4",
        "a3a3a3",
        "737373",
        "525252",
        "404040",
        "262626",
        "171717",
        "fafaf9",
        "f5f5f4",
        "e7e5e4",
        "d6d3d1",
        "a8a29e",
        "78716c",
        "57534e",
        "44403c",
        "292524",
        "1c1917",
        "fef2f2",
        "fee2e2",
        "fecaca",
        "fca5a5",
        "f87171",
        "ef4444",
        "dc2626",
        "b91c1c",
        "991b1b",
        "7f1d1d",
        "fff7ed",
        "ffedd5",
        "fed7aa",
        "fdba74",
        "fb923c",
        "f97316",
        "ea580c",
        "c2410c",
        "9a3412",
        "7c2d12",
        "fffbeb",
        "fef3c7",
        "fde68a",
        "fcd34d",
        "fbbf24",
        "f59e0b",
        "d97706",
        "b45309",
        "92400e",
        "78350f",
        "fefce8",
        "fef9c3",
        "fef08a",
        "fde047",
        "facc15",
        "eab308",
        "ca8a04",
        "a16207",
        "854d0e",
        "713f12",
        "f7fee7",
        "ecfccb",
        "d9f99d",
        "bef264",
        "a3e635",
        "84cc16",
        "65a30d",
        "4d7c0f",
        "3f6212",
        "365314",
        "f0fdf4",
        "dcfce7",
        "bbf7d0",
        "86efac",
        "4ade80",
        "22c55e",
        "16a34a",
        "15803d",
        "166534",
        "14532d",
        "ecfdf5",
        "d1fae5",
        "a7f3d0",
        "6ee7b7",
        "34d399",
        "10b981",
        "059669",
        "047857",
        "065f46",
        "064e3b",
        "f0fdfa",
        "ccfbf1",
        "99f6e4",
        "5eead4",
        "2dd4bf",
        "14b8a6",
        "0d9488",
        "0f766e",
        "115e59",
        "134e4a",
        "ecfeff",
        "cffafe",
        "a5f3fc",
        "67e8f9",
        "22d3ee",
        "06b6d4",
        "0891b2",
        "0e7490",
        "155e75",
        "164e63",
        "f0f9ff",
        "e0f2fe",
        "bae6fd",
        "7dd3fc",
        "38bdf8",
        "0ea5e9",
        "0284c7",
        "0369a1",
        "075985",
        "0c4a6e",
        "eff6ff",
        "dbeafe",
        "bfdbfe",
        "93c5fd",
        "60a5fa",
        "3b82f6",
        "2563eb",
        "1d4ed8",
        "1e40af",
        "1e3a8a",
        "eef2ff",
        "e0e7ff",
        "c7d2fe",
        "a5b4fc",
        "818cf8",
        "6366f1",
        "4f46e5",
        "4338ca",
        "3730a3",
        "312e81",
        "f5f3ff",
        "ede9fe",
        "ddd6fe",
        "c4b5fd",
        "a78bfa",
        "8b5cf6",
        "7c3aed",
        "6d28d9",
        "5b21b6",
        "4c1d95",
        "faf5ff",
        "f3e8ff",
        "e9d5ff",
        "d8b4fe",
        "c084fc",
        "a855f7",
        "9333ea",
        "7e22ce",
        "6b21a8",
        "581c87",
        "fdf4ff",
        "fae8ff",
        "f5d0fe",
        "f0abfc",
        "e879f9",
        "d946ef",
        "c026d3",
        "a21caf",
        "86198f",
        "701a75",
        "fdf2f8",
        "fce7f3",
        "fbcfe8",
        "f9a8d4",
        "f472b6",
        "ec4899",
        "db2777",
        "be185d",
        "9d174d",
        "831843",
        "fff1f2",
        "ffe4e6",
        "fecdd3",
        "fda4af",
        "fb7185",
        "f43f5e",
        "e11d48",
        "be123c",
        "9f1239",
        "881337",
      ],
      listColumnComponent: [
        {
          name: "NameCanteen",
          dataField: "Name",
          Fixed: false,
          //search: true,
          Show: true,
        },
        {
          name: "NameInEng",
          dataField: "NameEnglish",
          Fixed: false,
          //search: true,
          Show: true,
        },
        {
          name: "NameArea",
          dataField: "AreaIndex",
          Fixed: false,
          //search: true,
          Show: true,
          dataType: "lookup",
          lookup: {
            dataSource: this.listAreasLookup,
            displayMember: "Name",
            valueMember: "Index",
          },
        },
        {
          name: "Description",
          dataField: "Description",
          Fixed: false,
          //search: true,
          Show: true,
        },
      ],
      columns: [
        {
          name: "Avatar",
          dataField: "Avatar",
          dataType: "image",
          fixed: true,
          width: 150,
          show: true,
        },
        {
          name: "UserCode",
          dataField: "EmployeeATID",
          fixed: true,
          width: 150,
          show: true,
        },
        {
          name: "FullName",
          dataField: "FullName",
          fixed: true,
          width: 150,
          show: true,
        },
        {
          name: "Parents",
          dataField: "ParentsInfoStr",
          fixed: true,
          width: 150,
          show: true,
          dataType: "viewDetailPopup",
        },
        {
          name: "Gender",
          dataField: "Gender",
          dataType: "lookup",
          fixed: false,
          width: 150,
          show: true,
          lookup: {
            dataSource: {
              1: { Index: 1, Name: "Nam", NameInEng: "Male" },
              0: { Index: 0, Name: "Nữ", NameInEng: "Female" },
            },
            displayMember: "Name",
            valueMember: "Index",
          },
        },
        {
          name: "Class",
          dataField: "ClassID",
          dataType: "lookup",
          fixed: false,
          width: 150,
          show: true,
          lookup: {
            dataSource: this.allClassLookup,
            displayMember: "Name",
            valueMember: "Index",
          },
        },
        {
          name: "Password",
          dataField: "Password",
          fixed: false,
          width: 150,
          show: true,
        },
        {
          name: "CardNumber",
          dataField: "CardNumber",
          fixed: false,
          width: 150,
          show: true,
        },
        {
          name: "NameOnMachine",
          dataField: "NameOnMachine",
          fixed: false,
          width: 150,
          show: true,
        },
      ],
      listTabItem: [
        {
          titleDishOfOffice: "Ca ăn văn phòng buổi trưa",
          titleTabMenu: "",
          checkDishMainMenu: true,
          isShowMessageError: false,
          checkRegisteredDish: false,
          name: "Ca ăn văn phòng buổi sáng",
          index: 1,
          totalPrice: 0,
          listMealmenu: [
            {
              title: "món cố định",
              listItem: [
                {
                  titleServing: "khẩu phần ăn cố định 1",
                  titleDish: "món ăn cố đinh 1",
                  index: 1,
                  indexParent: 1,
                  titleParent: "Ca ăn văn phòng buổi sáng",
                  price: 1000,
                },
                {
                  titleServing: "khẩu phần ăn cố định 2",
                  titleDish: "món ăn cố đinh 2",
                  index: 2,
                  indexParent: 1,
                  titleParent: "Ca ăn văn phòng buổi sáng",
                  price: 2000,
                },
              ],
            },
            {
              title: "món ăn kèm",
              listItem: [
                {
                  titleServing: "khẩu phần ăn cố định 3",
                  titleDish: "món ăn cố đinh 3",
                  index: 1,
                  price: 3000,
                },
              ],
            },
          ],
          //du lieu mon chon
          listItemRegistion: [
            {
              checkedRegistion: false,
              indexItemRegistion: 1,
              isShowNotifi: true,
              ruleCheckMenu: 1,
              ruleChooseDish: 2,
              setDishMenu: "Sét đồ ăn chọn măn 1",
              idSetDishMenu: 333,
              listItem: [
                {
                  isCheck: false,
                  isCheckRule: false,
                  titleItemMeal: "Mặn",
                  titleItemDish: "Cải luôt thịt heo",
                  price: 2000,
                  indexItem: 1,
                  ruleChooseDish: 2,
                },
                {
                  isCheck: false,
                  isCheckRule: false,
                  titleItemMeal: "Chay",
                  titleItemDish: "canh chua chay",
                  price: 3000,
                  indexItem: 2,
                  ruleChooseDish: 2,
                },
                {
                  isCheck: false,
                  isCheckRule: false,
                  titleItemMeal: "Ngọt",
                  titleItemDish: "bánh kem",
                  price: 5000,
                  indexItem: 3,
                  ruleChooseDish: 2,
                },
              ],
            },
            {
              checkedRegistion: false,
              indexItemRegistion: 2,
              ruleCheckMenu: 2,
              ruleChooseDish: 3,
              setDishMenu: "Sét đồ ăn chọn măn 2",
              idSetDishMenu: 444,
              isShowNotifi: false,
              listItem: [
                {
                  isCheck: false,
                  isCheckRule: false,
                  titleItemMeal: "Mặn",
                  titleItemDish: "Cải luôt thịt heo",
                  price: 2000,
                  indexItem: 1,
                  ruleChooseDish: 3,
                },
              ],
            },
          ],
          // du lieu khau phan an
          arrDataMenuServing: [
            {
              titleServing: "khẩu phần ăn cố định 1",
              titleDish: "món ăn cố đinh 1",
              index: 1,
              indexParent: 1,
              titleParent: "Ca ăn văn phòng buổi sáng",
              price: 1000,
              IsMenuSelected: false,
            },
            {
              titleServing: "khẩu phần ăn cố định 2",
              titleDish: "món ăn cố đinh 2",
              index: 2,
              indexParent: 1,
              titleParent: "Ca ăn văn phòng buổi sáng",
              price: 2000,
              IsMenuSelected: false,
            },
          ],
        },
        {
          name: "Ca ăn văn phòng buổi chiều",
          titleTabMenu: "",
          isShowMessageError: false,
          index: 2,
          totalPrice: 0,
          checkDishMainMenu: false,
          checkRegisteredDish: false,
          listMealmenu: [
            {
              title: "món cố định",
              listItem: [
                {
                  titleServing: "khẩu phần ăn cố định 5",
                  titleDish: "món ăn cố đinh 5",
                  index: 1,
                  indexParent: 2,
                  titleParent: "Ca ăn văn phòng buổi chiều",
                  price: 5000,
                },
              ],
            },
          ],
          listItemRegistion: [
            {
              checkedRegistion: false,
              ruleCheckMenu: 2,
              ruleChooseDish: 1,
              setDishMenu: "Sét đồ ăn chọn măn 3",
              idSetDishMenu: 555,
              indexItemRegistion: 2,
              isShowNotifi: false,
              listItem: [
                {
                  isCheck: false,
                  isCheckRule: false,
                  titleItemMeal: "Mặn",
                  titleItemDish: "Cải luôt thịt heo",
                  price: 2000,
                  indexItem: 1,
                  ruleChooseDish: 1,
                },
                {
                  isCheck: false,
                  isCheckRule: false,
                  titleItemMeal: "Chay",
                  titleItemDish: "Rau luộc",
                  price: 1000,
                  indexItem: 2,
                  ruleChooseDish: 1,
                },
              ],
            },
          ],
          arrDataMenuServing: [
            {
              titleServing: "khẩu phần ăn cố định 5",
              titleDish: "món ăn cố đinh 5",
              index: 34,
              price: 4000,
              indexParent: 2,
              titleParent: "Ca ăn văn phòng buổi chiều",
              IsMenuSelected: false,
            },
          ],
        },
      ],
      options: [
        {
          value: "Option1",
          label: "Option1",
        },
        {
          value: "Option2",
          label: "Option2",
        },
        {
          value: "Option3",
          label: "Option3",
        },
        {
          value: "Option4",
          label: "Option4",
        },
        {
          value: "Option5",
          label: "Option5",
        },
      ],
      listColumnRation: [
        {
          index: 0,
          name: "",
          dataField: "",
        },
        {
          index: 4200,
          name: "Ca ăn sáng",
          dataField: "Ca4200",
        },
        {
          index: 4201,
          name: "Ca ăn trưa",
          dataField: "Ca4201",
        },
        {
          index: 4213,
          name: "Ca ăn khuya",
          dataField: "Ca4213",
        },
      ],
      dataTemplate: [
        {
          Index: 5160,
          Name: "Nhà ăn 1",
          CompanyIndex: 2,
          NameEnglish: "",
          Description: "Canteen 1",
          IsDefault: null,
          AreaIndex: 3196,
        },
        {
          Index: 5161,
          Name: "Nhà ăn 2",
          CompanyIndex: 2,
          NameEnglish: "",
          Description: "",
          IsDefault: null,
          AreaIndex: 3196,
        },
        {
          Index: 5164,
          Name: "Nhà ăn văn phòng",
          CompanyIndex: 2,
          NameEnglish: "",
          Description: "",
          IsDefault: null,
          AreaIndex: 3197,
        },
        {
          Index: 5165,
          Name: "DUongtest",
          CompanyIndex: 2,
          NameEnglish: "",
          Description: "",
          IsDefault: null,
          AreaIndex: 3196,
        },
      ],
      valueTempate: true,
      radio1: 1,
      numA: 1,
      numB: 1,
      inputA: "",
      inputB: "",
      value: true,
      RationIndex: "",
      timeValue: "",
      tableData: [
        {
          date: "2016-05-03",
          name: "DUY",
          address: "No. 189, Grove St, Los Angeles",
        },
        {
          date: "2016-05-02",
          name: "THAI",
          address: "No. 189, Grove St, Los Angeles",
        },
        {
          date: "2016-05-04",
          name: "TRAN",
          address: "No. 189, Grove St, Los Angeles",
        },
        {
          date: "2016-05-01",
          name: "HAO",
          address: "No. 189, Grove St, Los Angeles",
        },
      ],
      search: "",
      listColumn: [
        {
          ColumnName: this.$t("GroupName").toString(),
          ID: "Name",
          Width: 210,
          Fixed: true,
          IsHasSearch: true,
          Show: true,
        },
        {
          ColumnName: this.$t("IsAdmin").toString(),
          ID: "IsAdmin",
          Width: 130,
          CheckBox: true,
          Show: true,
        },
        {
          ColumnName: this.$t("Default").toString(),
          ID: "UseForDefault",
          Width: 130,
          CheckBox: true,
          IsHasSearch: false,
          Show: true,
        },
        {
          ColumnName: this.$t("Note").toString(),
          ID: "Note",
          Width: 600,
          IsHasSearch: true,
          Show: true,
        },
      ],
      listData: [
        {
          Index: 5160,
          Name: "Nhà ăn 1",
          CompanyIndex: 2,
          NameEnglish: "",
          Description: "Canteen 1",
          IsDefault: null,
          AreaIndex: 3196,
        },
        {
          Index: 5161,
          Name: "Nhà ăn 2",
          CompanyIndex: 2,
          NameEnglish: "",
          Description: "",
          IsDefault: null,
          AreaIndex: 3196,
        },
        {
          Index: 5164,
          Name: "Nhà ăn văn phòng",
          CompanyIndex: 2,
          NameEnglish: "",
          Description: "",
          IsDefault: null,
          AreaIndex: 3197,
        },
        {
          Index: 5165,
          Name: "DUongtest",
          CompanyIndex: 2,
          NameEnglish: "",
          Description: "",
          IsDefault: null,
          AreaIndex: 3196,
        },
      ],
      listColumnLeftDevice: [
        {
          Name: "AliasName",
          DataField: "AliasName",
          DataType: "",
        },
        {
          Name: "MachineSerial",
          DataField: "SerialNumber",
          DataType: "",
        },
        {
          Name: "AddressIP",
          DataField: "IPAddress",
          DataType: "",
        },
      ],
      listRules: [
        {
          Index: 2092,
          CompanyIndex: 2,
          MinOfTwoCheck: 0,
          StartTimeOfDay: "2022-07-05T06:00:56",
          PercentageOfPayment: 0.5,
          UseSound: false,
          SuccessSound:
            '[{"name":"am-thanh-thanh-cong.mp3","url":"RulesGlobal/RulesGlobal_2092/132929082751257533/"}]',
          FailSound: null,
          MaxTimeForSound: 5,
          Description: "Nhà ăn văn phòng 123",
          PrintBill: true,
          CheckMaxMealCountBy: 1,
          MaxMealCount: 0,
          CheckMaxMealInMonth: false,
          StartDayOfMonth: 1,
          MonthOfCalculating: 0,
          MaxMealInMonth: 1,
          UseGlobalRule: false,
          AppliedDate: "2022-02-16T00:00:00",
          UpdatedDate: "2022-11-09T13:46:02.623",
          UpdatedUser: "admin@giaiphaptinhhoa.com",
          MaxTimeEditCount: 0,
          AllowedEarly: 60,
          AllowedLate: 60,
          IntegrateWorkingSchedule: false,
          CheckEmployeePermission: false,
          AutoOfflineProcess: false,
          UseDaysLock: true,
          DaysToLock: 0,
          DayFirstOFWeek: 1,
          WeeksToLock: 0,
          GiveBackCateringByRation: false,
          GiveBackCateringByTime: false,
          AllowTimeGiveBack: 10,
          CanteenIndexes: [5160, 5161],
          IntegrateShift: false,
          UseRuleEatOfGlobalRule: false,
          RequiredInLog: false,
          RequiredInLogBy: 0,
          MinTimeWorking: "2022-01-01T00:00:00",
          RequiredRegisterBefore: true,
          ActionWhenForgetRegisterBefore: 0,
          DefaultServingsIndex: 6675,
          ListDevice: "A7QT184960230",
          TimeToLock: 10,
          CheckLockBy: 2,
          MaxChange: 0,
          StartTimeOfDayForLock: "2000-02-01T08:00:00",
          StartDayOfMonthForLock: 1,
          SavingType: 0,
          LastSavingType: 2,
          SavingTypeLastChange: "2022-11-09T12:35:48.0242747",
          IsEmployeeShift: false,
          LimitedSnacks: false,
          LimitedSnacksBy: 2,
          LimitedSnacksTime: 1,
          NotificationEmail: true,
          ManagerEmail: true,
          EmailOfManagers: ["nguyenngoctran230@gmail.com"],
          TemplateTitle: "Thông báo thay đổi món ăn",
          TemplateContent:
            "<h2>Xin chào: [name]</h2><p>Nhà ăn [canteen] xin thông báo : Món ăn [oldDish] mà bạn đã đăng ký vào lúc [DD/MM/YYYY] đã được thay đổi thành món ăn [newDish] , vui lòng chọn món ăn hoặc nhóm khẩu phần khác</p><p>Xin chân thành cảm ơn.</p>",
        },
      ],
      editableTabsValueA: "2",
      editableTabsValueB: "2",
      editableTabs: [
        {
          title: "Tab 1",
          name: "1",
          content: "Tab 1 content",
        },
        {
          title: "Tab 2",
          name: "2",
          content: "Tab 2 content",
        },
      ],
      tabIndex: 2,
      fromTime: new Date(2016, 9, 10, 18, 40),
      toTime: new Date(2016, 9, 10, 18, 40),
      selectA: "",
      showEdit: true,
      showDelete: true,
      showMore: true,
    };
  },
  methods: {
    handleTabsEditA(targetName, action) {
      if (action === "add") {
        const newTabName = ++this.tabIndex + "";
        this.editableTabs.push({
          title: "New Tab",
          name: newTabName,
          content: "New Tab content",
        });
        this.editableTabsValue = newTabName;
      }

      if (action === "remove") {
        const tabs = this.editableTabs;
        let activeName = this.editableTabsValue;
        if (activeName === targetName) {
          tabs.forEach((tab, index) => {
            if (tab.name === targetName) {
              const nextTab = tabs[index + 1] || tabs[index - 1];
              if (nextTab) {
                activeName = nextTab.name;
              }
            }
          });
        }

        this.editableTabsValue = activeName;
        this.editableTabs = tabs.filter(tab => tab.name !== targetName);
      }
    },
    handleTabsEditB(targetName, action) {
      if (action === "add") {
        const newTabName = ++this.tabIndex + "";
        this.editableTabs.push({
          title: "New Tab",
          name: newTabName,
          content: "New Tab content",
        });
        this.editableTabsValue = newTabName;
      }

      if (action === "remove") {
        const tabs = this.editableTabs;
        let activeName = this.editableTabsValue;
        if (activeName === targetName) {
          tabs.forEach((tab, index) => {
            if (tab.name === targetName) {
              const nextTab = tabs[index + 1] || tabs[index - 1];
              if (nextTab) {
                activeName = nextTab.name;
              }
            }
          });
        }

        this.editableTabsValue = activeName;
        this.editableTabs = tabs.filter(tab => tab.name !== targetName);
      }
    },
    resetDateFilter() {
      this.$refs.filterTable.clearFilter("date");
    },

    clearFilter() {
      this.$refs.filterTable.clearFilter();
    },

    formatter(row, column) {
      return row.address;
    },

    filterTag(value, row) {
      return row.tag === value;
    },

    filterHandler(value, row, column) {
      const property = column["property"];
      return row[property] === value;
    },

    openAdd() {
      this.$alert(
        `
        <div class="modal">
          <div class="modal-container">
            <form action="/action_page.php">
              <label for="fname">First name:</label>
              <input type="text" id="fname" name="fname"><br><br>
              
              <label for="lname">Last name:</label>
              <input type="text" id="lname" name="lname"><br><br>

            </form>
          </div>
        </div>`,
        "Add",
        {
          dangerouslyUseHTMLString: true,
        }
      );
    },

    openEdit() {
      this.$alert(
        `
        <div class="modal">
          <div class="modal-container">
            <form action="/action_page.php">
              <label for="fname">First name:</label>
              <input type="text" id="fname" value="Nhật" name="fname"><br><br>
              
              <label for="lname">Last name:</label>
              <input type="text" id="lname" value="Minh" name="lname"><br><br>

            </form>
          </div>
        </div>`,
        "Edit",
        {
          dangerouslyUseHTMLString: true,
        }
      );
    },

    openAddEditDialog(action) {
      if (action === "add") {
        this.openAdd();
      }

      if (action === "edit") {
        this.openEdit();
      }
    },
  },

  mounted() {
    document
      .querySelector(".Content")
      .setAttribute("style", "width: 100%; height: 100%");
  },
};
</script>

<style lang="scss">
h3 {
  margin: 0;
  padding: 0;
}

.el-row {
  box-sizing: border-box;
  margin: 20px;
}

.el-collapse-item__header {
  background-color: white;
  color: black;
}

.modal-container {
  display: flex;
  justify-content: center;
  // width: 200px;

  input {
    width: 350px;
  }
}

.select input.el-input__inner {
  border: unset;
}

.container {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 15px;
  width: 100%;
}

.color-item {
  height: 20px;
  border-radius: 20px;
  margin-bottom: 10px;
}

.color-title {
  font-size: 12px;
  font-weight: bold;
}

.color-wrappwer {
  text-align: center;
  font-size: 12px;
  gap: 7px;
}

.demo-input-size {
  width: 55%;
  display: flex;
  gap: 10px;
}

.ths {
  background: white;
  overflow: scroll;
  width: 100%;
  height: 100%;
}

.thsLayout {
  display: flex;
  flex-direction: column;
  gap: 23px;
  padding: 36px;
  border: 1px solid #969faf;
  border-radius: 10px;
  margin: 0 100px 20px 100px;
}

.cm-rules-global {
  .file-item .name {
    width: 100%;
  }
}

.colList {
  height: 200px;
}

.colDetail {
  height: 100%;
}

.el-table__row {
  cursor: pointer;
}

.el-table__body tr.current-row > td {
  background-color: #b5d8ff;
}

.el-collapse-item__header {
  font-weight: bold;
  margin-left: 10px;
  margin-right: 10px;
}

.el-collapse-item {
  margin-bottom: 5px;
  background-color: white;
  border-radius: 10px;
  box-shadow: rgba(0, 0, 0, 0.25) 0px 10px 10px;
}

.el-collapse-item__content {
  padding-bottom: 0px;
  margin-left: 10px;
  margin-right: 10px;
}

.divScrollCollapse {
  height: 100%;
  overflow-y: auto;
  padding-left: 20px;
  padding-bottom: 20px;
  padding-right: 20px;
}

.el-table__body-wrapper {
  height: 400px;
}

.el-form-item__label {
  font-size: 12px;
}

#editor {
  height: 400px;
}
</style>
