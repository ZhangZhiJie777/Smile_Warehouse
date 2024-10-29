using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slac_DataCollect.DatabaseSql.DBModel
{
    /// <summary>
    /// 实体类_测试
    /// </summary>
    [SugarTable("system_config")]
    public class DBSystemConfig
    {
        ///<summary> 
        /// 主键ID
        ///</summary>
        [SugarColumn(ColumnName = "ID", IsPrimaryKey = true, IsIdentity = true),]
        public int ID { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        [SugarColumn(ColumnName = "Name", IsNullable = true)]
        public string Name { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        [SugarColumn(ColumnName = "Value", IsNullable = true, ColumnDataType = "Nvarchar(255)")]
        public string Value { get; set; }

        /// <summary>
        /// 参数描述
        /// </summary>
        [SugarColumn(ColumnName = "Description", IsNullable = true, ColumnDataType = "Nvarchar(255)")]
        public string Description { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        [SugarColumn(ColumnName = "parms_type", IsNullable = true, ColumnDataType = "Nvarchar(255)")]//自定格式的情况 length不要设置
        public string ParmsType { get; set; }
        
        
        

    }
}
