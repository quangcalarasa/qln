import { Component, OnInit, Input } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { TypeEditHistoryEnum } from 'src/app/shared/utils/enums';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { EditHistoryRepository } from 'src/app/infrastructure/repositories/edit-history.repository';

@Component({
    selector: 'app-shared-confirm-update-list',
    templateUrl: './confirm-update-list.component.html'
})
export class SharedConfirmUpdateListComponent implements OnInit {
    @Input() TypeEditHistoryEnum: TypeEditHistoryEnum;
    @Input() targetId: number;

    data: any[] = [];
    paging: GetByPageModel = new GetByPageModel();

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Nội dung sửa', index: 'ContentUpdate' },
        { title: 'Lý do sửa', index: 'ReasonUpdate' },
        {
            title: 'Tờ trình', width: 100, className: 'text-center', buttons: [{
                icon: 'download',
                click: record => this.downloadFile(record.AttactmentUpdate),
                tooltip: 'Tải tờ trình'
            }]
        },
        { title: 'Người sửa', index: 'CreatedBy', width: 100 },
        { title: 'Thời gian sửa', index: 'CreatedAt', type: 'date', width: 120, className: 'text-center', dateFormat: 'dd/MM/yyyy HH:mm' },
    ];
    constructor(
        private fb: FormBuilder,
        private modal: NzModalRef,
        private uploadRepository: UploadRepository,
        private editHistoryRepository: EditHistoryRepository
    ) { }

    ngOnInit(): void {
        this.getData();
    }

    tableRefChange(e: STChange): void {
        switch (e.type) {
            case 'pi':
                this.paging.page = e.pi;
                this.getData();
                break;
            default:
                break;
        }
    }

    async getData() {
        this.paging.page_size = 10;
        this.paging.query = this.TypeEditHistoryEnum ==  TypeEditHistoryEnum.RENT_CONTRACT ? `RentFileId.ToString().Contains("${this.targetId}")` : `TargetId=${this.targetId}`;
        this.paging.order_by = this.paging.order_by ? this.paging.order_by : 'CreatedAt Desc';

        try {
            const resp = await this.editHistoryRepository.getByPage(this.paging, this.TypeEditHistoryEnum);

            if (resp.meta?.error_code == 200) {
                this.data = resp.data;
                this.paging.item_count = resp.metadata;
            }
        } catch (error) {
            throw error;
        } finally {}
    }

    close(): void {
        this.modal.close();
    }

    downloadFile(fileName: string) {
        this.uploadRepository.downloadFile(fileName);
    }
}
