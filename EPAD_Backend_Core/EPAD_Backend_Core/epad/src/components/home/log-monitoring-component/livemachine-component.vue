<template>
  <div>
    <div class="title-live-machine">
      <span v-if="isConnect" class="title online">{{ $t("LiveAttendanceLog") }}</span>
      <span v-else class="title offline">{{ $t("LiveAttendanceLog") }}</span>
    </div>
    <div class="item-attend-wrapper">
      <div class="item-attend" v-for="(item, index) in tableData" :key="index">
        <div class="live-attend-nameandtype">
          <div class="live-attend-fullname">{{ item.fullname }}</div>
          <div class="live-attend-attendtype">{{ $t('AttendanceMachine') + " " + $t(item.verify) }}</div>
        </div>
        <div class="datetime">
          <div class="live-attend-time">{{ item.time }}</div>
          <div class="live-attend-date">{{ item.date }}</div>
        </div>
      </div>
    </div>
  </div>
</template>
<style>
.title-live-machine {
  width: 100%;
  height: 62px;
  padding-top: 27px;
  padding-left: 26px;
  font-family: 'Noto Sans';
  font-style: normal;
  font-weight: bold;
  font-size: 12px;
  letter-spacing: 0.05em;
  color: #000000;
  border-bottom: 0.5px solid #767676;
}
span.title {
  float: left;
  display: block;
  text-transform: uppercase;
}
span.title::before, span.title.online::before {
  width: 6px;
  height: 6px;
  background-color: #EB5757;
  content: "";
  display: block;
  float: right;
  margin-left: 5px;
  border-radius: 10px;
  margin-top: -5px;
}
span.title.online::before {
  background-color: #67C23A;
}

.item-attend-wrapper {
  height: calc(100vh - 150px);
  overflow-y: auto;
}
/* .live-attend-image {
  float: left;
} */
.item-attend {
  padding-left: 24px;
  margin-top: 16px;
  padding-right: 19px;
  height: 36px;
  width: 100%;
  display: inline-flex;
  justify-content: space-between;
}

.live-attend-nameandtype {
  flex-grow: 1;
}
.live-attend-fullname, .live-attend-attendtype {
  width: 100%;
  overflow-x: hidden;
  height: 18px;
  line-height: 18px;
  font-size: 12px;
}
.live-attend-fullname {
  font-weight: 600;
}
.live-attend-attendtype {
  text-transform: lowercase;
}
.datetime {
  margin-left: 10px;
  width: fit-content;
  height: 36px;
}
.live-attend-date, .live-attend-time {
  width: fit-content;
  height: 18px;
  line-height: 18px;
  font-size: 12px;
}
</style>
<script src="./livemachine-component.ts"></script>
