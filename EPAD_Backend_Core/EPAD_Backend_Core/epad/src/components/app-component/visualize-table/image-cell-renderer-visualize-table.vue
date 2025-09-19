<template>
<div>
    <img
        v-if="imageSrc"
        :src="`data:image/jpeg;base64,${imageSrc}`"
        class="image-thumbnail"
        @click="showModal = true"
    />
    <div class="image-modal">
      <el-dialog
        :title="$t('DialogOption')"
        :visible.sync="showModal"
        custom-class="customdialog"
        width="30%"
        :append-to-body="true"
      >
      <div class="center">
          <img
            :src="`data:image/jpeg;base64,${imageSrc}`"
            alt=""
            style="max-width: 100%;"
          />
      </div>
      </el-dialog>
    </div>
</div>
</template>
<script lang="ts">
import Vue from 'vue'
export default Vue.extend({
    name: "ImageCellRendererVisualizeTable",
    data() {
        return {
            showModal: false,
            imageSrc: '',
        }
    },
    mounted() {
        this.imageSrc = (this as any).params?.value;
    }
})
</script>
<style lang="scss" scoped>
.center {
    display: flex;
    justify-content: center;
}
.image-modal {
  .ui-focus-container {
    width: auto;
    img {
      width: auto;
      height: 85vh;
    }
  }
}

.image-thumbnail {
    width: 32px; 
    height: 32px; 
    border-radius: 50%;
    object-fit: cover;
}
.image-thumbnail:hover {
    cursor: pointer;
    transition: .2s ease-in-out;
    transform: scale(1.1);
}
</style>