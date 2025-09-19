import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import { throws } from "assert";
@Component({
  name: "transfer-labor",
  components: { HeaderComponent }
})
export default class TransferLaborComponent extends Vue {
  Title = "";
  FromDate = "";
  BtnView = "";
  Schedule = "";
  tableData = [];
  data = [];
  mounted() {
    this.Title = "Điều chuyển phòng ban";
    this.FromDate = "Từ ngày";
    this.BtnView = "Xem";
    this.Schedule = "Đồng bộ người dùng trên máy";
    this.tableData = [
      {
        date: "2016-05-03",
        name: "Tom",
        address: "No. 189, Grove St, Los Angeles"
      },
      {
        date: "2016-05-02",
        name: "Tom",
        address: "No. 189, Grove St, Los Angeles"
      },
      {
        date: "2016-05-04",
        name: "Tom",
        address: "No. 189, Grove St, Los Angeles"
      },
      {
        date: "2016-05-01",
        name: "Tom",
        address: "No. 189, Grove St, Los Angeles"
      }
    ];
    this.data = [{
      id: 1,
      label: 'Tinh Hoa Solutions',
      children: [
        {
          id: 2, label: 'Phần mềm',
          children: [
            { id: 2.1, label: 'Lập trình', children: [{ id: 2.11, label: 'Trần Xuân Vũ' }, { id: 2.12, label: 'Trần Hoàng Phúc' }, { id: 2.13, label: 'Nguyễn Phương Nam' }, { id: 2.14, label: 'Nguyễn Hùng Anh' }] },
            { id: 2.2, label: 'SIS', children: [{ id: 3.11, label: 'Hồ Ngọc Lương' }, { id: 3.12, label: 'Trần Đức Hải' }, { id: 3.13, label: 'Huỳnh Phước An' }, { id: 3.14, label: 'Nguyễn Kính' }] },
            { id: 2.3, label: 'QC' ,children: [{ id: 4.11, label: 'Nguyễn Ngọc Ánh Hồng' }, { id: 3.12, label: 'Lê Bá Hưng' }, { id: 3.13, label: 'Nguyễn Kim Luyến' }]},
            { id: 2.4, label: 'BA' }
          ]
        }, {
          id: 3, label: 'Phần cứng',
          children: [
            { id: 3.1, label: 'HIS' },
            { id: 3.2, label: 'CS' }
          ]
        }]
    }];
  }
}
