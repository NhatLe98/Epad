<template>
  <div style="max-height: 100%" v-loading="!load">
    <el-row style="height: 100%" :gutter="20">
      <el-col :span="16" style="height: 100%">
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
          @clickedItem="handleChartClicked"
          @updateIsShowExplodeData="updateIsShowExplodeData"
        ></CustomLegendBarChart>
      </el-col>
      <el-col
        v-if="load"
        :span="8"
        style="
          height: 100%;
          display: flex;
          flex-direction: column;
          justify-content: space-between;
        "
      >
        <div>
          <span>{{ $t("Employee") }}</span
          >:
          <span style="font-weight: bold">{{ logData.Item2.length }}</span>
          <el-progress
            class="no-padding-right"
            :color="chartSetColors[0]"
            :stroke-width="20"
            :percentage="(logData.Item2.length / totalValue) * 100"
            :format="format"
          >
          </el-progress>
        </div>
        <div>
          <span>{{
            clientName == "Mondelez" ? $t("CustomerAndNT24") : $t("Customer")
          }}</span
          >:
          <span style="font-weight: bold">{{ logData.Item3.length }}</span>
          <el-progress
            class="no-padding-right"
            :color="chartSetColors[1]"
            :stroke-width="20"
            :percentage="(logData.Item3.length / totalValue) * 100"
            :format="format"
          >
          </el-progress>
        </div>
        <div v-if="totalType == 5">
          <span>{{ $t("Contractor") }}</span
          >:
          <span style="font-weight: bold">{{
            totalType == 5 ? logData.Item4.length : logData.Item4.length
          }}</span>
          <el-progress
            class="no-padding-right"
            :color="chartSetColors[2]"
            :stroke-width="20"
            :percentage="(logData.Item4.length / totalValue) * 100"
            :format="format"
          >
          </el-progress>
        </div>
        <!-- <div v-else > -->
          <div v-if="totalType != 5" >
            <span>{{ $t(logData.Item1[2]) }}</span
            >:
            <span style="font-weight: bold">{{ logData.Item4.length }}</span>
            <el-progress
              class="no-padding-right"
              :color="chartSetColors[2]"
              :stroke-width="20"
              :percentage="(logData.Item4.length / totalValue) * 100"
              :format="format"
            >
            </el-progress>
          </div>
          <div v-if="totalType != 5">
            <span>{{ $t(logData.Item1[3]) }}</span
            >:
            <span style="font-weight: bold">{{ logData.Item5.length }}</span>
            <el-progress
              class="no-padding-right"
              :color="chartSetColors[3]"
              :stroke-width="20"
              :percentage="(logData.Item5.length / totalValue) * 100"
              :format="format"
            >
            </el-progress>
          </div>
        <!-- </div> -->
        <div>
          <span>{{ $t("Driver") }}</span
          >:
          <span style="font-weight: bold">{{
            totalType == 5 ? logData.Item5.length : logData.Item6.length
          }}</span>
          <el-progress
            class="no-padding-right"
            :color="chartSetColors[3]"
            :stroke-width="20"
            :percentage="
              ((totalType == 5 ? logData.Item5.length : logData.Item6.length) /
                totalValue) *
              100
            "
            :format="format"
          >
          </el-progress>
        </div>
        <div>
          <span>{{ $t("ExtraDriver") }}</span
          >:
          <span style="font-weight: bold">{{
            totalType == 5 ? logData.Item6.length : logData.Item7.length
          }}</span>
          <el-progress
            class="no-padding-right"
            :color="chartSetColors[0]"
            :stroke-width="20"
            :percentage="
              ((totalType == 5 ? logData.Item6.length : logData.Item7.length) /
                totalValue) *
              100
            "
            :format="format"
          >
          </el-progress>
        </div>
      </el-col>
    </el-row>
  </div>
</template>
<script src="./employee-present-by-user-type-bar-chart-component-2.ts"></script>

<style lang="scss">
.el-progress.no-padding-right {
  .el-progress-bar {
    padding: 0 !important;
  }
}
</style>