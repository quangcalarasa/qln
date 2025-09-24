import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
  providedIn: 'root'
})
export class LandRepository {
  public baseUrl = '/api/land';

  constructor(private baseHttpClient: BaseHttpClient) {}

  public getByPage(params: GetByPageModel) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetByPage/${objectToQueryString(params)}`
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

  public getById(id: number) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/${id}`
    });
  }

  public async delete(param: any) {
    return this.baseHttpClient.deleteRequest({
        url: `${this.baseUrl}/${param.Id}`
    });
  }

  public ExportExcel() {
    return this.baseHttpClient.downloadFile({
      url: `${this.baseUrl}/ExportExcel`
    });
  }

  public async importDataExcel(param: any) {
    return this.baseHttpClient.uploadRequest({
      url: `${this.baseUrl}/ImportDataExcel`,
      body: param
    });
  }
}
