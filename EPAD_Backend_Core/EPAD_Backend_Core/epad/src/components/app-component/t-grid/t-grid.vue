<template>
  <div class="t-grid">
    <el-table
      ref="grid-ref"
      :data="dataSource"
      :empty-text="$t('NoData')"
      @selection-change="handleSelectionChange"
      @row-click="showListImages"
      highlight-current-row
    >
      <el-table-column
        type="selection"
        :fixed="true"
        :selectedRows="selectedRows"
      ></el-table-column>

      <el-table-column
        v-if="hasIndex"
        type="index"
        width="50"
        :label="$t('Idx')"
        :fixed="true"
        :index="indexMethod"
      ></el-table-column>

      <template v-for="column in gridColumns">
        <el-table-column
          :label-class-name="column.show === false ? 'hid' : ''"
          :key="column.name"
          :fixed="column.fixed"
          :prop="column.dataField"
          :label="$t(column.name)"
          :width="column.width"
        >
          <template slot-scope="scope">
            <el-checkbox
              disabled
              v-model="scope.row[column.name]"
              v-if="column.dataType === 'checkbox'"
            />

            <span v-else-if="column.dataType === 'lookup'">
              {{ getLookup(column.lookup, scope.row[column.dataField]) }}
            </span> 

            <span v-else-if="column.dataType === 'date'">
              {{ getDate(column.format, scope.row[column.dataField]) }}
            </span>

            <span v-else-if="column.dataType === 'translate'">
              {{ getTranslate(scope.row[column.dataField]) }}
            </span>

            <span v-else-if="column.dataType === 'viewDetailPopup'">
              <el-button @click="viewDetailPopup(scope.row)" class="view-detail-btn" >
                  <p>{{ scope.row[column.dataField] }}</p>
              </el-button>
            </span>

            <span v-else-if="column.dataType === 'image'">
              <img
                v-if="!isEmpty(scope.row[column.dataField])"
                :src="`data:image/jpeg;base64,${scope.row[column.dataField]}`"
                style="width: 40px; border-radius: 50%"
                @click="showImage($t(column.name), scope.row[column.dataField])"
              />
            </span>

            <el-tooltip
              placement="top"
              :value="scope.row['Id'] == clickedData"
              v-else-if="column.dataType === 'mainImageTooltip'"
              effect="light"
            >
              <template slot="content">
                <div :class="getIdTooltip(scope.row['Id'])"></div>
                <div style="display: inline-flex">
                  <div
                    class="block group-image"
                    v-for="(item, index) in ListImages"
                    :key="index"
                  >
                    <span class="demonstration">{{ item.label }}</span>
                    <el-image
                      :src="item.value"
                      style="width: 125px; height: 125px"
                    ></el-image>
                  </div>
                </div>
              </template>
              <el-image
                :src="scope.row[column.dataField]"
                style="width: 50px; height: 50px"
                :class="getIdImage(scope.row['Id'])"
              ></el-image>
            </el-tooltip>

            <span v-else-if="column.dataType === 'imageTooltip'">
              <el-image
                :src="scope.row[column.dataField]"
                style="width: 50px; height: 50px"
              ></el-image>
            </span>

            <span v-else> {{ scope.row[column.dataField] }}</span>
          </template>
        </el-table-column>
      </template>
    </el-table>

    <el-col class="page-container">
      <slot name="pagination" v-if="showFooter">
        <div class="page-number">
          <small>{{$t("Display")}}</small>
          
          <el-input 
            v-model="pageSize"
            @change="onPageSizeChange"
            filterable
            default-first-option
            style=" margin-left:10px;width:80px "
          >
          </el-input>
          
          <el-pagination
            class="custom-pagination"
            :total="total"
            :page-sizes="[20, 40, 50, 100]"
            :page-size="pageSize"
            :current-page="page"
            @current-change="onPageClick"
            @size-change="onPageSizeChange"
            layout="prev, pager, next"
          ></el-pagination>
          
          <div class="total-record">
            <small>Tổng số: <b>{{total}}</b></small>
          </div>
        </div>
      </slot>
    </el-col>

    <div class="image-modal">
      <el-dialog
        custom-class="customdialog"
        :title="$t('DialogOption')"
        :visible.sync="showModalImage"
        width="30%"
      >
        <div>
          <img
            :src="`data:image/jpeg;base64,${Image.src}`"
            :alt="Image.label"
            style="max-width: 100%"
          />
        </div>
      </el-dialog>
    </div>
  </div>
</template>

<script src="./t-grid.ts" />

<style lang="scss">
.group-image {
  display: grid;
  .demonstration {
    font-weight: 600;
    text-align: center;
  }
}
.t-grid {
  .page-container {
    margin-top: 5px;
    .page-number{
      display: flex;
      justify-content: center;
      small {
        line-height: 28px;
      }
    }
  }
  .has-search > .cell {
    display: block;
    min-height: 28px;
    min-width: 50px;
    position: relative;
    .search-column,
    .column-name {
      position: absolute;
      top: 0;
      left: 30px;
    }
    .search {
      position: absolute;
      top: 5px;
      left: 0;
      display: block;
      width: fit-content;
      height: fit-content;
    }
    .search-column {
      width: calc(100% - 35px);
    }
    .column-name {
      z-index: 1;
    }
    .search-column {
      opacity: 0;
      z-index: 2;
    }
    &:hover {
      .search-column {
        opacity: 1;
      }
      .column-name {
        opacity: 0;
      }
    }
  }
}
.el-checkbox__input.is-disabled.is-checked .el-checkbox__inner {
  background-color: #122658;
  border-color: #dcdfe6;
}
.hid {
  display: none;
}
.image-modal {
  .ui-focus-container {
    width: auto;
    img {
      width: auto;
      height: 85vh;
    }
  }
}

.customdialog {
  align-items: center;
}
.view-detail-btn {
  border: none;
  text-decoration: underline;
  background: none !important;
  color: rgb(23, 93, 243);
  padding: 0px;
}
.total-record {
  display: inline;
  margin-left: 10px;
}
</style>

<style scoped>
.t-grid >>> .el-pagination__sizes {
  /* display: inline-block !important; */
}
</style>
