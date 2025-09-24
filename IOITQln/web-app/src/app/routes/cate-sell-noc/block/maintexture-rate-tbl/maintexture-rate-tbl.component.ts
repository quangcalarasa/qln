import { Component, Input, OnChanges, OnInit, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { CurrentStateMainTextureRepository } from 'src/app/infrastructure/repositories/ct-maintexture.repository';
import { RatioMainTextureRepository } from 'src/app/infrastructure/repositories/ratio-maintexture.repository';
import { TypeMainTexTure } from 'src/app/shared/utils/consts';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { TypeReportApplyEnum } from 'src/app/shared/utils/enums';

export interface RowItemLv3 {
  currstate_maintexture?: number;
  remaining_rate?: number;
  main_rate?: number;
}

export interface RowItemLv2 {
  type_maintexture: number;
  rowItemLv3: RowItemLv3[];
}

export interface RowItemLv1 {
  levelblock: number;
  ratio_maintexture?: number;
  numRow: number;
  totalValue?: number;
  totalValue1?: number;
  totalValue2?: number;
  rowItemLv2: RowItemLv2[];
}

@Component({
  selector: 'app-block-maintexture-rate-tbl',
  templateUrl: './maintexture-rate-tbl.component.html'
})
export class MainTextureRateTblComponent implements OnInit, OnChanges {
  @Input() levelblocks?: any[];
  @Input() blockMaintextureRaties?: any[];
  @Input() typeReportApply: number;
  @Input() parentTypeReportApply: number;

  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  listOfData: any[] = [];
  ct_maintexture_data: any[] = [];
  ratio_maintexture_data: any[] = [];

  TypeReportApplyEnum = TypeReportApplyEnum;

  constructor(
    private currentStateMainTextureRepository: CurrentStateMainTextureRepository,
    private ratioMainTextureRepository: RatioMainTextureRepository
  ) { }

  ngOnInit(): void {
    this.getDataCtMaintexture();
    this.getRatioMaintexture();

    if (this.levelblocks && this.blockMaintextureRaties) {
      if (this.blockMaintextureRaties.length) this.convertData()
    };
  }

  convertData() {
    this.listOfData = [];

    if (this.blockMaintextureRaties) {
      this.blockMaintextureRaties.forEach((item: any) => {
        let LevelBlockId = item.LevelBlockId;

        let index = this.listOfData.findIndex(x => x.levelblock == LevelBlockId);
        if (index == -1) {
          let rowItemLv3_data: RowItemLv3 = {
            currstate_maintexture: item.CurrentStateMainTextureId,
            remaining_rate: item.RemainingRate,
            main_rate: item.MainRate
          };

          let rowItemLv2_data: RowItemLv2 = {
            type_maintexture: item.TypeMainTexTure,
            rowItemLv3: [rowItemLv3_data]
          };

          let rowItemLv1_data: RowItemLv1 = {
            levelblock: item.LevelBlockId,
            ratio_maintexture: item.RatioMainTextureId,
            totalValue: item.TotalValue,
            totalValue1: item.TotalValue1,
            totalValue2: item.TotalValue2,
            numRow: 2,
            rowItemLv2: [rowItemLv2_data]
          };

          this.listOfData.push(rowItemLv1_data);
        } else {
          let rowItemLv3_data: RowItemLv3 = {
            currstate_maintexture: item.CurrentStateMainTextureId,
            remaining_rate: item.RemainingRate,
            main_rate: item.MainRate
          };

          let typeMainTexTure = item.TypeMainTexTure;
          let indexlv2 = this.listOfData[index].rowItemLv2.findIndex((x: any) => x.type_maintexture == typeMainTexTure);

          if (indexlv2 == -1) {
            let rowItemLv2_data: RowItemLv2 = {
              type_maintexture: item.TypeMainTexTure,
              rowItemLv3: [rowItemLv3_data]
            };

            this.listOfData[index].rowItemLv2.push(rowItemLv2_data);
            this.listOfData[index].rowItemLv2 = this.listOfData[index].rowItemLv2.sort(
              (a: any, b: any) => a.type_maintexture - b.type_maintexture
            );
          } else {
            this.listOfData[index].rowItemLv2[indexlv2].rowItemLv3.push(rowItemLv3_data);
          }

          this.listOfData[index].numRow += 1;
        }
      });

      this.listOfData = this.listOfData.sort((a: any, b: any) => a.levelblock - b.levelblock);
    }
  }

  emitChooseBlock() {
    if (this.levelblocks && this.blockMaintextureRaties) {
      if (this.blockMaintextureRaties.length) this.convertData()
    };
  }

  ngOnChanges(changes: SimpleChanges) {
    this.changeLevelBlock(this.levelblocks);
  }

  compareFnMt = (o1: any, o2: any) => {
    return o1 && o2 ? parseInt(o1) === parseInt(o2) : o1 === o2;
  };

  returnTypeMainTexTure(key: string) {
    return TypeMainTexTure[key as unknown as keyof typeof TypeMainTexTure];
  }

  async getDataCtMaintexture() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `1=1`;
    paging.select = 'Id,TypeMainTexTure,Name,LevelBlock';

    const resp = await this.currentStateMainTextureRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.ct_maintexture_data = resp.data;
    }
  }

  changeLevelBlock(items: any) {
    if (items.length == 0) {
      this.listOfData = [];
    } else {
      let listOfData = [...this.listOfData];
      this.listOfData = [];

      items.forEach((item: any) => {
        let levelBlockId = item.LevelId ? item.LevelId : parseInt(item.key);
        let index = listOfData.findIndex(x => x.levelblock == levelBlockId);
        if (index == -1) {
          //Thêm 1 RowItemLv1 mới
          let rowItemLv1: RowItemLv1;
          rowItemLv1 = {
            levelblock: levelBlockId,
            ratio_maintexture: undefined,
            totalValue: item.TotalValue,
            totalValue1: item.TotalValue1,
            totalValue2: item.TotalValue2,
            numRow: 7,
            rowItemLv2: []
          };

          Object.keys(TypeMainTexTure).forEach(v => {
            let rowItemLv3: RowItemLv3 = {};

            let rowItemLv2: RowItemLv2;
            rowItemLv2 = {
              type_maintexture: parseInt(v),
              rowItemLv3: [rowItemLv3]
            };

            rowItemLv1.rowItemLv2.push(rowItemLv2);
          });

          this.listOfData.push(rowItemLv1);
        } else {
          this.listOfData.push(listOfData[index]);
        }
      });

      this.listOfData = this.listOfData.sort((a: any, b: any) => a.levelblock - b.levelblock);
    }
  }

  changeRatioMaintexture(items: any, index: number) {
    this.listOfData[index].rowItemLv2.forEach((rowItemLv2: any) => {
      rowItemLv2.rowItemLv3.forEach((rowItemLv3: any) => {
        rowItemLv3.main_rate = this.getMainRate(this.listOfData[index].ratio_maintexture, rowItemLv2.type_maintexture);
      });
    });

    this.changeReMainingRate(index);
  }

  addRowItemLv3(index_lv1: number, index_lv2: number) {
    let row: RowItemLv3 = {
      currstate_maintexture: undefined,
      remaining_rate: undefined,
      main_rate: this.getMainRate(
        this.listOfData[index_lv1].ratio_maintexture,
        this.listOfData[index_lv1].rowItemLv2[index_lv2].type_maintexture
      )
    };

    this.listOfData[index_lv1].numRow += 1;
    this.listOfData[index_lv1].rowItemLv2[index_lv2].rowItemLv3.push(row);
  }

  removeRowItemLv3(index_lv1: number, index_lv2: number, index_lv3: number) {
    this.listOfData[index_lv1].rowItemLv2[index_lv2].rowItemLv3.splice(index_lv3, 1);

    if (this.listOfData[index_lv1].rowItemLv2[index_lv2].rowItemLv3.length == 0) {
      // let typeMainTexTure = this.listOfData[index_lv1].rowItemLv2[index_lv2].type_maintexture;

      // let indexMainTexture = this.listOfData[index_lv1].maintexture.findIndex((x: any) => parseInt(x) == parseInt(typeMainTexTure));

      // this.listOfData[index_lv1].maintexture.splice(indexMainTexture, 1);
      // this.listOfData[index_lv1].maintexture = [...this.listOfData[index_lv1].maintexture];

      this.listOfData[index_lv1].rowItemLv2.splice(index_lv2, 1);
    }

    this.listOfData[index_lv1].numRow -= 1;
  }

  getValue() {
    let res: any[] = [];
    this.listOfData.forEach((item: any) => {
      item.rowItemLv2.forEach((itemLv2: any) => {
        itemLv2.rowItemLv3.forEach((itemLv3: any) => {
          res.push({
            BlockId: undefined,
            LevelBlockId: item.levelblock,
            RatioMainTextureId: item.ratio_maintexture,
            TypeMainTexTure: itemLv2.type_maintexture,
            CurrentStateMainTextureId: itemLv3.currstate_maintexture,
            RemainingRate: itemLv3.remaining_rate,
            MainRate: itemLv3.main_rate ?? 0,
            TotalValue: item.totalValue,
            TotalValue1: item.totalValue1,
            TotalValue2: item.totalValue2
          });
        });
      });
    });

    return res;
  }

  //lấy ds giá trị của kết cấu chính so với tổng giá trị căn nhà
  async getRatioMaintexture() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `1=1`;

    const resp = await this.ratioMainTextureRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.ratio_maintexture_data = resp.data;
    }
  }

  //Lấy giá trị tỉ lệ kết cấu chính
  getMainRate(ratio_maintexture: number, type_maintexture: number) {
    let main_rate;

    if (!ratio_maintexture || !type_maintexture) return main_rate;

    let ratio_maintexture_data = this.ratio_maintexture_data.find(x => x.Id == ratio_maintexture);
    switch (true) {
      case type_maintexture == 1:
        main_rate = ratio_maintexture_data.TypeMainTexTure1;
        break;
      case type_maintexture == 2:
        main_rate = ratio_maintexture_data.TypeMainTexTure2;
        break;
      case type_maintexture == 3:
        main_rate = ratio_maintexture_data.TypeMainTexTure3;
        break;
      case type_maintexture == 4:
        main_rate = ratio_maintexture_data.TypeMainTexTure4;
        break;
      case type_maintexture == 5:
        main_rate = ratio_maintexture_data.TypeMainTexTure5;
        break;
      case type_maintexture == 6:
        main_rate = ratio_maintexture_data.TypeMainTexTure6;
        break;
      default:
        break;
    }

    return main_rate;
  }

  changeReMainingRate(index_lv1: number) {
    let valueRow: number | undefined;
    let valueRow1: number | undefined;

    this.listOfData[index_lv1].rowItemLv2.forEach((rowItemLv2: any) => {
      let rowItemLv3 = rowItemLv2.rowItemLv3.filter((x: any) => x.main_rate != undefined && x.remaining_rate != undefined);

      if (rowItemLv3.length > 0) {
        let totalValueRowLv3 = rowItemLv3.reduce((value: number, curr: any) => {
          return value + curr.remaining_rate;
        }, 0);

        let mediumValueRowLv3 = (totalValueRowLv3 / rowItemLv3.length) * rowItemLv3[0].main_rate;

        valueRow = valueRow ? valueRow + mediumValueRowLv3 : mediumValueRowLv3;
        valueRow1 = valueRow1 ? valueRow1 + rowItemLv3[0].main_rate : rowItemLv3[0].main_rate;
      }

      // rowItemLv2.rowItemLv3.forEach((rowItemLv3: any) => {
      //   if (rowItemLv3.main_rate != undefined && rowItemLv3.remaining_rate != undefined) {
      //     valueRow = valueRow ? valueRow + (rowItemLv3.main_rate * rowItemLv3.remaining_rate) : (rowItemLv3.main_rate * rowItemLv3.remaining_rate);

      //     valueRow1 = valueRow1 ? valueRow1 + (rowItemLv3.remaining_rate ? rowItemLv3.main_rate : 0) : (rowItemLv3.remaining_rate ? rowItemLv3.main_rate : 0);
      //   }
      // });
    });

    if (valueRow && valueRow1) {
      this.listOfData[index_lv1].totalValue2 = Math.floor(valueRow / valueRow1);
      this.listOfData[index_lv1].totalValue1 = Math.round((valueRow / valueRow1) * 100) / 100;
    } else {
      this.listOfData[index_lv1].totalValue2 = undefined;
      this.listOfData[index_lv1].totalValue1 = undefined;
    }

    this.compareTotalValue(index_lv1);
  }

  compareTotalValue(index_lv1: number) {
    if (this.listOfData[index_lv1].totalValue1 && this.listOfData[index_lv1].totalValue2) {
      this.listOfData[index_lv1].totalValue =
        this.listOfData[index_lv1].totalValue1 > this.listOfData[index_lv1].totalValue2
          ? this.listOfData[index_lv1].totalValue1
          : this.listOfData[index_lv1].totalValue2;
    } else {
      this.listOfData[index_lv1].totalValue = undefined;
    }
  }
}
