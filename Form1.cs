using Opc.Ua;
using OPC_UA_Client_A50.Comom;
using OPC_UA_Client_A50.Model;
using OPC_UA_Client_A50.OpcBingEvent;
using OPC_UA_Client_A50.OpcClienHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace OPC_UA_Client_A50
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
        StationModel C_OP035A;

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
        StationModel OP020_2;
        StationModel OP040;
        StationModel OP050;

        StationModel OP100A;

        StationModel C_OP010B;
        StationModel C_OP020B_1;
        StationModel C_OP020B_2;
        StationModel C_OP020B_3;
        StationModel C_OP030B_2;
        StationModel C_OP030B_3;
        StationModel C_OP040B_1;
        StationModel C_OP035B;

        StationModel  OP055;
        StationModel OP135;
        StationModel OP155;

        private void Btnconn_Click(object sender, EventArgs e)
        {
            APPConfiguration config = new APPConfiguration();
            config.GetAppConfig();
            ProtocolModel p = new ProtocolModel();
            XmlSerializer xs = new XmlSerializer(typeof(ProtocolModel));
            using (StreamReader reader = new StreamReader(Application.StartupPath + @"\cfg\Protocol.xml"))
            {
                p = xs.Deserialize(reader) as ProtocolModel;
            }
            stationModelsList = p.StationModelList;
            protocol = p.STN_BaseProtocol;
            //foreach (StationModel item in p.StationModelList)
            //{
            //    if (Convert.ToBoolean(item.STN_Status))
            //    {
            //        BingEvent eve = new BingEvent(config, item, p.STN_BaseProtocol);
            //    }
            //}
            try
            {
                int port = Convert.ToInt32(ConfigHelper.GetConfigValue("MarkPort"));
                socket.ReceiveTimeout = 13000;
                socket.SendTimeout = 3000;
                socket.Connect(ConfigHelper.GetConfigValue("MarkIP"), port);
                eve.SocketClient = socket;
            }
            catch (Exception ex)
            {
                MessageBox.Show("链接打标机出错");
            }
            C_OP010 = p.StationModelList.Where((item) => { return item.StationCode == "C-OP010A_1"; }).First();
            if   ( Convert.ToBoolean( C_OP010.STN_Status))
            {
                eve.ConnectServer(C_OP010, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP010, config);
                C_OP010.StationOPCServer.DataChangedEvent += C_OP010_StationOPCServer_DataChangedEvent;
                
            }
            C_OP020_1 = p.StationModelList.Where(item => { return item.StationCode == "C-OP020A_1-1"; }).First();
            if (Convert.ToBoolean(C_OP020_1.STN_Status))
            {
                eve.ConnectServer(C_OP020_1, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP020_1, config);
                C_OP020_1.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP020_1_StationOPCServer_DataChangedEvent);
            }
            stnc_op020_2 = p.StationModelList.Where(item => { return item.StationCode == "C-OP020A_1-2"; }).First();
            if (Convert.ToBoolean(stnc_op020_2.STN_Status))
            {
                eve.ConnectServer(stnc_op020_2, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(stnc_op020_2, config);
                stnc_op020_2.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(STN_COP020_2_StationOPCServer_DataChangedEvent);
            }
            stnc_op020_3 = p.StationModelList.Where(item => { return item.StationCode == "C-OP020A_1-3"; }).First();
            if (Convert.ToBoolean(stnc_op020_3.STN_Status))
            {
                eve.ConnectServer(stnc_op020_3, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(stnc_op020_3, config);
                stnc_op020_3.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(STN_COP020_3_StationOPCServer_DataChangedEvent);
            }
            stnc_op030_2 = p.StationModelList.Where(item => { return item.StationCode == "C-OP030A_1-2"; }).First();
            if (Convert.ToBoolean(stnc_op030_2.STN_Status))
            {
                eve.ConnectServer(stnc_op030_2, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(stnc_op030_2, config);
                stnc_op030_2.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(STN_COP030_2_StationOPCServer_DataChangedEvent);
            }
            stnc_op030_3 = p.StationModelList.Where(item => { return item.StationCode == "C-OP030A_1-3"; }).First();
            if (Convert.ToBoolean(stnc_op030_3.STN_Status))
            {
                eve.ConnectServer(stnc_op030_3, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(stnc_op030_3, config);
                stnc_op030_3.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(STN_COP030_3_StationOPCServer_DataChangedEvent);
            }
            OP060 = p.StationModelList.Where(item => { return item.StationCode == "OP060A"; }).First();
            if (Convert.ToBoolean(OP060.STN_Status))
            {
                eve.ConnectServer(OP060, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP060, config);
                OP060.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP060_StationOPCServer_DataChangedEvent);
            }
            OP080 = p.StationModelList.Where(item => { return item.StationCode == "OP080A"; }).First();
            if (Convert.ToBoolean(OP080.STN_Status))
            {
                eve.ConnectServer(OP080, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP080, config);
                OP080.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP080_StationOPCServer_DataChangedEvent);
            }
            OP110 = p.StationModelList.Where(item => { return item.StationCode == "OP110A"; }).First();
            if (Convert.ToBoolean(OP110.STN_Status))
            {
                eve.ConnectServer(OP110, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP110, config);
                OP110.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP110_StationOPCServer_DataChangedEvent);
            }
            OP090 = p.StationModelList.Where(item => { return item.StationCode == "OP090A"; }).First();
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
            OP100A = p.StationModelList.Where(item => { return item.StationCode == "OP100A"; }).First();
            if (Convert.ToBoolean(OP100A.STN_Status))
            {
                eve.ConnectServer(OP100A, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP100A, config);
                OP100A.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP100A_StationOPCServer_DataChangedEvent);
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
            C_OP040_1 = p.StationModelList.Where(item => { return item.StationCode == "C-OP040A_1"; }).First();
            if (Convert.ToBoolean(C_OP040_1.STN_Status))
            {
                eve.ConnectServer(C_OP040_1, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP040_1, config);
                C_OP040_1.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP040_1_StationOPCServer_DataChangedEvent);
            }
            C_OP035A= p.StationModelList.Where(item => { return item.StationCode == "C-OP035A_1"; }).First();
            if (Convert.ToBoolean(C_OP035A.STN_Status))
            {
                eve.ConnectServer(C_OP035A, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP035A, config);
                C_OP035A.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP035A_StationOPCServer_DataChangedEvent);
            }
            C_OP035B = p.StationModelList.Where(item => { return item.StationCode == "C-OP035A_2"; }).First();
            if (Convert.ToBoolean(C_OP035B.STN_Status))
            {
                eve.ConnectServer(C_OP035B, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP035B, config);
                C_OP035B.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP035B_StationOPCServer_DataChangedEvent);
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

            OP020_2 = p.StationModelList.Where(item => { return item.StationCode == "OP020A-2"; }).First();
            if (Convert.ToBoolean(OP020_2.STN_Status))
            {
                eve.ConnectServer(OP020_2, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP020_2, config);
                OP020_2.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP020A_2_StationOPCServer_DataChangedEvent);
            }
            C_OP010B = p.StationModelList.Where(item => { return item.StationCode == "C-OP010A_2"; }).First();
            if (Convert.ToBoolean(C_OP010B.STN_Status))
            {
                eve.ConnectServer(C_OP010B, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP010B, config);
                C_OP010B.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP010B_StationOPCServer_DataChangedEvent);
            }
            C_OP020B_1 =  p.StationModelList.Where(item => { return item.StationCode == "C-OP020A_2-1"; }).First();
            if (Convert.ToBoolean(C_OP020B_1.STN_Status))
            {
                eve.ConnectServer(C_OP020B_1, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP020B_1, config);
                C_OP020B_1.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP020B_1_StationOPCServer_DataChangedEvent);
            }
            C_OP020B_2= p.StationModelList.Where(item => { return item.StationCode == "C-OP020A_2-2"; }).First();
            if (Convert.ToBoolean(C_OP020B_2.STN_Status))
            {
                eve.ConnectServer(C_OP020B_2, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP020B_2, config);
                C_OP020B_2.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP020B_2_StationOPCServer_DataChangedEvent);
            }
            C_OP020B_3 = p.StationModelList.Where(item => { return item.StationCode == "C-OP020A_2-3"; }).First();
            if (Convert.ToBoolean(C_OP020B_3.STN_Status))
            {
                eve.ConnectServer(C_OP020B_3, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP020B_3, config);
                C_OP020B_3.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP020B_3_StationOPCServer_DataChangedEvent);
            }

            C_OP030B_2 = p.StationModelList.Where(item => { return item.StationCode == "C-OP030A_2-2"; }).First();
            if (Convert.ToBoolean(C_OP030B_2.STN_Status))
            {
                eve.ConnectServer(C_OP030B_2, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP030B_2, config);
                C_OP030B_2.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP030B_2_StationOPCServer_DataChangedEvent);
            }
            C_OP030B_3 = p.StationModelList.Where(item => { return item.StationCode == "C-OP030A_2-3"; }).First();
            if (Convert.ToBoolean(C_OP030B_3.STN_Status))
            {
                eve.ConnectServer(C_OP030B_3, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP030B_3, config);
                C_OP030B_3.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP030B_3_StationOPCServer_DataChangedEvent);
            }

            C_OP040B_1= p.StationModelList.Where(item => { return item.StationCode == "C-OP040A_2"; }).First();
            if (Convert.ToBoolean(C_OP040B_1.STN_Status))
            {
                eve.ConnectServer(C_OP040B_1, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(C_OP040B_1, config);
                C_OP040B_1.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(C_OP040B_1_StationOPCServer_DataChangedEvent);
            }

            OP055 = p.StationModelList.Where(item => { return item.StationCode == "OP055"; }).First();
            if (Convert.ToBoolean(OP055.STN_Status))
            {
                eve.ConnectServer(OP055, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP055, config);
                OP055.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP055_StationOPCServer_DataChangedEvent);
            }

           OP135 = p.StationModelList.Where(item => { return item.StationCode == "OP135"; }).First();
            if (Convert.ToBoolean(OP135.STN_Status))
            {
                eve.ConnectServer(OP135, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP135, config);
                OP135.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP135_StationOPCServer_DataChangedEvent);
            }

            OP155 = p.StationModelList.Where(item => { return item.StationCode == "OP155"; }).First();
            if (Convert.ToBoolean(OP155.STN_Status))
            {
                eve.ConnectServer(OP155, config, p.STN_BaseProtocol);
                eve.CreateMySubscipition(OP155, config);
                OP155.StationOPCServer.DataChangedEvent += new Siemens.OpcUA.DataChangedEvent(OP155_StationOPCServer_DataChangedEvent);
            }
            btnconn.Enabled = false;
           
            Task.Run(()=> {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    OpcHelper.SendHeartBit(heartbeat, OP010, protocol);
                    OpcHelper.SendHeartBit(heartbeat, OP020, protocol);
                    OpcHelper.SendHeartBit(heartbeat, OP020_2, protocol);
                    OpcHelper.SendHeartBit(heartbeat, OP040, protocol);
                    r= OpcHelper.SendHeartBit(heartbeat, OP050, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                        LogHelper.Write("转运段发送心跳失败", "SendHeart");
                    }
                 
                }
              
            });

            Task.Run(()=> {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    OpcHelper.SendHeartBit(heartbeat, C_OP010, protocol);
                    OpcHelper.SendHeartBit(heartbeat, C_OP020_1, protocol);
                    OpcHelper.SendHeartBit(heartbeat, stnc_op020_2, protocol);
                    OpcHelper.SendHeartBit(heartbeat, stnc_op020_3, protocol);
                    OpcHelper.SendHeartBit(heartbeat, stnc_op030_2, protocol);
                    OpcHelper.SendHeartBit(heartbeat, stnc_op030_3, protocol);
                    r = OpcHelper.SendHeartBit(heartbeat, C_OP035A, protocol);
                    r =OpcHelper.SendHeartBit(heartbeat, C_OP040_1, protocol);
         
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                        LogHelper.Write("电芯一段发送心跳失败", "SendHeart");
                    }
                }
              
            });  //电芯1
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount < 20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, C_OP010B, protocol);
                    r = OpcHelper.SendHeartBit(heartbeat, C_OP020B_1, protocol);
                    r = OpcHelper.SendHeartBit(heartbeat, C_OP020B_2, protocol);
                    r = OpcHelper.SendHeartBit(heartbeat, C_OP020B_3, protocol);
                    r = OpcHelper.SendHeartBit(heartbeat, C_OP030B_2, protocol);
                    r = OpcHelper.SendHeartBit(heartbeat, C_OP030B_3, protocol);
                    r = OpcHelper.SendHeartBit(heartbeat, C_OP035B, protocol);
                    r = OpcHelper.SendHeartBit(heartbeat, C_OP040B_1, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                        LogHelper.Write("电芯二段发送心跳失败", "SendHeart");
                    }
                    
                }
               
            });//电芯2段

            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP060, protocol);
                  
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                        LogHelper.Write("OP060发送心跳失败", "SendHeart");
                    }
                }
              
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r =  OpcHelper.SendHeartBit(heartbeat, OP070A, protocol);
                    
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                        LogHelper.Write("OP070A发送心跳失败", "SendHeart");
                    }
                }
                
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r =OpcHelper.SendHeartBit(heartbeat, OP080, protocol);
                  
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                         LogHelper.Write("OP080发送心跳失败", "SendHeart");
                    }
                }
               
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP090, protocol);
                   
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                        LogHelper.Write("OP090发送心跳失败", "SendHeart"); 
                    }
                }
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP100A, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                         LogHelper.Write("OP100-2发送心跳失败", "SendHeart");
                    }
                }
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP110, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                         LogHelper.Write("OPOP110发送心跳失败", "SendHeart");
                    }
                }
               
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP130A, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                         LogHelper.Write("OP130A发送心跳失败", "SendHeart");
                    }
                }
               
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP130B, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                        LogHelper.Write("OP130B发送心跳失败", "SendHeart");
                       
                    }
                }
              
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP200A, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                       
                        LogHelper.Write("OP200A发送心跳失败", "SendHeart");
                    }
                }
               
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP200B, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                         LogHelper.Write("OP200B发送心跳失败", "SendHeart");
                    }
                }
               
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP210, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                         LogHelper.Write("OP210发送心跳失败", "SendHeart");
                    }
                }
               
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount < 20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP220, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                           LogHelper.Write("OP220发送心跳失败", "SendHeart");
                    }
                }
             
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                       r = OpcHelper.SendHeartBit(heartbeat, OP240, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                        LogHelper.Write("OP240发送心跳失败", "SendHeart"); 
                    }
                }
               
            });
            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP020_2, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                        LogHelper.Write("OP020A-2发送心跳失败", "SendHeart");
                    }
                }
             
            });

            Task.Run(() => {
                bool heartbeat = true;
                bool r = true;
                int resendCount = 0;
                while (resendCount<20)
                {
                    heartbeat = !heartbeat;
                    r = OpcHelper.SendHeartBit(heartbeat, OP055, protocol);
                    r = OpcHelper.SendHeartBit(heartbeat, OP135, protocol);
                    r = OpcHelper.SendHeartBit(heartbeat, OP155, protocol);
                    if (r)
                    {
                        Thread.Sleep(1000);
                        resendCount = 0;
                    }
                    else
                    {
                        Thread.Sleep(10000);
                        resendCount++;
                        LogHelper.Write("CZ4发送心跳失败", "SendHeart");
                    }
                }
               
            });
       

        }

        private void C_OP035A_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP035A, protocol);
                }
            }
        }
        private void C_OP035B_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP035B, protocol);
                }
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
                if (client!= 0 && value)
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
        private void OP100A_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP100A, protocol);
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
        private void OP020A_2_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP020_2, protocol);
                }
            }
        }
        private void C_OP010B_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP010B, protocol);
                }
            }
        }
        private void C_OP020B_1_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP020B_1, protocol);
                }
            }
        }
        private void C_OP020B_2_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP020B_2, protocol);
                }
            }
        }
        private void C_OP020B_3_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP020B_3, protocol);
                }
            }
        }
        private void C_OP030B_2_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP030B_2, protocol);
                }
            }
        }
        private void C_OP030B_3_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP030B_3, protocol);
                }
            }
        }

        private void C_OP040B_1_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, C_OP040B_1, protocol);
                }
            }
        }
        private void OP055_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP055, protocol);
                }
            }
        }
        private void OP135_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP135, protocol);
                }
            }
        }
        private void OP155_StationOPCServer_DataChangedEvent(List<object> clientHandleList, List<Opc.Ua.DataValue> valueList)
        {
            for (int i = 0; i < clientHandleList.Count; i++)
            {
                int client = Convert.ToInt32(clientHandleList[i]);
                bool value = Convert.ToBoolean(valueList[i].Value);
                if (client != 0 && value)
                {
                    eve.ClientDataChanged(clientHandleList, valueList, OP155, protocol);
                }
            }
        }
        private void BtnClose_Click(object sender, EventArgs e)
        {
            foreach (StationModel item in stationModelsList)
            {
                //&& item.StationOPCServer.Session.Connected
                if ( Convert.ToBoolean( item.STN_Status) && item.StationOPCServer !=null)
                {
                   item.StationOPCServer.Disconnect();
                }
               
            }
            this.Close();
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

     
        public static byte ConvertBCDToInt(byte b)
        {
            //高四位  
            byte b1 = (byte)((b >> 4) & 0xF);
            //低四位  
            byte b2 = (byte)(b & 0xF);

            return (byte)(b1 * 10 + b2);
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            BLL.ProdDataBll dd = new BLL.ProdDataBll();
             var r=  dd.GetFormulaData("7020002761", "OP050A");
            var r1 = dd.GetFormulaData("7020002761", "OP020A");
            var r2 = dd.GetFormulaData("7020002761", "C-OP010A");
        }



    }
}
