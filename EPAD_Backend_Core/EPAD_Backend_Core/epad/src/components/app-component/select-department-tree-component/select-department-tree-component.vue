<template>
  <div class="el-select-department-tree" v-on:click="getPopperLocate">
    <el-popover ref="elPopover" v-model="visible" transition="el-zoom-in-top" popper-class="el-select-department-tree__popover"
      trigger="click" :disabled="disabled" :placement="placement" :width="popoverWidth" 
      @after-enter="handleScroll()">
      <!-- scrollbar wrap -->
      <el-scrollbar wrap-class="el-select-dropdown__wrap" view-class="el-select-dropdown__list" ref="scrollbar">
        <el-input :placeholder="$t('Search')" v-model="filterText">
        </el-input>
        <!-- <el-tree
          ref="elTree"
          class="el-select-tree__list"
          :default-expanded-keys="defaultExpandedKeys"
          :show-checkbox="multiple"
          :expand-on-click-node="multiple"
          :style="{ 'min-width': minWidth + 'px' }"
          @check="handleCheck"
          @node-click="nodeClick"
          @check-change="checkChange"
          @transitionend.native="$refs.elPopover.updatePopper()"
          :data="data"
          :props="props"
          :node-key="propsValue"
          :default-expand-all="defaultExpandAll"
          :check-strictly="checkStrictly"
          :lazy="lazy"
          :indent="indent"
          :accordion="accordion"
          :auto-expand-parent="autoExpandParent"
          :render-after-expand="renderAfterExpand"
          :render-content="renderContent"
          :filter-node-method="filterNode"
        >
          <div
            class="el-select-tree__item"
            slot-scope="{ data }"
            :class="treeItemClass(data)"
          >
            {{ data[propsLabel] }}
          </div>
        </el-tree> -->
        <el-tree 
          ref="elTree" 
          class="el-select-tree__list" 
          :default-expanded-keys="defaultExpandedKeys"
          :show-checkbox="multiple" 
          :expand-on-click-node="multiple" 
          :style="{ 'min-width': minWidth + 'px' }"
          @check="handleCheck" 
          @node-click="nodeClick" 
          @transitionend.native="$refs.elPopover.updatePopper()" 
          :data="data"
          :props="props" 
          :node-key="propsValue" 
          :default-expand-all="defaultExpandAll"
          :check-strictly="checkStrictly"
          :lazy="lazy" 
          :indent="indent" 
          :accordion="accordion" 
          :auto-expand-parent="autoExpandParent"
          :render-after-expand="renderAfterExpand" 
          :render-content="renderContent" 
          :filter-node-method="filterNode">
          <div class="el-select-tree__item" slot-scope="{ data }" :class="treeItemClass(data)">
            {{ data[propsLabel] }}
          </div>
        </el-tree>
      </el-scrollbar>

      <!-- trigger input -->
      <el-input v-model="selectedLabel" ref="reference" slot="reference" readonly :validate-event="false" :size="size"
        :class="{
          'is-active': visible,
          'is-selected': selectedLabel,
          'is-clearable': clearable,
        }" :disabled="disabled" :placeholder="placeholder">
        <i v-if="selectedLabel && selectedLabel.length > 0 && !disabled" @click.stop="clear()" slot="suffix"
          class="el-input__icon el-input__icon-close el-icon-circle-close hidden-icon"></i>
        <i v-else slot="suffix" class="el-input__icon el-input__icon-arrow-down el-icon-arrow-down"></i>
      </el-input>
    </el-popover>
  </div>
</template>

<script src="./select-department-tree-component.ts" />

<style lang="scss">
@import "node_modules/element-ui/packages/theme-chalk/src/common/var.scss";

// 基准颜色
$color-blue: rgba(157, 184, 233, 1);
$color-primary: $--color-primary;
$color-white: $--color-white;
$color-black: $--color-black;
$color-success: $--color-success;
$color-warning: $--color-warning;
$color-danger: $--color-danger;
$color-info: $--color-info;

// 文本颜色
$color-text-primary: $--color-text-primary;
$color-text-regular: $--color-text-regular;
$color-text-secondary: $--color-text-secondary;
$color-text-placeholder: $--color-text-placeholder;

