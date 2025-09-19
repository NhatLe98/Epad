<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("Gates") }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent />
                    </el-col>
                </el-row>
            </el-header>
            <el-main style="margin: 0 !important;">                
                <el-row class="gates-info" :gutter="20">
                    <el-col :span="isShowLineDeviceInfo ? 6 : 11" id="gates-info__table">
                        <el-table :data="listGate"
                            :height="maxHeight"
                            highlight-current-row 
                            @current-change="handleCurrentChange"
                            @select="handleSelectChange"
                            @select-all="handleSelectAllChange"
                            class="gates-table not-table-text-color"
                            ref="gatesTable"
                        >
                            <el-table-column type="selection"></el-table-column>
                            <el-table-column prop="Name" :label="$t('Gate')"></el-table-column>
                        </el-table>
                    </el-col>
                    <el-col :span="isShowLineDeviceInfo ? 7 : 11" id="gates-info__gates-lines__table">
                        <el-transfer class="lines-transfer"
                            style="width: 100%; height: 90%;"
                            v-model="selectedLine"
                            :titles="linesTitles"
                            :data="linesData"
                            :render-content="renderLabel"
                            @left-check-change="handleUnselectChange"
                            @right-check-change="handleSelectedChange"
                        ></el-transfer>
                    </el-col>
                    <el-col :span="11" id="gates-info__gates-lines__lines-info__table" v-if="isShowLineDeviceInfo">
                        <p class="lines-info__form-header" style="height: 35px; 
                            margin: 0; 
                            vertical-align: middle;
                            background-color: #F5F7FA;
                            margin-left: -10px;
                            margin-right: -10px;"
                        >
                            <span style="font-size: 12px;
                                font-weight: 600;
                                line-height: 16px;
                                text-transform: uppercase;
                                color: #909399;
                                display: block;
                                line-height: 40px;
                                margin-left: 10px;"
                            >
                                {{ $t('ListDeviceByLines')}}: {{ viewingLineName }}
                                <!-- <span style="float: right; margin-right: 15px; line-height: 40px; cursor: pointer;" 
                                    class="el-icon-close" @click="hideLineDeviceInfo">
                                </span> -->
                            </span>
                        </p>
                        <el-form class="lines-info__form" :model="gateLinesModel" :rules="rules" ref="gateLinesModel" 
                            label-width="100px"
                            label-position="left" @keyup.enter.native="Submit"
                        >
                            <label class="lines-info__form-label">{{ $t('AttendanceDevice') }}</label>
                            <el-form-item prop="DeviceInSerial" :label="$t('DeviceIn')" 
                                style="width:95%">
                                <app-select
                                :dataSource="listDevice"
                                :dataDisabled="selectedDevice"
                                displayMember="AliasName"
                                valueMember="SerialNumber"
                                v-model="gateLinesModel.DeviceInSerial"
                                style="width: 100%; padding-bottom: 3px;"
                                :placeholder="$t('SelectMachine')"
                                :multiple="true"
                                ></app-select>
                            </el-form-item>
                            <el-form-item prop="DeviceOutSerial" :label="$t('DeviceOut')" 
                                style="width:95%">
                                <app-select
                                :dataSource="listDevice"
                                :dataDisabled="selectedDevice"
                                displayMember="AliasName"
                                valueMember="SerialNumber"
                                v-model="gateLinesModel.DeviceOutSerial"
                                style="width: 100%; padding-bottom: 3px;"
                                :placeholder="$t('SelectMachine')"
                                :multiple="true"
                                ></app-select>
                            </el-form-item>
                            <label class="lines-info__form-label">{{ $t('Camera') }}</label>
                            <el-form-item prop="CameraInIndex" :label="$t('CameraIn')" 
                                style="width:95%">
                                <app-select
                                :dataSource="listCamera"
                                :dataDisabled="selectedCamera"
                                displayMember="Name"
                                valueMember="Index"
                                v-model="gateLinesModel.CameraInIndex"
                                style="width: 100%; padding-bottom: 3px;"
                                :placeholder="$t('SelectCamera')"
                                :multiple="true"
                                ></app-select>
                            </el-form-item>
                            <el-form-item prop="CameraOutIndex" :label="$t('CameraOut')" 
                                style="width:95%">
                                <app-select
                                :dataSource="listCamera"
                                :dataDisabled="selectedCamera"
                                displayMember="Name"
                                valueMember="Index"
                                v-model="gateLinesModel.CameraOutIndex"
                                style="width: 100%; padding-bottom: 3px;"
                                :placeholder="$t('SelectCamera')"
                                :multiple="true"
                                ></app-select>
                            </el-form-item>
                            
                            <label class="lines-info__form-label">{{ $t('Controller') }}</label>

                            <div class="line-controller-select">
                                <el-row style="margin-bottom: 8px;">
                                    <el-col :span="7">
                                        <label style="display: block; font-size: 14px;">
                                    {{ $t('ControllerIn') }}
                                </label>
                                    </el-col>
                                    <el-col :span="7">
                                        <label style="display: block; font-size: 14px;">
                                    {{ $t('OpenChannel') }}
                                </label>
                                    </el-col>
                                    <el-col :span="7">
                                        <label style="display: block; font-size: 14px;">
                                    {{ $t('CloseChannel') }}
                                </label>
                                    </el-col>
                                    <el-col :span="3"></el-col>
                                </el-row>
                                <!-- <el-col :span="5">
                                    <el-form-item prop="ControllerType" style="width:95%">
                                        <app-select
                                        v-for="(e, ix) in gateLinesModel.LineControllersIn.length"
                                        :key="`controller-type-${ix}`"
                                        :dataSource="listControllerType"
                                        displayMember="Name"
                                        valueMember="Index"
                                        v-model="gateLinesModel.LineControllersIn[ix].ControllerType"
                                        style="width: 100%; padding-bottom: 3px; margin-top: 5px;"
                                        :placeholder="$t('')"
                                        ></app-select>
                                    </el-form-item>
                                    </el-col> -->
            
                                    <el-col :span="7" class="line-controller-select__col">
                                        <el-form-item class="no-label__item" prop="ControllerIndex" style="width:95%">
                                            <app-select
                                            v-for="(e, ix) in gateLinesModel.LineControllersIn.length"
                                            :key="`controller-${ix}`"
                                            :dataSource="listControllerIn"
                                            :dataDisabled="selectedControllerIn"
                                            displayMember="Name"
                                            valueMember="Index"
                                            v-model="gateLinesModel.LineControllersIn[ix].ControllerIndex"
                                            style="width: 100%; padding-bottom: 3px; margin-top: 5px;"
                                            :placeholder="$t('SelectController')"
                                            ></app-select>
                                        </el-form-item>
                                    </el-col>

                                    <el-col :span="7" class="line-controller-select__col">
                                        <el-form-item class="no-label__item" prop="OpenChannel" style="width:95%">
                                            <app-select
                                            v-for="(e, ix) in gateLinesModel.LineControllersIn.length"
                                            :key="`open-channel-${ix}`"
                                            :dataSource="listControllerInChannel[ix]"
                                            displayMember="Name"
                                            valueMember="Index"
                                            v-model="gateLinesModel.LineControllersIn[ix].OpenChannel"
                                            style="width: 100%; padding-bottom: 3px; margin-top: 5px;"
                                            :placeholder="$t('SelectChannel')"
                                            ></app-select>
                                        </el-form-item>
                                    </el-col>
                                    <el-col :span="7" class="line-controller-select__col">
                                        <el-form-item class="no-label__item" prop="CloseChannel" style="width:95%">
                                            <app-select
                                            v-for="(e, ix) in gateLinesModel.LineControllersIn.length"
                                            :key="`close-channel-${ix}`"
                                            :dataSource="listControllerInChannel[ix]"
                                            displayMember="Name"
                                            valueMember="Index"
                                            v-model="gateLinesModel.LineControllersIn[ix].CloseChannel"
                                            style="width: 100%; padding-bottom: 3px; margin-top: 5px;"
                                            :placeholder="$t('SelectChannel')"
                                            ></app-select>
                                        </el-form-item>
                                    </el-col>

                                    <el-col :span="3" class="line-controller-select__col">
                                    <el-form-item class="no-label__item">
                                        <el-row
                                            v-for="(e, ix) in gateLinesModel.LineControllersIn.length"
                                            :key="`add_or_remove_button-${ix}`"
                                            :gutter="20"
                                            class="group-button"
                                        >
            
                                            <el-col :span="24">
                                                <el-button
                                                class="add-remove-controller__button"
                                                size="mini"
                                                type="primary"
                                                icon="el-icon-plus"
                                                circle
                                                @click="addItemControllerIn">
                                                </el-button>
                
                                                <el-button
                                                class="add-remove-controller__button"
                                                size="mini"
                                                type="danger"
                                                icon="el-icon-close"
                                                circle
                                                @click="removeItemDetailControllerIn(ix)"
                                                v-if="ix !== 0">
                                                </el-button>
                                            </el-col>
                                        </el-row>
                                    </el-form-item>
                                </el-col>
                            </div>
                            <div class="line-controller-select">
                                <el-row style="margin-bottom: 8px;">
                                    <el-col :span="7">
                                        <label style="display: block; font-size: 14px;">
                                    {{ $t('ControllerOut') }}
                                </label>
                                    </el-col>
                                    <el-col :span="7">
                                        <label style="display: block; font-size: 14px;">
                                    {{ $t('OpenChannel') }}
                                </label>
                                    </el-col>
                                    <el-col :span="7">
                                        <label style="display: block; font-size: 14px;">
                                    {{ $t('CloseChannel') }}
                                </label>
                                    </el-col>
                                    <el-col :span="3"></el-col>
                                </el-row>
                                <!-- <el-col :span="5">
                                    <el-form-item prop="ControllerType" style="width:95%">
                                        <app-select
                                        v-for="(e, ix) in gateLinesModel.LineControllersOut.length"
                                        :key="`controller-type-${ix}`"
                                        :dataSource="listControllerType"
                                        displayMember="Name"
                                        valueMember="Index"
                                        v-model="gateLinesModel.LineControllersOut[ix].ControllerType"
                                        style="width: 100%; padding-bottom: 3px; margin-top: 5px;"
                                        :placeholder="$t('')"
                                        ></app-select>
                                    </el-form-item>
                                    </el-col> -->
            
                                    <el-col :span="7" class="line-controller-select__col">
                                        <el-form-item class="no-label__item" prop="ControllerIndex" style="width:95%">
                                            <app-select
                                            v-for="(e, ix) in gateLinesModel.LineControllersOut.length"
                                            :key="`controller-${ix}`"
                                            :dataSource="listControllerOut"
                                            :dataDisabled="selectedControllerOut"
                                            displayMember="Name"
                                            valueMember="Index"
                                            v-model="gateLinesModel.LineControllersOut[ix].ControllerIndex"
                                            style="width: 100%; padding-bottom: 3px; margin-top: 5px;"
                                            :placeholder="$t('SelectController')"
                                            ></app-select>
                                        </el-form-item>
                                    </el-col>

                                    <el-col :span="7" class="line-controller-select__col">
                                        <el-form-item class="no-label__item" prop="OpenChannel" style="width:95%">
                                            <app-select
                                            v-for="(e, ix) in gateLinesModel.LineControllersOut.length"
                                            :key="`open-channel-${ix}`"
                                            :dataSource="listControllerOutChannel[ix]"
                                            displayMember="Name"
                                            valueMember="Index"
                                            v-model="gateLinesModel.LineControllersOut[ix].OpenChannel"
                                            style="width: 100%; padding-bottom: 3px; margin-top: 5px;"
                                            :placeholder="$t('SelectChannel')"
                                            ></app-select>
                                        </el-form-item>
                                    </el-col>
                                    <el-col :span="7" class="line-controller-select__col">
                                        <el-form-item class="no-label__item" prop="CloseChannel" style="width:95%">
                                            <app-select
                                            v-for="(e, ix) in gateLinesModel.LineControllersOut.length"
                                            :key="`close-channel-${ix}`"
                                            :dataSource="listControllerOutChannel[ix]"
                                            displayMember="Name"
                                            valueMember="Index"
                                            v-model="gateLinesModel.LineControllersOut[ix].CloseChannel"
                                            style="width: 100%; padding-bottom: 3px; margin-top: 5px;"
                                            :placeholder="$t('SelectChannel')"
                                            ></app-select>
                                        </el-form-item>
                                    </el-col>

                                    <el-col :span="3" class="line-controller-select__col">
                                        <el-form-item class="no-label__item">
                                            <el-row
                                                v-for="(e, ix) in gateLinesModel.LineControllersOut.length"
                                                :key="`add_or_remove_button-${ix}`"
                                                :gutter="20"
                                                class="group-button"
                                            >
                
                                                <el-col :span="24">
                                                    <el-button
                                                    class="add-remove-controller__button"
                                                    size="mini"
                                                    type="primary"
                                                    icon="el-icon-plus"
                                                    circle
                                                    @click="addItemControllerOut">
                                                    </el-button>
                    
                                                    <el-button
                                                    class="add-remove-controller__button"
                                                    size="mini"
                                                    type="danger"
                                                    icon="el-icon-close"
                                                    circle
                                                    @click="removeItemDetailControllerOut(ix)"
                                                    v-if="ix !== 0">
                                                    </el-button>
                                                </el-col>
                                            </el-row>
                                        </el-form-item>
                                    </el-col>
                            </div>
                        </el-form>
                    </el-col>
                </el-row>
                <el-row class="gates-info__button" :gutter="20">
                    <el-col :span="isShowLineDeviceInfo ? 6 : 11" id="gates-info__table__button">
                        <el-button type="primary" @click="EditGate">
                            <i class="el-icon-edit"></i> {{ $t('Edit') }}
                        </el-button>
                        <span>
                            <el-button type="primary" style="margin-right: 5px;" @click="DeleteGate">
                                <i class="el-icon-delete"></i> {{ $t('Delete') }}
                            </el-button>
                            <el-button type="primary" @click="InsertGate">
                                <i class="el-icon-plus"></i> {{ $t('Add') }}
                            </el-button>
                        </span>                        
                    </el-col>
                    <el-col :span="isShowLineDeviceInfo ? 7 : 11" id="gates-info__gates-lines__table__button">
                        <el-button type="primary" @click="EditLine">
                            <i class="el-icon-edit"></i> {{ $t('Edit') }}
                        </el-button>
                        <span>
                            <el-button type="primary" style="margin-right: 5px;" @click="DeleteLine">
                                <i class="el-icon-delete"></i> {{ $t('Delete') }}
                            </el-button>
                            <el-button type="primary" @click="InsertLine">
                                <i class="el-icon-plus"></i> {{ $t('Add') }}
                            </el-button>
                        </span>
                    </el-col>
                    <el-col :span="isShowLineDeviceInfo ? 11 : 2" id="gates-info__gates-lines__lines-info__table__button">
                        <el-button type="success" style="float: right;" @click="UpdateGateLineDeviceInfo">
                            {{ $t('Save') }}
                        </el-button>
                    </el-col>
                </el-row>
            </el-main>
        </el-container>

        <div>
            <el-dialog :title="isEdit ? $t('Edit') : $t('Insert')" 
                style="margin-top:20px !important; height: fit-content;"
                custom-class="customdialog" v-if="showDialogGate" :visible.sync="showDialogGate" 
                :before-close="CancelGate" :close-on-click-modal="false">
                <el-form :model="gatesModel" :rules="rules" ref="gatesModel" label-width="168px"
                    label-position="top">
                    <el-row>
                        <el-col :span="24">
                            <el-form-item prop="Name" :label="$t('Name')">
                                <el-input v-model="gatesModel.Name" placeholder=""></el-input>
                            </el-form-item>
                        </el-col>
                    </el-row>
                    
                    <el-form-item prop="Description" :label="$t('Description')">
                        <el-input v-model="gatesModel.Description" placeholder=""></el-input>
                    </el-form-item>

                    <!-- <el-form-item prop="IsMandatory">
                        <el-checkbox v-model="gatesModel.IsMandatory">{{ $t('IsMandatory') }}</el-checkbox>
                    </el-form-item> -->
                </el-form>
                <span slot="footer" class="dialog-footer">
                    <el-button class="btnCancel" @click="CancelGate">
                        {{ $t('Cancel') }}
                    </el-button>
                    <el-button class="btnOK" type="primary" @click="ConfirmClickGate">
                        {{ $t('OK') }}
                    </el-button>
                </span>
            </el-dialog>

            <el-dialog :title="isEdit ? $t('Edit') : $t('Insert')" 
                style="margin-top:20px !important; height: fit-content;"
                custom-class="customdialog" v-if="showDialogLine" :visible.sync="showDialogLine" 
                :before-close="CancelLine"  :close-on-click-modal="false">
                <el-form :model="linesModel" :rules="rules" ref="linesModel" label-width="168px"
                    label-position="top">
                    <el-row>
                        <el-col :span="24">
                            <el-form-item prop="Name" :label="$t('Name')">
                                <el-input v-model="linesModel.Name" placeholder=""></el-input>
                            </el-form-item>
                        </el-col>
                    </el-row>
                    
                    <el-form-item prop="Description" :label="$t('Description')">
                        <el-input v-model="linesModel.Description" placeholder=""></el-input>
                    </el-form-item>

                    <el-form-item prop="LineForCustomer">
                        <el-checkbox v-model="linesModel.LineForCustomer">{{ $t('LineForCustomer') }}</el-checkbox>
                    </el-form-item>

                    <el-form-item  v-if="linesModel.LineForCustomer" prop="LineForCustomerIssuanceReturnCard"
                    style="margin-left: 20px;">
                        <el-radio v-model="linesModel.LineForCustomerIssuanceReturnCard" :label="false">{{ $t('NormalLine') }}</el-radio>
                        <el-radio v-model="linesModel.LineForCustomerIssuanceReturnCard" :label="true">{{ $t('IssuanceReturnCardLine') }}</el-radio>
                    </el-form-item>
                    
                    <el-form-item prop="LineForDriver">
                        <el-checkbox v-model="linesModel.LineForDriver">{{ $t('LineForDriver') }}</el-checkbox>
                    </el-form-item>

                    <el-form-item v-if="linesModel.LineForDriver" prop="LineForDriverIssuanceReturnCard"
                    style="margin-left: 20px;">
                        <el-radio v-model="linesModel.LineForDriverIssuanceReturnCard" :label="false">{{ $t('NormalLine') }}</el-radio>
                        <el-radio v-model="linesModel.LineForDriverIssuanceReturnCard" :label="true">{{ $t('IssuanceReturnCardLine') }}</el-radio>
                    </el-form-item>
                </el-form>
                <span slot="footer" class="dialog-footer">
                    <el-button class="btnCancel" @click="CancelLine">
                        {{ $t('Cancel') }}
                    </el-button>
                    <el-button class="btnOK" type="primary" @click="ConfirmClickLine">
                        {{ $t('OK') }}
                    </el-button>
                </span>
            </el-dialog>
        </div>
    </div>
