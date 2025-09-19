<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("ACDoorManagement") }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent />
                    </el-col>
                </el-row>
            </el-header>
            <div :class="{ 'full-screen fullscreen-container': isFullScreen() }" class="ac-door-management-container" style="overflow-y: hidden;">
                <el-main class="bgHome log-realtime-monitoring" style="height: 100vh !important;"
                    :class="{ 'el-main__full-screen': isFullScreen() }">
                    <!-- <div style="height:46px;">
                    <el-alert style="height:100%;" class="center16px" :title="$t('ConnectToMonitoringServerSuccessfully')"
                        type="success" :closable="false" effect="dark" v-if="isConnect"></el-alert>
                    <el-alert style="height:100%;" class="center16px" :title="$t('NotConnectServerMonitoring')" type="error"
                        :closable="false" effect="dark" v-if="!isConnect"></el-alert>
                </div> -->
                    <div style="height: 45vh !important;">
                        <el-row type="flex" justify="end" style="height: 10vh">
                            <el-col :span="2">
                                <el-button type="primary" style="width: 100px" @click="RunIntegrateLog">{{ $t('OpenDoor') }}</el-button>
                            </el-col>

                        </el-row>
                        <el-row>
                            <el-col :span="24">
                                <div v-for="(item, index) in listAllDoor" class="item">
                                    <div>
                                        <img v-bind:class="{ active: isActive == item.value }" :src="getUrl(item.value)"
                                            style="width: 70px; height: auto" @click="changeSelected(item.value)" />
                                        <span class="caption">{{ item.label }}</span>
                                    </div>
                                </div>



                            </el-col>
                        </el-row>



                    </div>
                    <div class="filterContainer">
                        <!-- <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll" :multiple="tree.multiple"
                        :placeholder="$t('SelectDepartment')" :disabled="tree.isEdit" :data="tree.treeData"
                        :props="tree.treeProps" :isSelectParent="true" :checkStrictly="tree.checkStrictly"
                        :clearable="tree.clearable" :popoverWidth="tree.popoverWidth" v-model="filter.Department"
                        style="padding: 0 5px; width: 45%; float: left"></select-department-tree-component>

                    <el-select v-model="filter.Machine" multiple collapse-tags clearable class="filter"
                        popper-class="select-dropdown" :placeholder="$t('SelectMachine')">
                        <el-option v-for="item in deviceFilter" :key="item.value" :label="`(${item.value}) ${item.label}`"
                            :value="item.value">
                        </el-option>
                    </el-select> -->

                        <data-table-function-component style="margin-right:55px; margin-top: calc(45vh + 5px);"
                            :gridColumnConfig.sync="columns" :isHiddenEdit="true" :isHiddenDelete="true"
                            :isHiddenSearch="true" :showButtonColumConfig="true"
                            :showButtonInsert="false"></data-table-function-component>

                        <el-button @click="toggleFullScreen" style="float: right;">
                            <img v-show="isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_off.svg"
                                style="width: 18px; height: 18px" />
                            <img v-show="!isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_on.svg"
                                style="width: 18px; height: 18px" />
                        </el-button>
                    </div>

                    <div>
                        <el-table :data="filteredTableData" class="table-log-realtime-monitoring"
                            :row-class-name="tableRowClassName" :max-height="maxHeight">
                            <el-table-column v-for="column in columns"
                                :label-class-name="column.display === false ? 'hid' : ''" :key="column.prop"
                                :fixed="column.fixed || false" v-if="column.display === true" v-bind="column"
                                :label="$t(column.label)"></el-table-column>
                        </el-table>
                    </div>
                    <el-dialog :title="$t('ChooseTimePeriod')" :visible.sync="isShowAttendance" custom-class="customdialog" :before-close="Cancel" >
            <el-form
              :model="TimeForm"
              :rules="rule"
              ref="TimeForm"
              label-width="168px"
              label-position="top"
              @keyup.enter.native="DownloadAttendanceData"
            >
           
             
              <el-form-item :label="$t('OffPeriod')" prop="ToTime">
                <el-input-number
                      ref="DownloadFromPreviousDay"
                      v-model="PreviousDays"
                      style="width: 90%"
                      :min="1" :max="254"
                    ></el-input-number>
                 <span style="margin-left: 5px">{{ $t("SecondOff") }}</span>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">{{ $t("Cancel") }}</el-button>
              <el-button class="btnOK" type="primary" @click="DownloadAttendanceData">{{ $t("OK") }}</el-button>
            </span>
          </el-dialog>
                </el-main>
            </div>
        </el-container>
    </div>
</template>
<style lang="scss">

.fullscreen-container {
    width: 100vw;
    height: 100vh;
    position: fixed;
    top: 0;
    left: 0;
    background-color: #fff;
    transition: all 0.3s;
}

.full-screen {
    width: 100%;
    height: 100%;
    overflow: hidden;
}

.ac-door-management-container {
    .customdialog .el-input--small input{
        height: auto !important;
    }
    .realtime-info {
        width: 100% !important;
        height: 45vh !important;

        .realtime-info__textbox {
            float: right;
            width: 75%;
            margin-top: calc((11vh - 6vh) / 2);
            margin-bottom: calc((11vh - 6vh) / 2);
            height: 6vh;

            .el-input__inner {
                height: 6vh;
                font-size: 26px;
            }
        }

        .item:hover {
            border: 1px solid #0F0;
        }

        .realtime-info__label {
            display: inline-block;
            width: 25% !important;
            margin-top: calc((11vh - 6vh) / 2);
            margin-bottom: calc((11vh - 6vh) / 2);
            height: 6vh;
            line-height: 6vh;
            font-size: 26px;
            font-weight: bold;
            padding-left: 5px;
        }
    }
    .logmonitoring .el-table {
        margin-top: 10px;
    }

    .el-table__body-wrapper {
        height: 90% !important;
        overflow-y: auto !important;
    }

    // .el-main.bgHome.log-realtime-monitoring .table-log-realtime-monitoring {
    //     width: 100%;
    //     height: calc(100vh - 360px);
    // }

    .el-main.bgHome.log-realtime-monitoring {
        max-height: 100vh !important;

        .table-log-realtime-monitoring {
            width: 100%;
            height: calc(50vh - 110px) !important;
            max-height: 100vh !important;

            .el-table__body-wrapper {
                max-height: calc(50vh - 160px) !important;
            }
        }
    }

    div.item {
        vertical-align: top;
        display: inline-block;
        text-align: center;
        width: 120px;
    }

    .caption {
        display: block;
    }

    .el-main.bgHome.el-main__full-screen {
        max-height: 100vh !important;

        .table-log-realtime-monitoring {
            width: 100%;
            height: calc(50vh - 60px) !important;
            max-height: 100vh !important;

            .el-table__body-wrapper {
                max-height: calc(50vh - 110px) !important;
            }
        }
    }

    span#byline {
        float: left;
    }

    .filterContainer {
        margin-top: 5px;
    }

    .filter {
        margin-left: 5px;
        width: 40%;
    }

    .el-select-dropdown.el-popper {
        max-width: none !important;
    }

    .warningrow {
        color: red;
    }

    .hid {
        display: none;
    }

    img.active {
        border: 1px solid red;
    }
}


</style>
<script src="./ac-door-management-component.ts"></script>
