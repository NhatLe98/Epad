<template>
    <div id="bgHome">
      <el-container>
        <el-header>
          <el-row>
            <el-col :span="12" class="left">
              <span id="FormName">{{ $t("LocationOperator") }}</span>
            </el-col>
            <el-col :span="12">
              <HeaderComponent />
            </el-col>
          </el-row>
        </el-header>
        <el-main class="bgHome">
          <div>
            <el-dialog :title="isEdit ? $t('EditLocationOperator') : $t('InsertLocationOperator')" custom-class="customdialog" 
            :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">
              <el-form
                :model="ruleForm"
                :rules="rules"
                ref="ruleForm"
                label-width="168px"
                label-position="top"
              >
         
                <el-form-item :label="$t('Name')" prop="Name" @click.native="focus('Name')">
                  <el-input ref="Name" v-model="ruleForm.Name"></el-input>
                </el-form-item>
                <el-form-item :label="$t('Part')" prop="Department" @click.native="focus('Department')">
                  <el-input ref="Department" v-model="ruleForm.Department"></el-input>
                </el-form-item>
              
              </el-form>
              <span slot="footer" class="dialog-footer">
                <el-button class="btnCancel" @click="Cancel">
                  {{
                  $t("Cancel")
                  }}
                </el-button>
                <el-button class="btnOK" type="primary" @click="Submit('ruleForm')">{{ $t("OK") }}</el-button>
              </span>
            </el-dialog>
          </div>
          <div>
            <data-table-function-component
            @insert="Insert" @edit="Edit" @delete="Delete"
            v-bind:listExcelFunction="listExcelFunction"
            :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
            >
            </data-table-function-component>
            <data-table-component
              :get-data="getData"
              ref="table"
              :columns="columns"
              :selectedRows.sync="rowsObj"
              :isShowPageSize="true"
            ></data-table-component>
          </div>
          <!-- {{showMessage + ''}} -->
             <RestartUserTypeDialog 
            :showDialog.sync="showMessage"
            :listSelectedUserType="rowsObj" />
        </el-main>
      </el-container>
    </div>
  </template>
  <script src="./ic-location-operator-component.ts"></script>
  
  