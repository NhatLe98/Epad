<template>
    <el-dialog :close-on-click-modal="false" :visible="showDialog" custom-class="notifydialog" :show-close="false" :modal-append-to-body="false">
        <div class="notifydialog-header">
			<div class="left">	
				<span style="font-size: 12px; line-height: 16px; color: #000000; margin-right: 5px;">
                    {{title}}</span>
				<template >
					<span
						style="margin-right: 10px; font-weight: bold; font-size: 12px; line-height: 16px;color: #333333;"
						>{{ $t('SelectAll') }}</span
					>
					<!-- <el-checkbox v-model="checkAll" @change="handleCheckAllChange"></el-checkbox> -->
					<el-checkbox @change="handleCheckAllChange"></el-checkbox>
				</template>
			</div>
			<div class="right">
                <el-button @click="DeleteAll" class="btn28 mrl4" type="danger" round>{{ $t('DeleteAll') }}</el-button>
				<a class="btnClose" @click="showHideDialog(false)">{{ $t('Close') }}</a>
			</div>
		</div>
        <div class="notifydialog-body">
			<div class="viewdetail-waitinglist" v-loading="loading" style="overflow:auto;">
				<!-- <el-checkbox v-model="checkedItems" @change="handleCheckedItemsChange"></el-checkbox> -->
                <template v-if="!loading" >
                    <div style="margin-left:20px;margin-right:20px;" v-for="(item, index) in waitingApproveList" :key="index">
                        <div class="item">
                            <el-row type="flex">
                                <el-col :span="2">
                                    <el-checkbox v-model="item.IsChecked"></el-checkbox>
                                </el-col>
                                <el-col :span="8">
                                    <div>
                                        <span>{{ $t('Employee') }}:</span>
                                        <span class="text-bold" style="margin-left:5px;">{{ $t(item.EmployeeName) }}</span>
                                    </div>
                                    <div>
                                        <span>{{ $t('CurrentDepartment') }}:</span>
                                        <span class="text-bold" style="margin-left:5px;">{{ $t(item.OldDepartment) }}</span>
                                    </div>
                                    <div>
                                        <span>{{ $t('DepartmentTransfer') }}:</span>
                                        <span class="text-bold" style="margin-left:5px;">{{ $t(item.NewDepartment) }}</span>
                                    </div>
                                    <div>
                                        <span>{{ $t('FromDate') }}:</span>
                                        <span class="text-bold" style="margin-left:5px;">{{ formatDateField(item.FromDate) }}</span>
                                    </div>
                                    <div>
                                        <span>{{ $t('ToDate') }}:</span>
                                        <span class="text-bold" style="margin-left:5px;">{{ formatDateField(item.ToDate) }}</span>
                                    </div>
                                </el-col>
                                <el-col :span="8">
                                    <div>
                                        <span>{{ $t('TypeTemporaryTransfer') }}:</span>
                                        <span class="text-bold" style="margin-left:5px;">{{ getTransferType(item.Type) }}</span>
                                    </div>
                                    <div>
                                        <span>{{ $t('SuggestUser') }}:</span>
                                        <span class="text-bold" style="margin-left:5px;">{{ $t(item.SuggestUser) }}</span>
                                    </div>
                                    <div>
                                        <span>{{ $t('SuggestDate') }}:</span>
                                        <span class="text-bold" style="margin-left:5px;">{{ formatDateField(item.SuggestDate) }}</span>
                                    </div>
                                </el-col>
                                <el-col :span="6" class="flex-col-center-end">
                                    <el-button @click="Delete(index)" class="mar-l0 border-warning btn-reject btn-pendinglist" round>{{ $t('Delete') }}</el-button>
                                </el-col>
                            </el-row>
                        </div>
                    </div>
                </template>
                
			</div>
		</div>
    </el-dialog>
</template>
<script src="./approve-popup-component.ts"></script>
<style lang="scss">
.viewdetail-waitinglist {
  overflow: auto;
}
.notify-wrapper {
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
  align-items: center;
}
.notify-item {
  display: flex;
  width: 100%;
  height: 55px;
  justify-content: center;
  align-items: center;
  &:hover {
    background-color: #f0f4ff;
    cursor: pointer;
  }
}

.notify-item img {
  cursor: pointer;
}

.notifydialog {
  .el-loading-parent--relative {
    height: 50px;
  }
  .el-dialog__body {
    padding: 0;
    height: 100%;
  }
  width: 85vw;
  height: 85vh;
  margin-top: 40px !important;
  &.approve-dialog {
    width: 50vw;
    height: 50vh;
  }
  .notifydialog-header {
    border-bottom: 0.5px solid #828282;
    padding: 0 18px 0 23px;
    width: 100%;
    height: 50px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    &.approvedialog-header {
      justify-content: space-between;
    }
    .right {
      display: flex;
      justify-content: flex-end;
      align-items: center;
      a {
        margin-left: 17px;
      }
    }
    .center {
      height: 100%;
      display: flex;
      align-items: center;
      // width: fit-content;
      span {
        display: flex;
        align-items: center;
        width: fit-content;
        padding: 0 4px;
        height: 100%;
        font-size: 12px;
        line-height: 16px;
        &:last-child {
          margin-left: 15px;
        }

        &:hover,
        &.isActive {
          border-bottom: 2px solid #2f80ed;
          font-weight: bold;
        }
        &:hover {
          cursor: pointer;
        }
        // display: flex;
        // justify-items: center;
      }
    }
  }
  .notifydialog-body {
    display: flex;
    flex-direction: column;
    width: 100%;
    // padding: 10px 18px 10px 23px;
    height: calc(100% - 51px);
    &.approvedialog-body {
      padding: 10px 30px;
    }
    .notify-form {
      .el-row {
        margin-bottom: 10px;
      }
      .el-form-item__label {
        padding-bottom: 2px;
      }
    }
    .el-select > .el-input > input {
      background-color: #ffffff !important;
    }

    .el-textarea__inner {
      border: none;
      background-color: #f3f3f3;
    }
  }
}
.notifydialog.confirm-reason {
  width: 500px;
  .logo-title {
    margin-top: 30px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    height: fit-contain;
    width: 100%;
    .logo {
      margin-bottom: 20px;
      img {
        width: 120px;
        height: 120px;
        object-fit: contain;
      }
    }
    .title {
      font-size: 18px;
    }
  }
}
.el-badge__content.is-fixed {
  right: 17px;
}
.text-bold {
  font-weight: 600;
}
.btnClose:hover {
  cursor: pointer;
}
</style>