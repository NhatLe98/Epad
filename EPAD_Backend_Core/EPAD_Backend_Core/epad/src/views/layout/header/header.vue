<template>
  <div>
    <div class="header">
      <el-row>
        <el-col :span="2" style="margin-top:-5px">
          <a href="#">
            <img
              id="avatar"
              src="https://cube.elemecdn.com/e/fd/0fc7d20532fdaf769a25683617711png.png"
            />
          </a>
        </el-col>

        <el-col :span="7">
          <el-dropdown @command="handleCommand">
            <a class="el-dropdown-link ml-10" id="Admin">
              {{ user.UserName }}
              <i class="el-icon-arrow-down el-icon--right"></i>
            </a>
            <el-dropdown-menu slot="dropdown">
              <el-dropdown-item
                command="ChangePassword"
                class="dropdown-header"
              >{{ $t("ChangePassword") }}</el-dropdown-item>
              <el-dropdown-item command="Logout" class="dropdown-header">{{ $t("Logout") }}</el-dropdown-item>
            </el-dropdown-menu>
          </el-dropdown>
        </el-col>
        <el-col :span="7">
          <el-select v-model="value" @change="changeLocales" placeholder="Select">
            <el-option
              v-for="locale in localeOptions"
              :key="locale.value"
              :label="locale.label"
              :value="locale.value"
              class="lang"
            ></el-option>
          </el-select>
        </el-col>
      </el-row>
    </div>

    <div>
      <el-dialog :visible.sync="dialogVisible" width="40%" top="5vh" :before-close="handleClose">
        <el-form ref="form" label-width="200px" label-position="right">
          <el-row style="margin-bottom:25px">
            <el-col :span="8">
              <div class="imgPopupBtnAdd" />
            </el-col>
            <el-col :span="12" id="NamePopup">{{ $t("ChangePassword") }}</el-col>
          </el-row>

          <el-form-item :label="$t('PasswordOld')">
            <el-input id="Input" v-model="user.Password" type="password"></el-input>
          </el-form-item>
          <el-form-item :label="$t('PasswordNew')">
            <el-input id="Input" v-model="user.NewPassword" type="password"></el-input>
          </el-form-item>
          <el-form-item :label="$t('ConfirmPassword')">
            <el-input id="Input" v-model="user.ConfirmPassword" type="password"></el-input>
            <div class="error" v-if="errName">{{ $t("ErrConfirmPassword") }}</div>
          </el-form-item>
        </el-form>

        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="btnCancel">
            <a class="span" style="padding-top:-20px">
              {{
              $t("Cancel")
              }}
            </a>
          </el-button>
          <el-button class="btnOK" type="primary" @click="btnOK">
            <a class="span">{{ $t("OK") }}</a>
          </el-button>
        </span>
      </el-dialog>
    </div>
  </div>
</template>
<script src="./header.ts"></script>
<style scoped>
.dropdown-header {
  font-family: "AvenirNext";
  font-size: 14px;
}

.header {
  margin-top: 20px;
  margin-right: 25px;
}

.ml-10 {
  margin-left: 10px;
}
</style>
