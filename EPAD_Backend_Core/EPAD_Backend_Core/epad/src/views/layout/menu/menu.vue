<template>
  <div class="leftMenu-wrapper">
   
    <div :class="getClassMenu">
      <template v-if="collapse">
        <div class="logoNav collapse">
          <span class="collapseMenu collapse" @click="collapseHandle"></span>
        </div>
      </template>
      <template v-else>
        <div class="logoNav">
          <span class="collapseMenu" @click="collapseHandle"></span>
          <span class="logoMenu"></span>
          <!-- <span class="logoMenu-flower"></span> -->
        </div>
      </template>

      <div class="botNav">
        <el-menu
          ref="menu"
          :default-active="activeIndex"
          class="el-menu-vertical-demo menuleft"
          :background-color="backGround"
          :text-color="menuText"
          :router="true"
          :collapse="collapse"
          @select="onClick"
          >
            <!--LV1-->
          <template v-for="group in listGroupMenu">
            <template v-if="group.group_key === 'Dashboard'">
              <el-menu-item
              :class="$route.name == group.group_path ? 'is-active-item' : ''"
                index="1"
                v-if="group.group_show === true"
                :key="group.group_key"
                :data-id="group.group_key"
                :id="1"
                :route="{name: group.group_path}"
              >
              <img id="IconHome" :src="getImgUrl(group.group_icon)" :style="{ color: menuText }"/>
              <span class="menu-item-icon lv1-menu-name">{{ $t(group.group_name) }}</span>
                
              </el-menu-item>
            </template>
            <template v-if="group.group_key === 'Reports'">
              <el-menu-item
              :class="$route.name == group.group_path 
                              || ($route.name != group.group_path && $route.name.includes('attendance-monitoring') 
                              && group.group_path.includes('attendance-monitoring') && isUsingNewAttendanceMonitoring) ? 'is-active-item' : ''"
                :index="group.group_id"
                v-if="group.group_show === true"
                :key="group.group_key"
                :data-id="group.group_key"
                :id="group.group_id"
                :route="{name: group.group_path}"
              >
              <img id="IconHome" :src="getImgUrl(group.group_icon)" :style="{ color: menuText }"/>
              <span class="menu-item-icon lv1-menu-name">{{ $t(group.group_name) }}</span>
                
              </el-menu-item>
            </template>
            <template v-else-if="group.group_key != 'Home' && group.group_key != 'Dashboard'">
                <el-submenu class="drop-sub-menu" :index="group.group_id" :key="group.group_key" :id="group.group_id"
              popper-class="menu-submenu">
                <template slot="title">
                  <img id="IconHome" v-bind:src="getImgUrl(group.group_icon)" :style="{ color: menuText }"/>
                  <span class="menu-item-icon lv1-menu-name" :style="{ color: menuText }">{{ $t(group.group_name) }}</span>
                </template>
                <div class="submenu-wrapper">
                  <!--LV2-->
                <template v-for="menu in group.list_menu">
                  <el-menu-item :class="$route.name == menu.path 
                              || ($route.name != menu.path && $route.name.includes('attendance-monitoring') 
                              && menu.path.includes('attendance-monitoring') && isUsingNewAttendanceMonitoring) ? 'is-active-item' : ''"
                    v-if="menu.show === true && (!menu.list_menu || (menu.list_menu && menu.list_menu.length == 0))"
                    :index="menu.id"
                    :id="menu.id"
                    :key="menu.key"
                    :data-id="menu.key"
                    :route="{ name: menu.path}"
                  >
                    <div class="child-menu-item-icon drop-child-menu-item-icon">{{ $t(menu.name) }}</div>
                  </el-menu-item>
                  <!-- <el-menu-item
                    v-else-if="menu.show === true && menu.list_menu && menu.list_menu.length > 0"
                    :index="menu.id"
                    :key="menu.key"
                    :data-id="menu.key"
                  >
                    <template v-for="childMenu in menu.list_menu">
                      <el-menu-item
                        v-if="childMenu.show === true"
                        :index="childMenu.id"
                        :key="childMenu.key"
                        :data-id="childMenu.key"
                        :route="{ name: childMenu.path}"
                      >
                        <div class="menu-item-icon">{{ $t(childMenu.name) }}</div>
                      </el-menu-item>
                    </template>
                  </el-menu-item> -->
                    <el-submenu class="hover-sub-menu have-child" v-else-if="menu.list_menu && menu.list_menu.length > 0"
                    :index="menu.id" :key="menu.key" :id="menu.id"
                    popper-class="menu-submenu"
                    @mouseenter.native="enter(menu)" @mouseleave.native="leave(menu)">
                    <template slot="title">
                      <span class="child-menu-item-icon drop-child-menu-item-icon" :style="{ color: menuText }">{{ $t(menu.name) }}</span>
                    </template>
                    <div class="submenu-wrapper">
                      <!--LV3-->
                    <template v-for="childMenuLv1 in menu.list_menu">
                      <el-menu-item :class="$route.name == childMenuLv1.path 
                              || ($route.name != childMenuLv1.path && $route.name.includes('attendance-monitoring') 
                              && childMenuLv1.path.includes('attendance-monitoring') && isUsingNewAttendanceMonitoring) ? 'is-active-item' : ''"
                        v-if="childMenuLv1.show === true 
                          && (!childMenuLv1.list_menu || (childMenuLv1.list_menu && childMenuLv1.list_menu.length == 0))"
                        :index="childMenuLv1.id"
                        :id="childMenuLv1.id"
                        :key="childMenuLv1.key"
                        :data-id="childMenuLv1.key"
                        :route="{ name: childMenuLv1.path}"
                      >
                        <div class="child-menu-item-icon">{{ $t(childMenuLv1.name) }}</div>
                      </el-menu-item>
                      <el-submenu class="hover-sub-menu have-child" v-else-if="childMenuLv1.list_menu && childMenuLv1.list_menu.length > 0"
                        :index="childMenuLv1.id" :key="childMenuLv1.key" :id="childMenuLv1.id"
                        popper-class="menu-submenu"
                        @mouseenter.native="enter(childMenuLv1)" @mouseleave.native="leave(childMenuLv1)">
                        <template slot="title">
                          <span class="child-menu-item-icon" :style="{ color: menuText }">{{ $t(childMenuLv1.name) }}</span>
                        </template>
                        <div class="submenu-wrapper">
                          <!--LV4-->
                        <template v-for="childMenuLv2 in childMenuLv1.list_menu">
                          <el-menu-item :class="$route.name == childMenuLv2.path 
                              || ($route.name != childMenuLv2.path && $route.name.includes('attendance-monitoring') 
                              && childMenuLv2.path.includes('attendance-monitoring') && isUsingNewAttendanceMonitoring) ? 'is-active-item' : ''"
                            v-if="childMenuLv2.show === true 
                              && (!childMenuLv2.list_menu || (childMenuLv2.list_menu && childMenuLv2.list_menu.length == 0))"
                            :index="childMenuLv2.id"
                            :id="childMenuLv2.id"
                            :key="childMenuLv2.key"
                            :data-id="childMenuLv2.key"
                            :route="{ name: childMenuLv2.path}"
                          >
                            <div class="child-menu-item-icon">{{ $t(childMenuLv2.name) }}</div>
                          </el-menu-item>
                          <el-submenu class="hover-sub-menu have-child" v-else-if="childMenuLv2.list_menu && childMenuLv2.list_menu.length > 0"
                            :index="childMenuLv2.id" :key="childMenuLv2.key" :id="childMenuLv2.id"
                            popper-class="menu-submenu"
                            @mouseenter.native="enter(childMenuLv2)" @mouseleave.native="leave(childMenuLv2)">
                            <template slot="title">
                              <span class="child-menu-item-icon" :style="{ color: menuText }">{{ $t(childMenuLv2.name) }}</span>
                            </template>
                            <div class="submenu-wrapper">
                              <!--LV5-->
                            <template v-for="childMenuLv3 in childMenuLv2.list_menu">
                              <el-menu-item :class="$route.name == childMenuLv3.path 
                              || ($route.name != childMenuLv3.path && $route.name.includes('attendance-monitoring') 
                              && childMenuLv3.path.includes('attendance-monitoring') && isUsingNewAttendanceMonitoring) ? 'is-active-item' : ''"
                                v-if="childMenuLv3.show === true 
                                  && (!childMenuLv3.list_menu || (childMenuLv3.list_menu && childMenuLv3.list_menu.length == 0))"
                                :index="childMenuLv3.id"
                                :id="childMenuLv3.id"
                                :key="childMenuLv3.key"
                                :data-id="childMenuLv3.key"
                                :route="{ name: childMenuLv3.path}"
                              >
                                <div class="child-menu-item-icon">{{ $t(childMenuLv3.name) }}</div>
                              </el-menu-item>
                            </template>
                            </div>
                          </el-submenu>
                        </template>
                        </div>
                      </el-submenu>
                    </template>
                    </div>
                  </el-submenu>
                </template>
              </div>
              </el-submenu>
            </template>
          </template>
        </el-menu>
        
      </div>
    </div>

    <!-- <div class="leftMenu" v-else>
      <div class="logoNav">
        <span class="collapseMenu" @click="collapseHandle"></span>
        <span class="logoMenu"></span>
        <span class="logoMenu-flower"></span>
      </div>
        
      <div class="botNav">
        <el-menu
          default-active="1"
          class="el-menu-vertical-demo"
          background-color="#FFFFFF"
          active-background-color="#EFF2FF"
          text-color="#2D3042"
          active-text-color="#25262B"
          :router="true"
          :collapse="collapse"
        >
          <template v-for="group in listGroupMenu">
            <template v-if="group.group_key === 'Home'">
              <el-menu-item
                :index="group.group_id"
                :key="group.group_key"
                :data-id="group.group_key"
                :route="{name: group.group_path}"
              >
              <img id="IconHome" :src="getImgUrl(group.group_icon)" />
              <span class="menu-item-icon">{{ $t(group.group_name) }}</span>
                
              </el-menu-item>
            </template>
            <template v-else>
              <el-submenu :index="group.group_id" :key="group.group_key">
                <template slot="title">
                  <img id="IconHome" :src="getImgUrl(group.group_icon)" />
                  <span class="menu-item-icon">{{ $t(group.group_name) }}</span>
                </template>
                <template v-for="menu in group.list_menu">
                  <el-menu-item
                    :index="menu.id"
                    :key="menu.key"
                    :data-id="menu.key"
                    :route="{ name: menu.path}"
                  >
                    <div class="menu-item-icon_sub">{{ $t(menu.name) }}</div> 
                  </el-menu-item>
                </template>
              </el-submenu>
            </template>
          </template>
        </el-menu>
      </div>
    </div> -->
  </div>
