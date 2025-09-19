<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("AreaGroup") }}</span>
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
                        :get-data="getData" ref="areaGroupTable" :columns="columns" :selectedRows.sync="rowsObj"
                        :isShowPageSize="true"></data-table-component>
                </div>
                <!-- dialog insert -->
                <div>
                    <el-dialog :title="isEdit ? $t('Edit') : $t('Insert')" style="margin-top:20px !important;"
                        custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">
                        <el-form class="formScroll" :model="areaGroupModel" :rules="rules" ref="areaGroupModel" label-width="168px"
                            label-position="top" @keyup.enter.native="Submit">
                            <el-form-item prop="Code" :label="$t('AreaGroupCode')">
                                <el-input v-model="areaGroupModel.Code" placeholder="" :disabled="isEdit"></el-input>
                            </el-form-item>
                            <el-form-item prop="Name" :label="$t('AreaGroupName')">
                                <el-input v-model="areaGroupModel.Name" placeholder=""></el-input>
                            </el-form-item>
                            <el-form-item prop="NameInEng" :label="$t('NameInEng')">
                                <el-input v-model="areaGroupModel.NameInEng" placeholder=""></el-input>
                            </el-form-item>
                            <el-form-item :label="$t('MachineGroup')">
                                <el-select style="width: 100%" v-model="selectedGroupDevice" filterable :clearable="true"
                                    multiple :placeholder="$t('SelectGroupDevice')">
                                    <el-option v-for="item in listGroupDevice" :key="item.Index" :label="item.Name"
                                        :value="item.Index"></el-option>
                                </el-select>
                            </el-form-item>
                            <el-form-item prop="Description" :label="$t('Description')">
                                <el-input v-model="areaGroupModel.Description" placeholder=""></el-input>
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
<script src="./area-group.ts"></script>
<style lang="scss">
.formScroll {
    height: 55vh;
    overflow-y: auto;
}

.el-dialog {
    margin-top: 20px !important;
}
</style>