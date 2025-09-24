import { HttpClient } from '@angular/common/http';
import { Injectable, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { ACLService } from '@delon/acl';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { MenuService, SettingsService, TitleService } from '@delon/theme';
import type { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzIconService } from 'ng-zorro-antd/icon';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

import { ICONS } from '../../../style-icons';
import { ICONS_AUTO } from '../../../style-icons-auto';

/**
 * Used for application startup
 * Generally used to get the basic data of the application, like: Menu Data, User Data, etc.
 */
@Injectable()
export class StartupService {
  constructor(
    iconSrv: NzIconService,
    private menuService: MenuService,
    private settingService: SettingsService,
    private aclService: ACLService,
    private titleService: TitleService,
    @Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService,
    private httpClient: HttpClient,
    private router: Router
  ) {
    iconSrv.addIcon(...ICONS_AUTO, ...ICONS);
  }


  private viaHttp(): Observable<void> {
    return this.httpClient.get('assets/tmp/app-data.json').pipe(
      catchError((res: NzSafeAny) => {
        console.warn(`StartupService.load: Network request failed`, res);
        setTimeout(() => this.router.navigateByUrl(`/exception/500`));
        return of({});
      }),
      map((res: NzSafeAny) => {
        // Application information: including site name, description, year
        this.settingService.setApp(res.app);
        // User information: including name, avatar, email address
        this.settingService.setUser(res.user);
        // ACL: Set the permissions to full, https://ng-alain.com/acl/getting-started
        this.aclService.setFull(true);
        // Menu data, https://ng-alain.com/theme/menu
        this.menuService.add(res.menu);
        // Can be set page suffix title, https://ng-alain.com/theme/title
        this.titleService.suffix = res.app.name;
      })
    );
  }

  private viaMock(): Observable<void> {
    // const tokenData = this.tokenService.get();
    // if (!tokenData.token) {
    //   this.router.navigateByUrl(this.tokenService.login_url!);
    //   return;
    // }
    // mock
    const app: any = {
      // name: `His`,
      description: `Tiktok`
    };

    this.settingService.setApp(app);
    // User information: including name, avatar, email address
    // this.settingService.setUser(user);
    // ACL: Set the permissions to full, https://ng-alain.com/acl/getting-started
    this.aclService.setFull(true);
    // Menu data, https://ng-alain.com/theme/menu
    // this.menuService.add([
    //   {
    //     text: 'Main',
    //     group: true,
    //     children: [
    //       {
    //         text: 'Dashboard',
    //         link: '/dashboard',
    //         icon: { type: 'icon', value: 'appstore' }
    //       }
    //     ]
    //   }
    // ]);
    // Can be set page suffix title, https://ng-alain.com/theme/title
    this.titleService.suffix = app.name;

    // if (this.settingService.user?.role) {
    //   const bytes = CryptoJS.AES.decrypt(this.settingService.user?.role, ROLE_SECRETKEY);
    //   const role = bytes.toString(CryptoJS.enc.Utf8);

    //   this.aclService.setRole([role]);
    // }

    window.addEventListener('storage', (event) => {
      // Nếu xóa access_token thì chuyển đến trang login
      if (event.key === '_token') {
        if (event.oldValue && !event.newValue) {
          window.location.href = this.tokenService.login_url!;
        } else if (!event.oldValue && event.newValue) {
          window.location.href = '/';
        }
      }
    });

    return of();
  }

  load(): Observable<void> {
    // return this.viaHttp();
    return this.viaMock();
  }
}
