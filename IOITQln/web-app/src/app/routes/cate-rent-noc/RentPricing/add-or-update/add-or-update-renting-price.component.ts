import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { RentingPriceRepository } from 'src/app/infrastructure/repositories/renting-price.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { TypeQD, LevelBlock } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-add-or-update-renting-price',
  templateUrl: './add-or-update-renting-price.component.html'
})
export class AddOrUpdateRentingPriceComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeQD = TypeQD;
  levelblockmap_data = LevelBlock;
  role = this.commonService.CheckAccessKeyRole(AccessKey.RENTING_PRICE_MANAGEMENT);
  @Input() record: NzSafeAny;
  @Input() typehouse_data: NzSafeAny;
  @Input() decree_typies: NzSafeAny;
  @Input() unit_price_data: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private rentingPriceRepository: RentingPriceRepository, private commonService: CommonService,) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      UnitPriceId: [this.record ? this.record.UnitPriceId : undefined, [Validators.required]],
      TypeQD: [this.record ? this.record.TypeQD.toString() : undefined, [Validators.required]],
      TypeBlockId: [this.record ? this.record.TypeBlockId : undefined, [Validators.required]],
      LevelId: [this.record ? this.record.LevelId.toString() : undefined, [Validators.required]],
      Price: [this.record ? this.record.Price : undefined, [Validators.required]],
      EffectiveDate: [this.record ? convertDate(this.record.EffectiveDate) : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined]
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    console.log(data);
    const resp = data.Id ? await this.rentingPriceRepository.update(data) : await this.rentingPriceRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }

  close(): void {
    this.drawerRef.close();
  }
}
