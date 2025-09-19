<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("AddCustomerInfo") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome bgCustomerInfo">
        <div style="display: flex; justify-content: center; margin-bottom: 15px;">
          <label style="display: flex; align-items: center;">{{ $t('SearchCustomerInfo') }}</label>
          <app-select-new :dataSource="listAllCustomerFilter" displayMember="FullName" valueMember="Index"
            :allowNull="true" v-model="customerID" :multiple="false" style="margin-left: 10px; width: 33vw;"
            :placeholder="$t('SelectCustomer')" ref="employeeList" @onChange="changeCustomer" class="customer-select"
            @changeVisible="changeVisible">
          </app-select-new>
        </div>
        <el-row :gutter="20" style="height: calc(100vh - 180px); padding: 0 10% 0 10%; width: calc(100vw - 360px)">
          <el-col style="width: 60%;">
            <label style="display: flex; justify-content: start; font-weight: bold; padding-left: 10px; 
              text-shadow: 0px 0.5px, 0.5px 0px, 0.5px 0.5px;">
              {{ $t('BasicInfo') }}
            </label>
            <el-row :gutter="10" class="customer-info" style="padding: 10px; height: calc(100vh - 150px); display: flex; 
            flex-direction: column; justify-content: space-between; width: 100%;">
              <div style="display: flex;align-items: center;">
                <el-col :span="6">
                  <label>{{ $t('FullName') }}</label>
                </el-col>
                <el-col :span="18">
                  <el-input readonly v-model="customerInfo.FullName" placeholder=""></el-input>
                </el-col>
              </div>
              <div style="display: flex;align-items: center;">
                <el-col :span="6">
                  <label>{{ $t('CompanyName') }}</label>
                </el-col>
                <el-col :span="18">
                  <el-input readonly v-model="customerInfo.Company" placeholder=""></el-input>
                </el-col>
              </div>
              <div style="display: flex;align-items: center;">
                <el-col :span="6">
                  <label>{{ $t('NRICLabel') }}</label>
                </el-col>
                <el-col :span="18">
                  <el-input readonly v-model="customerInfo.NRIC" placeholder=""></el-input>
                </el-col>
              </div>
              <div style="display: flex;align-items: end; margin-top: -15px; width: calc(100%); overflow-x: visible !important;">
                <el-col :span="24" style="flex-shrink: 0; display: flex; align-items: center;">
                  <el-col :span="6" style="flex-shrink: 0; padding: 0;">
                    <label>{{ $t('MobilePhone') }}</label>
                  </el-col>
                  <el-col :span="18" style="flex-shrink: 0; padding: 0 0 0 3px;">
                    <el-input readonly v-model="customerInfo.Phone" placeholder=""></el-input>
                  </el-col>
                </el-col>

                <el-col :span="1">             
                </el-col>
                <el-col :span="4" style="flex-shrink: 0; visibility: hidden; cursor: none;">
                </el-col>
                <el-col
                  style="flex-shrink: 0; min-width: 0; width: calc(((100% + 40px) / 60 * 100) - calc(100% / 24 * 29) - 5px);">
                  <label style="margin-top: 5px; display: flex; justify-content: center; font-weight: bold; color: blue;">
                    {{ $t('CardNumber') }}
                  </label>
                  <el-input class="customer-info__card-number__hidden" v-model="customerInfo.CardNumberHidden" 
                    placeholder="" ref="customerInfoCardNumberHidden" @focus="$refs.customerInfoCardNumberHidden.focus()"
                    @blur="!visibleCustomerSelect ? $refs.customerInfoCardNumberHidden.focus() : null" 
                    @keyup.enter.native="checkCardByNumber"
                    style="opacity: 0; height: 0; padding: 0; margin: 0; display: block;" 
                    onkeypress="return event.charCode >= 48 && event.charCode <= 57">
                  </el-input>
                  <el-input class="customer-info__card-number" v-model="customerInfo.CardNumber" 
                    placeholder="" ref="customerInfoCardNumber" readonly>
                  </el-input>
                </el-col>
              </div>
              <div style="display: flex;align-items: end; width: calc(100%); overflow-x: visible !important;"
              :style="(customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor != customerID) ? 'margin-top: -15px;' : ''">
                <el-col :span="24" style="flex-shrink: 0; display: flex; align-items: center;">
                  <el-col :span="6" style="flex-shrink: 0;padding:0;">
                    <label>{{ $t('VehiclePlate') }}</label>
                  </el-col>
                  <el-col :span="18" style="flex-shrink: 0;padding: 0 0 0 3px;">
                    <!-- <el-input readonly v-model="customerInfo.BikePlate" placeholder=""></el-input> -->
                    <span style="display: inline-block; width: 100%;color: #606266;
                      white-space: normal;word-wrap: break-word;overflow-wrap: break-word; 
                      min-height: 5vh; font-size: 1.5vw; border-radius: 5px; border-color: #C0C4CC;
                      border: 1px solid #DCDFE6;align-content: center;padding-left: 10px;"> 
                      {{ customerInfo.BikePlate }}
                    </span>
                  </el-col>
                </el-col>
                <el-col :span="1">             
                </el-col>
                <el-col :span="4" style="flex-shrink: 0; visibility: hidden; cursor: none;">
                </el-col>
                <el-col
                  style="flex-shrink: 0; min-width: 0; width: calc(((100% + 40px) / 60 * 100) - calc(100% / 24 * 29) - 5px);">
                  <label v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor != customerID"
                  style="margin-top: 15px; display: flex; justify-content: start; font-weight: bold;">
                    {{ $t('CardIssuanceFor') }}:
                  </label>
                  <el-input v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor != customerID"
                    class="customer-info__card-number" v-model="customerInfo.CardIssuanceForName" placeholder="" 
                    style="margin-top: 10px;" readonly>
                  </el-input>
                  <div style="display: flex; justify-content: center; margin-top: 15px;" 
                  v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor == customerID">
                    <el-button  
                    style="width: 66.67%; background-color: #02f061 !important; border: 2px solid lightgray; 
                    color: black; font-weight: bold;" 
                    type="primary" @click="returnOrDeleteCard('return')">
                      {{$t('ReturnCard')}}
                    </el-button>
                    <!-- <el-button v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor != customerID"
                    style="width: 66.67%; background-color: #02f061 !important; border: 2px solid lightgray; 
                    color: black; font-weight: bold;" 
                    type="primary" @click="returnOrDeleteCard('delete')">
                      {{$t('DeleteCard')}}
                    </el-button> -->
                  </div>
                </el-col>
              </div>
              <div v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor != customerID"
                style="display: flex;align-items: center; width: calc(100%); overflow-x: visible !important;">
                <el-col :span="24" style="flex-shrink: 0;">
                </el-col>
                <el-col :span="1">
                </el-col>
                <el-col :span="4" style="flex-shrink: 0;">
                </el-col>
                <el-col
                  style="flex-shrink: 0; min-width: 0; width: calc(((100% + 40px) / 60 * 100) - calc(100% / 24 * 29) - 5px);">
                  <div style="display: flex; justify-content: center; margin-top: 15px;">
                    <!-- <el-button v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor == customerID" 
                    style="width: 66.67%; background-color: #02f061 !important; border: 2px solid lightgray; 
                    color: black; font-weight: bold;" 
                    type="primary" @click="returnOrDeleteCard('return')">
                      {{$t('ReturnCard')}}
                    </el-button> -->
                    <el-button v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor != customerID"
                    style="width: 66.67%; background-color: #02f061 !important; border: 2px solid lightgray; 
                    color: black; font-weight: bold;" 
                    type="primary" @click="returnOrDeleteCard('delete')">
                      {{$t('DeleteCard')}}
                    </el-button>
                  </div>
                </el-col>
              </div>
              <div
                style="display: flex;align-items: center; width: calc(100%); overflow-x: visible !important;">
                <el-col :span="6" style="flex-shrink: 0;">
                  <label>{{ $t('ContactPerson') }}</label>
                </el-col>
                <el-col :span="18" style="flex-shrink: 0;">
                  <el-input readonly v-model="customerInfo.ContactPersonName" placeholder=""></el-input>
                </el-col>
                <el-col :span="1">
                </el-col>
                <el-col :span="4" style="flex-shrink: 0;">
                  <label>{{ $t('Department') }}</label>
                </el-col>
                <el-col
                  style="flex-shrink: 0; min-width: 0; width: calc(((100% + 40px) / 60 * 100) - calc(100% / 24 * 29) - 5px);">
                  <el-input readonly v-model="customerInfo.ContactDepartmentName" placeholder=""></el-input>
                </el-col>
              </div>
              <div style="display: flex;align-items: center;">
                <el-col :span="6">
                  <label>{{ $t('ContactPersonPhoneNumber') }}</label>
                </el-col>
                <el-col :span="18">
                  <el-input readonly v-model="customerInfo.ContactPersonPhoneNumber" placeholder=""></el-input>
                </el-col>
              </div>
              <div style="display: flex;align-items: center; width: calc(100%); overflow-x: visible !important;">
                <el-col :span="6" style="flex-shrink: 0;">
                  <label>{{ $t('RegisterTime') }}</label>
                </el-col>
                <el-col :span="18" style="flex-shrink: 0;">
                  <el-row :gutter="10">
                    <el-col :span="12">
                      <el-input readonly v-model="customerInfo.FromTimeDayString" placeholder=""></el-input>
                    </el-col>
                    <el-col :span="12">
                      <el-input readonly v-model="customerInfo.ToTimeDayString" placeholder=""></el-input>
                    </el-col>
                    <el-col :span="12" style="margin-top: 10px;">
                      <el-input readonly v-model="customerInfo.FromTimeHourString" placeholder=""></el-input>
                    </el-col>
                    <el-col :span="12" style="margin-top: 10px;">
                      <el-input readonly v-model="customerInfo.ToTimeHourString" placeholder=""></el-input>
                    </el-col>
                  </el-row>
                </el-col>
                <el-col :span="1">
                </el-col>
                <el-col :span="4" style="flex-shrink: 0;">
                  <label>{{ $t('WorkingContent') }}</label>
                </el-col>
                <el-col
                  style="flex-shrink: 0; min-width: 0; width: calc(((100% + 40px) / 60 * 100) - calc(100% / 24 * 29) - 5px);">
                  <el-input readonly v-model="customerInfo.WorkingContent" placeholder=""
                  type="textarea" :rows="3" style="padding-top: 3vh;"></el-input>
                </el-col>
              </div>
              <div style="display: flex;align-items: center;">
                <el-col :span="6">
                  <label>{{ $t('PhoneUseIsAllow') }}</label>
                </el-col>
                <input type="checkbox" id="isAllowPhoneCheckbox" onclick="return false;"
                  :checked="customerInfo.IsAllowPhone" style="margin-left: 15px;" />
                <!-- <el-col :span="9">
                  <label>{{ $t('PhoneUseIsAllow') }}</label>
                </el-col>
                <el-col :span="12">
                  <input type="checkbox" id="isAllowPhoneCheckbox" onclick="return false;" :checked="customerInfo.IsAllowPhone"/>
                </el-col> -->
              </div>
              <div style="display: flex;align-items: center;">
                <el-col :span="6">
                  <label>{{ $t('Note') }}</label>
                </el-col>
                <el-col :span="18">
                  <el-input readonly v-model="customerInfo.Note" placeholder=""></el-input>
                </el-col>
              </div>
              <!-- <div style="display: flex;align-items: center;">
                <el-col :span="6">
                  <label>{{ $t('VehiclePlate') }}</label>
                </el-col>
                <el-col :span="18">
                  <el-input readonly v-model="customerInfo.BikePlate" placeholder=""></el-input>
                </el-col>
              </div> -->
            </el-row>
          </el-col>
          <el-col style="width: 40%; padding-left: calc(10% - 20px);">
            <label style="font-weight: bold; display: block;">{{ $t('UserImage') }}</label>
            <el-image
              :src="(customerInfo.Avatar && customerInfo.Avatar != '') ? ('data:image/jpeg;base64, ' + customerInfo.Avatar) : defaultImage"
              fit="fill"
              style="width: 100%; height: 17vh; margin-top: 5px; border: 1px solid lightgray; border-radius: 1vw;">
            </el-image>

            <!-- <label style="margin-top: 5px; display: flex; justify-content: center; font-weight: bold; color: blue;">
              {{ $t('CardNumber') }}
            </label>
            <el-input class="customer-info__card-number" v-model="customerInfo.CardNumber" 
            placeholder="" style="margin-top: 10px;" readonly>
            </el-input>

            <label v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor != customerID" 
            style="margin-top: 15px; display: flex; justify-content: start; font-weight: bold;">
              {{ $t('CardIssuanceFor') }}:
            </label>
            <el-input v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor != customerID"
              class="customer-info__card-number" v-model="customerInfo.CardIssuanceForName" placeholder="" 
              style="margin-top: 10px;" readonly>
            </el-input>

            <div style="display: flex; justify-content: center; margin-top: 15px;">
              <el-button v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor == customerID" 
              style="width: 66.67%; background-color: #02f061 !important; border: 2px solid lightgray; 
              color: black; font-weight: bold;" 
              type="primary" @click="returnOrDeleteCard('return')">
                {{$t('ReturnCard')}}
              </el-button>
              <el-button v-if="customerInfo.CardUserIndex && customerInfo.CardUserIndex > 0 && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor != customerID"
              style="width: 66.67%; background-color: #02f061 !important; border: 2px solid lightgray; 
              color: black; font-weight: bold;" 
              type="primary" @click="returnOrDeleteCard('delete')">
                {{$t('DeleteCard')}}
              </el-button>
            </div> -->
          </el-col>
        </el-row>

        <div style="display: flex; justify-content: end; margin-top: 5px; padding: 0 calc(10% + 75px) 0 0;">
          <el-button class="btnCancel" @click="CancelClick">
            {{
            $t("Cancel")
          }}
          </el-button>
          <el-button class="btnOK" type="primary" @click="ConfirmClick"
            :disabled="customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor && customerInfo.CardIssuanceFor != ''">
            {{
            $t("OK")
          }}
          </el-button>
        </div>


      </el-main>
    </el-container>
  </div>
