<template>
  <div v-loading="loading" style="height: 100vh;">
    <el-row style="height: 100%; width: 100%;">
      <el-col :span="10" style="height: 100%;">
        <div style="height: 100%; width: 100%; position: relative;" v-if="clientName == 'Mondelez'">
          <img src="../../assets/images/background_login.png" style="height: 100%; width: 100%; display: block;" alt="" />
          <div class="epad-title-description">
            <div>eGCS</div> <br /> {{ $t('SecurityControlManagementSoftware') }}
          </div>
        </div>
        <div style="height: 100%; width: 100%; position: relative;">
          <img src="../../assets/images/background_login.png" style="height: 100%; width: 100%; display: block;" alt="" />
          <div class="epad-title-description">
            <div>ePAD</div> <br /> {{ $t('TimeAttendanceManagementSoftware') }}
          </div>
        </div>
      </el-col>
      <el-col :span="14" style="height: 100%;">
        <div style="height: 100%; width: 100%;">
          <div class="formlogin-wrapper" v-if="isLogin" style="height: 100%; width: 100%;">
            <div id="LogoLogin"></div>
            <div id="formInput">
              <div style="width: 100%; text-align: center; font-weight: bold; font-size: 64px; margin-bottom: 25%;
                  font-family: sans-serif;">
                {{ $t('Welcome') }}
              </div>
              <el-form :model="ruleForm" :rules="rules" ref="ruleForm" label-position="top" label-width="100px">
                <el-form-item prop="UserName" style="margin-bottom: 15px !important;">
                  <!-- <a id="lbl" @click="focus('inputUserName')">{{ $t("Email") }}</a> -->
                  <el-input id="InputEmailLogin" class="login-input" ref="inputUserName" v-model="ruleForm.UserName"
                    name="email" :autofocus="true" prefix-icon="el-icon-message" :placeholder="$t('Email')"></el-input>
                </el-form-item>
                <el-form-item style="width:360px" prop="Password">
                  <!-- <a id="lbl" @click="focus('inputPassword')">{{ $t("Password") }}</a> -->
                  <el-input @keyup.enter.native="login" ref="inputPassword" id="InputPWLogin" class="login-input"
                    type="password" v-model="ruleForm.Password" name="password" prefix-icon="el-icon-lock"
                    :placeholder="$t('Password')"></el-input>

                  <!-- <div v-if="onLogin && errors.has('password')"  style="margin-top:-15px;color:red">{{ $t("ValidatePassword") }}</div> -->
                </el-form-item>
                <el-form-item style="width:360px">
                  <el-checkbox v-model="isRemember" class="checkboxRememerLogin">
                    <a id="lbl" class="lblRemember" style="font-size: 16px; font-weight: bold;">{{ $t("SavePassword") }}</a>
                  </el-checkbox>
                  <a href="#" @click="forgetPassword" id="lblForgetPW">{{ $t("ForgetPW") }}</a>
                </el-form-item>

                <el-form-item>
                  <!--<el-button id="btn" class="btnLogin disable" type="primary" round @click="login" disabled v-if="isDisableLogin">
              {{ $t("Login") }}
            </el-button>-->
                  <el-select v-model="lang" @change="change" placeholder="Tiếng Việt" class="select-lang"
                    style="width: 25%;">
                    <template slot="prefix">
                      <div :class="getLanguageForRenderFlag"></div>
                    </template>
                    <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value"
                      class="option-language">
                      <div class="title">{{ item.label }}</div>
                      <div :class="item.image"></div>
                    </el-option>
                  </el-select>
                  <el-button id="btn" class="btnLogin" type="primary" round @click="login">
                    {{ $t("LOGIN") }}
                  </el-button>
                </el-form-item>

                <!-- <el-form-item>
                  <a id="NoEmail">{{ $t("NoAccount") }}</a>
                  <a id="Register">{{ $t("RegisterNow") }}</a>
                </el-form-item> -->
              </el-form>
            </div>
          </div>
          <div class="formlogin-wrapper" v-if="isForgetPassword" style="height: 100%; width: 100%;">
            <div id="LogoLogin"></div>
            <div id="TtileLogin">{{ $t("ForgetPW") }}</div>
            <div id="formInput">
              <el-form label-position="top" :model="ruleForm" :rules="rules">
                <el-form-item prop="UserName">
                  <span class="resetPW"><a style="letter-spacing: -0.12px" @click="focus('EmailForgetPW')" id="lbl">{{
                    $t("EmailForgetPW") }}</a></span>
                  <el-input @keyup.enter.native="restPassword" ref="EmailForgetPW" id="InputForgetLogin"
                    v-model="ruleForm.UserName"></el-input>
                </el-form-item>

                <el-form-item style="width:360px">
                  <el-button id="btnResetPW" type="primary" :loading="sendLoading" round @click="restPassword">
                    {{ $t("ResetPassword") }}

                  </el-button>
                  <span class="forgetPW"><a href="#" @click="backLogin" id="lblForgetPW">Đăng nhập</a></span>
                </el-form-item>
              </el-form>

            </div>

            <!-- <el-col
        :span="12"
        style="margin-left:0px;margin-right:0px;padding-left:0px;padding-right:0px;"
      >
        <img src="../../assets/background/group-2.png" id="BackgroundLogin" />
      </el-col> -->
          </div>

          <div class="formlogin-wrapper" v-if="isResetPassword" style="height: 100%; width: 100%;">
            <div id="LogoLogin"></div>
            <div id="TtileLogin">{{ $t("ResetPW") }}</div>
            <div id="formInput">

              <el-form :model="resetForm" :rules="ruleReset" ref="resetForm" label-position="top" label-width="100px">
                <el-form-item prop="Code">
                  <span class="resetPW"><a @click="focus('InputLoginCode')" id="lbl">{{ $t("Code") }}</a></span>
                  <el-input ref="InputLoginCode" id="InputLoginCode" v-model="resetForm.Code"></el-input>
                </el-form-item>

                <el-form-item style="width:360px" prop="NewPassword">
                  <span class="resetPW"><a @click="focus('NewPassword')" id="lbl">{{ $t("NewPassword") }}</a></span>
                  <template v-if="isInvalidNewPW">
                    <el-input ref="NewPassword" type="password" id="InputLoginNewPass" v-model="resetForm.NewPassword"
                      class="error-input"></el-input>
                    <div class="el-form-item__error">Mật khẩu phải có ít nhất 8 ký tự bao gồm: 1 chữ số, 1 ký tự in hoa và
                      1 ký tự in thường</div>
                  </template>
                  <el-input v-else ref="NewPassword" type="password" id="InputLoginNewPass"
                    v-model="resetForm.NewPassword"></el-input>
                </el-form-item>

                <el-form-item style="width:360px" prop="ConfirmPassword">
                  <span class="resetPW"><a @click="focus('ConfirmPassword')" id="lbl">{{ $t("ConfirmNewPassword")
                  }}</a></span>
                  <template v-if="isInvalidConfirmNewPW">
                    <el-input ref="ConfirmPassword" type="password" id="InputLoginConfirmPass"
                      v-model="resetForm.ConfirmNewPassword" class="error-input"></el-input>
                    <div class="el-form-item__error">{{ $t("ConfirmPWNotMatch") }}</div>
                  </template>
                  <el-input v-else ref="ConfirmPassword" type="password" id="InputLoginConfirmPass"
                    v-model="resetForm.ConfirmNewPassword"></el-input>
                </el-form-item>

                <el-form-item style="width:360px">
                  <el-button v-if="disableButtonSend" disabled id="btnResetPW" type="primary" round
                    @click="sendResetPWAll">
                    {{ $t("ResetPassword") }}
                  </el-button>
                  <el-button v-else id="btnResetPW" type="primary" round @click="sendResetPWAll">
                    {{ $t("ResetPassword") }}
                  </el-button>
                  <span class="forgetPW"><a href="#" @click="backLogin" id="lblForgetPW">Đăng nhập</a></span>
                </el-form-item>
              </el-form>

            </div>
            <!-- </el-col>
      <el-col
        :span="12"
        style="margin-left:0px;margin-right:0px;padding-left:0px;padding-right:0px;"
      >
        <img src="../../assets/background/group-2.png" id="BackgroundLogin" />
      </el-col> -->
          </div>


          <div style="height: 100%; width: 100%;">
            <el-dialog :title="$t('Notify')" :visible.sync="showDialog" width="35%" height="35%" center>
              <span>Hệ thống đang gửi mã khôi phục mật khẩu về tài khoản. Vui lòng đợi..</span>
            </el-dialog>
          </div>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<script src="./login.ts"></script>
