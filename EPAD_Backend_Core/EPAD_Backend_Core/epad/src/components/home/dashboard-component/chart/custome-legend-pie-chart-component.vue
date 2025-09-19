<template>
  <div class="custom-legend-pie-chart-container" style="height: 100%;">
    <div style="height: 20% !important; text-align: center; overflow: hidden; 
      font-weight: bold; white-space: pre-line;" :title="name">
      <p style="word-wrap: break-word; display: contents;">{{ name }}</p>
    </div>
    <div style="height: 1% !important;"><p></p></div>
    <div style="height: 79% !important; position: relative;">
      <el-row style="width: 100%; height: 100%;">
        <el-col :span="12" style="height: 100%;">
          <Pie
            v-if="load"
            :chart-options="chartOptions"
            :chart-data="chartData"
            :chart-id="chartId"
            :dataset-id-key="datasetIdKey"
            :plugins="plugins"
            :css-classes="cssClasses"
            :styles="styles"
            style="width: 100%; height: 100%;"
          />
        </el-col>
        <el-col :span="12" style="height: 100%;">
          <div :id="legendId" style="height: 100%;" class="custom-legend">

          </div>
        </el-col>
      </el-row>
    </div>
    <el-dialog
      :title="explodeName"
      :visible.sync="isShowExplodeData"
      :before-close="handleClose"
      width="70%"
      :append-to-body="true">
      <div>
        <el-button v-if="isAllowExportExplode" @click="exportToExcel">{{$t('Export')}}</el-button>
        <el-table
        height="50vh"
        :data="explodeData">
        <el-table-column v-for="col in explodeColumns" :key="col.ID"
          :prop="col.prop"
          :label="col.label"
          min-width="180">
        </el-table-column>
      </el-table>
      </div>
    </el-dialog>
  </div>
</template>
<script src="./custome-legend-pie-chart-component.ts"></script>

<style lang="scss" scoped>
</style>