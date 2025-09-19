<template>
  <div class="machineforhome">
    <div class="title-machineforhome"><span class="formname">{{ $t("ListMachine") }}</span></div>
    <div class="tableforhome">
        <VisualizeTable 
            :columnDefs="columnDefs"
            :loading="loading"
            :rowData="devices"
            :rowHeight="50"
            :rowBuffer="1000"
            :heightScale="130"
            :isKeepIndexAscending="true"
            ref="visualizeTable"
        />
    </div>
  </div>
</template>

<style>
.formname {
  font-family: 'Noto Sans';
  font-size: 18px;
  margin-left: 20px;
  font-weight: 600;
}
.machineforhome {
  padding: 32px 0 0 16px;
  display: block;
}

</style>
<script src="./device-info-for-home-component.ts"></script>
