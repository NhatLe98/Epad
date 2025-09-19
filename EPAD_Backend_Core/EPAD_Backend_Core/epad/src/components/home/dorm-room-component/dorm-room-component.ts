import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { dormRoomApi, DormRoomModel } from '@/$api/hr-dorm-room-api';

@Component({
	name: 'dorm-room',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class DormRoomComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	ruleForm: DormRoomModel = {
		Index: 0,
		Code: '',
		Name: '',
		FloorLevelIndex: null,
		Description: '',
	};
	rules: any = {};
	floorLevel = [];
	floorLevelLookup = {};
	floorLevelLookupTemp = {};
	initRule() {
		this.rules = {
			Code: [
				{
					required: true,
					message: this.$t('PleaseInputDormRoomCode'),
					trigger: 'blur',
				}
			],
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputName'),
					trigger: 'blur',
				},
			],
			FloorLevelIndex: [
				{
					required: true,
					message: this.$t('PleaseSelectFloor'),
					trigger: 'blur',
				},
			],
		};
	}

	async beforeMount() {
		this.setColumns();
		this.initRule();
		await this.getFloorLevelData();
	}

	mounted() {
		
	}

	setColumns() {
		this.columns = [
			{
				prop: 'Code',
				label: 'RoomCode',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'Name',
				label: 'RoomName',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'FloorLevel',
				label: 'FloorLevel',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'Description',
				label: 'Description',
				minWidth: '180',
				sortable: true,
				display: true
			},
		];
	}

	async getFloorLevelData() {
		await dormRoomApi.GetAllFloorLevel().then((res: any) => {
			if (res.status == 200) {
				const data = res.data;
				const dictData = {};
				this.floorLevel = data;
				data.forEach((e: any) => {
					dictData[e.Index] = {
						Name: e.Name,
						Code: e.Code,
					};
				});
				this.floorLevelLookup = dictData;
				this.floorLevelLookupTemp = dictData;
			}
		});
	}

	reset() {
		const obj: DormRoomModel = {
			Index: 0,
			Code: '',
			Name: '',
			FloorLevelIndex: null,
			Description: '',
		};
		this.ruleForm = obj;
	}

	searchData()
	{
		this.page = 1;
		(this.$refs.table as any).getTableData(this.page);
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await dormRoomApi.GetDormRoomAtPage(page, pageSize, filter).then((res) => {
			const { data } = res as any;
			data.data.forEach(element => {
				element.FloorLevel = this.floorLevelLookup[element.FloorLevelIndex]?.Name ?? "";
			});
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	Insert() {
		this.reset();
		this.showDialog = true;
		this.isEdit = false;
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				if (this.isEdit == true) {
					dormRoomApi.UpdateDormRoom(this.ruleForm).then((res) => {
						if(res && res.data){
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							this.reset();
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
							}
						}
					});
				} else {
					dormRoomApi.AddDormRoom(this.ruleForm).then((res) => {
						if(res && res.data){
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							this.reset();
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
							}
						}
					});
				}
			}
		});
	}

	Edit() {
		this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));
		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length == 1) {
			this.showDialog = true;
			this.ruleForm = obj[0];
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}

	async Delete() {
		const obj: number[] = JSON.parse(JSON.stringify(this.rowsObj.map(x => x.Index)));
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(() => {
				dormRoomApi
					.DeleteDormRoom(obj)
					.then((res) => {
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$deleteSuccess();
						}
						(this.$refs.table as any).getTableData(this.page, null, null);
					})
					.catch((er) => {
						console.log(er.response.data);
					});
			});
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}
	Cancel() {
		this.reset();
		var ref = <ElForm>this.$refs.ruleForm;
		ref.resetFields();
		this.showDialog = false;
	}
}
