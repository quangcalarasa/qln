import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { TypeEditHistoryEnum} from 'src/app/shared/utils/enums';

@Injectable({
    providedIn: 'root',
})
export class EditHistoryRepository {
    public baseUrl = "/api/EditHistory";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public getByPage(params: GetByPageModel, TypeEditHistoryEnum: TypeEditHistoryEnum) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetByPage/${TypeEditHistoryEnum}${objectToQueryString(params)}`,
        });
    }
}
