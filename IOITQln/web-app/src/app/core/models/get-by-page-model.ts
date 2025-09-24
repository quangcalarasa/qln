export default class GetByPageModel {
  public page: number = 1;
  public page_size: number = Math.max(Math.floor((window.innerHeight - 220) / 34), 5);
  public query: string = '1=1';
  public order_by?: string = '';
  public search?: string;
  public select?: string;
  public item_count: number = 0;
}
export class GetByPageLandPriceModel extends GetByPageModel {
  public landPriceType: number = 1;
}
export class GetByPageMd167HouseModel extends GetByPageModel {
  public TypeHouse: number = 1;
}
