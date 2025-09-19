<template>
  <div style="max-height: 100%;">
    <div v-loading="!load" style="height: 100%;">
      <CustomLegendPieChartVer2 
        v-if="load"
        :index="index"
        :id="id"
        :name="name"
        :dataIds="chartIds"
        :dataLabels="chartLabels"
        :dataValues="chartValues"
        :padding="40"
        :colorData="colorData"
        :isRandomColor="false"
        :isDisplayName="false"
        :isDisplayCustomLegend="false"
        :colorDataLabel="colorData"
        :onClickToExplode="onClickToExplode"
        :explodeColumns="explodeColumns"
        :explodeData="explodeData"
        :isShowExplodeData="isShowExplodeData"
        :isAllowExportExplode="true"
        @clickedItem="handleChartClicked"
        @updateIsShowExplodeData="updateIsShowExplodeData"
      ></CustomLegendPieChartVer2>
    </div>
    <div style="position: absolute; bottom: 5px; right: 5px;">
      <!-- <el-button type="primary"
      style="margin-right: 5px;"
      :disabled="!load"
      :style="load ? 'background-color: rgb(19, 56, 152) !important;' : 'background-color: rgb(19 56 152 / 50%) !important; '"
      @click="updateEmergencyData">
        {{ (load ? $t('Update') : ($t('Updating') + '...')) }}
      </el-button> -->
      <el-button type="primary" id="clearEmergencyBtn"
      style="background-color: rgb(19, 56, 152) !important;"
      @click="clearEmergencyData">
        {{ $t('ClearData') }}
      </el-button>
    </div>
  </div>
</template>
<script src="./emergency-log-pie-chart-component.ts"></script>

<style lang="scss">

</style>