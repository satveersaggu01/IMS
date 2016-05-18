using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace DataAccessLayer
{
    public partial class BalanceInfo
    {
        public static TransactionStatus UpdateRecievedCustomerBalance(int customerID, int newRecievedAmount)
        {
            int recievedAmountBalance = newRecievedAmount;
            using (InventoryManagementDataContext ivtDataContext = new InventoryManagementDataContext())
            {
                List<Sale> sales = Sale.GetSales(ivtDataContext, customerID).ToList();
                sales.ForEach(sale =>
                {
                    int saleAmount = sale.TotalSalePrice;
                    int currentSaleBalance = saleAmount - (sale.AmountRecieved ?? 0);
                    if (currentSaleBalance > 0)
                    {
                        if (currentSaleBalance <= recievedAmountBalance)//enough balance to clear the sale
                        {
                            sale.AmountRecieved = saleAmount;//all amount recieved for sale
                            recievedAmountBalance -= currentSaleBalance;
                        }
                        else
                        {
                            sale.AmountRecieved = (sale.AmountRecieved ?? 0) + recievedAmountBalance;//amount recieved is less than amount
                            recievedAmountBalance = 0;
                        }
                        sale.LastRecievedDate = DateTime.Now;
                    }
                    if (recievedAmountBalance == 0)
                        return;
                });
                if (recievedAmountBalance == 0)//in case all the amount devided for payment pending
                {
                    #region  add balance details info
                    BalanceInfo balanceInfo = new BalanceInfo();
                    balanceInfo.CusomerID = customerID;
                    balanceInfo.AmountRecieved = newRecievedAmount;
                    balanceInfo.ReturnDate = DateTime.Now;
                    ivtDataContext.BalanceInfos.InsertOnSubmit(balanceInfo); 
                    #endregion

                    ivtDataContext.SubmitChanges();
                    return TransactionStatus.Success;
                }
                else
                {
                    return TransactionStatus.AmountHigherThanBalance;
                }
            }
        }
        public static TransactionStatus UpdateRecievedSaleBalance(int saleID, int newRecievedAmount)
        {
            DAL cls = new DAL();
            using (InventoryManagementDataContext ivtDataContext = new InventoryManagementDataContext())
            {
                Sale sale = cls.GetSaleInfo(ivtDataContext, saleID);
                if (sale != null)
                {
                    int saleAmount = sale.TotalSalePrice;
                    if ((sale.AmountRecieved ?? 0) + newRecievedAmount <= saleAmount)
                    {
                        sale.AmountRecieved = (sale.AmountRecieved ?? 0) + newRecievedAmount;
                        sale.LastRecievedDate = DateTime.Now;
                        #region  add balance details info
                        BalanceInfo balanceInfo = new BalanceInfo();
                        balanceInfo.CusomerID = sale.CustomerID.Value;
                        balanceInfo.SaleID = sale.SaleID;
                        balanceInfo.AmountRecieved = newRecievedAmount;
                        balanceInfo.ReturnDate = DateTime.Now;
                        ivtDataContext.BalanceInfos.InsertOnSubmit(balanceInfo);
                        #endregion
                        ivtDataContext.SubmitChanges();
                        return TransactionStatus.Success;
                    }
                    else
                    {
                        return TransactionStatus.AmountHigherThanBalance;
                    }
                }
                return TransactionStatus.RecordNotFound;
            }
        }
    }
}
