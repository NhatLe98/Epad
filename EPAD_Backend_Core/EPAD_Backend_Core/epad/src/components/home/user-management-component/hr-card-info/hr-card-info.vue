<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("CardInfoManagement") }}</span>
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
            :title="isEdit ? $t('EditCard') : $t('InsertCard')"
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
                :label="$t('EmployeeATID')"
                prop="EmployeeATID"
                @click.native="focus('EmployeeATID')"
              >
                <el-input ref="EmployeeATID" v-model="ruleForm.EmployeeATID"  :disabled="isEdit"></el-input>
              </el-form-item>
              <el-form-item
                :label="$t('CardNumber')"
                prop="CardNumber"
                @click.native="focus('CardNumber')"
              >
                <el-input
                  ref="CardNumber"
                  v-model="ruleForm.CardNumber"
                  :disabled="isEdit"
                ></el-input>
              </el-form-item>
              <el-form-item
                :label="$t('Status')"
                prop="IsActive"
                @click.native="focus('IsActive')"
              >
                  <el-checkbox :disabled="!isEdit" ref="IsActive" v-model="ruleForm.IsActive">{{$t('true')}}</el-checkbox>
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
<script src="./hr-card-info.ts"></script>