<template>
    <div>
  <el-table
    ref="history-table"
    :data="data"
    :height="tableHeight"
    :empty-text="$t('NoData')"
    :row-class-name="tableRowClassName"
    class="history-table"
  >
    <el-table-column
      type="index"
      label="STT"
      width="50" :fixed="true">
      </el-table-column>
     
    <template v-for="column in columns">
      <el-table-column
        :class-name="getColumnClassName(column.Show)"
        :key="column.Name"
        :prop="column.DataField"
        :label="$t(column.Name)"
      >
        <template slot-scope="scope" >
          <el-checkbox
            disabled
            v-model="scope.row[column.Name]"
            v-if="column.DataType === 'checkbox'"
          />
          <span v-else-if="column.DataType === 'lookup'">
            {{ getLookup(column.Lookup, scope.row[column.DataField]) }}
          </span>
          <span v-else-if="column.DataType === 'date'">
            {{ getDate(column.Format, scope.row[column.DataField]) }}
          </span>
          <span v-else-if="column.DataType === 'status'">
            {{ getStatus(scope.row[column.DataField]) }}
          </span>
           <span v-else-if="column.DataType === 'inout'">
            {{ getInOutData(scope.row[column.DataField]) }}
          </span>
          <span v-else-if="column.DataType === 'image'">
            <el-link :type="scope.row[column.DataField] == null || scope.row[column.DataField] == undefined || scope.row[column.DataField] == '' ? 'default' : 'primary'" 
                @click="viewImage(scope.row[column.DataField])">
                {{scope.row[column.DataField] == null || scope.row[column.DataField] == undefined || scope.row[column.DataField] == '' ? $t('NotFound') : $t('View')  }}
            </el-link>
          </span>
          <span v-else> {{ $t(scope.row[column.DataField]) }}</span>
        </template>
      </el-table-column>
    </template>
  </el-table>
    <el-dialog title="" :visible.sync="dialogViewVisible" width="50%" >
        <el-image fit="contain" style="width:100%;height:300px;" :src="srcImage"></el-image>
    </el-dialog>
  </div>
</template>
<script src="./history-table.ts"></script>
<style lang="scss">
    .fs-fz-active {
      font-size: 1.7em;
      overflow: visible !important;
    }

    .fs-fz-deactive {
      font-size: 1em;
    }

    .history-table .warning-row td{
        background-color:rgb(255, 255, 186) !important;
    }
    .history-table .success-row td{
        background: #f0f9eb;
    }
    .el-table .error-row {
        background: #ffbbb8;
    }
    .el-table .el-table__header-wrapper th, .el-table .el-table__fixed-header-wrapper th{
        background-color:#ffffff;
    }

    .history-table {
      .el-table__body-wrapper,
      .el-table__header-wrapper {
        table {
          width: 100% !important;
        }
      }
    }
</style>