<template>
    <span>{{this.timeDisplay}}</span>
</template>
<script lang="ts">
import { fHMS } from '@/utils/datetime-utils';
import Vue, { PropType } from 'vue'
export default Vue.extend({
    props: {
        time: {
            type: Date as PropType<Date>,
            required: true,
        },
    },
    data() {
        return {
            countTimeInterval: null,
            timeDisplay: '',
        }
    },
    mounted() {
        this.countTimeInterval = setInterval(() => {
            this.timeDisplay = fHMS(this.time, new Date());
        }, 1000);
    },
    destroyed() {
        clearInterval(this.countTimeInterval);
    }
})
</script>