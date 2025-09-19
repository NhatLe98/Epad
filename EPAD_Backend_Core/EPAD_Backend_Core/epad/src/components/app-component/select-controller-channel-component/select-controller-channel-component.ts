import * as mime from 'mime-types';
import { Component, Model, Prop, Vue, Watch } from 'vue-property-decorator';
import { ControllerWarningModel } from '@/$api/gc-rules-warning-api';
import { lineApi, LineModel } from '@/$api/gc-lines-api';
import { relayControllerApi } from '@/$api/relay-controller-api';

const AccessType = {
	Line: 1,
	Gate: 2
}
const IOType = {
	Input: 1,
	Output: 2,
	ESDInput: 4
}

@Component({
	name: 'select-controller-channel-component',
})
export default class SelectControllerChannelComponent extends Vue {
	@Model() model: Array<ControllerWarningModel>;

	@Prop() type: number;
	@Prop() useLine: boolean;
	listData: Array<ControllerWarningModel> = [];
	listControllerSelect: Array<any> = [];
	listLines: Array<any> = [];
	listGates: Array<any> = [];
	@Prop() listDevices: Array<any>;
	// listLinesDefault = [];
	async beforeMount() {
		await Promise.all([
			this.getListController(),
			this.getListLines()
		]).then(() => {
			this.listLines = this.listLines.concat(this.listGates);
		});
	}
	GetKey(text, type, useLine, index, tableIndex) {
		// // console.log("GetKey", text + type + useLine + index + tableIndex, tableIndex);
		return text + type + useLine + index + tableIndex;
	}
	async getListController() {
		await relayControllerApi.GetAllController().then((res: any) => {
			this.listControllerSelect = res.data;
			// console.log(this.listControllerSelect);
			this.listData = this.model;
			this.UpdateListData();
			this.updateModel();
		});
	}
	async getListLines() {
		lineApi.GetAllLineBasic().then((res: any) => {
			this.listLines = res.data;
			this.listLines.forEach(e => {
				e.AccessType = AccessType.Line;
			});
				// this.listLinesDefault = res.data.Data;
		});
	}
	async addChannelToListData() {
		const arrTemp = [];
		this.listData.forEach((item) => {
			const a = Object.assign(item, {
				ListChannel: this.setChannel(item.ControllerIndex),
				ListSerialNumber: this.setDevice(item.LineIndex)
			});
			// console.log("channel", a);
			arrTemp.push(a);
			// console.log(a)
		});
		this.listData = arrTemp;
		// console.log(this.listData)
	}
	AddEmptyRow(index) {
		this.listData.unshift({
			Index: index + 2,
			ControllerIndex: null,
			ChannelIndex: null,
			Order: index + 2,
			Error: "",
			Type: this.type,
			LineIndex: null,
			GateIndex: null,
			SerialNumber: ""
		});
	}
	async loadDeviceByLine(row, noDel: boolean) {
		if (this.listDevices.length < 1) {
			this.delay(200);
		}
		const serialNumbers = this.listDevices.filter(e => e.Index == row.LineIndex);

		// console.log("serialNumbers", serialNumbers, this.listDevices, row);
		if (!noDel) {
			row.SerialNumber = "";
		}
		row.ListSerialNumber = serialNumbers;

		this.updateModel();
	}

	loadChannel(row, noDel: boolean) {
		if (this.listControllerSelect.length < 1) {
			this.delay(200);
		}
		const controller = this.listControllerSelect.find(e => e.Index == row.ControllerIndex);

		// console.log("controller", controller);
		if (!noDel) {
			row.ChannelIndex = null;
		}
		row.ListChannel = controller.ListChannel;

		this.updateModel();
	}
	// loadLines(){
	// 	const arrTemp = this.listLinesDefault.filter((item) => {
	// 		const oldData = this.listData.find(e => e.LineIndex == item.Index);
	// 		return oldData == null;
	// 	});

	// 	// console.log("loadLine", arrTemp);
	// 	this.listLines = arrTemp;
	// }

