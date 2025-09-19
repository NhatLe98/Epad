<template>
     
         <div class="datatable-function">
                <span class="groupfunction">
                    <el-button v-if="!isHiddenEdit" type="text" @click="Edit">
                        <img src="@/assets/icons/Button/Edit.png" alt="edit" />
                    </el-button>
                    <el-button v-if="!isHiddenDelete" type="text" @click="Delete">
                        <img src="@/assets/icons/Button/Delete.png" alt="delete" />
                    </el-button>
                    <el-dropdown @command="handleCommand" trigger="click" v-if="getHasMore">
                        <span class="el-dropdown-link">
                            <!-- <img src="@/assets/icons/Button/More.png" alt="more"/> -->
                            <span class="more-text" style="font-weight: bold;"><span style="font-size: 20px;">. . .</span>{{$t("More")}}</span>
                        </span>
                        <el-dropdown-menu slot="dropdown">
                            <el-dropdown-item v-for="(item, index) in listExcelFunction" :key="index" :command="item">
                                {{ $t(item) }}
                            </el-dropdown-item>
                        </el-dropdown-menu>
                    </el-dropdown>

                </span>

                <div class="group-btn">
                    <el-button v-if="has1MoreBtn" type="primary" class="add-button" @click="Restart">
                        {{ $t("Restart") }}
                    </el-button>
                    <el-button v-if="showButtonCustom" type="primary" @click="CustomButtonClick" class="add-button" style="margin-left:10px">
                        <span v-if="buttonCustomIcon == 'Edit'" class="add-icon"><img src="@/assets/icons/Button/circle-add.svg" alt="insert" /></span>
                        <span class="add">{{ buttonCustomText }}</span>
                    </el-button>
                    <el-button v-if="showButtonIntegrate" type="primary" @click="Integrate" class="add-button" style="margin-left:10px">
                        <span class="add-icon"><img src="@/assets/icons/Button/circle-add.svg" alt="integrate" /></span>
                        <span class="add">{{ $t("IntegrateData") }}</span>
                    </el-button>
                    <el-button v-if="showButtonInsert" type="primary" @click="Insert" class="add-button" style="margin-left:10px">
                        <span class="add-icon"><img src="@/assets/icons/Button/circle-add.svg" alt="insert" /></span>
                        <span class="add">{{ $t("Insert") }}</span>
                    </el-button>
                    <el-button v-if="showButtonSave" type="primary" @click="SaveBtn" style="margin-left:10px">
                        <span class="add">{{ $t("Save") }}</span>
                    </el-button>

                    <el-button @click="openCloseGridConfigPanel" v-if="showButtonColumConfig" style="margin-left: 10px;">
                        <img v-show="showConfigGridPanel === false" src="@/assets/icons/function-bar/adjust.svg" style="width: 18px; height: 18px" />
                        <img class="no-filter" v-show="showConfigGridPanel === true" src="@/assets/icons/function-bar/cross.svg" style="width: 18px; height: 18px" />
                    </el-button>
                    <div class="popupshowcol" v-show="showConfigGridPanel">
                        <div class="switch-wapper">
                            <div class="col-item" v-for="col in listColumn" :key="col.ID" style="overflow-x: hidden;">
                                <el-switch v-model="col.display" :active-text="col.ColumnName"
                                :title="col.ColumnName" style="white-space: nowrap; text-overflow: ellipsis;">

                                </el-switch>
                            </div>
                        </div>
                        <el-button @click="saveListCol" style="margin-right: 10px; margin-top: 10px">
                            <img v-show="showConfigGridPanel === true" src="@/assets/icons/function-bar/foursquare-check-in.svg" style="width: 18px; height: 18px" />
                        </el-button>
                    </div>


                    <!--<div class="function-bar">
        <div class="left">

        </div>
        <div class="right" v-if="showButtonColumConfig">
            <el-button @click="openCloseGridConfigPanel">
                <img v-show="showConfigGridPanel === false" src="@/assets/icons/function-bar/adjust.svg" style="width: 18px; height: 18px" />
                <img class="no-filter" v-show="showConfigGridPanel === true" src="@/assets/icons/function-bar/cross.svg" style="width: 18px; height: 18px" />
            </el-button>
            <div class="popupshowcol" v-show="showConfigGridPanel">
                <div class="switch-wapper">
                    <div class="col-item" v-for="col in listColumn" :key="col.ID">
                        <el-switch :disabled="col.Fixed" v-model="col.display" :active-text="col.ColumnName">

                        </el-switch>
                    </div>
                </div>
                <el-button @click="saveListCol" style="margin-right: 10px; margin-top: 10px">
                    <img v-show="showConfigGridPanel === true" src="@/assets/icons/function-bar/foursquare-check-in.svg" style="width: 18px; height: 18px" />
                </el-button>
            </div>
        </div>
    </div>-->
                </div>
            </div>
  
