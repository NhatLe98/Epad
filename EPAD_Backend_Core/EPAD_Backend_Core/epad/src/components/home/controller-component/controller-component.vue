<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ListController") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog :title="isEdit ? $t('EditController') : $t('InsertController')" custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel">
            <el-form
              :model="ruleForm"
              :rules="rules"
              ref="ruleForm"
              label-width="168px"
              label-position="top"
            >
              <el-form-item :label="$t('ControllerName')" prop="Name" @click.native="focus('Name')">
                <el-input ref="Name" v-model="ruleForm.Name"></el-input>
              </el-form-item>
               <el-form-item :label="$t('IDController')" prop="IDController" @click.native="focus('IDController')">
                <el-input ref="IDController" v-model="ruleForm.IDController"></el-input>
              </el-form-item>
              <el-form-item :label="$t('AddressIP')" prop="IPAddress" @click.native="focus('IPAddress')">
                <el-input ref="IPAddress" v-model="ruleForm.IPAddress"></el-input>
              </el-form-item>
                <el-form-item :label="$t('Port')" prop="Port" @click.native="focus('Port')">
                <el-input ref="Port" v-model="ruleForm.Port"></el-input>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">
                {{
                $t("Cancel")
                }}
              </el-button>
              <el-button class="btnOK" type="primary" @click="Submit()">{{ $t("OK") }}</el-button>
            </span>
          </el-dialog>
        </div>
        <div>
          <!-- <div class="restart-service">
            <el-button type="primary" @click="Restart">{{ $t("Restart") }}</el-button>
          </div> -->
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
      </el-main>
    </el-container>
  </div>
</template>
<script src="./controller-component.ts"></script>

<style>
/* .restart-service {
  position: absolute;
  top: 0;
  right: 100px;
} */
</style>
