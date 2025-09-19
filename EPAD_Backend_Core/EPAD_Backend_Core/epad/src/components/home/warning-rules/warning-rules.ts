import { Component, Vue, Mixins, Watch } from 'vue-property-decorator';
import { WarningRulesModel, EmailScheduleModel, EmailScheduleRequestModel, ControllerWarningModel, 
	ControllerWarningRequestModel, rulesWarningApi } from '@/$api/gc-rules-warning-api';
import SelectControllerChannelComponent from '@/components/app-component/select-controller-channel-component/select-controller-channel-component.vue'
import { EzFile } from "@/$api/ez-portal-file-api";
import SingleUploadComponent from '@/components/home/custom-upload-component/single-upload-component.vue';
import moment from 'moment';
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
// import { isNullOrUndefined } from 'util';
// import { isLength } from 'lodash';
import { EzFileRequest } from '@/$api/gc-rules-warning-api';
import { lineApi, LineModel } from '@/$api/gc-lines-api';
import { split } from 'lodash';
// import { store } from '@/store';
import { Getter } from 'vuex-class';
const AccessType = {
	Line: 1,
	Gate: 2
}

@Component({
	name: 'warning-rules',
	components: {
		SingleUploadComponent,
		SelectControllerChannelComponent,
		HeaderComponent
	},
})
export default class WarningRules extends Mixins(ComponentBase) {
	// @Getter('getRoleByRoute', { namespace: 'RoleResource' }) roleByRoute: any;
	form: any = {
		UseSpeaker: false,
		UseSpeakerFocus: false,
		UseSpeakerInPlace: false,
		UseLed: false,
		UseLedFocus: false,
		UseLedInPlace: false,
		UseEmail: false,
		Email: null,
		EmailSendType: null,
		UseComputerSound: false,
		UseChangeColor: false,
	};
	rule: any;
	rowsObj: any;
	acceptFile = "audio/mpeg"
	confirmDelete = false;
	Attachments: EzFile[] = [];
	serialNumber = '';
	listColumn = [];
	// rowsObj = [];
	listData = [];
	listGroup = {};
	listGroupSelect = [];
	listControllerSelect = [];
	listChannelSelectSpeaker = [];
	listChannelSelectLed = [];
	fileList = [];
	listEmailScheduler: Array<EmailScheduleModel> = [
		{ Index: 0, Time: new Date(2021, 1, 1, 8, 0), DayOfWeekIndex: 1, Order: 0, Error: "" }
	];
	listSpeakerWarningFocus: Array<ControllerWarningModel> = [
		{ Index: 0, ControllerIndex: null, ChannelIndex: null, Order: 0, Error: "", Type: 0, LineIndex: null, GateIndex: null, SerialNumber: "" }
	];
	listSpeakerWarningInPlace: Array<ControllerWarningModel> = [
		{ Index: 0, ControllerIndex: null, ChannelIndex: null, Order: 0, Error: "", Type: 0, LineIndex: null, GateIndex: null, SerialNumber: "" }
	];
	listLedWarningFocus: Array<ControllerWarningModel> = [
		{ Index: 0, ControllerIndex: null, ChannelIndex: null, Order: 0, Error: "", Type: 1, LineIndex: null, GateIndex: null, SerialNumber: "" }
	];
	listLedWarningInPlace: Array<ControllerWarningModel> = [
		{ Index: 0, ControllerIndex: null, ChannelIndex: null, Order: 0, Error: "", Type: 1, LineIndex: null, GateIndex: null, SerialNumber: "" }
	];

	timeToSendMail = [];
	activeMenuIndex = 0;
	currentGroup = null;
	currentGroupName = '';
	isAdd = true;
	listDayOfWeek = [
		{ Index: 0, Name: "Sunday" },
		{ Index: 1, Name: "Monday" },
		{ Index: 2, Name: "Tuesday" },
		{ Index: 3, Name: "Wednesday" },
		{ Index: 4, Name: "Thursday" },
		{ Index: 5, Name: "Friday" },
		{ Index: 6, Name: "Saturday" },
	];
	formError = "";
	groupError = "";
	errorSound = "";
	errorSpeaker = "";
	errorLed = "";
	errorSoundFile = "";
	isShowFileSelect = true;
	// listLines = [];
	listDevices = [];
	async beforeMount() {
		await this.getListGroup();
		await this.getListData();
		// await this.getListController();
		// this.createColumnHeader();
		// this.initForm();
		this.initRule();

		// await this.getListLines();
		// const res: any = await Promise.all([
		// 	this.getListDevices(),
		// 	this.getListGateDevices()
		// ]);
		// this.listDevices = this.listDevices.concat(...res);
		this.getListDevices();
		// console.log(this.listDevices)
	}
	async mounted() {
		
	}

