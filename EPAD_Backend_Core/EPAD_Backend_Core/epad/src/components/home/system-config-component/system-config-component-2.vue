<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("Configuration") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome config" style="padding-bottom:12px">
        <!-- <el-row>
          <el-button class="classLeft" type="primary">Tất cả</el-button>
          <el-button class="classLeft bygroupmachine" type="primary">Theo nhóm máy</el-button>
        </el-row> -->
        <template v-if="!isLoading">
          <template v-for="cfg in getListConfig">
            <ConfigComponent
               v-if="cfg.UsingBasicMenu === true"
              :key="cfg.EventType"
              :timePosOption="timePosOption"
              :emailOption="emailOption"
              :serialNumberOption="serialNumberOption"
              :configModel.sync="configCollection[cfg.EventType]"
              :departmentOption="departmentOptions"
              :groupDeviceOption="groupDeviceOption"
            >
            </ConfigComponent>
          </template>
          <IntegrateLogRealTimeConfigComponent v-if="usingBasicMenu === false" :configModel.sync="configIntegrateLogRealtime" />
          <el-collapse style="margin-bottom: 10px;">
            <el-collapse-item :title="$t('ChangeColorTheme')" :name="'ChangeColorTheme'">
              <label class="el-form-item__label" 
                style="width: 250px; display: inline-block;line-height: 32px;">
                {{ $t('ChangeColorTheme') }}
              </label>
              <el-select
                v-model="colorTheme"
                filterable
                class="w-100"
                style="width: calc(100% - 250px)"
              >
                <el-option
                  v-for="item in colorThemes"
                  :key="item.value"
                  :label="item.name"
                  :value="item.value"
                ></el-option>
              </el-select>
            </el-collapse-item>
          </el-collapse>
            
        </template>

        <el-row>
          <el-col :span="24" class="left">
            <!-- <el-button
              type="primary"
              @click="changeUpdateUI"
            >{{ $t("Change") }}</el-button> -->
            <el-button
              type="primary"
              @click="SaveConfig"
            >{{ $t("Save") }}</el-button>
          </el-col>
        </el-row>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./system-config-component-2.ts"></script>

