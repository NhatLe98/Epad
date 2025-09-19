<template>
  <AppLayout :formName="$t('SystemCommand')">
    <el-row class="system-command__custom-function-bar">
      <el-col :span="3" style="margin-right: 10px;">
        <el-date-picker
            v-model="fromTime"
            type="datetime"
            id="inputFromDate"
            :placeholder="$t('SelectDateTime')"
          ></el-date-picker>
      </el-col>
      <el-col :span="3" style="margin-right: 10px;">
        <el-date-picker
            v-model="toTime"
            type="datetime"
            :placeholder="$t('SelectDateTime')"
          ></el-date-picker>
      </el-col>
      <el-col :span="1">
        <el-button class="system-command__view-btn" type="primary" @click="View">{{ $t("View") }}</el-button>
      </el-col>
    </el-row>
    <el-row class="system-command__custom-button-bar">
      <div style="display: flex; justify-content: flex-end; width: 100%;">
        <el-tooltip style="margin-right: 10px;"
            :open-delay="2000"
            content="Nạp lại lệnh vào bộ đệm của máy chủ, sử dụng trong trường hợp PUSH hoặc SDK lấy danh sách lệnh rỗng trên epad"
          >
            <el-button type="primary" @click="ResetCaching">{{
              $t("ResetCaching")
            }}</el-button>
          </el-tooltip>
        <el-tooltip
            :open-delay="1000"
            content="Tạo lại 1 lệnh nếu lệnh bị treo khi đang thực hiện hoặc lệnh ở trạng thái chờ thực hiện quá lâu"
          >
            <el-button type="primary" @click="renewCommands">{{
              $t("RenewCommand")
            }}</el-button>
          </el-tooltip>
      </div>
    </el-row>
    <div class="table">
        <data-table-function-component class="system-command__data-table-function"
          style="top: 49px"
          @delete="Delete"
          v-bind:isHiddenEdit="true"
          v-bind:isHiddenDelete="false"
          v-bind:showButtonCustom="false"
          v-bind:showButtonInsert="false"
          :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
        ></data-table-function-component>

        <data-table-component class="system-command__data-table"
          ref="table"
          :get-data="getData"
          :columns="columns"
          :showCheckbox="true"
          v-bind:isFromHasView="true"
          :selectedRows.sync="rowsObj"
          :isShowPageSize="true"
        ></data-table-component>
      </div>
      <CommandDeleteConfirmDialog
        :showDialog.sync="showDeleteConfirmDialog"
        :commandsWillDelete="rowsObj"
        @onDeleteSuccess="fetchAndSetData"
      />
  </AppLayout>
</template>
<script src="./system-command-component.ts"></script>
<style lang="scss">
.system-command__data-table {
  .el-table {
    margin-top: 0 !important;
    height: calc(100vh - 215px) !important;
  }
}
.system-command__data-table {
  .filter-input {
    margin-right: 10px;
  }
}
.system-command__data-table-function {
  position: unset !important;
  display: flex !important;
  margin-bottom: 5px;
  justify-content: flex-start !important;
  width: 100% !important;
}
.system-command__view-btn {
  height: 32px;
  span {
    line-height: 16px;
  }
}
</style>
