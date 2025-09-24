import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { SettingsService, User } from '@delon/theme';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private router: Router, private settings: SettingsService) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    let url: string = state.url;
    let valid = this.invalidUrl(url);
    return valid;
  }

  invalidUrl(url: string) {
    let menuData = this.settings.user['listMenus'];
    // let subSystem = this.settings.user['subSystem'];

    let arr = new Array();

    for (var i = 0; i < menuData.length; i++) {
      this.checkUrlPermission(arr, undefined, menuData[i]);
    }

    let invalid = false;
    url = url.split("?")[0];
    let menu = arr.find(x => x.FullUrl == url);
    if (menu) {
      // if (menu.ActiveKey.indexOf('1', 7) == 7 && (menu.SubSystem == subSystem || menu.SubSystem == 1)) {
      //   invalid = true;
      // }
      if (menu.ActiveKey.indexOf('1', 7) == 7) {
        invalid = true;
      }
    }

    if (!invalid) this.router.navigate(['/exception/404']);

    return invalid;

    let arrayCustom = url.split('?');
    arrayCustom = arrayCustom[0].split('/');

    let lastIdex = arrayCustom[arrayCustom.length - 1];
    if (!isNaN(Number(lastIdex))) {
      arrayCustom[arrayCustom.length - 1] = '0';
    }

    let arrayCustom_Lv2 = arrayCustom.join();
    let urlCheck = arrayCustom_Lv2.replace(new RegExp(',', 'g'), '/');

    if (arr.indexOf(urlCheck) == -1) {
      this.router.navigate(['/exception/404']);
      return false;
    } else {
      return true;
    }
  }

  checkUrlPermission(arr: any, urlParent: any, item: any): void {
    let url = '';
    if (item['IsSpecialFunc'] == true) {
      // url = urlParent == undefined ? "/" + item["Url"] + "/0" : urlParent.replace("/0", "") + "/" + item["Url"] + "/0";
      url = urlParent == undefined ? '/' + item['Url'] : urlParent.replace('/0', '') + '/' + item['Url'];
    } else {
      url = urlParent == undefined ? '/' + item['Url'] : urlParent.replace('/0', '') + '/' + item['Url'];
    }

    item.FullUrl = url;

    arr.push(item);
    if (item['listMenus'].length > 0) {
      for (var i = 0; i < item['listMenus'].length; i++) {
        this.checkUrlPermission(arr, url, item['listMenus'][i]);
      }
    }
  }
}
