import { Injectable } from '@angular/core';
import { BaseHttpClient } from '../http/base-http-client';

@Injectable({
    providedIn: 'root',
})
export class GeneralDataImportRepository {
    public baseUrl = "/api/GeneralDataImport";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public async importDataExcel(param: any) {
        return this.baseHttpClient.uploadRequest({
          url: `${this.baseUrl}/ImportDataExcel`,
          body: param
        });
    }
}
