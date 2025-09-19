<template>
  <div id="bgCamera">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("HanwhaCameraTest") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      
      <el-main class="bgCamera main" style="padding-bottom: 0;">
        <el-row>
          <el-col :span="12" style="padding-right: 5px;">
            <!-- {{ resCamera }} -->
            <span style="display: block; text-align: center; margin-bottom: 5px;">Stream</span>
            <el-select 
              v-model="selectingCamera" 
              :multiple="false"
              :clearable="true"
              style="margin-bottom: 5px;"
            >
              <el-option 
                  v-for="item in resCamera" 
                  :key="item.id" 
                  :label="item.name" 
                  :value="item.id"
              ></el-option>
            </el-select>
            <div>
              <img id="motionjpeg" style="max-width: 100%;" :src="streamSrc"/>
            </div>
            <!-- <el-button
              @click="saveImage"
              color="primary"
              type="primary"
              size="normal"
              style="margin-right: 5px;"
              >Save image</el-button
            > -->
            <el-button
              v-show="streamSrc != ''"
              @click="captureImage"
              color="primary"
              type="primary"
              size="normal"
              >Capture image</el-button
            >
            <el-button
              v-show="streamSrc != ''"
              @click="zoomStreamWisenetClient"
              color="primary"
              type="primary"
              size="normal"
              style="margin-left: 5px;"
              >Zoom</el-button
            >
            <el-button
              v-show="streamSrc != ''"
              @click="unzoomStreamWisenetClient"
              color="primary"
              type="primary"
              size="normal"
              style="margin-left: 5px;"
              >Unzoom</el-button
            >
            <!-- <video id="video" controls ref="cameraVideo"></video> -->
          </el-col>
          <el-col :span="12">
            <span style="display: block; text-align: center; margin-bottom: 42px;">Capture</span>
            <div>
              <img id="captureImage" style="max-width: 100%;"/>
            </div>
          </el-col>
        </el-row>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./hanwhaCameraTest.ts"></script>

<style scoped>
.el-collapse{
  border: 0;
  .dashboard-collapse{
  .el-collapse-item__header{
    background-color: transparent !important;
    color: black;
    font-weight: bold;
  }
  .el-collapse-item__content{
    background-color: transparent;
  }
  .el-collapse-item__wrap{
    border: 0;
  }
}
}
.vue-grid-layout {
    .vue-grid-item{
      background-color: white;
      border-radius: 10px;
      /* box-shadow: 0 4px 8px 0 gray; */
    }
}

.vue-grid-item:not(.vue-grid-placeholder) {
    background: white !important;
    /* border: 1px solid black; */
}

.vue-grid-item .resizing {
    opacity: 0.9;
}

.vue-grid-item.static {
    background: white !important;
}
.vue-grid-item .static {
    background: white !important;
}

.vue-grid-item .text {
    font-size: 24px;
    text-align: center;
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    margin: auto;
    height: 100%;
    width: 100%;
}

.vue-grid-item .no-drag {
    height: 100%;
    width: 100%;
}

.vue-grid-item .minMax {
    font-size: 12px;
}

.vue-grid-item .add {
    cursor: pointer;
}

.vue-draggable-handle {
    position: absolute;
    width: 20px;
    height: 20px;
    top: 0;
    left: 0;
    background-position: bottom right;
    padding: 0 8px 8px 0;
    background-repeat: no-repeat;
    background-origin: content-box;
    box-sizing: border-box;
    cursor: pointer;
}
</style>