<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
              <span id="FormName">{{ $t("PositionInfoManagement") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <data-table-function-component
            @insert="Insert"
            @edit="Edit"
            @delete="Delete"
            @add-excel="showDialogImportMachine = true"
            v-bind:listExcelFunction="listExcelFunction"
            :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
          ></data-table-function-component>
          <data-table-component
            :get-data="getData"
            ref="table"
            :columns="columns"
            :selectedRows.sync="rowsObj"
            :isShowPageSize="true"
          ></data-table-component>
        </div>
        <div>
          <el-dialog
            :title="isEdit ? $t('EditPosition') : $t('InsertPosition')"
            :visible.sync="showDialog"
            :before-close="Cancel"
            custom-class="customdialog"
            :close-on-click-modal="false"
          >
            <el-form
              :model="ruleForm"
              :rules="rules"
              ref="ruleForm"
              label-width="168px"
              label-position="top"
              @keyup.enter.native="Submit"
            >
              <el-form-item
                :label="$t('Name')"
                prop="Name"
                @click.native="focus('Name')"
                style="margin-bottom: 20px;"
              >
                <el-input ref="Name" v-model="ruleForm.Name"></el-input>
              </el-form-item>
              <el-form-item
                :label="$t('PositionCode')"
                prop="Code"
                @click.native="focus('Code')"
                style="margin-bottom: 20px;"
              >
                <el-input
                  ref="Code"
                  v-model="ruleForm.Code"
                  :disabled="isEdit"
                ></el-input>
              </el-form-item>
              <el-form-item
                :label="$t('Description')"
                prop="Description"
                @click.native="focus('Description')"
              >
                <el-input
                  ref="Description"
                  v-model="ruleForm.Description"
                ></el-input>
              </el-form-item>
            </el-form>
            <span slot="footer">
              <el-button class="btnOK" type="primary" @click="Submit">{{
                $t("Save")
              }}</el-button>

              <el-button class="btnCancel" @click="Cancel">{{
                $t("Cancel")
              }}</el-button>
            </span>
          </el-dialog>
        </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./hr-position-info.ts"></script>