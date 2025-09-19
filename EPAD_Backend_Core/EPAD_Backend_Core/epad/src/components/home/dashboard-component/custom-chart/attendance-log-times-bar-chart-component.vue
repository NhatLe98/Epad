<template>
  <div style="max-height: 90%;" v-loading="!load">
    <CustomLegendBarChart
      v-if="load"
      :index="index"
      :id="id"
      :name="name"
      :dataLabels="chartLabels"
      :dataSets="chartSets"
      :XValueColor="textChartColor"
      :YValueColor="textChartColor"
      :onClickToExplode="onClickToExplode"
      :explodeColumns="explodeColumns"
      :explodeData="explodeData"
      :isShowExplodeData="isShowExplodeData"
      :isAllowExportExplode="true"
      @clickedItem="handleChartClicked"
      @updateIsShowExplodeData="updateIsShowExplodeData"
    ></CustomLegendBarChart>
  </div>
</template>
<script src="./attendance-log-times-bar-chart-component.ts"></script>

<style lang="scss">

</style>