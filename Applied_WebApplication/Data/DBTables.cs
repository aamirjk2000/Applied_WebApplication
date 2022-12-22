﻿using System.Data;

public enum Tables
{
    COA=101,
    COA_Nature = 102,
    COA_Class = 103,
    COA_Notes = 104,
    
    Customers = 201,
    City = 202,
    Country = 203,
    
    Inventory = 301,
    Inv_Category = 302,
    Inv_SubCategory = 303,
    Inv_Packing = 304,
    Inv_UOM = 305


}


public enum Messaages
{
    Record_Saved=100,
    Record_not_Saved=101

}


public enum CommandAction 
{
    Insert,
    Update,
    Delete
}