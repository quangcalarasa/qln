import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
import { ImportHistoryTypeEnum, TypeDecree } from 'src/app/shared/utils/enums';
import { Router, ActivatedRoute } from '@angular/router';
import { AddOrUpdateIngredientsPriceComponent } from './add-or-update/add-or-update-ingredientsprice.component';
import { IngredientsPriceRepository } from 'src/app/infrastructure/repositories/ingredients-price.repository';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';

@Component({
    selector: 'app-ingredientsprice',
    templateUrl: './ingredientsprice.component.html',
    styles: [
    ]
})
export class IngredientspriceComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    data2: any[] = [];
    loading = false;

    titleString?: string = 'thành phần giá bán cấu thành';

    columns: STColumn[];

    constructor(
        private fb: FormBuilder,
        private modalSrv: NzModalService,
        private ingredientsPriceRepository: IngredientsPriceRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private router: Router
    ) {
        const curentUrl = this.router.url;

        this.columns = [
            { title: 'Stt', type: 'no', width: 40 },
            { title: 'Mã thành phần', index: 'Code' },
            { title: 'Tên thành phần', index: 'Name' },
            { title: 'Diễn giải', index: 'Note' },
            {
                title: 'Chức năng',
                width: 100,
                className: 'text-center',
                buttons: [
                    {
                        icon: 'edit',
                        iif: i => !i.edit,
                        click: record => this.addOrUpdate(record)
                    },
                    {
                        icon: 'delete',
                        type: 'del',
                        pop: {
                            title: `Bạn có chắc chắn muốn xoá ${this.titleString} này?`,
                            okType: 'danger',
                            icon: 'star'
                        },
                        click: record => this.delete(record)
                    }
                ]
            }
        ];
    }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        this.getData();
    }

    tableRefChange(e: STChange): void {
        switch (e.type) {
            case 'pi':
                this.paging.page = e.pi;
                this.getData();
                break;
            case 'dblClick':
                this.addOrUpdate(e.dblClick?.item);
                break;
            case 'checkbox':
                break;
            default:
                break;
        }
    }

    reset(): void {
        this.query = new QueryModel();
        this.paging.page = 1;
        this.getData();
    }

    searchData() {
        this.paging.page = 1;
        this.getData();
    }

    async getData() {
        this.paging.query = '1=1';
        this.paging.order_by = 'CreatedAt Desc';

        if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
            if (this.query.txtSearch.trim() != '')
                this.paging.query += ` and (Code.Contains("${this.query.txtSearch}") or Name.Contains("${this.query.txtSearch}") or Note.Contains("${this.query.txtSearch}"))`;
        }

        try {
            this.loading = true;
            const resp = await this.ingredientsPriceRepository.getByPage(this.paging);

            if (resp.meta?.error_code == 200) {
                this.data = resp.data;
                this.paging.item_count = resp.metadata;
            } else {
                this.modalSrv.error({
                    nzTitle: 'Không lấy được dữ liệu.'
                });
            }
        } catch (error) {
            throw error;
        } finally {
            this.loading = false;
        }
    }

    addOrUpdate(record?: any): void {
        let add;
        record ? add = true : add = false
        localStorage.setItem('add', add.toString())
        const drawerRef = this.drawerService.create<AddOrUpdateIngredientsPriceComponent>({
            nzTitle: record ? `Sửa ${this.titleString} : ${record.Code}` : `Thêm mới ${this.titleString}`,
            // record.khoa_chinh
            nzWidth: '60vw',
            nzContent: AddOrUpdateIngredientsPriceComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record,
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa ${this.titleString} ${data.Code} thành công!` : `Thêm mới ${this.titleString} ${data.Code} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.ingredientsPriceRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa ${this.titleString} ${data.Code} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }
    async csvExport() {
        const resp = await this.ingredientsPriceRepository.ExportExcel();
      }
    
    onBack() {
        window.history.back();
    }
    
    import() {
        this.drawerService.create<SharedImportExcelComponent>({
            nzTitle: `Import excel`,
            nzWidth: '85vw',
            nzPlacement: 'left',
            nzContent: SharedImportExcelComponent,
            nzContentParams: {
                importHistoryType: ImportHistoryTypeEnum.TdcIngredientsPrice
            }
        });
      }
}
