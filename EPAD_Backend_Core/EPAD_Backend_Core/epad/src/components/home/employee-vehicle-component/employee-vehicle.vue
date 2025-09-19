<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("EmployeeVehicle") }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent :showMasterEmployeeFilter="true"/>
                    </el-col>
                </el-row>
            </el-header>
            <el-main class="bgHome">
                <el-input style="padding-bottom:3px; float:left; width:238px;" :placeholder="$t('SearchData')"
                    v-model="filter" @keyup.enter.native="reloadVehicleData" class="filter-input">
                    <i slot="suffix" class="el-icon-search" @click="reloadVehicleData"></i>
                </el-input>
                <data-table-function-component ref="employeeVehicleDataTableFunction" :showButtonColumConfig="true"
                    :gridColumnConfig.sync="columns" @insert="Insert" @edit="Edit" @delete="Delete"
                    v-bind:listExcelFunction="[]" @add-excel="AddOrDeleteFromExcel('add')">
                </data-table-function-component>
                <el-tabs type="card" v-model="activeTab" @tab-click="handleClick" style="margin-top: 45px;">
                    <el-tab-pane :label="$t('Employee')" name="employeeVehicle">
                        <div>
                            <el-col :span="4">
                                <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll"
                                    :multiple="tree.multiple" :placeholder="$t('SelectDepartment')"
                                    :disabled="tree.isEdit" :data="tree.treeData" :props="tree.treeProps"
                                    :isSelectParent="true" :checkStrictly="tree.checkStrictly"
                                    :clearable="tree.clearable" :popoverWidth="tree.popoverWidth"
                                    v-model="filterDepartment" style="margin-right: 10px"
                                    @change="onChangeDepartmentFilter"></select-department-tree-component>
                            </el-col>
                            <el-col :span="4">
                                <app-select-new class="employee-vehicle_employee-select"
                                    :dataSource="listAllEmployeeFilter" displayMember="FullName" valueMember="Index"
                                    :allowNull="true" v-model="filterModel.ListEmployeeATID" :multiple="true"
                                    style="margin-right: 10px; width: calc(100% - 10px)"
                                    :placeholder="$t('SelectEmployee')" @getValueSelectedAll="selectAllEmployeeFilter"
                                    ref="employeeList">
                                </app-select-new>
                            </el-col>
                            <el-col :span="4">
                                <el-button style="margin-left: 10px;" type="primary" class="smallbutton" size="small"
                                    @click="onViewClick">
                                    {{ $t("View") }}
                                </el-button>
                                <el-dropdown style="margin-left: 10px; margin-top: 5px" @command="handleCommand"
                                    trigger="click">
                                    <span class="el-dropdown-link" style="font-weight: bold">
                                        . . .<span class="more-text">{{ $t("More") }}</span>
                                    </span>

                                    <el-dropdown-menu slot="dropdown">
                                        <el-dropdown-item v-for="(item, index) in listExcelFunction" :key="index"
                                            :command="item">
                                            {{ $t(item) }}
                                        </el-dropdown-item>
                                    </el-dropdown-menu>
                                </el-dropdown>
                            </el-col>
                            <data-table-component :get-data="getData" ref="employeeVehicleTable" :columns="columns"
                                :selectedRows.sync="rowsObj" :isShowPageSize="true" :showSearch="false"
                                class="employee-vehicle-data-table">
                            </data-table-component>
                        </div>
                        <!-- dialog insert -->
                        <div>
                            <el-dialog :title="isEdit ? $t('Edit') : $t('Insert')" style="margin-top: 20px !important;"
                                :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">
                                <el-form class="" :model="employeeVehicleModel" :rules="rules"
                                    ref="employeeVehicleModel" label-width="168px" label-position="top"
                                    @keyup.enter.native="ConfirmClick">
                                    <el-row :gutter="20">
                                        <el-col :span="12">
                                            <el-form-item :label="$t('Department')">
                                                <select-department-tree-component
                                                    :defaultExpandAll="tree.defaultExpandAll" :multiple="tree.multiple"
                                                    :placeholder="$t('SelectDepartment')" :disabled="isEdit"
                                                    :data="tree.treeData" :props="tree.treeProps" :isSelectParent="true"
                                                    :checkStrictly="tree.checkStrictly" :clearable="tree.clearable"
                                                    :popoverWidth="tree.popoverWidth" @change="onChangeDepartmentForm"
                                                    v-model="filterFormDepartmentIndex"
                                                    style="width: 100%"></select-department-tree-component>
                                            </el-form-item>
                                        </el-col>
                                        <el-col :span="12">
                                            <el-form-item prop="EmployeeATIDs" :label="$t('Employee')"
                                                class="employee-vehicle__form-employee-label">
                                                <app-select-new :dataSource="listAllEmployeeForm"
                                                    displayMember="FullName" valueMember="Index" :disabled="isEdit"
                                                    :allowNull="true" v-model="employeeVehicleModel.EmployeeATIDs"
                                                    :multiple="true" :placeholder="$t('SelectEmployee')"
                                                    @getValueSelectedAll="selectAllEmployeeForm" ref="employeeList"
                                                    style="width: 100%;">
                                                </app-select-new>
                                            </el-form-item>
                                        </el-col>
                                        <el-col :span="12">
                                            <el-form-item prop="VehicleType" :label="$t('VehicleType')">
                                                <el-select style="width: 100%" v-model="employeeVehicleModel.Type"
                                                    :clearable="true" :placeholder="$t('SelectVehicleType')">
                                                    <el-option v-for="item in vehicleType" :key="item.Index"
                                                        :label="item.Name" :value="item.Index"></el-option>
                                                </el-select>
                                            </el-form-item>
                                        </el-col>
                                        <el-col :span="12">
                                            <el-form-item prop="VehicleStatusType" :label="$t('VehicleStatusType')">
                                                <el-select style="width: 100%" v-model="employeeVehicleModel.StatusType"
                                                    :clearable="true" :placeholder="$t('SelectVehicleStatusType')">
                                                    <el-option v-for="item in vehicleStatusType" :key="item.Index"
                                                        :label="item.Name" :value="item.Index"></el-option>
                                                </el-select>
                                            </el-form-item>
                                        </el-col>
                                        <el-col :span="12">
                                            <el-form-item prop="Plate">
                                                <label class="el-form-item__label">{{ $t('Plate') }}
                                                    <span v-if="employeeVehicleModel.StatusType == 0"
                                                        style="color: red;">*</span>
                                                </label>
                                                <el-input v-model="employeeVehicleModel.Plate"
                                                    placeholder=""></el-input>
                                            </el-form-item>
                                        </el-col>
                                        <el-col :span="12">
                                            <el-form-item prop="Branch" :label="$t('Branch')">
                                                <el-input v-model="employeeVehicleModel.Branch"
                                                    placeholder=""></el-input>
                                            </el-form-item>
                                        </el-col>
                                        <el-col :span="12">
                                            <el-form-item prop="Color" :label="$t('Color')">
                                                <el-input v-model="employeeVehicleModel.Color"
                                                    placeholder=""></el-input>
                                            </el-form-item>
                                        </el-col>
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
                    </el-tab-pane>
                    <el-tab-pane :label="$t('Customer')" name="customerVehicle">
                        <div>
                            <el-col :span="4">
                                <app-select-new class="employee-vehicle_employee-select"
                                    :dataSource="listAllCustomerFilter" displayMember="FullName" valueMember="Index"
                                    :allowNull="true" v-model="filterModel.ListEmployeeATID" :multiple="true"
                                    style="margin-right: 10px; width: calc(100% - 10px)"
                                    :placeholder="$t('SelectCustomer')" @getValueSelectedAll="selectAllCustomerFilter"
                                    ref="customerList">
                                </app-select-new>
                            </el-col>
                            <el-col :span="4">
                                <el-button style="margin-left: 10px;" type="primary" class="smallbutton" size="small"
                                    @click="onViewClickCustomer">
                                    {{ $t("View") }}
                                </el-button>
                                <el-dropdown style="margin-left: 10px; margin-top: 5px" @command="handleCommand"
                                    trigger="click">
                                    <span class="el-dropdown-link" style="font-weight: bold">
                                        . . .<span class="more-text">{{ $t("More") }}</span>
                                    </span>

                                    <el-dropdown-menu slot="dropdown">
                                        <el-dropdown-item v-for="(item, index) in listExcelFunction" :key="index"
                                            :command="item">
                                            {{ $t(item) }}
                                        </el-dropdown-item>
                                    </el-dropdown-menu>
                                </el-dropdown>
                            </el-col>
                            <data-table-component :get-data="getDataCustomer" ref="customerVehicleTable"
                                :columns="columns" :selectedRows.sync="rowsObj" :isShowPageSize="true"
                                :showSearch="false">
                            </data-table-component>
                        </div>
                        <!-- dialog insert -->
                        <div>
                            <el-dialog :title="isEdit ? $t('Edit') : $t('Insert')" style="margin-top:20px !important;"
                                :visible.sync="showDialog" :before-close="CancelCustomer" :close-on-click-modal="false">
                                <el-form class="" :model="customerVehicleModel" :rules="rules"
                                    ref="customerVehicleModel" label-width="168px" label-position="top"
                                    @keyup.enter.native="ConfirmClickCustomer">

                                    <el-row :gutter="20">
                                        <el-col :span="12">
                                            <el-form-item prop="EmployeeATIDs" :label="$t('Customer')">
                                                <app-select-new :dataSource="listAllCustomerForm"
                                                    displayMember="FullName" valueMember="Index" :disabled="isEdit"
                                                    :allowNull="true" v-model="customerVehicleModel.EmployeeATIDs"
                                                    :multiple="true" :placeholder="$t('SelectCustomer')"
                                                    @getValueSelectedAll="selectAllCustomerForm" ref="customerList"
                                                    style="width: 100%;">
                                                </app-select-new>
                                            </el-form-item>
                                        </el-col>
                                        <el-col :span="12">
                                            <el-form-item prop="VehicleType" :label="$t('VehicleType')">
                                                <el-select style="width: 100%" v-model="customerVehicleModel.Type"
                                                    :clearable="true" :placeholder="$t('SelectVehicleType')">
                                                    <el-option v-for="item in vehicleType" :key="item.Index"
                                                        :label="item.Name" :value="item.Index"></el-option>
                                                </el-select>
                                            </el-form-item>
                                        </el-col>
                                        <el-col :span="12">
                                            <el-form-item prop="Plate" :label="$t('Plate')">
                                                <el-input v-model="customerVehicleModel.Plate"
                                                    placeholder=""></el-input>
                                            </el-form-item>
                                        </el-col>
                                        <el-col :span="12">
                                            <el-form-item prop="Branch" :label="$t('Branch')">
                                                <el-input v-model="customerVehicleModel.Branch"
                                                    placeholder=""></el-input>
                                            </el-form-item>
                                        </el-col>
                                        <el-col :span="12">
                                            <el-form-item prop="Color" :label="$t('Color')">
                                                <el-input v-model="customerVehicleModel.Color"
                                                    placeholder=""></el-input>
                                            </el-form-item>
                                        </el-col>
                                    </el-row>




                                </el-form>
                                <span slot="footer" class="dialog-footer">
                                    <el-button class="btnCancel" @click="CancelCustomer">
                                        {{ $t('Cancel') }}
                                    </el-button>
                                    <el-button class="btnOK" type="primary" @click="ConfirmClickCustomer">
                                        {{ $t('OK') }}
                                    </el-button>
                                </span>
                            </el-dialog>
                        </div>
                    </el-tab-pane>
                </el-tabs>
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
                        <a v-if="activeTab == 'employeeVehicle'" class="fileTemplate-lbl"
                            href="/Template_EmployeeVehicle.xlsx" download>{{ $t("Download")
                            }}</a>
                        <a v-else class="fileTemplate-lbl" href="/Template_CustomerVehicle.xlsx" download>{{
                        $t("Download")
                    }}</a>
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
                        <a v-if="activeTab == 'employeeVehicle'" class="fileTemplate-lbl"
                            href="/Files/Template_EmployeeVehicle_Error.xlsx" download>{{
                        $t('Download')
                    }}</a>
                        <a v-else class="fileTemplate-lbl" href="/Files/Template_CustomerVehicle_Error.xlsx" download>{{
                            $t('Download')
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
<script src="./employee-vehicle.ts"></script>
<style lang="scss">
.formScroll {
    // height: 55vh;
    overflow-y: auto;
    overflow-x: hidden;
}

.el-dialog {
    margin-top: 20px !important;
}

.parking-lot-accessed__table .el-table {
    margin-top: 0;
}

.v-modal {
    display: none;
}

.employee-vehicle-data-table {
    .el-table {
        margin-top: 0;
        height: calc(100vh - 265px) !important;
    }
}

.employee-vehicle__form-employee-label.el-form-item.is-required:not(.is-no-asterisk)>.el-form-item__label:before {
    content: '';
    color: #F56C6C;
    margin-right: 0;
}

.employee-vehicle__form-employee-label.el-form-item.is-required:not(.is-no-asterisk)>.el-form-item__label:after {
    content: '*';
    color: #F56C6C;
    margin-left: 4px;
}

.employee-vehicle_employee-select {
    .el-select__caret.el-input__icon.el-icon-arrow-up {
        height: 100% !important;
    }
    
    .el-select__caret.el-input__icon.el-icon-circle-close {
        height: 100% !important;
    }
}

.employee-vehicle_employee-select .el-select__tags:has(span span:nth-child(1)) {
    top: 50% !important;
    max-width: 90% !important;

    span span:first-child {
        width: max-content;
        max-width: calc(100% - 20%);
        margin-left: 1px;

        .el-select__tags-text {
            vertical-align: top;
            width: max-content;
            max-width: 80%;
            display: inline-block;
            text-overflow: ellipsis;
            overflow: hidden;
        }
    }
}

.employee-vehicle_employee-select .el-select__tags:has(span span:nth-child(2)) {
    top: 50% !important;
    max-width: 90% !important;

    span span:first-child {
        width: max-content;
        max-width: calc(100% - 30%);
        margin-left: 1px;

        .el-select__tags-text {
            vertical-align: top;
            width: max-content;
            max-width: 80%;
            display: inline-block;
            text-overflow: ellipsis;
            overflow: hidden;
        }
    }

    span span:nth-child(2) {
        max-width: 30%;
        margin-left: 1px;

        .el-select__tags-text {
            width: 100%;
            max-width: 100% !important;
        }
    }
}
</style>