<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t('FrmEmployee') }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent />
                    </el-col>
                </el-row>
            </el-header>
            <el-main class="bgHome">
                <div>
                    <el-dialog :title="isEdit ? $t('EditEmployee') : $t('InsertEmployee')" custom-class="customDialogEmployee" :visible.sync="showDialog" :before-close="Cancel">
                        <el-form class="h-600" :model="ruleForm" :rules="rules" ref="ruleForm" label-width="168px" label-position="top" @keyup.enter.native="Submit">
                            <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
                                <div class="picture-image">
                                    <img v-if="ruleForm.ImageUpload && !errorUpload" :src="'data:image/jpeg;base64, ' + ruleForm.ImageUpload" />
                                </div>
                                <el-form-item :label="$t('UserImage')">
                                    <div class="box">
                                        <input ref="fileImageInput" accept=".png, .jpeg, .jpg" type="file" name="file-3[]" id="fileImageUpload" class="inputfile inputfile-3" @change="processImageFile($event)" />
                                        <label for="fileImageUpload" class="custom-file-upload">
                                            <!-- <svg xmlns="http://www.w3.org/2000/svg" width="20" height="17" viewBox="0 0 20 17">
                                    <path d="M10 0l-5.2 4.9h3.3v5.1h3.8v-5.1h3.3l-5.2-4.9zm9.3 11.5l-3.2-2.1h-2l3.4 2.6h-3.5c-.1 0-.2.1-.2.1l-.8 2.3h-6l-.8-2.2c-.1-.1-.1-.2-.2-.2h-3.6l3.4-2.6h-2l-3.2 2.1c-.4.3-.7 1-.6 1.5l.6 3.1c.1.5.7.9 1.2.9h16.3c.6 0 1.1-.4 1.3-.9l.6-3.1c.1-.5-.2-1.2-.7-1.5z" />
                                </svg> -->
                                            <!-- <span>Choose a file&hellip;</span> -->
                                            <i class="el-icon-upload2"></i>
                                            <span>{{ $t('SelectFileImage') }}</span>
                                        </label>
                                        <button class="capture-camera">
                                            <i class="el-icon-camera"></i> {{$t("CaptureImage")}}
                                        </button>
                                        <span v-if="fileImageName === ''" class="fileImageName">{{ $t('NoFileChoosen') }}</span>
                                        <br />
                                        <span v-if="fileImageName !== ''" class="fileImageName">{{ fileImageName }}</span>
                                        <small style="color:red;font-size:10px;" v-if="errorUpload">{{$t("FileIsNotLarge25Kb")}}</small>
                                        <br />

                                    </div>
                                </el-form-item>
                            </el-col>
                            <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
                                <el-form-item :label="$t('EmployeeATID')" prop="EmployeeATID" @click.native="focus('EmployeeATID')">
                                    <el-input ref="EmployeeATID" v-model="ruleForm.EmployeeATID"></el-input>
                                </el-form-item>
                                <el-form-item :label="$t('EmployeeCode')" @click.native="focus('EmployeeCode')">
                                    <el-input ref="EmployeeCode" v-model="ruleForm.EmployeeCode"></el-input>
                                </el-form-item>
                                <el-form-item :label="$t('FullName')" @click.native="focus('FullName')">
                                    <el-input ref="FullName" v-model="ruleForm.FullName"></el-input>
                                </el-form-item>
                                <el-form-item :label="$t('Gender')">
                                    <el-radio-group v-model="ruleForm.Gender">
                                        <el-radio :label="1">{{ $t('Male') }}</el-radio>
                                        <el-radio :label="2">
                                            {{ $t('Female') }}
                                        </el-radio>
                                    </el-radio-group>
                                </el-form-item>
                                <el-form-item :label="$t('Department')" prop="Department" @click.native="focus('Department')" :placeholder="$t('SelectDepartment')">
                                    <el-select ref="Department" v-model="ruleForm.DepartmentIndex" :disabled="isDepartMent" >
                                        <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value"></el-option>
                                    </el-select>
                                </el-form-item>

                            </el-col>
                            <el-col :xs="12" :sm="8" :md="8" :lg="8" :xl="8" class="p-20">
                                <el-form-item :label="$t('NameOnMachine')" @click.native="focus('NameOnMachine')">
                                    <el-input ref="NameOnMachine" v-model="ruleForm.NameOnMachine"></el-input>
                                </el-form-item>
                                <el-form-item :label="$t('Password')" @click.native="focus('Password')">
                                    <el-input ref="Password" v-model="ruleForm.Password" type="password"></el-input>
                                </el-form-item>
                                <el-form-item :label="$t('CardNumber')" prop="CardNumber" @click.native="focus('CardNumber')">
                                    <el-input ref="CardNumber" v-model="ruleForm.CardNumber"></el-input>
                                </el-form-item>
                                <el-form-item :label="$t('Finger')">
                                    <el-button class="register-biometrics" @click="showOrHideRegisterFingerDialog">
                                        <i class="el-icon-thumb"></i> {{$t("Register")}}
                                    </el-button>
                                </el-form-item>
                                <el-form-item :label="$t('JoinedDate')" prop="JoinedDate">
                                    <el-date-picker :disabled="isJoinedDate" ref="JoinedDate" v-model="ruleForm.JoinedDate" type="date"></el-date-picker>
                                </el-form-item>
                            </el-col>
                            <div class="button-footer">
                                <span slot="footer" class="dialog-footer custom-center">
                                    <el-button class="btnCancel" @click="Cancel">
                                        {{ $t('Cancel') }}
                                    </el-button>
                                    <el-button class="btnOK" type="primary" @click="submitObj">
                                        {{ $t('OK') }}
                                    </el-button>
                                </span>
                            </div>
                        </el-form>
                    </el-dialog>
                </div>
                <div>
                    <el-dialog :title="$t('DialogHeaderTitle')" custom-class="customdialog" :visible.sync="showDialogImportError" @close="showOrHideImportError(false)">
                        <el-form label-width="168px" label-position="top">
                            <el-form-item >
                                <div class="box">
                                    <label>
                                        <span>{{ importErrorMessage }}</span>
                                    </label>
                                </div>
                            </el-form-item>
                            <el-form-item >
                                <a class="fileTemplate-lbl" href="/Files/EmployeesImportError.xlsx" download>{{ $t('Download') }}</a>
                            </el-form-item>
                        </el-form>

                        <span slot="footer" class="dialog-footer">
                            <el-button class="btnOK" type="primary" @click="showOrHideImportError(false)">
                                OK
                            </el-button>
                        </span>
                    </el-dialog>
                </div>
                <div>

                    <data-table-function-component @insert="Insert"
                                                   @edit="Edit"
                                                   @delete="showOrHideDialogDeleteUser(true)"
                                                   @add-excel="AddOrDeleteFromExcel('add')"
                                                   @delete-excel="AddOrDeleteFromExcel('delete')"
                                                   @export-excel="ExportToExcel"
                                                   v-bind:listExcelFunction="listExcelFunction"></data-table-function-component>

                </div>
                <div>

                    <el-select 
                               v-model="filterDepartmentId"
                               :placeholder="$t('SelectDepartment')"
                               @change="onChangeDepartment"
                               multiple 
                               filterable
                               clearable
                               default-first-option
                               style="float: left; margin-right:10px; ">
                        <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value"></el-option>
                    </el-select>
                    <data-table-component :get-data="getData" ref="table" :columns="columns" :selectedRows.sync="rowsObj" :isShowPageSize="true"></data-table-component>
                </div>
                <div>
                    <el-dialog :title="isAddFromExcel ? $t('AddFromExcel') : $t('DeleteFromExcel')" custom-class="customdialog" :visible.sync="showDialogExcel" @close="AddOrDeleteFromExcel('close')">
                        <el-form :model="formExcel" ref="formExcel" label-width="168px" label-position="top">
                            <el-form-item :label="$t('SelectFile')">
                                <div class="box">
                                    <input ref="fileInput" accept=".xls, .xlsx" type="file" name="file-3[]" id="fileUpload" class="inputfile inputfile-3" @change="processFile($event)" />
                                    <label for="fileUpload">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="17" viewBox="0 0 20 17">
                                            <path d="M10 0l-5.2 4.9h3.3v5.1h3.8v-5.1h3.3l-5.2-4.9zm9.3 11.5l-3.2-2.1h-2l3.4 2.6h-3.5c-.1 0-.2.1-.2.1l-.8 2.3h-6l-.8-2.2c-.1-.1-.1-.2-.2-.2h-3.6l3.4-2.6h-2l-3.2 2.1c-.4.3-.7 1-.6 1.5l.6 3.1c.1.5.7.9 1.2.9h16.3c.6 0 1.1-.4 1.3-.9l.6-3.1c.1-.5-.2-1.2-.7-1.5z" />
                                        </svg>
                                        <!-- <span>Choose a file&hellip;</span> -->
                                        <span>{{ $t('ChooseAExcelFile') }}</span>
                                    </label>
                                    <span v-if="fileName === ''" class="fileName">{{ $t('NoFileChoosen') }}</span>
                                    <span v-else class="fileName">{{ fileName }}</span>

                                </div>
                            </el-form-item>
                            <el-form-item :label="$t('DownloadTemplate')">
                                <a class="fileTemplate-lbl" href="/Template_IC_Employee.xlsx" download>{{ $t('Download') }}</a>
                            </el-form-item>
                            <el-form-item v-if="isDeleteFromExcel === true">
                                <el-checkbox v-model="isDeleteOnDevice">{{$t("DeleteEmployeeOnDeviceHint")}}</el-checkbox>
                            </el-form-item>
                        </el-form>

                        <span slot="footer" class="dialog-footer">
                            <el-button class="btnCancel" @click="AddOrDeleteFromExcel('close')">
                                {{ $t('Cancel') }}
                            </el-button>
                            <el-button v-if="isAddFromExcel" class="btnOK" type="primary" @click="UploadDataFromExcel">
                                {{ $t('OK') }}
                            </el-button>
                            <el-button v-else class="btnOK" type="primary" @click="DeleteDataFromExcel">
                                {{ $t('OK') }}
                            </el-button>
                        </span>
                    </el-dialog>
                </div>
                <div>
                    <el-dialog :title="$t('DialogOption')"
                               custom-class="customdialog"
                               :visible.sync="showDialogDeleteUser"
                               :before-close="cancelDialogDeleteUser">
                        <el-form label-width="168px"
                                 label-position="top">
                            <div style="margin-bottom:20px">
                                <i style="font-weight:bold; font-size:larger; color:orange" class="el-icon-warning-outline" />
                                <span style="font-weight:bold">{{ $t('DeleteEmployeeCofirm')}}</span>
                            </div>
                            <el-form-item>
                                <el-checkbox v-model="isDeleteOnDevice">{{$t("DeleteEmployeeOnDeviceHint")}}</el-checkbox>
                            </el-form-item>
                        </el-form>
                        <span slot="footer" class="dialog-footer">
                            <el-button class="btnCancel"
                                       @click="showOrHideDialogDeleteUser(false)">
                                {{ $t("Cancel") }}
                            </el-button>
                            <el-button type="primary"
                                       @click="Delete">
                                {{ $t("OK") }}
                            </el-button>

                        </span>
                    </el-dialog>
                </div>
                <div>
                    <el-dialog :title="$t('DialogOption')"
                               custom-class="customDialogEmployee"
                               :visible.sync="showRegisterFingerDialog"
                               :before-close="cancelRegisterFingerDialog">
                        <el-form label-position="top" label-width="168px">
                            <el-row>
                                <el-col :span="3" style="margin-right:55px" v-for="item in listFinger" :key="item.ID">
                                    <el-form-item :label="$t('Finger'+ item.ID)" v-if="item.ID < 6">
                                        <el-card style="cursor:pointer" v-bind:class="{ 'has-focus': item.FocusFinger}">
                                            <img style="height:80px; width:55px" @click="onFocusFinger(item.ID)" v-bind:src="item.ImageFinger || getImgUrl('base_fpVerify_clearImage.png')" class="image">
                                        </el-card>
                                    </el-form-item>
                                </el-col>
                            </el-row>
                            <el-row>
                                <el-col :span="3" style="margin-right:55px" v-for="item in listFinger" :key="item.ID">
                                    <el-form-item :label="$t('Finger'+ item.ID)" v-if="item.ID > 5">
                                        <el-card style="cursor:pointer" v-bind:class="{ 'has-focus': item.FocusFinger}">
                                            <img style="height:80px; width:55px" @click="onFocusFinger(item.ID)" :src="item.ImageFinger || getImgUrl('base_fpVerify_clearImage.png')" class="image">
                                        </el-card>
                                    </el-form-item>
                                </el-col>
                            </el-row>
                        </el-form>
                        <span slot="footer" class="dialog-footer">

                            <h4>
                                <span style="float:left;">
                                    {{ ConnectedDevice ? $t("ConnectedFingerDevice") : $t("NotConnectedDevice")}}
                                </span>
                            </h4>
                            <el-button class="" @click="reconnect">
                                {{$t('ReConnectDevice')}}
                            </el-button>
                            <!--<el-button class="" @click="closedev">
                    {{$t('DisConnectDevice')}}
                </el-button>-->

                            <el-button class="btnCancel" @click="cancelRegisterFingerDialog">
                                {{ $t('Cancel') }}
                            </el-button>
                            <el-button class="btnOK" type="primary" @click="submitRegisterFinger">
                                {{ $t('OK') }}
                            </el-button>
                        </span>
                    </el-dialog>
                </div>
            </el-main>
        </el-container>

    </div>
