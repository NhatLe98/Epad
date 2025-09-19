<template>
  <el-time-picker
    @keydown="onKeyDown($event)"
    ref="input"
    v-model="value"
    @change="onBr"
  >
  </el-time-picker>
</template>
<script lang="ts">
import Vue from "vue";
const KEY_BACKSPACE = "Backspace";
const KEY_ENTER = "Enter";
const KEY_TAB = "Tab";

export default Vue.extend({
  methods: {
    getValue() {
      return this.value;
    },
    onBr(ev){
      this.params.context.componentParent.parentMethod()
    },
    onKeyDown(event) {
      console.log(event);
            if (event.key === 'Escape') {
                return;
            }
            if (this.isLeftOrRight(event) || this.isBackspace(event)) {
                event.stopPropagation();
                return;
            }

            // if (!this.finishedEditingPressed(event) && !this.isNumericKey(event)) {
            //     if (event.preventDefault) event.preventDefault();
            // }
        },
        isLeftOrRight(event) {
            return ['ArrowLeft', 'ArrowRight'].indexOf(event.key) > -1;
        },
        isBackspace(event) {
            return event.key === KEY_BACKSPACE;
        },

    finishedEditingPressed(event) {
      console.log(event);
      const key = event.key;
      return key === KEY_ENTER || key === KEY_TAB;
    },

    
    
    setInitialState(params) {
      let startValue;

      const eventKey = params.eventKey;

      if (eventKey === KEY_BACKSPACE) {
        // if backspace or delete pressed, we clear the cell
        startValue = "";
      } else if (eventKey && eventKey.length === 1) {
        // if a letter was pressed, we start with the letter
        startValue = eventKey;
      } else {
        // otherwise we start with the current value
        startValue = params.value;
      }

      this.value = startValue;
    },

    mounted() {
      this.$nextTick(() => {
        const an = this.$refs.input as any;
        an.focus();
      });
    },
  },
  data() {
    return {
      value: "",
      params: null
    };
  },
  watch: {
    refreshData(co) {
      console.log(co);
    },
  },
});
</script>