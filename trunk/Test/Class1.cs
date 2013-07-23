using AdChina.Base.Data.DBClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdChina.Base.Data.Extentions;
using AdChina.Base.Data.Mapping;
using AdChina.Base.Reflection;
using AdChina.Base.Dynamic;
using AdChina.Base.Data.Dynamic.Extentions;
using AdChina.Base.Data.Provider;
using System.Text.RegularExpressions;

namespace Test
{
    public class Class1
    {
        public void Test()
        {
            var helper = DbHelper.GetHelper();//这是获取名称为ConnectionString的连接字符串数据对象
            helper = DbHelper.GetHelper("ConnectionString2");//也可以设置要用的连接字符串

            //事务使用要和DbHelper是同样连接才可以使用
            using (helper = DbHelper.GetHelper())
            {
                using (AdTransactionScope scope = new AdTransactionScope())
                {
                    helper.ExecuteNonQuery("update book set id = 1");
                }
            }

            //事务使用要和DbHelper是同样连接才可以使用
            using (helper = DbHelper.GetHelper("ConnectionString2"))
            {
                using (AdTransactionScope scope = new AdTransactionScope("ConnectionString2"))
                {
                    helper.ExecuteNonQuery("update book set id = 1");
                }
            }

            //进行DataReader查询实体时可以进行简化，字段名与属性名一样且数据类型对应
            DbHelper.GetHelper().ExecuteDataReader("select * from book").ToEnumerable<Book>();

            //字段名称不对应时，可以通过映射来对应
            DbHelper.GetHelper().ExecuteDataReader("select * from book").ToEnumerable<EBook>();

            //如果一个实体在多个sql查询中有不同的字段名，也可以映射
            DbHelper.GetHelper().ExecuteDataReader("select Price as price1 , Price as price2 from book").ToEnumerable<EBook2>();

            //也可以在执行时手工映射
            DbHelper.GetHelper().ExecuteDataReader("select Price as price1 , Price as price2 from book").ToEnumerable<EBook3>
                (
                new Dictionary<string, string>
                {
                    {"price1","BookPrice"},
                    {"price2","BookPrice"}
                });

            //切记，字段类型要和属性类型匹配
            /*
             * 增加这两个命名空间后，可以使用ToEnumerable生成动态类型实体。动态类型实体根据sql返回的结果生成实体类，并缓存类型。
             *using AdChina.Base.Dynamic;
             *using AdChina.Base.Data.Dynamic.Extentions; 
             *dynamic使用时需要另加Microsoft.CSharp程序集引用
             */
            foreach (dynamic item in DbHelper.GetHelper().ExecuteDataReader("select CREATE_TIME from mobile_sync_balance").ToEnumerable())
            {
                Console.WriteLine(item.CREATE_TIME);
            }
        }

        public void TestSqlBuilder()
        {
            var sql = Sql.Builder
            .Select("*")
            .From()
            .Select("*")
            .From("articles")
            .Where("date_created < @0", DateTime.UtcNow)
            .OrderBy("date_created DESC").Build();
            Console.WriteLine(sql.SQL);
        }
        // Helper to handle named parameters from object properties 
    }


    /*
        * 表结构
        * Book (Id int autoincrement, Name varchar, Price decimal)
        */
    public class Book
    {
        public Book() { }
        public Book(int id, string name, double price)
        {
            Id = id;
            Name = name;
            Price = price;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    public class EBook
    {
        public int Id { get; set; }

        [DataField(FieldNames = new string[] { "Name" })]
        public string BookName { get; set; }

        public decimal Price { get; set; }
    }

    public class EBook2
    {
        public int Id { get; set; }

        public string BookName { get; set; }

        [DataField(FieldNames = new string[] { "price1", "price2" })]
        public decimal BookPrice { get; set; }
    }

    public class EBook3
    {
        public int Id { get; set; }

        public string BookName { get; set; }

        public decimal BookPrice { get; set; }
    }

    public class MobileSyncBalance
    {
        [DataField(FieldName = "create_date")]
        public decimal CreateDate { get; set; }
        [DataField(FieldName = "media_buy_id")]
        public int MediaBuyId { get; set; }
        [DataField(FieldName = "last_saved_time")]
        public DateTime LastSavedTime { get; set; }
    }
}
