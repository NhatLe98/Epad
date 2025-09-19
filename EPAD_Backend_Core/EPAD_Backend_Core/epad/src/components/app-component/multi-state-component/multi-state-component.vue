<template>
  <div v-if="hasTooltip">
    <el-tooltip class="item" effect="dark" :content="$t(stateTitle)" placement="right">
      <el-button :type="stateType" :icon="stateIcon" @click="changeState" class="circlebutton" circle></el-button>
    </el-tooltip>
  </div>

  <div v-else>
    <el-button :type="stateType" :icon="stateIcon" @click="changeState" class="circlebutton" circle></el-button>
  </div>
</template>

<script src="./multi-state-component.ts"></script>

<style scoped>
.circlebutton {
  height: auto;
}
.el-button--primary {
  background: #409EFF !important;
}

</style>