<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("Home") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      
    <el-main class="bgHome main">
        <div class="listmachine">
          <DeviceInfoForHomeComponent/>
        </div>
        <div class="liveattend">
          <LiveMachineComponent/>
        </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./home-component.ts"></script>

<style>
#bgHome {
  display: flex;
  flex-direction: column;
  margin: 0;
  padding: 0;
}
.header {
  width: 100%;
  height: 51px;
  display: block;
  box-shadow: 0px 4px 4px rgba(123, 123, 123, 0.25);
}
.header-title {
  display: block;
  width: 50%;
  float: left;
  padding: 12px 0 0 18px;
  font-size: 18px;
  font-weight: 600;
  font-family: 'Noto Sans';
}
.header-function {
  display: block;
  width: 50%;
  float: left;
}
.el-main.bgHome.main {
  display: flex;
  justify-content: flex-start;
  padding-right: 0;
  padding-top: 6px;
}
.listmachine {
  width: 70%;
  display: block;
  float: left;
}
.liveattend {
  width: 28%;
  margin-left: 2%;
  display: block;
  float: left;
  margin-top: 18px;
  border-left: 0.5px solid #767676;
  border-top: 0.5px solid #767676;
  border-top-left-radius: 40px;
}
</style>