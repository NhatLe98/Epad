<template>
  <el-row :gutter="20">
    <el-form>
      <el-col :span="24">
        <el-form-item>
          <el-collapse>
            <el-collapse-item :title="$t(config.Title)" :name="config.Title">
              <el-row :gutter="20">
                <el-col :span="23">
                  <el-form-item class="mt-3">
                    <el-checkbox v-model="config.IntegrateLogRealtime">{{ $t('IntegrateLogRealTime')}}</el-checkbox>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col :span="23">
                  <el-form-item @click.native="focus('LinkAPI')" :label="$t('Link API')" label-width="250px">
                    <el-input ref="LinkAPI" v-model="config.LinkAPI"></el-input>
                  </el-form-item>
                </el-col>
              </el-row>
              <slot></slot>
            </el-collapse-item>
          </el-collapse>
        </el-form-item>
      </el-col>
    </el-form>
  </el-row>
</template>
<script src="./integrate-log-realtime-config.ts"></script>
<style scoped>
.el-checkbox {
  margin-left: 250px;
}
</style>
