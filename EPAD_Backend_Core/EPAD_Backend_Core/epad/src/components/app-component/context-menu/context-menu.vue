<template>
  <ul class="contextmenu">
    <li
      v-for="item in menu"
      class="contextmenu__item"
      :key="item.action || item.name"
      :id="item.action || item.name"
    >
      <div @click.stop="fnHandler(item)" class="button">
        <i v-if="icon" :class="item.icon"></i>
        <span>{{ item.name }}</span>
        <i
          class="el-icon-arrow-right"
          v-if="item.children && item.children.length > 0"
        ></i>
        <context-menu
          v-if="item.children && item.children.length > 0"
          :menu="item.children"
          :icon="icon"
          :resolve="resolve"
        ></context-menu>
      </div>
    </li>
  </ul>
</template>

<script src="./context-menu.ts">
</script>