</template>
<script src="./gates.ts"></script>
<style lang="scss" scoped>
.gates-info {
    height: 95%;
}

.gates-info__button{
    height: 5%;
}

#gates-info__table,
#gates-info__gates-lines__table,
#gates-info__gates-lines__lines-info__table {
    height: 100%;
    overflow-y: auto;
}

#gates-info__table__button,
#gates-info__gates-lines__table__button,
#gates-info__gates-lines__lines-info__table__button {
    margin-top: 5px;
}

#gates-info__table__button,
#gates-info__gates-lines__table__button {
    display: flex;
    justify-content: space-between;
}

#gates-info__gates-lines__lines-info__table {
    border: 1px solid #ebeef5;
}

.gates-table{
    cursor: pointer; 
    height: 100%; 
    margin: 0 !important;
    border: 1px solid #ebeef5;
    /deep/ .el-table__header-wrapper th {
        background-color: #F5F7FA;
    }

    /deep/ .el-checkbox {
        margin-left: 15px !important;
    }

    /deep/ .current-row {
        td{
            background-color: #B9B9B9 !important;
        }
    }
}

.lines-transfer {
  display: contents;
  justify-content: space-between;
  align-items: center; 
}

.lines-transfer /deep/ .el-transfer-panel {
  flex-grow: 1;
  width: 100% !important;
  height: calc((100% - 46px) / 2) !important;
}

