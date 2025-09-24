import { ACLService } from '@delon/acl';
import { Component } from '@angular/core';
import { SettingsService, User } from '@delon/theme';
import { LayoutDefaultOptions } from '@delon/theme/layout-default';
import { environment } from '@env/environment';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';

@Component({
  selector: 'layout-basic',
  styleUrls: [`basic.component.less`],
  templateUrl: `basic.component.html`,
})
export class LayoutBasicComponent {
  options: LayoutDefaultOptions = {
    logoExpanded: `./assets/img/logo.png`,
    logoCollapsed: `./assets/img/logo.png`,
  };
  searchToggleStatus = false;
  showSettingDrawer = !environment.production;
  isCollapsed = false;
  menuData: any[] = [];
  subSystem: number = 2;
  radioValue?: string;

  linkCe: string = environment["linkCe"];
  breadcrumbData : any[];
  get user(): User {
    return this.settings.user;
  }

  constructor(private settings: SettingsService, private router: Router) {
    this.menuData = settings.user["listMenus"];
    this.subSystem = settings.user["subSystem"];
    this.radioValue = settings.user["subSystem"] ? settings.user["subSystem"].toString() : undefined;

    this.router.events.subscribe((val ) => {
      if(val instanceof NavigationEnd) {
        this.breadcrumbData = this.getBreadcrumb();
      }
    });
  }

  isSelected(route: string): boolean {
    if (this.router.url.includes(route)) {
      return true;
    }
    return false;
  }

  changeSubSystem(subSystem: any) {
    let user = this.settings.user;
    this.settings.setUser({ ...user, subSystem });

    let url = this.router.url;
    window.location.href = url;
  }

  getBreadcrumb() {
    let arrayUrl = this.router.url.split("/");
    return this.check(arrayUrl[1]?.split("?")[0], arrayUrl[2]?.split("?")[0], arrayUrl[3]?.split("?")[0]);
  }

  check(firstRouterUrl: string, secondRouterUrl?: string, thirdRouterUrl?: string) {
    let res = [];
    if(thirdRouterUrl != undefined) {
      let first = this.menuData.find(x => x.Url == firstRouterUrl);
      let second = first.listMenus.find((x:any) => x.Url == secondRouterUrl);
      let third = second.listMenus.find((x:any) => x.Url == thirdRouterUrl);
      res = [first.Name, second.Name, third.Name];
    }
    else if(secondRouterUrl != undefined) {
      let first = this.menuData.find(x => x.Url == firstRouterUrl);
      let second = first.listMenus.find((x:any) => x.Url == secondRouterUrl);
      res = [first.Name, second.Name];
    }
    else {
      let first = this.menuData.find(x => x.Url == firstRouterUrl);
      res = [first.Name];
    }

    return res;
  }
}
