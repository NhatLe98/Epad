import { Component, Vue, Prop, Watch } from 'vue-property-decorator';
import { isNullOrUndefined } from 'util';
@Component({
	name: 'app-function-bar-component',
	components: {},
})
export default class AppFunctionBarComponent extends Vue {
    @Prop({ default: () => [] }) gridColumnConfig: Array<any>;
    
    @Prop({ default: () => true }) showDelete: boolean;
	@Prop({ default: () => false }) showSave: boolean;
	@Prop({ default: () => true }) showEdit: boolean;
	@Prop({ default: () => true }) showMore: boolean;
	@Prop({ default: () => true }) showAdd: boolean;

	//RoleDistribute Button
	@Prop({ default: () => false }) isAdd: boolean;
	@Prop({ default: () => false }) isUpdate: boolean;
	@Prop({ default: () => false }) isDelete: boolean;
	@Prop({ default: () => false }) isExport: boolean;
	@Prop({ default: () => false }) isImport: boolean;
    @Prop({ default: () => false }) isRead: boolean;
	@Prop({ default: () => false }) IsMonitoring: boolean;
	@Prop({ default: '' }) importPath: string;
	isSearching = false;
	txtSearch = '';
	showConfigGridPanel = false;
	listColumn: any = [];

	closeSearch() {
		if (this.txtSearch === '') {
			this.isSearching = false;
        }
	}

	beforeMount() {
		//console.log(this.isRead);
		const path = this.$route.path.substr(1);
		const key = `${path}-config-column`;
		const configColumnByPath = localStorage.getItem(key);
		
		let listCol = [];
		if(!Misc.isEmpty(configColumnByPath)){
			listCol = JSON.parse(configColumnByPath);
		}

		if (listCol.length === 0) {
			listCol = this.gridColumnConfig.map(col => {
				return {
					ID: col.ID || col.name,
					ColumnName: col.ColumnName || this.$t(col.name),
					Show: true,
					Fixed: col.Fixed || false
				}
			});
			localStorage.setItem(key, JSON.stringify(listCol));
		}

		this.listColumn = listCol;

		// nạp lại config từ localStorage lên UI
		const arrTemp = this.gridColumnConfig.map((col, index) => {
			return {
				...col,
				Show: this.listColumn[index].Show
			};
		});
		this.$emit('update:gridColumnConfig', arrTemp);
	}

	addClick() {
		this.$emit('openDialog', 'add');
    }
    
	saveClick() {
		this.$emit('saveHandler');
    }
    
	editClick() {
		this.$emit('openDialog', 'edit');
    }
    
	deleteClick() {
		this.$confirmDelete(this.$t('MSG_ConfirmDeleteMulti').toString(), this.$t('Confirm').toString(), null)
			.then(() => {
				this.$emit('deleteHandler');		
			})
			.catch(() => {
				return;
			});
	}

	openCloseGridConfigPanel() {
		this.showConfigGridPanel = !this.showConfigGridPanel;
	}

	saveListCol() {
		const arrTemp = this.gridColumnConfig.map((col, index) => {
			return {
				...col,
				Show: this.listColumn[index].Show
			};
		});
		this.$emit('update:gridColumnConfig', arrTemp);
		const path = this.$route.path.substr(1);
		const key = `${path}-config-column`;
		localStorage.setItem(key, JSON.stringify(this.listColumn));

		setTimeout(() => {
			this.showConfigGridPanel = false;
		}, 200);
	}

	get getSrcForGear() {
		return this.showConfigGridPanel === false ? '@/assets/icons/function-bar/gear.svg' : '@/assets/icons/function-bar/cross.svg';
	}

	moreActionClick(command){
		switch(command){
			case 'Import': this.$router.push(this.importPath); break;
			default: break;
		}
	}
}
