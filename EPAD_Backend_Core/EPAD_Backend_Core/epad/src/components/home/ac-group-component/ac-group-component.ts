import { Component, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { AC_AreaDTO, areaApi } from '@/$api/ac-area-api';
import { timezoneApi } from '@/$api/ac-timezone-api';
import { groupApi } from '@/$api/ac-group-api';
import { doorApi } from '@/$api/ac-door-api';
import { commandApi, CommandRequest } from "@/$api/command-api";

@Component({
	name: 'group',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class GroupComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];
	ruleForm: any = {};
	rules: any = {};
	timeZoneLst :any[] = [];
	allDoorLst = [];

	// Function ALL TAB ==============================
	commandRequest: CommandRequest = {
		Action: "",
		ListSerial: [],
		ListUser: [],
		FromTime: null,
		ToTime: null,
		Privilege: 0,
		AuthenMode: [],
		IsOverwriteData: false,
		EmployeeType: 0,
		IsUsingTimeZone: false,
		TimeZone: [],
		Group: 0
	};

	beforeMount() {
		this.initColumns();
		this.initRule();
		this.getAllTimezone();
		this.getAllDoor();
	}
	initRule() {
		this.rules = {
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputName'),
					trigger: 'blur',
				},
			],
			Timezone:[
				{
					required: true,
					message: this.$t('PleaseChooseTimeZone'),
					trigger: 'blur',
				},
			],
			DoorIndex:[
				{
					required: true,
					message: this.$t('PleaseChooseDoor'),
					trigger: 'blur',
				},
			]
		};
	}

	mounted() {
		
	}

	initColumns(){
		this.columns = [
			{
				prop: 'Name',
				label: 'GroupName',
				minWidth: 100,
				display: true
			},
			{
				prop: 'DoorName',
				label: 'Door',
				minWidth: 100,
				display: true
			},
			{
				prop: 'TimezoneName',
				label: 'Timezone',
				minWidth: 300,
				display: true
			}
			
		];
	}

	displayPopupInsert() {
		this.showDialog = false;
	}

	async getAllTimezone(){
		return await timezoneApi.GetAllTimezone().then((res) => {
            const { data } = res as any;
            this.timeZoneLst = data;
        })
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await groupApi.GetGroupAtPage(page, filter, pageSize).then((res) => {
			const { data } = res as any;
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	async getAllDoor() {
		doorApi.GetAllDoor().then(res => {
			const { data } = res as any;
			this.allDoorLst = data;
		})
	}

	reset() {
	
		this.ruleForm = {};
	}

	Insert() {
		this.showDialog = true;
		this.isEdit = false;
		this.reset();
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				if (this.isEdit == true) {
					return await groupApi.UpdateGroup(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
					
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
							this.InsertToMachine(this.ruleForm.UID);
							this.reset();
						}
					});
				} else {
					return await groupApi
						.AddGroup(this.ruleForm)
						.then((res) => {
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							this.reset();
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
							}
						})
						.catch(() => {});
				}
			}
		});
	}

	async InsertToMachine(group) {
		let message = '';
		const self = this;

		this.commandRequest.Action = "Upload ac user";

		this.commandRequest.ListUser = [];
		this.commandRequest.AreaLst = [];
		this.commandRequest.DoorLst =[];
		this.commandRequest.IsUsingArea =true;
		this.commandRequest.Group = group;
		try {
			await commandApi.UploadACUsersWhenUpdateDoor(this.commandRequest).then((res) => {
				if (!isNullOrUndefined(res.status) && res.status === 200) {
					this.$notify({
						type: 'success',
						title: 'Thông báo từ thiết bị',
						dangerouslyUseHTMLString: true,
						message: self.$t("SendRequestSuccess").toString(),
						customClass: 'notify-content',
						duration: 8000
					});
				}
			}).catch((err) => {
				this.$alertSaveError(null, err);
			});

			return true;
		} catch (error) {
			this.$alertSaveError(null, error);
			return false;
		}
	}

	Edit() {
		this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));

		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length === 1) {
			this.showDialog = true;
			this.ruleForm = obj[0];
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}


	async Delete() {
		const obj: AC_AreaDTO[] = JSON.parse(JSON.stringify(this.rowsObj));
		console.log(obj);
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(async () => {
				await groupApi
					.DeleteGroup(obj)
					.then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$deleteSuccess();
						}
					})
					.catch(() => {});
			});
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	Cancel() {
		var ref = <ElForm>this.$refs.ruleForm;
		ref.resetFields();
		this.showDialog = false;
	}
}