</template>

<style lang="scss">
.leftMenu:not(.collapse) {
  // .have-child:hover{
  //   pointer-events: none;
  // }
  // .have-child{
  //   pointer-events: auto;
  // }
  .menu-not-inside {
    .el-menu{
      position: fixed;
      top: unset !important;
      bottom: 20px;
      left: 240px;
    }
  }
  .drop-sub-menu{
    .el-menu{
      .submenu-wrapper{
        overflow-y: visible;
      }
    }
  }
  .hover-sub-menu{
    position: relative;
    .el-menu{
      .submenu-wrapper{
        overflow-y: auto;
        padding-bottom: 2px;
        max-height: calc(70vh);
      }
    }
  }
  .drop-sub-menu > .el-submenu__title .el-submenu__icon-arrow {
    transform: rotateZ(-90deg) !important;  
  }
  .drop-sub-menu.is-opened > .el-submenu__title .el-submenu__icon-arrow {
    transform: rotateZ(0deg) !important;  
  }
  .hover-sub-menu > .el-submenu__title .el-submenu__icon-arrow {
    transform: rotateZ(-90deg) !important;  
  }
  .submenu-wrapper:has(.submenu-wrapper){
    // max-height: calc(260px);
    overflow-y: visible;
  }
  .submenu-wrapper:not(:has(.submenu-wrapper)){
    // max-height: calc(260px);
    overflow-y: auto;
  }
  .hover-sub-menu.is-opened {
    display: unset;
    overflow-y: visible;
    max-height: 48px;
    .el-menu {
      // border: 1px solid black;
      box-shadow: 0 4px 16px 0 rgba(0, 0, 0, 0.3), 0 4px 16px 0 rgba(0, 0, 0, 0.3);
      position: fixed;
      opacity: 1;
      z-index: 9999999999;
      // top: 0;
      left: calc(230px - 15px);
      overflow-y: visible;
      overflow-x: visible;

      width: 240px;
      height: fit-content !important;
    }
  }
  .hover-sub-menu.is-opened {
    ul {
      display: unset;
    }
  }
  .hover-sub-menu {
    ul {
      display: none;
    }
  }

  .el-menu-item, .el-submenu .el-submenu__title {
    padding-left: 16px !important;
  }
  .menu-item-icon, .menu-item-icon_sub {
    margin-left: 12px;
    width: 147px;
    font-family: 'Noto Sans';
    font-style: normal;
    font-weight: 500;
    word-break: break-all;
    height: 48px;
    line-height: 48px;
  }
  .child-menu-item-icon {
    margin-left: 12px;
    width: 147px;
    font-family: 'Noto Sans';
    font-style: normal;
    font-weight: 500;
    word-break: break-all;
    height: 48px;
    line-height: 48px;
  }
  .child-menu-item-icon.drop-child-menu-item-icon {
    margin-left: 40px;
  }
}

