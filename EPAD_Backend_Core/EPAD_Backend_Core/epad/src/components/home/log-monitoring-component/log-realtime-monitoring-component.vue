<template>
    
        <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("LogRealtimeMonitoringOnline") }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent />
                    </el-col>
                </el-row>
            </el-header>
            <div :class="{ 'full-screen fullscreen-container': isFullScreen() }" style="overflow-y: hidden;">
                <el-main class="bgHome log-realtime-monitoring" style="height: 100vh !important;" :class="{ 'el-main__full-screen': isFullScreen() }">
                <!-- <div style="height:46px;">
                    <el-alert style="height:100%;" class="center16px" :title="$t('ConnectToMonitoringServerSuccessfully')"
                        type="success" :closable="false" effect="dark" v-if="isConnect"></el-alert>
                    <el-alert style="height:100%;" class="center16px" :title="$t('NotConnectServerMonitoring')" type="error"
                        :closable="false" effect="dark" v-if="!isConnect"></el-alert>
                </div> -->
                <div style="height: 45vh !important;">
                    <el-row>
                        <el-col :span="6">
                            <div class="realtime-info">
                                <img :src="avatar" 
                                    style="width: 80% !important; height: 40vh !important;
                                        max-width: 80% !important; max-height: 40vh !important;
                                        border: 3px solid black;" 
                                    class="avatar" />
                            </div>
                        </el-col>
                        <el-col :span="18">
                            <div class="realtime-info">
                                <span class="realtime-info__label">{{$t('MCC')}}</span>
                                <el-input class="realtime-info__textbox" readonly v-model="employeeATID"></el-input>
                                <span class="realtime-info__label">{{$t('FullName')}}</span>
                                <el-input class="realtime-info__textbox" readonly v-model="fullName"></el-input>
                                <span class="realtime-info__label">{{$t('Department')}}</span>
                                <el-input class="realtime-info__textbox" readonly v-model="departmentName"></el-input>
                                <span class="realtime-info__label">{{$t('LogTime')}}</span>
                                <el-input class="realtime-info__textbox" readonly v-model="logTime"></el-input>
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
                    <el-table :data="filteredTableData" class="table-log-realtime-monitoring" :row-class-name="tableRowClassName"
                        :max-height="maxHeight">
                        <el-table-column v-for="column in columns" :label-class-name="column.display === false ? 'hid' : ''"
                            :key="column.prop" :fixed="column.fixed || false" v-if="column.display === true" v-bind="column"
                            :label="$t(column.label)"></el-table-column>
                    </el-table>
                </div>
            </el-main>
            </div>
        </el-container>
    </div>
    
</template>
<style lang="scss">
.realtime-info{
    width: 100% !important; 
    height: 45vh !important;
    .realtime-info__textbox{
        float: right;
        width: 75%;
        margin-top: calc((11vh - 6vh) / 2);
        margin-bottom: calc((11vh - 6vh) / 2);
        height: 6vh;
        .el-input__inner{
            height: 6vh;
            font-size: 26px;
            font-weight: bold;
        }
    }
    .realtime-info__label{
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

.logmonitoring .el-table {
    margin-top: 10px;
}

.table-log-realtime-monitoring {
    .el-table__body-wrapper {
        height: 90% !important;
        overflow-y: auto !important;
    }
}

// .el-main.bgHome.log-realtime-monitoring .table-log-realtime-monitoring {
//     width: 100%;
//     height: calc(100vh - 360px);
// }

.el-main.bgHome.log-realtime-monitoring
{
    max-height: 100vh !important;
    .table-log-realtime-monitoring {
        width: 100%;
        height: calc(50vh - 110px) !important;
        max-height: 100vh !important;
        .el-table__body-wrapper{
            max-height: calc(50vh - 160px) !important;
        }
    }
}

.el-main.bgHome.el-main__full-screen
{
    max-height: 100vh !important;
    .table-log-realtime-monitoring {
        width: 100%;
        height: calc(50vh - 60px) !important;
        max-height: 100vh !important;
        .el-table__body-wrapper{
            max-height: calc(50vh - 110px) !important;
        }
    }
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
</style>
<script src="./log-realtime-monitoring-component.ts"></script>
