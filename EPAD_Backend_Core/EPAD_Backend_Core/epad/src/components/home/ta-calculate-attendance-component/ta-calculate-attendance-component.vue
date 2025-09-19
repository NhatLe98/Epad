<template>
    <AppLayout :formName="$t('TACalculateAttendance')" :showMasterEmployeeFilter="true">
       
      <div v-for="tab in tabConfig" :key="tab.id">
            <template v-if="tab.active">
              <t-button-bar
                :model="tab"
                
                @onInsertClick="onInsertClick"
                @onEditClick="onEditClick(tab)"
                @onDeleteClick="onDeleteClick"
                @onCommand="onCommand($event)"
              ></t-button-bar>
            </template>
          </div>
          <el-tabs type="card" @tab-click="handleTabClick">      
            <el-tab-pane
              v-for="(tab, idx) in tabConfig"
              :key="tab.tabName"
              :label="tab.title"
              :name="tab.Name"
              :id="tab.id"
            >        
              <keep-alive>
                <component
                  :is="tab.componentName"
                  :ref="`${tab.tabName}-tab-ref`"
                  @selectedRowKeys="setSelected($event, idx)"
                  @filterModel="setFilterModel($event, idx)"
                  @showImportExcel="tab.showMore"
                  :idEnum="tab.id"
                  :tabName="tab.tabName"
                  :isActive="tabActive"
                  :showMore="tab.showMore"
                />
              </keep-alive>
            </el-tab-pane>
          </el-tabs>
    </AppLayout>
  </template>
  <script src="./ta-calculate-attendance-component.ts"></script>