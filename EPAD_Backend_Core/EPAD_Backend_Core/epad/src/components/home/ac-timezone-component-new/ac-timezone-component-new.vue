<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ListTimeZone") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome ac-timezone-el-main" style="height: calc(100vh - 200px); overflow-y: hidden;">
        <el-row class="rowdeviceby" :gutter="10" style="height: 100%;">
          <el-col :span="5" style="height: 100%">
            <el-table
              ref="controllerTable"
              :data="tblTimezone"
              highlight-current-row
              @current-change="handleCurrentController"
              style="cursor: pointer; height: 100%; overflow-y: auto; overflow-x: hidden;"
            >
              <el-table-column
                prop="Name"
                :label="$t('ListTimeZone')"
              ></el-table-column>
            </el-table>
          </el-col>
          <el-col :span="19" style="height: 100%;">
            <el-form
              ref="ruleForm"
              :model="ruleForm"
              style="margin-top: 5px; height: 100%; overflow-y: auto; overflow-x: hidden;"
              :rules="rules"
            >
              <el-row>
                <el-col :span="24">
                  <el-col :span="3">
                    <el-form-item>
                      <label class="el-form-item__label required">{{
                        $t("TimezoneName")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="21">
                    <el-form-item prop="Name" @click.native="focus('Name')">
                      <el-input ref="Name" v-model="ruleForm.Name"></el-input>
                    </el-form-item>
                  </el-col>
                </el-col>
                <el-col :span="24">
                  <el-col :span="3">
                    <el-form-item>
                      <label class="el-form-item__label">{{
                        $t("UID")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="21">
                    <el-form-item prop="UIDIndex" @click.native="focus('UIDIndex')">
                      <el-input ref="UIDIndex" v-model="ruleForm.UIDIndex" disabled></el-input>
                    </el-form-item>
                  </el-col>
                </el-col>
                <el-col :span="24">
                  <el-row :gutter="24">
                    <el-col :span="4" :offset="5">
                      <div class="el-form-item el-form-item--small">
                        <label for="MonStart1" class="el-form-item__label" >{{
                          $t("Range1")
                        }}</label>
                      </div>
                    </el-col>
                    <el-col :span="4" :offset="3">
                      <div class="el-form-item el-form-item--small">
                        <label for="MonStart1" class="el-form-item__label">{{
                          $t("Range2")
                        }}</label>
                      </div>
                    </el-col>
                    <el-col :span="4" :offset="3">
                      <div class="el-form-item el-form-item--small">
                        <label for="MonStart1" class="el-form-item__label">{{
                          $t("Range3")
                        }}</label>
                      </div>
                    </el-col>
                  </el-row>

                  <el-row :gutter="24">
                    <el-col :span="5">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("UIDByRange")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="4">
                      <div class="el-form-item el-form-item--small">
                        <label
                          for="MonStart1"
                          class="el-form-item__label"
                          style="font-weight: 200; margin-left: 20px;"
                          >{{ ruleForm.UIDIndex }}</label
                        >
                      </div>
                    </el-col>
                    <el-col :span="4" :offset="3">
                      <div class="el-form-item el-form-item--small">
                        <label
                          for="MonStart1"
                          class="el-form-item__label"
                          style="font-weight: 200; margin-left: 20px;"
                          >{{ ruleForm.UIDIndex2 }}</label
                        >
                      </div>
                    </el-col>
                    <el-col :span="4" :offset="3">
                      <div class="el-form-item el-form-item--small">
                        <label
                          for="MonStart1"
                          class="el-form-item__label"
                          style="font-weight: 200; margin-left: 20px;"
                          >{{ ruleForm.UIDIndex3 }}</label
                        >
                      </div>
                    </el-col>
                  </el-row>
                  <el-row :gutter="24">
                    <el-col :span="10">
                      <div class="el-form-item el-form-item--small">
                        <label
                          class="el-form-item__label"
                          style="font-weight: 200"
                          >(Áp dụng cho máy có timezone ứng với 1 khoảng thời
                          gian)</label
                        >
                      </div>
                    </el-col>
                  </el-row>
                </el-col>
                <el-col :span="24">
                  <el-row :gutter="24">
                    <el-col :span="3">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("Monday")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="21">
                      <el-row :gutter="24">
                        <el-col :span="4">
                          <el-form-item
                            prop="MonStart1"
                            @click.native="focus('MonStart1')"
                          >
                            <el-select
                              v-model="ruleForm.MonStart1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="MonStart1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('MonEnd1')">
                            <el-select
                              v-model="ruleForm.MonEnd1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="MonEnd1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('MonStart2')">
                            <el-select
                              v-model="ruleForm.MonStart2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="MonStart2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('MonEnd2')">
                            <el-select
                              v-model="ruleForm.MonEnd2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="MonEnd2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('MonStart3')">
                            <el-select
                              v-model="ruleForm.MonStart3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="MonStart3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('MonEnd3')">
                            <el-select
                              v-model="ruleForm.MonEnd3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="MonEnd3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                  <el-row :gutter="24" style="padding-bottom: 10px">
                    <el-col :span="2" :offset="17">
                      <el-button type="primary" @click="copyAllProperties">
                        {{ $t("ApplyTimezoneForAllDayOfWeek") }}
                      </el-button>
                    </el-col>
                  </el-row>
                  <el-row :gutter="24">
                    <el-col :span="3">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("Tuesday")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="21">
                      <el-row :gutter="24">
                        <el-col :span="4">
                          <el-form-item
                            prop="TuesStart1"
                            @click.native="focus('TuesStart1')"
                          >
                            <el-select
                              v-model="ruleForm.TuesStart1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="TuesStart1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('TuesEnd1')">
                            <el-select
                              v-model="ruleForm.TuesEnd1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="TuesEnd1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('TuesStart2')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.TuesStart2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="TuesStart2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('TuesEnd2')">
                            <el-select
                              v-model="ruleForm.TuesEnd2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="TuesEnd2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('TuesStart3')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.TuesStart3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="TuesStart3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('TuesEnd3')">
                            <el-select
                              v-model="ruleForm.TuesEnd3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="TuesEnd3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                  <el-row :gutter="24">
                    <el-col :span="3">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("Wednesday")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="21">
                      <el-row :gutter="24">
                        <el-col :span="4">
                          <el-form-item
                            :label="$t('')"
                            prop="WedStart1"
                            @click.native="focus('WedStart')"
                          >
                            <el-select
                              v-model="ruleForm.WedStart1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="WedStart1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('WedEnd1')">
                            <el-select
                              v-model="ruleForm.WedEnd1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="WedEnd1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('WedStart2')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.WedStart2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="WedStart2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('WedEnd2')">
                            <el-select
                              v-model="ruleForm.WedEnd2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="WedEnd2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('WedStart3')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.WedStart3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="WedStart3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('WedEnd3')">
                            <el-select
                              v-model="ruleForm.WedEnd3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="WedEnd3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                  <el-row :gutter="24">
                    <el-col :span="3">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("Thursday")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="21">
                      <el-row :gutter="24">
                        <el-col :span="4">
                          <el-form-item
                            :label="$t('')"
                            prop="ThursStart1"
                            @click.native="focus('ThursStart1')"
                          >
                            <el-select
                              v-model="ruleForm.ThursStart1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="ThursStart1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('ThursEnd1')">
                            <el-select
                              v-model="ruleForm.ThursEnd1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="ThursEnd1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('ThursStart2')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.ThursStart2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="ThursStart2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('ThursEnd2')">
                            <el-select
                              v-model="ruleForm.ThursEnd2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="ThursEnd2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('ThursStart3')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.ThursStart3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="ThursStart3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('ThursEnd3')">
                            <el-select
                              v-model="ruleForm.ThursEnd3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="ThursEnd3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                  <el-row :gutter="24">
                    <el-col :span="3">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("Friday")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="21">
                      <el-row :gutter="24">
                        <el-col :span="4">
                          <el-form-item
                            :label="$t('')"
                            prop="FriStart1"
                            @click.native="focus('FriStart1')"
                          >
                            <el-select
                              v-model="ruleForm.FriStart1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="FriStart1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('FriEnd1')">
                            <el-select
                              v-model="ruleForm.FriEnd1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="FriEnd1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('FriStart2')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.FriStart2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="FriStart2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('FriEnd2')">
                            <el-select
                              v-model="ruleForm.FriEnd2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="FriEnd2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('FriStart3')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.FriStart3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="FriStart3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('FriEnd3')">
                            <el-select
                              v-model="ruleForm.FriEnd3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="FriEnd3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                  <el-row :gutter="24">
                    <el-col :span="3">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("Saturday")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="21">
                      <el-row :gutter="24">
                        <el-col :span="4">
                          <el-form-item
                            :label="$t('')"
                            prop="SatStart1"
                            @click.native="focus('SatStart1')"
                          >
                            <el-select
                              v-model="ruleForm.SatStart1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SatStart1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('SatEnd1')">
                            <el-select
                              v-model="ruleForm.SatEnd1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SatEnd1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('SatStart2')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.SatStart2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SatStart2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('SatEnd2')">
                            <el-select
                              v-model="ruleForm.SatEnd2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SatEnd2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('SatStart3')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.SatStart3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SatStart3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('SatEnd3')">
                            <el-select
                              v-model="ruleForm.SatEnd3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SatEnd3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                  <el-row :gutter="24">
                    <el-col :span="3">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("Sunday")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="21">
                      <el-row :gutter="24">
                        <el-col :span="4">
                          <el-form-item
                            :label="$t('')"
                            prop="SunStart1"
                            @click.native="focus('SunStart1')"
                          >
                            <el-select
                              v-model="ruleForm.SunStart1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SunStart1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('SunEnd1')">
                            <el-select
                              v-model="ruleForm.SunEnd1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SunEnd1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('SunStart2')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.SunStart2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SunStart2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('SunEnd2')">
                            <el-select
                              v-model="ruleForm.SunEnd2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SunEnd2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('SunStart3')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.SunStart3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SunStart3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('SunEnd3')">
                            <el-select
                              v-model="ruleForm.SunEnd3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="SunEnd3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                  <el-row :gutter="24">
                    <el-col :span="3">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("Holiday1")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="21">
                      <el-row :gutter="24">
                        <el-col :span="4">
                          <el-form-item
                            :label="$t('')"
                            prop="Holiday1Start1"
                            @click.native="focus('Holiday1Start1')"
                          >
                            <el-select
                              v-model="ruleForm.Holiday1Start1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday1Start1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('Holiday1End1')">
                            <el-select
                              v-model="ruleForm.Holiday1End1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday1End1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('Holiday1Start2')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.Holiday1Start2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday1Start2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('Holiday1End2')">
                            <el-select
                              v-model="ruleForm.Holiday1End2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday1End2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('Holiday1Start3')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.Holiday1Start3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday1Start3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('Holiday1End3')">
                            <el-select
                              v-model="ruleForm.Holiday1End3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday1End3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                  <el-row :gutter="24">
                    <el-col :span="3">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("Holiday2")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="21">
                      <el-row :gutter="24">
                        <el-col :span="4">
                          <el-form-item
                            :label="$t('')"
                            prop="Holiday2Start1"
                            @click.native="focus('Holiday1Start1')"
                          >
                            <el-select
                              v-model="ruleForm.Holiday2Start1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday2Start1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('Holiday2End1')">
                            <el-select
                              v-model="ruleForm.Holiday2End1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday2End1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('Holiday2Start2')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.Holiday2Start2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday2Start2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('Holiday2End2')">
                            <el-select
                              v-model="ruleForm.Holiday2End2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday2End2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('Holiday2Start3')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.Holiday2Start3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday2Start3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('Holiday2End3')">
                            <el-select
                              v-model="ruleForm.Holiday2End3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday2End3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>

                  <el-row :gutter="24">
                    <el-col :span="3">
                      <el-form-item>
                        <label class="el-form-item__label">{{
                          $t("Holiday3")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="21">
                      <el-row :gutter="24">
                        <el-col :span="4">
                          <el-form-item
                            :label="$t('')"
                            prop="Holiday3Start1"
                            @click.native="focus('Holiday1Start1')"
                          >
                            <el-select
                              v-model="ruleForm.Holiday3Start1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday3Start1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('Holiday3End1')">
                            <el-select
                              v-model="ruleForm.Holiday3End1"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday3End1"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('Holiday3Start2')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.Holiday3Start2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday3Start2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('Holiday3End2')">
                            <el-select
                              v-model="ruleForm.Holiday3End2"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday3End2"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item
                            @click.native="focus('Holiday3Start3')"
                            :label="$t('')"
                          >
                            <el-select
                              v-model="ruleForm.Holiday3Start3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday3Start3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                        <el-col :span="4">
                          <el-form-item @click.native="focus('Holiday3End3')">
                            <el-select
                              v-model="ruleForm.Holiday3End3"
                              filterable
                              clearable
                              allow-create
                              class="w-100"
                              ref="Holiday3End3"
                            >
                              <el-option
                                v-for="item in timePosOption"
                                :key="item"
                                :label="item"
                                :value="item"
                              ></el-option>
                            </el-select>
                          </el-form-item>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                </el-col>
                <el-col :span="24">
                  <el-form-item
                    prop="IntegrateTimezone"
                    @click.native="focus('IntegrateTimezone')"
                    v-if="!isEdit"
                  >
                    <el-checkbox v-model="IntegrateTimezone">{{
                      $t("IntegrateTimezone")
                    }}</el-checkbox>
                  </el-form-item>
                </el-col>
                <el-col :span="24">
                  <el-col :span="3">
                    <el-form-item>
                      <label class="el-form-item__label">{{
                        $t("Description")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="21">
                    <el-form-item
                      prop="Description"
                      @click.native="focus('Description')"
                    >
                      <el-input
                        ref="Description"
                        type="textarea"
                        :rows="1"
                        v-model="ruleForm.Description"
                      ></el-input>
                    </el-form-item>
                  </el-col>
                </el-col>
              </el-row>
            </el-form>
          </el-col>
        </el-row>
      </el-main>
      <el-row style="margin-left: 10px;">
        <el-col :span="2" align="left">
          <el-button style="float: left" type="primary" @click="Integrate()">
            {{ $t("SyncAll") }}
          </el-button>
        </el-col>
        <el-col :span="3" align="right" style="margin-bottom: 10px">
          <el-button
            type="primary"
            @click="Delete"
            style="margin-right: 5px; background-color: red !important"
          >
            <i class="el-icon-delete"></i> {{ $t("Delete") }}
          </el-button>
          <el-button type="primary" @click="addTimezoneClick">
            <i class="el-icon-plus"></i> {{ $t("Add") }}
          </el-button>
        </el-col>
        <el-col :span="19" align="right">
          <el-button
            type="primary"
            @click="Submit"
            style="background-color: #67c23a !important; margin-right: 10px"
          >
            <i class="el-icon-circle-check"></i> {{ $t("Save") }}</el-button
          >
        </el-col>
      </el-row>
    </el-container>
  </div>
</template>
<script src="./ac-timezone-component-new.ts"></script>

<style lang="scss">
.ac-timezone-el-main {
  .customdialog {
    width: 1200px;
  }
}

.required:after {
  content: "*";
  color: red;
}
</style>