	initForm() {
		(this.form as WarningRulesModel) = {
			Index: 0,
			UseSpeaker: false,
			UseLed: false,
			UseEmail: false,
			UseComputerSound: false,
			UseChangeColor: false,

			SpeakerChannel: null,
			SpeakerController: null,
			SpeakerDescription: '',

			LedChannel: null,
			LedController: null,
			LedDescription: '',

			Email: '',
			EmailSendType: 0,

			ComputerSoundPath: '',

			RulesWarningGroupIndex: null
		};
		this.Attachments = [];
		this.listEmailScheduler = [
			{ Index: 0, Time: new Date(2021, 1, 1, 8, 0), DayOfWeekIndex: 1, Order: 0, Error: "" }
		];
		this.listSpeakerWarningFocus = [
			{ Index: 0, ControllerIndex: null, ChannelIndex: null, Order: 0, Error: "", Type: 0, LineIndex: null, GateIndex: null, SerialNumber: "" }
		];
		this.listSpeakerWarningInPlace = [
			{ Index: 0, ControllerIndex: null, ChannelIndex: null, Order: 0, Error: "", Type: 0, LineIndex: null, GateIndex: null, SerialNumber: "" }
		];
		this.listLedWarningFocus = [
			{ Index: 0, ControllerIndex: null, ChannelIndex: null, Order: 0, Error: "", Type: 1, LineIndex: null, GateIndex: null, SerialNumber: "" }
		];
		this.listLedWarningInPlace = [
			{ Index: 0, ControllerIndex: null, ChannelIndex: null, Order: 0, Error: "", Type: 1, LineIndex: null, GateIndex: null, SerialNumber: "" }
		];
	}
	initRule() {
		this.rule = {
			// RulesWarningGroupIndex: [
			// 	{
			// 		required: true,
			// 		message: this.$t('PleaseInputWarningGroup'),
			// 		trigger: 'change',
			// 	},
			// ],
			// SpeakerController: [
			// 	{
			// 		required: true,
			// 		message: this.$t('PleaseInputController'),
			// 		trigger: 'change',
			// 	},
			// ],
			// SpeakerChannel: [
			// 	{
			// 		required: true,
			// 		message: this.$t('PleaseInputChannel'),
			// 		trigger: 'change',
			// 	},
			// ],
			// LedController: [
			// 	{
			// 		required: true,
			// 		message: this.$t('PleaseInputController'),
			// 		trigger: 'change',
			// 	},
			// ],
			// LedChannel: [
			// 	{
			// 		required: true,
			// 		message: this.$t('PleaseInputChannel'),
			// 		trigger: 'change',
			// 	},
			// ],
			ControllerIndex: [
				{
					required: true,
					message: this.$t('PleaseInputController'),
					trigger: 'change',
				},
			],
			Email: [
				{
					required: true,
					message: this.$t('PleaseInputEmail'),
					trigger: 'change',
				},
				{
					type: "email",
					message: this.$t('InvalidEmail'),
					trigger: 'change',
				}
			],
			EmailSendType: [
				{
					required: true,
					message: this.$t('PleaseInputEmailSendType'),
					trigger: 'change',
				}
			]
		};
	}

	// async getListControllerSelect() {
	// 	await lines_RelayController.GetAllController().then((res: any) => {
	// 		this.listControllerSelect = res.data.Data;
	// 		// console.log(this.listControllerSelect);
	// 	});
	// }
	// async getListLines() {
	// 	await lineApi.GetAllLineBasic().then((res: any) => {
	// 		this.listLines = res.data.Data;
	// 		// this.listLinesDefault = res.data.Data;
	// 	});
	// }
	// async getListDevices() {
	// 	const res: any = await lineApi.GetAllDevicesLines();
	// 	if (res.status == 200 && res.data.Status == "success") {
	// 		return res.data.Data;
	// 	}
	// 	return [];
	// }
	async getListDevices() {
		const res: any = await lineApi.GetAllDevicesLines();
		// console.log(res)
		if (res.status == 200) {
			this.listDevices = res.data;
		}
	}
	// async getListGateDevices() {
	// 	const res: any = await accesscontrol_deviceApi.GetAllDevices();
	// 	if (res.status == 200 && res.data.Status == "success") {
	// 		return res.data.Data;
	// 	}
	// 	return [];
	// }


