using Slac_DataCollect.DatabaseSql.DBModel;
using Slac_DataCollect.DatabaseSql.DBOper;
using Slac_DataCollect.Modules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slac_DataCollect.Common
{
    /// <summary>
    /// 参数类：获取公共参数配置
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// 系统配置参数
        /// </summary>
        

        /// <summary>
        /// 获取参数配置
        /// </summary>
        ///<param name="paramType">参数类型</param>
        public static void GetParamConfig(ref Parameter sysConfigParam)
        {
            try
            {                               
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取参数配置异常：{ex.Message}");
                throw;
            }
        }


    }
}
