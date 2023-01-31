﻿using System.Data;

namespace Applied_WebApplication;

public enum Tables
{
    Registry = 1,

    COA = 101,
    COA_Nature = 102,
    COA_Class = 103,
    COA_Notes = 104,
    CashBook = 105,
    WriteCheques = 106,
    Taxes =107,
    ChequeTranType = 108,
    ChequeStatus = 109,
    TaxTypeTitle = 110,
    BillPayable = 111,
    BillPayable2 = 112,
    

    Customers = 201,
    City = 202,
    Country = 203,
    Project = 204,
    Employees = 205,

    Inventory = 301,
    Inv_Category = 302,
    Inv_SubCategory = 303,
    Inv_Packing = 304,
    Inv_UOM = 305,

    Ledger = 401,
    view_Ledger = 402,

    PostCashBook = 501,
    PostBankBook = 502,
    PostWriteCheque = 503,
    PostBillReceivable = 504,
    PostBillPayable = 505,
    PostPayments = 506,
    PostReceipts = 507,

    fun_BillPayableAmounts = 601,                    // Function of Bill Payable Amount and Tax Amount
    fun_BillPayableEntry = 602


}

public enum CommandAction
{
    Insert,
    Update,
    Delete
}

public enum KeyType
{
    Number,
    Currency,
    Date,
    Boolean,
    Text,
    UserName,
    From,
    To,
    FromTo,
}

public enum PostType
{
    CashBook = 1,
    Bankbook = 2,
    WriteCheque = 3,
    BillPayable = 4,
    BillReceivable = 5,
    Payment = 6,
    Receipt = 7
}

public enum VoucherStatus
{
    Submitted = 1,
    Posted = 2,
}

