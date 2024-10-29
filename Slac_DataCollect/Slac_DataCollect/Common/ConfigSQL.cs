using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using Slac_DataCollect.DatabaseSql.DBOper;
using SqlSugar;
using Slac_DataCollect.DatabaseSql.DBModel;

namespace Slac_DataCollect.Common
{
     public class ConfigSQL
    {
        public static string connStr = @"Data Source=.\System.Data.Sql.db;Initial Catalog=sqlite;Integrated Security=True;Max Pool Size=10";

        #region dashboard 看板列表

        public static DataTable Get_dashboard_list()
        {
            #region MySQL
            DBOper.Init();                                          //连接            
            DBOper dBOper = new DBOper();
            //dBOper.Insertable();                                  //插入

            DBSystemConfig dBSystemConfig = new DBSystemConfig();
            DataTable dt = dBOper.QueryDataTable(dBSystemConfig);    //查询
            //DataTable dt = dBOper.QueryDataTableCondition(dBSystemConfig, $"ID = '1' && Value = '1'"); //根据条件查询

            return dt;
            #endregion

            #region SQLite
            //SQLiteConnection conn = new SQLiteConnection(connStr);
            //conn.Open();
            //string sql = "SELECT * FROM dashboard ";
            //SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);
            //DataSet ds = new DataSet();
            //ap.Fill(ds);
            //DataTable dt = ds.Tables[0];
            //conn.Close();
            //return ds.Tables[0];
            #endregion
        }

        #endregion

        #region data2API 将需要的数据发送到REDIS,供API调用

        public static DataTable Get_data2api_list()
        {
            #region MySQL
            //DBOper.Init();
            //DBOper dBOper = new DBOper();
            //DBSystemConfig dBSystemConfig = new DBSystemConfig();
            //DataTable ds = dBOper.QueryDataTable(dBSystemConfig);    // 查询                        

            //return ds;
            #endregion

            #region SQLite
            SQLiteConnection conn = new SQLiteConnection(connStr);
            conn.Open();
            string sql = "SELECT * FROM data2api where enable=1 ";
            SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);
            DataSet ds = new DataSet();
            ap.Fill(ds);
            DataTable dt = ds.Tables[0];
            conn.Close();
            return ds.Tables[0];
            #endregion
        }

        #endregion


    }
}
