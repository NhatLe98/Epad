import { Component, Mixins, Vue, Prop } from 'vue-property-decorator';
import { PageBase } from '@/mixins/application/page-mixins';
import { Mutation, Getter } from 'vuex-class';
import { API_URL } from '@/$core/config';
import axios from 'axios';
import Recaptcha2 from './Recaptcha2.vue';
import { Form as ElForm, Form } from 'element-ui';
import { EventBus } from '@/$core/event-bus';

import { userAccountApi, IPasswordInfo, IResetPassword } from '@/$api/user-account-api';
import { isNullOrUndefined } from 'util';

@Component({
	name: 'login',
	components: { Recaptcha2 },
})
export default class Login extends Mixins(PageBase) {
	@Mutation('setToken', { namespace: 'Token' }) setToken;
	@Getter('getNewPassword', { namespace: 'NewPassword' }) $getNewPassword;
	// @Prop({default: 'asd'})
	// abc: string

	isLogin = true;
	emailReset = '';
	redirect = '';
	isForgetPassword = false;
	isChangePassword = false;
	isResetPassword = false;
	loading = false;
	isRemember = false;
	showDialog = false;
	sendLoading = false;
	dialogChangePassoword = false;
	showRecaptcha = false;
	ruleForm: IPasswordInfo = {
		UserName: '',
		Email: '',
		Password: '',
		NewPassword: '',
		ConfirmPassword: '',
		ServiceId: 0,
	};

	rules: any = {};

	resetForm: IResetPassword = {
		Code: '',
		NewPassword: '',
		ConfirmNewPassword: '',
		UserName: '',
	};
	ruleReset: any = {};

	options = [
		{
			value: 'vi',
			label: 'Tiếng Việt',
			image: 'flagvn',
		},
		{
			value: 'en',
			label: 'English',
			image: 'flaguk',
		},
	];
	lang = "vi";

	get getLanguageForRenderFlag() {
		if (this.lang === 'en') {
			return 'select-language-flaguk';
		} else {
			return 'select-language-flagvn';
		}
	}
	clientName = "";
	
	isHuman: boolean = true;
	get isSendDisabled(): boolean {
		return !this.isHuman;
	}
	robotChange(v: boolean) {
		this.isHuman = v;
		self.localStorage.removeItem('wrongCount');
	}

	beforeCreate() {
		const promiseA = new Promise((resolve, reject) => {
			Misc.readFileAsync('static/variables/common-utils.json').then(x => {
			 resolve(this.clientName);
			 this.clientName = x.ClientName;
		 })
		 });
		if (isNullOrUndefined(localStorage.getItem('access_token'))) {
			// this.setToken(null)
		} else {
			this.$router.push(this.redirect || (this.clientName == "Mondelez" ? '/factory-user-monitoring' : '/dashboard'));
			// this.setToken(null);
		}
	}
	initRules() {
		this.rules = {
			UserName: [
				{
					type: 'email',
					message: this.$t('PleaseInputEmailFormat'),
					trigger: 'blur',
				},
			],
			Password: [
				{
					required: true,
					message: this.$t('PleaseInputPassword'),
					trigger: 'blur',
				},
			],
		};
		this.ruleReset = {
			Code: [
				{
					required: true,
					message: this.$t('PleaseInputCode'),
					trigger: 'blur',
				},
			],
			NewPassword: [
				{
					required: true,
					message: this.$t('PleaseInputNewPassword'),
					trigger: 'blur',
				},
				{
					message: this.$t('RequiredTypePW'),
					validator: (rule, value: string, callback) => {
						const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{8,}$/;
						if (isNullOrUndefined(value) === false && (value.length < 8 || regex.test(value) === false)) {
							callback(new Error());
						} else {
							callback();
						}
					},
				},
			],
			ConfirmNewPassword: [
				{
					required: true,
					message: this.$t('PleaseInputComfirmNewPW'),
					trigger: 'blur',
				},
				{
					message: this.$t('ConfirmPWMustMatch'),
					validator: (rule, value: string, callback) => {
						if (isNullOrUndefined(value) === false && value !== this.resetForm.NewPassword) {
							callback(new Error());
						} else {
							callback();
						}
					},
				},
			],
		};
	}
	beforeMount() {
		this.initRules();
		if (!isNullOrUndefined(this.$route.params.email)) {
			localStorage.setItem('formResetPW', 'true');
			localStorage.setItem('userNameResetPW', this.resetForm.UserName);
			this.resetForm.UserName = this.$route.params.email;
			this.resetForm.Code = this.$route.params.code;
			this.isLogin = false;
			this.isForgetPassword = false;
			this.isResetPassword = true;
		} else if (JSON.parse(localStorage.getItem('formResetPW')) === true) {
			this.resetForm.UserName = localStorage.getItem('userNameResetPW');
			this.isLogin = false;
			this.isForgetPassword = false;
			this.isResetPassword = true;
		}
	}

