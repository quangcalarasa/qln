import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DelegateComponent } from './delegate/delegate.component';
import { LandTaxComponent } from './land-tax/land-tax.component';
import { LandPriceComponent } from './land-price/land-price.component';
import { AuctioneerComponent } from './auctioneer/auctioneer.component';
import { PositionValueComponent } from './position-value/position-value.component';
import { AreaValueComponent } from './area-value/area-value.component';
import { VatValueComponent } from './vat-value/vat-value.component';
import { HouseTypeComponent } from './house-type/house-type.component';
import { HouseComponent } from './house/house.component';
import { Md167ProfitValueComponent } from './profit-value/profit-value.component';
import { Md167ManagePurposeComponent } from './md167-manage-purpose/md167-manage-purpose.component';
import { StateOfUseComponent } from './state-of-use/state-of-use.component';
import { TranferUnitComponent } from './tranfer-unit/tranfer-unit.component';
import { PlantContentComponent } from './plant-content/plant-content.component';
import { ListContractComponent } from '../md167-contract/list-contract/list-contract.component';
import { Report08Component } from '../report-md167/report08/report08.component';
import { Report07Component } from '../report-md167/report07/report07.component';
import { DebtInfoComponent } from '../report-md167/debt-info/debt-info.component';
import { ProvinceComponent } from '../category/province/province.component';
import { DistrictComponent } from '../category/district/district.component';
import { WardComponent } from '../category/ward/ward.component';
import { LaneComponent } from '../category/lane/lane.component';
import { ManagePaymentComponent } from './manage-payment/manage-payment.component';

const routes: Routes = [
  // { path: 'type-attribute', component: TypeAttributeComponent, data: { title: 'Loại hình' } },
  { path: 'md167-value/md167-land-tax', component: LandTaxComponent, data: { title: 'Thuế suất đất' } },
  { path: 'md167-office/md167-delegate', component: DelegateComponent, data: { title: 'Đại diện tổ chức/Cá nhân thuê' } },
  { path: 'md167-office/md167-auctioneer', component: AuctioneerComponent, data: { title: 'Đơn vị thẩm định giá' } },
  { path: 'md167-location/md167-land-price', component: LandPriceComponent, data: { title: 'Bảng giá đất' } },
  { path: 'md167-location/md167-provine', component: ProvinceComponent, data: { title: 'Tỉnh/thành phố' } },
  { path: 'md167-location/md167-district', component: DistrictComponent, data: { title: 'Quận/huyện' } },
  { path: 'md167-location/md167-ward', component: WardComponent, data: { title: 'Phường/xã' } },
  { path: 'md167-location/md167-lane', component: LaneComponent, data: { title: 'Đường' } },
  { path: 'md167-house-man/plan-content', component: PlantContentComponent, data: { title: 'Nội dung phương án được phê duyệt theo Quyết định' } },
  { path: 'md167-value/md167-postion-value', component: PositionValueComponent, data: { title: 'Hệ số vị trí' } },
  { path: 'md167-house-man/md167-manage-purpose', component: Md167ManagePurposeComponent, data: { title: 'Mục đích quản lý sử dụng phê duyệt' } },
  { path: 'md167-house-man/state-of-use', component: StateOfUseComponent, data: { title: 'Hiện trạng sử dụng' } },
  { path: 'md167-value/md167-profit-value', component: Md167ProfitValueComponent, data: { title: 'Hệ số lãi phạt thuê' } },
  { path: 'md167-value/md167-area-value', component: AreaValueComponent, data: { title: 'Hệ số khu vực' } },
  { path: 'md167-value/md167-vat-value', component: VatValueComponent, data: { title: 'Thuế VAT' } },
  { path: 'md167-house-man/md167-house-type', component: HouseTypeComponent, data: { title: 'Loại nhà' } },
  { path: 'md167-contract', component: ListContractComponent, data: { title: 'Hợp đồng nhà ở 167' } },
  { path: 'report-md167/report08-md167', component: Report08Component, data: { title: 'Thống kê ds cơ sở nhà đất 167' } },
  { path: 'report-md167/report07-md167', component: Report07Component, data: { title: 'Thống kê danh sách tiền thuê nhà' } },
  { path: 'report-md167/info-debt-md167', component: DebtInfoComponent, data: { title: 'Thông tin công nợ' } },

  { path: 'report-md167', component: Md167ProfitValueComponent, data: { title: 'Hệ số lãi phạt thuê' } },

  { path: 'md167-house-man/md167-house', component: HouseComponent, data: { title: 'Nhà' } },
  { path: 'md167-office/tranfer-unit', component: TranferUnitComponent, data: { title: 'đơn vị vận chuyển' } },
  { path: 'Manage-payment', component: ManagePaymentComponent, data: {title: 'Phiếu chi'}}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CateMd167RoutingModule { }