</template>
<script src="./gc-customer-info-component.ts"></script>
<style lang="scss">
.customer-info {
  label {
    font-weight: bold;
  }

  .el-input {
    height: 5vh;

    .el-input__inner {
      height: 100%;
      font-size: 1.5vw;
    }
  }

  .el-textarea__inner {

      font-size: 1.5vw;
    }
}

.customer-info__card-number {
  height: 5vh;
  margin-top: 5px !important;

  .el-input__inner {
    caret-color: transparent;
    pointer-events: none;
    height: 100%;
    font-size: 1.5vw;
  }
}

.customer-select {
  .el-input {
    height: 40px;

    .el-input__inner {
      height: 100%;
      font-size: 1.5vw;
    }
  }
}

/* Unstyled checkbox */
#isAllowPhoneCheckbox {
  appearance: none;
  -webkit-appearance: none;
  -moz-appearance: none;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  border: 2px solid #ccc;
  outline: none;
  transition: border-color 0.3s, background-color 0.3s;
}

/* Checked checkbox */
#isAllowPhoneCheckbox:checked {
  border-color: #007bff;
  /* Blue color */
  background-color: #007bff;
  position: relative;
}

/* White dot in the middle */
#isAllowPhoneCheckbox:checked::after {
  content: "";
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: #fff;
}

.bgCustomerInfo {
  padding-top: 10px !important;
  overflow-y: hidden;
}
</style>