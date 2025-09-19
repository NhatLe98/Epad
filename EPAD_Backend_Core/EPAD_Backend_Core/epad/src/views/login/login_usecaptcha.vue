<template>
  <div v-loading="loading">
    <el-row
      v-if="isLogin"
      :gutter="20"
      style="padding-left:0px;padding-right:0px; margin-left:0px;margin-right:0px"
    >
      <el-col :span="12" style="padding-left:0px;padding-right:0px;">
        <div id="LogoLogin"></div>
        <div id="formInput">
          <el-form
            :model="ruleForm"
            :rules="rules"
            ref="ruleForm"
            label-position="top"
            label-width="100px"
          >
            <el-form-item prop="UserName">
              <a id="lbl" @click="focus('inputUserName')">{{ $t("Email") }}</a>
              <el-input id="InpuLogin" ref="inputUserName" v-model="ruleForm.UserName" name="email" autofocus=""></el-input>
            </el-form-item>
            <el-form-item style="width:360px" prop="Password">
              <a id="lbl" @click="focus('inputPassword')">{{ $t("Password") }}</a>
              <el-input
                ref="inputPassword"
                show-password
                id="InpuLogin"
                type="password"
                v-model="ruleForm.Password"
                name="password"
                @keyup.enter.native="login"
              ></el-input>

              <!-- <div v-if="onLogin && errors.has('password')"  style="margin-top:-15px;color:red">{{ $t("ValidatePassword") }}</div> -->
            </el-form-item>
            <el-form-item style="width:360px">
              <el-checkbox v-model="isRemember" class="checkboxRememerLogin">
                <a id="lbl" class="lblRemember">{{ $t("Remember") }}</a>
              </el-checkbox>
              <a href="#" @click="forgetPassword" id="lblForgetPW">{{ $t("ForgetPW") }}</a>
            </el-form-item>
            <Recaptcha2 v-if="!isHuman" site-key="6Ldvt-YUAAAAAFFmKjG0dk3sblPJV65s7k0tM13t" @change="robotChange"></Recaptcha2>
            <el-form-item >
              <el-button id="btn" class="btnLogin disable" type="primary" round @click="login" disabled v-if="isDisableLogin">
                {{ $t("Login") }}
              </el-button>
              <el-button id="btn" class="btnLogin" type="primary" round @click="verifyHuman" v-else-if="!isHuman">
                {{ $t("Login") }}
              </el-button>
              <el-button id="btn" class="btnLogin" type="primary" round @click="login" v-else>
                {{ $t("Login") }}
              </el-button>
                
              
            </el-form-item>

            <el-form-item>
              <a id="NoEmail">{{ $t("NoAccount") }}</a>
              <a id="Register">{{ $t("RegisterNow") }}</a>
            </el-form-item>
          </el-form>
        </div>
      </el-col>
      <el-col
        :span="12"
        style="margin-left:0px;margin-right:0px;padding-left:0px;padding-right:0px;"
      >
        <img src="../../assets/background/group-2.png" id="BackgroundLogin" />
      </el-col>
    </el-row>

    <el-row
      v-if="isForgetPassword"
      :gutter="20"
      style="padding-left:0px;padding-right:0px; margin-left:0px;margin-right:0px"
    >
      <el-col :span="12" style="padding-left:0px;padding-right:0px;">
        <div id="LogoLogin"></div>
        <div id="TtileLogin">{{ $t("ForgetPW") }}</div>
        <div id="formInput">
          <el-form label-position="top" label-width="100px" :model="ruleForm" :rules="rules">
            <el-form-item prop="UserName">
              <a id="lbl">{{ $t("Email") }}</a>
              <el-input id="InpuLogin" v-model="ruleForm.UserName"></el-input>
            </el-form-item>

            <el-form-item>
              <el-button
                v-if="isChangePassword"
                id="btnResetPW"
                type="primary"
                @click="changePassword"
              >
                {{$t('ChangePassword')}}
                <i class="el-icon-refresh" />
              </el-button>
              <el-button
                v-else
                id="btnResetPW"
                type="primary"
                :loading="sendLoading"
                round
                @click="restPassword"
              >
                {{ $t("ResetPassword") }}
                <!-- <i
                  class="el-icon-arrow-right el-icon-right"
                  id="iconRightBtn"
                />-->
              </el-button>
              <el-button id="btnBackHome" type="primary" round @click="backLogin">
                {{ $t("BackToHome") }}
                <i class="el-icon-refresh-left"></i>
              </el-button>
            </el-form-item>
          </el-form>

          <el-dialog :visible.sync="dialogChangePassoword" width="660px" top="5vh">
            <el-form
              :model="resetForm"
              :rules="ruleReset"
              label-width="168px"
              label-position="right"
              ref="resetForm"
            >
              <el-row style="margin-bottom:39px" >
                <el-col style="width:145px">
                  <div class="imgPopupBtnAdd" />
                </el-col>
                <el-col style="float:left; margin-left:25px; width:410px"
                  id="NamePopup">{{ $t("ChangePassword") }}</el-col>
              </el-row>
              <el-form-item @click.native="focus('Code')" :label="$t('Code')" prop="Code">
                <el-input ref="Code" id="Input" v-model="resetForm.Code"></el-input>
              </el-form-item>
              <el-form-item @click.native="focus('UserName')" :label="$t('UserName')" prop="UserName">
                <el-input ref="UserName" id="Input" v-model="resetForm.UserName"></el-input>
              </el-form-item>
              <el-form-item @click.native="focus('NewPassword')" :label="$t('NewPassword')" prop="NewPassword">
                <el-input ref="NewPassword" id="Input" v-model="resetForm.NewPassword" type="password"></el-input>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel('resetForm')">
                <a>
                  {{
                  $t("Cancel")
                  }}
                </a>
              </el-button>
              <el-button class="btnOK" type="primary" @click="submitChangePassword">
                <a>{{ $t("OK") }}</a>
              </el-button>
            </span>
          </el-dialog>
        </div>
      </el-col>
      <el-col
        :span="12"
        style="margin-left:0px;margin-right:0px;padding-left:0px;padding-right:0px;"
      >
        <img src="../../assets/background/group-2.png" id="BackgroundLogin" />
      </el-col>
    </el-row>
    <div>
      <el-dialog title="Thông báo" :visible.sync="showDialog" width="35%" height="35%" center>
        <span>Hệ thống đang gửi mã khôi phục mật khẩu về tài khoản. Vui lòng đợi..</span>
      </el-dialog>
    </div>
  </div>
</template>

<script src="./login.ts"></script>
<style scoped>
body {
  overflow: scroll;
}
</style>
