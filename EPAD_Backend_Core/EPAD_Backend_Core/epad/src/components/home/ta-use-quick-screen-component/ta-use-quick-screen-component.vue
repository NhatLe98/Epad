<template>
  <AppLayout :formName="$t('TAUseQuickScreen')" :showMasterEmployeeFilter="true">
    
    <!-- <el-button v-if="!hideTextInTab" @click="clickHideTextInTab"><img src="@/assets/icons/Button/left.png" class="tab-icon-hide"/></el-button>
    <el-button v-else @click="clickHideTextInTab"><img src="@/assets/icons/Button/right.png" class="tab-icon-hide" /></el-button> -->
    <!-- <div v-for="tab in tabConfig" :key="tab.id">
      <template v-if="tab.active">
        <t-button-bar
          :model="tab"
          @onInsertClick="onInsertClick"
          @onEditClick="onEditClick(tab)"
          @onDeleteClick="onDeleteClick"
          @onCommand="onCommand($event)"
        ></t-button-bar>
      </template> 
    </div> -->
    <el-tabs v-loading="loading" type="card" @tab-click="handleTabClick" class="quick_screen">
      <el-tab-pane
        v-for="(tab, idx) in tabConfig"
        :key="tab.tabName"
        :label="tab.title"
        :name="tab.Name"
        :id="tab.id"
      >
        <template #label>
          <div class="tab-label">
            <img  v-if="!hideTextInTab" :src="tab.iconImage" alt="" class="tab-icon" />
            <span class="tab-title">{{ tab.title }}</span>
          </div>
        </template>
        <template>
            <t-button-bar
              :model="tab"
              @onInsertClick="onInsertClick"
              @onEditClick="onEditClick(tab)"
              @onDeleteClick="onDeleteClick"
              @onCommand="onCommand($event)"
            ></t-button-bar>
          </template> 
        <keep-alive style="margin-top: -10px;">
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
            :listEmployeeATID="tab.listEmployeeATID"
            :departmentData="tab.departmentData"
            @dataDepartment="setDataDepartmentFilter"
            :departmentFilter="departmentFilter"
            :employeeFilter="employeeFilter"
          />
        </keep-alive>
       
      </el-tab-pane>
    </el-tabs>
  </AppLayout>
  
</template>
  <script src="./ta-use-quick-screen-component.ts"></script>
  <style lang="scss">
  .quick_screen{
    margin-top: -28px;
    .el-tabs__header{
      height: 80px !important;
      .el-tabs__nav-wrap{
        height: 100%;
        .el-tabs__nav-scroll{
          height: 100%;
          .el-tabs__nav {
            height: 100%;
            .el-tabs__item {
              height: 100%;
            }
          }
        }
      }
    }

  
  }

  .quick_screen{
    .el-tabs__content{
      margin-top: -10px !important;
    }
  }

  .el-tabs__active-bar {
    bottom: 0; /* Adjust the active bar position if needed */
  }

  .tab-label {
    display: flex;
    align-items: start;
    flex-direction: column;
    align-content: center;
    align-items: center;
  }

.tab-icon {
  width: 40px;
  height: 40px; 
  margin-right: 5px; 
  vertical-align: middle;
}


.tab-icon-hide-text {
  width: 30px;
  height: 30px; 
  margin-right: 10px; 
  margin-left: 10px; 
  vertical-align: middle;
}

</style>