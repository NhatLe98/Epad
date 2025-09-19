<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("TruckDriverOutInfo") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div style="padding: 0 10% 0 10%;">
          <label style="display: flex; justify-content: start; font-weight: bold; padding-left: 20px; font-size: 1.5vw;">
              {{ $t('VehicleOutInfo') }}
          </label>
          <div style="margin-top: 20px; height: calc(100vh - 470px); padding: 20px; display: flex; 
            flex-direction: column; justify-content: space-between; width: 100%; position: relative;"
            class="truck-driver-out-info">
            <el-row :gutter="40">
              <el-col :span="4">
                <label>{{ $t('TripCode') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input ref="hiddenTripCodeOut" v-model="hiddenTripCodeOut"
                  @keyup.enter.native="setTripCode" 
                  @click.native="focusTripCodeField()"
                  @blur="unfocusTripCodeField()"
                  style="opacity: 0; position: absolute; top: 0; left: 0;"></el-input>
                <el-input readonly ref="tripCode"
                  v-model="truckDriverInfo.TripCode" 
                  @click.native="focusTripCodeField()"
                  :placeholder="$t('ScanQR')"
                  @keyup.enter.native="getTruckDriverInfo"
                  :class="isTripCodeFocus == true ? 'is-focus' : ''"
                  id="tripCode">
                </el-input>
              </el-col>
              <el-col :span="4">
                <label>{{ $t('FullName') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input readonly v-model="truckDriverInfo.FullName" placeholder=""></el-input>
              </el-col>
            </el-row>
            <el-row :gutter="40">
              <el-col :span="4">
                <label>{{ $t('VehiclePlate') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input readonly v-model="truckDriverInfo.VehiclePlate" placeholder=""></el-input>
              </el-col>
              <!-- <el-col :span="4">
                <label>{{ $t('Supplier') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input readonly v-model="truckDriverInfo.Supplier" placeholder="" type="textarea" :rows="3" resize="none"></el-input>
              </el-col> -->
            </el-row>
            <el-row :gutter="40">
              <el-col :span="4">
                <label>{{ $t('OrderCode') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input readonly v-model="truckDriverInfo.OrderCode" placeholder="" type="textarea" :rows="3" resize="none"></el-input>
              </el-col>
              <el-col :span="4">
                <label>{{ $t('Supplier') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input readonly v-model="truckDriverInfo.Supplier" placeholder="" type="textarea" :rows="3" resize="none"></el-input>
              </el-col>
            </el-row>
            <el-row :gutter="40">
              <el-col :span="4">
                <label>{{ $t('NRICLabel') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input readonly v-model="truckDriverInfo.NRIC" placeholder=""></el-input>
              </el-col>
              <el-col :span="4">
                <label>{{ $t('DeliveryPoint') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input readonly v-model="truckDriverInfo.DeliveryPoint" placeholder=""></el-input>
              </el-col>
            </el-row>
            <el-row :gutter="40">
              <el-col :span="4">
                <label>{{ $t('MobilePhone') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input readonly v-model="truckDriverInfo.Phone" placeholder=""></el-input>
              </el-col>
              <el-col :span="4">
                <label>{{ $t('DeliveryTime') }}</label>
              </el-col>
              <el-col :span="8">
                <el-row :gutter="10">
                  <el-col :span="12">
                    <el-input readonly v-model="truckDriverInfo.DeliveryTimeDayString" placeholder=""></el-input>
                  </el-col>
                  <el-col :span="12">
                    <el-input readonly v-model="truckDriverInfo.DeliveryTimeHourString" placeholder=""></el-input>
                  </el-col>
                </el-row>
              </el-col>
            </el-row>
            <el-row :gutter="40">
              <el-col :span="4">
                <label>{{ $t('VehicleStatus') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input readonly v-model="truckDriverInfo.VehicleStatus" placeholder=""></el-input>
              </el-col>
              <el-col :span="4">
                <label>{{ $t('PassingVehicle') }}</label>
              </el-col>
              <el-col :span="8">
                <el-input readonly v-model="truckDriverInfo.PassingVehicleName" placeholder=""></el-input>
              </el-col>
            </el-row>
            <!-- <div style="width: 100%; position: relative;">
              <el-col :span="8" style="padding: 0 0 0 25px; position: absolute; bottom: 20px; right: 0;">
                <label style="display: flex; justify-content: center; color: red; font-weight: bold;">
                  {{ $t('ScanCardNumberOut') }}
                </label>
                <el-input readonly v-model="truckDriverInfo.CardNumber" placeholder=""
                :class="(truckDriverInfo.CardNumber == $t('CardNumberNotMatch') 
                || truckDriverInfo.CardNumber == $t('CardNumberNotExist')
                || truckDriverInfo.CardNumber == $t('CardReturned')) ? 'truck-driver__error-card-number' : ''"></el-input>
              </el-col>
            </div> -->
            <el-row :gutter="40">
              <el-col :span="4">
                <label>{{ $t('GateEntryTime') }}</label>
              </el-col>
              <el-col :span="8">
                <el-row :gutter="10">
                  <el-col :span="12">
                    <el-input readonly v-model="truckDriverInfo.GateEntryTimeDateString" placeholder=""></el-input>
                  </el-col>
                  <el-col :span="12">
                    <el-input readonly v-model="truckDriverInfo.GateEntryTimeHourString" placeholder=""></el-input>
                  </el-col>
                </el-row>
              </el-col>
              <el-col :span="4">
                <label style="color: blue; font-weight: bold;">{{ $t('GateExitTime') }}</label>
              </el-col>
              <el-col :span="8">
                <el-row :gutter="10">
                  <el-col :span="12">
                  <el-input readonly v-model="truckDriverInfo.GateExitTimeDateString" placeholder=""></el-input>
                  </el-col>
                  <el-col :span="12">
                    <el-input readonly v-model="truckDriverInfo.GateExitTimeHourString" placeholder=""></el-input>
                  </el-col>
                </el-row>
              </el-col>
            </el-row>
          </div>
        </div>
        <div style="height: calc(100vh - calc(100vh - 470px) - 170px); position: absolute;
          bottom: 20px;padding: 10px 10% 0 10%; width: calc(100% - 12px);"
          class="extra-truck-driver-out-info">
          <el-row :gutter="40" style="width: 100%; height: 100%;">
            <el-col :span="12" style="height: 100%; padding-left: 36px; padding-right: 8px;">
              <div style="height: calc(100% - 10px);margin-top: 10px;">
                <el-table
                  class="extra-driver-table"
                  :data="extraDriverData"
                  style="width: 100%; height: 100%; border: 1px solid lightgray;">
                  <el-table-column
                    prop="ExtraDriverName"
                    :label="$t('ExtraDriverName')">
                  </el-table-column>
                  <el-table-column
                    prop="ExtraDriverCode"
                    :label="$t('CCCD')">
                  </el-table-column>
                  <el-table-column
                    prop="BirthDayString"
                    :label="$t('BirthDay')">
                  </el-table-column>
                  <el-table-column
                    prop="CardNumber"
                    :label="$t('CardNumber')">
                  </el-table-column>
                </el-table>
              </div>
            </el-col>
            <el-col :span="12" 
            style="display: flex; flex-shrink: 0; padding-right: 8px;">
              <el-col :span="8"></el-col>
              <el-col :span="16" style="padding-right: 0;">
                <label style="display: flex; justify-content: center; color: red; font-weight: bold;">
                  {{ $t('ScanCardNumberOut') }}
                </label>
                <el-input ref="hiddenTruckDriverOutCardNumber" v-model="hiddenCardNumber" placeholder=""
                  @keyup.enter.native="setCardNumber" @click.native="focusCardNumberField()"
                  @blur="unfocusCardNumberField"
                  style="opacity: 0; height: 0; display: block;">
                </el-input>
                <el-input ref="truckDriverOutCardNumber" readonly v-model="truckDriverInfo.CardNumber" placeholder=""
                :class="[(truckDriverInfo.CardNumber == $t('CardNumberNotMatch') 
                || truckDriverInfo.CardNumber == $t('CardNumberNotExist')
                || truckDriverInfo.CardNumber == $t('CardReturned')) ? 'truck-driver__error-card-number' : '', 
                isCardNumberFocus ? 'focus-card-number' : '']"
                @click.native="focusCardNumberField()"
                ></el-input>
                <el-checkbox v-model="isException">{{
                  $t("Exception")
                }}</el-checkbox>
                <label v-if="isException" 
                style="display: flex; justify-content: center; font-weight: bold;">
                  {{ $t('ReasonAllowForExceptionOut') }}
                </label>
                <el-input v-model="truckDriverInfo.ReasonException" placeholder=""
                v-if="isException"></el-input>
              </el-col>
            </el-col>
          </el-row>
        </div>
        <div style="display: flex; justify-content: end; padding: 0 calc(10% + 20px) 0 10%;
        position: absolute; bottom: 20px; right: 0;">
          <el-button style="margin-right: 10px;width: 100px;"
              class="btnCancel"
              size="small"
              @click="cancel"
            >
              {{ $t("Cancel") }}
            </el-button>
            <el-button
            style="width: 100px;"
              type="primary"
              size="small"
              :disabled="isSavedLog"
              @click="saveLog"
            >
              {{ $t("OK") }}
            </el-button>
        </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./truck-driver-out-monitoring-component.ts"></script>
<style lang="scss">
.extra-truck-driver-out-info,
.truck-driver-out-info{
  label{
    display: flex;
    align-items: center;
    height: 40px;
    font-weight: bold;
  }
  .el-input {
    height: 5vh;
    .el-input__inner{
      height: 100%;
      font-size: 1.5vw;
    }
    .el-input__inner::placeholder{
      font-style: italic;
    }
  }
  .el-textarea {
    .el-textarea__inner{
      font-size: 1.5vw;
      line-height: 1.6vw;
    }
    .el-textarea__inner::placeholder{
      font-style: italic;
    }
  }
  .is-focus{
    #tripCode{
      border-color: #00b4ff !important;
    }
  }
}

.truck-driver__error-card-number {
  input{
    border-color: red;
    color: red;
    font-weight: bold;
  }
}

.extra-driver-table {
  .cell{
    text-align: center !important;
  }
  .el-table__header-wrapper{
    height: 35px !important;
  }
  .el-table__body-wrapper {
    height: calc(100% - 35px);
    overflow-y: auto;
  }
}
.focus-card-number{
  .el-input__inner{
    border-color: #00b4ff !important;
  }
}
</style>