<template>
  <div class="contextmenu-wrap" v-show="status" :style="style">
    <ContextMenu :icon="icon" :menu="menu" :resolve="resolve"></ContextMenu>
  </div>
</template>

<script src="./context-menu-wrap.ts"></script>

<style lang="scss">
$menu-height: 30px;
.contextmenu-wrap {
  display: block;
  position: fixed;
  z-index: 9999;
  top: 0;
  left: 0;
  border-radius: 5px;
  /*overflow: hidden;*/
  /*border: 1px solid #ccc;*/
  .contextmenu {
    margin: 0;
    box-shadow: 0 0 5px #ccc;
    border: 1px solid #ccc;
  }
  .contextmenu__item {
    width: 155px;
    height: $menu-height;
    line-height: $menu-height;
    border-radius: 4px;
    background: #fff;
    text-decoration: none;
    list-style: none;
    position: relative;
    .button {
      cursor: pointer;
      width: 100%;
      height: 100%;
      display: flex;
      align-items: center;
      outline: 0;
      border: 0;
      background: #fff;
      padding: 0 5px;
      > * {
        color: #333;
      }
      span {
        flex: 1;
        text-align: left;
        margin-left: 5px;
      }
      i {
        text-align: center;
      }
      .contextmenu {
        position: absolute;
        right: calc(-100% - 2px);
        top: -1px;
        display: none;
      }
      &:hover {
        box-shadow: 0px 1px 3px rgba(34, 25, 25, 0.2);
        /*border-radius: 4px;*/
        background: -webkit-linear-gradient(
          bottom,
          #5a6a76,
          #2e3940
        ); /* Safari 5.1 - 6.0 */
        background: -o-linear-gradient(
          bottom,
          #5a6a76,
          #2e3940
        ); /* Opera 11.1 - 12.0 */
        background: -moz-linear-gradient(
          bottom,
          #5a6a76,
          #2e3940
        ); /* Firefox 3.6 - 15 */
        background: linear-gradient(to bottom, #5a6a76, #2e3940);
        > .contextmenu {
          display: block;
        }
        > * {
          color: #fff;
        }
      }
    }
  }
}
</style>