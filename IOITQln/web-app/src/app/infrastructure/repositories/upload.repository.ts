import { Injectable } from '@angular/core';
import { BaseHttpClient } from '../http/base-http-client';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';

@Injectable({
  providedIn: 'root',
})
export class UploadRepository {
  public baseUrl = "/api/upload";

  constructor(private baseHttpClient: BaseHttpClient) { }

  public async uploadImage(param: any) {
    return this.baseHttpClient.uploadRequest({
      url: `${this.baseUrl}/uploadImage`,
      body: param
    });
  }

  public async uploadFile(param: any) {
    return this.baseHttpClient.uploadRequest({
      url: `${this.baseUrl}/uploadFile`,
      body: param
    });
  }

  public async downloadFile(url: string) {
    return this.baseHttpClient.exportFileWord({
      url: `${this.baseUrl}/downloadFile`,
      body: {
        FileName: url
      }
    });
  }

  public async downloadFileExcelTemplate(type: ImportHistoryTypeEnum) {
    return this.baseHttpClient.exportFileWord({
      url: `${this.baseUrl}/downloadFileExcelTemplate/${type}`,
      body: {
        FileName: undefined
      }
    });
  }

  public async downloadFileExcelHistory(id: number) {
    return this.baseHttpClient.exportFileWord({
      url: `${this.baseUrl}/downloadFileExcelHistory/${id}`,
      body: {
        FileName: undefined
      }
    });
  }
}
