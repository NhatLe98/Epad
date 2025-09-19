<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("UserGuide") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome_guide two-columns">
        <div class="text_guide" v-for="item in listGuide" :key="item.link">
          <el-link :href="item.link" target="_blank" class="text_link" style="font-weight: bolder">{{item.title}}</el-link>
        </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./user-guide-page-component.ts"></script>
<style scoped>
.bgHome {
  display: flex;
  flex-wrap: wrap;
}

.text_link .el-link--inner {
  font-size: 30px;
}

.text_guide {
  width: calc(50% - 50px); /* Adjust width for two columns and add some gap */
  margin: 8px; /* Adjust margin for spacing between items */
  box-sizing: border-box;
  margin-bottom: 30px;
}

.two-columns {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-content: flex-start;
}

</style>