	forgetPassword() {
		this.isForgetPassword = true;
		this.isLogin = false;
	}

	backLogin() {
		this.ruleForm.UserName = '';
		this.ruleForm.Password = '';
		this.isForgetPassword = false;
		this.isResetPassword = false;
		this.isLogin = true;
		localStorage.removeItem('formResetPW');
		localStorage.removeItem('userNameResetPW');
	}

	async restPassword() {
		var regex_checkEmailFormat = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
		if (this.ruleForm.UserName === '') {
			this.$alertSaveError(null, null, null, this.$t('EmailIsRequired').toString());
		} else if (regex_checkEmailFormat.test(this.ruleForm.UserName) === false) {
			this.$alertSaveError(null, null, null, this.$t('InvalidEmail').toString());
			return;
		} else {
			this.showDialog = true;
			this.sendLoading = true;

			var isResFirst = null;
			var isTimeFirst = null;
			var isErrFirst = null;
			setTimeout(() => {
				if (isResFirst === true) {
					this.showDialog = false;
					this.sendLoading = false;
					this.isForgetPassword = false;
					this.isResetPassword = true;

					this.resetForm.Code = '';
					this.resetForm.NewPassword = '';
					this.resetForm.ConfirmNewPassword = '';

					this.resetForm.UserName = this.ruleForm.UserName;
					localStorage.setItem('formResetPW', 'true');
					localStorage.setItem('userNameResetPW', this.resetForm.UserName);
				} else if (isErrFirst === true) {
					this.isLogin = false;
					this.showDialog = false;
					this.sendLoading = false;
				} else {
					isTimeFirst = true;
				}
			}, 1000);
			await userAccountApi
				.SendResetPasswordCode(this.ruleForm)
				.then((res) => {
					if (isTimeFirst === true && res.status !== 200) {
						this.showDialog = false;
						this.sendLoading = false;
						this.isForgetPassword = false;
						this.isResetPassword = true;

						this.resetForm.Code = '';
						this.resetForm.NewPassword = '';
						this.resetForm.ConfirmNewPassword = '';

						this.resetForm.UserName = this.ruleForm.UserName;
						localStorage.setItem('formResetPW', 'true');
						localStorage.setItem('userNameResetPW', this.resetForm.UserName);
					} else {
						isResFirst = true;
					}
				})
				.catch((err) => {
					if (isTimeFirst === true) {
						this.isLogin = false;
						this.showDialog = false;
						this.sendLoading = false;
					} else {
						isErrFirst = true;
					}
				});
		}
	}

	verifyHuman() {
		this.notify(this.$t('Notify').toString(), this.$t('VerifyNotRobot').toString(), 'e', 'tr');
	}

