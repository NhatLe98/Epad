<template>
    
        <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("LogMonitoringOnline") }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent />
                    </el-col>
                </el-row>
            </el-header>
            <div :class="{ 'full-screen fullscreen-container': isFullScreen() }" class="logmonitoring-container" style="overflow-y: hidden;">
                <el-main class="bgHome logmonitoring" style="height: 100vh !important;" :class="{ 'el-main__full-screen': isFullScreen() }">
                <div style="height:46px;">
                    <el-alert style="height:100%;" class="center16px" :title="$t('ConnectToMonitoringServerSuccessfully')"
                        type="success" :closable="false" effect="dark" v-if="isConnect"></el-alert>
                    <el-alert style="height:100%;" class="center16px" :title="$t('NotConnectServerMonitoring')" type="error"
                        :closable="false" effect="dark" v-if="!isConnect"></el-alert>
                </div>
                <div class="filterContainer">
                    <!-- <el-select v-model="filter.Department"
                               multiple
                               collapse-tags
                               clearable
                               class="filter"
                               popper-class="select-dropdown"
                               :placeholder="$t('SelectDepartment')">
                        <el-option v-for="item in lstDepartment"
                                   :key="item.value"
                                   :label="item.label"
                                   :value="item.value">
                        </el-option>
                    </el-select> -->

                    <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll" :multiple="tree.multiple"
                        :placeholder="$t('SelectDepartment')" :disabled="tree.isEdit" :data="tree.treeData"
                        :props="tree.treeProps" :isSelectParent="true" :checkStrictly="tree.checkStrictly"
                        :clearable="tree.clearable" :popoverWidth="tree.popoverWidth" v-model="filter.Department"
                        style="padding: 0 5px; width: 45%; float: left"></select-department-tree-component>

                    <el-select v-model="filter.Machine" multiple collapse-tags clearable class="filter"
                        popper-class="select-dropdown" :placeholder="$t('SelectMachine')">
                        <el-option v-for="item in deviceFilter" :key="item.value" :label="`(${item.value}) ${item.label}`"
                            :value="item.value">
                        </el-option>
                    </el-select>

                    <data-table-function-component style="top:80px; width:auto; margin-right: 55px;" :gridColumnConfig.sync="columns"
                        :isHiddenEdit="true" :isHiddenDelete="true" :isHiddenSearch="true" :showButtonColumConfig="true"
                        :showButtonInsert="false"></data-table-function-component>
                    
                    <el-button @click="toggleFullScreen" style="float: right;">
                        <img v-show="isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_off.svg" 
                        style="width: 18px; height: 18px" />
                        <img v-show="!isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_on.svg" 
                        style="width: 18px; height: 18px" />
                    </el-button>
                </div>

                <div>
                    <el-table :data="filteredTableData" class="table-logmonitoring" :row-class-name="tableRowClassName"
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

.logmonitoring-container {
    .logmonitoring .el-table {
        margin-top: 10px;
    }

    .el-table__body-wrapper {
        height: 95% !important;
        overflow-y: auto !important;
    }

    .el-main.bgHome.logmonitoring .table-logmonitoring {
        width: 100%;
        height: calc(100vh - 185px);
    }

    .el-main.bgHome.el-main__full-screen
    {
        max-height: 100vh !important;
        .table-logmonitoring {
            width: 100%;
            height: calc(100vh - 160px) !important;
            max-height: 100vh !important;
            .el-table__body-wrapper{
                max-height: calc(100vh - 220px) !important;
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
}

</style>
<script src="./log-monitoring-component.ts"></script>
