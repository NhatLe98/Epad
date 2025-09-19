<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ListDormRoom") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog :title="isEdit ? $t('EditDormRoom') : $t('AddDormRoom')" :visible.sync="showDialog"
            custom-class="customdialog" :before-close="Cancel" :close-on-click-modal="false">
            <el-form :model="ruleForm" :rules="rules" ref="ruleForm" label-width="168px" label-position="top"
              @keyup.enter.native="Submit">
              <el-form-item prop="Code" :label="$t('RoomCode')">
                <el-input :disabled="isEdit" ref="Code" v-model="ruleForm.Code"></el-input>
              </el-form-item>
              <el-form-item prop="Name" :label="$t('RoomName')" style="margin-top:32px">
                <el-input ref="Name" v-model="ruleForm.Name"></el-input>
              </el-form-item>
              <el-form-item prop="FloorLevelIndex" :label="$t('FloorLevel')">
                <el-select v-model="ruleForm.FloorLevelIndex" :placeholder="$t('SelectFloorLevel')" :clearable="true">
                  <el-option
                    v-for="item in floorLevel"
                    :key="item.Index"
                    :label="item.Name"
                    :value="item.Index">
                  </el-option>
                </el-select>
              </el-form-item>
              <el-form-item prop="Description"  :label="$t('Description')">
                <el-input ref="Description" type="textarea" :autosize="{ minRows: 3, maxRows: 6 }"
                  v-model="ruleForm.Description" class="InputArea"></el-input>
              </el-form-item>
            </el-form>

            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">
                {{
                    $t("Cancel")
                }}
              </el-button>
              <el-button class="btnOK" type="primary" @click="Submit">{{ $t("OK") }}</el-button>
            </span>
          </el-dialog>

        </div>
        <div>
          <data-table-function-component @insert="Insert" @edit="Edit" @delete="Delete" 
            class="dorm-room__data-table-function" :showButtonColumConfig="true" :gridColumnConfig.sync="columns">
          </data-table-function-component>

          <data-table-component :get-data="getData" ref="table" :selectedRows.sync="rowsObj" :columns="columns"
            class="dorm-room__data-table"
            :isShowPageSize="true"></data-table-component>

        </div>

      </el-main>
    </el-container>
  </div>
</template>
<script src="./dorm-room-component.ts"></script>
<style lang="scss">
.dorm-room__data-table {
  .filter-input {
    margin-right: 10px;
  }
}

.dorm-room__data-table {
  .el-table {
    height: calc(100vh - 174px) !important;
  }
  .el-dropdown {
    height: 32px !important;
    .el-dropdown-link {
      line-height: 32px;
    }
  }
}
</style>
