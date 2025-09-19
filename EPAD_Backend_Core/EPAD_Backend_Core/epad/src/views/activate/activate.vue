<template>
  <div >
    <div class="formlogin-wrapper">
      <div id="LogoLogin"></div>
      <div id="formInput">
        <el-form
          :model="dataModel"
          :rules="dataRules"
          ref="ruleForm"
          label-position="top"
          label-width="200px"
        >
          <el-form-item prop="LicenseKey" :label="$t('LicenseKey')">
            <el-input v-model="dataModel.LicenseKey"></el-input>
          </el-form-item>

          <el-form-item label="ID2" prop="ComputerIdentify" v-if="dataModel.IsOffline">
            <el-input :readonly="true" ref="inputID2" v-model="ComputerIdentify">
              <el-button @click="copyComputerIdentify" icon="el-icon-document-copy" slot="append"></el-button>
            </el-input>
          </el-form-item>

          <el-form-item>
            <el-upload
              :auto-upload="false"
              :multiple="false"
              :on-change="uploadLicenseFile"
              accept=".bin"
              action
              drag
              v-if="dataModel.IsOffline"
            >
              <i class="el-icon-upload"></i>
              <div class="el-upload__text">
                Drop file here or
                <em>click to upload</em>
              </div>
              <div class="el-upload__tip" slot="tip">{{ this.$t('BinFileSize5kb') }}</div>
            </el-upload>
          </el-form-item>

          <el-form-item>
            <el-button
              id="btn"
              class="disable"
              type="primary"
              round
              @click="confirmLicense"
            >{{ $t("Activate") }}</el-button>
            <span class="forgetPW">
              <a href="#" @click="$router.push('/login')" id="lblForgetPW">{{ $t('Login') }}</a>
            </span>
          </el-form-item>
        </el-form>
      </div>
    </div>
  </div>
</template>

<script src="./activate.ts"></script>
<style scoped>
body {
  overflow: scroll;
}

.formlogin-wrapper {
  width: 35%;
}

#formInput {
  width: 100%;
}

.el-button--small .el-button--small.is-round {
  padding: 8px 30px;
}

#btn {
  height: 36px;
  width: 140px;
}

.el-form .el-form-item__content {
  height: 32px;
}
</style>
