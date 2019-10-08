using OPC_UA_Client_A50.Comom;
using OPC_UA_Client_A50.Model;
using OPC_UA_Client_A50.OpcBingEvent;
using OPC_UA_Client_A50.OpcClienHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace OPC_UA_Client_A50
{
    partial class amsService : ServiceBase
    {
        public amsService()
        {
            InitializeComponent();
        }
        BingEvent eve = new BingEvent();
        List<StationModel> stationModelsList;
        BaseProtocol protocol;
        StationModel C_OP010;
        StationModel C_OP020_1;
        StationModel stnc_op020_2;
        StationModel stnc_op020_3;
        StationModel stnc_op030_2;
        StationModel stnc_op030_3;
        StationModel C_OP040_1;

        StationModel OP060;
        StationModel OP070A;
        StationModel OP080;
        StationModel OP090;
        StationModel OP110;
        StationModel OP130A;
        StationModel OP130B;
        StationModel OP200A;
        StationModel OP200B;
        StationModel OP210;
        StationModel OP220;
        StationModel OP240;

        StationModel OP010;
        StationModel OP020;
        StationModel OP040;
        StationModel OP050;

        StationModel OP100_2;
        protected override void OnStart(string[] args)
        {
            APPConfiguration config = new APPConfiguration();
            config.GetAppConfig();
            ProtocolModel p = new ProtocolModel();
            XmlSerializer xs = new XmlSerializer(typeof(ProtocolModel));
            using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"\cfg\Protocol.xml"))
            {
                p = xs.Deserialize(reader) as ProtocolModel;
            }
            stationModelsList = p.StationModelList;
            protocol = p.STN_BaseProtocol;
            C_OP010 = p.StationModelList.Where((item) => { return item.StationCode == "C-OP010"; }).First();
            if (Convert.ToBoolean(C_OP010.STN_Status))
            {
                eve.ConnectServer(C_OP010, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP010, config);
                C_OP010.StationOPCServer.DataChangedEvent += C_OP010_StationOPCServer_DataChangedEvent;

            }
            C_OP020_1 = p.StationModelList.Where(item => { return item.StationCode == "C-OP020-1"; }).First();
            if (Convert.ToBoolean(C_OP020_1.STN_Status))
            {
                eve.ConnectServer(C_OP020_1, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP020_1, config);
                C_OP020_1.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP020_1_StationOPCServer_DataChangedEvent);
            }
            stnc_op020_2 = p.StationModelList.Where(item => { return item.StationCode == "C-OP020-2"; }).First();
            if (Convert.ToBoolean(stnc_op020_2.STN_Status))
            {
                eve.ConnectServer(stnc_op020_2, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(stnc_op020_2, config);
                stnc_op020_2.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(STN_COP020_2_StationOPCServer_DataChangedEvent);
            }
            stnc_op020_3 = p.StationModelList.Where(item => { return item.StationCode == "C-OP020-3"; }).First();
            if (Convert.ToBoolean(stnc_op020_3.STN_Status))
            {
                eve.ConnectServer(stnc_op020_3, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(stnc_op020_3, config);
                stnc_op020_3.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(STN_COP020_3_StationOPCServer_DataChangedEvent);
            }
            stnc_op030_2 = p.StationModelList.Where(item => { return item.StationCode == "C-OP030-2"; }).First();
            if (Convert.ToBoolean(stnc_op030_2.STN_Status))
            {
                eve.ConnectServer(stnc_op030_2, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(stnc_op030_2, config);
                stnc_op030_2.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(STN_COP030_2_StationOPCServer_DataChangedEvent);
            }
            stnc_op030_3 = p.StationModelList.Where(item => { return item.StationCode == "C-OP030-3"; }).First();
            if (Convert.ToBoolean(stnc_op030_3.STN_Status))
            {
                eve.ConnectServer(stnc_op030_3, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(stnc_op030_3, config);
                stnc_op030_3.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(STN_COP030_3_StationOPCServer_DataChangedEvent);
            }
            OP060 = p.StationModelList.Where(item => { return item.StationCode == "OP060"; }).First();
            if (Convert.ToBoolean(OP060.STN_Status))
            {
                eve.ConnectServer(OP060, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP060, config);
                OP060.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP060_StationOPCServer_DataChangedEvent);
            }
            OP080 = p.StationModelList.Where(item => { return item.StationCode == "OP080"; }).First();
            if (Convert.ToBoolean(OP080.STN_Status))
            {
                eve.ConnectServer(OP080, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP080, config);
                OP080.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP080_StationOPCServer_DataChangedEvent);
            }
            OP110 = p.StationModelList.Where(item => { return item.StationCode == "OP110"; }).First();
            if (Convert.ToBoolean(OP110.STN_Status))
            {
                eve.ConnectServer(OP110, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP110, config);
                OP110.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP110_StationOPCServer_DataChangedEvent);
            }
            OP090 = p.StationModelList.Where(item => { return item.StationCode == "OP090"; }).First();
            if (Convert.ToBoolean(OP090.STN_Status))
            {
                eve.ConnectServer(OP090, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP090, config);
                OP090.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP090_StationOPCServer_DataChangedEvent);
            }
            OP010 = p.StationModelList.Where(item => { return item.StationCode == "OP010A"; }).First();
            if (Convert.ToBoolean(OP010.STN_Status))
            {
                eve.ConnectServer(OP010, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP010, config);
                OP010.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP010_StationOPCServer_DataChangedEvent);
            }
            OP020 = p.StationModelList.Where(item => { return item.StationCode == "OP020A"; }).First();
            if (Convert.ToBoolean(OP020.STN_Status))
            {
                eve.ConnectServer(OP020, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP020, config);
                OP020.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP020_StationOPCServer_DataChangedEvent);
            }
            OP040 = p.StationModelList.Where(item => { return item.StationCode == "OP040A"; }).First();
            if (Convert.ToBoolean(OP040.STN_Status))
            {
                eve.ConnectServer(OP040, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP040, config);
                OP040.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP040_StationOPCServer_DataChangedEvent);
            }
            OP050 = p.StationModelList.Where(item => { return item.StationCode == "OP050A"; }).First();
            if (Convert.ToBoolean(OP050.STN_Status))
            {
                eve.ConnectServer(OP050, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP050, config);
                OP050.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP050_StationOPCServer_DataChangedEvent);
            }
            OP100_2 = p.StationModelList.Where(item => { return item.StationCode == "OP100A"; }).First();
            if (Convert.ToBoolean(OP100_2.STN_Status))
            {
                eve.ConnectServer(OP100_2, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP100_2, config);
                OP100_2.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP100_2_StationOPCServer_DataChangedEvent);
            }
            OP070A = p.StationModelList.Where(item => { return item.StationCode == "OP070A"; }).First();
            if (Convert.ToBoolean(OP070A.STN_Status))
            {
                eve.ConnectServer(OP070A, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP070A, config);
                OP070A.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP070A_StationOPCServer_DataChangedEvent);
            }
            OP130B = p.StationModelList.Where(item => { return item.StationCode == "OP130B"; }).First();
            if (Convert.ToBoolean(OP130B.STN_Status))
            {
                eve.ConnectServer(OP130B, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP130B, config);
                OP130B.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP130B_StationOPCServer_DataChangedEvent);
            }
            OP130A = p.StationModelList.Where(item => { return item.StationCode == "OP130A"; }).First();
            if (Convert.ToBoolean(OP130B.STN_Status))
            {
                eve.ConnectServer(OP130A, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP130A, config);
                OP130A.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP130A_StationOPCServer_DataChangedEvent);
            }
            OP210 = p.StationModelList.Where(item => { return item.StationCode == "OP210A"; }).First();
            if (Convert.ToBoolean(OP210.STN_Status))
            {
                eve.ConnectServer(OP210, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP210, config);
                OP210.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP210_StationOPCServer_DataChangedEvent);
            }
            OP200A = p.StationModelList.Where(item => { return item.StationCode == "OP200A"; }).First();
            if (Convert.ToBoolean(OP200A.STN_Status))
            {
                eve.ConnectServer(OP200A, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP200A, config);
                OP200A.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP200A_StationOPCServer_DataChangedEvent);
            }
            OP200B = p.StationModelList.Where(item => { return item.StationCode == "OP200B"; }).First();
            if (Convert.ToBoolean(OP200B.STN_Status))
            {
                eve.ConnectServer(OP200B, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP200B, config);
                OP200B.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP200B_StationOPCServer_DataChangedEvent);
            }
            C_OP040_1 = p.StationModelList.Where(item => { return item.StationCode == "C-OP040-1"; }).First();
            if (Convert.ToBoolean(C_OP040_1.STN_Status))
            {
                eve.ConnectServer(C_OP040_1, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP040_1, config);
                C_OP040_1.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP040_1_StationOPCServer_DataChangedEvent);
            }
            OP240 = p.StationModelList.Where(item => { return item.StationCode == "OP240A"; }).First();
            if (Convert.ToBoolean(OP240.STN_Status))
            {
                eve.ConnectServer(OP240, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP240, config);
                OP240.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP240_StationOPCServer_DataChangedEvent);
            }
            OP220 = p.StationModelList.Where(item => { return item.StationCode == "OP220A"; }).First();
            if (Convert.ToBoolean(OP220.STN_Status))
            {
                eve.ConnectServer(OP220, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP220, config);
                OP220.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP220_StationOPCServer_DataChangedEvent);
            }
            System.Timers.Timer sendHeartDx = new System.Timers.Timer(1500);
            sendHeartDx.AutoReset = true;
            sendHeartDx.Start();
            sendHeartDx.Elapsed += new System.Timers.ElapsedEventHandler(SendHeartDxFun);

            System.Timers.Timer sendHeartZy = new System.Timers.Timer(1500);
            sendHeartZy.AutoReset = true;
            sendHeartZy.Start();
            sendHeartZy.Elapsed += new System.Timers.ElapsedEventHandler(SendHeartZyFun);
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP060, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP060发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP070A, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP070A发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP080, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP080发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP090, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP090发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP100_2, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP100-2发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP110, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OPOP110发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP130A, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP130A发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP130B, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP130B发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP200A, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP200A发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP200B, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP200B发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP210, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP210发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP220, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP220发送心跳失败", "SendHeart");
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                while (r)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP240, protocol);
                    Thread.Sleep(1500);
                }
                LogHelper.Write("OP240发送心跳失败", "SendHeart");
            });
            LogHelper.Write("启动服务");

        }
        bool heartbeatDx = true;
        private void SendHeartDxFun(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Timers.Timer t2 = (System.Timers.Timer)sender;
            t2.Stop();
            try
            {

                heartbeatDx = !heartbeatDx;
                OpcHelper.SendHeartBit(heartbeatDx, C_OP010, protocol);
                OpcHelper.SendHeartBit(heartbeatDx, C_OP020_1, protocol);
                OpcHelper.SendHeartBit(heartbeatDx, stnc_op020_2, protocol);
                OpcHelper.SendHeartBit(heartbeatDx, stnc_op020_3, protocol);
                OpcHelper.SendHeartBit(heartbeatDx, stnc_op030_2, protocol);
                OpcHelper.SendHeartBit(heartbeatDx, stnc_op030_3, protocol);
                if (OpcHelper.SendHeartBit(heartbeatDx, C_OP040_1, protocol))
                {
                    t2.Start();
                }
            }
            catch (Exception ex)
            {
                t2.Stop();
                LogHelper.Write(ex.Message);
            }


        }

        bool heartbeatZy = true;
        private void SendHeartZyFun(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Timers.Timer t2 = (System.Timers.Timer)sender;
            t2.Stop();
            try
            {
                heartbeatZy = !heartbeatZy;
                OpcHelper.SendHeartBit(heartbeatZy, OP010, protocol);
                OpcHelper.SendHeartBit(heartbeatZy, OP020, protocol);
                OpcHelper.SendHeartBit(heartbeatZy, OP040, protocol);
                if (OpcHelper.SendHeartBit(heartbeatZy, OP050, protocol))
                {
                    t2.Start();
                }
            }
            catch (Exception ex)
            {
                t2.Stop();
                LogHelper.Write(ex.Message);
            }
        }
        private void C_OP010_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP010, protocol);
                }
            }

        }
        private void C_OP020_1_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP020_1, protocol);
                }
            }

        }
        private void STN_COP020_2_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, stnc_op020_2, protocol);
                }
            }

        }
        private void STN_COP020_3_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, stnc_op020_3, protocol);
                }
            }

        }
        private void STN_COP030_2_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, stnc_op030_2, protocol);
                }
            }

        }
        private void STN_COP030_3_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, stnc_op030_3, protocol);
                }
            }
        }
        private void OP060_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP060, protocol);
                }
            }

        }
        private void OP080_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP080, protocol);
                }
            }

        }
        private void OP110_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP110, protocol);
                }
            }

        }
        private void OP090_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP090, protocol);
                }
            }

        }
        private void OP010_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP010, protocol);
                }
            }
        }
        private void OP020_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP020, protocol);
                }
            }
        }
        private void OP040_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP040, protocol);
                }
            }
        }
        private void OP050_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP050, protocol);
                }
            }
        }
        private void OP100_2_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP100_2, protocol);
                }
            }
        }
        private void OP070A_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP070A, protocol);
                }
            }
        }
        private void OP130B_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP130B, protocol);
                }
            }
        }
        private void OP130A_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP130A, protocol);
                }
            }
        }
        private void OP210_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP210, protocol);
                }
            }
        }
        private void OP200A_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP200A, protocol);
                }
            }
        }
        private void OP200B_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP200B, protocol);
                }
            }
        }
        private void C_OP040_1_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP040_1, protocol);
                }
            }
        }
        private void OP240_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP240, protocol);
                }
            }
        }
        private void OP220_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP220, protocol);
                }
            }
        }
        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            foreach (StationModel item in stationModelsList)
            {
                if (Convert.ToBoolean(item.STN_Status) && item.StationOPCServer != null)
                {
                    item.StationOPCServer.Disconnect();
                }
            }
            LogHelper.Write("停止服务");
        }
    }
}
