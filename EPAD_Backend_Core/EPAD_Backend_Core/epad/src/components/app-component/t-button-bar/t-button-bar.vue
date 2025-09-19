<template>
  <div class="t-button-bar">
    <div class="group-function">
      <el-button
        type="primary"
        v-if="showReadGoogleSheet"
        @click="$emit('onReadGoogleSheetClick')"
        style="float: right;margin-left: 10px"
        class="add-button"
      >
        {{ $t("ReadDataGoolgeSheet") }}
      </el-button>
      <el-button
        type="primary"
        v-if="showIntegrate"
        @click="$emit('onIntegrateClick')"
        style="float: right;margin-left: 10px"
        icon="el-icon-circle-plus-outline"
        class="add-button"
      >
        {{ $t("RunIntegrate") }}
      </el-button>
      <el-button v-if="showEdit" type="text" @click="$emit('onEditClick')">
        <img src="@/assets/icons/Button/Edit.png" alt="edit" />
      </el-button>

      <el-button v-if="showDelete" type="text" @click="$emit('onDeleteClick')">
        <img src="@/assets/icons/Button/Delete.png" alt="delete" />
      </el-button>

      <el-dropdown @command="handleCommand" trigger="click" v-if="showMore">
        <span class="el-dropdown-link">
          <img src="@/assets/icons/Button/More.png" alt="more" />
        </span>
        <el-dropdown-menu slot="dropdown">
          <el-dropdown-item
            v-for="(item, index) in moreFunctions"
            :key="index"
            :command="item"
          >
            {{ $t(item.title) }}
          </el-dropdown-item>
        </el-dropdown-menu>
      </el-dropdown>

      <el-button
        v-if="showInsert"
        type="primary"
        @click="$emit('onInsertClick')"
        style="float: right"
        icon="el-icon-circle-plus-outline"
        class="add-button"
      >
        {{ $t("Insert") }}
      </el-button>
    </div>
  </div>
</template>

<script src="./t-button-bar.ts"></script>
<style scoped>
.group-function {
    min-height: 36px;
}

.group-function >>> .el-button--text {
  padding: 0;
}

.add-button {
    display: inline-flex;
}
.add-button >>> .el-icon-circle-plus-outline {
    font-size: 18px;
}
</style>
