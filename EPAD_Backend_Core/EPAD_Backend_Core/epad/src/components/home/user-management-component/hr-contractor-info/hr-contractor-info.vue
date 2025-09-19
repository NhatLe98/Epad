<template>
    <div class="tab">
      <div class="tab-filter">
        <el-row>
          <span>Lọc danh sách</span>
          <el-button
            type="primary"
            class="smallbutton"
            size="small"
            @click="onViewClick"
            >{{ $t("View") }}
          </el-button>
        </el-row>
      </div>
  
      <div class="tab-grid">
        <t-grid
          @onPageChange="onPageChange"
          @onPageSizeChange="onPageSizeChange"
          :gridColumns="gridColumns"
          :dataSource="dataSource"
          :selectedRows.sync="selectedRows"
          :total="total"
          :page.sync="page"
          :pageSize.sync="pageSize"
        ></t-grid>
      </div>
  
    </div>
  </template>
  <script src="./hr-contractor-info.ts"></script>