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
          :isDisplayCustomLegend="true"
          :valuesDisplay="true"
          :dataLabels="chartLabels"
          :dataSets="chartSets"
          :XValueColor="'white'"
          :YValueColor="'white'"
          :customLegendColor="'white'"
          :onClickToExplode="onClickToExplode"
          :explodeColumns="explodeColumns"
          :explodeData="explodeData"
          :isShowExplodeData="isShowExplodeData"
          :isAllowExportExplode="true"
          @clickedItem="handleChartClicked"
          @updateIsShowExplodeData="updateIsShowExplodeData"
        ></CustomLegendBarChart>
      </el-col>
    </el-row>
  </div>
</template>
<script src="./vehicle-in-out-bar-chart-component.ts"></script>

<style lang="scss">
.el-progress.no-padding-right {
  .el-progress-bar{
    padding: 0 !important;
  }
}
</style>