using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OPC_UA_Client_A50.Comom;
using OPC_UA_Client_A50.Model;
using OPC_UA_Client_A50.OpcClienHelper;

namespace OPC_UA_Client_A50.BLL
{
   public class OpcBll
    {


        public StationModel StnModel { get; set; }
        public BaseProtocol MyaseProtocol { get; set; }


        public  OpcHelper myopcHelper { get; set; }
        private ProdDataBll prodbll;

        public OpcBll()
        {
          //  sqlHelper = new SqlHelper();
            prodbll = new ProdDataBll();
        }
        

        public  void InitData()
        {
            int res;
            if (StnModel.StationCode.Contains("C-OP010"))
            {
                res = InitDxStnData();
            }
            else
            {
                res = InitStnData();
            }
            if (res==0)
            {
                myopcHelper.SendErrorsCode(0);
                myopcHelper.SendWriteDownCmd();
            }
            else
            {
                byte errorcode = (byte)res;
                myopcHelper.SendErrorsCode(errorcode);
            }
        }


        /// <summary>
        /// 初始化转运线和pack线工位
        /// </summary>
        private int InitStnData()
        {
            myopcHelper.SendWritingCmd();
            string keticode = myopcHelper.ReadQRCodeData();
            DataTable tb = prodbll. GetOrderDataTable(StnModel.StationCode, keticode);
             if (! String.IsNullOrEmpty(tb.Rows[0]["OrderNum"].ToString()))
            {
                string ptcode = tb.Rows[0]["PartNum"].ToString();//成品物料号
                string ordnum = tb.Rows[0]["OrderNum"].ToString();
                string sncode = tb.Rows[0]["SnCode"].ToString();//sncode(pakcid)
                string ordID = tb.Rows[0]["ordID"].ToString();
                string mzpartcode = tb.Rows[0]["MozuPartCode"].ToString();
                string mzsncode = tb.Rows[0]["MozuCode"].ToString();
                Dictionary<string, byte[]> dic = prodbll.GetFormulaData(ptcode, StnModel.StationCode);
                if (myopcHelper.InitRequestData(ordnum,sncode,ptcode,mzpartcode,mzsncode,dic))
                {
                        if (prodbll.SaveOnLineInfo(ordID, sncode, mzsncode,StnModel.StationCode))
                        {
                            return 0;
                        }
                        else
                        {
                            return 10;
                        } 
                }
                else
                {
                    return 3;
                }
            }
            else
            {
                return 2;
            }
        }

        public  int InitRepairData()
        {
            try
            {
                string keticode = myopcHelper.ReadQRCodeData();//读一下下壳体码
                DataTable tb = prodbll.GetRepairData(keticode);
                string[] objdata = tb.Rows[0]["TagValue"].ToString().Split(',');
                byte[] data = new byte[objdata.Length];
                for (int i = 0; i < objdata.Length; i++)
                {
                    data[i] = Convert.ToByte(objdata[i]);
                }
                if (myopcHelper.InitRepairPLCData(data))
                {
                    return 0;
                }
                else
                {
                    return 3;
                }
            }
            catch (Exception ex )
            {
                LogHelper.Write("准备返修数据时异常:" + ex.Message, "system");
                return 100;
            }
        }

       

