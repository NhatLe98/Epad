import { PUSH_NOTIFICATION_URL } from "@/$core/config";
import i18n from "@/i18n";
import { fDateTime } from "@/utils/datetime-utils";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import Vue from 'vue';

const vue = new Vue();

export class SignalRConnection {
    static connection: HubConnection = new HubConnectionBuilder()
        .withUrl(PUSH_NOTIFICATION_URL + "/notificationHub")
        .configureLogging(LogLevel.None)
        .withAutomaticReconnect()
        .build();
}


export function setupHubConnection() {
    // receive data from server
    type ReceiveNotificationItem = {
        serialNumber: string,
        result: string,
        ipAddress: string,
        error: string,
        detail: any,
    }
    
    SignalRConnection.connection.on("ReceiveNotification", (lsMessage: ReceiveNotificationItem[]) => {

        let message = '';
        for (let i = 0; i < lsMessage.length; i++) {
            const msgItem = lsMessage[i];

            if (msgItem.detail) {
                message += i18n.t('DeviceNotificationMessage', {
                    ipAddress: msgItem.ipAddress,
                    createdTime: fDateTime(msgItem.detail.CreatedDate),
                    command: i18n.t(msgItem.detail.Command),
                    success: msgItem.detail.Success,
                    fail: msgItem.detail.Fail,
                }).toString() + '<br />';
            } else {
                message += lsMessage[i]["ipAddress"] + ': ' + i18n.t(lsMessage[i]["result"]) + ` ${i18n.t((lsMessage[i] as any).erorr ?? '')}` + '<br/>';
            }
        }
        message = `<p class="notify-content">${message}</p>`;

        vue.$notify({
            type: Boolean(lsMessage.find(x => x.error) || lsMessage.find(x => x.result.toLowerCase().includes('fail'))) ? 'error' : 'success',
            title: i18n.t('NotificationFromDevice').toString(),
            dangerouslyUseHTMLString: true,
            message: message,
            customClass: 'notify-content',
            duration: 8000
        });
    });

    const openConnection = () => {
        SignalRConnection.connection
            .start()
            .then(() => {
                SignalRConnection.connection
                    .invoke("AddUserToGroup", 2, self.localStorage.getItem("user"));
                    console.log("Notification server is connected!");
            })
            .catch(err => {
                console.log('Connect notification server failed: ', err);
                setTimeout(() => {
                    console.log('Try reconnect notification server...');
                    openConnection();
                }, 30000);
            });
    };

    openConnection();

    SignalRConnection.connection.onclose(() => {
        console.log('Connection is closed. Try reconnect notification server...');
        openConnection();
    });
}