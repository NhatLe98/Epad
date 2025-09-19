import Vue from 'vue';
import { ElMessageBoxOptions, MessageBoxData } from 'element-ui/types/message-box';
import { ElNotificationOptions } from 'element-ui/types/notification';
import { isNullOrUndefined } from 'util';
import { EventBus } from '@/$core/event-bus';
import { store } from '@/store';

declare module 'vue/types/vue' {
  interface Vue {
    $broadCast: Vue;
    /**
     * Thông báo lưu thành công
     * @param options
     */
    $saveSuccess(options?: Partial<ElNotificationOptions>);

    /**
     * Thông báo lưu thất bại
     * @param options
     */
    $saveError(
      options?: Partial<ElNotificationOptions>,
      title?: string,
      message?: string
    );

    /**
     * Thông báo lưu thất bại
     * @param options
     */
    $alertSaveError(
      error,
      options?: Partial<ElNotificationOptions>,
      title?: string,
      message?: string
    );

    /**
     * Thông báo lỗi khi request
     * @param options
     */
    $alertRequestError(
      error,
      options?: Partial<ElNotificationOptions>,
      title?: string,
      message?: string
    );

    $apiAlertRequestError(
      error,
      options?: Partial<ElNotificationOptions>,
      title?: string,
      message?: string
    );

    /**
     * Xác nhận xóa dữ liệu
     * @param message
     * @param tittle
     * @param options
     */
    $confirmDelete(
      message?: string,
      tittle?: string,
      options?: Partial<ElMessageBoxOptions>
    ): Promise<MessageBoxData>;

    $confirmDeleteNew(
      message?: string,
      tittle?: string,
      options?: Partial<ElMessageBoxOptions>
    ): Promise<MessageBoxData>;

    /**
     * Thông báo xóa thành công
     * @param options
     */
    $deleteSuccess(options?: Partial<ElNotificationOptions>);

    /**
     * Thông báo xóa thất bại
     * @param options
     */
    $deleteError(options?: Partial<ElNotificationOptions>, error?: Error);

    /**
     * Xác nhận thêm phần tử
     * @param message
     * @param tittle
     * @param options
     */
    $confirmInsert(
      message?: string,
      tittle?: string,
      options?: Partial<ElMessageBoxOptions>
    ): Promise<MessageBoxData>;

    /**
     * Cảnh báo chọn duy nhất 1 dòng để xử lý
     * @param options
     */
    $warningSelectOnlyOneRow(options?: Partial<ElNotificationOptions>);

    /**
     * Cảnh báo chọn dòng để chỉnh sửa
     * @param options
     */
    $warningSelectRowToEdit(options?: Partial<ElNotificationOptions>);
  }
}

Vue.prototype.$saveSuccess = function(
  this: Vue,
  error?: ElNotificationOptions
) {
  const title = this.$t('Notify').toString();
  const message = this.$t('MSG_SaveSuccessTitle').toString();

  const defaultOption: ElNotificationOptions = {
    title,
    message,
    position: 'top-right'
  };
  this.$notify.success(Object.assign(defaultOption, error));
  return;
};

Vue.prototype.$saveError = function(
  this: Vue,
  option: ElMessageBoxOptions = null,
  title = null,
  message = null
) {
  const defaultMessage =
     this.$t('MSG_SaveError').toString();
  const defaultTitle =
    title || this.$t('MSG_SaveErrorTitle').toString();

  const defaultOption: ElMessageBoxOptions = {
    type: 'error',
    confirmButtonText: this.$t('MSG_Confirm').toString(),
    showCancelButton: false
  };
  return this.$alert(
    defaultMessage,
    defaultTitle,
    Object.assign(defaultOption, option)
  );
};

Vue.prototype.$alertSaveError = function(
  this: Vue,
  error,
  option = {},
  title = '',
  message = ''
) {
  const defaultMessage =
    message || this.$t('MSG_SaveError').toString();
  const defaultTitle =
    title || this.$t('MSG_SaveErrorTitle').toString();
  const defaultOption: ElMessageBoxOptions = {
    confirmButtonText: this.$t('MSG_Confirm').toString(),
    dangerouslyUseHTMLString: true,
    showCancelButton: false,
    title: defaultTitle,
    message: defaultMessage,
    type: 'error'
  };

  if(store.getters['Misc/isErrorStack'] === false)
    return this.$msgbox(Object.assign(defaultOption, option));
};

Vue.prototype.$confirmDelete = function(
  this: Vue,
  message = null,
  title = null,
  option: ElMessageBoxOptions = null
) {
  const defaultMessage =
    message || this.$t('MSG_ConfirmDelete');
  const defaultTitle =
    title || this.$t('MSG_Confirm');

  const defaultOption: ElMessageBoxOptions = {
    type: 'warning',
    confirmButtonText: this.$t('MSG_Delete').toString(),
    cancelButtonText: this.$t('MSG_No').toString(),
    showCancelButton: true
  };
  return this.$confirm(
    defaultMessage,
    defaultTitle,
    Object.assign(defaultOption, option)
  );
};

