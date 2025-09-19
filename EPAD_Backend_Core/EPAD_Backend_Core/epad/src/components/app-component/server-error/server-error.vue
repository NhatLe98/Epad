<template>
  <el-dialog
    :show-close="false"
    :title="$t('Notify')"
    class="server-error"
    :visible="true"
  >
    <div class="el-message-box__content">
      <div class="el-message-box__container">
        <div class="el-message-box__status el-icon-error"></div>
        <div class="el-message-box__message">
          <p>{{ $t("MSG_NetworkError") }}</p>
        </div>
      </div>
    </div>
    <span class="el-message-box__btns" slot="footer">
      <el-button
        :loading="confirmButtonText !== `OK`"
        @click="closeDialog"
        type="primary"
        >{{ confirmButtonText }}</el-button
      >
    </span>
  </el-dialog>
</template>
<script src="./server-error.ts" />

<style scoped>

.server-error >>> .el-dialog__body {
  padding: 0;
}

.server-error >>> .el-dialog {
  border-radius: 3px !important;
  margin-top: calc(100vh / 2 - 160px) !important;
  width: 450px;
}

.el-message-box__btns .el-button {
  width: 90px;
}

.is-loading >>> span {
  display: inline-block;
}
</style>