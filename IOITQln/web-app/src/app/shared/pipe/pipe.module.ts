import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FilterCtMainTexturePipe } from './filter-status.pipe';
import { FilterAreaByFloorPipe } from './filter-area-by-floor.pipe';
import { FilterByPropertyNull } from './filter-by-propertynull.pipe';
import { FilterLaneByWardPipe } from './filter-lane-by-ward.pipe';
import { FilterPriceListItemByDecreeType1IdPipe } from './filter-pricelistitem-by-decreeType1Id.pipe';
import { FilterConstByListPipe } from './filter-const-by-list.pipe';
import { FilterTermApplyByDecreePipe } from './filter-termapply-by-decree.pipe';
import { ShowDecreeNamePipe } from './show-decreename.pipe';
import { ShowTermApplyNamePipe } from './show-termapplyname.pipe';
import { FilterItemsByDecreeType1IdPipe } from './filter-items-by-decreeType1Id.pipe';
import { ShowLevelBlockNamePipe } from './show-levelblockname.pipe';
import { ShowNameItemPipe } from './show-nameitem.pipe';
import { FilterTypeReportApplyWithoutAnyItemPipe } from './filter-typeReportApply-without-any-tem.pipe';
import { FilterApartmentDetailDataPricingPipe } from './filter-apartment-detail-data-pricing.pipe';
import { FilterTypeBlockByTypeReportApplyPipe } from './filter-typeBlock-by-typeReportApply.pipe';
import { FilterDecreeWithoutAnyItemPipe } from './filter-decree-without-any-tem.pipe';
import { ShowContractStatus167NamePipe } from './show-contractstatus167name.pipe';
import { ShowModuleSystemPipe } from './show-modulesystem.pipe';

@NgModule({
  declarations: [FilterCtMainTexturePipe, FilterAreaByFloorPipe, FilterByPropertyNull, FilterLaneByWardPipe, FilterPriceListItemByDecreeType1IdPipe, FilterConstByListPipe, FilterTermApplyByDecreePipe, ShowDecreeNamePipe, ShowTermApplyNamePipe, FilterItemsByDecreeType1IdPipe, ShowLevelBlockNamePipe, ShowNameItemPipe, FilterTypeReportApplyWithoutAnyItemPipe, FilterApartmentDetailDataPricingPipe, FilterTypeBlockByTypeReportApplyPipe, FilterDecreeWithoutAnyItemPipe, ShowContractStatus167NamePipe, ShowModuleSystemPipe],
  imports: [CommonModule],
  exports: [FilterCtMainTexturePipe, FilterAreaByFloorPipe, FilterByPropertyNull, FilterLaneByWardPipe, FilterPriceListItemByDecreeType1IdPipe, FilterConstByListPipe, FilterTermApplyByDecreePipe, ShowDecreeNamePipe, ShowTermApplyNamePipe, FilterItemsByDecreeType1IdPipe, ShowLevelBlockNamePipe, ShowNameItemPipe, FilterTypeReportApplyWithoutAnyItemPipe, FilterApartmentDetailDataPricingPipe, FilterTypeBlockByTypeReportApplyPipe, FilterDecreeWithoutAnyItemPipe, ShowContractStatus167NamePipe, ShowModuleSystemPipe]
})
export class PipeModule { }
