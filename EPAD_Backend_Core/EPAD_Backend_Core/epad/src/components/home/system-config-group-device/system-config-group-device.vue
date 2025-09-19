<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("SystemConfigGroupDevice") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>

      <el-main class="bgHome config" style="padding-bottom:12px">
        <el-form :inline="true">
          <el-form-item :label="$t('GroupDeviceName')">
            <el-select
              ref="DepartmentTransfer"
              v-model="ruleForm.Index"
              @change="comboGroupDeviceChange()"
            >
              <el-option
                v-for="(item, index) in comboGroupDevice"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-form>

        <template v-if="!isLoading">
          <template v-for="cfg in getListConfig">
            <ConfigComponent
              :isHideMachineSerialSelect="cfg.EventType == 'DOWNLOAD_USER'"
              :key="cfg.EventType"
              :timePosOption="timePosOption"
              :emailOption="emailOption"
              :configModel.sync="configCollection[cfg.EventType]"
            ></ConfigComponent>
          </template>
        </template>

        <el-row>
          <el-col :span="24" class="left">
            <el-button type="primary" @click="SaveConfig">{{ $t("Save") }}</el-button>
          </el-col>
        </el-row>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./system-config-group-device.ts"></script>

