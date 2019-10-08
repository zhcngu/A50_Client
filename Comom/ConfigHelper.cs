using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_UA_Client_A50.Comom
{
    public static class ConfigHelper
    {
        public static string GetConfigValue(string  key)
        {
               return ConfigurationManager.AppSettings[key].ToString().Trim();
        }
    }
}