</template>
<script src="./data-table-function-component.ts"></script>

<style lang="scss">
    .popupshowcol {
        z-index: 10;
        position: absolute;
        top: 40px;
        right: 5px;
        height: fit-content;
        width: 200px;
        background-color: whitesmoke;
        border-radius: 10px;
        border: 0.5px solid #bdbdbd;
        box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.25);
        padding: 10px 5px 10px 10px;
        display: flex;
        flex-direction: column;
        align-items: flex-end;

        .switch-wapper {
            height: fit-content;
            max-height: 40vh;
            overflow: auto;
            width: 100%;
        }
    }
    .more {
        display: flex;
        align-items: center;
        border-radius: 10px;
        padding: 0 10px;
        margin-left: 10px;
        height: 30px;
        width: fit-content;
        background-color: #d2e5f8;
    }
/*.function-bar {
  height: 45px;
  width: 100%;
  display: flex;
  justify-content: space-between;
  margin-top:0px;
  align-items: center;
  .left {
    display: flex;
    align-items: center;
  }
  .right {
    display: flex;
    align-items: center;
    position: relative;
    .popupshowcol {
      z-index: 10;
      position: absolute;
      top: 40px;
      right: 5px;
      height: fit-content;
      width: 200px;
      background-color: whitesmoke;
      border-radius: 10px;
      border: 0.5px solid #bdbdbd;
      box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.25);
      padding: 10px 5px 10px 10px;
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      .switch-wapper {
        height: fit-content;
        max-height: 40vh;
        overflow: auto;
        width: 100%;
      }
    }
  }
  .more {
    display: flex;
    align-items: center;
    border-radius: 10px;
    padding: 0 10px;
    margin-left: 10px;
    height: 30px;
    width: fit-content;
    background-color: #d2e5f8;
  }*/
  /*.ui-switch__label-text {
    font-size: 12px;
  }*/
/*}*/
</style>

<style scoped>
    .add-button {
        padding: 8px 16px 8px 10px;
    }

    .add, .add-icon {
        float: left;
        display: block;
        height: 20px;
    }

    .add-icon {
        width: 20px;
    }

    .add {
        margin-left: 10px;
        height: 20px;
        line-height: 20px;
    }

    .datatable-function {
        margin-right: 24px;
        width: calc(100% - 304px);
        /* width: 300px; */
        display: flex;
        justify-content: space-between;
        height: 36px;
        position: absolute;
        top: 29px;
        right: 12px;
    }

        .datatable-function .el-button span {
            font-weight: 600;
            font-size: 14px;
        }
    /* .datatable-function.btn {
      width: 413px;
    } */
    .el-main.bgHomeHasView .datatable-function {
        margin-right: 24px;
        width: calc(100% - 314px);
        display: flex;
        justify-content: space-between;
        height: 36px;
        position: absolute;
        top: 50px;
        right: 12px;
    }

    .el-button--primary {
        font-weight: 600;
    }

    .group-btn {
        width: fit-content;
        display: inline-block;
    }
        /* .group-btn button:first-child {
      font-weight: normal;
    } */
        .group-btn button:last-child {
            margin-left: 10px;
        }

        .group-btn button {
            float: left;
        }
</style>
