using OPC_UA_Client_A50.Comom;
using OPC_UA_Client_A50.Dal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OPC_UA_Client_A50.BLL
{
    public  class ProdDataBll
    {
        private SqlHelper sqlHelper;
        public ProdDataBll()
        {
            sqlHelper = new SqlHelper();
        }
        /// <summary>
        /// 保存10上线信息
        /// </summary>
        /// <param name="ordid">订单号</param>
        /// <param name="sncode">产品sn号(packid)</param>
        /// <param name="monum">生成的模组号</param>
        /// <returns></returns>
        public  bool SaveOnLineInfo( string  ordid,string  sncode,string monum,string stncode)
        {
            try
            {
                if (stncode=="OP010A")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("insert into tblProduct (WorkOrderID,ProductSerialNumber,MozuSerialNum,CreateDatetime)  values ('{0}','{1}','{2}','{3}') ", ordid, sncode, monum, DateTime.Now);//保存上线信息
                    StringBuilder sb2 = new StringBuilder();
                    sb2.AppendFormat("select count(1) from tbDaySerialNumber where YearTag='{0}' and MonthTag='{1}' and DayTag='{2}'", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);//查询当天是否有上线记录
                    if (sqlHelper.ExecNonQuery(sb.ToString()) > 0)
                    {
                        int record = Convert.ToInt32(sqlHelper.GetObjectVal(sb2.ToString()));//
                        if (record > 0)
                        {
                            sb2.Clear();
                            sb2.AppendFormat("update [tbDaySerialNumber] set SearialNumber=SearialNumber+1 where YearTag='{0}' and MonthTag='{1}' and DayTag='{2}'", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                            sqlHelper.ExecNonQuery(sb2.ToString());
                        }
                        else
                        {
                            sb2.Clear();
                            sb2.AppendFormat("insert into [dbo].[tbDaySerialNumber] (YearTag,MonthTag,DayTag,SearialNumber) values ('{0}','{1}','{2}','{3}') ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1);
                            sqlHelper.ExecNonQuery(sb2.ToString());
                        }
                         bool res= UpdateOrderStatus(ordid);
                        return res;
                    }
                    else
                    {
                        // UpdateOrderStatus(ordid);
                        //如果单工位上线要存上线记录可以在这里面写代码
                        return false;
                    } 
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex, "system");
                return false;
            }
        }


        public  int UpdateDianXinOrder(string  ordernum)
        {
            string sqlstr = "select * from tblDianXinOrder where WorkOrderNo='" + ordernum + "'";
            DataTable ordtb = sqlHelper.GetDataTb(sqlstr);
            if (ordtb.Rows.Count > 0)
            {
                int planCount = Convert.ToInt32(ordtb.Rows[0]["PackCount"]);
                int onlineCount = Convert.ToInt32(ordtb.Rows[0]["OnlineCount"]);
                int r;
                if (onlineCount + 1 < planCount)     //上线数量99 计划数量100
                {
                    string updatesql = "update [tblDianXinOrder] set [OnlineCount]=[OnlineCount]+1 ,OrderStatus=1 where WorkOrderNo='" + ordernum + "' ";
                    r = sqlHelper.ExecNonQuery(updatesql);
                }
                else
                {
                    string updatesql = "update [tblDianXinOrder] set [OnlineCount]=[OnlineCount]+1 ,OrderStatus=2 where WorkOrderNo='" + ordernum + "' ";
                    r = sqlHelper.ExecNonQuery(updatesql);
                }
                if (r > 0)
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
                return 2;
            }
        }


       public bool UpdateOrderStatus(string  ordID)
        {
            string sqlstr = "select * from tblWorkOrder where WorkOrderID='"+ordID+"'";
            DataTable ordtb = sqlHelper.GetDataTb(sqlstr);
            int planCount = Convert.ToInt32(ordtb.Rows[0]["PlanQty"]);
            int onlineCount = Convert.ToInt32(ordtb.Rows[0]["ReleaseQty"]);
            int r;
            if (onlineCount +1 <planCount)     //上线数量99 计划数量100
            {
                string updatesql = "update [tblWorkOrder] set [ReleaseQty]=[ReleaseQty]+1 ,OrderStatus=1 where WorkOrderID='" + ordID+"' ";
              r=  sqlHelper.ExecNonQuery(updatesql);
            }
            else
            {
                string updatesql = "update [tblWorkOrder] set [ReleaseQty]=[ReleaseQty]+1 ,OrderStatus=2 where WorkOrderID='" + ordID + "' ";
             r=   sqlHelper.ExecNonQuery(updatesql);
            }
            if (r>0)
            {
                return true;
            }
            else
            {
                return false;
            }
           
        }
        public DataTable GetOrderDataTable(string stncode,string  frontstncode,string keticode)
        {
            DataTable tb;
            string sqlstr;
            if (stncode=="OP010A")
            {
                sqlstr = "exec [SP_GetCurrendOrder]'" + stncode + "','" + frontstncode + "',''";
            }
            else
            {
                sqlstr = "exec [SP_GetCurrendOrder] '" + stncode + "','"+ frontstncode + "','"+ keticode + "'";
            }
            tb = sqlHelper.GetDataTb(sqlstr);
            return tb;
        }
        public DataTable GetDianXinOrder()
        {
            DataTable tb;
            string sqlstr = "exec [dbo].[SP_GetDianXinOrder]";
            tb = sqlHelper.GetDataTb(sqlstr);
            return tb;
        }

        public  DataTable  GetBom(string  stncode,string partnum)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("select * from tblComponentPart where ComponentPartNo='{0}' and ComponentPartStationName='{1}' ",partnum ,stncode );
             return  sqlHelper.GetDataTb(sb.ToString());
        }
        /// <summary>
        /// 根据配方查询工位bom
        /// </summary>
        /// <param name="stncode"></param>
        /// <param name="partnum"></param>
        /// <returns></returns>
        public DataTable GetBomByFormula(string stncode, string partnum)
        {
            StringBuilder sb = new StringBuilder();
            //if (stncode=="OP100A")
            //{
            //    sb.AppendFormat("select * from VI_Formula where ComponentPartNo='{0}' and [StationCode]  like '%{1}%' ", partnum, "OP100");
            //}
            //else
            //{
            //    sb.AppendFormat("select * from VI_Formula where ComponentPartNo='{0}' and [StationCode] = '{1}' ", partnum,stncode);
            //}
            sb.AppendFormat("select * from VI_Formula where ComponentPartNo='{0}' and [StationCode] = '{1}' ", partnum, stncode);
            return sqlHelper.GetDataTb(sb.ToString());
        }

        /// <summary>
        /// 唯一性检验结果，不重复返回true，重复返回false
        /// </summary>
        /// <param name="partcode">物料条码</param>
        /// <returns></returns>
        public  bool CheckUniqueness( string  partcode)
        {
           
            string sqlstr = " select  count(1)  from tblComponentPartData  where ComponentPartBarcode='" + partcode + "' and  IsDelete!=1";
            object  res= sqlHelper.GetObjectVal(sqlstr);
            if (Convert.ToInt32(res)>0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 校验电芯唯一性
        /// </summary>
        /// <param name="batteryCode"></param>
        /// <returns></returns>
        public bool CheckBatteryUniqueness(string batteryCode)
        {

            string sqlstr = " select  count(1)  from tblPassStationData  where SerialNumber='" + batteryCode + "' and  IsDelete!=1";
            object res = sqlHelper.GetObjectVal(sqlstr);
            if (Convert.ToInt32(res) > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 获取生产记录表
        /// </summary>
        /// <param name="packid"></param>
        /// <returns></returns>
        public DataTable GetProdRecord(string packid)
        {
            StringBuilder sb2 = new StringBuilder();
            sb2.AppendFormat("select * from [tblProduct]   where ProductSerialNumber='{0}'", packid);
            return sqlHelper.GetDataTb(sb2.ToString());
        }

        public  int  SaveBomData(string sql)
        {
            return sqlHelper.ExecNonQuery(sql);
        }



        /// <summary>
        /// 查询配方程序号
        /// </summary>
        /// <param name="ptcode"></param>
        /// <param name="stncode"></param>
        /// <returns></returns>
        public Dictionary<string,byte[]> GetFormulaData(string ptcode, string stncode)
        {
            string itemName, sqlstr;
            byte[] buff;
            Dictionary<string, byte[]> dic = new Dictionary<string, byte[]>();
            if (stncode.Equals("OP050A"))//pack段上线工位
            {
                string sqlstr1 = "select StationName,StationSeq,LineCode from tblStation  where LineCode='PACK'  and StationName  not in ('OP010A','OP020A1_1','OP020A1_3','OP020A1_4','OP020A2_1','OP020A2_3','OP020A2_4','OP030M','OP040A_1','OP040A_2','OP040A_3','OP040A_4') ORDER BY StationSeq ";
                DataTable packstntb = sqlHelper.GetDataTb(sqlstr1);//把所有工位查出来
                sqlstr = "select [PartNo],[StationCode],[StationSeq],[WorkStepNo],[Operation],[ProgramNo],[OperationDescription] from VI_Formula  where [PartNo]='" + ptcode + "'  order by  StationSeq ,WorkStepNo  ";
                itemName = ".292,b,192";
                buff = new byte[192];
                DataTable formutb = sqlHelper.GetDataTb(sqlstr);

                #region   配方
                int j = 0;
                for (int i = 0; i < packstntb.Rows.Count; i++)
                {
                    string tempstncode = packstntb.Rows[i]["StationName"].ToString();
                    //  DataRow[] rows = formutb.Select("StationCode='" + tempstncode + "' and ProgramNo is  not  null   ");//按照工位查配方表
                    DataRow[] rows = formutb.Select("StationCode='" + tempstncode + "'", "WorkStepNo");//按照工位查配方表
                    if (tempstncode != "OP220A")
                    {
                        for (int k = 0; k < rows.Length; k++)
                        {
                            byte programNum;
                            if (byte.TryParse(rows[k]["ProgramNo"].ToString(), out programNum))
                            {
                                buff[j + k] = Convert.ToByte(rows[k]["ProgramNo"].ToString());
                            }
                        }
                    }
                    else  //PLC要求 220工位只写一个程序号，这里就写第一个程序要
                    {
                        byte programNum;
                        byte.TryParse(rows[0]["ProgramNo"].ToString(), out programNum);
                        buff[j] = programNum;
                    }

                    j += 8;
                }
                #endregion

                dic[itemName] = buff;
                return dic;
            }
            else if (stncode.Contains("C-OP010A1")||stncode.Contains("C-OP010A2"))
            {
                string sqlstr1 = "";
                if (stncode.Contains("C-OP010A1"))
                {
                    sqlstr1 = "select StationName,StationSeq,LineCode from tblStation  where LineCode='DX1'  ORDER BY StationSeq ";
                }
                else
                {
                    sqlstr1 = "select StationName,StationSeq,LineCode from tblStation  where LineCode='DX2'  ORDER BY StationSeq ";
                }
                DataTable packstntb = sqlHelper.GetDataTb(sqlstr1);//把所有工位查出来
                sqlstr = "select [FormulaID],[PartNo],[BatteryTypeName],[FormulaNo],[StationCode],[StationSeq],[WorkStepNo],[Operation],[OperationDescription],[ComponentPartCount],[ComponentPartName],[ProgramNo],[ComponentPartNo],[ComponentPartOnly] from VI_Formula  where [PartNo]='" + ptcode + "'  order by  StationCode ,WorkStepNo  ";
                DataTable formutb = sqlHelper.GetDataTb(sqlstr);
                itemName = ".292,b,56";
                buff = new byte[56];
                #region   配方
                int j = 0;
                for (int i = 0; i < packstntb.Rows.Count; i++)
                {
                    string tempstncode = packstntb.Rows[i]["StationName"].ToString();
                    //   DataRow[] rows = formutb.Select("StationCode='" + tempstncode + "' and ProgramNo is  not  null   ");//按照工位查配方表
                    DataRow[] rows = formutb.Select("StationCode='" + tempstncode + "' ", "WorkStepNo");//按照工位查配方表
                    for (int k = 0; k < rows.Length; k++)
                    {
                        byte programNum;
                        if (byte.TryParse(rows[k]["ProgramNo"].ToString(), out programNum))
                        {
                            buff[j + k] = Convert.ToByte(rows[k]["ProgramNo"].ToString());
                        }
                    }
                    j += 8;
                }
                #endregion
                dic[itemName] = buff;
                return dic;
            }
            else
            {
              
                sqlstr = "select * from VI_Formula  where  [PartNo]='" + ptcode + "'  and  StationCode ='" + stncode + "'  order by  StationCode ,WorkStepNo";
                itemName = ".292,b,8";
                buff = new byte[8];
                 DataTable formutb = sqlHelper.GetDataTb(sqlstr);
                for (int i = 0; i < formutb.Rows.Count; i++)
                {
                    byte programNum;
                    if (byte.TryParse(formutb.Rows[i]["ProgramNo"].ToString(), out programNum))
                    {
                        buff[i] = programNum;
                    }
                }
                dic[itemName] = buff;
                return dic;
            }
        }


        /// <summary>
        /// 根据下壳体码 获取数据
        /// </summary>
        /// <param name="keticode"></param>
        /// <returns></returns>
        public  DataTable  GetRepairData(string keticode)
        {
            try
            {
                string sqlstr = "select * from  tblTagValue  where KetiCode='" + keticode + "'";
                DataTable tb = sqlHelper.GetDataTb(sqlstr);
                return tb;
            }
            catch (Exception  ex)
            {
                LogHelper.Write("获取返修数据时异常:" + ex.Message, "system");
                return null;
            }
        }


        /// <summary>
        /// 获取电芯OCV测试K值结果
        /// </summary>
        /// <param name="dianxin">电芯二维码</param>
        /// <param name="vol">测试电压值</param>
        /// <returns></returns>
        public int GetDianXinOCVTestResult(string  stncode,string  dianxin,float  vol)
        {
            string sqlStr = "select PartNo,StationCode,Operation, MaxValue1,MinValue1,MaxValue2,MinValue3,MaxValue3,MinValue3 from  [VI_Formula]   where  StationCode ='"+stncode+"' AND  Operation='OCV测试'";
            DataTable tempTable = sqlHelper.GetDataTb(sqlStr);
            if (tempTable.Rows.Count<=0)
            {
                return 28;//未获取到配方信息
            }
            else
            {
                float tempMax = Convert.ToSingle(tempTable.Rows[0]["MaxValue1"]);
                float tempMin = Convert.ToSingle(tempTable.Rows[0]["MinValue1"]);
                if (tempMin<= vol && vol<= tempMax)//电压在配方范围里
                {
                    string sqlGetkValue = "exec SP_CalcOCVKvalue  '"+stncode+"' , '" + dianxin + "','" + vol + "'";
                    DataTable tb = sqlHelper.GetDataTb(sqlGetkValue);
                    if (tb != null)
                    {
                        int dianxinCount = Convert.ToInt32(tb.Rows[0]["DianXinCount"]);
                        if (dianxinCount > 1)
                        {
                            return 25;//电芯二维码来料重复
                        }
                        else if (dianxinCount < 1)
                        {
                            return 24;//电芯二维码不在库里
                        }
                        else
                        {
                            if (tb.Rows[0]["Result"].ToString() == "1")
                            {
                                return 0;
                            }
                            else
                            {
                                return 26;//ocv测试NG
                            }
                        }
                    }
                    else
                    {
                        return 9;//查数据库失败
                    }
                }
                else
                {
                    return 14;//数据有效性不通过（不在配方范围内）
                }
            }

           
        }
    }
}
