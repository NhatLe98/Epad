<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("LoginAccount") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog :title="isEdit ? $t('EditLoginAccount') : $t('InsertLoginAccount')" 
          custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">
            <el-form
              :model="ruleForm"
              :rules="rules"
              ref="ruleForm"
              label-width="168px"
              label-position="top"
              @keyup.enter.native="Submit"
            >
              
              <el-form-item @click.native="focus('ShowName')" :label="$t('ShowName')" prop="Name">
                <el-input ref="ShowName" v-model="ruleForm.Name"></el-input>
              </el-form-item>
              <el-form-item v-if="isEdit" :label="$t('Account')" prop="UserName">
                <el-input disabled v-model="ruleForm.UserName"></el-input>
              </el-form-item>
              <el-form-item v-else @click.native="focus('Account')" :label="$t('Account')" prop="UserName">
                <el-input ref="Account" v-model="ruleForm.UserName"></el-input>
              </el-form-item>
              <el-form-item @click.native="focus('Password')" :label="$t('Password')" v-if="!isEdit" prop="Password">
                <el-input ref="Password" v-model="ruleForm.Password" type="password"></el-input>
              </el-form-item>

              <el-form-item @click.native="focus('GroupAccount')" :label="$t('GroupAccount')">
                <el-select ref="GroupAccount" v-model="ruleForm.AccountPrivilege" >
                  <el-option
                    v-for="item in accountPrivilege"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">{{ $t("Cancel") }}</el-button>
              <el-button class="btnOK" type="primary" @click="Submit">{{ $t("OK") }}</el-button>

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
        <!-- <div>
          <el-row>
            <el-col :span="12" class="left">
              <el-button
                class="classLeft"
                id="btnFunction"
                type="primary"
                @click="Insert"
                round
              >{{ $t("Insert") }}</el-button>
              <el-button
                class="classLeft"
                id="btnFunction"
                type="primary"
                round
                @click="Edit"
              >{{ $t("Edit") }}</el-button>
            </el-col>
            <el-col :span="12">
              <el-button
                class="classRight"
                id="btnFunction"
                type="primary"
                round
                @click="Delete"
              >{{ $t("Delete") }}</el-button>
            </el-col>
          </el-row>
        </div> -->
      </el-main>
    </el-container>
  </div>
</template>
<script src="./login-account-component.ts"></script>
