<template>
  <div
    v-loading="loading"
    class="center"
    element-loading-text="Đang đăng xuất. Vui lòng chờ"
    element-loading-spinner="el-icon-loading"
  ></div>
</template>

<script src="./signout-oidc.ts"></script>

<style scoped>
.center {
  width: 100vw;
  height: 100vh;
  background-color: #404456;
}
</style>
