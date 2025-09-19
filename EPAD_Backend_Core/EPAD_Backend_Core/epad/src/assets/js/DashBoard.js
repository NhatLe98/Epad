var bot = new Vue({
    data() {
        return {
            value1: ''
        };
    },
    methods: {
        refresh: function (event) {
            let html = "";
            var at = document.getElementById("dteAt").value;
            if(at == ''){
                var d = new Date();
                at = getFormattedDate(d);
            }else{
                at = getFormattedDateForEl(at);
            }
            //alert(at);
            $.ajax({
            contentType: "text/plain",
            dataType: "json",
            url: "https://localhost:44330/api/AttendanceLogs/GetAttendanceLogByDay/"+at,
            data: '',
            method: "Get",
            success: response => {
                for(let i = 0 ; i < response.length; i++){
                    html += "<tr>";
                    html+= "<td>"+normalizeDate(response[i].date)+"</td>";
                    html+= "<td>"+response[i].name+"</td>";
                    html+= "<td>"+response[i].in.hours+":"+response[i].in.minutes+":"+response[i].in.seconds+"</td>";
                    html+= "<td>"+response[i].out.hours+":"+response[i].out.minutes+":"+response[i].out.seconds+"</td>";
                    html += "</tr>";
                }
                document.getElementById("tblAtt").innerHTML = html;
            },
            error: err => console.log('Error')
            });
        }
    }
}).$mount("#bot")