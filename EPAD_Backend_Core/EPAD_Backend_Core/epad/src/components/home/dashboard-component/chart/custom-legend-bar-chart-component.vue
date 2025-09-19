<template>
  <div class="custom-legend-bar-chart-container"  style="height: 100%;">
    <div v-if="isDisplayName" style="height: 20% !important; text-align: center; overflow: hidden; 
      font-weight: bold; white-space: pre-line;" :title="name">
      <p style="word-wrap: break-word; display: contents;">{{ name }}</p>
    </div>
    <div v-if="isDisplayName" style="height: 1% !important;"><p></p></div>
    <div v-if="isDisplayCustomLegend" style="height: 15% !important; text-align: center;">
      <el-row style="height: 100%; overflow-y: auto;" :id="legendId">
      </el-row>
    </div>
    <div class="chartAreaWrapper" :style="!isDisplayName && !isDisplayCustomLegend ? 
    'height: 100% !important;' : 
    (!isDisplayName ? 
    'height: 85% !important;' : 
    'height: 64% !important;')">
      <Bar
      v-if="load"
      :chart-options="chartOptions"
      :chart-data="chartData"
      :chart-id="chartId"
      :dataset-id-key="datasetIdKey"
      :plugins="plugins"
      :css-classes="cssClasses"
      :styles="styles"
      style="height: 100% !important;"
      :style="{ 'width': chartWidth + '%' }"
    />
    </div>
    <el-dialog
      :title="explodeName"
      :visible.sync="isShowExplodeData"
      :before-close="handleClose"
      width="70%"
      :append-to-body="true">
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
    </el-dialog>
  </div>
</template>
<script src="./custom-legend-bar-chart-component.ts"></script>

<style lang="scss">
.chartAreaWrapper {
  overflow-x: auto;
}
</style>