import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
  providedIn: 'root'
})
export class Md167FileRepository {
  public baseUrl = '/api/ExportWordContract';

  constructor(private baseHttpClient: BaseHttpClient) { }

  public getByPage(params: GetByPageModel) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetByPage${objectToQueryString(params)}`
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

  public async GetExportContract1(id: number) {
    return this.baseHttpClient.exportFileWord({
      url: `/api/ExportWordContract/GetExportContract1/${id}`,
      body: undefined
    });
  }
  public async GetExportContract2(id: number) {
    return this.baseHttpClient.exportFileWord({
      url: `/api/ExportWordContract/GetExportContract2/${id}`,
      body: undefined
    });
  }

  public async GetExportContract3(id: number) {
    return this.baseHttpClient.exportFileWord({
      url: `/api/ExportWordContract/GetExportContract3/${id}`,
      body: undefined
    });
  }

  public async GetExportContract4(id: number) {
    return this.baseHttpClient.exportFileWord({
      url: `/api/ExportWordContract/GetExportContract4/${id}`,
      body: undefined
    });
  }

  public async GetExportContract5(id: number) {
    return this.baseHttpClient.exportFileWord({
      url: `/api/ExportWordContract/GetExportContract5/${id}`,
      body: undefined
    });
  }
}
