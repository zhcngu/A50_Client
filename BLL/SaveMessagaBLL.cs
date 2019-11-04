using OPC_UA_Client_A50.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_UA_Client_A50.BLL
{
    public class SaveMessagaBLL
    {
        SqlHelper sqlHelper { get; set; }

        public SaveMessagaBLL()
        {
            sqlHelper = new SqlHelper();
        }
        public void WriteMessage(string  message,string stncode)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("insert into tblLogInfo (LogLevel,OccruTime,UserCode,StationNo,Info)  values ('{0}','{1}','{2}','{3}','{4}')","1",DateTime.Now,"",stncode,message);
            sqlHelper.ExecNonQuery(sb.ToString());

        }
       
    }
}
