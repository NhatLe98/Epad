<template>
  <div id="divNav" ref="divNav">
    <el-row style="margin-left:0px;margin-right:0px;padding-left:0px;padding-right:0px;">
      <el-col :span="5" id="Home" class="divColMain">
        <MenuComponent></MenuComponent>
      </el-col>

      <el-col :span="19" id="Content" class="divColMain">
        <div id="bgHome">
          <el-container>
            <el-header>
              <el-row>
                <el-col :span="12">
                  <span id="FormName">{{ $t(formName) }}</span>
                </el-col>
                <el-col :span="12">
                  <HeaderComponent></HeaderComponent>
                </el-col>
              </el-row>
            </el-header>
            <router-view :key="$route.fullPath" ></router-view>
          </el-container>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<script src="./main.ts"></script>
<style scoped>
  .divColMain {
    margin-left:0px;
    margin-right:0px;
    padding-left:0px;
    padding-right:0px;
  }
</style>