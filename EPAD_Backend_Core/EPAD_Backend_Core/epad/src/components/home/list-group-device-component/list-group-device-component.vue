<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ListGroupDevice") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog :title="isEdit ? $t('EditGroupDevice') : $t('InsertGroupDevice')" 
          custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">
            <el-form
              :model="ruleForm"
              :rules="rules"
              ref="ruleForm"
              label-width="168px"
              label-position="top"
            >
              <el-form-item :label="$t('GroupDeviceName')" prop="Name" @click.native="focus('Name')">
                <el-input ref="Name" v-model="ruleForm.Name"></el-input>
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
                @click="Edit"
                round
              >{{ $t("Edit") }}</el-button>
            </el-col>
            <el-col :span="12">
              <el-button
                class="classRight"
                id="btnFunction"
                type="primary"
                @click="Delete"
                round
              >{{ $t("Delete") }}</el-button>
              <el-button
                class="classRight"
                id="btnFunction"
                type="primary"
                @click="Restart"
                round
              >{{ $t("Restart") }}</el-button>
            </el-col>
          </el-row>
          <div>
            <el-dialog title="Thông báo" :visible.sync="showMessage" width="20%" height="20%" center>
              <span>Yêu cầu đang được hệ thống xử lý...</span>
            </el-dialog>
          </div>
        </div> -->
      </el-main>
    </el-container>
  </div>
</template>
<script src="./list-group-device-component.ts"></script>

<style>
/* .restart-service {
  position: absolute;
  top: 0;
  right: 100px;
} */
</style>
