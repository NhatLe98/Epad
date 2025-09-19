<template>
  <div class="container">
    <div id="reader" style="width: 750px"></div>
    <!-- Xuất ra chuỗi text -->
    <!-- <div v-if="qrCodeDataList.length > 0">
      <strong>QR Code Data</strong> :
      <ul>
        <li v-for="(data, index) in qrCodeDataList" :key="index">{{ data }}</li>
      </ul>
    </div> -->

    <!-- Cắt thông tin gán vào các fields -->
    <form v-if="cccdInfo">
      <label for="cccdNumber">Số CCCD:</label>
      <input type="text" id="cccdNumber" v-model="cccdInfo.cccdNumber" />
      <br />
      <label for="oldIDNumber">Số CMND cũ:</label>
      <input type="text" id="oldIDNumber" v-model="cccdInfo.oldIDNumber" />
      <br />
      <label for="fullName">Họ và Tên:</label>
      <input type="text" id="fullName" v-model="cccdInfo.fullName" />
      <br />
      <label for="dateOfBirth">Ngày sinh:</label>
      <input type="text" id="dateOfBirth" v-model="cccdInfo.dateOfBirth" />
      <br />
      <label for="gender">Giới tính:</label>
      <input type="text" id="gender" v-model="cccdInfo.gender" />
      <br />
      <label for="permanentAddress">Địa chỉ thường trú:</label>
      <input
        type="text"
        id="permanentAddress"
        v-model="cccdInfo.permanentAddress"
      />
      <br />
      <label for="issueDate">Ngày cấp:</label>
      <input type="text" id="issueDate" v-model="cccdInfo.issueDate" />
    </form>
  </div>
</template>
<script>
import { Html5Qrcode } from "html5-qrcode";
import axios from "axios";

export default {
  data() {
    return {
      qrCodeDataList: [],
      scanner: null,
      cccdInfo: {
        cccdNumber: "",
        oldIDNumber: "",
        fullName: "",
        dateOfBirth: "",
        gender: "",
        permanentAddress: "",
        issueDate: "",
      },
    };
  },
  mounted() {
    this.startScan();
  },
  methods: {
    startScan() {
      this.scanner = new Html5Qrcode("reader", { verbose: false });
      const config = { fps: 7, qrbox: { width: 450, height: 350 } };
      this.scanner
        .start(
          { facingMode: "environment" },
          config,
          this.onScanSuccess,
          this.onScanFailure
        )
        .catch((err) => console.error("Unable to start scanning", err));
    },
    async onScanSuccess(decodedText) {
      this.qrCodeDataList.push(decodedText);
      try {
        const response = await axios.post(
          "https://localhost:44327/BarcodeQRCode/parse-cccd",
          { qrData: decodedText }
        );
        this.cccdInfo = response.data;
      } catch (error) {
        console.error(
          "Error sending QR code data to API:",
          error.response ? error.response.data : error.message
        );
      }
    },
  },
  beforeUnmount() {
    if (this.scanner) {
      this.scanner
        .stop()
        .catch((err) => console.error("Unable to stop scanning", err));
    }
  },
};
</script>
<style lang="scss">
.container {
  display: flex;
}
#reader{
  margin-left: 50px;
}
form {
  display: grid;
  margin-left: 100px;
  grid-template-columns: repeat(2, 1);
  label {
    font-weight: bold;
    font-size: 16px;
    margin-bottom: 5px;
  }
  input {
    padding: 3px 5px;
    width: 300px;
    height: 30px;
    border-radius: 5px;
    outline: none;
    border: 1px solid;
  }
}
</style>

