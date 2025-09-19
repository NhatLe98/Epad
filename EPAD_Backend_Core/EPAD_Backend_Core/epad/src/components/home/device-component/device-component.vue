<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("FormListMachine") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main id="bgHome">
        <div>
          <data-table-component
            :get-data="getData"
            ref="table"
            :columns="columns"
            
          >
          </data-table-component>
        </div>
        <div>
          <el-row>
            <el-col :span="12" class="left">
              <el-button
                class="classLeft"
                id="btnFunction"
                type="primary"
                @click="Insert"
                round
                >{{ $t("Insert") }}</el-button
              >
              <el-button
                class="classLeft"
                id="btnFunction"
                type="primary"
                round
                >{{ $t("Edit") }}</el-button
              >
              <!-- <el-button class="classLeft" id="btnFunction" type="primary" round
                >{{ $t("Export") }}</el-button
              > -->
            </el-col>
            <el-col :span="12">
              <el-button
                class="classRight"
                id="btnFunction"
                type="primary"
                round
                >{{ $t("Delete") }}</el-button
              >
            </el-col>
          </el-row>
        </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./device-component.ts"></script>