        /// <summary>
        /// 初始化电芯上线工位
        /// </summary>
        /// <returns></returns>
        private  int InitDxStnData()
        {
            myopcHelper.SendWritingCmd();
            DataTable tb = prodbll.GetDianXinOrder();
            if (!String.IsNullOrEmpty(tb.Rows[0]["OrderNum"].ToString()))
            {
                string ptcode = tb.Rows[0]["PartNum"].ToString();//成品物料号
                string ordnum = tb.Rows[0]["OrderNum"].ToString();

                Dictionary<string, byte[]> dic = prodbll.GetFormulaData(ptcode, StnModel.StationCode);
                if (myopcHelper.InitRequestData(ordnum, "", "", "", "",dic))
                {
                    return prodbll.UpdateDianXinOrder(ordnum);
                }
                else
                {
                   return 3;//opc写数据失败
                }
            }
            else
            {
               // myopcHelper.SendErrorsCode(2);//没订单
                return 2;
            }
        }
        

        
        public  void CheckBom()
        {
            try
            {
                string qrcode = myopcHelper.ReadQRCodeData();
                if (qrcode.Length > 10)
                {
                    string partnum = qrcode.Substring(0, 10);//物料条码前10位 是  物料号
                    if (StnModel.StationCode.Contains("C"))
                    {
                        ////电芯段物料单独处理
                        //string sntcode = StnModel.StationCode.Substring(0, 8);
                        //DataTable bomtb = prodbll.GetBomByFormula(sntcode, partnum);
                        //if (bomtb.Rows.Count > 0)
                        //{
                        //    bool isPartOnly = false;
                        //    bool.TryParse(bomtb.Rows[0]["ComponentPartOnly"].ToString(), out isPartOnly);
                        //    if (isPartOnly)
                        //    {
                        //        #region 唯一性检验
                        //        if (!prodbll.CheckUniqueness(qrcode))//唯一性检验不合格，直接告诉PLC，不存库了
                        //        {
                        //            myopcHelper.SendErrorsCode(13);//唯一性检验不合格
                        //            myopcHelper.SendUniqNG();
                        //            myopcHelper.SendBomNGCmd();
                        //            return;
                        //        }
                        //        else//唯一性检验合格
                        //        {
                        //            myopcHelper.SendErrorsCode(0);
                        //            myopcHelper.SendUniqOK();
                        //            myopcHelper.SendBomOKCmd();
                        //        }
                        //        #endregion
                        //    }
                        //}
                        //else
                        //{
                        //    myopcHelper.SendBomNGCmd();
                        //}
                          myopcHelper.SendBomOKCmd();
                    }
                    else
                    {
                        DataTable bomtb = prodbll.GetBomByFormula(StnModel.StationCode, partnum);
                        if (bomtb.Rows.Count > 0)
                        {
                            #region 需要唯一性检验的物料,,暂时不启用
                            bool isPartOnly = false;
                            bool.TryParse(bomtb.Rows[0]["ComponentPartOnly"].ToString(), out isPartOnly);
                            if (isPartOnly)
                            {
                                #region 唯一性检验
                                //if (!prodbll.CheckUniqueness(qrcode))//唯一性检验不合格，直接告诉PLC，不存库了
                                //{
                                //    myopcHelper.SendErrorsCode(13);//唯一性检验不合格
                                //    myopcHelper.SendUniqNG();
                                //    myopcHelper.SendBomNGCmd();
                                //    return;
                                //}
                                //else//唯一性检验合格
                                //{
                                //    myopcHelper.SendErrorsCode(0);
                                //    myopcHelper.SendUniqOK();
                                //    if (StnModel.StationCode == "OP010A")//OP10先把下壳体二维码发过来了，此时还没上线，所以不需要写库，下线时候在写库
                                //    {
                                //        myopcHelper.SendBomOKCmd();
                                //        return;
                                //    }
                                //} 
                                #endregion

                                #region 调试阶段
                                if (StnModel.StationCode == "OP010A")
                                {
                                    myopcHelper.SendBomOKCmd();
                                    return;
                                }
                                #endregion
                            }
                            #endregion
                            string packid = myopcHelper.ReadPackID();
                            string operatorNum = myopcHelper.ReadOperatorID();
                            #region 获取生产记录，查找订单ID和生产记录ID
                            DataTable prodRecord = prodbll.GetProdRecord(packid);
                            int prodid = -1, orderid = -1;
                            if (prodRecord.Rows.Count > 0)
                            {
                                prodid = Convert.ToInt32(prodRecord.Rows[0]["ProductID"]);
                                orderid = Convert.ToInt32(prodRecord.Rows[0]["WorkOrderID"]);
                            }
                            #endregion

                            #region 物料条码写库
                            StringBuilder sb2 = new StringBuilder();
                            sb2.AppendFormat("insert into [tblComponentPartData] (ProductID,WorkOrderID,ComponentPartNo,ComponentPartBarcode,OperationTime,StationCode,[Description],UserNo,[AllocNum]) values ");
                            sb2.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')", prodid, orderid, partnum, qrcode, DateTime.Now, StnModel.StationCode, bomtb.Rows[0]["ComponentPartName"].ToString(), operatorNum, bomtb.Rows[0]["ComponentPartCount"].ToString());
                            int r = prodbll.SaveBomData(sb2.ToString());
                            if (r > 0)
                            {
                                myopcHelper.SendBomOKCmd();
                            }
                            else
                            {
                                myopcHelper.SendErrorsCode(10);//写库失败
                            }
                            #endregion
                        }
                        else
                        {
                            myopcHelper.SendBomNGCmd();
                        }
                    }
                }
                else
                {
                    myopcHelper.SendErrorsCode(12);//物料条码长度不合格
                    myopcHelper.SendBomNGCmd();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write("检验BOM是异常:" + ex.Message, "system");
                myopcHelper.SendErrorsCode(100);
            }
            
        }

        /// <summary>
        /// ocv检测
        /// </summary>
        /// <param name="bytes"></param>
        public void CheckOCVTest(byte[]  bytes)
        {
            string dxQrcode = ConvertHelper.ByteToString(bytes, 500, 32);
            float dxvol = ConvertHelper.ByteArrtoFolat(bytes, 532);
            float dxres = ConvertHelper.ByteArrtoFolat(bytes, 538);
            float dxhig1 = ConvertHelper.ByteArrtoFolat(bytes, 544);
            float dxhig2 = ConvertHelper.ByteArrtoFolat(bytes, 550);
            myopcHelper.SendOCV_OKCmd();
          //  myopcHelper.SendOCV_NGCmd();
        }
    }
}
