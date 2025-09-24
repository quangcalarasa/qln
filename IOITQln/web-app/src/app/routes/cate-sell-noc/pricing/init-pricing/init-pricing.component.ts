import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { ApartmentRepository } from 'src/app/infrastructure/repositories/apartment.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { TypeReportApply } from 'src/app/shared/utils/consts';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { TypeBlockEntityEnum, TypeApartmentEntityEnum, TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { PricingRepository } from 'src/app/infrastructure/repositories/pricing.repository';

@Component({
    selector: 'app-init-pricing',
    templateUrl: './init-pricing.component.html'
})
export class InitPricingComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() typehouse_data: NzSafeAny;

    // decree_type1_data = Decree;
    // termapply_data = TermApply;
    typeReportApply_data = TypeReportApply;
    block_data: any[] = [];
    apartment_data: any[] = [];
    pricing_data: any[] = [];

    data: any;
    block: any;

    loadingDataBlock = false;

    TypeReportApplyEnum = TypeReportApplyEnum;
    code: string;
    isProcessSearch = false;

    constructor(
        private fb: FormBuilder,
        private blockRepository: BlockRepository,
        private apartmentRepository: ApartmentRepository,
        private modal: NzModalRef,
        private cdr: ChangeDetectorRef,
        private pricingRepository: PricingRepository
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            // DecreeType1Id: [undefined, [Validators.required]],
            // TermApply: [undefined, [Validators.required]],
            TypeBlockId: [undefined, [Validators.required]],
            TypeReportApply: [undefined, [Validators.required]],
            BlockId: [undefined, [Validators.required]],
            ApartmentId: [undefined, [Validators.required]],
            pricingReplaceds: [undefined, []]
        });
    }

    async submitForm() {
        this.data = { ...this.validateForm.value };
        this.modal.triggerOk();
    }

    close(): void {
        this.modal.close();
    }

    async getBlockData(removeTypeBlockId: boolean) {
        this.block_data = [];
        this.validateForm.get('BlockId')?.setValue(undefined);
        if (removeTypeBlockId)
            this.validateForm.get('TypeBlockId')?.setValue(undefined);

        // let decreeType1Id = this.validateForm.value.DecreeType1Id;
        let typeBlockId = this.validateForm.value.TypeBlockId;
        let typeReportApply = this.validateForm.value.TypeReportApply;


        if (typeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) {
            this.validateForm.get('ApartmentId')?.setValidators(null);
        }
        else {
            this.validateForm.get('ApartmentId')?.setValidators([Validators.required]);
        }

        this.cdr.detectChanges();
        this.validateForm.updateValueAndValidity();

        if (typeBlockId && typeReportApply) {
            let paging: GetByPageModel = new GetByPageModel();
            paging.page_size = 0;
            if (typeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_LIEN_KE) {
                paging.query = `TypeBlockEntity=${TypeBlockEntityEnum.BLOCK_NORMAL} AND TypeBlockId=${typeBlockId} AND TypeReportApply=${typeReportApply}`;
            }
            else {
                paging.query = `TypeBlockEntity=${TypeBlockEntityEnum.BLOCK_NORMAL} AND TypeBlockId=${typeBlockId} AND (TypeReportApply=${TypeReportApplyEnum.NHA_HO_CHUNG} OR TypeReportApply=${TypeReportApplyEnum.NHA_RIENG_LE})`;
            }

            paging.select = "Id,Address,TypeReportApply,Lane,Ward,District,Province,Name";
            
            this.loadingDataBlock = true;
            const resp = await this.blockRepository.getByPage(paging);

            if (resp.meta?.error_code == 200) {
                this.block_data = resp.data;
            }

            this.loadingDataBlock = false;
        }

        if (typeReportApply != TypeReportApplyEnum.NHA_RIENG_LE) {
            this.getApartmentData();
        }
    }

    async getApartmentData() {
        this.apartment_data = [];
        if(!this.isProcessSearch) this.validateForm.get('ApartmentId')?.setValue(undefined);
        this.isProcessSearch = false;

        let blockId = this.validateForm.value.BlockId;
        this.block = this.block_data.find(x => x.Id == blockId);
        let typeReportApply = this.validateForm.value.TypeReportApply;

        if ((typeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_LIEN_KE && this.block?.TypeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) || typeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) {
            this.validateForm.get('ApartmentId')?.setValidators(null);
        }
        else {
            this.validateForm.get('ApartmentId')?.setValidators([Validators.required]);
        }

        this.validateForm.get('ApartmentId')?.updateValueAndValidity();

        if (this.block) {
            let paging: GetByPageModel = new GetByPageModel();
            paging.page_size = 0;
            paging.query = `BlockId=${blockId}`;
            paging.select = "Id,Address,Code";

            const resp = await this.apartmentRepository.getByPage(paging);

            if (resp.meta?.error_code == 200) {
                this.apartment_data = resp.data;
            }
        }

        this.getPricingReplaceds();
    }

    async getPricingReplaceds() {
        this.pricing_data = [];
        this.validateForm.get('pricingReplaceds')?.setValue(undefined);

        let typeReportApply = this.validateForm.value.TypeReportApply;
        if (typeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) {
            let blockId = this.validateForm.value.BlockId;

            if (blockId) {
                let paging: GetByPageModel = new GetByPageModel();
                paging.page_size = 0;
                paging.query = `BlockId=${blockId}`;
                paging.select = "Id,DateCreate";
                paging.order_by = "DateCreate Desc";

                const resp = await this.pricingRepository.getByPage(paging);

                if (resp.meta?.error_code == 200) {
                    this.pricing_data = resp.data;
                }
            }
        }
        else {
            let apartmentId = this.validateForm.value.ApartmentId;

            if (apartmentId) {
                let paging: GetByPageModel = new GetByPageModel();
                paging.page_size = 0;
                paging.query = `ApartmentId=${apartmentId}`;
                paging.select = "Id,DateCreate";
                paging.order_by = "DateCreate Desc";

                const resp = await this.pricingRepository.getByPage(paging);

                if (resp.meta?.error_code == 200) {
                    this.pricing_data = resp.data;
                }
            }
        }
    }

    getInfoApartment(id: number) {
        let apa = this.apartment_data.find(x => x.Id == id);
        return apa ? `${apa.Address ?? ""}(${apa.Code})` : ""; 
    }

    async SearchByCode() {
        if(this.code == undefined || this.code == "") {
            return;
        }
        else {
            const resp = await this.pricingRepository.SearchByCode(this.code);
            if (resp.meta?.error_code == 200) {
                let data = resp.data;

                this.validateForm.get('TypeReportApply')?.setValue(data.TypeReportApply.toString());
                this.validateForm.get('TypeBlockId')?.setValue(data.TypeBlockId);
                this.validateForm.get('BlockId')?.setValue(data.Id);
                this.validateForm.get('ApartmentId')?.setValue(data.child?.Id);
                this.isProcessSearch = true;
                this.validateForm.get('pricingReplaceds')?.setValue(undefined);
            }
            else {
                this.validateForm.get('TypeReportApply')?.setValue(undefined);
                this.validateForm.get('BlockId')?.setValue(undefined);
                this.validateForm.get('TypeBlockId')?.setValue(undefined);
                this.validateForm.get('ApartmentId')?.setValue(undefined);
                this.validateForm.get('pricingReplaceds')?.setValue(undefined);
            }
        }
    }
}
