<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("RelayController") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome relay-controller-el-main">
        <el-row class="rowdeviceby" :gutter="10" style="height:90%;">
          <el-col :span="8" style="height:100%;">
            <el-table
             ref="controllerTable"
             :data="tblController"
              highlight-current-row
              @current-change="handleCurrentController"
              style="cursor:pointer;height:100%;"
            >
              <el-table-column
                prop="Name"
                :label="$t('ListRelayController')"
              ></el-table-column>
            </el-table>
            <el-button style="float:left;" type="primary"  @click="updateControllerClick()">
              {{ $t("Update") }}
            </el-button>
            <el-button style="float:right;" type="primary" icon="el-icon-plus" @click="addControllerClick()">
              {{ $t("Add") }}
            </el-button>
            <el-button
              style="float:right;margin-right:10px;"
              type="primary"
              icon="el-icon-delete"
              @click="removeControllerClick()">
              {{ $t("Delete") }}
            </el-button>
          </el-col>
          <el-col :span="16" style="height:100%;">
            <el-form
              ref="form"
              :model="controllerData"
              style="margin-top:5px;height:100%;"
              label-width="120px"
            >
              <el-row style="height:40%;">
                  <el-col :span="12">
                      <span>{{ $t("ControllerInfo") }}</span>
                      <el-form-item :label="$t('MachineName')">
                          <el-input v-model="controllerData.Name"></el-input>
                      </el-form-item>
                      <el-form-item :label="$t('AddressIP')">
                          <el-input v-model="controllerData.IPAddress"></el-input>
                      </el-form-item>
                      <el-form-item :label="$t('Port')">
                          <el-input v-model="controllerData.Port"></el-input>
                      </el-form-item>
                      <el-form-item :label="$t('RelayType')">
                          <el-select v-model="controllerData.RelayType"
                                     filterable
                                     default-first-option
                                     style="width: 100%;"
                                     :placeholder="$t('SelectRelayType')">
                              <el-option key="1"
                                         label="Modbus TCP/IP"
                                         value="ModbusTCP"></el-option>
                              <el-option key="2"
                                         label="Client TCP/IP"
                                         value="ClientTCP"></el-option>
                          </el-select>
                      </el-form-item>
                  </el-col>
                <el-col :span="12" style="margin-top:20px;">
                 
                       <el-form-item :label="$t('Signal')">
                          <el-select v-model="controllerData.SignalType"
                                     filterable
                                     default-first-option
                                      @change="onChangeSingalType($event)"
                                      style="width: 100%;"
                                     >
                                      <el-option v-for="tag in relayTypeLst" :key="tag.value" :value="tag.value" :label="tag.name">{{tag.name}}</el-option>
                                
                          </el-select>
                      </el-form-item>
                       <el-form-item :label="$t('Note')">
                    <el-input
                      type="textarea"
                      v-model="controllerData.Description"
                    ></el-input>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row style="height:60%;">
                <el-col :span="12" style="height:100%;">
                  <el-table
                    :data="controllerData.ListChannel.filter(x => x.SignalType == 0)"
                    highlight-current-row
                    @current-change="handleCurrentChannel"
                    style="cursor:pointer; height: calc(100% - 5px);">
                    <el-table-column
                       :label="controllerData.SignalType != 2 ? $t('ChannelList') : $t('OutputChannelList')">
                        <template slot-scope="scope">
                            <div style="width:100%">
                                <span >
                                    Channel {{ scope.row.Index }}
                                </span>
                                <el-switch :key="scope.row.Index" style="float:right" @change="changeRelayStatus(scope.row.Index)" v-model="scope.row.ChannelStatus">
                                </el-switch>
                            </div>
                        </template>
                    </el-table-column>
                  </el-table>
                  <el-button
                    style="float:right;"
                    type="primary"
                    icon="el-icon-plus" @click="addChannelClick(0)">
                    {{ $t("Add") }}
                  </el-button>
                  <el-button
                    style="float:right;margin-right:10px;"
                    type="primary"
                    icon="el-icon-delete"
                    @click="deleteChannelClick()"
                  >
                    {{ $t("Delete") }}
                  </el-button>
                </el-col>

                <el-col :span="12">
                  <span>{{ $t("ChannelInfo") }}</span>
                  <el-form-item
                    style="margin-top:5px;"
                    :label="$t('OrdinalNumber')"
                  >
                    <el-input-number
                      v-model="controllerData.ChannelIndex"
                      :disabled="true"
                    ></el-input-number>
                  </el-form-item>
                  <el-form-item style="margin-top:5px;" label="Số giây tắt">
                    <el-input-number
                      v-model="controllerData.ChannelSeconds"
                      :min="0"
                    ></el-input-number>
                  </el-form-item>
                </el-col>
              </el-row>
                <el-row style="height:60%;margin-top: 50px;" v-if="isInOutPut">
                <el-col :span="12" style="height:100%;">
                  <el-table
                    :data="controllerData.ListChannel.filter(x => x.SignalType != 0)"
                    highlight-current-row
                    height="331"
                    @current-change="handleCurrentChannel"
                    style="cursor:pointer;height:100%;">
                    <el-table-column
                       :label="controllerData.SignalType != 2 ? $t('ChannelList') : $t('InputChannelList')">
                        <template slot-scope="scope">
                            <div style="width:100%">
                                <span >
                                    Channel {{ scope.row.Index }}
                                </span>
                                <el-switch :key="scope.row.Index" style="float:right" @change="changeRelayStatus(scope.row.Index)" v-model="scope.row.ChannelStatus">
                                </el-switch>
                            </div>
                        </template>
                      </el-table-column>
                  </el-table>
                  <el-button
                    style="float:right;"
                    type="primary"
                    icon="el-icon-plus" @click="addChannelClick(1)">
                    {{ $t("Add") }}
                  </el-button>
                  <el-button
                    style="float:right;margin-right:10px;"
                    type="primary"
                    icon="el-icon-delete"
                    @click="deleteChannelClick()"
                  >
                    {{ $t("Delete") }}
                  </el-button>
                </el-col>

                <el-col :span="12">
                  <span>{{ $t("ChannelInfo") }}</span>
                  <el-form-item
                    style="margin-top:5px;"
                    :label="$t('OrdinalNumber')"
                  >
                    <el-input-number
                      v-model="controllerData.ChannelIndex"
                      :disabled="true"
                    ></el-input-number>
                  </el-form-item>
                  <el-form-item style="margin-top:5px;" label="Số giây tắt">
                    <el-input-number
                      v-model="controllerData.ChannelSeconds"
                      :min="0"
                    ></el-input-number>
                  </el-form-item>
                </el-col>
              </el-row>
            </el-form>
          </el-col>
        </el-row>
        <el-row>

        </el-row>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./relay-controller.ts"></script>
<style lang="scss">
.bgHome {
  padding: 5px 12px 0 12px;
}

.relay-controller-el-main {
  .el-button--primary > span {
    display: unset !important;
  }
  .el-form-item__label {
    font-weight: normal;
  }
  .el-table{
      margin-top:0px;
  } 
}
</style>
