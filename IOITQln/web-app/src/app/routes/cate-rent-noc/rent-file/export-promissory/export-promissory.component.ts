import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { PromissoryRepository } from 'src/app/infrastructure/repositories/Promissory.repository';
import { SettingsService } from '@delon/theme';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { RentFileRepository } from 'src/app/infrastructure/repositories/rent-fille.repositories';
import { DebtsRepository } from 'src/app/infrastructure/repositories/Debts.repository';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-rent-file-export-promissory',
    templateUrl: './export-promissory.component.html',
    styles: []
})
export class ExportPromissoryComponent implements OnInit {
    @Input() code: string;
    loading: boolean = false;
    validateForm!: FormGroup;

    data:any[] = [];
    update: boolean = false;

    constructor(
        private fb: FormBuilder,
        private message: NzMessageService,
        private modalSrv: NzModalService,
        private drawerRef: NzDrawerRef<string>,
        private commonService: CommonService,
        private promissoryRepository: PromissoryRepository,
        private settings: SettingsService,
        private rentFileRepository: RentFileRepository,
        private debtsRepository: DebtsRepository,
        private drawerService: NzDrawerService
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            FromDate: [undefined, [Validators.required]],
            ToDate: [undefined, [Validators.required]],
            Update: undefined,
            Vat: undefined
          });
    }

    close(): void {
        this.drawerRef.close();
    }
    
    async view() {
        let data = { ...this.validateForm.value };
        this.data = [];
        this.loading = true;
        const resp = await this.rentFileRepository.getPromissoryReport(data);
        if (resp.meta?.error_code == 200) {
            this.data = resp.data;
            this.update = false;
            this.loading = false;
        }
    }

    async export() {
        this.rentFileRepository.exportPromissoryReport(this.data);
    }

    async save() {
        const resp = await this.rentFileRepository.updatePromissoryReport(this.data);
        if (resp.meta?.error_code == 200) {
            this.update = true;
            this.message.create('success', `Lưu thông tin thành công!`);
        }
        else {
            this.message.create('error', `Lưu thông tin thất bại!`);
        }
    }
}
