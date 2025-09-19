<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName"></span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main id="bgHome">
        <div>
          <h1>Non privilege</h1>
        </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./personal-access-token.ts"></script>