Vue.prototype.$confirmDeleteNew = function(
  this: Vue,
  message = null,
  title = null,
  option: ElMessageBoxOptions = null
) {
  // const msg1 = this.$t('MSG_ConfirmDeletePlus').toString();
  // const msg2 = this.$t('MSG_ConfirmDelete').toString();
  const defaultMessage = 'Xóa đối tượng ' + message + ' sẽ xóa tất cả dữ liệu người dùng liên quan. Bạn có chắc muốn xóa những dòng được chọn?';
  const defaultTitle =
    title || this.$t('MSG_Confirm');

  const defaultOption: ElMessageBoxOptions = {
    type: 'warning',
    confirmButtonText: this.$t('MSG_Delete').toString(),
    cancelButtonText: this.$t('MSG_No').toString(),
    showCancelButton: true
  };
  return this.$confirm(
    defaultMessage,
    defaultTitle,
    Object.assign(defaultOption, option)
  );
};

Vue.prototype.$deleteSuccess = function(
  this: Vue,
  error?: ElNotificationOptions
) {
  const defaultOption: ElNotificationOptions = {
    title: this.$t('Notify').toString(),
    message: this.$t('MSG_DeleteSuccess').toString(),
    position: 'top-right'
  };
  this.$notify.success(Object.assign(defaultOption, error));
  return;
};

Vue.prototype.$deleteError = function(
  this: Vue,
  option?: ElNotificationOptions,
  error?: any
) {
  const defaultOption: ElNotificationOptions = {
    type: 'error',
    title: this.$t('MSG_DeleteErrorTitle').toString(),
    message: this.$t('MSG_DeleteError').toString(),
    position: 'top-right'
  };
  if (
    !isNullOrUndefined(error) &&
    !isNullOrUndefined(error.responseStatus) &&
    !isNullOrUndefined(error.responseStatus.message)
  ) {
    const message: string = error.responseStatus.message;
    if (
      !Misc.isEmpty(message) &&
      message.includes(
        'The DELETE statement conflicted with the REFERENCE constraint'
      )
    ) {
      Object.assign(defaultOption, {
        message: this.$t(
          'MSG_DeleteConflictMessage'
        ).toString()
      });
    }
  }

  this.$msgbox(Object.assign(defaultOption, option));
};

Vue.prototype.$confirmInsert = function(
  this: Vue,
  message = null,
  title = null,
  option: ElMessageBoxOptions = null
) {
  const defaultMessage =
    message || this.$t('MSG_ConfirmInsert');
  const defaultTitle =
    title || this.$t('MSG_ConfirmInsertTitle');

  const defaultOption: ElMessageBoxOptions = {
    type: 'error',
    confirmButtonText: this.$t('MSG_Yes').toString(),
    showCancelButton: false,
    showClose: false,
    closeOnClickModal: true
  };
  return this.$alert(
    defaultMessage,
    defaultTitle,
    Object.assign(defaultOption, option)
  );
};

Vue.prototype.$warningSelectOnlyOneRow = function(
  this: Vue,
  error?: ElNotificationOptions
) {
  const defaultOption: ElNotificationOptions = {
    title: this.$t('Notify').toString(),
    message: this.$t('MSG_SelectOnlyOneRow').toString(),
    position: 'top-right'
  };

  this.$notify.warning(Object.assign(defaultOption, error));
};

Vue.prototype.$warningSelectRowToEdit = function(
  this: Vue,
  error?: ElNotificationOptions
) {
  const defaultOption: ElNotificationOptions = {
    title: this.$t('Notify').toString(),
    message: this.$t('MSG_SelectRowToEdit').toString(),
    position: 'top-right'
  };

  this.$notify.warning(Object.assign(defaultOption, error));
};

Vue.prototype.$alertRequestError = function(
  this: Vue,
  error,
  option = {},
  title = '',
  message = ''
) {
  const defaultMessage =
    message || this.$t('MSG_SystemError').toString();
  const defaultTitle =
    title || this.$t('Notify').toString();
  const defaultOption: ElMessageBoxOptions = {
    confirmButtonText: this.$t('MSG_Confirm').toString(),
    dangerouslyUseHTMLString: true,
    showCancelButton: false,
    title: defaultTitle,
    message: defaultMessage,
    type: 'error'
  };

  return this.$msgbox(Object.assign(defaultOption, option));
};

Vue.prototype.$apiAlertRequestError = function(
  this: Vue,
  error,
  option = {},
  title = '',
  message = ''
) {
  const defaultMessage =
    message || this.$t('MSG_SystemError').toString();
  const defaultTitle =
    title || this.$t('Notify').toString();
  const defaultOption: ElMessageBoxOptions = {
    confirmButtonText: this.$t('MSG_Confirm').toString(),
    dangerouslyUseHTMLString: true,
    showCancelButton: false,
    title: defaultTitle,
    message: defaultMessage,
    type: 'error',
    callback(action, instance){
      if(action === 'confirm' || action === 'cancel') {
        const errorStack = store.getters['Misc/isErrorStack'];
        store.commit('Misc/offErrorStack');
      }
    }
  };

  return this.$msgbox(Object.assign(defaultOption, option));
};

Vue.prototype.$broadCast = EventBus;
declare global {
  interface Window { VueInstance: Vue; }
}