	checkClass(index) {
		if (this.currentGroup == index) {
			return 'is-focus';
		} else {
			return 'un-focus';
		}
	}
	// loadChannel(controllerIndex, type, noDel: boolean) {
	// 	const controller = this.listControllerSelect.find(e => e.Index == controllerIndex);
	// 	if (type == "speaker") {
	// 		if (!noDel) {
	// 			this.form.SpeakerChannel = null;
	// 		}
	// 		this.listChannelSelectSpeaker = controller.ListChannel;
	// 		// console.log("this.listChannelSelectSpeaker", this.listChannelSelectSpeaker);
	// 	} else {
	// 		if (!noDel) {
	// 			this.form.LedChannel = null;
	// 		}
	// 		this.listChannelSelectLed = controller.ListChannel;
	// 	}
	// }
	changeFormData(index) {
		this.initForm();
		this.listData.forEach(element => {
			if (element.RulesWarningGroupIndex == index) {
				this.getAllListControllerChannels(element.Index);
				this.isAdd = false;
				this.activeMenuIndex = element.Index;
				this.currentGroupName = element.GroupName;
				this.currentGroup = index;
				this.form = element;
				// if (element.UseSpeaker) {
				// 	this.loadChannel(this.form.SpeakerController, "speaker", true);
				// }
				// if (element.UseLed) {
				// 	this.loadChannel(this.form.LedController, "led", true);
				// }
				if (element.UseComputerSound) {
					const ezFile: EzFile[] = JSON.parse(element.ComputerSoundPath);
					// console.log("ezFile", ezFile, element.ComputerSoundPath);
					this.Attachments = ezFile;
				}
				this.getListEmailScheduler(element.Index);
			}
		});
	}
	// async getListController() {
	// 	await lines_RelayController.GetAllController().then((res: any) => {
	// 		// console.log("getListController", res);
	// 		this.listControllerSelect = res.data.Data;
	// 		// console.log("listData", this.listData);
	// 	});
	// }
	async getListData() {
		await rulesWarningApi.GetRulesWarningByCompanyIndex().then((res: any) => {
			// console.log("GetRulesWarningByCompanyIndex", res);
			const arrTemp = [];
			res.data.forEach((item) => {
				const warning = this.listGroup[item.RulesWarningGroupIndex] || {};
				const a = Object.assign(item, {
					GroupName: warning.Name,
				});
				arrTemp.push(a);
				this.removeItemInListGroup(warning.Index);
			});
			this.listData = arrTemp;
			// console.log("listData", this.listData);
		});
	}
	async getAllListControllerChannels(rulesWarningIndex) {
		await rulesWarningApi.GetControllerChannelByRuleWarningIndex(rulesWarningIndex).then((res: any) => {
			// console.log("GetControllerChannelByRuleWarningIndex", res);
			const list: Array<ControllerWarningRequestModel> = res.data;
			this.listSpeakerWarningFocus = [];
			this.listSpeakerWarningInPlace = [];
			this.listLedWarningFocus = [];
			this.listLedWarningInPlace = [];

			list.forEach((element, index) => {

				// console.log("list.forEach", element.Type, element.LineIndex);
				if (element.Type == 0) { //Speaker
					if (element.LineIndex == null && element.GateIndex == null) { // Focus
						const order = (this.listSpeakerWarningFocus.length);
						const item = this.getObjectControllerChannel(element, order);
						this.listSpeakerWarningFocus.push(item);
					} else { //InPlace
						const order = (this.listSpeakerWarningInPlace.length);
						const item = this.getObjectControllerChannel(element, order);
						this.listSpeakerWarningInPlace.push(item);
					}
				} else if (element.Type == 1) { //Led
					if (element.LineIndex == null && element.GateIndex == null) {
						const order = (this.listLedWarningFocus.length);
						const item = this.getObjectControllerChannel(element, order);
						this.listLedWarningFocus.push(item);
					} else {
						const order = (this.listLedWarningInPlace.length);
						const item = this.getObjectControllerChannel(element, order);
						this.listLedWarningInPlace.push(item);
					}
				}
			});
			// console.log("GetControllerChannel >>", this.listSpeakerWarningFocus, this.listLedWarningInPlace);
		});
	}
	async getListEmailScheduler(rulesWarningIndex) {
		if (rulesWarningIndex == 0) {
			this.listEmailScheduler = [
				{ Index: 0, Time: new Date(2021, 1, 1, 8, 0), DayOfWeekIndex: 1, Order: 0, Error: "" }
			];
			// console.log("rulesWarningIndex == 0", this.listEmailScheduler);
		} else {
			await rulesWarningApi.GetEmailScheduleByRuleWarningIndex(rulesWarningIndex).then((res: any) => {
				// console.log("GetRulesWarningEmailSchedule", res);
				const list = res.data;
				if (list && list.length == 0) {
					this.listEmailScheduler = [
						{ Index: 0, Time: new Date(2021, 1, 1, 8, 0), DayOfWeekIndex: 1, Order: 0, Error: "" }
					];
				} else {
					this.listEmailScheduler = [];
					list.forEach((element, index) => {
						this.listEmailScheduler.push({
							Index: element.Index,
							Error: "",
							Order: index,
							DayOfWeekIndex: element.DayOfWeekIndex,
							Time: new Date(2021, 1, 1, element.Time.split(':')[0], element.Time.split(':')[1], element.Time.split(':')[2])
						})
					});
				}
				// console.log("listEmailSchedule", this.listEmailScheduler);
			});
		}

	}
	async getListGroup() {
		await rulesWarningApi.GetRulesWarningGroup().then((res: any) => {
			// console.log("GetRulesWarningGroup", res);
			const data = res.data;
			this.listGroupSelect = data;
			const dictData = {};
			data.forEach((e: any) => {
				dictData[e.Index] = {
					Index: e.Index,
					Name: e.Name
				};
			});
			this.listGroup = dictData;
			// console.log("listGroup", this.listGroup);
		});
	}
	getObjectControllerChannel(item, order) {
		const isGate = item.GateIndex != null;
		return {
			Index: item.Index,
			Error: "",
			Order: order,
			ControllerIndex: item.ControllerIndex,
			ChannelIndex: item.ChannelIndex,
			// LineIndex: isGate ? `gate#${item.GateIndex}` : item.LineIndex,
			LineIndex: item.LineIndex,
			GateIndex: item.GateIndex, 
			SerialNumber: item.SerialNumber,
			Type: item.Type
		};
	}
	removeItemInListGroup(groupIndex) {

		const group = this.listGroupSelect.find(e => e.Index == groupIndex);
		const index = this.listGroupSelect.indexOf(group, 0);
		if (index > -1) {
			this.listGroupSelect.splice(index, 1);
		}
	}
	getWarningName(groupIndex: number) {
		// console.log("getWarningName", groupIndex);
		const warning = this.listGroup[groupIndex] || {};
		return warning.Name;
	}
	handleRemove(file, fileList) {
		this.form.ComputerSoundPath = "";
		// console.log(file, fileList);
	}
	handlePreview(file) {
		// console.log(file);
	}
	handleExceed(files, fileList) {
		this.$message.warning(`The limit is 3, you selected ${files.length} files this time, add up to ${files.length + fileList.length} totally`);
	}
	beforeRemove(file, fileList) {
		return this.$confirm(this.$t('CancelTheTransfertOf') + file.name + '?');
	}

