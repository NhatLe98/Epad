<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("ParkingLotAccessed") }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent :showMasterEmployeeFilter="true"/>
                    </el-col>
                </el-row>
            </el-header>
            <el-main class="bgHome" style="padding-top: 10px !important;">
                <div>
                    <!-- <el-select style="float: left; margin-right: 5px;"></el-select> -->
                    <div style="margin-bottom: 15px;">
                        <data-table-function-component id="parking-lot-accessed__data-table-function"
                            :showButtonColumConfig="true" 
                            :gridColumnConfig.sync="columns"
                            :showButtonIntegrate="true"
                            @insert="Insert" @edit="Edit" @delete="Delete" @integrate="Integrate" isHiddenSearch>
                        </data-table-function-component>
                    </div>
                    <el-row>
                        <el-col :span="4">
                            <!-- <span style="font-weight: bold; display: block; line-height: 32px; margin-top: -32px;">{{ $t('UserType') }}</span> -->
                            <app-select-new :dataSource="listAccessType" displayMember="Name" valueMember="Index"
                                :allowNull="true" v-model="filterAccessType" :multiple="true" style="width: 100%"
                                :placeholder="$t('SelectUserType')" @getValueSelectedAll="selectAllAccessTypeFilter" 
                                ref="accessTypeFilter">
                            </app-select-new>
                        </el-col>
                        <el-col :span="4" style="margin-left: 10px;">
                            <!-- <span style="font-weight: bold; display: block; line-height: 32px; margin-top: -32px;">{{ $t('ParkingLot') }}</span> -->
                            <!-- <app-select
                                style="width: 95%;"
                                :dataSource="listParkingLot"
                                displayMember="Name"
                                valueMember="Index"
                                v-model="filterParkingLotIndex"
                                :placeholder="$t('ParkingLot')"
                            ></app-select> -->
                            <app-select-new :dataSource="listParkingLot" displayMember="Name" valueMember="Index"
                                :allowNull="true" v-model="filterParkingLotIndex" :multiple="true" style="width: 100%"
                                :placeholder="$t('SelectParkingLot')" @getValueSelectedAll="selectAllParkingLotFilter" 
                                ref="parkingLotFilter">
                            </app-select-new>
                        </el-col>
                        <el-col :span="4" style="margin-left: 10px;">
                            <!-- <span style="font-weight: bold; display: block; line-height: 32px; margin-top: -32px;">{{ $t('FromDateString') }}</span> -->
                            <el-date-picker v-model="filterFromDate" style="width: 100%;" type="date" :editable="false"
                                :placeholder="$t('SelectFromDate')"></el-date-picker>
                        </el-col>
                        <el-col :span="4" style="margin-left: 10px;">
                            <!-- <span style="font-weight: bold; display: block; line-height: 32px; margin-top: -32px;">{{ $t('ToDateString') }}</span> -->
                            <el-date-picker v-model="filterToDate" style="width: 100%;" type="date" :editable="false"
                                :placeholder="$t('SelectToDate')"></el-date-picker>
                        </el-col>
                        <el-col :span="4" style="margin-left: 10px;">
                            <!-- <span style="font-weight: bold; display: block; line-height: 32px; margin-top: -32px;">{{ $t('Search') }}</span> -->
                            <el-input style="padding-bottom: 3px; width: 100%;"
                                :placeholder="$t('SearchData')"
                                suffix-icon="el-icon-search"
                                v-model="filterString"
                                @keyup.enter.native="viewData"
                                class="filter-input"></el-input>
                        </el-col>
                        <el-col :span="1" style="margin-left: 10px;">
                            <el-button type="primary"
                                class="smallbutton"
                                size="small"
                                @click="viewData">
                                    {{ $t("View") }}
                            </el-button>
                        </el-col>
                        <el-col :span="2">
                            <el-dropdown
                                style="margin-top: 10px"
                                @command="handleCommand"
                                trigger="click"
                            >
                                <span
                                    class="el-dropdown-link"
                                    style="font-weight: bold"
                                >
                                    . . .<span class="more-text">{{ $t("More") }}</span>
                                </span>

                                <el-dropdown-menu slot="dropdown">
                                <el-dropdown-item
                                    v-for="(item, index) in listExcelFunction"
                                    :key="index"
                                    :command="item"
                                    >
                                        {{ $t(item) }}
                                    </el-dropdown-item>
                                </el-dropdown-menu>
                            </el-dropdown>
                        </el-col>
                    </el-row>
                    <!-- <div style="width: 70%; height: 36px; display: flex; align-items: center;">
                        <el-dropdown
                            style="margin-left: 10px"
                            @command="handleCommand"
                            trigger="click"
                        >
                            <span
                                class="el-dropdown-link"
                                style="font-weight: bold"
                            >
                                . . .<span class="more-text">{{ $t("More") }}</span>
                            </span>

                            <el-dropdown-menu slot="dropdown">
                              <el-dropdown-item
                                v-for="(item, index) in listExcelFunction"
                                :key="index"
                                :command="item"
                                >
                                    {{ $t(item) }}
                                </el-dropdown-item>
                            </el-dropdown-menu>
                        </el-dropdown>
                    </div> -->
                    <data-table-component :showSearch="false" class="parking-lot-accessed__table"
                        :get-data="getData" ref="parkingLotAccessedTable" :columns="columns"
                        :selectedRows.sync="rowsObj" :isShowPageSize="true"></data-table-component>
                </div>
                <!-- dialog insert -->
                <div>
                    <el-dialog :title="isEdit ? $t('Edit') : $t('Insert')" style="margin-top:20px !important;"
                        custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false"
                        class="parking-lot-accessed__dialog">
                        <el-form class="formScroll" :model="parkingLotAccessedModel" :rules="rules" ref="parkingLotAccessedModel"
                            label-width="168px" label-position="top" @keyup.enter.native="Submit"
                            style="height: fit-content !important;">
                            <el-row>
                                <el-col :span="12">
                                    <el-form-item prop="ParkingLotIndex" :label="$t('ParkingLot')" style="margin-right: 10px;">
                                        <el-select style="width: 100%" v-model="parkingLotAccessedModel.ParkingLotIndex" 
                                            :disabled="isEdit"
                                            :clearable="true"
                                            :placeholder="$t('SelectParkingLot')">
                                            <el-option v-for="item in listParkingLot" :key="item.Index" :label="item.Name"
                                                :value="item.Index"></el-option>
                                        </el-select>
                                    </el-form-item>
                                    <el-form-item :label="$t('FromDateString')" prop="FromDate" style="margin-right: 10px;">
                                        <el-date-picker v-model="parkingLotAccessedModel.FromDate" type="date"  :editable="false"
                                        :clearable="false" :disabled="isEdit"
                                        :placeholder="$t('SelectFromDate')"></el-date-picker>
                                    </el-form-item>
                                    <el-form-item :label="$t('Department')" style="margin-right: 10px;"
                                     v-if="parkingLotAccessedModel.AccessType == 1">
                                        <select-department-tree-component 
                                            :defaultExpandAll="tree.defaultExpandAll"
                                            :multiple="tree.multiple"
                                            :placeholder="$t('SelectDepartment')"
                                            :disabled="isEdit"
                                            :data="tree.treeData"
                                            :props="tree.treeProps"
                                            :isSelectParent="true"
                                            :checkStrictly="tree.checkStrictly"
                                            :clearable="tree.clearable"
                                            :popoverWidth="tree.popoverWidth"
                                            @change ="onChangeDepartmentFilter"
                                            v-model="filterFormDepartmentIndex"
                                            style="width: 100%; height: 50px !important;"
                                        ></select-department-tree-component>
                                    </el-form-item>
                                    <el-form-item prop="EmployeeATIDs" :label="$t('Customer')" style="margin-right: 10px;"
                                    v-show="parkingLotAccessedModel.AccessType != 1">
                                        <app-select-new :dataSource="listAllCustomerFilter" displayMember="FullName" 
                                            valueMember="Index" :disabled="isEdit"
                                            :allowNull="true" v-model="parkingLotAccessedModel.EmployeeATIDs" :multiple="true"
                                            :placeholder="$t('Customer')" @getValueSelectedAll="selectAllCustomerFilter" 
                                            ref="customerList">
                                        </app-select-new>
                                    </el-form-item>
                                </el-col>
                                <el-col :span="12">
                                    <el-form-item :label="$t('UserType')" style="margin-left: 10px;">
                                        <el-select style="width: 100%" v-model="parkingLotAccessedModel.AccessType" 
                                        @change="changeUserType"
                                            :disabled="isEdit"
                                            :clearable="false"
                                            :placeholder="$t('SelectUserType')">
                                            <el-option v-for="item in listAccessType" :key="item.Index" :label="item.Name"
                                                :value="item.Index"></el-option>
                                        </el-select>
                                    </el-form-item>
                                    <el-form-item :label="$t('ToDateString')" prop="ToDate" style="margin-left: 10px;">
                                        <el-date-picker v-model="parkingLotAccessedModel.ToDate" type="date" :editable="false"
                                        :placeholder="$t('SelectToDate')"></el-date-picker>
                                    </el-form-item>
                                    <el-form-item prop="EmployeeATIDs" :label="$t('Employee')" style="margin-left: 10px;"
                                    v-if="parkingLotAccessedModel.AccessType == 1">
                                        <app-select-new :dataSource="listAllEmployeeFilter" displayMember="FullName" 
                                            valueMember="Index" :disabled="isEdit"
                                            :allowNull="true" v-model="parkingLotAccessedModel.EmployeeATIDs" :multiple="true"
                                            :placeholder="$t('Employee')" @getValueSelectedAll="selectAllEmployeeFilter" 
                                            ref="employeeList">
                                        </app-select-new>
                                    </el-form-item>
                                    <el-form-item prop="Description" :label="$t('Description')" style="margin-left: 10px;"
                                        v-if="parkingLotAccessedModel.AccessType != 1">
                                        <el-input class="parking-lot-accessed-register__description" type="textarea" 
                                        :autosize="{ minRows: 1, maxRows: 6 }"
                                        v-model="parkingLotAccessedModel.Description" placeholder=""></el-input>
                                    </el-form-item>
                                </el-col>
                                <el-form-item prop="Description" :label="$t('Description')"
                                v-if="parkingLotAccessedModel.AccessType == 1">
                                    <el-input class="parking-lot-accessed-register__description" type="textarea" :autosize="{ minRows: 1, maxRows: 6 }"
                                    v-model="parkingLotAccessedModel.Description" placeholder=""
                                    ></el-input>
                                </el-form-item>
                            </el-row>
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

        <div>
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
                        <a class="fileTemplate-lbl" href="/Template_EmployeeParkingLot.xlsx" download>{{ $t("Download") }}</a>
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
        </div>

        <div>
            <el-dialog :title="$t('DialogHeaderTitle')" custom-class="customdialog" :visible.sync="showDialogImportError"
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
                        <a class="fileTemplate-lbl" href="/Files/Template_EmployeeParkingLot_Error.xlsx" download>{{ $t('Download')
                        }}</a>
                    </el-form-item>
                </el-form>

                <span slot="footer" class="dialog-footer">
                    <el-button class="btnOK" type="primary" @click="showOrHideImportError(false)">
                        OK
                    </el-button>
                </span>
            </el-dialog>
        </div>
    </div>
</template>
<script src="./parking-lot-accessed.ts"></script>
<style lang="scss">
.customdialog {
    .el-input--small {
        input {
            height: 50px !important;
            border-radius: 8px;
        }
    }
}

.formScroll {
    height: 55vh;
    overflow-y: auto;
    overflow-x: hidden;
}

.el-dialog {
    margin-top: 20px !important;
}

.parking-lot-accessed__table {
    height: calc(100vh - 205px);
    .el-table {
        margin-top: 0;
        height: 100% !important;
    }
}

.parking-lot-accessed__dialog {
    .el-dialog {
        width: 40vw;
    }
}

.parking-lot-accessed-register__description {
    .el-textarea__inner {
        min-height: 50px !important;
        height: 50px;
    }
}

#parking-lot-accessed__data-table-function {
    position: relative !important;
    top: unset;
    right: unset;
    display: flex;
    width: 100%;
    .group-btn {
        float: right;
    }
}
</style>