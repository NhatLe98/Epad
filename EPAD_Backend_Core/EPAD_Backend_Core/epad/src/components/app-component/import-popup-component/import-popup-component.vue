<template>
    <el-dialog :title="title" custom-class="import-dialog" :visible="showDialog"  
        v-loading="isProcess" @close="showHideDialog(false)">
		<div>
            <el-upload
                class="upload-demo"
                :action="linkUpload"
                :headers="paramHeader"
                :on-change="fileChanged"
                :on-success="uploadFileSuccess"
                :before-remove="removeFileSuccess"
                ref="upload"
                :limit="numberOfFiles"
                drag
                :auto-upload="true"
                accept=".xls, .xlsx, .txt">
                <i class="el-icon-upload"></i>
                <div class="el-upload__text">
                    {{$t('DragFileHereOr')}}
                    <em>{{$t('ClickToUpload')}}</em>
                </div>
                 <div class="el-upload__tip" slot="tip">{{ this.$t('AcceptTextFileOrExcelFile') }}</div>
            </el-upload>
            <label class="el-form-item__label">{{ $t('DownloadTemplate') }}</label>
            <br />
            <div>
                <a class="fileTemplate-lbl" href="/Template_AttendanceLog.xlsx" download>{{ $t('Download') }}</a>
            </div>
             
            
            <span style="display: block; margin-top: 20px;">
                {{$t("Result")}}</span>
            <el-input
                style="width: 100%; margin-top: 5px;"
                v-model="txtImportResult"
                type="textarea"
                :rows="6">
            </el-input>
            <div class="dialog-action">
              <el-button type="primary" @click="importProcess" :disabled="listFilePath.length==0" >{{ $t("Upload") }}</el-button>
            </div>
        </div>
	</el-dialog>
</template>
<script src="./import-popup-component.ts"></script>
<style >
    .import-dialog{
        width:600px;
    }
    .upload-demo{
        width:560px;
    }
    .el-upload{
        width:100%;
    }
    .el-upload-dragger{
        width:100%;
    }
    .dialog-action{
        width:100%;
        text-align: right;
        margin-top: 10px;
    }
</style>