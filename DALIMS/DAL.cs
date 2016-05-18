using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace DataAccessLayer
{
    public class DAL
    {
        InventoryManagementDataContext _obj = new InventoryManagementDataContext();
        static void Main(string[] args)
        {

        }

        public int AddProductType(string PType)
        {
            var tbl = from m in _obj.ProductTypes where m.PType.ToLower() == PType.ToLower() && (m.IsDeleted ?? false) == false select m;
            if (tbl.Count() > 0)
            {
                return 0;
            }
            ProductType productType = new ProductType();
            productType.PType = PType;
            try
            {
                _obj.ProductTypes.InsertOnSubmit(productType);
                _obj.SubmitChanges();
                return productType.ProductTypeID;
            }
            catch (Exception)
            {
                return -1;
            }

        }
        public int UpdateProductType(string PType, int ID)
        {
            var productType = (from p in _obj.ProductTypes where p.PType.ToLower() == PType.ToLower() && p.ProductTypeID != ID select p).SingleOrDefault();

            if (productType != null)//if already exists
                return 0;
            try
            {
                productType = (from p in _obj.ProductTypes where p.ProductTypeID == ID select p).SingleOrDefault();
                productType.PType = PType;
                _obj.SubmitChanges();
                return 1;
            }
            catch (Exception)
            {
                return -1;
            }

        }
        public IEnumerable<ProductType> GetProductTypes()
        {
            return from m in _obj.ProductTypes where (m.IsDeleted == false || m.IsDeleted == null) select m;
        }
        public IQueryable GetProducts()
        {
            return from m in _obj.Products join n in _obj.ProductTypes on m.ProductTypeID equals n.ProductTypeID where (m.IsDeleted == false || m.IsDeleted == null) select m;
        }
        //public int AddProduct(Product ProductInfo)
        //{
        //    var tbl = from m in _obj.Products where m.ProductName.ToLower() == ProductInfo.ProductName.ToLower() && m.ManufacturedBy.ToLower() == ProductInfo.ManufacturedBy.ToLower() select m;
        //    if (tbl.Count() > 0)
        //        return 0;
        //    try
        //    {
        //        _obj.Products.InsertOnSubmit(ProductInfo);
        //        CreatProductHistory(ProductInfo);
        //        _obj.SubmitChanges();
        //        return ProductInfo.ProductID;
        //    }
        //    catch (Exception)
        //    {
        //        return -1;
        //    }
        //}
        public int AddUpdateProduct(int ProductID, int PTypeID, string ProductName, string MfdBy, string mfd, string expDate, int PurchasePrice, int SellingPrice, int Itemcount)
        {
            try
            {
                Product productInfo;
                productInfo = (from p in _obj.Products where p.ProductName.ToLower() == ProductName.ToLower() && p.ProductID != ProductID && (p.IsDeleted ?? false) == false select p).SingleOrDefault();
                if (productInfo != null)//if already exists
                    return 0;
                if (ProductID > 0)
                    productInfo = (from p in _obj.Products where p.ProductID == ProductID select p).SingleOrDefault();
                else
                    productInfo = new Product();
                productInfo.ProductTypeID = PTypeID;
                productInfo.ProductName = ProductName;
                productInfo.ManufacturedBy = MfdBy;
                DateTime enteredDate;
                DateTime.TryParseExact(mfd, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out enteredDate);
                productInfo.MfgDate = enteredDate;
                DateTime.TryParseExact(expDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out enteredDate);
                productInfo.ExpiryDate = enteredDate;
                productInfo.PurchasePrice = Convert.ToInt32(PurchasePrice);
                productInfo.SellingPrice = Convert.ToInt32(SellingPrice);

                productInfo.NumberOfItems = Convert.ToInt32(Itemcount);
                if (ProductID <= 0)
                {
                    productInfo.CreateDate = DateTime.Now;
                    _obj.Products.InsertOnSubmit(productInfo);
                }
                CreatProductHistory(productInfo);
                _obj.SubmitChanges();
                return productInfo.ProductID;
            }
            catch (Exception)
            {
                return -1;
            }

        }

        private void CreatProductHistory(Product product)
        {
            ProductHistory prodhistory = new ProductHistory();
            prodhistory.CreateDate = product.CreateDate;
            prodhistory.ExpiryDate = product.ExpiryDate;
            prodhistory.ManufacturedBy = product.ManufacturedBy;
            prodhistory.MfgDate = product.MfgDate;
            prodhistory.Product = product;
            prodhistory.ProductName = product.ProductName;
            prodhistory.ProductTypeID = product.ProductTypeID;
            prodhistory.PurchasePrice = product.PurchasePrice;
            prodhistory.SellingPrice = product.SellingPrice;
            _obj.ProductHistories.InsertOnSubmit(prodhistory);
        }
        public IEnumerable<Customer> GetCustomers()
        {
            return from m in _obj.Customers where (m.isDeleted == false || m.isDeleted == null) select m;
        }
        public int AddCustomer(string CName, string FareName)
        {
            var tbl = from m in _obj.Customers where m.CustomerName.ToLower() == CName.ToLower() && m.FareName.ToLower() == FareName.ToLower() select m;
            if (tbl.Count() > 0)
                return 0;
            Customer customer = new Customer();
            try
            {
                customer.CustomerName = CName;
                customer.FareName = FareName;
                _obj.Customers.InsertOnSubmit(customer);
                _obj.SubmitChanges();
                return customer.CustomerID;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public int UpdateCustomer(int ID, string CName, string FareName)//int ProductId,int PTypeId,string ProductName,string MfdBy,DateTime mfd,DateTime expDate,int PurchasePrice,int SellingPrice
        {
            Customer customer = (from c in _obj.Customers where c.CustomerName.ToLower() == CName.ToLower() && c.CustomerID != ID select c).SingleOrDefault();

            if (customer != null)
                return 0;
            try
            {
                var customer1 = (from p in _obj.Customers where p.CustomerID == ID select p).SingleOrDefault();
                customer1.CustomerName = CName;
                customer1.FareName = FareName;
                _obj.SubmitChanges();
                return 1;
            }
            catch (Exception)
            {
                return -1;
            }

        }

        public IEnumerable<SaleDetail> GetSaleDetails(InventoryManagementDataContext ivtDataContext, int SaleID)
        {
            return ivtDataContext.SaleDetails.Where(s => s.SaleID == SaleID && (s.IsActive ?? true) == true);
        }
        /// <summary>
        /// Retruns all the sales for a customer
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="ivtDataContext"></param>
        /// <returns></returns>
        public IEnumerable<SaleDetail> GetSaleDetails(int customerID, InventoryManagementDataContext ivtDataContext)
        {
            return ivtDataContext.SaleDetails.Where(s => s.Sale.CustomerID == customerID && (s.IsActive ?? true) == true);
        }
        public IEnumerable<SaleDetail> GetSaleDetails(InventoryManagementDataContext ivtDataContext, List<Int64> saleIDs)
        {
            return ivtDataContext.SaleDetails.Where(s => saleIDs.Contains(s.SaleID) && (s.IsActive ?? true) == true);
        }
        public Sale GetSaleInfo(InventoryManagementDataContext ivtDataContext, int SaleID)
        {
            return ivtDataContext.Sales.FirstOrDefault(s => s.SaleID == SaleID);
        }

        public Int64 NewSaleId()
        {
            Sale Sale = (from m in _obj.Sales orderby m.SaleID descending select m).FirstOrDefault();
            if (Sale == null)//if First Case
                return 1;
            return Sale.SaleID + 1;//get next sale to add
        }
        public Int64 CreateNewSale(InventoryManagementDataContext ivtDataContext, int customerID)
        {
            Sale s = new Sale();
            s.CustomerID = customerID;
            s.SaleStartDate = DateTime.Now;
            ivtDataContext.Sales.InsertOnSubmit(s);
            ivtDataContext.SubmitChanges();
            return s.SaleID;
        }
        //public string AddSaleProduct(int ProductID, Int64 SaleID, int CustomerID, int SalePrice, int NoOfItemsToSale, int Discount)
        public string AddSaleProduct(int ProductID, Int64 SaleID, int SalePrice, int NoOfItemsToSale, int Discount)
        {
            try
            {
                if (SaleID == 0)
                {
                    Sale s = new Sale();
                    // s.CustomerID = CustomerID;
                    s.SaleStartDate = DateTime.Now;
                    _obj.Sales.InsertOnSubmit(s);
                    _obj.SubmitChanges();
                    SaleID = s.SaleID;
                }
                Product p = _obj.Products.Where(i => i.ProductID == ProductID).FirstOrDefault();
                if (p != null)
                {
                    SaleDetail saleDetails = new SaleDetail();
                    saleDetails.SaleID = SaleID;
                    saleDetails.ProductID = p.ProductID;
                    saleDetails.PurchasePrice = p.PurchasePrice;
                    saleDetails.SalePrice = SalePrice;
                    saleDetails.NoOfItems = NoOfItemsToSale;
                    saleDetails.Discount = Discount;
                    saleDetails.IsActive = true;
                    _obj.SaleDetails.InsertOnSubmit(saleDetails);
                    p.NumberOfItems -= NoOfItemsToSale;
                    _obj.SubmitChanges();
                    return "{\"saleItemID\":" + saleDetails.SaleDetailID + ",\"saleID\":" + SaleID + "}";//id of item Added
                }
                return "-1";
            }
            catch (Exception)
            {
                return "-1";
            }
        }
    }
}
