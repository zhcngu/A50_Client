
using OPC_UA_Client_A50.Comom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_UA_Client_A50.Model
{
   public class APPConfiguration
    {
        
        public string SqlServer_DB { get; set; }
        public string OPCServerUrl { get; set; }
        public string OPCServerNameSpace { get; set; }
       /// <summary>
        /// 发布时间间隔
       /// </summary>
        public string PublishingInterval { get; set; }
       /// <summary>
        /// 采样频率
       /// </summary>
        public string SamplingRate { get; set; }
        public string WebSocketIP { get; set; }
        public string WebSocketPort { get; set; }

        public void GetAppConfig()
        {
            SqlServer_DB = ConfigHelper.GetConfigValue("SqlServer_DB");
            OPCServerUrl = ConfigHelper.GetConfigValue("OPCServerUrl");
            OPCServerNameSpace = ConfigHelper.GetConfigValue("OPCServerNameSpace");
            PublishingInterval = ConfigHelper.GetConfigValue("PublishingInterval");
            SamplingRate = ConfigHelper.GetConfigValue("SamplingRate");
            WebSocketIP = ConfigHelper.GetConfigValue("WebSocketIP");
            WebSocketPort = ConfigHelper.GetConfigValue("WebSocketPort");
        }
    }
}
