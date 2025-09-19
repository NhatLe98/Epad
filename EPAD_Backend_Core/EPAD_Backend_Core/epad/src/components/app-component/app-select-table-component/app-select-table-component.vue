<template>
  <app-select-new :dataSource="listShift" @change="change" v-model="value" displayMember="Name" valueMember="Index"></app-select-new>
</template>

<script>
 // Import your API module here
 import { taShiftApi } from '@/$api/ta-shift-api';
export default {
  listShift : [],
  value : 0,
  created() {
    this.getShiftList(); // Call the API method to populate listShift when the component is created
  },
  methods: {
    getShiftList() {
      taShiftApi.GetShiftByCompanyIndex().then((res) => {
        if (res.status && res.status == 200 && res.data && res.data.length > 0) {
          this.listShift = res.data;
          console.log("taShiftApi.GetShiftByCompanyIndex ~ listShift:",  res.data)
        }
      }).catch((error) => {
        console.error('Error fetching shift list:', error);
      });
    },

    change(){
      console.log('listShift', this.listShift)
    },

    getValue() {
      return this.value;
    },

    onKeyDown(event) {
      if (event.key === "Escape") {
        return;
      }
    },

    data() {
    return {
      shiftIndex: 0
    };
  }
  }
};
</script>
