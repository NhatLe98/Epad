<template>
    <div>
        <el-dialog :visible="show" :before-close="Cancel" class="alert-dialog">
            <div style="text-align: center;">
                <span style="display: block; font-weight: bold; font-size: 1vw">{{ title }}</span>
                <img v-if="type == true" src="@/assets/icons/correct.png" alt="edit" style="width: 3vw; margin-top: 3vh;"/>
                <img v-if="type == false" src="@/assets/icons/wrong.png" alt="edit" style="width: 3vw; margin-top: 3vh;"/>
                <span v-if="message && message != ''" style="display: block; margin-top: 3vh;">{{ message }}</span>
                <el-button class="alert-dialog__cancel-button" 
                    style="margin-top: 3vh;display: block; left: 50%; position: relative; transform: translate(-50%, 0);"
                    @click="Cancel">{{ $t('Cancel') }}</el-button>
            </div>
            <el-input ref="hiddenScanQR" v-if="show && enableHiddenScanQR" v-model="hiddenScanQR"
                style="opacity: 0; height: 0; position: absolute; top: 0; left: 0; width: 0;" 
                @keyup.enter.native="changeScanQR"></el-input>
        </el-dialog>
    </div>
</template>
<script src="./alert-component.ts"></script>
<style lang="scss">
.alert-dialog{
    .el-dialog{
        width: 20vw !important;
        margin-top: 0 !important;
        top: 50%;
        transform: translate(0, -50%);
        .el-dialog__header{
            /*padding: 0;
            margin-top: 10px;*/
        }
        .el-dialog__close{
            position: relative;
            top: -15px;
        }
        .alert-dialog__cancel-button{
            background-color: white;
        }
    }
}
</style>