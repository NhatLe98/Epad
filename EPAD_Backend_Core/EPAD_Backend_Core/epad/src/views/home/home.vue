<template>
  <div class="divNav" ref="divNav">
    <MenuComponent v-bind:collapse="collapse" @collapse-handle="collapseHandle"></MenuComponent>
    <div class="Content">
      <!-- <HeaderComponent></HeaderComponent> -->
      <router-view :key="$route.fullPath" ></router-view> 
    </div>
  </div>
</template>

<!-- <div id="divNav" ref="divNav">
    <el-row style="margin-left:0px;margin-right:0px;padding-left:0px;padding-right:0px;">
      <template v-if="collapse">
        <el-col 
          id="Home"
          style="margin-left:0px;margin-right:0px;padding-left:0px;padding-right:0px;"
          class="collapse"
        >
          <MenuComponent v-bind:collapse="collapse" @collapse-handle="collapseHandle"></MenuComponent>
        </el-col>

        <el-col
          id="Content" class="collapse"
          v-bind:style="{  borderTopLeftRadius: 0, borderBottomLeftRadius: 0}"
          v-if="hasTreeView"
        >
          <router-view :key="$route.fullPath" ></router-view>
        </el-col>
        <el-col
          
          id="Content" class="collapse"
          v-bind:style="{  borderTopLeftRadius: 0, borderBottomLeftRadius: 0}"
          v-else
        >
          <router-view :key="$route.fullPath" ></router-view>
        </el-col>
      </template>

      <template v-else>
        <el-col
          id="Home"
          style="padding-left:0px;padding-right:0px;"
        >
          <MenuComponent v-bind:collapse="collapse" @collapse-handle="collapseHandle"></MenuComponent>
        </el-col>

        <el-col
          
          id="Content"
          style="padding-left:0px;padding-right:0px;border-top-left-radius:0px;border-bottom-left-radius:0px;"
          v-if="hasTreeView"
        >
          <router-view :key="$route.fullPath" ></router-view>
        </el-col>
        <el-col
          
          id="Content"
          style="padding-left:0px;padding-right:0px;"
          v-else
        >
          <router-view :key="$route.fullPath" ></router-view>
        </el-col>
      </template>
    </el-row>
  </div> -->


<script src="./home.ts"></script>