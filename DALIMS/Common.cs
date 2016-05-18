using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccessLayer
{
    public enum TransactionStatus
    {
        Fail = 0,
        Success = 1,
        AmountHigherThanBalance = 3,
        RecordNotFound = 4
    }
    public class Common
    {

        InventoryManagementDataContext _obj = new InventoryManagementDataContext();
        public int DeleteItem(Int64 ID, string tbl)
        {
            try
            {
                switch (tbl)
                {
                    case "PType":
                        var PType = (from m in _obj.ProductTypes where m.ProductTypeID == ID select m).SingleOrDefault();
                        PType.IsDeleted = true;
                        break;

                    case "Product":
                        var Product = (from m in _obj.Products where m.ProductID == ID select m).SingleOrDefault();
                        Product.IsDeleted = true;
                        break;
                    case "Customer":
                        var Customer = (from m in _obj.Customers where m.CustomerID == ID select m).SingleOrDefault();
                        Customer.isDeleted = true;
                        break;
                    case "SaleDetail":
                        var saleDetail = (from m in _obj.SaleDetails where m.SaleDetailID == ID select m).SingleOrDefault();
                        saleDetail.IsActive = false;
                        break;
                    default:
                        return -1;
                }
                _obj.SubmitChanges();
                return 1;
            }
            catch (Exception)
            {
                return -1;
            }

        }
    }
}
