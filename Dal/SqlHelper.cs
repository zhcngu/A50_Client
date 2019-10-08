
using OPC_UA_Client_A50.Comom;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_UA_Client_A50.Dal
{
    public class SqlHelper
    {
        private string _constr;

        public SqlHelper()
        {
            _constr = ConfigHelper.GetConfigValue("SqlServer_DB");
            //sqlcon = new SqlConnection(_constr);
        }
        public DataTable GetDataTb(string sqlstr)
        {
            DataTable tb;
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(_constr))
                {
                    sqlcon.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlstr, sqlcon))
                    {
                        tb = new DataTable();
                        adapter.Fill(tb);
                        sqlcon.Close();
                        return tb;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "SqlLog");
                tb = null;
                return tb;
            }

        }

        /// <summary>
        /// 返回第一行第一列的方法
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <returns></returns>
        public object GetObjectVal(string sqlstr)
        {
            try
            {
                object val;
                using (SqlConnection sqlcon = new SqlConnection(_constr))
                {
                    sqlcon.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlstr, sqlcon))
                    {
                        val = cmd.ExecuteScalar();
                        return val;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "SqlLog");
                return null;
            }

        }


        /// <summary>
        /// 启用事务，实现对数据库增删改，事务回滚或者出错时候，返回-1
        /// </summary>
        /// <param name="sqlstr">sql语句</param>
        /// <returns></returns>

        public int ExecNonQuery(string sqlstr)
        {
            int result;
            using (SqlConnection sqlcon = new SqlConnection(_constr))
            {
                sqlcon.Open();
                SqlCommand cmd = new SqlCommand();
                SqlTransaction myTran = sqlcon.BeginTransaction();
                try
                {
                    cmd.Connection = sqlcon;
                    cmd.Transaction = myTran;
                    cmd.CommandText = sqlstr;
                    result = cmd.ExecuteNonQuery();
                    myTran.Commit();
                    sqlcon.Close();
                }
                catch (Exception ex)
                {
                    LogHelper.Write("提交事务出错:"+ex.Message, "SqlLog");
                    myTran.Rollback();
                    result = -1;
                    sqlcon.Close();
                }
            }
            return result;
        }

        public int ExecNonQuery(List<string> sqllist)
        {
            int result;
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(_constr))
                {
                    sqlcon.Open();
                    SqlCommand cmd = new SqlCommand();
                    SqlTransaction myTran = sqlcon.BeginTransaction();
                    try
                    {
                        result = 0;
                        foreach (string sqlstr in sqllist)
                        {
                            cmd.Connection = sqlcon;
                            cmd.Transaction = myTran;
                            cmd.CommandText = sqlstr;
                            result += cmd.ExecuteNonQuery();
                        }
                        myTran.Commit();
                        sqlcon.Close();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Write("提交事务出错:"+ex.Message, "SqlLog");
                        myTran.Rollback();
                        result = -1;
                        sqlcon.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "SqlLog");
                result = -1;
            }
            return result;
        }
    }
}
