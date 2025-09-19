<template v-if="loading">
  <div class="matrix-checkbox">
    <el-table
      :data="list"
      v-loading="false"
      element-loading-text="Loading"
      border
      fit
      highlight-current-row
      style="width: 100%"
      :max-height="maxHeight"
    >
      <el-table-column min-width="220px" align="center" :label="getLabel" fixed>
        <template slot-scope="scope">
          <span>{{ $t(scope.row.name) }}</span>
        </template>
      </el-table-column>
      <el-table-column
        min-width="100px"
        align="center"
        :label="$t(col.name)"
        :key="col.group_id"
        v-for="(col, ix) in tableHead"
      >
        <template slot-scope="scope">
          <MultiStateComponent :state="list[scope.$index].roles[ix]" :listState="listState" :hasTooltip="true"></MultiStateComponent>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>
<script src="./matrix-checkbox-component.ts"></script>