.menuleft > span {
  font-size: 14px !important;
}

.menuleft {
  .menu-item-icon {
    font-size: 12px !important;
  }

  .child-menu-item-icon {
    font-size: 12px !important;
  }

  .lv1-menu-name {
    // font-weight: bold !important;
  }

  .is-active,
  .is-active-item {
    background-color: white !important;
  }

  .is-active > div:first-of-type:not(:has(span)){
    font-weight: bold !important;
    color: rgb(19, 56, 152) !important;
  }
  .is-active-item > div:first-of-type:not(:has(span)) {
    font-weight: bold !important;
    color: rgb(19, 56, 152) !important;
  }
  .hover-sub-menu.is-active > div:first-of-type:has(span) {
    background-color: white !important;
    span{
      font-weight: bold !important;
      color: rgb(19, 56, 152) !important;
    }
  }
  .is-active > span:first-of-type {
    font-weight: bold !important;
    color: rgb(19, 56, 152) !important;
  }
  .is-active-item > span:first-of-type {
    font-weight: bold !important;
    color: rgb(19, 56, 152) !important;
  }
}



// li:has(.is-active) {
//   background-color: white !important;
//   font-weight: bold !important;
//   color: rgb(19, 56, 152) !important;
// }

// li:has(.is-active) > div:first-of-type {
//   background-color: white !important;
//   font-weight: bold !important;
//   color: rgb(19, 56, 152) !important;
//   span {
//     font-weight: bold !important;
//     color: rgb(19, 56, 152) !important;
//   } 
// }
</style>

