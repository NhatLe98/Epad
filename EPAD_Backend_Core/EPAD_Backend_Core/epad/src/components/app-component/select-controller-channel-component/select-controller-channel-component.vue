<template>
  <div>
    <el-table :data="listData" style="width: 100%; margin-bottom: 20px">
      
      <el-table-column
        v-if="useLine"
        prop="LineIndexs"
        :label="$t('LineAndGate')"
        width="150"
      >
        <template slot-scope="scope">
          <el-select
            v-model="scope.row.LineIndex"
            :placeholder="$t('SelectLineOrGate')"
            @change="loadDeviceByLine(scope.row)"
          >
            <!-- @change="loadLines" -->
            <el-option
              v-for="item in listLines"
              :key="GetKey('Line', useLine, type, item.Index, scope.row.Index)"
              :label="item.Name"
              :value="item.Index"
            >
            </el-option>
              <!-- :disabled="checkLineDisabled(item.Index)" -->
          </el-select>
        </template>
      </el-table-column>
      <el-table-column
        v-if="useLine"
        prop="DeviceIndex"
        :label="$t('Device')"
        width="150"
      >
        <template slot-scope="scope">
          <el-select
            v-model="scope.row.SerialNumber"
            :placeholder="$t('SelectDevice')"
            @change="updateModel"
            :disabled="scope.row.LineIndex == null"
          >
            <!-- @change="loadDevices" -->
            <el-option
              v-for="item in scope.row.ListSerialNumber"
              :key="GetKey('Device', useLine, type, item.SerialNumber, scope.row.Index)"
              :label="item.Name"
              :value="item.SerialNumber"
              :disabled="checkControllerDisabled(scope.row, item.SerialNumber)"
            >
            
              <!-- :disabled="checkControllerDisabled(scope.row, item.SerialNumber)" -->
            </el-option>
          </el-select>
        </template>
      </el-table-column>
      <el-table-column
        prop="ControllerIndex"
        :label="$t('RemoteControll')"
        :width="useLine ? 180 : 230"
      >
        <template slot-scope="scope">
          <el-select
            v-model="scope.row.ControllerIndex"
            :placeholder="$t('RemoteControll')"
            style="width: 100%"
            @change="loadChannel(scope.row)"
          >
            <!-- <slot v-bind="listControllerSelect"> -->
            <el-option
              :key="GetKey('Controller', useLine, type, item.Index, scope.row.Index)"
              :label="`${item.Name}`"
              :value="item.Index"
              v-for="item in listControllerSelect"
            ></el-option>
            <!-- </slot> -->
          </el-select>
        </template>
      </el-table-column>
      <el-table-column
        prop="ChannelIndexs"
        :label="$t('RemoteChannel')"
        :width="useLine ? 140 : 210"
      >
        <template slot-scope="scope">
          <el-select
            v-model="scope.row.ChannelIndex"
            :placeholder="$t('RemoteChannel')"
            style="width: 100%"
            :disabled="scope.row.ControllerIndex == null"
            @change="updateModel"
          >
            <!-- <slot v-bind="loadChannel(scope.row)"> -->
            <el-option
              :key="GetKey('Line', useLine, type, item.Index, scope.row.Index)"
              :label="`${$t('SignalingLights')} ${index + 1}`"
              :value="item.Index"
              v-for="(item, index) in scope.row.ListChannel"
            :disabled="checkChannelDisabled(scope.row, item.Index)"
              
            ></el-option>
            <!-- :disabled="checkChannelDisabled(scope.row.ControllerIndex, scope.row.LineIndex, item.Index)" -->
            <!-- </slot> -->
          </el-select>
        </template>
      </el-table-column>
      <el-table-column prop="Function" label="" width="100">
        <template slot-scope="scope">
          <el-button
            type="warning"
            icon="el-icon-close"
            size="mini"
            @click="deleteRow(scope.$index, scope.row)"
            v-if="listData.length > 1"
            circle
          ></el-button>

          <el-button
            type="primary"
            icon="el-icon-plus"
            size="mini"
            @click="addRow(scope.$index, scope.row)"
            v-if="scope.$index == 0"
            circle
            style="height: 28px !important; margin-left: 5px; margin-top: 1px;"
          ></el-button>
        </template>
      </el-table-column>
      <el-table-column prop="Error">
        <template slot-scope="scope">
          <span class="text-danger">{{ scope.row.Error }} </span>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script src="./select-controller-channel-component.ts" />

<style lang="scss" scoped>
</style>
