<template v-if="loading">
  <div class="matrix-checkbox-menu">
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
      <el-table-column min-width="220px" :label="getLabel" fixed>
        <template slot-scope="scope">
          <!-- {{ scope.row }} -->
          <span :style="'padding-left: calc(' + (scope.row.level - 1) + ' * 3vw);'">{{ $t(scope.row.name) }}</span>
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
          <MultiStateComponent @changeState="changeState" :menuId="list[scope.$index].menuId" 
            :state="list[scope.$index].roles[ix]" :listState="listState" :hasTooltip="true"></MultiStateComponent>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>
<script src="./matrix-checkbox-menu-component.ts"></script>