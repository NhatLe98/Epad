<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("AccessedGroup") }}</span>
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
                    <data-table-component :get-data="getData" ref="accessedGroupTable" :columns="columns"
                        :selectedRows.sync="rowsObj" :isShowPageSize="true"></data-table-component>
                </div>
                <!-- dialog insert -->
                <div>
                    <el-dialog :title="isEdit ? $t('Edit') : $t('Insert')" style="margin-top:20px !important;"
                        custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel"
                        :close-on-click-modal="false">
                        <el-form class="formScroll" :model="accessedGroupModel" :rules="rules" ref="accessedGroupModel"
                            label-width="168px" label-position="top" @keyup.enter.native="Submit">
                            <el-form-item prop="Name" :label="$t('AccessedGroupName')">
                                <el-input v-model="accessedGroupModel.Name" placeholder=""></el-input>
                            </el-form-item>
                            <el-form-item prop="NameInEng" :label="$t('NameInEng')">
                                <el-input v-model="accessedGroupModel.NameInEng" placeholder=""></el-input>
                            </el-form-item>
                            <el-form-item prop="GeneralAccessRuleIndex" :label="$t('GeneralAccessRules')">
                                <el-select style="width: 100%" v-model="accessedGroupModel.GeneralAccessRuleIndex"
                                    filterable :clearable="true" :placeholder="$t('GeneralAccessRules')">
                                    <el-option v-for="item in listGeneralAccessRules" :key="item.Index"
                                        :label="item.Name" :value="item.Index"></el-option>
                                </el-select>
                            </el-form-item>
                            <!-- <el-form-item prop="ParkingLotRuleIndex" :label="$t('ParkingLotRules')">
                                <el-select style="width: 100%" v-model="accessedGroupModel.ParkingLotRuleIndex" filterable :clearable="true"
                                     :placeholder="$t('ParkingLotRules')">
                                    <el-option v-for="item in listParkingLotRules" :key="item.Index" :label="item.Name"
                                        :value="item.Index"></el-option>
                                </el-select>
                            </el-form-item> -->
                            <el-form-item prop="Description" :label="$t('Description')">
                                <el-input v-model="accessedGroupModel.Description" placeholder=""></el-input>
                            </el-form-item>
                            <el-form-item >
                                <el-checkbox v-model="accessedGroupModel.IsGuestDefaultGroup">{{
                                                        $t("GuestDefaultGroup")
                                                    }}</el-checkbox>
                            </el-form-item>
                            <el-form-item >
                                <el-checkbox v-model="accessedGroupModel.IsDriverDefaultGroup">{{
                                                        $t("DriverDefaultGroup")
                                                    }}</el-checkbox>
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
<script src="./accessed-group.ts"></script>
<style lang="scss">
.formScroll {
    height: 55vh;
    overflow-y: auto;
}

.el-dialog {
    margin-top: 20px !important;
}
</style>