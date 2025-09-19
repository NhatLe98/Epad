<template>
    <div class="tab">
        <div class="tab-filter">
            <el-row>
                <span>Lọc danh sách</span>
                <el-select v-model="filterModel.SelectedClass"
                           style="width: 300px; padding: 0 10px"
                           :multiple="true"
                           clearable
                           collapse-tags>
                    <el-option v-for="item in allClass"
                               :key="item.Index"
                               :label="item.Name"
                               :value="item.Index">
                    </el-option>
                </el-select>
                <el-input style="padding-bottom: 3px; width: 238px; padding-right: 5px"
                          :placeholder="$t('SearchData')"
                          suffix-icon="el-icon-search"
                          v-model="filterModel.TextboxSearch"
                          @keyup.enter.native="onViewClick"
                          class="filter-input"></el-input>
                <el-button type="primary"
                           class="smallbutton"
                           size="small"
                           @click="onViewClick">
                    {{ $t("View") }}
                </el-button>
                <el-dropdown style="margin-left: 10px; margin-top: 5px"
                             @command="handleCommand"
                             trigger="click">
                    <span class="el-dropdown-link" style="font-weight: bold">
                        . . .<span class="more-text">{{ $t("More") }}</span>
                    </span>

                    <el-dropdown-menu slot="dropdown">
                        <el-dropdown-item v-for="(item, index) in listExcelFunction"
                                          :key="index"
                                          :command="item">
                            {{ $t(item) }}
                        </el-dropdown-item>
                    </el-dropdown-menu>
                </el-dropdown>
            </el-row>
        </div>

        <div class="tab-grid">
            <t-grid @onPageChange="onPageChange"
                    @onPageSizeChange="onPageSizeChange"
                    @viewDetailPopup="viewDetailPopup"
                    :gridColumns="gridColumns"
                    :dataSource="dataSource"
                    :selectedRows.sync="selectedRows"
                    :total="total"
                    :page.sync="page"
                    :pageSize.sync="pageSize"></t-grid>
        </div>

        <div class="tab-modal">
            <el-dialog custom-class="customdialog"
                       width="1000px"
                       :title="isEdit ? $t('EditStudent') : $t('InsertStudunt')"
                       :visible.sync="showDialog"
                       :close-on-click-modal="false"
                       :before-close="onCancelClick">
                <el-form class="h-600"
                         :model="formModel"
                         :rules="formRules"
                         ref="StudentformModel"
                         label-width="168px"
                         label-position="top">
                    <el-row>
                        <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
                            <el-form-item :label="$t('UserImage')">
                                <el-upload class="avatar-uploader"
                                           action=""
                                           accept="image/png, image/jpeg"
                                           :multiple="false"
                                           :file-list="fileList"
                                           :auto-upload="false"
                                           :on-change="onChangeAvatar"
                                           :on-remove="onRemoveAvatar">
                                    <img class="avatar"
                                         v-if="formModel.Avatar && !errorUpload"
                                         :src="'data:image/jpeg;base64, ' + formModel.Avatar" />
                                    <i v-else
                                       class="el-icon-plus avatar-uploader-icon"
                                       style="width: 100%"></i>
                                </el-upload>
                            </el-form-item>
                        </el-col>
                        <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
                            <el-form-item :label="$t('UserCode')" prop="EmployeeATID">
                                <el-input v-model="formModel.EmployeeATID"
                                          :disabled="isEdit"></el-input>
                            </el-form-item>

                            <el-form-item :label="$t('Password')">
                                <el-input v-model="formModel.Password"
                                          type="password"></el-input>
                            </el-form-item>

                            <el-form-item :label="$t('Gender')">
                                <el-radio-group v-model="formModel.Gender">
                                    <el-radio :label="1">{{ $t("Male") }}</el-radio>
                                    <el-radio :label="0">{{ $t("Female") }}</el-radio>
                                </el-radio-group>
                            </el-form-item>

                            <el-form-item :label="$t('Class')"
                                          prop="ClassID"
                                          :placeholder="$t('SelectClass')">
                                <el-select v-model="formModel.ClassID" :disabled="isEdit">
                                    <el-option v-for="item in allClass"
                                               :key="item.Index"
                                               :label="item.Name"
                                               :value="item.Index"
                                               :disabled="isEdit">
                                    </el-option>
                                </el-select>
                            </el-form-item>
                        </el-col>

                        <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
                            <el-form-item :label="$t('NameOnMachine')">
                                <el-input v-model="formModel.NameOnMachine"></el-input>
                            </el-form-item>

                            <el-form-item :label="$t('CardNumber')" prop="CardNumber">
                                <el-input v-model="formModel.CardNumber"></el-input>
                            </el-form-item>

                            <el-form-item :label="$t('Fingerprint')">
                                <el-button class="register-biometrics"
                                           @click="showFingerDialog = true">
                                    <i class="el-icon-thumb"></i> {{ $t("Register") }}
                                </el-button>
                            </el-form-item>

                            <el-form-item :label="$t('FullName')" prop="FullName">
                                <el-input ref="FullName"
                                          v-model="formModel.FullName"></el-input>
                            </el-form-item>
                        </el-col>
                    </el-row>
                </el-form>
                <span slot="footer" class="dialog-footer">
                    <el-button class="btnCancel" @click="onCancelClick">
                        {{
            $t("Cancel")
                        }}
                    </el-button>
                    <el-button class="btnOK" type="primary" @click="onSubmitClick">
                        {{
            $t("OK")
                        }}
                    </el-button>
                </span>
            </el-dialog>
        </div>
        <div>
            <el-dialog :title="$t('DialogHeaderTitle')" custom-class="customdialog" :visible.sync="showDialogImportError" @close="showOrHideImportError(false)">
                <el-form label-width="168px" label-position="top">
                    <el-form-item>
                        <div class="box">
                            <label>
                                <span>{{ importErrorMessage }}</span>
                            </label>
                        </div>
                    </el-form-item>
                    <el-form-item>
                        <a class="fileTemplate-lbl" href="/Files/StudentImportError.xlsx" download>{{ $t('Download') }}</a>
                    </el-form-item>
                </el-form>

                <span slot="footer" class="dialog-footer">
                    <el-button class="btnOK" type="primary" @click="showOrHideImportError(false)">
                        OK
                    </el-button>
                </span>
            </el-dialog>
        </div>
        <div class="tab-modal-delete">
            <el-dialog custom-class="customdialog"
                       :title="$t('DialogOption')"
                       :visible.sync="showDialogDeleteUser"
                       :before-close="cancelDeleteUser">
                <el-form label-width="168px" label-position="top">
                    <div style="margin-bottom: 20px">
                        <i style="font-weight: bold; font-size: larger; color: orange"
                           class="el-icon-warning-outline" />
                        <span style="font-weight: bold">
                            {{
              $t("DeleteEmployeeCofirm")
                            }}
                        </span>
                    </div>
                    <el-form-item>
                        <el-checkbox v-model="isDeleteOnDevice">
                            {{
              $t("DeleteEmployeeOnDeviceHint")
                            }}
                        </el-checkbox>
                    </el-form-item>
                </el-form>
                <span slot="footer" class="dialog-footer">
                    <el-button class="btnCancel" @click="showDialogDeleteUser = false">
                        {{ $t("Cancel") }}
                    </el-button>
                    <el-button type="primary" @click="doDelete">
                        {{ $t("OK") }}
                    </el-button>
                </span>
            </el-dialog>
        </div>

        <div class="tab-modal-excel">
            <!--Dialog chosse excel-->
            <el-dialog :title="isAddFromExcel ? $t('AddFromExcel') : $t('DeleteFromExcel')"
                       custom-class="customdialog"
                       :visible.sync="showDialogExcel"
                       @close="AddOrDeleteFromExcel('close')">
                <el-form :model="formExcel"
                         ref="formExcel"
                         label-width="168px"
                         label-position="top">
                    <el-form-item :label="$t('SelectFile')">
                        <div class="box">
                            <input ref="fileInput"
                                   accept=".xls, .xlsx"
                                   type="file"
                                   name="file-3[]"
                                   id="fileStudentUpload"
                                   class="inputfile inputfile-3"
                                   @change="processFile($event)" />
                            <label for="fileStudentUpload">
                                <svg xmlns="http://www.w3.org/2000/svg"
                                     width="20"
                                     height="17"
                                     viewBox="0 0 20 17">
                                    <path d="M10 0l-5.2 4.9h3.3v5.1h3.8v-5.1h3.3l-5.2-4.9zm9.3 11.5l-3.2-2.1h-2l3.4 2.6h-3.5c-.1 0-.2.1-.2.1l-.8 2.3h-6l-.8-2.2c-.1-.1-.1-.2-.2-.2h-3.6l3.4-2.6h-2l-3.2 2.1c-.4.3-.7 1-.6 1.5l.6 3.1c.1.5.7.9 1.2.9h16.3c.6 0 1.1-.4 1.3-.9l.6-3.1c.1-.5-.2-1.2-.7-1.5z" />
                                </svg>
                                <!-- <span>Choose a file&hellip;</span> -->
                                <span>{{ $t("ChooseAExcelFile") }}</span>
                            </label>
                            <span v-if="fileName === ''" class="fileName">
                                {{
                $t("NoFileChoosen")
                                }}
                            </span>
                            <span v-else class="fileName">{{ fileName }}</span>
                        </div>
                    </el-form-item>
                    <el-form-item :label="$t('DownloadTemplate')">
                        <a class="fileTemplate-lbl"
                           href="/Template_HR_Student.xlsx"
                           download>{{ $t("Download") }}</a>
                    </el-form-item>
                    <el-form-item v-if="isDeleteFromExcel === true">
                        <el-checkbox v-model="isDeleteOnDevice">
                            {{
              $t("DeleteEmployeeOnDeviceHint")
                            }}
                        </el-checkbox>
                    </el-form-item>
                </el-form>

                <span slot="footer" class="dialog-footer">
                    <el-button class="btnCancel" @click="AddOrDeleteFromExcel('close')">
                        {{ $t("Cancel") }}
                    </el-button>
                    <el-button v-if="isAddFromExcel"
                               class="btnOK"
                               type="primary"
                               @click="UploadDataFromExcel">
                        {{ $t("OK") }}
                    </el-button>
                    <el-button v-else
                               class="btnOK"
                               type="primary"
                               @click="DeleteDataFromExcel">
                        {{ $t("OK") }}
                    </el-button>
                </span>
            </el-dialog>
        </div>
        <div>
            <el-dialog :title="$t('DialogHeaderTitle')" :visible.sync="showDetailPopup" @close="closeDetailPopup(false)" class="view-detail-popup">
                <t-grid  :gridColumns="gridColumnsDetail" :dataSource="dataSourceDetail" :showFooter="false"></t-grid>
            </el-dialog>
        </div>
    </div>
</template>
<script src="./hr-student-info.ts"></script>