<template>
  <el-dialog :visible.sync="visible" :title="title" @close="close">
    <iframe
      :src="mapUrl"
      width="100%"
      height="500"
      frameborder="0"
      style="border: 0"
    ></iframe>
  </el-dialog>
</template>

<script>
export default {
  props: {
    title: String,
    visible: Boolean,
    mapUrl: String,
  },
  methods: {
    close() {
      this.$emit("close");
    },
  },
};
</script>

<style scoped>
.dialog-footer {
  text-align: right;
}
</style>
