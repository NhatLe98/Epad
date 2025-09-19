<template>
     <span :class="{
      haveLicense: license == 'HwLicense',
      noLicense: license == 'NotLicense'
    }">{{ $t(license) }}</span>
</template>
<script lang="ts">
import Vue from 'vue'
export default Vue.extend({
    computed: {
        license() {
            return (this as any).params.value;
        }
    }
})
</script>
<style scoped>
.haveLicense {
  color: #3BD97F !important;
  font-weight: 600 !important;
}

.noLicense {
  color: red !important;
  font-weight: 600 !important;
}
</style>