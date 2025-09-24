import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
  providedIn: 'root'
})
export class BlockRepository {
  public baseUrl = '/api/block';

  constructor(private baseHttpClient: BaseHttpClient) {}

  public getByPage(params: GetByPageModel, decree?: number) {
    return this.baseHttpClient.getExtraResponseRequest({
      url: `${this.baseUrl}/GetByPage${objectToQueryString(params)}&decree=${decree ?? ''}`
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

  public getById(id: number) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/${id}`
    });
  }

  public checkCodeStatus(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/checkCodeStatus`,
      body: param
    });
  }

  public getBlockByDecree(param: number[]) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/getBlockByDecree`,
      body: param
    });
  }

  public getReportType2() {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetTypeReport2`
    });
  }
}