	// checkLineDisabled(index) {
	// 	const oldData = this.listData.find(e => e.LineIndexs != null && e.LineIndexs.indexOf(index) != -1);
	// 	return oldData != null;
	// }
	checkChannelDisabled(row, index) {
		// if (this.useLine) {
		// 	return false;
		// } else {
		// 	// this.listData.forEach(element => {
		// 	// 	if (element.ControllerIndex == row.ControllerIndex
		// 	// 		&& element.ChannelIndex != null && element.ChannelIndex == index) {
		// 	// 		return true;
		// 	// 	}
		// 	// });
		// 	// return false;
			
		// }
		const isExist = this.listData.find(element => element.ControllerIndex == row.ControllerIndex
			&& element.ChannelIndex != null && element.ChannelIndex == index)
		if (isExist != null) {
			return true;
		} else {
			return false;
		}
	}
	checkControllerDisabled(row, serialNumber) {
		// this.listData.forEach(element => {
		// 	// console.log("checkdis", row, serialNumber, element);
		// 	if (element.LineIndex == row.LineIndex && element.SerialNumber != null && element.SerialNumber == serialNumber) {
		// 		// console.log("dis", element.SerialNumber);
		// 		return true;
		// 	}
		// });

		const isExist = this.listData.find(element => element.LineIndex == row.LineIndex
			&& element.SerialNumber != null
			&& element.SerialNumber == serialNumber);
		if (isExist != null) {
			return true;
		} else {
			return false;
		}

	}
	// checkChannelDisabled(row, useLine) {
	// 	let arrTemp = [];
	// 	this.listData.forEach(element => {
	// 		if (useLine) {
	// 			if (element.ControllerIndex == row.ControllerIndex && element.LineIndexs.indexOf(row.LineIndex) != -1) {
	// 				if (element.ChannelIndexs != null && element.ChannelIndexs.length > 0) {
	// 					arrTemp = arrTemp.concat(element.ChannelIndexs);	
	// 				}
	// 			}
	// 		}else{
	// 			if (element.ControllerIndex == row.ControllerIndex && element.LineIndexs.indexOf(lineIndex) != -1) {
	// 				if (element.ChannelIndexs != null && element.ChannelIndexs.length > 0) {
	// 					arrTemp = arrTemp.concat(element.ChannelIndexs);	
	// 				}
	// 			}
	// 		}

	// 	});
	// 	// console.log("checkChannelDisabled", arrTemp);
	// 	const channel = arrTemp.find(e => e == index);
	// 	return channel != null;
	// }
	setChannel(controllerIndex) {
		// console.log(this.listControllerSelect)
		// console.log(controllerIndex)
		const controller = this.listControllerSelect.find(e => e.Index == controllerIndex);

		// console.log("controller", controller);
		if (controller === undefined) {
			// console.log("null")
			return null;
		}
		return controller.ListChannel;
	}
	setDevice(lineIndex) {
		const devices = this.listDevices.filter(e => e.Index == lineIndex);
		return devices;
	}
	// CheckAllDuplicate() {
	// 	this.listData.forEach(element => {
	// 		if (element.Error != "") {
	// 			const schedule = this.listData.find(row => element.Index != row.Index
	// 				&& row.ControllerIndex == element.ControllerIndex
	// 				&& row.ChannelIndexs == element.ChannelIndexs);
	// 			if (schedule == null) {
	// 				element.Error = "";
	// 			}
	// 		}
	// 	});
	// }
	deleteRow(index, row) {
		if (this.listData.length <= 1) return;

		const arrTemp = this.listData.filter((item) => {
			return item.Index != row.Index;
		});

		this.listData = arrTemp;
		this.UpdateIndex();
		this.updateModel();
		// this.CheckAllDuplicate();
	}
	UpdateIndex() {
		for (let index = 0; index < this.listData.length - 1; index++) {
			this.listData[index].Index = (index + 1);
		}
	}
	addRow(index, row) {
		if (index == 0) {
			this.AddEmptyRow(index);
			this.UpdateIndex();
			this.updateModel();
		}
	}
	delay(ms: number) {
		return new Promise(resolve => setTimeout(resolve, ms));
	}
	@Watch("model", {deep: true}) UpdateListData() {
		// console.log(this.model)
		this.listData = this.model;
		// console.log("this.model", this.model);
		this.addChannelToListData();
	}
	updateModel() {
		if (this.listData.length < 1) {
			this.listData.push({
				Index: 0,
				ControllerIndex: null,
				ChannelIndex: null,
				Order: 0,
				Error: "",
				Type: this.type,
				LineIndex: null,
				GateIndex: null,
				SerialNumber: ""
			});
		}
		this.$emit('model', this.listData);
	}
}