// 边框颜色
$border-color-base: $--border-color-base;
$border-color-light: $--border-color-light;
$border-color-lighter: $--border-color-lighter;
$border-color-extra-light: $--border-color-extra-light;

// 背景颜色
$background-color-base: $--background-color-base;

// 图标颜色
$icon-color: $--icon-color;
$icon-color-base: $color-info;

// 边框样式
$border-width-base: $--border-width-base;
$border-style-base: $--border-style-base;
$border-color-hover: $--color-text-placeholder;
$border-base: $--border-width-base $--border-style-base $--border-color-base;
$border-radius-base: $--border-radius-base;
$border-radius-small: $--border-radius-small;
$border-radius-circle: $--border-radius-circle;
$border-radius-zero: $--border-radius-zero;

// 投影样式
$box-shadow-default: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
$box-shadow-base: $--box-shadow-base;
$box-shadow-dark: $--box-shadow-dark;
$box-shadow-light: $--box-shadow-light;

// 字体大小
$font-size-extra-small: $--font-size-extra-small; // 12px
$font-size-small: $--font-size-small; // 13px
$font-size-base: $--font-size-base; // 14px
$font-size-medium: $--font-size-medium; // 16px
$font-size-large: $--font-size-large; // 18px
$font-size-extra-large: $--font-size-extra-large; // 20px

// 字体颜色
$font-color: #444;

// 间距大小
$spacing-base: 10px;
$spacing-medium: 20px;
$spacing-large: 30px;

// 过渡效果
$transition-base: all 0.3s;

.el-select-department-tree {
  height: 32px !important;
  span .el-popover__reference-wrapper .el-input{
    height: 28px !important;
    input {
      height: 28px !important;
    }
  }
}

.el-select-department-tree .hidden-icon{
  visibility: hidden;
}

.el-select-department-tree:hover .hidden-icon{
  visibility: visible;
}

.el-tree {
  .el-tree-node__children {
    padding-left: 15px;
  }
}

.el-select-tree {
  display: inline-block;
  width: 100%;

  .el-input__icon {
    cursor: pointer;
    transition: transform 0.3s;

    &-close {
      display: none;
    }
  }

  .el-input__inner {
    cursor: pointer;
    padding-right: 30px;
  }

  .el-input {
    &:hover:not(.is-disabled) {
      .el-input__inner {
        border-color: $--input-border-color-hover;
      }

      &.is-selected.is-clearable {
        .el-input__icon {
          &-close {
            display: inline-block;
          }

          &-arrow-down {
            display: none;
          }
        }
      }
    }

    &.is-active {
      .el-input__icon-arrow-down {
        transform: rotate(-180deg);
      }

      .el-input__inner {
        border-color: $--button-primary-border-color;
      }
    }
  }

  &__popover {
    padding: 0 !important;
    // extends el-select-dropdown - start
    border: $--select-dropdown-border !important;
    border-radius: $--border-radius-base !important;

    // extends el-select-dropdown - end
    .popper__arrow {
      left: 35px !important;
    }

    .el-tree-node__expand-icon.is-leaf {
      cursor: pointer;
    }
  }

  &__list {
    overflow-y: auto;

    // scroll style - start
    &::-webkit-scrollbar-track-piece {
      background: #d3dce6;
    }

    &::-webkit-scrollbar {
      width: 4px;
    }

    &::-webkit-scrollbar-thumb {
      background: #99a9bf;
    }

    // scroll style - end
  }

  &__item {
    position: relative;
    white-space: nowrap;
    padding-right: $spacing-medium;

    &.is-selected {
      color: $--select-option-selected-font-color;
      font-weight: bolder;
    }

    &.is-disabled {
      color: $--font-color-disabled-base;
      cursor: not-allowed;
    }
  }
}

.custom-tree-node__tooltip{
  visibility: hidden;
}

.custom-tree-node__data:hover + .custom-tree-node__tooltip{
  visibility: visible;
  position: fixed;
  z-index: 99999;
  background-color: #444;
  color: white;
  padding: 3px;
  border-radius: 5px;
}
</style>