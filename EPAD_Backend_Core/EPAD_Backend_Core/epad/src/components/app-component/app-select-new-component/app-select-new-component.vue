<template>
    <!-- => Change from :value="value" to v-model="value", don't ask how this work cause at that moment,
    the one who change this (me) don't have enough knowlegde to explain-->
    <el-select 
        v-model="value" 
        :placeholder="placeholder"
        :v-bind="$attrs"
        :multiple="isMultiple"
        filterable 
        collapse-tags
        reserve-keyword
        :clearable="allowNull"
        @change="$emit('value', $event); $emit('onChange', $event);onSelectChange($event)"
        @visible-change="changeVisible"
        :disabled="disabled"
    >
        <el-button 
            v-show="isShowBtnSelectedAll && isMultiple"
            type="primary" 
            class="btn-all"
            @click="selectAll" 
            style="width: -webkit-fill-available;max-width:100%;margin: 0 5px"
        >
            {{$t('SelectAll')}}
        </el-button>

        <el-button 
        v-show="!isShowBtnSelectedAll && isMultiple"
        type="primary" 
        class="btn-all"
        @click="DeselectAll" 
        style="width: -webkit-fill-available;max-width:100%;margin: 0 5px"
        >
        {{$t('DeselectAll')}}
        </el-button>
        <el-option 
            v-for="item in dataSource" 
            :key="item[valueMember]" 
            :label="item[displayMember]" 
            :value="item[valueMember]"
        ></el-option>
    </el-select>
</template>

<script src="./app-select-new-component.ts" />
