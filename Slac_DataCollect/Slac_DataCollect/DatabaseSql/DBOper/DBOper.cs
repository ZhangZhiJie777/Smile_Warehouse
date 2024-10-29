using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using System.Xml.Linq;
using Slac_DataCollect.DatabaseSql.DBModel;
using SqlSugar;
using System.Data;

namespace Slac_DataCollect.DatabaseSql.DBOper
{
    public class DBOper
    {
        private static SqlSugarClient db;
        readonly object lockObj = new object();
        private static string connstr = System.Configuration.ConfigurationManager.AppSettings["MySqlPath"]; //采用@ ，否则？

        /// <summary>
        /// 初始连接数据库
        /// </summary>
        public static void Init()
        {
            try
            {
                if (db == null)
                {
                    db = new SqlSugarClient(new ConnectionConfig()
                    {
                        //Server：表示数据库地址    uid：表示数据库管理员id        pwd：表示数据库管理员密码     database：表示连接数据库的库名(如果没有可以自定义，调用 mDB.DbMaintenance.CreateDatabase()会生成)

                        //ConnectionString = "Database = 004standardframe; Server = localhost; Uid = root; Pwd = 123456; Port = 3306; charset=utf8mb4;",
                        ConnectionString = connstr,
                        DbType = SqlSugar.DbType.MySql,     // 设置数据库类型     
                        IsAutoCloseConnection = true,       // 自动释放数据务，如果存在事务，在事务结束后释放     
                        InitKeyType = InitKeyType.Attribute // 从实体特性中读取主键自增列信息  
                    });

                    //用来打印Sql方便调试   
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        Console.WriteLine(sql + "\r\n" +
                        db.Utilities.SerializeObject(pars.ToString()));
                        Console.WriteLine();
                    };
                }

                ////创建数据库
                //db.DbMaintenance.CreateDatabase();

                //初始化数据表，如果没有则创建
                //db.CodeFirst.InitTables(typeof(DBSystemConfig));
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("output\\" + DateTime.Now.ToString("yyyyMMdd_") + ".log", "Error:" + ex.ToString() + " @ " + DateTime.Now.ToString() + "\r\n\r\n");
                Console.WriteLine("数据库建立连接异常：" + ex.Message);
            }


        }

        #region 插入
        /// <summary>
        /// 插入数据
        /// </summary>
        public void Insertable()
        {
            //在表的末尾只插入指定的列数据
            DBSystemConfig userData = new DBSystemConfig()
            {
                ID = 66,
                Name = "参数名称",
                Value = "参数值",
                ParmsType = "参数类型",
                Description = "参数描述：指定列插入测试，返回自增列序号"
            };

            Insert(userData);

        }

        /// <summary>
        /// 通用插入方法
        /// </summary>
        public bool Insert<T>(T entity) where T : class, new()
        {
            try
            {
                // 使用 AOP 特性来动态生成表名并执行插入操作  
                var result = db.Insertable(entity).ExecuteCommand();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"插入数据时发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 插入数据到表中，插入时上锁 (锁是保证在高并发修改数据时数据的完整性，保证在同一时间只能由一个Task去修改该数据
        /// 从而避免同时间有多个Task去修改该数据导致数据的异常)
        /// </summary>
        public bool InsertWithUpLock<T>(T entity) where T : class, new()
        {
            try
            {
                // 使用 AOP 特性来动态生成表名并执行插入操作  
                var result = db.Insertable(entity).With("SQLLock").ExecuteCommand();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"插入数据时发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 插入数据_测试
        /// </summary>
        public void InsertableTest()
        {
            ////在表的末尾插入一条数据  返回值是插入数据的个数
            //DBSystemConfig userdata1 = new DBSystemConfig() { ID = 4, Name = "zm100", RegisterTime = DateTime.Now, signture = "数据插入测试1" };
            //int count = db.Insertable(userdata1).ExecuteCommand();
            //Console.WriteLine($"插入了 {count} 条数据");

            ////在表的末尾插入一条数据  返回值是插入成功的自增列
            //DBSystemConfig userdata2 = new DBSystemConfig() { ID = 6, Name = "zm100", RegisterTime = DateTime.Now, signture = "数据插入测试1，返回自增列" };
            //int column = db.Insertable(userdata2).ExecuteReturnIdentity();
            //Console.WriteLine($"在数据库中插入了一条数据，自增列数值为：" + column);

            ////在表的末尾插入一条数据，返回值是插入成功的实体对象
            //DBSystemConfig userData3 = new DBSystemConfig() { ID = 5, Name = "zm200", RegisterTime = DateTime.Now, signture = "返回对象数据插入测试" };
            //DBSystemConfig userData = db.Insertable(userData3).ExecuteReturnEntity();
            //Console.WriteLine($"数据插入成功 插入对象 {userData.Name} 行插入了一条数据");
        }
        #endregion

        #region 查询
        /// <summary>
        /// 查询表中的所有数据，返回DataTable
        /// </summary>
        public DataTable QueryDataTable<T>(T entity) where T : class, new()
        {
            try
            {
                lock (lockObj)
                {
                    DataTable dataTable = db.Queryable<T>().ToDataTable();
                    return dataTable;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查询数据异常{ex.Message}");
                return null;
            }

        }

        /// <summary>
        /// 根据条件查询表中数据，返回DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="whereConditions">where条件</param>
        /// <returns></returns>
        public DataTable QueryDataTableCondition<T>(T entity, string whereConditions) where T : class, new()
        {
            try
            {
                lock (lockObj)
                {
                    var param = db.Queryable<T>().Where(whereConditions);
                    DataTable dataTable = param.ToDataTable();
                    if (dataTable != null)
                    {
                        return dataTable;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"根据条件查询数据异常：{ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 查询表中数据，返回List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<T> QueryList<T>(T entity) where T : class, new()
        {
            try
            {
                lock (lockObj)
                {
                    List<T> list = db.Queryable<T>().ToList();                    
                    if (list != null)
                    {
                        return list;
                    }   
                    return new List<T>(); 

                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("output\\" + DateTime.Now.ToString("yyyyMMdd_") + ".log", "Error:" + ex.ToString() + " @ " + DateTime.Now.ToString() + "\r\n\r\n");
                Console.WriteLine($"查询数据到列表异常： {ex.Message}");
                return new List<T>();
            }
        }

        /// <summary>
        /// 根据条件查询表中数据，返回List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="whereConditions"></param>
        /// <returns></returns>
        public List<T> QueryListCondition<T>(T entity, string whereConditions) where T : class, new()
        {
            try
            {
                lock (lockObj)
                {                     
                    List<T> list = db.Queryable<T>().Where(whereConditions).ToList();
                    if (list != null)
                    {
                        return list;
                    }
                    return new List<T>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查询数据到列表异常： {ex.Message}");
                return new List<T>();
            }
        }
        #endregion
    }
}