<script src="./menu.ts"></script>



<!-- <div id="leftMenu">
    <div id="topNav">
      <div id="fill" />
      <div id="LogoNav" class="collapse" v-if="collapse"/>
      <div id="LogoNav" v-else />
      <span class="collapseMenu collapse" v-if="collapse" @click="collapseHandle"></span>
      <span class="collapseMenu" v-else @click="collapseHandle"></span>
      
    </div>
    <div id="botNav">
      <el-menu
        default-active="1"
        class="el-menu-vertical-demo"
        background-color="#003159"
        text-color="#a8b3d6"
        active-text-color="#FFFFFF"
        :router="true"
        :collapse="collapse"
      >
        <template v-for="group in listGroupMenu">
          <template v-if="group.group_key === 'Home'">
            <el-menu-item
              :index="group.group_id"
              :key="group.group_key"
              :data-id="group.group_key"
              :route="{name: group.group_path}"
            >
              <img id="IconHome" :src="getImgUrl(group.group_icon)" />
              <span class="menu-item-icon">{{ $t(group.group_name) }}</span>
            </el-menu-item>
          </template>
          <template v-else>
            <el-submenu :index="group.group_id" :key="group.group_key">
              <template slot="title">
                <img id="IconHome" :src="getImgUrl(group.group_icon)" />
                <span class="menu-item-icon">{{ $t(group.group_name) }}</span>
              </template>
              <template v-for="menu in group.list_menu">
                <el-menu-item
                  :index="menu.id"
                  :key="menu.key"
                  :data-id="menu.key"
                  :route="{ name: menu.path}"
                >
                  <img id="IconHome" :src="getImgUrl(menu.icon)" />
                  <span class="menu-item-icon">{{ $t(menu.name) }}</span>
                </el-menu-item>
              </template>
            </el-submenu>
          </template>
        </template>
      </el-menu>
    </div>
  </div> -->