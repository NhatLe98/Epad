<template>
  <div class="function-bar">
    <div class="left">
      <ui-icon-button
        v-if="isDelete == true && IsMonitoring == false"
        @click="deleteClick"
        v-show="showDelete"
        icon
        class="btn-icon transparent pointer"
        style="margin-left: 10px;"
      >
        <ui-tooltip
          :appendToBody="true"
          position="top"
          animation="shift-away"
        ></ui-tooltip>
        <img
          src="@/assets/icons/function-bar/trash.svg"
          style="width: 18px; height: 18px;"
        />
      </ui-icon-button>

      <ui-icon-button
        v-if="isUpdate == true && IsMonitoring == false"
        @click="editClick"
        v-show="showEdit"
        icon
        class="btn-icon transparent pointer"
        style="margin-left: 10px;"
      >
        <ui-tooltip position="top" animation="shift-away"></ui-tooltip>
        <img
          src="@/assets/icons/function-bar/edit.svg"
          style="width: 18px; height: 18px;"
        />
      </ui-icon-button>

      <el-dropdown
        v-show="showMore"
        @command="moreActionClick"
        trigger="click"
        placement="bottom-end"
      >
        <slot>
          <ui-icon-button
            icon
            class="btn-icon transparent pointer"
            style="margin-left: 10px;"
          >
            <img
              src="@/assets/icons/function-bar/more.svg"
              style="width: 18px; height: 18px;"
            />
          </ui-icon-button>
        </slot>
        <el-dropdown-menu slot="dropdown">
          <el-dropdown-item v-if="isImport" command="Import">{{
            $t("Import")
          }}</el-dropdown-item>
        </el-dropdown-menu>
      </el-dropdown>
    </div>

    <div class="right">
      <el-button
        v-if="isAdd == true && IsMonitoring == false"
        v-show="showAdd"
        @click="addClick"
        class="add-btn pointer"
        color="primary"
        type="primary"
        size="normal"
        >{{ $t("Add") }}</el-button
      >

      <ui-button
        v-if="(isAdd == true || isUpdate == true) && IsMonitoring == false"
        v-show="showSave"
        @click="saveClick"
        class="add-btn save pointer"
        color="primary"
        size="normal"
        >{{ $t("Save") }}</ui-button
      >

      <ui-icon-button
        icon
        @click="openCloseGridConfigPanel"
        class="btn-icon pointer"
        style="margin-left: 10px; border: 1px solid #154284; border-radius: 5px; padding: 5px 8px;"
      >
        <img
          v-show="showConfigGridPanel === false"
          src="@/assets/icons/function-bar/adjust.svg"
          style="width: 18px; height: 18px;"
        />
        <img class="no-filter"
          v-show="showConfigGridPanel === true"
          src="@/assets/icons/function-bar/cross.svg"
          style="width: 18px; height: 18px;"
        />
      </ui-icon-button>
      <div class="popupshowcol" v-show="showConfigGridPanel">
        <!-- <ui-button  class="add-btn save pointer" color="primary">{{ $t('Save') }}</ui-button> -->
        <div class="switch-wapper">
          <div class="col-item" v-for="col in listColumn" :key="col.ID">
            <ui-switch :disabled="col.Fixed" v-model="col.Show">{{
              col.ColumnName
            }}</ui-switch>
          </div>
        </div>
      
      </div>
    </div>
  </div>
</template>

<script src="./app-function-bar-component.ts" />

<style lang="scss">
.function-bar {
  height: 45px;
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 31px 0;

  .left {
    display: flex;
    align-items: center;
  }

  .right {
    display: flex;
    align-items: center;
    position: relative;
    margin-right: 16px;
    .popupshowcol {
      z-index: 10;
      position: absolute;
      top: 40px;
      right: 5px;
      height: fit-content;
      width: 200px;
      background-color: whitesmoke;
      border-radius: 10px;
      border: 0.5px solid #bdbdbd;
      box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.25);
      padding: 10px 5px 10px 10px;
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      .switch-wapper {
        height: fit-content;
        max-height: 40vh;
        overflow: auto;
        width: 100%;
      }
    }
  }

  .more {
    display: flex;
    align-items: center;
    border-radius: 10px;
    padding: 0 10px;
    margin-left: 10px;
    height: 30px;
    width: fit-content;
    background-color: #d2e5f8;
  }

  .ui-switch__label-text {
    font-size: 12px;
  }
}
</style>
