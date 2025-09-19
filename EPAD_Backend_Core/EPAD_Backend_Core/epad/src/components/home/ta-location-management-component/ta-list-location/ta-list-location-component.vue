<template>
  <div class="bgHome" style="height: calc(100vh - 185px)">
    <div class="tab-modal">
      <el-row :span="4">
        <el-input
          style="
            padding-bottom: 3px;
            float: left;
            width: 200px;
            margin-top: 3px;
          "
          :placeholder="$t('SearchData')"
          v-model="filterModel.TextboxSearch"
          @keyup.enter.native="onViewClick"
          class="filter-input"
        >
          <i slot="suffix" class="el-icon-search" @click="onViewClick"></i>
        </el-input>
      </el-row>
      <el-dialog
        v-if="showDialog"
        custom-class="customdialog"
        :title="isEdit ? $t('EditLocation') : $t('InsertLocation')"
        :visible.sync="showDialog"
        :before-close="onCancelClick"
        :close-on-click-modal="false"
      >
        <el-form
          :model="ruleForm"
          :rules="rules"
          ref="ruleForm"
          label-width="168px"
          label-position="top"
          @keyup.enter.native="onSubmitClick"
        >
          <el-form-item
            @click.native="focus('LocationName')"
            :label="$t('LocationName')"
            prop="LocationName"
          >
            <el-input
              ref="LocationName"
              v-model="ruleForm.LocationName"
            ></el-input>
          </el-form-item>
          <el-form-item
            @click.native="focus('Address')"
            :label="$t('Address')"
            prop="Address"
          >
            <el-input ref="Address" v-model="ruleForm.Address"></el-input>
          </el-form-item>
          <el-form-item
            @click.native="focus('Coordinates')"
            :label="$t('Coordinates')"
            prop="Coordinates"
          >
            <el-input
              ref="Coordinates"
              v-model="ruleForm.Coordinates"
              :disabled="isAdding"
            ></el-input>
          </el-form-item>
          <el-form-item
            @click.native="focus('Radius')"
            :label="$t('Radius')"
            prop="Radius"
          >
            <el-input ref="Radius" v-model="ruleForm.Radius"></el-input>
          </el-form-item>
          <el-form-item
            :label="$t('Description')"
            prop="Description"
            @click.native="focus('Description')"
          >
            <el-input
              ref="Description"
              type="textarea"
              :rows="2"
              v-model="ruleForm.Description"
            ></el-input>
          </el-form-item>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="onCancelClick">
            {{ $t("Cancel") }}
          </el-button>
          <el-button class="btnOK" type="primary" @click="onSubmitClick">{{
            $t("OK")
          }}</el-button>
        </span>
      </el-dialog>
    </div>
    <div class="tab-modal-delete">
      <el-dialog
        v-if="showDialogDeleteUser"
        custom-class="customdialog"
        :title="$t('DialogOption')"
        :visible.sync="showDialogDeleteUser"
        :before-close="cancelDeleteUser"
      >
        <el-form label-width="168px" label-position="top">
          <div style="margin-bottom: 20px">
            <i
              style="font-weight: bold; font-size: larger; color: orange"
              class="el-icon-warning-outline"
            />
            <span style="font-weight: bold">
              {{ $t("MSG_ConfirmDelete") }}
            </span>
          </div>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="showDialogDeleteUser = false">
            {{ $t("Cancel") }}
          </el-button>
          <el-button type="primary" @click="Delete">
            {{ $t("OK") }}
          </el-button>
        </span>
      </el-dialog>
    </div>
    <div class="tab-grid" style="height: 100% !important">
      <VisualizeTable
        v-loading="isLoading"
        :columnDefs="columnDefs.filter((x) => x.display)"
        :rowData="dataSource"
        :rowHeight="40"
        @onSelectionChange="onSelectionChange"
        :heightScale="260"
        :isKeepIndexAscending="true"
        :shouldResetColumnSortState="shouldResetColumnSortState"
        style="height: calc(100% - 83px); border-bottom: 1px solid lightgray"
      />
      <AppPagination
        :page.sync="page"
        :pageSize.sync="pageSize"
        :get-data="loadData"
        :total="total"
        ref="table"
      />
    </div>
    <div class="location_popup">
      <location-popup-component
        v-if="showMapDialog"
        :title="$t('ViewCoordinates')"
        :visible.sync="showMapDialog"
        :mapUrl="mapUrl"
        @close="onCancelLocationClick"
      >
      </location-popup-component>
    </div>
  </div>
</template>

<script src="./ta-list-location-component.ts"></script>