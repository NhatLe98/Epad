import { Component, Vue, Prop, Watch } from 'vue-property-decorator';

@Component({
	name: 't-grid',
	components: {},
})
export default class TGrid extends Vue {
	@Prop({ default: () => [] }) gridColumns: Array<any>;
	@Prop({ default: () => [] }) dataSource: Array<any>;
	@Prop({ default: () => [] }) selectedRows: Array<any>;
	@Prop({ default: () => 0 }) total: number;
	@Prop({ default: () => 1 }) page: number;
	@Prop({ default: () => 20 }) pageSize: number;
	@Prop({ default: () => false }) hasIndex: boolean;
	@Prop({ default: () => true }) showFooter: boolean;
	@Prop({ default: false }) isShowPageSize: Boolean

	showModalImage = false;
	Image = {};
	ListImages = [];
	clickedData = -1;
	arrKey = [];

	beforeMount() {
		this.gridColumns.forEach(element => {
			if (element.dataType == "imageTooltip" || element.dataType == "mainImageTooltip") {
				this.arrKey.push(element.dataField);
			}
		});
	}

	indexMethod(index) {
		return index + 1;
	}

	getHasSearchClass(value) {
		if (value === true) {
			return 'has-search';
		}
	}

	handleSelectionChange(val) {
		this.$emit('update:selectedRows', val);
		// this.$forceUpdate();
	}

	onPageClick(e) {
		this.page = e;
		this.$emit('update:page', e);
		this.$emit('onPageChange', e);
	}

	onPageSizeChange(e) {
		// this.pageSize = e;
		this.$emit('update:pageSize', e);
		this.$emit('onPageSizeChange', e);
	}

	getLookup(lookup, key) {
		const dummy = lookup.dataSource[key] || {};
		return dummy[lookup.displayMember] || '';
	}

	getDate(format, value) {
		const fmt = this.$i18n.locale === 'vi'
			? (format || 'DD-MM-yyyy').replace('MM-DD', 'DD-MM')
			: (format || 'MM-DD-yyyy').replace('DD-MM', 'MM-DD');

		return value != null ? moment(value).format(fmt) : '';
	}

	getTranslate(value) {
		return this.$t(value);
	}

	showImage(label, src) {
		this.Image = { "label": label, "src": src };
		this.showModalImage = true;
	}

	closeImage() {
		this.Image = { "label": "", "src": "" };
		(this.$refs.openImage as any).close();
	}

	async showListImages(row, column, event) {

		if (row.Id != this.clickedData) {
			this.closeListImage();
			this.arrKey.forEach(element => {
				const obj = {
					label: this.$t(element),
					value: row[element]
				}
				this.ListImages.push(obj);
			});
			this.clickedData = row.Id;
			await this.delay(300);

			const id = this.getIdTooltip(row.Id);
			const elements = document.getElementsByClassName(id);
			if (elements.length > 1) {
				for (let i = 0; i < elements.length; i++) {
					if (i == elements.length - 1) {
						const spreadSheet = elements[i].parentElement;
						spreadSheet.remove();
					}
				}
			}
		} else {

			this.closeListImage();
		}
	}

	async delay(ms: number) {
		return new Promise(resolve => setTimeout(resolve, ms));
	}

	closeListImage() {
		this.ListImages = [];
		this.clickedData = -1;
	}

	checkStatus(row) {
		console.log(row);
		return row.Id == this.clickedData;
	}

	getIdTooltip(Id) {
		return "tooltip_" + Id;
	}

	getIdImage(Id) {
		return "image_" + Id;
	}

	isEmpty(obj) {
		return Misc.isEmpty(obj);
	}

	// @Watch('selectedRows', { deep: true })
	// selectedRowChange(val) {
	// 	this.$emit('selectedRowKeys', val);
	// }
	viewDetailPopup(e) {
		this.$emit('viewDetailPopup', e);
	}
}
