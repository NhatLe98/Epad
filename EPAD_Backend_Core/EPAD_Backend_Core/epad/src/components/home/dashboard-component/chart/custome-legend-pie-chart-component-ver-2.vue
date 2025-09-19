<template>
  <div class="custom-legend-pie-chart-container" style="height: 100%;">
    <div v-if="isDisplayName" style="height: 20% !important; text-align: center; overflow: hidden; 
      font-weight: bold; white-space: pre-line;" :title="name">
      <p style="word-wrap: break-word; display: contents;">{{ name }}</p>
    </div>
    <div v-if="isDisplayName" style="height: 1% !important;"><p></p></div>
    <div v-if="dataValues && dataValues.length > 0" style="position: relative;" :style="isDisplayName ? 'height: 79%;' : 'height: 100%;'">
      <el-row style="width: 100%; height: 100%;">
        <el-col :span="isDisplayCustomLegend ? 18 : 24" style="height: 100%;">
          <Doughnut
            v-if="load"
            ref="myChart"
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
        <el-col :span="isDisplayCustomLegend ? 6 : 0" style="height: 100%;">
          <div :id="legendId" style="height: 100%;">

          </div>
        </el-col>
      </el-row>
    </div>
    <div v-else style="height: 100%;
    text-align: center;
    justify-content: center;
    display: flex;
    flex-direction: column;">{{$t('NoData')}}</div>
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
<script src="./custome-legend-pie-chart-component-ver-2.ts"></script>

<style lang="scss" scoped>
</style>