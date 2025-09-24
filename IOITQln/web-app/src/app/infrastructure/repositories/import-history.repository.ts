import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';

@Injectable({
    providedIn: 'root',
})
export class ImportHistoryRepository {
    public baseUrl = "/api/ImportHistory";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public getByPage(params: GetByPageModel, type: ImportHistoryTypeEnum) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetByPage/${type}${objectToQueryString(params)}`,
        });
    }
}
