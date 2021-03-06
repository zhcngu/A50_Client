﻿using Opc.Ua;
using OPC_UA_Client_A50.BLL;
using OPC_UA_Client_A50.Comom;
using OPC_UA_Client_A50.Model;
using OPC_UA_Client_A50.OpcClienHelper;
using Siemens.OpcUA;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OPC_UA_Client_A50.OpcBingEvent
{
    public class BingEvent
    {
        public BingEvent(Socket socket)
        {
            messagaBLL = new SaveMessagaBLL();
            SocketClient = socket;
        }
        public BingEvent()
        {
            messagaBLL = new SaveMessagaBLL();
          
        }
        public BingEvent(APPConfiguration appconfig,StationModel stnmodel,BaseProtocol basepro)
        {
           // MyServer = new Server();
            MyAppConfig = appconfig;
            MyStationModel = stnmodel;
            MyStnBaseProtocol = basepro;
          //  InitOpc();
           // DataChangeBing();
            //opchelper = new OpcHelper(MyStnBaseProtocol, MyStationModel, _serverNamespaceIndex);
            //opchelper.MyServer = MyServer;
            //opcbll = new OpcBll();
            //opcbll.StnModel = stnmodel;
           // opcbll.myopcHelper = opchelper;
    
          
        }

        private ushort _serverNamespaceIndex;

        //private OpcHelper opchelper;
      //  private OpcBll opcbll;

        private Siemens.OpcUA.Subscription my_Subscipition;

        private int[] monitorClientHandl;

        private NodeIdCollection my_MonitorNodes; 
        public  Server MyServer { get; set; }

        public APPConfiguration MyAppConfig { get; set; }
        public StationModel MyStationModel { get; set; }
        public BaseProtocol MyStnBaseProtocol { get; set; }

        private SaveMessagaBLL messagaBLL;
        private NodeIdCollection InitMonitorItem(ushort nameSpaceIndex)
        {
            my_MonitorNodes = new NodeIdCollection();
            string  comDB=  MyStationModel.DataReadDB;//握手信号交互DB块
            my_MonitorNodes.Add(new NodeId(comDB + MyStnBaseProtocol.PLC_MES_Heart, nameSpaceIndex));//心跳
            my_MonitorNodes.Add(new NodeId(comDB + MyStnBaseProtocol.PLC_MES_CellNG, nameSpaceIndex));//电芯NG剔料
            my_MonitorNodes.Add(new NodeId(comDB + MyStnBaseProtocol.PLC_MES_DataRequest, nameSpaceIndex));//上线请求
            my_MonitorNodes.Add(new NodeId(comDB + MyStnBaseProtocol.PLC_MES_DataSave, nameSpaceIndex));//下线请求
            my_MonitorNodes.Add(new NodeId(comDB + MyStnBaseProtocol.PLC_MES_ReadBom, nameSpaceIndex));//读BOM
            my_MonitorNodes.Add(new NodeId(comDB + MyStnBaseProtocol.PLC_MES_RepairRequest, nameSpaceIndex));//返修上线
            my_MonitorNodes.Add(new NodeId(comDB + MyStnBaseProtocol.PLC_MES_RepairSave, nameSpaceIndex));//返修下线
            my_MonitorNodes.Add(new NodeId(comDB + MyStnBaseProtocol.PLC_MES_MaterialAndonRequest, nameSpaceIndex));//PLC安东物料拉动
            my_MonitorNodes.Add(new NodeId(comDB + MyStnBaseProtocol.PLC_MES_OCV_Test, nameSpaceIndex));//OCV测试
            monitorClientHandl = new int[9];
            for (int i = 0; i < 9; i++)
            {
                monitorClientHandl[i] = i;
            }
            return my_MonitorNodes;
        }

        public  Socket SocketClient { get; set; }
        public void DataChangeBing()
        {
            object monitoredItemServerHandle = null;
            int interval = Convert.ToInt32(MyAppConfig.PublishingInterval);
            uint rate = Convert.ToUInt32(MyAppConfig.SamplingRate);
            my_Subscipition = MyServer.AddSubscription(interval);//订阅
            for (int i = 0; i < my_MonitorNodes.Count; i++)
            {
                my_Subscipition.AddDataMonitoredItem(
                    my_MonitorNodes[i],
                     monitorClientHandl[i], // ITEM_READ[i],
                     rate,
                    out monitoredItemServerHandle);
            }
          //  MyServer.DataChangedEvent += new DataChangedEvent(ClientDataChanged);
        }
        private int SendPackidToMark(string packid)
        {
            try
            {
                byte[] buff = Encoding.Default.GetBytes(packid);
                SocketClient.Send(buff);
                byte[] readbuff = new byte[20];
                int r = SocketClient.Receive(readbuff);
                string result = Encoding.Default.GetString(readbuff, 0, r);
                if (result == "ok")
                {
                    return 0;
                }
                else
                {
                    return 22;
                }

            }
            catch (Exception ex)
            {
                LogHelper.Write("发送packID给打标机时异常:" + ex.Message, "system");
                return 22;
            }
        }

        public void ClientDataChanged(List<object> clientHandleList, List<DataValue> valueList,StationModel stnmodel, BaseProtocol protocol)
        {
            OpcHelper opchelper = new OpcHelper(protocol, stnmodel, stnmodel.ServerIndex);
            try
            {
                OpcBll bll = new OpcBll();
                bll.myopcHelper = opchelper;
                bll.StnModel = stnmodel;
                for (int i = 0; i < clientHandleList.Count; i++)
                {
                    int clientHandle = Convert.ToInt32(clientHandleList[i]);
                    DataValue datavalue = valueList[i];
                    bool value = Convert.ToBoolean(datavalue.Value);
                    switch (clientHandle)
                    {
                        case 0://心跳
                           // opchelper.SendHeartBit(!value);
                            break;
                        case 1://电芯NG剔料,记录过点，然后电芯补一个订单
                            if (value)
                            {
                                messagaBLL.WriteMessage("收到电芯NG剔料请求", stnmodel.StationCode,0);
                                byte[] array = opchelper.ReadPlcData();
                                AnalDataHelper a = new AnalDataHelper(stnmodel);
                                int res = a.DianXinNg(array);
                                if (res == 0)
                                {
                                    messagaBLL.WriteMessage("发送电芯NG剔料完成", stnmodel.StationCode,res);
                                    opchelper.SendDianXinNgOk();
                                }
                                else
                                {
                                    messagaBLL.WriteMessage("发送电芯NG剔料失败，发送错误代码" + res, stnmodel.StationCode,res);
                                    opchelper.SendErrorsCode((byte)res);
                                }
                            }
                            break;
                        case 2://上线请求
                            if (value)
                            {
                                messagaBLL.WriteMessage("收到上线请求", stnmodel.StationCode,0);
                                bll.InitData();
                            }
                            break;
                        case 3://下线请求
                            if (value)
                            {
                                messagaBLL.WriteMessage("收到下线请求", stnmodel.StationCode,0);
                                byte[] array = opchelper.ReadPlcData();
                                AnalDataHelper a = new AnalDataHelper(stnmodel);
                                int res = a.AnalData(array);
                                if (res == 0)
                                {
                                    if (stnmodel.StationCode == "OP240A")
                                    {
                                        byte[] tagValues;
                                        tagValues = opchelper.ReadRepairPlcData();
                                        int temp = a.SaveOP240TagValue(tagValues);
                                        if (temp != 0)
                                        {
                                            opchelper.SendErrorsCode(50);
                                            messagaBLL.WriteMessage("保存TAG数据失败发送错误代码:" + 50, stnmodel.StationCode, 50);
                                            return;
                                        }
                                    }
                                    opchelper.SendReaddowngDownCmd();
                                    opchelper.SendErrorsCode(0);
                                    messagaBLL.WriteMessage("发送下线完成", stnmodel.StationCode,0);
                                }
                                else
                                {
                                    opchelper.SendErrorsCode((byte)res);
                                    messagaBLL.WriteMessage("发送错误代码"+ res, stnmodel.StationCode,res);
                                }
                            }
                            break;
                        case 4://读bom
                            if (value)
                            {
                                messagaBLL.WriteMessage("收到BOM比对请求" , stnmodel.StationCode,0);
                                bll.CheckBom();
                            }
                            break;
                        case 5://返修上线
                            if (value)
                            {
                                messagaBLL.WriteMessage("收到返修上线请求", stnmodel.StationCode,0);
                                int  res= bll.InitRepairData();
                                if (res==0)
                                {
                                    opchelper.SendRepairWriteDown();
                                    opchelper.SendErrorsCode(0);
                                    messagaBLL.WriteMessage("发送返修上线完成", stnmodel.StationCode,0);
                                }
                                else
                                {
                                    opchelper.SendErrorsCode((byte)res);
                                    messagaBLL.WriteMessage("返修上线失败,发送错误代码"+ res, stnmodel.StationCode,res);
                                }
                            }
                            break;
                        case 6://返修下线
                            if (value)
                            {
                                messagaBLL.WriteMessage("收到返修下线请求", stnmodel.StationCode,0);
                                byte[] array = opchelper.ReadRepairPlcData();
                                AnalDataHelper a = new AnalDataHelper(stnmodel);
                                int  r=a.SaveTagValue(array);
                                if (r == 0)
                                {
                                    opchelper.SendRepairReadDown();
                                    opchelper.SendErrorsCode(0);
                                    messagaBLL.WriteMessage("发送返修下线完成", stnmodel.StationCode,0);
                                }
                                else
                                {
                                    opchelper.SendErrorsCode((byte)r);
                                    messagaBLL.WriteMessage("返修下线失败,发送错误代码" + r, stnmodel.StationCode,0);
                                }
                            }
                            break;
                        case 7://andon物料拉动
                            if (value)
                            {
                                messagaBLL.WriteMessage("收到物料拉动请求" , stnmodel.StationCode, 0);
                                string materialNum = opchelper.ReadMaterialNum();
                                string operatornum = opchelper.ReadOperatorNum();
                                int  res=AndonHelper.SendMaterialPull(materialNum, operatornum, stnmodel.StationCode);
                                if (res==0)
                                {
                                    opchelper.SendMaterialPullOk();
                                    opchelper.SendErrorsCode(0);
                                    messagaBLL.WriteMessage("发送物料拉动完成", stnmodel.StationCode, 0);
                                }
                                else
                                {
                                    opchelper.SendMaterialPullNG();
                                    //opchelper.SendErrorsCode((byte)res);//PLC那边不要这个错误代码
                                    messagaBLL.WriteMessage("发送物料拉动失败,发送错误代码:"+res, stnmodel.StationCode, res);
                                }
                            }
                            break;
                        case 8://OCV 检测
                            if (value)
                            {
                                messagaBLL.WriteMessage("收到OCV测试请求", stnmodel.StationCode);
                                byte[] array = opchelper.ReadPlcData();
                                bll.CheckOCVTest(array);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "BingEvent");
                opchelper.SendErrorsCode(100);
            }
        }
        public void InitOpc()
        {
            try
            {
                string url = MyAppConfig.OPCServerUrl;
                MyServer = new Server();
                MyServer.CertificateEvent += new certificateValidation(my_Server_CertificateEvent);
                MyServer.Connect(url);
                NodeIdCollection nodesNamespace = new NodeIdCollection();
                DataValueCollection results;
                nodesNamespace.Add(Variables.Server_NamespaceArray);
                ushort serverNamespaceIndex = 0;
                MyServer.ReadValues(nodesNamespace, out results);
                string[]  resultArray=  results[0].Value as string[];
                 for (int i = 0; i < resultArray.Length; i++)
                {
                    if (resultArray[i].ToString().Equals("S7:"))
                    {
                        serverNamespaceIndex = (ushort)i;
                        break;
                    }
                }
                 _serverNamespaceIndex = serverNamespaceIndex;
                InitMonitorItem(serverNamespaceIndex);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "BingEvent");
            }
        }

        private void my_Server_CertificateEvent(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            // Accept all certificate -> better ask user
            e.Accept = true;
        }
      
  
       

         public void ConnectServer(StationModel  stationmodel, APPConfiguration appconfig, BaseProtocol baseProtocol)
        {
            if (Convert.ToBoolean( stationmodel.STN_Status))
            {
                stationmodel.StationOPCServer = new Server();
                stationmodel.StationOPCServer.CertificateEvent += new certificateValidation(my_Server_CertificateEvent);
                stationmodel.StationOPCServer.Connect(appconfig.OPCServerUrl);

                NodeIdCollection nodesNamespace = new NodeIdCollection();
                DataValueCollection results;
                nodesNamespace.Add(Variables.Server_NamespaceArray);
                ushort serverNamespaceIndex = 0;
                stationmodel.StationOPCServer.ReadValues(nodesNamespace, out results);
                string[] resultArray = results[0].Value as string[];
                for (int i = 0; i < resultArray.Length; i++)
                {
                    if (resultArray[i].ToString().Equals("S7:"))
                    {
                        serverNamespaceIndex = (ushort)i;
                        break;
                    }
                }
                //_serverNamespaceIndex = serverNamespaceIndex;
                CreateMonitorItem(serverNamespaceIndex, stationmodel, baseProtocol);
                stationmodel.ServerIndex = serverNamespaceIndex;
                //opchelper = new OpcHelper(baseProtocol, stationmodel, serverNamespaceIndex);
                //opchelper.MyServer = stationmodel.StationOPCServer;
                
               // opcbll.myopcHelper = opchelper;
            }
        }

        public void CreateMonitorItem(ushort nameSpaceIndex,StationModel stationModel,  BaseProtocol baseProtocol)
        {
            stationModel.MonitorNodes = new NodeIdCollection();
          //  my_MonitorNodes = new NodeIdCollection();
            string comDB = stationModel.DataReadDB;//握手信号交互DB块
           // stationModel.MonitorNodes.Add(new NodeId(comDB + baseProtocol.PLC_MES_Heart, nameSpaceIndex));//心跳
            stationModel.MonitorNodes.Add(new NodeId(comDB + baseProtocol.PLC_MES_CellNG, nameSpaceIndex));//电芯NG剔料
            stationModel.MonitorNodes.Add(new NodeId(comDB + baseProtocol.PLC_MES_DataRequest, nameSpaceIndex));//上线请求 
            stationModel.MonitorNodes.Add(new NodeId(comDB + baseProtocol.PLC_MES_DataSave, nameSpaceIndex));//下线请求
            stationModel.MonitorNodes.Add(new NodeId(comDB + baseProtocol.PLC_MES_ReadBom, nameSpaceIndex));//读BOM
            stationModel.MonitorNodes.Add(new NodeId(comDB + baseProtocol.PLC_MES_RepairRequest, nameSpaceIndex));//返修上线
            stationModel.MonitorNodes.Add(new NodeId(comDB + baseProtocol.PLC_MES_RepairSave, nameSpaceIndex));//返修下线
            stationModel.MonitorNodes.Add(new NodeId(comDB + baseProtocol.PLC_MES_MaterialAndonRequest, nameSpaceIndex));//andon物料拉动
            stationModel.MonitorNodes.Add(new NodeId(comDB + baseProtocol.PLC_MES_OCV_Test, nameSpaceIndex));//OCV测试
            stationModel.MonitorClientHandl= new int[8];
            for (int i = 0; i < 8; i++)
            {
                stationModel.MonitorClientHandl[i] = i+1;
            }
           // return my_MonitorNodes;
        }

        public  void CreateMySubscipition(StationModel  station, APPConfiguration appconfig)
        {
            object monitoredItemServerHandle = null;
            int interval = Convert.ToInt32(appconfig.PublishingInterval);
            uint rate = Convert.ToUInt32(appconfig.SamplingRate);
            Subscription mysubscription = station.StationOPCServer.AddSubscription(interval);
            for (int i = 0; i < station.MonitorNodes.Count; i++)
            {
                mysubscription.AddDataMonitoredItem(
                    station.MonitorNodes[i],
                     station.MonitorClientHandl[i], 
                     rate,
                    out monitoredItemServerHandle);
            }
        }
    }
}