</template>
<script src="./employee-component.ts"></script>
<style>
    .filterCompononet div > input {
        height: 36px !important;
    }

    .datatable-function {
        width: calc(100% - 530px) !important;
    }

    .customDialogEmployee {
        width: 800px;
    }

    .h-600 {
        height: 600px;
    }

    .p-20 {
        padding: 20px;
    }

    .button-footer {
        text-align: center;
        width: 100%;
        display: block;
        float: left;
    }

    .custom-file-upload {
        border-radius: 4px;
        border: 1px solid #DCDFE6;
        height: 24px;
        padding-left: 10px;
        padding-right: 10px;
    }
    .custom-file-upload span {
        color: #606266 !important;
        font-size: 11px !important;
        font-weight: 700;
    }

    .custom-file-upload i {
        color: #606266 !important;
        font-size: 11px !important;
        margin-right: 5px;
    }
    .capture-camera {
        height: 24px;
        background: #fff;
        border-radius: 4px;
        border: 1px solid #DCDFE6;
        color: #606266;
        cursor: pointer;
        font-size: 11px;
        font-weight: 700;
        padding-left: 10px;
        padding-right: 10px;
        padding-bottom: 0px;
        padding-top: 4px;
        margin-left: 4px;
    }   
    .capture-camera i {
        margin-right: 5px;
    }
    .register-biometrics {
        height: 30px;
        background: #fff;
        border-radius: 4px;
        border: 1px solid #DCDFE6;
        color: #606266;
        cursor: pointer;
        font-size: 11px;
        font-weight: 700;
        padding-left: 10px;
        padding-right: 10px;
    }
    .register-biometrics i {
        margin-right: 5px;
    }

    .picture-image{
        display: block;
        width: 100px;
        height: 120px;
        border: 1px solid #DCDFE6;
        padding:2px;
    }
    .picture-image img{
        object-fit: contain;
        width: 100%;
        height: 100%;
    }
        
    .has-focus {
        border: cornflowerblue solid 3px;
    }
</style>
