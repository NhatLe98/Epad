<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("TruckDriverInInfo") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <el-select style="padding: 0 0 10px calc(10% + 20px);"
          v-model="gateIndex"
          filterable
          clearable
        >
          <el-option
            v-for="item in listGate"
            :key="item.Index"
            :label="item.Name"
            :value="item.Index"
          ></el-option>
        </el-select>
        <div style="padding: 0 10% 0 10%;">
          <div>
            <label style="font-weight: bold; padding-left: 20px; font-size: 1.5vw;">
              {{ $t('VehicleInInfo') }}
            </label>
            <el-button  
              style="width: 10vw; background-color: #02f061 !important; border: 2px solid lightgray; 
              color: black; font-weight: bold; position: absolute; right: 10px;" 
              type="primary" @click="openReturnCardDialog">
                {{$t('ReturnCard')}}
            </el-button>
          </div>

          <div style="margin-top: 20px; height: calc(100vh - 510px); padding: 20px; display: flex; 
            flex-direction: column; justify-content: space-between; width: 100%; position: relative;"
            class="truck-driver-in-info">
            <el-row :gutter="40">
              <el-col :span="4">
                <label>{{ $t('TripCode') }}</label>
              </el-col>
              <el-col :span="8">
                <!-- Use @blur below for both el-input tripCode below if you want force focus on it-->
                <!-- @blur="!showExtraDriverDialog && !showReturnCardForm ? $refs.hiddenTripCodeIn.focus() : null" -->
                <el-input ref="hiddenTripCodeIn" v-model="hiddenTripCodeIn"
                  @click.native="focusTripCodeField()"
                  @keyup.enter.native="setTripCode" 
                  @blur="unfocusTripCodeField()"
                  style="opacity: 0; position: absolute; top: 0; left: 0;width: 1px;"></el-input>
                <el-input readonly ref="tripCode" 
                  @click.native="focusTripCodeField()"
                  v-model="truckDriverInfo.TripCode" 
                  :placeholder="$t('ScanQR')"
                  @keyup.enter.native="getTruckDriverInfo"
                  id="tripCode"
                  :class="isTripCodeFocus == true ? 'is-focus' : ''">
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
                <el-input ref="vehiclePlate"
                class="truck-deriver-in__vehicle-plate" v-model="truckDriverInfo.VehiclePlate" placeholder=""
                :placeholder="$t('PleaseInputTransitVehiclePlateAndPressEnter')"
                @click.native="focusVehiclePlateField()"
                @keyup.enter.native="getTransitTruckDriverInfoByVehiclePlate"
                ></el-input>
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
                <el-input readonly :value="truckDriverInfo.PassingVehicleName ? 'VC' : ''" placeholder=""></el-input>
              </el-col>
            </el-row>
            <!-- <el-col :span="8" style="padding: 0 20px 0 20px; position: absolute; bottom: 20px; right: 0;">
                <label style="display: flex; justify-content: center; color: red; font-weight: bold;">
                  {{ $t('CardNumber') }}
                </label>
                <el-input readonly v-model="truckDriverInfo.CardNumber" placeholder=""></el-input>
            </el-col> -->
          </div>
        </div>
        <div style="height: calc(100vh - calc(100vh - 510px) - 210px); position: absolute;
          bottom: 20px;padding: 10px 10% 0 10%; width: calc(100% - 12px);"
          class="extra-truck-driver-in-info">
          <el-row :gutter="40" style="width: 100%; height: 100%;">
            <el-col :span="12" style="height: 100%; padding-left: 36px; padding-right: 8px;">
              <el-button
                type="primary"
                @click="openExtraDriverDialog"
                icon="el-icon-circle-plus-outline"
                class="add-button"
              >
                {{ $t("AddExtraDriver") }}
              </el-button>
              <div style="height: calc(100% - 36px - 10px);margin-top: 10px;">
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
                  <el-table-column width="60">
                    <template slot-scope="scope">
                      <div style="display: flex;">
                        <el-button type="text" style="margin-right: 10px;padding: 0 !important;" 
                        @click="editExtraDriver(scope.row)">
                          <img src="@/assets/icons/Button/Edit.png" alt="edit" />
                        </el-button>
                        <el-button type="text" style="margin-right: 10px;padding: 0 !important;" 
                        @click="deleteExtraDriver(scope.row)">
                          <img src="@/assets/icons/Button/Delete.png" alt="delete" />
                        </el-button>
                      </div>
                    </template>
                  </el-table-column>
                </el-table>
              </div>
            </el-col>
            <el-col :span="12" 
            style="display: flex; flex-shrink: 0; padding-right: 8px;">
              <el-col :span="8"></el-col>
              <el-col :span="16" style="padding-right: 0;">
                <label style="display: flex; justify-content: center; color: red; font-weight: bold;">
                  {{ $t('CardNumber') }}
                </label>
                <el-input ref="hiddenTruckDriverInCardNumber" v-model="hiddenCardNumber" placeholder=""
                  @keyup.enter.native="setCardNumber" @click.native="focusCardNumberField()"
                  @blur="unfocusCardNumberField"
                  style="opacity: 0; height: 0; display: block;">
                </el-input>
                <el-input ref="truckDriverInCardNumber" readonly v-model="truckDriverInfo.CardNumber" placeholder="" 
                  @click.native="focusCardNumberField()" 
                  :class="isCardNumberFocus ? 'focus-card-number' : ''">
                </el-input>
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
            @click="saveLog"
            :disabled="isSavedLog || (truckDriverInfo.CardNumber && truckDriverInfo.CardNumber != '' 
            && (truckDriverInfo.CardNumber.includes($t('CardNumberNotExist')) 
            || truckDriverInfo.CardNumber.includes($t('CardIsAssigned'))))"
          >
            {{ $t("OK") }}
          </el-button>
        </div>
        <el-dialog :title="isEdit ? $t('EditExtraDriver') : $t('AddExtraDriver')" custom-class="customdialog" 
          :visible.sync="showExtraDriverDialog" :before-close="cancelExtraDriverDialog" :close-on-click-modal="false">
            <el-form
              :model="extraDriverForm"
              :rules="extraDriverRules"
              ref="extraDriverForm"
              label-width="168px"
              label-position="top"
            >
            <el-form-item :label="$t('ExtraDriverName')" prop="ExtraDriverName" @click.native="focus('ExtraDriverName')">
                <el-input ref="ExtraDriverName" v-model="extraDriverForm.ExtraDriverName"></el-input>
              </el-form-item>
              <el-form-item :label="$t('CCCD')" prop="ExtraDriverCode"  @click.native="focus('ExtraDriverCode')">
                <el-input ref="ExtraDriverCode" v-model="extraDriverForm.ExtraDriverCode"></el-input>
              </el-form-item>
              <el-form-item :label="$t('BirthDay')" prop="BirthDay"  @click.native="focus('BirthDay')">
                <el-date-picker :placeholder="$t('SelectBirthDay')" format="dd/MM/yyyy" v-model="extraDriverForm.BirthDay"
                  type="date"></el-date-picker>
              </el-form-item>
              <el-form-item :label="$t('CardNumber')" prop="CardNumber" @click.native="focus('CardNumber')">
                <el-input ref="hiddenExtraCardNumber" v-model="hiddenExtraCardNumber" placeholder=""
                  @keyup.enter.native="setExtraCardNumber" @click.native="focusExtraCardNumberField()"
                  @blur="unfocusExtraCardNumberField"
                  style="opacity: 0; height: 0; display: block;">
                </el-input>
                <el-input readonly ref="extraCardNumber" v-model="extraDriverForm.CardNumber" 
                @click.native="focusExtraCardNumberField()" :class="isExtraCardNumberFocus ? 'focus-card-number' : ''"></el-input>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnOK" v-if="!isEdit" type="primary" @click="submitExtraDriver(true)" 
                style="width: fit-content;">{{ $t("SaveAndContinue") }}</el-button>
              <el-button class="btnOK" type="primary" @click="submitExtraDriver(false)">{{ $t("Save") }}</el-button>
              <el-button class="btnCancel" @click="cancelExtraDriverDialog">
                {{
                $t("Cancel")
                }}
              </el-button>
            </span>
          </el-dialog>

          <el-dialog :title="$t('ReturnCard')" custom-class="customdialog returnCardDialog"
          :visible.sync="showReturnCardForm" :before-close="cancelReturnCardDialog" :close-on-click-modal="false">
          <el-row :gutter="10">
            <el-col :span="12">
              <el-form
                :model="returnCardForm"
                :rules="returnCardFormRules"
                ref="returnCardForm"
                label-width="168px"
                label-position="top"
              >
              <el-form-item :label="$t('CardNumber')" prop="CardNumber">
                <el-input ref="hiddenReturnCardNumber" v-model="hiddenReturnCardNumber" placeholder=""
                    @keyup.enter.native="setReturnCardNumber" @click.native="focusReturnCardNumberField()"
                    @blur="unfocusReturnCardNumberField"
                    style="opacity: 0; height: 0; display: block;">
                  </el-input>
                  <el-input readonly :disabled="returnCardForm.IsLostCard" ref="returnCardNumber" v-model="returnCardForm.CardNumber" 
                  :placeholder="!returnCardForm.IsLostCard ? $t('PleaseScanCard') : ''" 
                  @click.native="focusReturnCardNumberField()" :class="isReturnCardNumberFocus ? 'focus-card-number' : ''"></el-input>
                </el-form-item>
                <el-form-item prop="IsLostCard">
                  <el-checkbox ref="IsLostCard" @change="changeIsLostCard" v-model="returnCardForm.IsLostCard">{{$t('LostCard')}}</el-checkbox>
                </el-form-item>
                <el-form-item :label="$t('CCCD')" prop="NRIC">
                  <el-input @keyup.enter.native="searchDriverByCCCD" :readonly="!returnCardForm.IsLostCard" 
                    ref="NRIC" :placeholder="returnCardForm.IsLostCard ? $t('PleaseInputNRICAndPressEnterToSearch'): ''" 
                    v-model="returnCardForm.NRIC"></el-input>
                </el-form-item>
                <el-form-item :label="$t('TripCode')" prop="TripCode">
                  <el-input readonly ref="TripCode" v-model="returnCardForm.TripCode"></el-input>
                </el-form-item>
                <el-form-item :label="$t('FullName')" prop="FullName">
                  <el-input readonly ref="FullName" v-model="returnCardForm.FullName"></el-input>
                </el-form-item>
                <el-form-item :label="$t('Note')" prop="Description">
                  <el-input ref="Description" v-model="returnCardForm.Description"></el-input>
                </el-form-item>
              </el-form>
            </el-col>
            <el-col :span="12" style="position: absolute; top: 0; bottom: 0; right: 0;">
              <label class="el-form-item__label" 
              style="height: 31px;">
                {{$t('ListDriverOfTrip')}}
              </label>
              <el-table
                  class="extra-driver-table"
                  :data="returnCardForm.ListLogDriver"
                  style="width: 100%; height: 100%; border: 1px solid lightgray;">
                  <el-table-column
                    prop="FullName"
                    :label="$t('FullName')">
                  </el-table-column>
                  <el-table-column
                    prop="CardNumber"
                    :label="$t('CardNumber')">
                  </el-table-column>
                  <el-table-column
                    prop="IsExtraDriver"
                    :label="$t('Object')">
                    <template slot-scope="scope">
                      {{ scope.row.IsExtraDriver ? $t('ExtraDriver') : $t('Driver') }}
                    </template>
                  </el-table-column>
                  <el-table-column
                    prop="IsExpired"
                    :label="$t('ReturnedCard')">
                    <template slot-scope="scope">
                      <el-checkbox v-model="scope.row.IsExpired" style="color: none; pointer-events: none;">
                      </el-checkbox>
                    </template>
                  </el-table-column>
                </el-table>
            </el-col>
          </el-row>
          <span slot="footer" class="dialog-footer">
            <el-button class="btnOK" v-if="!isEdit" type="primary" @click="returnCard(true)" 
              style="width: fit-content;">{{ $t("SaveAndContinue") }}</el-button>
            <el-button class="btnOK" type="primary" @click="returnCard(false)">{{ $t("Save") }}</el-button>
            <el-button class="btnCancel" @click="cancelReturnCardDialog">
              {{
              $t("Cancel")
              }}
            </el-button>
          </span>
        </el-dialog>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./truck-driver-in-monitoring-component.ts"></script>
<style lang="scss">
.extra-truck-driver-in-info,
.truck-driver-in-info{
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
  .truck-deriver-in__vehicle-plate{
    .el-input__inner::placeholder{
      font-size: 0.8vw;
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

.add-button {
    display: inline-flex;
}
.add-button .el-icon-circle-plus-outline {
    font-size: 18px;
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
.returnCardDialog{
  width: 50vw;
}
</style>