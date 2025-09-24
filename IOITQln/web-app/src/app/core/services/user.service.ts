import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ACLService } from '@delon/acl';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { SettingsService } from '@delon/theme';
import * as CryptoJS from 'crypto-js';
import ResponseModel from "../models/reponse-model";
import { UserRepository } from '../../infrastructure/repositories/user.repository';
import { KeycloakService } from 'keycloak-angular';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  constructor(
    private authenRepository: UserRepository,
    private settingService: SettingsService,
    @Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService,
    private aclService: ACLService,
    private router: Router,
    private keycloakService: KeycloakService
  ) { }

  public registerAccount(param: any) {
    return this.authenRepository.registerAccount(param);
  }

  public async login(param: any, isLoginViaSso: boolean) {
    const { password, username, token } = param;
    const passwordEncryp = CryptoJS.MD5(password).toString();

    const resp: ResponseModel = !isLoginViaSso ? await this.authenRepository.login({
      username: username, password: passwordEncryp
    }) || {} : await this.authenRepository.loginViaSso({
      username: username, token: token
    }) || {};
    const { meta, data } = resp;

    let subSystem = this.settingService.user["subSystem"];

    if (meta?.error_code == 200) {
      const { roleLevel, access_key, access_token, ...user } = data;

      this.settingService.setUser({ ...param, roleLevel, ...user, password: passwordEncryp, subSystem });
      this.tokenService.set({
        token: data?.AccessToken,
        refresh_token: data?.refresh_token,
        expired: +new Date() + 1000 * 60 * 60 * 59
      });

      this.aclService.setRole([roleLevel + '']);
    }

    let url = this.getFirstRoute(this.settingService.user["listMenus"]);
    // console.log(url);

    return { code: meta?.error_code == 200 ? 0 : -1, subSystem: subSystem, url: url };
  }

  public async refreshTokenRequest() {
    const userJSON = localStorage.getItem('user');
    if (userJSON) {
      const user = JSON.parse(userJSON);
      const { username, password } = user;

      const resp: any = (await this.authenRepository.login({ email: username, password: password })) || {};

      if (resp?.data && resp?.data?.access_token) {
        return { token: resp?.data?.access_token, refresh_token: resp?.data?.refresh_token, expired: +new Date() + 1000 * 23 * 60 * 58 };
      } else {
        throw Error();
      }
    }
    return null;
  }

  public logout() {
    this.authenRepository.logout();
    this.tokenService.clear();
    localStorage.removeItem('user');
    this.settingService.setUser({});
    this.keycloakService.logout();
    // this.aclService.setRole([]);
    // this.settingService.setUser(undefined);
    // this.router.navigateByUrl('/login');
  }

  public getLoggedUser() {
    let user = this.settingService.user;

    return user;
  }

  public getFirstRoute(menuData: any[]) {
    let url = '';

    if (menuData) {
      let firstMenu = menuData.length > 0 ? menuData[0] : undefined;
      if (firstMenu) {
        url += '/' + firstMenu.Url;
        url += this.getFirstRoute(firstMenu.listMenus);
      }
    }

    return url;
  }
}
