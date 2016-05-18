using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccessLayer
{
    public partial class Sale
    {
        private int? _TotalSalePrice;
        public int TotalSalePrice
        {
            get
            {
                if (_TotalSalePrice == null)
                {
                    _TotalSalePrice = this.SaleDetails.Where(s => (s.IsActive ?? true) == true).Sum(sd => ((sd.SalePrice * sd.NoOfItems) - (sd.Discount ?? 0)));
                }
                return _TotalSalePrice.Value;
            }
        }
        public int PaymentPending
        {
            get
            {
                return TotalSalePrice - (this.AmountRecieved ?? 0);
            }
        }
        private int? _Profit;
        public int ProfitOnSale
        {
            get
            {
                if (_Profit == null)
                {
                    _Profit = this.SaleDetails.Where(s => (s.IsActive ?? true) == true).Sum(sd => ((sd.SalePrice * sd.NoOfItems) - (sd.Discount ?? 0)) - (sd.PurchasePrice * sd.NoOfItems));
                }
                return _Profit.Value;
            }
        }
        public static IEnumerable<Sale> GetSales(InventoryManagementDataContext imsDataContext)
        {
            return imsDataContext.Sales.Where(s => (s.Customer.isDeleted ?? false) == false).OrderByDescending(s => s.SaleID);
        }
        /// <summary>
        /// Sales on Start Date and Later
        /// </summary>
        /// <param name="imsDataContext"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IEnumerable<Sale> GetSales(InventoryManagementDataContext imsDataContext, DateTime startDate)
        {
            return imsDataContext.Sales.Where(s => s.SaleStartDate.Value.Date >= startDate.Date).OrderByDescending(s => s.SaleID);
        }
        public static IEnumerable<Sale> GetSales(InventoryManagementDataContext imsDataContext, int customerID)
        {
            return imsDataContext.Sales.Where(s => s.CustomerID == customerID);
        }
    }
}
