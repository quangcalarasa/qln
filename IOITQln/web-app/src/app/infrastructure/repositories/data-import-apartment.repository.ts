import { Injectable } from '@angular/core';
import { BaseHttpClient } from '../http/base-http-client';

@Injectable({
    providedIn: 'root',
})
export class DataImportApartmentRepository {
    public baseUrl = "/api/ImportDataApartment";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public async importDataExcel(param: any) {
        return this.baseHttpClient.uploadRequest({
          url: `${this.baseUrl}/ImportDataExcel`,
          body: param
        });
    }
}
