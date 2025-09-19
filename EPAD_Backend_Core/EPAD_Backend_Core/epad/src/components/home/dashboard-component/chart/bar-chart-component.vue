<template>
  <div class="bar-chart-container"  style="height: 100%;">
    <div style="height: 20% !important; text-align: center; overflow: hidden; 
      font-weight: bold; white-space: pre-line;" :title="name">
      <p style="word-wrap: break-word; display: contents;">{{ name }}</p>
    </div>
      <div style="height: 1% !important;"><p></p></div>
    <Bar
      v-if="load"
      :chart-options="chartOptions"
      :chart-data="chartData"
      :chart-id="chartId"
      :dataset-id-key="datasetIdKey"
      :plugins="plugins"
      :css-classes="cssClasses"
      :styles="styles"
      style="height: 79% !important;"
    />
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
<script src="./bar-chart-component.ts"></script>

<style lang="scss">

</style>