	/////// this.$refs.upload.submit();
	fileUploadChange(file, fileList) {
		if (this.fileList.length > 0) {
			this.fileList = fileList.slice(1);
		}
		const reader = new FileReader();
		reader.readAsDataURL(file.raw);

		reader.onload = () => {
			// this.imageNRICBackUrl = reader.result.toString();
			this.form.ComputerSoundPath = reader.result.toString();
			this.errorSound = "";
			// console.log("this.form.ComputerSoundPath", this.form.ComputerSoundPath);
		};
	}
	async submit() {
		(this.$refs.formref as any).validate(async (valid) => {
			// console.log("this.form", this.form);
			if (this.currentGroup == null || this.currentGroup == 0) {
				this.groupError = this.$t("PleaseInputWarningGroup").toString();
				return;
			} else if (!this.form.UseEmail && !this.form.UseSpeaker
				&& !this.form.UseLed && !this.form.UseComputerSound && !this.form.UseChangeColor) {
				this.formError = this.$t("PleaseChooseAtLeastOneWarningEvent").toString();
				return;
			} else if (!valid) {
				return;
			} else if (this.form.UseSpeaker && (!this.form.UseSpeakerFocus && !this.form.UseSpeakerInPlace)) {
				this.errorSpeaker = this.$t("TypeWaringRequired").toString();
				return;
			} else if (this.form.UseLed && (!this.form.UseLedFocus && !this.form.UseLedInPlace)) {
				this.errorLed = this.$t("TypeWaringRequired").toString();
				return;
			} else if (this.form.UseComputerSound && (this.Attachments.length == 0)) {
				this.errorSound = this.$t("FileSoundNotFound").toString();
				return;
			}
			else if (this.validateControllerChannel()) {
				return;
			}
			else {
				if (this.form.UseEmail && this.form.EmailSendType == 1) {
					const schedule = this.listEmailScheduler.find(e => e.Error != "");
					if (schedule != null) {
						return;
					}
				}
				this.form.UseLed = this.form.UseSpeaker;
				this.form.UseLedFocus = this.form.UseSpeakerFocus;
				this.form.UseLedInPlace = this.form.UseSpeakerInPlace;
				if (this.isAdd) {
					// console.log("this.form", this.form);
					this.form.RulesWarningGroupIndex = this.currentGroup;
					this.form.CompanyIndex = 0;
					await rulesWarningApi.AddRulesWarning(this.form).then((res: any) => {
						// console.log("res", res);
						if (res.status == 200) {
							this.$alert(this.$t('AddSuccess').toString(), this.$t('Notify').toString());

							this.currentGroup = null;
							const indexCallBack = res.data;

							if (this.form.UseEmail && this.form.EmailSendType == 1) {
								const list: Array<EmailScheduleRequestModel> = [];
								this.listEmailScheduler.forEach(element => {
									const item: EmailScheduleRequestModel = {
										Time: new Date(element.Time.getTime() - (new Date().getTimezoneOffset() * 60000)),
										DayOfWeekIndex: element.DayOfWeekIndex,
										RulesWarningIndex: indexCallBack,
										CompanyIndex: 0,
										Index: 0
									}
									list.push(item);
								});

								// console.log("Schedule add", list);
								rulesWarningApi.AddRulesWarningEmailSchedule(list).then((res: any) => {
									if (res.status == 200) {
										// console.log("Schedule add success");
										this.listEmailScheduler = [
											{ Index: 0, Time: new Date(2021, 1, 1, 8, 0), DayOfWeekIndex: 1, Order: 0, Error: "" }
										];
										this.getListData().then(() => {
											rulesWarningApi.SendReloadWarningRulesSignal().catch((err) => {
												console.log(err)
											});
										});
									}
								});
							}
							if (this.form.UseComputerSound && this.Attachments.length > 0) {
								const ezFileRequest: EzFileRequest = {
									Index: indexCallBack,
									Attachments: this.Attachments
								}
								rulesWarningApi.AddEzFileRulesWarning(ezFileRequest).then((res: any) => {
									if (res.status == 200) {
										// console.log("Upload file success");
										this.listEmailScheduler = [
											{ Index: 0, Time: new Date(2021, 1, 1, 8, 0), DayOfWeekIndex: 1, Order: 0, Error: "" }
										];
										this.getListData().then(() => {
											rulesWarningApi.SendReloadWarningRulesSignal().catch((err) => {
												console.log(err)
											});
										});
									}
								});
							}
							if (this.form.UseLed || this.form.UseSpeaker) {
								this.addControllerChannels(indexCallBack);
							}

							// this.changeFormData(this.currentGroup);
							this.initForm();

						}
					});

					await this.getListData().then(() => {
						rulesWarningApi.SendReloadWarningRulesSignal().catch((err) => {
							console.log(err)
						});
					});
				} else {
					// // console.log(this.Attachments.length);
					if (this.Attachments.length == 0 || !this.form.UseComputerSound) {
						this.form.ComputerSoundPath = null;
					}
					await rulesWarningApi.UpdateRulesWarning(this.form).then((res: any) => {
						if (res.status == 200) {
							this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());


							const indexCallBack = res.data;

							if (this.form.UseEmail && this.form.EmailSendType == 1) {
								const list: Array<EmailScheduleRequestModel> = [];
								this.listEmailScheduler.forEach(element => {
									const item: EmailScheduleRequestModel = {
										Time: new Date(element.Time.getTime() - (new Date().getTimezoneOffset() * 60000)),
										DayOfWeekIndex: element.DayOfWeekIndex,
										RulesWarningIndex: indexCallBack,
										CompanyIndex: 0,
										Index: 0
									}
									list.push(item);
								});

								// console.log("Schedule add", list);
								rulesWarningApi.AddRulesWarningEmailSchedule(list).then((res: any) => {
									if (res.status == 200) {
										// console.log("Schedule add success");
										this.listEmailScheduler = [
											{ Index: 0, Time: new Date(2021, 1, 1, 8, 0), DayOfWeekIndex: 1, Order: 0, Error: "" }
										];
										this.getListData().then(() => {
											rulesWarningApi.SendReloadWarningRulesSignal().catch((err) => {
												console.log(err)
											});
										});
									}
								});
							}
							// console.log("this.Attachments", this.Attachments);
							if (this.form.UseComputerSound && this.Attachments.length > 0) {
								const ezFileRequest: EzFileRequest = {
									Index: indexCallBack,
									Attachments: this.Attachments
								}
								rulesWarningApi.AddEzFileRulesWarning(ezFileRequest).then((res: any) => {
									if (res.status == 200) {
										// console.log("Upload file success");
										this.listEmailScheduler = [
											{ Index: 0, Time: new Date(2021, 1, 1, 8, 0), DayOfWeekIndex: 1, Order: 0, Error: "" }
										];
										this.getListData().then(() => {
											rulesWarningApi.SendReloadWarningRulesSignal().catch((err) => {
												console.log(err)
											});
										});
									}
								});
							}

							if (this.form.UseLed || this.form.UseSpeaker) {
								this.addControllerChannels(indexCallBack);
							}
							this.initForm();
							this.currentGroup = null;
							this.isAdd = true;
						}
					}).finally(() => {
						this.getListData().then(() => {
							rulesWarningApi.SendReloadWarningRulesSignal().catch((err) => {
								console.log(err)
							});
						});
					});
				}
			}
		});
	}
	validateControllerChannel() {
		let hasError = false;
		if (this.form.UseSpeaker && this.form.UseSpeakerFocus
			&& (this.listSpeakerWarningFocus.length < 1 || this.checkListIsDefault(this.listSpeakerWarningFocus, false))) {
			// console.log("1");
			hasError = true;
		}
		if (this.form.UseSpeaker && this.form.UseSpeakerInPlace
			&& (this.listSpeakerWarningInPlace.length < 1 || this.checkListIsDefault(this.listSpeakerWarningInPlace, true))) {
			// console.log("2");
			hasError = true;
		}
		if (this.form.UseLed && this.form.UseLedFocus
			&& (this.listLedWarningFocus.length < 1 || this.checkListIsDefault(this.listLedWarningFocus, false))) {
			// console.log("3");
			hasError = true;
		}
		if (this.form.UseLed && this.form.UseLedInPlace
			&& (this.listLedWarningInPlace.length < 1 || this.checkListIsDefault(this.listLedWarningInPlace, true))) {
			// console.log("4");
			hasError = true;
		}

		return hasError;
	}
	checkListIsDefault(list: Array<ControllerWarningModel>, useLine: boolean) {
		let hasError = false;
		list.forEach(element => {
			element.Error = "";
			if (useLine) {
				if (element.ControllerIndex == null || element.ChannelIndex == null || element.LineIndex == null || element.SerialNumber == "") {
					element.Error = this.$t("AllDataRequired").toString();
				}
			} else {
				if (element.ControllerIndex == null && element.ChannelIndex == null) {
					element.Error = this.$t("ControllerChannelRequired").toString();
				} else if (element.ControllerIndex == null) {
					element.Error = this.$t("ControllerRequired").toString();
				} else if (element.ChannelIndex == null) {
					element.Error = this.$t("ChannelRequired").toString();
				}
			}
			if (element.Error != "") {
				// console.log("error");
				hasError = true;
			}

		});
		return hasError;
	}
	IsNullOrEmpty(data: Array<any>) {
		return (data == null || data.length < 1);
	}
	addControllerChannels(indexCallBack) {
		let listController: Array<ControllerWarningModel> = [];
		if (this.form.UseSpeaker && this.form.UseSpeakerFocus) {
			listController = listController.concat(this.listSpeakerWarningFocus);

			const listLedFocus = Misc.cloneData(this.listSpeakerWarningFocus);
			listLedFocus.forEach(element => {
				element.Type = 1;
			});
			listController = listController.concat(listLedFocus);
		}
		if (this.form.UseSpeaker && this.form.UseSpeakerInPlace) {
			listController = listController.concat(this.listSpeakerWarningInPlace);

			const listLedInPlace = Misc.cloneData(this.listSpeakerWarningInPlace);
			listLedInPlace.forEach(element => {
				element.Type = 1;
			});
			console.log(listLedInPlace)
			listController = listController.concat(listLedInPlace);
		}
		console.log(listController)
		// if (this.form.UseLed && this.form.UseLedFocus) {
		// 	listController = listController.concat(this.listLedWarningFocus);
		// }
		// if (this.form.UseLed && this.form.UseLedInPlace) {
		// 	listController = listController.concat(this.listLedWarningInPlace);
		// }
		const listRequest: Array<ControllerWarningRequestModel> = [];
		listController.forEach(element => {			
			let lineIndex = element.LineIndex;
			let isGate = false;
			if(lineIndex) {
				const gateIndex = split(lineIndex.toString(), '#', 2);
				if(gateIndex.length > 1) {
					lineIndex = parseInt(gateIndex[1]);
					isGate = true;
				}
			}
			const item: ControllerWarningRequestModel = {
				LineIndex: lineIndex ? (isGate ? null : element.LineIndex) : null,
				GateIndex: lineIndex ? (isGate ? lineIndex : null) : null,
				SerialNumber: element.SerialNumber,
				Type: element.Type,
				ControllerIndex: element.ControllerIndex,
				ChannelIndex: element.ChannelIndex,
				RulesWarningIndex: indexCallBack,
				CompanyIndex: 0,
				Index: 0
			}
			this.serialNumber = element.SerialNumber;
			listRequest.push(item);
		});


		rulesWarningApi.AddRulesWarningControllerChannels(listRequest).then((res: any) => {
			if (res.status == 200) {
				// console.log("ControllerChannel add success");
				this.getListData().then(() => {
					rulesWarningApi.SendReloadWarningRulesSignal().catch((err) => {
						console.log(err)
					});
				});
			}
		});
	}
	async del() {
		if (this.activeMenuIndex < 1) return;

		this.$confirm(this.$t("AreYouSureWantToDelete?").toString(), this.$t("Warning").toString(), {
			confirmButtonText: this.$t("Delete").toString(),
			cancelButtonText: this.$t("MSG_No").toString(),
			type: 'warning'
		}).then(() => {
			rulesWarningApi.DeleteRulesWarning(this.activeMenuIndex).then((res: any) => {
				if (res.status == 200) {
					this.$alert(this.$t('DeleteSuccess').toString(), this.$t('Notify').toString());

					this.initForm();
					this.getListGroup();
					this.getListData().then(() => {
						rulesWarningApi.SendReloadWarningRulesSignal().catch((err) => {
							console.log(err)
						});
					});
					this.currentGroup = null;
					this.isAdd = true;
				}
			});
		}).catch(() => {
		});

	}
	add() {
		this.initForm();
		this.isAdd = true;
		this.currentGroup = null;
		this.currentGroupName = "";
	}
	get getHiddenEdit() {
		if (this.rowsObj.length === 1) {
			return false;
		} else return true;
	}
	get getHiddenDelete() {
		if (this.rowsObj.length < 1) {
			return true;
		} else return false;
	}

	CheckDuplicateSchedule(row) {
		this.listEmailScheduler.forEach(element => {
			element.Error = "";
		});
		this.listEmailScheduler.forEach(element => {
			const same = element.Time.getHours() === row.Time.getHours() && element.Time.getMinutes() === row.Time.getMinutes();
			// console.log("start for", element, element.Time.getHours(), element.Time.getMinutes(), row.Time.getTimeSpan());
			if (same && element.Index != row.Index && element.DayOfWeekIndex == row.DayOfWeekIndex) {
				row.Error = this.$t("DuplicateSchedule");
				// console.log("duplicate", element);
			}
		});
		// console.log("end", row);
	}
	CheckAllDuplicateSchedule() {
		this.listEmailScheduler.forEach(element => {
			if (element.Error != "") {
				const schedule = this.listEmailScheduler.find(row => element.Index != row.Index
					&& element.Time.getHours() === row.Time.getHours()
					&& element.Time.getMinutes() === row.Time.getMinutes()
					&& element.DayOfWeekIndex == row.DayOfWeekIndex);
				if (schedule == null) {
					element.Error = "";
				}
			}
		});
	}
	deleteScheduleSendMail(index, row) {
		if (this.listEmailScheduler.length <= 1) return;

		const arrTemp = this.listEmailScheduler.filter((item) => {
			return item.Index != row.Index;
		});

		this.listEmailScheduler = arrTemp;
		this.UpdateIndexScheduleSendMail();
		this.CheckAllDuplicateSchedule();
	}
	AddEmptyRowInScheduleSendMail(index) {
		this.listEmailScheduler.unshift({
			Index: index + 2,
			Time: null,
			DayOfWeekIndex: 1,
			Order: index + 2,
			Error: ""
		});

	}
	UpdateIndexScheduleSendMail() {
		for (let index = 0; index < this.listEmailScheduler.length - 1; index++) {
			this.listEmailScheduler[index].Index = (index + 1);
		}
	}
	addScheduleSendMail(index, row) {
		// console.log("tet", index);
		if (index == 0) {
			this.AddEmptyRowInScheduleSendMail(index);
			this.UpdateIndexScheduleSendMail();
		}
	}
	@Watch('currentGroup', { deep: true }) hande(val) {
		if (this.currentGroup != null && this.currentGroup != 0) {
			this.groupError = "";
		}

		// console.log('val :>> ', val);
	}
	@Watch('form.UseSpeaker', { deep: true }) handeUseSpeaker(val) {
		if (this.form.UseSpeaker) {
			this.formError = "";
		} else {
			this.errorSpeaker = "";
		}
	}
	@Watch('form.UseSpeakerFocus', { deep: true }) handeUseSpeakerFocus(val) {
		if (this.form.UseSpeakerFocus) {
			this.errorSpeaker = "";
		} else {
			if (this.form.UseSpeakerInPlace) {
				this.errorSpeaker = "";
			} else {
				if (this.form.UseSpeaker) {
					this.errorSpeaker = this.$t("TypeWaringRequired").toString();
				}
			}
		}
	}
	@Watch('form.UseSpeakerInPlace', { deep: true }) handeUseSpeakerInPlace(val) {
		if (this.form.UseSpeakerInPlace) {
			this.errorSpeaker = "";
		} else {
			if (this.form.UseSpeakerFocus) {
				this.errorSpeaker = "";
			} else {
				if (this.form.UseSpeaker) {
					this.errorSpeaker = this.$t("TypeWaringRequired").toString();
				}
			}
		}
	}

	@Watch('form.UseLed', { deep: true }) handeUseLed(val) {
		if (this.form.UseLed) {
			this.formError = "";
		} else {
			this.errorSpeaker = "";
		}
	}
	@Watch('form.UseLedFocus', { deep: true }) handeUseLedFocus(val) {
		if (this.form.UseLedFocus) {
			this.errorLed = "";
		} else {
			if (this.form.UseLedInPlace) {
				this.errorLed = "";
			} else {
				if (this.form.UseLed) {
					this.errorLed = this.$t("TypeWaringRequired").toString();
				}
			}
		}
	}
	@Watch('form.UseLedInPlace', { deep: true }) handeUseLedInPlace(val) {
		if (this.form.UseLedInPlace) {
			this.errorLed = "";
		} else {
			if (this.form.UseLedFocus) {
				this.errorLed = "";
			} else {
				if (this.form.UseLed) {
					this.errorLed = this.$t("TypeWaringRequired").toString();
				}
			}
		}
	}
	@Watch('form.UseEmail', { deep: true }) handeUseEmail(val) {
		if (this.form.UseEmail) {
			this.formError = "";
		}
	}
	@Watch('form.UseComputerSound', { deep: true }) handeUseComputerSound(val) {
		if (this.form.UseComputerSound) {
			this.formError = "";
			// console.log(this.Attachments)
			if(this.Attachments && this.Attachments.length == 0){
				this.errorSoundFile = this.$t("PleaseSelectSoundFile").toString();
			}
		}
		this.isShowFileSelect = false;
		setTimeout(() => {
			this.isShowFileSelect = true;
		}, 50);
	}
	@Watch('Attachments', { deep: true }) handeAttachments(val) {
		if (this.form.UseComputerSound && this.Attachments && this.Attachments.length == 0){
			this.errorSoundFile = this.$t("PleaseSelectSoundFile").toString();
		}else{
			this.errorSoundFile = '';
		}
		this.isShowFileSelect = false;
		setTimeout(() => {
			this.isShowFileSelect = true;
		}, 50);
	}
	@Watch('form.UseChangeColor', { deep: true }) handeUseChangeColor(val) {
		if (this.form.UseChangeColor) {
			this.formError = "";
		}
	}
	handleModel(value) {
		// console.log(value)
		this.Attachments = value;
	}
}