	async login() {
		if (this.isLogin === false) {
			return;
		} else {
			var theField = eval('this.$refs.inputPassword');
			theField.blur();
			this.loading = true;
			await axios
				.post(`${API_URL}/login`, this.ruleForm, {})
				.then((res) => {
					self.localStorage.removeItem('wrongCount');
					this.isHuman = true;
					this.loading = false;
					this.setToken(res.data.access_token);
					if (this.isRemember == true) {
						self.localStorage.setItem('username', this.ruleForm.UserName);
						self.localStorage.setItem('password', this.ruleForm.Password);
						self.localStorage.setItem('isRemember', 'true');
					} else {
						self.localStorage.setItem('username', '');
						self.localStorage.setItem('password', '');
						self.localStorage.setItem('isRemember', 'false');
					}

					localStorage.removeItem("masterEmployeeFilter");
					this.$router.push(this.redirect || (this.clientName == "Mondelez" ? '/factory-user-monitoring' : '/dashboard')).catch((err) => {});
					this.notify(this.$t('Notify').toString(), this.$t('LoginSuccess').toString(), 's', 'tr');

					if(res.data.message && res.data.message != ""){
						this.$alert(`${this.$t(res.data.message).toString()}`, this.$t('Warning').toString(), {
							dangerouslyUseHTMLString: true
						});
					}
				})
				.catch((error) => {
					if (error.response.status === 401) {
						var count = self.localStorage.getItem('wrongCount');
						if (count === null) {
							self.localStorage.setItem('wrongCount', '1');
						} else {
							self.localStorage.setItem('wrongCount', parseInt(count) + 1 + '');
						}
						if (parseInt(count) > 1 || count === '2') {
							this.isHuman = false;
						}
						this.loading = false;
						const message = error.response.data.message || 'UsernamePasswordInvalid';
						if (message === 'MSG_LicenseInvalid' || message === 'MSG_LicenseExpired') {
							// this.$alertRequestError(null, null, this.$t('Notify').toString(), this.$t(message).toString())
							this.$alert(this.$t(message).toString(), this.$t('Notify').toString(), {
								confirmButtonText: 'OK',
								callback: (action) => {
									this.$router.push('activate');
								},
							});
						} else {
							this.$alertRequestError(null, null, this.$t('Notify').toString(), this.$t(message).toString());
						}
					}
				});
		}
	}

	mounted() {
		const { redirect } = this.$route.query;
		if (!Misc.isEmpty(redirect)) {
			this.redirect = redirect.toString();
		}
		if (self.localStorage.getItem('isRemember') == 'true') {
			this.ruleForm.UserName = self.localStorage.getItem('username');
			this.ruleForm.Password = self.localStorage.getItem('password');
			this.isRemember = true;
		} else {
			this.ruleForm.Email = '';
			this.ruleForm.Password = '';
			this.isRemember = false;
		}
		//var theField = eval('this.$refs.inputUserName')
		// theField.focus()
	}
	get isDisableLogin() {
		if (this.ruleForm.UserName !== '' && this.ruleForm.Password !== '') {
			return false;
		} else {
			return true;
		}
	}
	Cancel(x) {
		var ref = <ElForm>this.$refs[x];
		ref.resetFields();
		this.dialogChangePassoword = false;
	}
	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	get disableButtonSend() {
		const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{8,}$/;
		if (
			this.resetForm.Code !== '' &&
			this.resetForm.NewPassword !== '' &&
			this.resetForm.ConfirmNewPassword !== '' &&
			this.resetForm.NewPassword.length > 7 &&
			regex.test(this.resetForm.NewPassword) === true &&
			this.resetForm.NewPassword === this.resetForm.ConfirmNewPassword
		) {
			return false;
		} else {
			return true;
		}
	}
	get isInvalidNewPW() {
		const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{8,}$/;
		let newPW = this.resetForm.NewPassword;
		if (newPW !== '' && regex.test(newPW) === false) {
			return true;
		} else {
			return false;
		}
	}
	get isInvalidConfirmNewPW() {
		let confirmNewPW = this.resetForm.ConfirmNewPassword;
		if (confirmNewPW !== '' && confirmNewPW !== this.resetForm.NewPassword) {
			return true;
		} else {
			return false;
		}
	}
	async sendResetPWAll() {
		// (this.$refs.resetForm as any).validate(async(valid) => {
		if (this.disableButtonSend === true) {
			return;
		} else {
			await userAccountApi.ResetPassword(this.resetForm).then((res) => {
				this.notify(this.$t('Notify').toString(), this.$t('ChangePasswordSuccess').toString(), 's', 'tr');
				localStorage.removeItem('formResetPW');
				localStorage.removeItem('userNameResetPW');
				this.isResetPassword = false;
				this.isLogin = true;
			});
		}
		//})
	}
}
