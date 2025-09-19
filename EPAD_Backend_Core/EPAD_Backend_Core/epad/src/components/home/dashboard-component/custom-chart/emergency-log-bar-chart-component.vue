<template>
  <div style="max-height: 100%;" v-loading="!load">
    <el-row style="height: 100%;" :gutter="20">
      <el-col :span="24" style="height: 100%;">
        <CustomLegendBarChart
          v-if="load"
          :index="index"
          :id="id"
          :name="name"
          :isDisplayName="false"
          :isDisplayCustomLegend="false"
          :valuesDisplay="true"
          :valuesFormat="'percent'"
          :XValueColor="'white'"
          :YValueColor="'white'"
          :tooltipEnable="false"
          :dataLabels="chartLabels"
          :dataSets="chartSets"
          :onClickToExplode="onClickToExplode"
          :explodeColumns="explodeColumns"
          :explodeData="explodeData"
          :isShowExplodeData="isShowExplodeData"
          :isAllowExportExplode="true"
          :randomBarColorInSet="true"
          @clickedItem="handleChartClicked"
          @updateIsShowExplodeData="updateIsShowExplodeData"
        ></CustomLegendBarChart>
      </el-col>
    </el-row>
  </div>
</template>
<script src="./emergency-log-bar-chart-component.ts"></script>

<style lang="scss">
.el-progress.no-padding-right {
  .el-progress-bar{
    padding: 0 !important;
  }
}
</style>