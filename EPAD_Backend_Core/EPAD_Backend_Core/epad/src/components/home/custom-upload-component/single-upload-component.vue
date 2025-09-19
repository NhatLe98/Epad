<template>
  <div :style="!multiple ? 'display:inline-flex' : ''">

    <el-button type="primary" size="mini" class="input-btn" @click="test">{{ $t(buttonName)
    }}<input type="file" :name="id" :accept="getAccept" v-bind="$attrs" @change="fileSelect" v-on="$listeners" :id="id"
        :multiple="multiple" class="my_file" /></el-button>

    <div v-if="model == null || model.length == 0" class="file-item">
      <div class="no-file">
        <span :title="$t('NoFileSelected')">{{ $t("NoFileSelected") }}</span>
      </div>
    </div>
    <div v-else v-for="(f, i) in model" :key="i" class="file-item">
      <div class="name">
        <span :title="f.Name" @click="openFile(f)">{{ f.Name }}</span>
      </div>
      <!-- <el-button
        size="mini"
        icon="el-icon-download"
        circle
        @click="downloadFile(f)"
      /> -->
      <el-button size="mini" icon="el-icon-delete" circle @click="deleteFile(f)" />
    </div>
    <!-- 
    <el-col :span="20">
      
    </el-col>
    <el-col :span="4"> </el-col> -->

    <!-- <el-button
        size="mini"
        
        type="primary"
        class="input-btn"
        
        @click="downloadFile()"
      >
      {{ $t("resources.download-file")}}</el-button> -->
  </div>
</template>

<script src="./single-upload-component.ts" />

<style lang="scss" scoped>
.file-item {
  width: 100%;
  height: 30px;
  display: flex;
  justify-content: space-between;
  align-items: center;

  .name {
    width: calc(100% - 50px);
    display: block;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;

    &:hover span {
      color: blue;
      text-decoration: underline;
      cursor: pointer;
    }
  }

  .no-file {
    background-color: #f5f7fa;
    width: 100%;
  }

  button {
    display: none;
    border: none;
  }

  &:hover button {
    display: inline-block;
  }
}

input {
  opacity: 0;
  overflow: hidden;
  top: 0;
  left: 0;
}

.my_file {
  display: none;
}

.input-btn {

  margin-right: 10px;
}
</style>
