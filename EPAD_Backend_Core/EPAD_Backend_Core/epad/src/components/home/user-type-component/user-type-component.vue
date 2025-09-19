<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("UserType") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog :title="isEdit ? $t('EditUserType') : $t('InsertUserType')" custom-class="customdialog" 
          :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">
            <el-form
              :model="ruleForm"
              :rules="rules"
              ref="ruleForm"
              label-width="168px"
              label-position="top"
            >
            <el-form-item :label="$t('UserTypeCode')" prop="UserTypeCode" @click.native="focus('UserTypeCode')">
                <el-input ref="UserTypeCode" v-model="ruleForm.Code"></el-input>
              </el-form-item>
              <el-form-item :label="$t('UserTypeId')" prop="UserTypeId" @click.native="focus('UserTypeId')">
                <el-input ref="UserTypeId" v-model="ruleForm.UserTypeId"></el-input>
              </el-form-item>
              <el-form-item :label="$t('Name')" prop="Name" @click.native="focus('Name')">
                <el-input ref="Name" v-model="ruleForm.Name"></el-input>
              </el-form-item>
              <el-form-item :label="$t('EnglishName')" prop="EnglishName" @click.native="focus('EnglishName')">
                <el-input ref="EnglishName" v-model="ruleForm.EnglishName"></el-input>
              </el-form-item>
              <el-form-item :label="$t('Description')" prop="Description" @click.native="focus('Description')">
                <el-input ref="Description" type="textarea" :rows="6" v-model="ruleForm.Description"></el-input>
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
<script src="./user-type-component.ts"></script>

