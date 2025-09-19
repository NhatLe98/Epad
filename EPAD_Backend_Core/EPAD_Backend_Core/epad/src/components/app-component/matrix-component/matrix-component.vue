<template v-if="loading">
  <div>
    <el-table
      :data="YList"
      v-loading="false"
      element-loading-text="Loading"
      border
      fit
      highlight-current-row
      style="width: 100%"
    >
      <el-table-column min-width="250px" align="center" :label="matrixLabel" fixed>
        <template slot-scope="scope">
          <span>{{ $t(scope.row.name) }}</span>
        </template>
      </el-table-column>
      <el-table-column
        min-width="100px"
        align="center"
        :label="$t(col.name)"
        :key="col.id"
        v-for="col in XList"
      >
        <template slot-scope="scope">
          <el-tooltip
            class="item"
            effect="dark"
            :content="getStateTitle(col.id, scope.row.roles)"
            placement="right"
          >
            <el-button
              :type="getStateType(col.id, scope.row.roles)"
              :icon="getStateIcon(col.id, scope.row.roles)"
              @click="changeState(col.id, scope.row)"
              circle
            ></el-button>
          </el-tooltip>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>
<script src="./matrix-component.ts"></script>