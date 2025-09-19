<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("Reports") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent :showMasterEmployeeFilter="true"/>
          </el-col>
        </el-row>
      </el-header>

      <el-main class="bgHome">
        <el-form
          id="form"
          :action="getReportLink"
          :inline="true"
          :model="optionReport"
          :validate-on-rule-change="false"
          method="POST"
          ref="param-form"
          target="main-report"
          validate
        >
        <el-form-item :label="$t('SelectTypeReport')">
            <el-select v-model="typeReport" @change="selectTypeReport"
            class="reportList">
              <el-option
                v-for="(item, index) in listTypeReport"
                :key="`report_${index}`"
                :label="item.label"
                :value="item.Name"
                :title="item.label"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item :label="$t('SelectReport')">
            <el-select v-model="selectedReport" style="width: 400px"  @change="_handleReportChange"
            class="reportList">
              <el-option
                v-for="(item, index) in listReport"
                :key="`report_${index}`"
                :label="item.label"
                :value="item.value"
                :title="item.label"
              ></el-option>
            </el-select>
          </el-form-item>

          <template v-if="hasDepartmentSelect">
            <el-dropdown
              size="small"
              trigger="click"
              @visible-change="_selectDepartmentVisibleChange"
              class="item-filter"
              key="drdDepartment"
            >
              <el-button>
                {{ $t("SelectDepartment")
                }}<i class="el-icon-arrow-down el-icon--right"></i>
              </el-button>
              <el-dropdown-menu slot="dropdown" class="dropdown-report">
                <department-tree
                  :showEmployee="false"
                  ref="treeDepartment"
                ></department-tree>
              </el-dropdown-menu>
            </el-dropdown>
          </template>

          <template v-if="hasEmployeeSelect">
            <el-dropdown
              size="small"
              trigger="click"
              @visible-change="_selectEmployeeVisibleChange"
              class="item-filter report-employee-filter"
              key="drdEmployee"
            >
              <el-button type="primary">
                {{ $t("SelectEmployee")
                }}<i class="el-icon-arrow-down el-icon--right"></i>
              </el-button>
              <el-dropdown-menu slot="dropdown" class="dropdown-report">
                <department-tree
                  :showEmployee="true"
                  ref="treeEmployee"
                ></department-tree>
              </el-dropdown-menu>
            </el-dropdown>
          </template>

          <template v-for="(item, index) in reportParamsValue">
            <template
              v-if="reportParams[index].Name === 'SpecialParamEmployeeSelected'"
            >
              <span :key="`item_${index}`" style="display: none">
                <input
                  :key="i"
                  name="SpecialParamEmployeeSelected"
                  :value="value"
                  type="hidden"
                  v-for="(value, i) in listEmployeeSelected"
                />
              </span>
            </template>

            <template v-else-if="reportParams[index].Name === 'Department'">
              <span :key="`item_${index}`" style="display: none">
                <input
                  :key="i"
                  name="Department"
                  :value="value"
                  type="hidden"
                  v-for="(value, i) in listDepartmentSelected"
                />
              </span>
            </template>

            <template
              v-else-if="
                reportParams[index].Name.toLowerCase().indexOf('date') > -1
              "
            >
              <el-form-item
                :key="`item_${index}`"
                :label="reportParams[index].Prompt"
              >
                <el-date-picker
                  style="width: 180px"
                  v-model="reportParamsValue[index]"
                  type="date"
                  placeholder
                >
                </el-date-picker>
                <input
                  :name="reportParams[index].Name"
                  :value="getDateTimeFor(reportParamsValue[index])"
                  type="hidden"
                />
              </el-form-item>
            </template>

            <template
              v-else-if="
                reportParams[index].Name.toLowerCase().indexOf('month') > -1
              "
            >
              <el-form-item
                :key="`item_${index}`"
                :label="reportParams[index].Prompt"
              >
                <el-date-picker
                  style="width: 180px"
                  v-model="reportParamsValue[index]"
                  type="month"
                  placeholder
                >
                </el-date-picker>
                <input
                  :name="reportParams[index].Name"
                  :value="getDateTimeFor(reportParamsValue[index])"
                  type="hidden"
                />
              </el-form-item>
            </template>

            <template
              v-else-if="
                reportParams[index].Name.toLowerCase().indexOf('year') > -1
              "
            >
              <el-form-item
                :key="`item_${index}`"
                :label="reportParams[index].Prompt"
              >
                <el-date-picker
                  style="width: 180px"
                  v-model="reportParamsValue[index]"
                  type="year"
                  placeholder
                >
                </el-date-picker>
                <input
                  :name="reportParams[index].Name"
                  :value="getDateTimeFor(reportParamsValue[index])"
                  type="hidden"
                />
              </el-form-item>
            </template>

            <template v-else>
              <el-form-item
                :key="`item_${index}`"
                v-if="reportParams[index].ValidValues.length > 0"
              >
                <el-select
                  :multiple="reportParams[index].MultiValue"
                  :placeholder="reportParams[index].Prompt"
                  collapse-tags
                  v-model="reportParamsValue[index]"
                  :clearable="true"
                  @change="changeLocation"
                  v-if="clientName == 'PSV' && reportParams[index].Name === 'SpecialParamLocationSelected'"
                  
                >
                  <el-button
                    @click="selectAll(index)"
                    class="w-100"
                    size="mini"
                    >{{ $t("SelectAll") }}</el-button
                  >
                  <el-option
                    v-for="(item, ixMulti) in reportParams[index].ValidValues"
                    :key="`param_${ixMulti}`"
                    :label="item.Label"
                    :value="item.Value"
                  >
                  </el-option>
                </el-select>
                <el-select
                  :multiple="reportParams[index].MultiValue"
                  :placeholder="reportParams[index].Prompt"
                  collapse-tags
                  v-model="reportParamsValue[index]"
                  :clearable="true"
                  v-else-if="clientName == 'PSV' && reportParams[index].Name === 'SpecialParamDepartmentNoteSelected'"
                  
                >
                  <el-button
                    @click="selectAllNote(index)"
                   
                    class="w-100"
                    size="mini"
                    >{{ $t("SelectAll") }}</el-button
                  >
                  <el-option
                    v-for="(item, ixMulti) in listNote"
                    :key="`param_${ixMulti}`"
                    :label="item.Note"
                    :value="item.Note"
                  >
                  </el-option>
                </el-select>

                
                <el-select
                  :multiple="reportParams[index].MultiValue"
                  :placeholder="reportParams[index].Prompt"
                  collapse-tags
                  v-model="reportParamsValue[index]"
                  :clearable="true"
                  v-else
                >
                
                  <el-button
                    @click="selectAll(index)"
                     v-if="reportParams[index].MultiValue"
                    class="w-100"
                    size="mini"
                    >{{ $t("SelectAll") }}</el-button
                  >
                  <el-option
                    v-for="(item, ixMulti) in reportParams[index].ValidValues"
                    :key="`param_${ixMulti}`"
                    :label="item.Label"
                    :value="item.Value"
                  >
                  </el-option>
                </el-select>
                <template v-if="reportParams[index].MultiValue === true">
                  <template v-for="(val, i) in reportParamsValue[index]">
                    <input
                      v-if="val != null && val != undefined && val != ''"
                      :key="`val_${i}`"
                      :name="reportParams[index].Name"
                      :value="val"
                      type="hidden"
                    />
                  </template>
                </template>
                <template v-else>
                  <input
                    :name="reportParams[index].Name"
                    :value="reportParamsValue[index]"
                    type="hidden"
                  />
                </template>
              </el-form-item>

              <el-form-item :key="index" v-else>
                <el-input
                  :placeholder="reportParams[index].Prompt"
                  :name="reportParams[index].Name"
                  v-model="reportParamsValue[index]"
                ></el-input>
              </el-form-item>
            </template>
          </template>

          <input
            :key="`viewOption_${i}`"
            :name="k"
            :value="v"
            type="hidden"
            v-for="(v, k, i) in reportHost.ViewOption"
          />

          <el-form-item>
            <el-button
              type="primary"
              size="small"
              class="smallbutton"
              @click="submit"
              >{{ $t("View") }}</el-button
            >
          </el-form-item>
        </el-form>
        <div class="body-report" v-loading="loadingParam">
          <iframe
            style="
              width: 100%;
              height: 100%;
              background-color: #fff;
              color: black;
            "
            frameborder="0"
            id="mainReport"
            name="main-report"
          ></iframe>
        </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./report.ts"></script>

<style scoped>
.bgHome {
  padding: 5px 12px 0 12px;
}
.el-button--primary {
  font-weight: 600;
}
.group-btn {
  width: fit-content;
  display: inline-block;
}
.group-btn button:last-child {
  margin-left: 10px;
}
.group-btn button {
  float: left;
}

.body-report {
  width: 100%;
  height: calc(100% - 120px);
  margin-top: 5px;
}

.el-form-item--mini.el-form-item,
.el-form-item--small.el-form-item {
  margin-bottom: 8px;
}

.item-filter {
  margin: 0 10px;
}
.dropdown-report {
  top: 100px;
  height: 500px;
  overflow-y: scroll;
}

/* .el-select-dropdown__item {
  display: block;
  word-wrap: break-word;
  word-break: break-all;
  white-space: pre-line;
  line-height: unset;
  overflow-wrap: break-word;
  height: calc(40px);
  span {
    display: block;
    word-wrap: break-word;
    word-break: break-all;
    white-space: pre-line;
    line-height: unset;
    overflow-wrap: break-word;
  }
} */
</style>
