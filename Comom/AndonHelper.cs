using Newtonsoft.Json;
using OPC_UA_Client_A50.BLL;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OPC_UA_Client_A50.Comom
{
    public  class AndonHelper
    {
        private static IRestResponse SendDataToMES(string parsb)
        {
            //if (restClient == null)
            //restClient = new RestClient();
            string apiurl = ConfigHelper.GetConfigValue("AndonAPI");
            //restClient.BaseUrl = new Uri(apiurl);
            var client = new RestClient(apiurl);
            client.Timeout = 3000;
            var request = new RestRequest(Method.POST);
            request.Timeout = 3000;
            request.AddHeader("Content-Type", "application/json");
            //request.AddParameter(parsb, ParameterType.RequestBody);
            request.AddParameter("undefined", parsb, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response;
        }

        public  static   int  SendMaterialPull(string   partno,string operatornum,string  stncode)
        {
            if (String.IsNullOrEmpty(operatornum))
            {
                operatornum = "无操作员";
            }
            string jsonstr = "[{ \"prodLineCode\":\""+ ConfigHelper.GetConfigValue("LineCode") + "\",\"stationCode\":\""+stncode+"\",\"componentPartNo\":\""+ partno + "\",\"userNo\":\""+ operatornum + "\",\"time\":\""+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"\"}]";
            IRestResponse response = SendDataToMES(jsonstr);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                SaveMessagaBLL bll = new SaveMessagaBLL();
                bll.WriteMessage("发送物料拉动请求时,MES服务器未能正确响应:" + response.StatusCode, stncode, 0);
                LogHelper.Write("发送物料拉动请求时,MES服务器未能正确响应:响应代码" + response.StatusCode, "system");
                return 50;
            }
            string content = response.Content;
            APIResult apiResult = new APIResult();
            apiResult = JsonConvert.DeserializeObject<APIResult>(content);
            if (apiResult.Result)
            {
                return 0;
            }
            else
            {
                SaveMessagaBLL bll = new SaveMessagaBLL();
                bll.WriteMessage("发送物料拉动请求时, MES服务器拒绝:" + apiResult.message, stncode, 0);
                LogHelper.Write("发送物料拉动请求时,MES服务器拒绝:响应代码" + apiResult.message, "system");
                return 40;
            }
        }
       
    }


    public class APIResult
    {
        public bool Result { get; set; }
        public int ResultInt { get; set; }
        public string message { get; set; }
        public string TaskID { get; set; }
        public object Data { get; set; }
    }
}
