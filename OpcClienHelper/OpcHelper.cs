using Opc.Ua;
using OPC_UA_Client_A50.Comom;
using OPC_UA_Client_A50.Model;
using Siemens.OpcUA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_UA_Client_A50.OpcClienHelper
{
   public   class OpcHelper
    {
        public OpcHelper(BaseProtocol protocol, StationModel stnmodel, ushort serverindex)
        {
            MyBaseProtocol = protocol;
            MyStationModel = stnmodel;
            ServerIndex = serverindex;
            MyServer = stnmodel.StationOPCServer;
        }

       public  Server MyServer { get; set; }
   
       public   ushort   ServerIndex { get; set; }
       public  BaseProtocol  MyBaseProtocol { get; set; }
       public  StationModel MyStationModel { get; set; }

       
      


       public  byte[]  ReadPlcData()
       {
           string readDb = MyStationModel.DataReadDB;
           string address = MyStationModel.DataAddress;
           string dblen = MyStationModel.DBLength;
           string len = MyStationModel.DataLength;
           NodeIdCollection readNode = new NodeIdCollection();
           readNode.Add(new NodeId(readDb + ".0" + ",b," + dblen, ServerIndex));//读整个DB块
           DataValueCollection  valuecoll=new DataValueCollection();
           MyServer.ReadValues(readNode, out valuecoll);
           byte[] bArray = (byte[])valuecoll[0].Value;
           return bArray;
       }

        /// <summary>
        /// 在240 工位  不管是返修还是正常下线 PLC那边都会把整个tag读出来，发给MES
        /// </summary>
        /// <returns></returns>
        public byte[] ReadRepairPlcData()
        {
            string readDb = MyStationModel.RepairDataReadDB;
            string dblen = "8876";//tag数据字节数
            NodeIdCollection readNode = new NodeIdCollection();
            readNode.Add(new NodeId(readDb + ".0" + ",b," + dblen, ServerIndex));//读整个DB块
            DataValueCollection valuecoll = new DataValueCollection();
            MyServer.ReadValues(readNode, out valuecoll);
            byte[] bArray = (byte[])valuecoll[0].Value;
            return bArray;
        }
        public string ReadMaterialNum()
        {
            string readDb = MyStationModel.DataReadDB;
            NodeIdCollection readNode = new NodeIdCollection();
           
            readNode.Add(new NodeId(readDb + ".10" + ",b," + 10, ServerIndex));//读取安东物料号
            DataValueCollection valuecoll = new DataValueCollection();
            MyServer.ReadValues(readNode, out valuecoll);
            byte[] bArray = (byte[])valuecoll[0].Value;
            return ConvertHelper.ByteToString(bArray, 0, 10);
        }

        public string ReadOperatorNum()
        {
            string readDb = MyStationModel.DataReadDB;
            NodeIdCollection readNode = new NodeIdCollection();

            readNode.Add(new NodeId(readDb + ".362" + ",b," + 10, ServerIndex));//读取安东物料号
            DataValueCollection valuecoll = new DataValueCollection();
            MyServer.ReadValues(readNode, out valuecoll);
            byte[] bArray = (byte[])valuecoll[0].Value;
            return ConvertHelper.ByteToString(bArray, 0, 10);
        }
        public  string ReadQRCodeData()
        {
            string readDb = MyStationModel.DataReadDB;
            NodeIdCollection readNode = new NodeIdCollection();
            readNode.Add(new NodeId(readDb + ".380" + ",b," + 64, ServerIndex));//读取二维码
            DataValueCollection valuecoll = new DataValueCollection();
            MyServer.ReadValues(readNode, out valuecoll);
            byte[] bArray = (byte[])valuecoll[0].Value;
            return  ConvertHelper.ByteToString(bArray, 0, 64);
        }
        public string ReadPackID()
        {
            string readDb = MyStationModel.DataReadDB;
            NodeIdCollection readNode = new NodeIdCollection();
            readNode.Add(new NodeId(readDb + ".120" + ",b," + 70, ServerIndex));//读取packid
            DataValueCollection valuecoll = new DataValueCollection();
            MyServer.ReadValues(readNode, out valuecoll);
            byte[] bArray = (byte[])valuecoll[0].Value;
            return ConvertHelper.ByteToString(bArray, 0, 70);
        }
        public string ReadOperatorID()
        {
            string readDb = MyStationModel.DataReadDB;
            NodeIdCollection readNode = new NodeIdCollection();
            readNode.Add(new NodeId(readDb + ".362" + ",b," + 10, ServerIndex));//读取packid
            DataValueCollection valuecoll = new DataValueCollection();
            MyServer.ReadValues(readNode, out valuecoll);
            byte[] bArray = (byte[])valuecoll[0].Value;
            return ConvertHelper.ByteToString(bArray, 0, 10);
        }

       /// <summary>
       /// 给PLC写数据
       /// </summary>
       /// <param name="ordrNum">订单号</param>
       /// <param name="sncode">packid</param>
       /// <param name="ptcode">成品物料号</param>
       /// <param name="mozuCode"></param>
       /// <param name="mozusnnum"></param>
       /// <param name="keyValues">配方(key:Item  value:配方值)</param>
       /// <returns></returns>
        public  bool InitRequestData( string ordrNum, string sncode,string ptcode,string mozuCode, string mozusnnum ,int canwork,int notwork,int ngmodel,int normalwork,int  workfinish,int agluefinish,int bgluefinish,int planqty,Dictionary<string,byte[]> keyValues)
       {
           try
           {
                string ordnum = ".60,b,40";//工单号地址
                string prdcode = ".100,b,20";//pack总成物料号
                string snnum = ".120,b,70";//产品sn号
                string mzcode = ".190,b,20";// 模组物料号
                string mzsn = ".210,b,70";//模组序列号--已经不写了
                string canWorkItem = ".1,x4";//可加工
                string notWorkItem = ".1,x5";//不可加工
                string ngModelItem = ".2,x6";//NG排出
                string normalWorkItem = ".2,x7";//正常加工

                string workFinishItem = ".1,x7";//当前工位加工完成
                string aGlueFinishItem = ".3,x0";//A扫码完成
                string bGlueFinishItem = ".3,x1";//B扫码完成
                string formulaItem = keyValues.Keys.ToList()[0];//配方的地址

                string orderQtyItem = ".280,i";//订单数量

                string dbnum = MyStationModel.DataWriteDB;//写数据的DB块
                NodeIdCollection writeNodecoll = new NodeIdCollection();
                writeNodecoll.Add(new NodeId(dbnum + ordnum, ServerIndex));//工单号
                writeNodecoll.Add(new NodeId(dbnum + prdcode, ServerIndex));//物料号
                writeNodecoll.Add(new NodeId(dbnum + snnum, ServerIndex));//packid
                writeNodecoll.Add(new NodeId(dbnum + mzcode, ServerIndex));//模组物料号
                writeNodecoll.Add(new NodeId(dbnum + mzsn, ServerIndex));//模组物料号

                writeNodecoll.Add(new NodeId(dbnum + canWorkItem, ServerIndex));//可加工
                writeNodecoll.Add(new NodeId(dbnum + notWorkItem, ServerIndex));//不可加工
                writeNodecoll.Add(new NodeId(dbnum + ngModelItem, ServerIndex));//ng排料
                writeNodecoll.Add(new NodeId(dbnum + normalWorkItem, ServerIndex));//正常加工

                writeNodecoll.Add(new NodeId(dbnum + workFinishItem, ServerIndex));//本工位加工完成
                writeNodecoll.Add(new NodeId(dbnum + aGlueFinishItem, ServerIndex));//A扫码完成
                writeNodecoll.Add(new NodeId(dbnum + bGlueFinishItem, ServerIndex));//B扫码完成

                writeNodecoll.Add(new NodeId(dbnum + formulaItem, ServerIndex));//配方

                writeNodecoll.Add(new NodeId(dbnum + orderQtyItem, ServerIndex));//订单数量

                DataValueCollection values = new DataValueCollection();
                byte[] ordnumbuff = ConvertHelper.StringToByteArray(ordrNum, 40);
                byte[] ptcocebuff = ConvertHelper.StringToByteArray(ptcode, 20);
                byte[] sncodebuff = ConvertHelper.StringToByteArray(sncode, 70);
                byte[] mzcodebuff = ConvertHelper.StringToByteArray(mozuCode, 20);
                byte[] mzsnnumbuff = ConvertHelper.StringToByteArray(mozusnnum, 70);

                bool canworkValue = Convert.ToBoolean(canwork);
                bool notworkValue = Convert.ToBoolean(notwork);
                bool ngmodelValue = Convert.ToBoolean(ngmodel);
                bool normalVlue = Convert.ToBoolean(normalwork);

                bool workFinishValue = Convert.ToBoolean(workfinish);
                bool aGlueFinishValue = Convert.ToBoolean(agluefinish);
                bool bGlueFinishValue = Convert.ToBoolean(bgluefinish);

                byte[] formulaValue = keyValues[formulaItem];//配方值

                short qty = (short)planqty;

                values.Add(new DataValue(ordnumbuff));
                values.Add(new DataValue(ptcocebuff));
                values.Add(new DataValue(sncodebuff));
                values.Add(new DataValue(mzcodebuff));
                values.Add(new DataValue(mzsnnumbuff));

                values.Add(new DataValue(canworkValue));
                values.Add(new DataValue(notworkValue));
                values.Add(new DataValue(ngmodelValue));
                values.Add(new DataValue(normalVlue));


                values.Add(new DataValue(workFinishValue));
                values.Add(new DataValue(aGlueFinishValue));
                values.Add(new DataValue(bGlueFinishValue));

                values.Add(new DataValue(formulaValue));
                values.Add(new DataValue(qty));//数量

                StatusCodeCollection resultCodes;
                MyServer.WriteValues(writeNodecoll, values, out resultCodes);
                foreach (StatusCode item in resultCodes)
                {
                    if (StatusCode.IsBad(item.Code))
                    {
                        return false;
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
                return false;
            }

        } 


        public  bool InitRepairPLCData(byte[] data)
        {
            try
            {
                string itemName = ".0,b,"+data.Length;//tag数据
                string dbnum = MyStationModel.RepairDataWriteDB;//写数据的DB块
                NodeIdCollection writeNodecoll = new NodeIdCollection();
                writeNodecoll.Add(new NodeId(dbnum + itemName, ServerIndex));
                DataValueCollection values = new DataValueCollection();
                values.Add(new DataValue(data));
                StatusCodeCollection resultCodes;
                MyServer.WriteValues(writeNodecoll, values, out resultCodes);
                foreach (StatusCode item in resultCodes)
                {
                    if (StatusCode.IsBad(item.Code))
                    {
                        return false;
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Write("向PLC写入返修数据时出错" + ex.Message + "\r\n" + ex.StackTrace, "system");
                return false;
            }
        }


       /// <summary>
       /// 发送心跳
       /// </summary>
       /// <param name="heartbit"></param>
       public  static  bool SendHeartBit(bool heartbit,StationModel station,BaseProtocol protocol)
       {
           try
           {
                if (station.StationOPCServer==null || !station.StationOPCServer.Session.Connected)
                {
                    return false;
                }
                NodeIdCollection nodesToWriteHeart = new NodeIdCollection();
               string comDB = station.DataWriteDB;//握手信号交互DB块
               nodesToWriteHeart.Add(new NodeId(comDB + protocol.MES_PLC_Heart, station.ServerIndex));//心跳
               DataValueCollection values = new DataValueCollection();
               values.Add(new DataValue(heartbit));
               StatusCodeCollection codes;
                station.StationOPCServer.WriteValues(nodesToWriteHeart, values, out  codes);
                foreach (StatusCode item in codes)
                {
                    if (StatusCode.IsBad(item.Code))
                    {
                        return false;
                    }
                }
                return true;
            }
           catch (Exception ex)
           {
               LogHelper.Write("发送心跳异常:"+ex.Message, "system");
                return false;
           }
       }

       /// <summary>
       /// 发送上线中信号
       /// </summary>
       public   void SendWritingCmd()
       {
           try
           {
               NodeIdCollection writingNode = new NodeIdCollection();
              // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                writingNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_Writing, ServerIndex));//上线中
               DataValueCollection values = new DataValueCollection();
              // byte[] buff = { 1 };
               values.Add(new DataValue(true));
               StatusCodeCollection codes;
               MyServer.WriteValues(writingNode, values, out  codes);
                foreach (StatusCode item in codes)
                {
                    if (StatusCode.IsBad(item.Code))
                    {
                        Console.WriteLine("Error"+item.Code.ToString());
                    }

                }
            }
           catch (Exception ex)
           {
               LogHelper.Write(ex, "system");
           }
       }
       /// <summary>
       /// 发送上线完成信号
       /// </summary>
       public   bool SendWriteDownCmd()
       {
           try
           {
               NodeIdCollection writeDowdNode = new NodeIdCollection();
                //  string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                writeDowdNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_WriteDowd, ServerIndex));//上线中
               DataValueCollection values = new DataValueCollection();
        
               values.Add(new DataValue(true));
               StatusCodeCollection codes;
               MyServer.WriteValues(writeDowdNode, values, out  codes);
                foreach (StatusCode item in codes)
                {
                    if (StatusCode.IsBad(item.Code))
                    {
                        return false;
                    }
                }
                return true;
            }
           catch (Exception ex)
           {
               LogHelper.Write(ex, "system");
                return false;
           }
       }

       /// <summary>
       /// 发送读数据中命令
       /// </summary>
       public   void SendReadingDownCmd()
       {
           try
           {
               NodeIdCollection readingdNode = new NodeIdCollection();
              // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readingdNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_Reading, ServerIndex));//上线中
               DataValueCollection values = new DataValueCollection();
            
               values.Add(new DataValue(true));
               StatusCodeCollection codes;
               MyServer.WriteValues(readingdNode, values, out  codes);
           }
           catch (Exception ex)
           {
               LogHelper.Write(ex, "system");
           }
       }
       /// <summary>
       /// 发送读数据完成
       /// </summary>
       public   void SendReaddowngDownCmd()
       {
           try
           {
               NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_ReadDown, ServerIndex));//度完成
               DataValueCollection values = new DataValueCollection();
           
               values.Add(new DataValue(true));
               StatusCodeCollection codes;
               MyServer.WriteValues(readdownNode, values, out  codes);
           }
           catch (Exception ex)
           {
               LogHelper.Write(ex, "system");
           }
       }

        public   void SendBomOKCmd()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_BomOK, ServerIndex));
                DataValueCollection values = new DataValueCollection();
                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }
        public   void SendBomNGCmd()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_BomNG, ServerIndex));//度完成
                DataValueCollection values = new DataValueCollection();

                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }

        public   void SendOCV_OKCmd()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_OCVOK, ServerIndex));
                DataValueCollection values = new DataValueCollection();

                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }
        public   void SendOCV_NGCmd()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_OCVNG, ServerIndex));
                DataValueCollection values = new DataValueCollection();

                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }

        /// <summary>
        /// 发送唯一性检验合格信号
        /// </summary>
        public  void SendUniqOK()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_UniqOK, ServerIndex));
                DataValueCollection values = new DataValueCollection();

                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }

        /// <summary>
        /// 发送电芯NG剔料完成
        /// </summary>
        public  void SendDianXinNgOk()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_CellRemoveOK, ServerIndex));
                DataValueCollection values = new DataValueCollection();

                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }



        /// <summary>
        /// 发送唯一性检验NG信号
        /// </summary>
        public void SendUniqNG()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_UniqNG, ServerIndex));
                DataValueCollection values = new DataValueCollection();

                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }


        public  void SendMaterialPullOk()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_MaterialPullOK, ServerIndex));
                DataValueCollection values = new DataValueCollection();

                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }

        public void SendMaterialPullNG()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_MaterialPullNG, ServerIndex));
                DataValueCollection values = new DataValueCollection();

                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }

        /// <summary>
        /// 发送错误代码
        /// </summary>
        public   void SendErrorsCode(byte errorcode)
        {
            try
            {
                NodeIdCollection writingNode = new NodeIdCollection();
                //string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                writingNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_ErrorCode, ServerIndex));//错误代码
                DataValueCollection values = new DataValueCollection();
                byte [] buff = { errorcode };
                DataValue v = new DataValue();v.Value = errorcode;
                values.Add(new DataValue(v));
                StatusCodeCollection codes;
                MyServer.WriteValues(writingNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }

        public  void SendRepairReadDown()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_RepairReadDown, ServerIndex));//度完成
                DataValueCollection values = new DataValueCollection();

                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }

        public void SendRepairWriteDown()
        {
            try
            {
                NodeIdCollection readdownNode = new NodeIdCollection();
                // string comDB = MyStationModel.CommunicationDB;//握手信号交互DB块
                string comDB = MyStationModel.DataWriteDB;//握手信号交互DB块
                readdownNode.Add(new NodeId(comDB + MyBaseProtocol.MES_PLC_RepairWriteDown, ServerIndex));//度完成
                DataValueCollection values = new DataValueCollection();

                values.Add(new DataValue(true));
                StatusCodeCollection codes;
                MyServer.WriteValues(readdownNode, values, out codes);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
            }
        }

        ~OpcHelper()
        {
            Console.WriteLine("内存释放");
        }

    }
}
