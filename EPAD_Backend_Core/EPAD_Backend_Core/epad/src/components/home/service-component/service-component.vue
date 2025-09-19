<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ListService") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog :title="isEdit ? $t('EditService') : $t('InsertService')" 
          custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">
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
              <el-form-item :label="$t('ServiceType')" prop="ServiceType" @click.native="focus('ServiceType')">
                <el-select ref="ServiceType" placeholder="Chọn loại Service" v-model="ruleForm.ServiceType">
                  <el-option
                    v-for="item in [
                      {value: 'SDKInterfaceService', label: 'SDK Interface Service'},
                      {value: 'PUSHInterfaceService', label: 'PUSH Interface Service'}
                    ]"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  ></el-option>
                </el-select>
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
          @insert="Insert" @edit="Edit" @delete="Delete" @restart="Restart"
          v-bind:listExcelFunction="listExcelFunction"
          v-bind:has1MoreBtn="true"
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
           <RestartServiceDialog 
          :showDialog.sync="showMessage"
          :listSelectedService="rowsObj" />
      </el-main>
    </el-container>
  </div>
</template>
<script src="./service-component.ts"></script>

<style>
/* .restart-service {
  position: absolute;
  top: 0;
  right: 100px;
} */
</style>
