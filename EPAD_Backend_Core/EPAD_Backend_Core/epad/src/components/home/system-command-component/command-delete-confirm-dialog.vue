<template>
    <el-dialog
  title="Warning"
  :visible.sync="kShowDialog"
  width="30%"
  top="38vh">
  <div class="title">Cảnh báo!</div>
  <div class="warning-message">{{messageConfirm}}</div>
  <el-checkbox
    v-if="hasUncompletedCommand"
    v-model="isConfirmDeleteUncompletedCommand"
    style="margin-top: 16px">
    Tôi xác nhận xóa dữ liệu
    </el-checkbox>
  <span slot="footer" class="dialog-footer">
    <el-button @click="closeDialog">{{$t('Cancel')}}</el-button>
    <el-button type="primary" @click="deleteCommands">{{$t('Delete')}}</el-button>
  </span>
</el-dialog>
</template>
<script lang="ts">
import { systemCommandApi } from '@/$api/system-command-api';
import Vue, { PropType } from 'vue'
export default Vue.extend({
    props: {
        showDialog: {
            type: Boolean,
            default: false,
        },
        commandsWillDelete: {
            type: Array as PropType<Array<any>>,
            default: [],
        },
    },
    data() {
        return {
            kShowDialog: this.showDialog,
            messageConfirm: '',
            hasUncompletedCommand: false,
            isConfirmDeleteUncompletedCommand: false,
        }
    },
    watch: {
        kShowDialog() {
            this.$emit('update:showDialog', this.kShowDialog);
        },
        showDialog() {
            this.kShowDialog = this.showDialog;
            if (this.showDialog) {
                this.hasUncompletedCommand = Boolean(this.commandsWillDelete.find(cmd => cmd.Status == this.$t('Processing') || cmd.Status == this.$t('Unexecuted')));
                if (this.hasUncompletedCommand) {
                    this.messageConfirm = 'Lưu ý, khi bạn xóa những dòng dữ liệu được chọn, sau đó sẽ không thực hiện tạo lại lệnh được cho những dòng dữ liệu này?'; 
                } else {
                    this.messageConfirm = 'Bạn có chắc muốn xóa những dòng dữ liệu được chọn?';
                }
            } else {
                this.isConfirmDeleteUncompletedCommand = false;
            }
        },
    },
    methods: {
        closeDialog() {
            this.kShowDialog = false;
        },
        deleteCommands() {
            if (!this.hasUncompletedCommand || (this.hasUncompletedCommand && this.isConfirmDeleteUncompletedCommand)) {
                this.$notify({
                    type: 'info',
                    title: 'Thông báo từ thiết bị',
                    dangerouslyUseHTMLString: true,
                    message: "Đang xóa lệnh, vui lòng chờ.",
                    customClass: 'notify-content',
                    duration: 2000
                });
                systemCommandApi.DeleteByIds(this.commandsWillDelete.map(cmd => cmd.Index.toString()))
                  .then(() => {
                      this.$emit('onDeleteSuccess');
                      this.$deleteSuccess();
                  });
            } else {
               this.$notify({
                   title: 'Chưa xóa lệnh',
                   message: 'Xóa lệnh không thành công vì chưa xác nhận!',
                   type: 'info',
               });
            }
           this.closeDialog();
        }
    }
})
</script>
<style scoped>
.title {
    color: red;
    font-weight: 600;
    margin-bottom: 8px;
}
.warning-message {
    word-break: keep-all;
}
</style>