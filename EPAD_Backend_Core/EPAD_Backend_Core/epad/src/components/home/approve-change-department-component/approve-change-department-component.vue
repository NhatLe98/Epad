<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="10" class="left">
                        <span id="FormName">
                            {{
                                    $t("ApproveChangeDepartment")
                            }}
                        </span>
                    </el-col>
                    <el-col :span="14">
                        <HeaderComponent></HeaderComponent>
                    </el-col>
                </el-row>
            </el-header>
            <el-main class="bgHome">
                <div>
                    <div>
                        <el-row class="approve-change-department__custom-function-bar">
                            <el-col :span="3" style="margin-right: 10px;">
                            <el-date-picker :placeholder="$t('FromDateString')" v-model="fromDate"
                                                            type="date"
                                                            id="inputFromDate"></el-date-picker>
                            </el-col>
                            <el-col :span="3" style="margin-right: 10px;">
                            <el-date-picker :placeholder="$t('ToDateString')" v-model="toDate"
                                                            type="date"
                                                            id="inputToDate"></el-date-picker>
                            </el-col>
                            <el-col :span="1">
                            <el-button type="primary"
                                                        size="small"
                                                        class="smallbutton"
                                                        @click="searchData">{{ $t("View") }}</el-button>
                            </el-col>
                        </el-row>                        
                    </div>
                    <div>
                        <div class="button-function approve-change-department__data-table-function">
                            <el-button type="primary" @click="btnshowDialog('reject')">
                                {{ $t("RejectTransfer") }}
                            </el-button>
                            <el-button style="margin-left: 10px" type="primary" @click="btnshowDialog('approve')">
                                {{ $t("ApproveTransfer") }}
                            </el-button>
                            <data-table-function-component
                                :showButtonInsert="false"
                                :isHiddenEdit="true" :isHiddenDelete="true"
                                :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
                                style="height: fit-content; display: flex; position: relative; top: 0; width: fit-content;
                                display: unset !important; margin-right: 0 !important; margin-left: 10px !important;"
                            ></data-table-function-component>
                        </div>
                        <data-table-component :get-data="initTableData" class="approve-change-department__data-table"
                                              ref="table"
                                              :columns="columns"
                                              :selectedRows.sync="rowsObj"
                                              v-bind:isFromHasView="true"
                                              :isShowPageSize="true">
                        </data-table-component>

                    </div>
                    <div>
                        <el-dialog :title="isApproveAction ? $t('ApproveChangeDepartment') : $t('RejectChangeDepartment')"
                                   custom-class="customdialog"
                                   :visible.sync="showDialog"
                                   :before-close="cancelDialog">
                            <el-form label-width="168px"
                                     label-position="top"
                                     @keyup.enter.native="Submit">
                                <div>
                                    <i style="font-weight:bold; font-size:larger; color:orange" class="el-icon-warning-outline" /> 
                                    <span style="font-weight:bold">{{ isApproveAction ? $t('DialogMessageApproveDepartmentTransfer') : $t('DialogMessageRejectDepartmentTransfer') }}</span>
                                </div>
                                <el-form-item>
                                    <el-radio-group v-model="transferNow" v-if="isApproveAction">
                                        <el-radio :label="false"> {{ $t( 'DialogUpdateFollowSystemScheduled' )}}</el-radio>
                                        <el-radio :label="true"> {{$t( 'DialogUpdateRightNow' )}}</el-radio>
                                    </el-radio-group>
                                </el-form-item>
                            </el-form>
                            <span slot="footer" class="dialog-footer">
                                <el-button class="btnCancel"
                                           @click="cancelDialog()">
                                    {{ $t("Cancel") }}
                                </el-button>
                                <el-button class="btnOK" v-if="!isApproveAction"
                                           type="primary"
                                           @click="rejectDepartmentTransfer">
                                    {{ $t("RejectTransfer") }}
                                </el-button>
                                <el-button class="btnOK" v-if="isApproveAction"
                                           type="primary"
                                           @click="approveDepartmentTransfer">
                                    {{ $t("ApproveTransfer") }}
                                </el-button>
                            </span>
                        </el-dialog>
                    </div>
                </div>
            </el-main>
        </el-container>
    </div>
</template>
<script src="./approve-change-department-component.ts"></script>

<style lang="scss">
    .button-function {
        margin-right: 24px;
        width: fit-content;
        display: flex;
        justify-content: space-between;
        height: 36px;
        position: absolute;
        margin-top: 5px;
        right: 12px;
    }
    .approve-change-department__data-table {
        .filter-input {
            margin-right: 10px;
        }
        .el-table {
            height: calc(100vh - 174px) !important;
        }
        .approve-change-department__data-table-function {
            .datatable-function{
                .group-btn{
                    button{
                        height: 35px !important;
                    }
                }
            }
        }
    }
</style>