.lines-transfer /deep/ .el-transfer__buttons {
    width: 100%;
    height: 46px;
    text-align: center;
    padding: 0 5px;
    .el-transfer__button {
        margin-top: 5px;
        margin-bottom: 5px;
        border: none;
    }
    .el-transfer__button:first-child {
        margin-right: 5px;
        span {
            i {
                rotate: -90deg;
            }
        }
    }
    .el-transfer__button:nth-child(2) {
        span {
            i {
                rotate: -90deg;
            }
        }
    }
}

.lines-transfer /deep/ .el-transfer-panel__body {
  height: calc(100% - 40px);
}

.lines-transfer /deep/ .el-checkbox-group.el-transfer-panel__list {
  height: 100%;
}

.lines-info__form {
    .el-form-item {
        /deep/ .el-form-item__label {
            font-weight: normal !important;
        }
    }
}

.lines-info__form-label {
    font-weight: bold;
    display: block;
    margin-bottom: 15px;
}

.no-label__item /deep/ .el-form-item__content{
    margin-left: 0 !important;
}

.add-remove-controller__button {
    margin-left: 5px; 
    width: 28px !important; 
    height: 28px !important; 
    margin-top: 8px;
    margin-bottom: 3px;
}

.line-controller-select {
    width: 100%;
    display: inline-block;
}

.line-controller-select__col {
    padding: 0 !important;
}
</style>