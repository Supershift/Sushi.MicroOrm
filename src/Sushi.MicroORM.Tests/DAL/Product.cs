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
                Map(x => x.MetaData.Name, "Product_Name").SqlType(System.Data.SqlDbType.NVarChar);
                Map(x => x.MetaData.Description, "Product_Description").SqlType(System.Data.SqlDbType.VarChar);
                Map(x => x.Price, "Product_Price");
                Map(x => x.MetaData.Identification.ExternalID, "Product_ExternalID");
                Map(x => x.MetaData.Identification.BarCode, "Product_BarCode");
                Map(x => x.MetaData.Identification.GUID, "Product_Guid");
                Map(x => x.MetaData.ProductTypeID, "Product_ProductTypeID");
            }
        }

        //these classes are here to test mapping of 'nested' members
        public class ProductMetaData
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public ProducType? ProductTypeID { get; set; }
            public Identification Identification { get; set; }
        }

        public class Identification
        {
            public int? ExternalID { get; set; }
            public byte[] BarCode { get; set; }
            public Guid GUID { get; set; }
        }

        public int ID { get; set; }        
        public decimal Price { get; set; }
        public ProductMetaData MetaData { get; set; }
        

        public enum ProducType
        {
            Hifi = 1,
            Computer = 2,
            Living = 3,
            Food = 4
        }

        public static Product FetchSingle(int id)
        {
            var connector = new Connector<Product>();
            var result = connector.FetchSingle(id);
            return result;
        }

        public static async Task<Product> FetchSingleAsync(int id)
        {
            var connector = new Connector<Product>();
            var result = await connector.FetchSingleAsync(id);
            return result;
        }
    }
}
