<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("CustomerDriverCard") }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent />
                    </el-col>
                </el-row>
            </el-header>
            <el-main class="bgHome customer-card__main">
                <div>
                    <data-table-function-component :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
                        :isHiddenEdit="true"
                        @insert="Insert" @delete="Delete" 
                        :listExcelFunction="listExcelFunction"
                        @add-excel="AddOrDeleteFromExcel('add')"
                        @custombuttonclick="syncDataToDevice"
                        v-bind:showButtonCustom="true"
                        v-bind:buttonCustomText="$t('SyncData')"
                        v-bind:buttonCustomIcon="'Edit'"
                        >
                    </data-table-function-component>
                    <data-table-component
                        :get-data="getData" ref="customerCardTable" :columns="columns" :selectedRows.sync="rowsObj"
                        :isShowPageSize="true"></data-table-component>
                </div>
                <!-- dialog insert -->
                <div>
                    <el-dialog :title="isEdit ? $t('Edit') : $t('AddCustomer')" 
                        style="margin-top:20px !important;"
                        custom-class="customdialog" :visible.sync="showDialog" 
                        :before-close="Cancel" :close-on-click-modal="false">
                        <el-form :model="customerCardModel" :rules="rules" ref="customerCardModel" label-width="168px"
                            label-position="top">
                            <el-form-item prop="CardNumber" :label="$t('CardNumber')">
                                <el-input v-model="customerCardModel.CardNumber" placeholder="" :disabled="isEdit"
                                onkeypress="return event.charCode >= 48 && event.charCode <= 57"
                                ></el-input>
                            </el-form-item>
                            <el-checkbox v-model="customerCardModel.IsSyncToDevice">{{ $t('SyncCardToDevice') }}                                 
                            </el-checkbox>
                            <br/><span style="font-size: 12px;">({{ $t('lineForCustomerOrDriver') }})</span>
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

                <el-dialog :title="isAddFromExcel ? $t('AddFromExcel') : $t('DeleteFromExcel')" custom-class="customdialog"
                    :visible="showDialogExcel && !isChoose" @close="AddOrDeleteFromExcel('close')">
                    <el-form :model="formExcel" ref="formExcel" label-width="168px" label-position="top">
                        <el-form-item :label="$t('SelectFile')">
                            <div class="box">
                                <input ref="fileInput"
                                    accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
                                    type="file" name="file-3[]" id="fileUpload" class="inputfile inputfile-3"
                                    @change="processFile($event)" />
                                <label for="fileUpload">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="17" viewBox="0 0 20 17">
                                        <path
                                            d="M10 0l-5.2 4.9h3.3v5.1h3.8v-5.1h3.3l-5.2-4.9zm9.3 11.5l-3.2-2.1h-2l3.4 2.6h-3.5c-.1 0-.2.1-.2.1l-.8 2.3h-6l-.8-2.2c-.1-.1-.1-.2-.2-.2h-3.6l3.4-2.6h-2l-3.2 2.1c-.4.3-.7 1-.6 1.5l.6 3.1c.1.5.7.9 1.2.9h16.3c.6 0 1.1-.4 1.3-.9l.6-3.1c.1-.5-.2-1.2-.7-1.5z" />
                                    </svg>
                                    <!-- <span>Choose a file&hellip;</span> -->
                                    <span>{{ $t("ChooseAExcelFile") }}</span>
                                </label>
                                <span v-if="fileName === ''" class="fileName">{{
                                    $t("NoFileChoosen")
                                }}</span>
                                <span v-else class="fileName">{{ fileName }}</span>
                            </div>
                        </el-form-item>
                        <el-form-item :label="$t('DownloadTemplate')">
                            <a class="fileTemplate-lbl" href="/Template_HR_CustomerCard.xlsx" download>{{ $t("Download") }}</a>
                        </el-form-item>
                    </el-form>

                    <span slot="footer" class="dialog-footer">
                        <el-button class="btnCancel" @click="AddOrDeleteFromExcel('close')">
                            {{ $t("Cancel") }}
                        </el-button>
                        <el-button class="btnOK" type="primary" @click="UploadDataFromExcel">
                            {{ $t("OK") }}
                        </el-button>
                    </span>
                </el-dialog>

                <el-dialog :title="$t('DialogHeaderTitle')" custom-class="customdialog" 
                :visible.sync="showDialogImportError" :close-on-click-modal="false"
                    @close="showOrHideImportError(false)">
                    <el-form label-width="168px" label-position="top">
                        <el-form-item>
                            <div class="box">
                                <label>
                                    <span>{{ importErrorMessage }}</span>
                                </label>
                            </div>
                        </el-form-item>
                        <el-form-item>
                            <a class="fileTemplate-lbl" href="/Files/Template_HR_CustomerCard_Error.xlsx" download>{{ $t('Download')
                            }}</a>
                        </el-form-item>
                    </el-form>

                    <span slot="footer" class="dialog-footer">
                        <el-button class="btnOK" type="primary" @click="showOrHideImportError(false)">
                            OK
                        </el-button>
                    </span>
                </el-dialog>
            </el-main>
        </el-container>
    </div>
</template>
<script src="./customer-card-component.ts"></script>
<style lang="scss">
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

.customer-card__main {
    .popupshowcol{
        width: 260px !important;
    }
}
</style>