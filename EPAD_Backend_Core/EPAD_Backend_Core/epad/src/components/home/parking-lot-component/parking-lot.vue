<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("ParkingLot") }}</span>
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
                    <data-table-component
                        :get-data="getData" ref="parkingLotTable" :columns="columns" :selectedRows.sync="rowsObj"
                        :isShowPageSize="true"></data-table-component>
                </div>
                <!-- dialog insert -->
                <div>
                    <el-dialog :title="isEdit ? $t('Edit') : $t('Insert')" 
                        style="margin-top:20px !important;"
                        custom-class="customdialog" :visible.sync="showDialog" 
                        :before-close="Cancel" :close-on-click-modal="false">
                        <el-form class="formScroll" :model="parkingLotModel" :rules="rules" ref="parkingLotModel" label-width="168px"
                            label-position="top" @keyup.enter.native="Submit">
                            <el-form-item prop="Code" :label="$t('ParkingLotCode')">
                                <el-input v-model="parkingLotModel.Code" placeholder="" :disabled="isEdit"></el-input>
                            </el-form-item>
                            <el-form-item prop="Name" :label="$t('ParkingLotName')">
                                <el-input v-model="parkingLotModel.Name" placeholder=""></el-input>
                            </el-form-item>
                            <el-form-item prop="NameInEng" :label="$t('NameInEng')">
                                <el-input v-model="parkingLotModel.NameInEng" placeholder=""></el-input>
                            </el-form-item>
                            <el-form-item :label="$t('ParkingLotCapacityNumber')">
                                <el-input-number
                                    class="capacity-number"
                                    v-model="parkingLotModel.Capacity"
                                    :min="1"
                                    :max="2147483647"
                                ></el-input-number>
                            </el-form-item>
                            <el-form-item prop="Description" :label="$t('Description')">
                                <el-input v-model="parkingLotModel.Description" placeholder=""></el-input>
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
<script src="./parking-lot.ts"></script>
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