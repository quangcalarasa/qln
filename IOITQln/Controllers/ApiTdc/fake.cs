//using IOITQln.Entities;

//if (input.tDCInstallmentOfficialDetails != null)
//{
//    bool check = Check(input.tDCInstallmentOfficialDetails, tDCInstallmentOfficialDetails);

//    if (check == false)
//    {
//        tDCInstallmentOfficialDetails.ForEach(x => {
//            x.ChangeTimes = tDCInstallmentOfficialDetails[0].Id;
//            x.Status = AppEnums.EntityStatus.DELETED;
//        });
//        _context.UpdateRange(tDCInstallmentOfficialDetails);
//        await _context.SaveChangesAsync();
//        foreach (var item in input.tDCInstallmentOfficialDetails)
//        {
//            item.Id = 0;
//            item.TDCInstallmentPriceId = input.Id;
//            _context.TDCInstallmentOfficialDetails.Add(item);
//        }
//        await _context.SaveChangesAsync();
//    }

//}