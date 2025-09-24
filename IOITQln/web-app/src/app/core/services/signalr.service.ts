import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr"
import { TypeSignalRNotify } from 'src/app/shared/utils/enums';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { UserService } from 'src/app/core/services/user.service';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {


  private hubConnection!: signalR.HubConnection;

  constructor(private nzNotificationService: NzNotificationService, private userService: UserService) {

  }

  public startConnection = (userId: string) => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/notify', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .build();
    this.hubConnection
      .start()
      .then(() => {
        this.hubConnection.invoke("SuscribeToUser", userId).then(() => {
          return console.log("SuscribeToUser");
        }).catch(function (err) {
          return console.error(err.toString());
        });
      })
      .catch(err => console.log('Error while starting connection: ' + err));
  }

  public addProductListner = () => {
    this.hubConnection.on('BroadcastMessage', (notification: any) => {
      if(notification.type == TypeSignalRNotify.LOG_OUT) {
        this.nzNotificationService.create('info', notification.title, notification.contents);
        new Promise(f => setTimeout(f, 5000)).then(() => { this.userService.logout() });
      }
    });
  }
}