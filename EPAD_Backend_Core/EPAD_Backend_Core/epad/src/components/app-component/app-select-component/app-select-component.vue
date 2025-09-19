<template>
    <el-select 
        :value="value" 
        :placeholder="placeholder"
        :v-bind="$attrs"
        :multiple="isMultiple"
        :filterable="filterable"
        :clearable="clearable"
        :collapse-tags="isCollapseTags"
        :disabled="disabled"
        @change="$emit('value', $event)"
    >
        <el-option 
            v-for="item in dataSource" 
            :key="item[valueMember]" 
            :label="item[displayMember]" 
            :value="item[valueMember]"
            :disabled="checkDisabled(item[valueMember])"
        ></el-option>
    </el-select>
</template>

<script src="./app-select-component.ts" />
