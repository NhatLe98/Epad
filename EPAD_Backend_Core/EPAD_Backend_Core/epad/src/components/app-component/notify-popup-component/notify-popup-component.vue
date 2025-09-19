<template>
    <el-dialog :visible="showDialog" :show-close="false">
        <div class="notifydialogheader">
            <div>
                <span style="font-size: 15px; line-height: 20px; color: #000000; margin-left: 10px; ">
                    {{title}}
                </span>
            </div>
            <div>
                <el-button @click="DeleteAll" size="mini" type="danger" style="margin-right:10px" round>{{ $t('DeleteAll') }}</el-button>
                <a class="btnClose" style="cursor:pointer" @click="showHideDialog(false)">{{ $t('Close') }}</a>
            </div>
        </div>
        <!--<div>
            <template>
                <el-checkbox @change="handleCheckAllChange" style="margin-left:10px"><span style="margin-right: 10px; font-weight: bold; font-size: 12px; line-height: 16px;color: #333333;">{{ $t('SelectAll') }}</span></el-checkbox>
            </template>
        </div>-->
        <div class="notifydialogbody">
            <div class="viewdetailwaitinglist" v-loading="loading">
                <template v-if="!loading" class="infinite-list" v-infinite-scroll="load" style=" overflow: auto">
                    <div style="padding-top:10px ;" v-for="(item, index) in listNotification" :key="index">
                        <div class="demo-shadow" style="margin:0 5px 0 5px; box-shadow: 0 2px 4px rgba(0, 0, 0, .12), 0 0 6px rgba(0, 0, 0, .04)">
                            <el-row style="margin:2px 5px 2px 5px">
                                <!--<el-col :span="1">
                                    <el-checkbox v-model="item.IsChecked" style="margin-top:20px"></el-checkbox>
                                </el-col>-->
                                <el-col :span="21">
                                    <div style="padding-top:10px;  padding-bottom:10px">
                                        <router-link :to="item.RouteURL" style="cursor:pointer; font-size: 16px; font-weight:bold "> {{ item.Data.FormatMessage }}</router-link>
                                    </div>
                                    <div style="">
                                        <span v-if="item.Type === 0" style="font-size: 16px">{{ $t('DialogNotifyRequester') }} :  {{ item.Data.FromUser }}</span>
                                        <span v-if="item.Type !== 0" style="font-size: 16px">{{ $t('DialogNotifyApprover') }} :  {{ item.Data.Approver }}</span>
                                    </div>
                                    <div style="padding-top:10px; padding-bottom:10px">
                                        <span v-if="item.Type === 0" style="font-size: 16px">{{ $t('DialogNotifyRequestDate') }} : {{ formatDateField(item.Data.FromDate ) }}</span>
                                        <span v-if="item.Type !== 0" style="font-size: 16px">{{ $t('DialogNotifyApproveDate') }} : {{ formatDateField(item.Data.ApproveDate ) }}</span>
                                    </div>
                                </el-col>
                                <el-col :span="2" class="flex-col-center-end" style="margin-top:20px">
                                    <el-button @click="deleteNotification(index)" type="danger" icon="el-icon-delete" circle></el-button>
                                </el-col>
                            </el-row>
                        </div>
                    </div>
                </template>

            </div>
        </div>
    </el-dialog>
</template>
<script src="./notify-popup-component.ts"></script>
<style lang="scss">

    .notifydialogheader {
        border-bottom: 0.5px solid #828282;
        padding: 0;
        width: 100%;
        height: 50px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-top: -20px
    }

    /*.notifydialogbody {
        display: flex;
        flex-direction: column;
        width: 100%;
        padding: 0;
        //height: calc(100% - 51px);

    }*/

    .viewdetailwaitinglist {
        overflow: auto;
        height: calc(100% - 51px);
    }

    .notifyitem {
        display: flex;
        width: 100%;
        height: 55px;
        //justify-content: center;
        //align-items: center;
    }

    /*.viewdetail-waitinglist {
        overflow: auto;
    }

    .notify-wrapper {
        display: flex;
        flex-direction: column;
        justify-content: flex-start;
        align-items: center;
    }

    .notify-item {
        display: flex;
        width: 100%;
        height: 55px;
        justify-content: center;
        align-items: center;
        &:hover
        {
            background-color: #f0f4ff;
            cursor: pointer;
        }

    }

    .notify-item img {
        cursor: pointer;
    }

    .notifydialog {
        .el-loading-parent--relative
    {
        height: 50px;
    }

    .el-dialog__body {
        padding: 0;
        height: 100%;
    }

    width: 85vw;
    height: 85vh;
    margin-top:40px !important;
    &.approve-dialog {
        width: 50vw;
        height: 50vh;
    }

    .notifydialog-header {
        border-bottom: 0.5px solid #828282;
        padding: 0 18px 0 23px;
        width: 100%;
        height: 50px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        &.approvedialog-header

    {
        justify-content: space-between;
    }

    .right {
        display: flex;
        justify-content: flex-end;
        align-items: center;
        a
    {
        margin-left: 17px;
    }

    }

    .center {
        height: 100%;
        display: flex;
        align-items: center;
        // width: fit-content;
        span

    {
        display: flex;
        align-items: center;
        width: fit-content;
        padding: 0 4px;
        height: 100%;
        font-size: 12px;
        line-height: 16px;
        &:last-child

    {
        margin-left: 15px;
    }

    &:hover,
    &.isActive {
        border-bottom: 2px solid #2f80ed;
        font-weight: bold;
    }

    &:hover {
        cursor: pointer;
    }

    // display: flex;
    // justify-items: center;
    }
    }
    }

    .notifydialog-body {
        display: flex;
        flex-direction: column;
        width: 100%;
        // padding: 10px 18px 10px 23px;
        height: calc(100% - 51px);
        &.approvedialog-body

    {
        padding: 10px 30px;
    }

    .notify-form {
        .el-row

    {
        margin-bottom: 10px;
    }

    .el-form-item__label {
        padding-bottom: 2px;
    }

    }

    .el-select > .el-input > input {
        background-color: #ffffff !important;
    }

    .el-textarea__inner {
        border: none;
        background-color: #f3f3f3;
    }

    }
    }

    .notifydialog.confirm-reason {
        width: 500px;
        .logo-title

    {
        margin-top: 30px;
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
        height: fit-contain;
        width: 100%;
        .logo

    {
        margin-bottom: 20px;
        img

    {
        width: 120px;
        height: 120px;
        object-fit: contain;
    }

    }

    .title {
        font-size: 18px;
    }

    }
    }

    .el-badge__content.is-fixed {
        right: 17px;
    }

    .el-dialog__header {
        display: none;
    }

    .text-bold {
        font-weight: 600;
    }

    .btnClose:hover {
        cursor: pointer;
    }*/
</style>