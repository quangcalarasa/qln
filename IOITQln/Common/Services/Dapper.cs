using Dapper;
using IOITQln.Common.Interfaces;
using IOITQln.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace IOITQln.Common.Services
{
    public class Dapperr : IDapper
    {
        private readonly IConfiguration _config;
        private string Connectionstring = "DbConnection";

        public Dapperr(IConfiguration config)
        {
            _config = config;
        }
        public void Dispose()
        {

        }

        public int Execute(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.Text)
        {
            IDbConnection db = new SqlConnection(_config.GetConnectionString(Connectionstring));
            return db.Query<T>(sp, parms, commandType: commandType).FirstOrDefault();
        }

        public List<T> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            IDbConnection db = new SqlConnection(_config.GetConnectionString(Connectionstring));
            return db.Query<T>(sp, parms, commandType: commandType).ToList();
        }

        public DbConnection GetDbconnection()
        {
            return new SqlConnection(_config.GetConnectionString(Connectionstring));
        }

        public T Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            T result;
            IDbConnection db = new SqlConnection(_config.GetConnectionString(Connectionstring));
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                var tran = db.BeginTransaction();
                try
                {
                    result = db.Query<T>(sp, parms, commandType: commandType, transaction: tran).FirstOrDefault();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }

            return result;
        }

        public T Update<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            T result;
            IDbConnection db = new SqlConnection(_config.GetConnectionString(Connectionstring));
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                var tran = db.BeginTransaction();
                try
                {
                    result = db.Query<T>(sp, parms, commandType: commandType, transaction: tran).FirstOrDefault();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }

            return result;
        }

        //public bool ImportNocReceipt(List<NocReceipt> promissoryParms, List<Debts> debtParms, List<DebtsTable> debtTblParms, ImportHistory importHistory)
        //{
        //    bool result;
        //    IDbConnection db = new SqlConnection(_config.GetConnectionString(Connectionstring));
        //    try
        //    {
        //        if (db.State == ConnectionState.Closed)
        //            db.Open();

        //        var tran = db.BeginTransaction();
        //        try
        //        {
        //            //Add Promissory
        //            string promissorySp = "INSERT INTO NocReceipt (Id, CreatedById, UpdatedById, CreatedBy, UpdatedBy, Number, Code, Date, Executor, Price, Action, NumberOfTransfer, InvoiceCode, DateOfTransfer, Note, Content, IsImportExcel) VALUES (@Id, @CreatedById, @UpdatedById, @CreatedBy, @UpdatedBy, @Number, @Code, @Date, @Executor, @Price, @Action, @NumberOfTransfer, @InvoiceCode, @DateOfTransfer, @Note, @Content, @IsImportExcel)";
        //            db.Execute(promissorySp, promissoryParms, tran, null, CommandType.Text);

        //            //Update Debt
        //            string debtSp = "UPDATE Debts SET SurplusBalance=@SurplusBalance, Paid=@Paid, Diff= @Diff WHERE Id=@Id";
        //            db.Execute(debtSp, param: debtParms, tran, null, CommandType.Text);

        //            //Update DebtTables
        //            string debtTblSp = "UPDATE DebtsTable SET UpdatedById=@UpdatedById, UpdatedBy=@UpdatedBy, UpdatedAt=@UpdatedAt, Code=@Code, [Index]=@Index, DateStart=@DateStart, DateEnd=@DateEnd, RentFileId=@RentFileId, Price=@Price, [Check]=@Check, Executor=@Executor, Date=@Date, NearestActivities=@NearestActivities, PriceDiff=@PriceDiff, AmountExclude=@AmountExclude, VATPrice=@VATPrice, VAT=@VAT, CheckPayDepartment=@CheckPayDepartment, Type=@Type, NocReceiptId=@NocReceiptId, SXN=@SXN WHERE Id=@Id";
        //            db.Execute(debtTblSp, param: debtTblParms, tran, null, CommandType.Text);

        //            //Add ImportHistory
        //            string importHistorySp = "INSERT INTO ImportHistory (CreatedById, UpdatedById, CreatedBy, UpdatedBy, Type, FileUrl, DataStorage, DataExtraStorage) VALUES (@CreatedById, @UpdatedById, @CreatedBy, @UpdatedBy, @Type, @FileUrl, @DataStorage, @DataExtraStorage)";
        //            db.Execute(importHistorySp, param: importHistory, tran, null, CommandType.Text);

        //            tran.Commit();
        //            result = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            tran.Rollback();
        //            result = false;
        //            throw ex;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = false;
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (db.State == ConnectionState.Open)
        //            db.Close();
        //    }
        //    return result;
        //}

        //QFix:Fix import dữ liệu
        private static object Db(object? v) => v ?? DBNull.Value;

        public bool ImportNocReceipt(
            List<NocReceipt> promissoryParms,
            List<Debts> debtParms,
            List<DebtsTable> debtTblParms,
            ImportHistory importHistory)
        {
            using var db = new SqlConnection(_config.GetConnectionString(Connectionstring));
            db.Open();
            using var cmd = db.CreateCommand();
            cmd.CommandText = "dbo.usp_ImportNocReceiptBulk";
            cmd.CommandType = CommandType.StoredProcedure;

            // --- TVP 1: NocReceiptType ---
            DataTable ToDtReceipts(IEnumerable<NocReceipt> items)
            {
                var dt = new DataTable();
                dt.Columns.Add("Id", typeof(Guid));
                dt.Columns.Add("CreatedById", typeof(long));   // BIGINT
                dt.Columns.Add("UpdatedById", typeof(long));   // BIGINT
                dt.Columns.Add("CreatedBy", typeof(string));
                dt.Columns.Add("UpdatedBy", typeof(string));
                dt.Columns.Add("Number", typeof(string));
                dt.Columns.Add("Code", typeof(string));
                dt.Columns.Add("Date", typeof(DateTime));
                dt.Columns.Add("Executor", typeof(string));
                dt.Columns.Add("Price", typeof(decimal));
                dt.Columns.Add("Action", typeof(byte));   // tinyint -> byte (hoặc int cũng được)
                dt.Columns.Add("NumberOfTransfer", typeof(string));
                dt.Columns.Add("InvoiceCode", typeof(string));
                dt.Columns.Add("DateOfTransfer", typeof(DateTime));
                dt.Columns.Add("Note", typeof(string));
                dt.Columns.Add("Content", typeof(string));
                dt.Columns.Add("IsImportExcel", typeof(bool));

                foreach (var x in items)
                    dt.Rows.Add(
                        Db(x.Id),
                        Db((long?)x.CreatedById),   // ép về long nếu model đang int
                        Db((long?)x.UpdatedById),
                        Db(x.CreatedBy),
                        Db(x.UpdatedBy),
                        Db(x.Number),
                        Db(x.Code),
                        Db(x.Date),
                        Db(x.Executor),
                        Db(x.Price),
                        Db((byte?)x.Action),
                        Db(x.NumberOfTransfer),
                        Db(x.InvoiceCode),
                        Db(x.DateOfTransfer),
                        Db(x.Note),
                        Db(x.Content),
                        Db(x.IsImportExcel)
                    );
                return dt;
            }

            // --- TVP 2: DebtsUpdateType ---
            DataTable ToDtDebts(IEnumerable<Debts> items)
            {
                var dt = new DataTable();
                dt.Columns.Add("Id", typeof(int));                 // <- ĐÚNG
                dt.Columns.Add("SurplusBalance", typeof(decimal));
                dt.Columns.Add("Paid", typeof(decimal));
                dt.Columns.Add("Diff", typeof(decimal));
                foreach (var x in items)
                    dt.Rows.Add(x.Id, x.SurplusBalance, x.Paid, x.Diff);
                return dt;
            }

            // --- TVP 3: DebtsTableUpdateType ---
            DataTable ToDtDebtsTbl(IEnumerable<DebtsTable> items)
            {
                var dt = new DataTable();
                dt.Columns.Add("Id", typeof(int));
                dt.Columns.Add("UpdatedById", typeof(long));
                dt.Columns.Add("UpdatedBy", typeof(string));
                dt.Columns.Add("UpdatedAt", typeof(DateTime));
                dt.Columns.Add("Code", typeof(string));
                dt.Columns.Add("Index", typeof(int));
                dt.Columns.Add("DateStart", typeof(DateTime));
                dt.Columns.Add("DateEnd", typeof(DateTime));
                dt.Columns.Add("RentFileId", typeof(Guid));
                dt.Columns.Add("Price", typeof(decimal));
                dt.Columns.Add("Check", typeof(bool));
                dt.Columns.Add("Executor", typeof(string));
                dt.Columns.Add("Date", typeof(DateTime));
                dt.Columns.Add("NearestActivities", typeof(string));
                dt.Columns.Add("PriceDiff", typeof(decimal));
                dt.Columns.Add("AmountExclude", typeof(decimal));
                dt.Columns.Add("VATPrice", typeof(double));   // float
                dt.Columns.Add("VAT", typeof(double));        // float
                dt.Columns.Add("CheckPayDepartment", typeof(bool));
                dt.Columns.Add("Type", typeof(byte));         // tinyint
                dt.Columns.Add("NocReceiptId", typeof(Guid));
                dt.Columns.Add("SXN", typeof(string));

                foreach (var x in items)
                    dt.Rows.Add(
                        x.Id, x.UpdatedById, x.UpdatedBy, x.UpdatedAt, x.Code, x.Index,
                        x.DateStart, x.DateEnd, x.RentFileId,
                        Math.Round(x.Price, 2),
                        x.Check, x.Executor, x.Date, x.NearestActivities,
                        Math.Round(x.PriceDiff, 2),
                        Math.Round(x.AmountExclude, 2),
                        x.VATPrice, x.VAT, x.CheckPayDepartment, x.Type,
                        x.NocReceiptId, x.SXN
                    );

                return dt;
            }

            // --- TVP 4: ImportHistoryType ---
            DataTable ToDtHistory(ImportHistory h)
            {
                var dt = new DataTable();
                dt.Columns.Add("CreatedById", typeof(long));       // BIGINT
                dt.Columns.Add("UpdatedById", typeof(long));       // BIGINT
                dt.Columns.Add("CreatedBy", typeof(string));
                dt.Columns.Add("UpdatedBy", typeof(string));
                dt.Columns.Add("Type", typeof(int));
                dt.Columns.Add("FileUrl", typeof(string));
                dt.Columns.Add("DataStorage", typeof(string));     // NVARCHAR(MAX) (TVP không hỗ trợ NTEXT)
                dt.Columns.Add("DataExtraStorage", typeof(string));

                dt.Rows.Add(
                    Db((long?)h.CreatedById),
                    Db((long?)h.UpdatedById),
                    Db(h.CreatedBy),
                    Db(h.UpdatedBy),
                    Db((int)h.Type),
                    Db(h.FileUrl),
                    Db(h.DataStorage),
                    Db(h.DataExtraStorage)
                );
                return dt;
            }

            var p1 = cmd.Parameters.AddWithValue("@Receipts", ToDtReceipts(promissoryParms));
            p1.SqlDbType = SqlDbType.Structured; p1.TypeName = "dbo.NocReceiptType";

            var p2 = cmd.Parameters.AddWithValue("@Debts", ToDtDebts(debtParms));
            p2.SqlDbType = SqlDbType.Structured; p2.TypeName = "dbo.DebtsUpdateType";

            var p3 = cmd.Parameters.AddWithValue("@DebtsTbl", ToDtDebtsTbl(debtTblParms));
            p3.SqlDbType = SqlDbType.Structured; p3.TypeName = "dbo.DebtsTableUpdateType";

            var p4 = cmd.Parameters.AddWithValue("@History", ToDtHistory(importHistory));
            p4.SqlDbType = SqlDbType.Structured; p4.TypeName = "dbo.ImportHistoryType";

            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            return true;
        }




        //public bool QuickPrice( List<Debts> debtParms, List<DebtsTable> debtTblParms, List<RentBctTable> RentBctsParamsUpdate, List<RentBctTable> RentBctsParamsAdd)
        //{
        //    bool result;
        //    IDbConnection db = new SqlConnection(_config.GetConnectionString(Connectionstring));

        //    try
        //    {
        //        if (db.State == ConnectionState.Closed)
        //            db.Open();

        //        var tran = db.BeginTransaction();
        //        try
        //        {
        //            //UpdateRentBctTable
        //            string RentBctTableSp = "UPDATE RentBctTable SET DateEnd=@DateEnd, UpdatedAt=@UpdatedAt WHERE Id=@Id";
        //            db.Execute(RentBctTableSp, param : RentBctsParamsUpdate, tran, null, CommandType.Text);

        //            //AddRentBctTable
        //            var queryUpdateDebtsTable = "INSERT INTO DebtsTable (Id, CreatedById, UpdatedById, CreatedBy, UpdatedBy, Code, Index, DateStart, DateEnd, RentFileId, Price, Check, Executor, Date, NearestActivities, PriceDiff, AmountExclude, VATPrice, VAT, CheckPayDepartment, Type, NocReceiptId, SXN) VALUES (@Id, @CreatedById, @UpdatedById, @CreatedBy, @UpdatedBy, @Code, @Index, @DateStart, @DateEnd, @RentFileId, @Price, @Check, @Date, @NearestActivities, @PriceDiff, @AmountExclude, @VATPrice, @VAT, @CheckPayDepartment, @Type, @NocReceiptId, @SXN)";
        //            db.Execute(queryUpdateDebtsTable, param: RentBctsParamsAdd, tran, null, CommandType.Text);

        //            //Update DebtTbl
        //            string debtTblSp = "UPDATE DebtsTable SET Price=@Price, PriceDiff=@PriceDiff, VATPrice= @VATPrice WHERE Id=@Id";
        //            db.Execute(debtTblSp, param: debtTblParms, tran, null, CommandType.Text);

        //            //Update Debt
        //            string debtSp = "UPDATE Debts SET Total=@Total, Paid=@Paid, Diff= @Diff WHERE Id=@Id";
        //            db.Execute(debtSp, param: debtParms, tran, null, CommandType.Text);

        //            tran.Commit();
        //            result = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            tran.Rollback();
        //            result = false;
        //            throw ex;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = false;
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (db.State == ConnectionState.Open)
        //            db.Close();
        //    }
        //    return result;
        //}
    }
}
