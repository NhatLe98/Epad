<template>
  <el-select
    :clearable="clearable"
    :filterable="filterable"
    :placeholder="$t(placeholder)"
    :reset="reset"
    :value="value"
    @change="$emit('value', $event)"
    v-bind="$attrs"
    v-on="$listeners"
  >
    <template v-if="dataRules.length">
      <slot name="options" v-bind="dataRules">
        <el-button
          @click="selectAll"
          class="w-100"
          size="mini"
          v-if="allowSelectAll && isMultiple"
          :key="0"
          >{{ $t("SelectAll") }}</el-button
        >
        <el-option
          :key="item.value"
          :label="item.label"
          :value="item.value"
          v-for="item in dataRules"
        ></el-option>
      </slot>
    </template>
  </el-select>
</template>

<script src="./select-department.ts" />
<style lang="scss" scoped>
.w-100 {
  width: 100%;
}
</style>