<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("EmployeeType") }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent />
                    </el-col>
                </el-row>
            </el-header>
            <el-main class="bgHome">
                <div>
                    <data-table-function-component :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
                        @insert="Insert" @edit="Edit" @delete="Delete">
                    </data-table-function-component>
                    <data-table-component v-if="showTable"
                        :get-data="getData" ref="employeeTypeTable" :columns="columns" :selectedRows.sync="rowsObj"
                        :isShowPageSize="true"></data-table-component>
                </div>
                <!-- dialog insert -->
                <div>
                    <el-dialog :title="isEdit ? $t('Edit') : $t('Insert')" style="margin-top:20px !important;"
                        custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel"
                        :close-on-click-modal="false">
                        <el-form class="formScroll" :model="employeeTypeModel" :rules="rules" ref="employeeTypeModel" 
                            label-width="168px"
                            label-position="top" @keyup.enter.native="Submit">
                            <el-form-item prop="Code" :label="$t('EmployeeTypeCode')">
                                <el-input v-model="employeeTypeModel.Code" placeholder=""></el-input>
                            </el-form-item>
                            <el-form-item prop="Name" :label="$t('Name')">
                                <el-input v-model="employeeTypeModel.Name" placeholder=""></el-input>
                            </el-form-item>
                            <el-form-item prop="NameInEng" :label="$t('NameInEng')">
                                <el-input v-model="employeeTypeModel.NameInEng" placeholder=""></el-input>
                            </el-form-item>
                            <el-form-item>
                                <el-checkbox
                                    class="employee-type__is-using"
                                    v-model="employeeTypeModel.IsUsing"
                                ><span>{{ $t('IsUsing') }}</span></el-checkbox>
                            </el-form-item>
                            <el-form-item prop="Description" :label="$t('Description')">
                                <el-input v-model="employeeTypeModel.Description" placeholder=""></el-input>
                            </el-form-item>
                        </el-form>
                        <span slot="footer" class="dialog-footer">
                            <el-button class="btnCancel" @click="Cancel">
                                {{ $t('Cancel') }}
                            </el-button>
                            <el-button class="btnOK" type="primary" @click="ConfirmClick">
                                {{ $t('OK') }}
                            </el-button>
                        </span>


                    </el-dialog>
                </div>
            </el-main>
        </el-container>
    </div>
</template>
<script src="./employee-type.ts"></script>
<style lang="scss">
.formScroll {
    height: 55vh;
    overflow-y: auto;
}

.el-dialog {
    margin-top: 20px !important;
}

.capacity-number {
  width: 100%;

  .el-input__inner {
    text-align: left !important;
  }

  .el-input-number__increase,
  .el-input-number__decrease {
    height: 50px;
    border-radius: 10px;
    i {
        margin-top: 20px;
    }
  }
}
</style>