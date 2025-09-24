import { Injectable } from '@angular/core';
import { BaseHttpClient } from '../http/base-http-client';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
  providedIn: 'root'
})
export class UserRepository {
  public baseUrl = '/api/user';

  constructor(private baseHttpClient: BaseHttpClient) { }

  public registerAccount(param: any) {
    return this.baseHttpClient.postRequest({
      url: '',
      body: {
        param
      }
    });
  }

  public login(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/login`,
      body: param
    });
  }

  public loginViaSso(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/loginViaSso`,
      body: param
    });
  }

  public getAccountInfo(id: number) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/getInfo/${id}`
    });
  }

  public refreshToken(param: any) {
    return this.baseHttpClient.postRequest({
      url: '',
      body: param
    });
  }

  public getByPage(params: GetByPageModel, roleId?: number) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetByPage${objectToQueryString(params)}&roleId=${roleId ?? ''}`
    });
  }

  public getByStatus(status: number) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/Status/${status}`
    });
  }

  public addNew(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}`,
      body: param
    });
  }

  public async update(param: any) {
    return this.baseHttpClient.putRequest({
      url: `${this.baseUrl}/${param.Id}`,
      body: param
    });
  }

  public async delete(param: any) {
    return this.baseHttpClient.deleteRequest({
      url: `${this.baseUrl}/${param.Id}`
    });
  }

  public async changeStatus(param: any, status: number) {
    return this.baseHttpClient.putRequest({
      url: `${this.baseUrl}/changeStatusUser/${param.Id}/${status}`,
      body: undefined
    });
  }

  public async changePassUser(param: any) {
    return this.baseHttpClient.putRequest({
      url: `${this.baseUrl}/changePassUser/${param.Id}`,
      body: param
    });
  }

  public async changePassUserApplyKeycloak(param: any) {
    return this.baseHttpClient.putRequest({
      url: `${this.baseUrl}/changePassUserApplyKeycloak/${param.Id}`,
      body: param
    });
  }

  public async changeInfoUser(param: any, id: number) {
    return this.baseHttpClient.putRequest({
      url: `${this.baseUrl}/changeInfoUser/${id}`,
      body: param
    });
  }

  public async changePass(param: any, id: number) {
    return this.baseHttpClient.putRequest({
      url: `${this.baseUrl}/changePass/${id}`,
      body: param
    });
  }

  public async changePassApplyKeycloak(param: any, id: number) {
    return this.baseHttpClient.putRequest({
      url: `${this.baseUrl}/changePassApplyKeycloak/${id}`,
      body: param
    });
  }

  public async logout() {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/logout`,
      body: undefined
    });
  }

  public ExportExcel() {
    return this.baseHttpClient.exportReport("danh_sach_tai_khoan.xlsx", {
      url: `${this.baseUrl}/ExportExcel`
    });
  }
}
