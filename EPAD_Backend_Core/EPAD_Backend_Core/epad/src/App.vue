<template>
  <div id="app">
    <router-view></router-view>
    <server-error v-if="showBroken" />
  </div>
</template>

<script src="./App.ts"></script>

<style lang="scss">
:root {

  --icon-filter: auto;
  --icon-filter-color: auto;
  --icon-color: #c0c4cc;

  --main-box-shadow: -2px -2px 4px rgba(123, 123, 123, 0.25);
  --color-scheme: light;

  --header-color: #22262e;
  --header-text-color: white;
  --menu-color: #22262e;
  --menu-text-color: #fff;
  --menu-text-active-color: #133898;

  --select-dropdown-item-color: black;

  --master-background: white;
  --background: white;
  --background-color: white;
  --item-background-color: white;
  --dashboard-background: white;
  --dashboard-background-color: white;
  --dashboard-item-background-color: #d4d8dc;

  --dialog-background: white;
  --dialog-background-color: white;
  --dialog-header-background-color: white;

  --tree-background: white;
  --tree-background-color: white;

  --collapse-item-background-color: white;

  --dropdown-background: white;
  --dropdown-background-color: white;

  --text-color: black;
  --placeholder-color: #a9a9a9;
  --active-color: #409EFF;

  --primary-button-text: white;
  --primary-button-background: #122658;

  --table-border-color: #ebeef5;
  --table-header-text-color: #909399;
  --table-body-text-color: #606266;
  --table-header-caret-color: #c0c4cc;

}

body {
  margin: 0;
}

#app {
  font-family: "Avenir", Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
}

main {
  text-align: center;
  margin-top: 40px;
}

header {
  margin: 0;
  height: 56px;
  padding: 0 16px 0 24px;
  background-color: #35495e;
  color: #ffffff;
}

header span {
  display: block;
  position: relative;
  font-size: 20px;
  line-height: 1;
  letter-spacing: 0.02em;
  font-weight: 400;
  box-sizing: border-box;
  padding-top: 16px;
}
</style>
