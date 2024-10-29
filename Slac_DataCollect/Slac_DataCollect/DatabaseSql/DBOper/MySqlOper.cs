using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace Slac_DataCollect.DatabaseSql.DBOper
{
    public class MySqlOper
    {
        private static string connstr = System.Configuration.ConfigurationManager.AppSettings["MySqlPath"]; //采用@ ，否则？

        //private static string connstr = "";
        /// <summary>
        /// 查询sql,返回DataTable
        /// </summary>
        /// <returns></returns>
        //public static DataTable QuerySqlDataSst(string sql)
        //{
        //    using (MySqlConnection conn = new MySqlConnection(connstr))
        //    {
        //        DataTable ds = new DataTable();
        //        try
        //        {
        //            conn.Open();
        //            MySqlCommand cmd = conn.CreateCommand();//命令对象（用来封装需要在数据库执行的语句）
        //            MySqlDataAdapter DataAdapter = new MySqlDataAdapter(sql, conn);
        //            DataAdapter.Fill(ds);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //        finally
        //        {
        //            conn.Close();
        //        }
        //        return ds;
        //    }

        //}

        /// <summary>
        /// 查询sql,返回DataTable
        /// </summary>
        /// <returns></returns>
        public static DataTable QuerySqlData(string sql)
        {
            DataTable dataTable = new DataTable();
            //using (MySqlConnection conn = new MySqlConnection(connstr))
            //{
            //    try
            //    {
            //        conn.Open();
            //        using (MySqlCommand cmd = new MySqlCommand(sql, conn)) // 使用 MySqlCommand，但在这个场景下其实不需要，因为我们只用到了 DataAdapter
            //        {
            //            // 注意：这里我们没有使用 MySqlCommand，因为 DataAdapter 已经封装了命令的执行
            //            // 如果需要执行非查询命令（如 INSERT、UPDATE、DELETE），则应该使用 MySqlCommand 的 ExecuteNonQuery 方法

            //            using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sql, conn))
            //            {
            //                dataAdapter.Fill(dataTable);
            //            }
            //        }
            //    }
            //    catch (MySqlException ex) // 捕获 MySQL 特定的异常
            //    {
            //        // 这里可以记录日志或执行其他错误处理逻辑
            //        // 例如：LogError(ex.Message);
            //        throw new Exception("Database query failed.", ex); // 重新抛出一个包含更多上下文信息的异常
            //    }
            //    catch (Exception ex) // 捕获其他类型的异常
            //    {
            //        // 同上，可以记录日志或执行其他错误处理逻辑
            //        throw; // 或者重新抛出一个异常，或者根据需要选择其他处理方式
            //    }
            //}
            // 注意：conn.Close() 是不必要的，因为 using 语句会自动处理资源的释放（包括关闭连接）
            return dataTable;
        }
    }
}