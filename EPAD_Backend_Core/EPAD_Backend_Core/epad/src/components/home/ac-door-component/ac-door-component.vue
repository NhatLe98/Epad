<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ListDoor") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog :title="isEdit ? $t('EditDoor') : $t('InsertDoor')" 
          custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">

            <el-form
              :model="ruleForm"
              :rules="rules"
              ref="ruleForm"
              label-width="168px"
              label-position="top"
            >
              <el-form-item :label="$t('DoorName')" prop="Name" @click.native="focus('Name')">
                <el-input ref="Name" v-model="ruleForm.Name"></el-input>
              </el-form-item>
              <el-form-item :label="$t('Area')" prop="AreaIndex" @click.native="focus('AreaIndex')">
                <el-select props="AreaIndex" v-model="ruleForm.AreaIndex" clearable  ref="AreaIndex"
              :placeholder="$t('SelectArea')">
              <el-option v-for="item in listAllArea" :key="item.value" :label="$t(item.label)"
                :value="item.value"></el-option>
            </el-select>
              </el-form-item>
              <el-form-item :label="$t('Device')" prop="SerialNumberLst" @click.native="focus('SerialNumberLst')">
                <el-select filterable props="SerialNumberLst" v-model="ruleForm.SerialNumberLst" clearable multiple ref="SerialNumberLst"
              :placeholder="$t('SelectDevice')">
              <el-option v-for="item in listDevices" :key="item.SerialNumber" :label="$t(item.DeviceName)"
                :value="item.SerialNumber"></el-option>
            </el-select>
              </el-form-item>
              <!-- <el-form-item :label="$t('Timezone')" prop="Timezone" @click.native="focus('Timezone')">
                <el-select props="Timezone" v-model="ruleForm.Timezone" clearable  ref="Timezone"
              :placeholder="$t('SelectTimezone')">
              <el-option v-for="item in listTimezone" :key="item.value" :label="$t(item.label)"
                :value="item.value"></el-option>
            </el-select>
              </el-form-item> -->
            
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
       
      </el-main>
    </el-container>
  </div>
</template>
<script src="./ac-door-component.ts"></script>

<style>
/* .restart-service {
  position: absolute;
  top: 0;
  right: 100px;
} */
</style>
