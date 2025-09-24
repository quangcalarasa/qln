import { Component, ElementRef, OnInit, Renderer2 } from '@angular/core';
import { NavigationEnd, NavigationError, RouteConfigLoadStart, Router } from '@angular/router';
import { TitleService, VERSION as VERSION_ALAIN } from '@delon/theme';
import { environment } from '@env/environment';
import { NzModalService } from 'ng-zorro-antd/modal';
import { VERSION as VERSION_ZORRO } from 'ng-zorro-antd/version';

import { getMessaging, getToken, onMessage } from "firebase/messaging";

@Component({
  selector: 'app-root',
  template: ` <router-outlet></router-outlet> `
})
export class AppComponent implements OnInit {
  constructor(
    el: ElementRef,
    renderer: Renderer2,
    private router: Router,
    private titleSrv: TitleService,
    private modalSrv: NzModalService
  ) {
    renderer.setAttribute(el.nativeElement, 'ng-alain-version', VERSION_ALAIN.full);
    renderer.setAttribute(el.nativeElement, 'ng-zorro-version', VERSION_ZORRO.full);
  }

  ngOnInit(): void {
    // this.requestPermission();
    // this.listen();

    let configLoad = false;
    this.router.events.subscribe(ev => {
      if (ev instanceof RouteConfigLoadStart) {
        configLoad = true;
      }
      if (configLoad && ev instanceof NavigationError) {
        this.modalSrv.confirm({
          nzTitle: `Thông báo`,
          nzContent: environment.production ? `Cập nhật phiên bản mới` : `Lỗi：${ev.url}`,
          nzCancelDisabled: false,
          nzOkText: 'Đồng ý',
          nzCancelText: 'Bỏ qua',
          nzOnOk: () => location.reload()
        });
      }
      if (ev instanceof NavigationEnd) {
        this.titleSrv.setTitle();
        this.modalSrv.closeAll();
      }
    });

    //Subcribe focus tab on Browser
    // document.addEventListener("visibilitychange", function() {
    //   document.title = document.hidden ? "I'm away" : "I'm here";
    // });
  }

  requestPermission() {
    const messaging = getMessaging();

    getToken(messaging, 
     { vapidKey: environment['firebase'].vapidKey}).then(
       (currentToken) => {
         if (currentToken) {
           console.log(currentToken);
         } else {
         }
     }).catch((err) => {
    });
  }

  listen() {
    const messaging = getMessaging();
    onMessage(messaging, (payload: any) => {
      console.log('Message received. ');
    });
  }
}
