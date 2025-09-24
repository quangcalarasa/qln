using IOITQln.Common.Bases;
using IOITQln.Common.Bases.Configurations;
using IOITQln.Common.Interfaces.Helpers;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Persistence
{
    public class ApiDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly IDateTimeService _dateTime;

        public ApiDbContext()
        {
        }

        public ApiDbContext(DbContextOptions<ApiDbContext> options,
            IDateTimeService datetime) : base(options)
        {
            _dateTime = datetime;
        }

        public virtual DbSet<Function> Functions { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<FunctionRole> FunctionRoles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<LogAction> LogActions { get; set; }
        public virtual DbSet<TypeAttribute> TypeAttributes { get; set; }
        public virtual DbSet<TypeAttributeItem> TypeAttributeItems { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<District> Districts { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<Province> Provincies { get; set; }
        public virtual DbSet<RentingPrice> RentingPricies { get; set; }
        public virtual DbSet<Template> Templaties { get; set; }
        public virtual DbSet<VerifycationUnit> VerifycationUnits { get; set; }
        public virtual DbSet<Ward> Wards { get; set; }
        public virtual DbSet<Holiday> Holidays { get; set; }
        public virtual DbSet<Block> Blocks { get; set; }
        public virtual DbSet<LevelBlockMap> LevelBlockMaps { get; set; }
        public virtual DbSet<FloorBlockMap> FloorBlockMaps { get; set; }
        public virtual DbSet<Floor> Floors { get; set; }
        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<Apartment> Apartments { get; set; }
        public virtual DbSet<ApartmentDetail> ApartmentDetails { get; set; }
        public virtual DbSet<CurrentStateMainTexture> CurrentStateMainTexturies { get; set; }
        public virtual DbSet<RatioMainTexture> RatioMainTexturies { get; set; }
        public virtual DbSet<ConstructionPrice> ConstructionPricies { get; set; }
        //public virtual DbSet<ConstructionPriceItem> ConstructionPriceItems { get; set; }
        public virtual DbSet<Decree> Decreies { get; set; }
        public virtual DbSet<PriceList> PriceLists { get; set; }
        public virtual DbSet<UnitPrice> UnitPricies { get; set; }
        public virtual DbSet<DeductionCoefficient> DeductionCoefficients { get; set; }
        public virtual DbSet<InvestmentRate> InvestmentRaties { get; set; }
        public virtual DbSet<SalaryCoefficient> SalaryCoefficients { get; set; }
        public virtual DbSet<UseValueCoefficient> UseValueCoefficients { get; set; }
        public virtual DbSet<UseValueCoefficientItem> UseValueCoefficientItems { get; set; }
        public virtual DbSet<AreaCorrectionCoefficient> AreaCorrectionCoefficients { get; set; }
        public virtual DbSet<LandPrice> LandPricies { get; set; }
        public virtual DbSet<LandSpecialCoefficient> LandSpecialCoefficients { get; set; }
        public virtual DbSet<Lane> Lanies { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<DeductionLandMoney> DeductionLandMonies { get; set; }
        public virtual DbSet<DistributionFloorCoefficient> DistributionFloorCoefficients { get; set; }
        public virtual DbSet<DistributionFloorCoefficientDetail> DistributionFloorCoefficientDetails { get; set; }
        public virtual DbSet<LandPriceCorrectionCoefficient> LandPriceCorrectionCoefficients { get; set; }
        public virtual DbSet<PositionCoefficient> PositionCoefficients { get; set; }
        public virtual DbSet<Vat> Vats { get; set; }
        public virtual DbSet<BlockDetail> BlockDetails { get; set; }
        public virtual DbSet<BlockMaintextureRate> BlockMaintextureRaties { get; set; }
        public virtual DbSet<PriceListItem> PriceListItems { get; set; }
        public virtual DbSet<LandPriceItem> LandPriceItems { get; set; }
        public virtual DbSet<Pricing> Pricings { get; set; }
        public virtual DbSet<PricingConstructionPrice> PricingConstructionPricies { get; set; }
        public virtual DbSet<PricingCustomer> PricingCustomers { get; set; }
        public virtual DbSet<PricingLandTbl> PricingLandTbls { get; set; }
        public virtual DbSet<PricingOfficer> PricingOfficers { get; set; }
        public virtual DbSet<PricingReducedPerson> PricingReducedPeople { get; set; }
        public virtual DbSet<UseValueCoefficientTypeReportApply> UseValueCoefficientTypeReportApplies { get; set; }
        public virtual DbSet<InvestmentRateItem> InvestmentRateItems { get; set; }
        public virtual DbSet<IngredientsPrice> IngredientsPrices { get; set; }
        public virtual DbSet<OriginalPriceAndTax> OriginalPriceAndTaxs { get; set; }
        public virtual DbSet<ResettlementApartment> ResettlementApartments { get; set; }
        public virtual DbSet<ProfitValue> ProfitValues { get; set; }
        public virtual DbSet<AnnualInstallment> AnnualInstallments { get; set; }
        public virtual DbSet<Land> Lands { get; set; }
        public virtual DbSet<TDCProject> TDCProjects { get; set; }
        public virtual DbSet<TDCProjectIngrePrice> TDCProjectIngrePrices { get; set; }
        public virtual DbSet<TDCProjectPriceAndTax> TDCProjectPriceAndTaxs { get; set; }
        public virtual DbSet<TDCProjectPriceAndTaxDetails> TDCProjectPriceAndTaxDetailss { get; set; }
        public virtual DbSet<BlockHouse> BlockHouses { get; set; }
        public virtual DbSet<FloorTdc> FloorTdcs { get; set; }
        public virtual DbSet<ApartmentTdc> ApartmentTdcs { get; set; }
        public virtual DbSet<PlatformTdc> PlatformTdcs { get; set; }
        public virtual DbSet<TdcCustomer>  TdcCustomers { get; set; }
        public virtual DbSet<TdcAuthCustomerDetail> TdcAuthCustomerDetails { get; set; }
        public virtual DbSet<TdcCustomerFile> TdcCustomerFiles { get; set; }
        public virtual DbSet<DecreeMap> DecreeMaps { get; set; }
        public virtual DbSet<TdcPriceRent> TdcPriceRents { get; set; }
        public virtual DbSet<TdcPriceOneSell> TdcPriceOneSells { get; set; }
        public virtual DbSet<TdcPriceOneSellOfficial> TdcPriceOneSellOfficials { get; set; }
        public virtual DbSet<TdcPriceOneSellTemporary> TdcPriceOneSellTemporaries { get; set; }
        public virtual DbSet<TdcPriceOneSellTax> TdcPriceOneSellTaxes { get; set; }
        public virtual DbSet<TdcPriceRentTemporary> TdcPriceRentTemporaries { get; set; }
        public virtual DbSet<TdcPriceRentOfficial> TdcPriceRentOfficials { get;set; }
        public virtual DbSet<TdcPriceRentTax> TdcPriceRentTaxs { get; set; }
        public virtual DbSet<ApartmentLandDetail> ApartmentLandDetails { get; set; }
        public virtual DbSet<TDCInstallmentPrice> TDCInstallmentPrices { get; set; }
        public virtual DbSet<TDCInstallmentPriceAndTax> TDCInstallmentPriceAndTaxs { get; set; }
        public virtual DbSet<TDCInstallmentTemporaryDetail> TDCInstallmentTemporaryDetails { get; set; }
        public virtual DbSet<TDCInstallmentOfficialDetail> TDCInstallmentOfficialDetails { get; set; }
        public virtual DbSet<TdcPriceRentPay> TdcPriceRentPays { get; set; }
        public virtual DbSet<TDCInstallmentPricePay> TDCInstallmentPricePays { get; set; }
        public virtual DbSet<No2LandPrice> No2LandPricies { get; set; }
        public virtual DbSet<TdcPriceRentExcelData> TdcPriceRentExcelDatas { get; set; }
        public virtual DbSet<InstallmentPriceTableTdc> InstallmentPriceTableTdcs { get; set; }
        public virtual DbSet<InstallmentPriceTableMetaTdc> InstallmentPriceTableMetaTdcs { get; set; }
        public virtual DbSet<PricingApartmentLandDetail> PricingApartmentLandDetails { get; set; }
        public virtual DbSet<TdcPriceRentExcelMeta> TdcPriceRentExcelMetas { get; set; }
        public virtual DbSet<LandscapeLimit> LandscapeLimits { get; set; }
        public virtual DbSet<LandscapeLimitItem> LandscapeLimitItems { get; set; }
        public virtual DbSet<Coefficient> Coefficients { get; set; }
        public virtual DbSet<Conversion> Conversions { get; set; }
        public virtual DbSet<DefaultCoefficient> DefaultCoefficients { get; set; }
        public virtual DbSet<RentFile> RentFiles { get; set; }
        public virtual DbSet<MemberRentFile> MemberRentFiles { get; set; }
        public virtual DbSet<PricingReplaced> PricingReplaceds { get; set; }
        public virtual DbSet<Files> Files { get; set; }
        public virtual DbSet<TypeBlock> TypeBlocks { get; set; }
        public virtual DbSet<TypeBlockMap> TypeBlockMaps { get; set; }
        public virtual DbSet<ChildDefaultCoefficient> ChildDefaults { get; set; }
        public virtual DbSet<RentFileBCT> RentFileBCTs { get; set; }
        public virtual DbSet<NocReceipt> NocReceipts { get; set; } 
        public virtual DbSet<Debts> debts { get; set; }
        public virtual DbSet<DebtsTable> DebtsTables { get; set; }
        public virtual DbSet<ProcessProfileCe> ProcessProfileCes { get; set; }
        public virtual DbSet<TdcApartmentManager> TdcApartmentManagers { get; set; }
        public virtual DbSet<TdcPlatformManager> TdcPlatformManagers { get; set; }
        public virtual DbSet<Md167AreaValue> Md167AreaValues { get; set; }
        public virtual DbSet<Md167Auctioneer> Md167Auctioneers { get; set; }
        public virtual DbSet<Md167Delegate> Md167Delegates { get; set; }
        public virtual DbSet<Md167House> Md167Houses { get; set; }
        public virtual DbSet<Md167HouseType> Md167HouseTypes { get; set; }
        public virtual DbSet<Md167LandTax> Md167LandTaxs { get; set; }
        public virtual DbSet<Md167PositionValue> Md167PositionValues { get; set; }
        public virtual DbSet<Md167VATValue> Md167VATValues { get; set; }
        public virtual DbSet<Md167AreaValueApply> Md167AreaValueApplies { get; set; }
        public virtual DbSet<Md167Contract> Md167Contracts { get; set; }
        public virtual DbSet<Md167PricePerMonth> Md167PricePerMonths { get; set; }
        public virtual DbSet<Md167AuctionDecision> Md167AuctionDecisions { get; set; }
        public virtual DbSet<Md167Valuation> Md167Valuations { get; set; }
        public virtual DbSet<Md167HouseInfo> Md167HouseInfos { get; set; }
        public virtual DbSet<Md167HousePropose> Md167HouseProposes { get; set; }
        public virtual DbSet<Md167Debt> Md167Debts { get; set; }
        public virtual DbSet<Md167Receipt> Md167Receipts { get; set; }
        public virtual DbSet<Md167ProfitValue> Md167ProfitValues { get; set; }
        public virtual DbSet<Salary> Salaries { get; set; }
        public virtual DbSet<Md167ManagePurpose> Md167ManagePurposes { get; set; }
        public virtual DbSet<Md167StateOfUse> Md167StateOfUses { get; set; }
        public virtual DbSet<Md167TranferUnit> Md167TranferUnits { get; set; }
        public virtual DbSet<DiscountCoefficient> DiscountCoefficients { get; set; }
        public virtual DbSet<TdcMemberCustomer> TdcMemberCustomers { get; set; }
        public virtual DbSet<Md167PlantContent> Md167PlantContents { get; set; }
        public virtual DbSet<DistrictAllocasionApartment> DistrictAllocasionApartments { get; set; }
        public virtual DbSet<DistrictAllocasionPlatform> DistrictAllocasionPlatforms { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }
        public virtual DbSet<ImportHistory> ImportHistories { get; set; }
        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<ExtraTemplate> ExtraTemplates { get; set; }
        public virtual DbSet<ExtraEmailDebt> ExtraEmailDebts { get; set; }
        public virtual DbSet<ExtraEmailNoti> ExtraEmailNotis { get; set; }
        public virtual DbSet<ExtraNewsArticle> ExtraNewsArticles { get; set; }
        public virtual DbSet<ExtraNewsArticleList> ExtraNewsArticleLists { get; set; }
        public virtual DbSet<ExtraPostingConfiguration> ExtraPostingConfigurations { get; set; }
        public virtual DbSet<ExtraConfigNotii> ExtraConfigNotiis { get; set; }
        public virtual DbSet<ExtraConfigDebt> ExtraConfigDebts { get; set; }
        public virtual DbSet<ExtraDebtReminder> ExtraDebtReminders { get; set; }
        public virtual DbSet<ExtraDbetManage> ExtraDbetManages { get; set; }
        public virtual DbSet<ExtraViewNotiSend> ExtraViewNotiSends { get; set; }
        public virtual DbSet<ExtraSupportRequest> ExtraSupportRequests { get; set; }
        public virtual DbSet<ExtraSupportHandle> ExtraSupportHandles { get; set; }
        public virtual DbSet<ExtraHistoryAndStatus> ExtraHistoryAndStatuses { get; set; }
        public virtual DbSet<ExtraDebtNotification> ExtraDebtNotifications { get; set; }
        public virtual DbSet<ImportHistoryItem> ImportHistoryItems { get; set; }

        public virtual DbSet<EditHistory> EditHistories { get; set; }
        public virtual DbSet<WardManagement> WardManagements { get; set; }

        public virtual DbSet<RentBctTable> RentBctTables { get; set; }
        public virtual DbSet<ChilDfTable> ChilDfTables { get; set; }

        public virtual DbSet<NocBlockFile> NocBlockFiles { get; set; }
        public virtual DbSet<NocApartmentFile> NocApartmentFiles { get; set; }

        public virtual DbSet<QuickMathHistory> QuickMathHistories { get; set; }

        public virtual DbSet<Md167ManagePayment> Md167ManagePayments { get; set; }
        public virtual DbSet<HousePayment> HousePayments { get; set; }
        public virtual DbSet<Md167HouseFile> Md167HouseFiles { get; set; }

        public virtual DbSet<QuickMathLog> QuickMathLogs { get; set; }
        public virtual DbSet<ManualDocument> ManualDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var assembly = typeof(AppEntityTypeBaseConfiguration<>).Assembly;
            builder.ApplyConfigurationsFromAssembly(assembly);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

                var configuration = builder.Build();
                string con = configuration.GetConnectionString("DbConnection");
                optionsBuilder.UseSqlServer(con);
            }

            //optionsBuilder.UseLazyLoadingProxies();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            AddAuditUserChange();

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            AddAuditUserChange();

            return base.SaveChanges();
        }

        private void AddAuditUserChange()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity<int>>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = _dateTime.Now;
                        entry.Entity.UpdatedAt = _dateTime.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = _dateTime.Now;
                        break;
                }
            }

            foreach (var entry in ChangeTracker.Entries<BaseEntity<Guid>>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = _dateTime.Now;
                        entry.Entity.UpdatedAt = _dateTime.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = _dateTime.Now;
                        break;
                }
            }

            foreach (var entry in ChangeTracker.Entries<BaseEntity<long>>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = _dateTime.Now;
                        entry.Entity.UpdatedAt = _dateTime.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = _dateTime.Now;
                        break;
                }
            }
        }
    }
}
