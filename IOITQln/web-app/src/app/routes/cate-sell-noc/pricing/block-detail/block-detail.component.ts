import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { DecreeEnum } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

export interface RowItemLv1 {
  floorId: number;
  floorCode: number;
  floorName: string;
  totalAreaFloor?: number;
  rowItemLv2: RowItemLv2[];
}

export interface RowItemLv2 {
  areaId: number;
  areaName: string;
  isMezzanine: boolean,
  totalAreaDetailFloor?: number;
  generalArea?: number;
  privateArea?: number;
  coefficient_99?: number;
  coefficient_34?: number;
  coefficient_61?: number;
}


@Component({
  selector: 'app-pricing-block-detail',
  templateUrl: './block-detail.component.html'
})
export class BlockDetailComponent implements OnInit {
  @Input() blockDetails?: any[];
  @Input() decreeMaps?: any[];

  listOfData: any[] = [];
  DecreeEnum = DecreeEnum;

  constructor(
    private commonService: CommonService
  ) { }

  ngOnInit(): void {
    this.convertData();
  }

  convertData() {
    this.listOfData = [];

    if (this.blockDetails) {
      this.blockDetails.forEach((item: any) => {
        let floorId = item.FloorId;

        let index = this.listOfData.findIndex(x => x.floorId == floorId);
        if (index == -1) {
          let rowItemLv2_data: RowItemLv2 = {
            areaId: item.AreaId,
            areaName: item.AreaName,
            isMezzanine: item.IsMezzanine,
            totalAreaDetailFloor: item.TotalAreaDetailFloor,
            generalArea: item.GeneralArea,
            privateArea: item.PrivateArea,
            coefficient_99: item.Coefficient_99,
            coefficient_34: item.Coefficient_34,
            coefficient_61: item.Coefficient_61
          };

          let rowItemLv1_data: RowItemLv1 = {
            floorId: floorId,
            floorName: item.FloorName,
            floorCode: item.FloorCode,
            totalAreaFloor: item.TotalAreaFloor,
            rowItemLv2: [rowItemLv2_data],

          };

          this.listOfData.push(rowItemLv1_data);
        }
        else {
          let rowItemLv2_data: RowItemLv2 = {
            areaId: item.AreaId,
            areaName: item.AreaName,
            isMezzanine: item.IsMezzanine,
            totalAreaDetailFloor: item.TotalAreaDetailFloor,
            generalArea: item.GeneralArea,
            privateArea: item.PrivateArea,
            coefficient_99: item.Coefficient_99,
            coefficient_34: item.Coefficient_34,
            coefficient_61: item.Coefficient_61
          };

          this.listOfData[index].rowItemLv2.push(rowItemLv2_data);
        }
      });
    }
  }

  checkDecree(decree: DecreeEnum) {
    return this.commonService.checkDecree(decree, this.decreeMaps);
  }
}