<style lang="scss" scoped>
body {
  overflow: scroll;
}

.formlogin-wrapper {
  display: flex;
  justify-content: center;
  align-items: center;
  flex-direction: column;
}

#LogoLogin {
  top: 5vh;
  right: 2.5vw;
  position: absolute;
}

.epad-title-description {
  color: white;
  font-size: 32px;
  position: absolute;
  top: 40%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 50%;
  text-align: center;

  div {
    font-size: 64px;
    font-weight: bold;
  }
}

.epad-title-description::after {
  content: '';
  display: inline-block;
  width: 100%;
}

#btn {
  width: 70%;
  left: 5% !important;
  position: relative;
  padding: 0 !important;
}

/deep/ span{
  font-size: 16px !important;
}

.option-language {
  display: flex;
  justify-content: space-between;
  align-items: center;

  .flagvn,
  .flaguk {
    width: 40px;
    height: 25px;
    background-image: url('../../assets/images/flagvn.svg');
    background-repeat: no-repeat;
    background-position: center;
    background-size: contain;
  }

  .flaguk {
    background-image: url('../../assets/images/flaguk.svg');
  }
}

.select-lang {
  position: relative;

  .select-language-flagvn,
  .select-language-flaguk {
    position: absolute;
    top: 4px;
    left: 10px;
    width: 40px;
    height: 25px;

    background-image: url('../../assets/images/flagvn.svg');
    background-repeat: no-repeat;
    background-position: center;
    background-size: contain;
  }

  .select-language-flaguk {
    background-image: url('../../assets/images/flaguk.svg');
  }

  /deep/ .el-input .el-input__inner {
    color: transparent;
  }
}

.login-input /deep/ .el-input__prefix {
  line-height: 50px !important;
  .el-input__icon{
    font-size: 16px;
  }
}

.login-input /deep/ .el-input__inner {
  font-size: 16px;
}
</style>
