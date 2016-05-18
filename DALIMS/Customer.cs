using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccessLayer
{
    public partial class Customer
    {
        private int? _PaymentRecieved;
        private int? _PaymentPending;
        private int? _TotalLedgerAmount;
        public int PaymentRecieved
        {
            get
            {
                if (_PaymentRecieved == null)
                    _PaymentRecieved = this.Sales.Sum(s => s.AmountRecieved ?? 0);
                return _PaymentRecieved ?? 0;
            }
        }
        public int PaymentPending
        {
            get
            {
                if (_PaymentPending == null)
                    _PaymentPending = (this.Sales.Sum(s => s.TotalSalePrice) - this.Sales.Sum(s => s.AmountRecieved ?? 0));
                return _PaymentPending ?? 0;
            }
        }
        public int TotalLedgerAmount
        {
            get
            {
                if (_TotalLedgerAmount == null)
                    _TotalLedgerAmount = this.Sales.Sum(s => s.TotalSalePrice);
                return _TotalLedgerAmount ?? 0;
            }
        }

        public static Customer GetCustomerDetails(InventoryManagementDataContext imsDataContext, int customerID)
        {
            return imsDataContext.Customers.FirstOrDefault(c => c.CustomerID == customerID);
        }
    }
}
