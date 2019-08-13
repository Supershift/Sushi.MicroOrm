using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Tests.DAL
{   
    public class Product
    {
        public class ProductMap : DataMap<Product>
        {
            public ProductMap()
            {
                Table("cat_Products");
                Id(x => x.ID, "Product_Key").Identity();
                Map(x => x.Name, "Product_Name").SqlType(System.Data.SqlDbType.NVarChar);
                Map(x => x.Description, "Product_Description").SqlType(System.Data.SqlDbType.VarChar);
                Map(x => x.Price, "Product_Price");
                Map(x => x.ExternalID, "Product_ExternalID");
                Map(x => x.BarCode, "Product_BarCode");
                Map(x => x.GUID, "Product_Guid");
                Map(x => x.ProductTypeID, "Product_ProductTypeID");
            }
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int? ExternalID { get; set; }
        public byte[] BarCode { get; set; }
        public Guid GUID { get; set; }
        public ProducType? ProductTypeID { get; set; }

        public enum ProducType
        {
            Hifi = 1,
            Computer = 2,
            Living = 3,
            Food = 4
        }
    }
}
