import { Component, Input, OnChanges, OnInit, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { DecreeEnum } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { DistributionFloorCoefficientRepository } from 'src/app/infrastructure/repositories/distribution-floor-coefficient.repository';
import { ChooseAreaBlockComponent } from '../choose-area/change-area-block.component';
import { NzModalService } from 'ng-zorro-antd/modal';

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
  selector: 'app-block-block-detail',
  templateUrl: './block-detail.component.html'
})
export class BlockDetailComponent implements OnInit, OnChanges {
  @Input() blockDetails?: any[];
  @Input() floorApplyPriceChange: number = 0;
  @Input() specialCase: boolean = false;
  @Input() decreeMaps?: any[];

  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  nodes: any[] = [];
  listOfData: any[] = [];
  DecreeEnum = DecreeEnum;
  distribution_floor_coefficient_data: any[] = [];
  constructor(
    private commonService: CommonService, private distributionFloorCoefficientRepository: DistributionFloorCoefficientRepository, private modalSrv: NzModalService
  ) { }

  ngOnInit(): void {
    this.convertData();
    this.getDistributionFloorCoefficient(this.decreeMaps, false);
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

  ngOnChanges(changes: SimpleChanges) {
    if (changes['decreeMaps']) {
      this.getDistributionFloorCoefficient(this.decreeMaps, true);
    }
    else {
      this.listOfData.forEach((item: any) => {
        item.rowItemLv2.forEach((childItem: any) => {
          childItem.coefficient_99 = this.commonService.getCoefficient(item.floorCode, this.floorApplyPriceChange, childItem.isMezzanine, DecreeEnum.ND_99, this.distribution_floor_coefficient_data, this.specialCase);
          childItem.coefficient_34 = this.commonService.getCoefficient(item.floorCode, this.floorApplyPriceChange, childItem.isMezzanine, DecreeEnum.ND_34, this.distribution_floor_coefficient_data, this.specialCase);
          childItem.coefficient_61 = this.commonService.getCoefficient(item.floorCode, this.floorApplyPriceChange, childItem.isMezzanine, DecreeEnum.ND_61, this.distribution_floor_coefficient_data, this.specialCase);
        });
      });
    }
  }

  changeLevelBlock(nodes: any) {
    if (nodes.length == 0) {
      this.listOfData = [];
    }
    else {
      this.listOfData = [];

      nodes.forEach((node: any) => {
        let valid = false;
        let rowItemLv1: RowItemLv1 = {
          floorId: node.key,
          floorCode: node.floorCode,
          floorName: node.title,
          totalAreaFloor: undefined,
          rowItemLv2: []
        };

        let chidlren = node.children.filter((x: any) => x.checked);
        chidlren.forEach((nodeItem: any, index: number) => {
          let rowItemLv2: RowItemLv2 = {
            areaId: nodeItem.key,
            areaName: nodeItem.title,
            isMezzanine: nodeItem.isMezzanine,
            totalAreaDetailFloor: undefined,
            generalArea: undefined,
            privateArea: undefined,
            coefficient_99: this.commonService.getCoefficient(rowItemLv1.floorCode, this.floorApplyPriceChange, nodeItem.isMezzanine, DecreeEnum.ND_99, this.distribution_floor_coefficient_data, this.specialCase),
            coefficient_34: this.commonService.getCoefficient(rowItemLv1.floorCode, this.floorApplyPriceChange, nodeItem.isMezzanine, DecreeEnum.ND_34, this.distribution_floor_coefficient_data, this.specialCase),
            coefficient_61: this.commonService.getCoefficient(rowItemLv1.floorCode, this.floorApplyPriceChange, nodeItem.isMezzanine, DecreeEnum.ND_61, this.distribution_floor_coefficient_data, this.specialCase),
          };

          rowItemLv1.rowItemLv2.push(rowItemLv2);
          valid = true;
        })

        if (valid) this.listOfData.push(rowItemLv1);
      });
    }
  }

  removeRowItemLv2(index_lv1: number, index_lv2: number) {
    this.listOfData[index_lv1].rowItemLv2.splice(index_lv2, 1);

    if (this.listOfData[index_lv1].rowItemLv2.length == 0) {
      this.listOfData.splice(index_lv1, 1);
    }
    else {
      this.listOfData[index_lv1].totalAreaFloor = this.listOfData[index_lv1].rowItemLv2.reduce((totalAreaFloor: number, currValue: any) => {
        return totalAreaFloor + (currValue.totalAreaDetailFloor ?? 0)
      }, 0);
    }
  }

  getValue() {
    let res: any[] = [];
    this.listOfData.forEach((item: any) => {
      item.rowItemLv2.forEach((itemLv2: any) => {
        res.push({
          FloorId: item.floorId,
          AreaId: itemLv2.areaId,
          TotalAreaFloor: item.totalAreaFloor,
          TotalAreaDetailFloor: itemLv2.totalAreaDetailFloor,
          GeneralArea: itemLv2.generalArea,
          PrivateArea: itemLv2.privateArea,
          Coefficient_99: itemLv2.coefficient_99,
          Coefficient_34: itemLv2.coefficient_34,
          Coefficient_61: itemLv2.coefficient_61
        });
      });
    });

    return res;
  }

  changeValueArea(index_lv1: number, index_lv2: number) {
    let row = this.listOfData[index_lv1].rowItemLv2[index_lv2];
    if (!row.generalArea && !row.privateArea) {
      this.listOfData[index_lv1].rowItemLv2[index_lv2].totalAreaDetailFloor = undefined;
    }
    else {
      let totalAreaDetailFloor = (row.generalArea ?? 0) + (row.privateArea ?? 0);
      this.listOfData[index_lv1].rowItemLv2[index_lv2].totalAreaDetailFloor = totalAreaDetailFloor;
    }

    this.listOfData[index_lv1].totalAreaFloor = this.listOfData[index_lv1].rowItemLv2.reduce((totalAreaFloor: number, currValue: any) => {
      return totalAreaFloor + (currValue.totalAreaDetailFloor ?? 0)
    }, 0);
  }

  checkDecree(decree: DecreeEnum) {
    return this.commonService.checkDecree(decree, this.decreeMaps);
  }

  //lấy ds hệ số phân bổ các tầng theo nghị định
  async getDistributionFloorCoefficient(decreeMaps: any, reCalc: boolean) {
    this.distribution_floor_coefficient_data = [];
    if (decreeMaps.length) {
      let paging: GetByPageModel = new GetByPageModel();
      paging.page_size = 0;
      // paging.query = `DecreeType1Id=${DecreeType1Id}`;
      paging.query = decreeMaps.map((d: any) => "DecreeType1Id=" + (d.key ?? d.DecreeType1Id)).join(' OR ');

      const resp = await this.distributionFloorCoefficientRepository.getByPage(paging);

      if (resp.meta?.error_code == 200) {
        this.distribution_floor_coefficient_data = resp.data;
        if (reCalc) {
          this.listOfData.forEach((item: any) => {
            item.rowItemLv2.forEach((childItem: any) => {
              childItem.coefficient_99 = this.commonService.getCoefficient(item.floorCode, this.floorApplyPriceChange, childItem.isMezzanine, DecreeEnum.ND_99, this.distribution_floor_coefficient_data, this.specialCase);
              childItem.coefficient_34 = this.commonService.getCoefficient(item.floorCode, this.floorApplyPriceChange, childItem.isMezzanine, DecreeEnum.ND_34, this.distribution_floor_coefficient_data, this.specialCase);
              childItem.coefficient_61 = this.commonService.getCoefficient(item.floorCode, this.floorApplyPriceChange, childItem.isMezzanine, DecreeEnum.ND_61, this.distribution_floor_coefficient_data, this.specialCase);
            });
          });
        }
      }
    }
  }

  chooseFloorDetail() {
    this.modalSrv.create({
      nzTitle: `Chọn thông tin tầng cụ thể`,
      nzContent: ChooseAreaBlockComponent,
      nzComponentParams: {
      },
      nzOnOk: (res: any) => {
        this.nodes = res.nodes;
        this.changeLevelBlock(this.nodes);
      }
    });
  }
}
