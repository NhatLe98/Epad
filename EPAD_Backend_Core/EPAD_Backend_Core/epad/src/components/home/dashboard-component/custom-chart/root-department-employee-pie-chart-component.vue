<template>
  <div style="max-height: 90%;" v-loading="!load">
    <CustomLegendPieChart
      v-if="load"
      :index="index"
      :id="id"
      :name="name"
      :dataIds="chartIds"
      :dataLabels="chartLabels"
      :dataValues="chartValues"
      :onClickToExplode="onClickToExplode"
      :explodeColumns="explodeColumns"
      :explodeData="explodeData"
      :isShowExplodeData="isShowExplodeData"
      :isAllowExportExplode="true"
      @clickedItem="handleChartClicked"
      @updateIsShowExplodeData="updateIsShowExplodeData"
    ></CustomLegendPieChart>
  </div>
</template>
<script src="./root-department-employee-pie-chart-component.ts"></script>

<style lang="scss">

</style>