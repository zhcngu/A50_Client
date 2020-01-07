
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Net.Sockets;

using OPC_UA_Client_A50.Dal;
using OPC_UA_Client_A50.Model;
using OPC_UA_Client_A50.Comom;

namespace OPC_UA_Client_A50.BLL
{
    public class AnalDataHelper
    {
        SqlHelper sqlHelper { get; set; }
        public StationModel StnModel { get; set; }

        public AnalDataHelper(StationModel stnmodel)
        {
            sqlHelper = new SqlHelper();
            StnModel = stnmodel;
        }

        /// <summary>
        /// 解析packid段 拧紧数据方法
        /// </summary>
        /// <param name="datas">读上来的原始数据</param>
        /// <returns></returns>
        public int AnalyScrewData(byte[] datas)
        {
            try
            {
                #region 基础值
                int star = Convert.ToInt32(StnModel.DataAddress);
                int end = Convert.ToInt32(StnModel.DataLength);
                int datacount = (end - star) / 26 + 1;//数据个数
                string ordernum = ConvertHelper.ByteToString(datas, 60, 40);//订单号
                string partNo = ConvertHelper.ByteToString(datas, 100, 20);//总成物料号
                string packID = ConvertHelper.ByteToString(datas, 120, 70);//packid
                short passmode = ConvertHelper.BytesToShort(datas, 360);//过站信息，0无操作过站，1有操作过站
                string operatorNum = ConvertHelper.ByteToString(datas, 362, 10);//操作员号
                if (String.IsNullOrEmpty(operatorNum))
                {
                    operatorNum = "自动";
                }
                int workstatus = datas[453];
                string devicecode, deviceid;
                devicecode = ConvertHelper.ByteToString(datas, 464, 16);//设备编号
                deviceid = ConvertHelper.ByteToString(datas, 480, 16);//设备id
                short taotongnum = ConvertHelper.BytesToShort(datas, 496);//套同号
                short pronum = ConvertHelper.BytesToShort(datas, 498);//程序调用号 
                float nm, angle, nmmax, nmmin, agmax, agmin;//扭矩，角度，扭矩最大，扭矩最小，角度最大，角度最小
                short tempres;//单个拧紧结果
                #endregion
                List<float> datalist = new List<float>();
                float[,] datamat = new float[datacount, 2];//数据校验矩阵
                int j = 1;
                StringBuilder sqlsb = new StringBuilder();
                sqlsb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],[DetectionAccessory],DetectedResult,TypeGroup,PassStatus,DeviceCode) values ");
                int deleteRes = DeleteQualitiesData(packID, StnModel.StationCode);
                int tempindex = 0;
                if (deleteRes == 0)
                {
                    for (int i = star; i <= end;)
                    {
                        nm = ConvertHelper.ByteArrtoFolat(datas, i);//501
                        datalist.Add(nm);
                        datamat[tempindex, 0] = nm;
                        angle = ConvertHelper.ByteArrtoFolat(datas, i + 4);//505
                        datalist.Add(angle);
                        datamat[tempindex, 1] = angle;
                        agmax = ConvertHelper.ByteArrtoFolat(datas, i + 4 + 4);//509
                        agmin = ConvertHelper.ByteArrtoFolat(datas, i + 4 + 4 + 4);//513
                        nmmax = ConvertHelper.ByteArrtoFolat(datas, i + 4 + 4 + 4 + 4);//517
                        nmmin = ConvertHelper.ByteArrtoFolat(datas, i + 4 + 4 + 4 + 4 + 4);//521
                        tempres = ConvertHelper.BytesToShort(datas, i + 4 + 4 + 4 + 4 + 4 + 4);//525
                        sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','1','{14}','{15}'),", packID, StnModel.StationCode, ordernum, operatorNum, DateTime.Now, j, nm, nmmax, nmmin, "N·m", deviceid, pronum, taotongnum, tempres,workstatus, devicecode);
                        sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','1','{14}','{15}'),", packID, StnModel.StationCode, ordernum, operatorNum, DateTime.Now, j, angle, agmax, agmin, "°", deviceid, pronum, taotongnum, tempres,workstatus, devicecode);
                        j += 1;
                        i += 26;
                        tempindex++;
                    }
                    #region 数据校验
                    if (datas[453] == 1)
                    {
                        int partValid = CheckPartDataComplete(packID, StnModel.StationCode, partNo);
                        if (partValid != 0)
                        {
                            return partValid;
                        }
                        if (CheckDataIntegrity(datalist))
                        {
                            bool checkres = CheckDataVaid(StnModel.StationCode, "拧紧", partNo, datamat);
                            if (checkres == false)
                            {
                                return 14;
                            }
                        }
                        else
                        {
                            return 15;
                        }
                    }
                    #endregion
                    string sqlstr = sqlsb.ToString().Substring(0, sqlsb.Length - 1);
                    int res2 = sqlHelper.ExecNonQuery(sqlstr);
                    if (res2 > 0)
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
                    return deleteRes;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;
            }
        }

        /// <summary>
        /// 解析OP060激光清洗数据
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public int AnalyLaserClear(byte[] datas)
        {
            try
            {
                #region 基础信息
                int start = Convert.ToInt32(StnModel.DataAddress);//配置文件里面数据地址
                int end = Convert.ToInt32(StnModel.DataLength);//数据结束地址
                string ordernum = ConvertHelper.ByteToString(datas, 60, 40);//订单号
                string partNo = ConvertHelper.ByteToString(datas, 100, 20);//总成物料号
                string packID = ConvertHelper.ByteToString(datas, 120, 70);//packid
                short passmode = ConvertHelper.BytesToShort(datas, 360);//过站信息，0无操作过站，1有操作过站
                string operatorNum = ConvertHelper.ByteToString(datas, 362, 10);//操作员号
                if ( String.IsNullOrEmpty( operatorNum))
                {
                    operatorNum = "自动";
                }
                int workstatus = datas[453];
                string devicecode = ConvertHelper.ByteToString(datas, 464, 16);//设备编号
                string deviceid = ConvertHelper.ByteToString(datas, 480, 16);//设备id
                short taotongNum = ConvertHelper.BytesToShort(datas, 496);//套筒选择
                short pronum = ConvertHelper.BytesToShort(datas, 498);//程序调用号
                #endregion
                float value, valuemax, valuemin,speed,speedmax=0,speedmin=0;//60激光清洗  
                List<float> datalist = new List<float>();//数据校验集合
                float[,] datamat = new float[26, 1];//数据校验矩阵
                short tempres;
                int j = 1;
                StringBuilder sqlsb = new StringBuilder();
                sqlsb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],DetectedResult,TypeGroup,PicName,PassStatus,DetectionAccessory,DeviceCode) values ");
                string picname;
                int tempindex = 0;
                for (int i = start; i <= end;)
                {
                    tempres = ConvertHelper.BytesToShort(datas, i+46);//单次的结果
                    picname = ConvertHelper.ByteToString(datas, i + 16, 30);//图片名字
                    value = ConvertHelper.ByteArrtoFolat(datas, i );//功率
                    datalist.Add(value);
                    datamat[tempindex, 0] = value;
                    valuemax = ConvertHelper.ByteArrtoFolat(datas, i+ 4);//功率最大
                    valuemin = ConvertHelper.ByteArrtoFolat(datas, i + 8);//功率最小
                    sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','13','{13}','{14}','{15}','{16}'),", packID, StnModel.StationCode, ordernum, operatorNum, DateTime.Now, j, value, valuemax, valuemin, "W", deviceid, pronum, tempres, picname,workstatus,taotongNum,devicecode);
                    speed = ConvertHelper.ByteArrtoFolat(datas, i + 12);//速度
                    sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','13','{13}','{14}','{15}','{16}'),", packID, StnModel.StationCode, ordernum, operatorNum, DateTime.Now, j, speed, speedmax, speedmin, "mm/s", deviceid, pronum, tempres, picname,workstatus, taotongNum, devicecode);

                    j++;
                    i += 48;
                    tempindex++;
                }
                float highValue;
                float rangeValue, rangeSet;

                //极柱高度以及高度差

                for (int i = 1808; i <= 2158;)
                {

                    tempres = ConvertHelper.BytesToShort(datas, i + 12);//单次的结果
                    highValue = ConvertHelper.ByteArrtoFolat(datas, i);//1号位极柱高度
                    sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','8','','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordernum, operatorNum, DateTime.Now, j, highValue, 0, 0, "mm", deviceid, pronum, tempres,workstatus, taotongNum, devicecode);
                    rangeValue = ConvertHelper.ByteArrtoFolat(datas, i + 4);//极差
                    sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','8','','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordernum, operatorNum, DateTime.Now, j, rangeValue, 0, 0, "mm", deviceid, pronum, tempres,workstatus, taotongNum, devicecode);
                    rangeSet = ConvertHelper.ByteArrtoFolat(datas, i + 8);//极差设定值
                    sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','8','','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordernum, operatorNum, DateTime.Now, j, rangeSet, 0, 0, "mm", deviceid, pronum, tempres,workstatus, taotongNum, devicecode);
                    j++;
                    i += 14;
                }

                #region 数据校验
                if (datas[453] == 1)
                {
                    int partValid = CheckPartDataComplete(packID, StnModel.StationCode, partNo);
                    if (partValid != 0)
                    {
                        return partValid;
                    }
                    if (CheckDataIntegrity(datalist))
                    {
                        bool checkres = CheckDataVaid(StnModel.StationCode, "激光清洗", partNo, datamat);
                        if (checkres == false)
                        {
                            return 14;
                        }
                    }
                    else
                    {
                        return 15;
                    } 
                }
                #endregion
                string sqlstr = sqlsb.ToString().Substring(0, sqlsb.Length - 1);
                #region 60工位2个mark点照片
                string markImg1 = ConvertHelper.ByteToString(datas, 2192, 30);
                short tempresult1 = ConvertHelper.BytesToShort(datas, 2222);
                string markImg2 = ConvertHelper.ByteToString(datas, 2224, 30);
                short tempresult2 = ConvertHelper.BytesToShort(datas, 2254);
                StringBuilder sb2 = new StringBuilder();
                sb2.AppendFormat("insert into tblSKQImage (SerialNumber,WorkOrderNr,StationCode,EnterStationTime,Location,PicName,PicResult,StationResult,ImgDescription)  values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')", packID, ordernum, StnModel.StationCode, DateTime.Now, 1, markImg1, tempresult1, datas[453], "Mark点1图片");
                sb2.AppendFormat(",('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')", packID, ordernum, StnModel.StationCode, DateTime.Now, 2, markImg2, tempresult2, datas[453], "Mark点2图片");
                #endregion
                List<string> sqllist = new List<string>();
                sqllist.Add(sqlstr);
                sqllist.Add(sb2.ToString());

                int deleteRes = DeleteQualitiesData(packID, StnModel.StationCode);//数据覆盖
                if (deleteRes != 0)
                {
                    return deleteRes;
                }
                int res2 = sqlHelper.ExecNonQuery(sqllist);
                if (res2 > 0)
                {
                    return 0;
                }
                else
                {
                    return 10;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;
            }
        }

        /// <summary>
        /// 解析OP080,OP110焊接数据
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public int AnalyWeldData(byte[] datas)
        {
            try
            {
                #region 基础值
                int start = Convert.ToInt32(StnModel.DataAddress);
                int end = Convert.ToInt32(StnModel.DataLength);
                string ordernum = ConvertHelper.ByteToString(datas, 60, 40);//订单号
                string partNo = ConvertHelper.ByteToString(datas, 100, 20);//总成物料号
                string packID = ConvertHelper.ByteToString(datas, 120, 70);//packid
                short passmode = ConvertHelper.BytesToShort(datas, 360);//过站信息，0无操作过站，1有操作过站
                string operatorNum = ConvertHelper.ByteToString(datas, 362, 10);//操作员号
                if (String.IsNullOrEmpty(operatorNum))
                {
                    operatorNum = "自动";
                }
                string devicecode, deviceid;
                int workstatus = datas[453];//加工状态
                devicecode = ConvertHelper.ByteToString(datas, 464, 16);//设备编号
                deviceid = ConvertHelper.ByteToString(datas, 480, 16);//设备id
                short taotongNum = ConvertHelper.BytesToShort(datas,496);//套筒
                short pronum = ConvertHelper.BytesToShort(datas,498);//程序调用号 
                #endregion
                float value, valuemax, valuemin;
                short tempres;
                List<float> datalist = new List<float>();//数据完整性检验集合
                float[,] datamat;//数据有效性验证矩阵
                if (StnModel.StationCode=="OP080A")
                {
                    datamat = new float[26, 3];//数据有效性验证矩阵
                }
                else
                {
                    datamat = new float[14, 3];//数据有效性验证矩阵
                }
                List<string> sqllist = new List<string>();
                int j = 1;
                StringBuilder imgsb = new StringBuilder();
                StringBuilder sqlsb = new StringBuilder();
                sqlsb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],DetectedResult,TypeGroup,PassStatus,DetectionAccessory,DeviceCode) values ");
                int deleteRes = DeleteQualitiesData(packID, StnModel.StationCode);
                if (deleteRes == 0)
                {
                   
                    for (int i = start; i <= end;)
                    {
                        tempres = ConvertHelper.BytesToShort(datas, i);//单次的结果
                        value = ConvertHelper.ByteArrtoFolat(datas, i + 2);//功率
                        valuemax = ConvertHelper.ByteArrtoFolat(datas, i + 6);//5功率最大
                        valuemin = ConvertHelper.ByteArrtoFolat(datas, i + 10);//511功率最小
                        sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','3','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordernum, operatorNum, DateTime.Now, j, value, valuemax, valuemin, "W", deviceid, pronum, tempres,workstatus,taotongNum, devicecode);
                        datalist.Add(value);
                        datamat[j-1, 0] = value;
                        value = ConvertHelper.ByteArrtoFolat(datas, i + 14);//515保护气流量
                        valuemax = ConvertHelper.ByteArrtoFolat(datas, i + 18);//519气流量最大
                        valuemin = ConvertHelper.ByteArrtoFolat(datas, i + 22);//523气流量最小
                        sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','3','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordernum, operatorNum, DateTime.Now, j, value, valuemax, valuemin, "L/min", deviceid, pronum, tempres, workstatus, taotongNum, devicecode);
                        datalist.Add(value);
                        datamat[j-1, 1] = value;
                        value = ConvertHelper.ByteArrtoFolat(datas, i + 26);//527 离焦量测距值
                        valuemax = ConvertHelper.ByteArrtoFolat(datas, i + 30);//531离焦量测距最大
                        valuemin = ConvertHelper.ByteArrtoFolat(datas, i + 34);//534离焦量测距最小
                        sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','3','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordernum, operatorNum, DateTime.Now, j, value, valuemax, valuemin, "mm", deviceid, pronum, tempres,workstatus, taotongNum, devicecode);
                        datalist.Add(value);
                        datamat[j-1, 2] = value;
                        j++;
                        i += 38;
                    }

                    if (StnModel.StationCode=="OP110A")
                    {
                       
                        imgsb.AppendFormat("insert into tblSKQImage (SerialNumber,WorkOrderNr,StationCode,EnterStationTime,Location,PicName,PicResult,StationResult,ImgDescription,DetectionDeviceID,DetectionProgramName,DeviceCode) values ");
                        int location = 1;
                        string imgName = "";//图片名字
                        short tempImgRes = 0;//拍照结果
                        string imgDescr = "";//描述
                        string imgdevicecode, imgdeviceid;
                        short imgpronum;
                        imgdevicecode = ConvertHelper.ByteToString(datas, 1480, 16);//设备编号
                        imgdeviceid = ConvertHelper.ByteToString(datas, 1496, 16);//设备id
                        imgpronum = ConvertHelper.BytesToShort(datas, 1514);//程序调用
                        for (int i = 1032; i <= 1448; )
                        {
                            imgName = ConvertHelper.ByteToString(datas, i, 30);
                            tempImgRes = ConvertHelper.BytesToShort(datas, i + 30);
                            imgDescr = "镍片焊接" + location + "号位拍照";
                            imgsb.AppendFormat(" ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}'),", packID, ordernum, StnModel.StationCode, DateTime.Now, location, imgName, tempImgRes, datas[453], imgDescr, imgdeviceid, imgpronum, imgdevicecode);
                            location++;
                            i += 32;
                        }
                        string igmsql = imgsb.ToString().Substring(0, imgsb.Length - 1);
                        sqllist.Add(igmsql);
                    }

                    #region 数据校验
                    if (datas[453] == 1)
                    {
                        int partValid = CheckPartDataComplete(packID, StnModel.StationCode, partNo);
                        if (partValid != 0)
                        {
                            return partValid;
                        }
                        if (CheckDataIntegrity(datalist))
                        {
                            bool checkres = CheckDataVaid(StnModel.StationCode, "焊接", partNo, datamat);
                            if (checkres == false)
                            {
                                return 14;
                            }
                        }
                        else
                        {
                            return 15;
                        } 
                    }
                    #endregion
                    string sqlstr = sqlsb.ToString().Substring(0, sqlsb.Length - 1);
                    sqllist.Add(sqlstr);
                    int res2 = sqlHelper.ExecNonQuery(sqllist);
                    if (res2 > 0)
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
                    return deleteRes;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;
            }
        }

        /// <summary>
        /// 解析OP090 拍照图片
        /// </summary>
        /// <returns></returns>
        public   int AnalyImageData(byte[] datas)
        {
            #region 基础值
            int start = Convert.ToInt32(StnModel.DataAddress);
            int end = Convert.ToInt32(StnModel.DataLength);
            string ordernum = ConvertHelper.ByteToString(datas, 60, 40);//订单号
            string partNo = ConvertHelper.ByteToString(datas, 100, 20);//总成物料号
            string packID = ConvertHelper.ByteToString(datas, 120, 70);//packid
            short passmode = ConvertHelper.BytesToShort(datas, 360);//过站信息，0无操作过站，1有操作过站
            string operatorNum = ConvertHelper.ByteToString(datas, 362, 10);//操作员号
            if (String.IsNullOrEmpty(operatorNum))
            {
                operatorNum = "自动";
            }
            string devicecode, deviceid;
            short pronum;
            devicecode = ConvertHelper.ByteToString(datas, 464, 16);//设备编号
            deviceid = ConvertHelper.ByteToString(datas, 480, 16);//设备id
            pronum = ConvertHelper.BytesToShort(datas, 498);//程序调用
            #endregion
            int deleteRes = DeleteImageData(packID, StnModel.StationCode);
            if (deleteRes != 0)
            {
                return deleteRes;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("insert into tblSKQImage (SerialNumber,WorkOrderNr,StationCode,EnterStationTime,Location,PicName,PicResult,StationResult,ImgDescription,DetectionDeviceID,DetectionProgramName,DeviceCode) values ");
            int location = 1;
            short tempImgRes = 0;
            string imgName;
            string imgDescr = "";
            for (int i = start; i <= end;)
            {
                imgName = ConvertHelper.ByteToString(datas, i, 30);
                tempImgRes = ConvertHelper.BytesToShort(datas, i + 30);
                if (location<27)
                {
                    imgDescr = "2D相机" + location + "号位拍照";
                }
                else
                {
                    imgDescr = "3D相机" + (location-26) + "号位拍照";
                }
               
                sb.AppendFormat(" ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}'),", packID, ordernum, StnModel.StationCode, DateTime.Now, location, imgName, tempImgRes, datas[453], imgDescr, deviceid, pronum, devicecode);
                location++;
                i += 32;
            }
            string sqlstr = sb.ToString().Substring(0,sb.Length - 1);
             int  res= sqlHelper.ExecNonQuery(sqlstr);
            if (res > 0)
            {
                return 0;
            }
            else
            {
                return 10;
            }
        }


        public int AnalData(byte[] bytesarr)
        {
            try
            {
                int r;
                string packidstr="";
                if (!StnModel.StationCode.Contains("C"))
                {
                    packidstr = ConvertHelper.ByteToString(bytesarr, 120, 70);
                    if (String.IsNullOrEmpty(packidstr))
                    {
                        return 20;
                    }
                    else
                    {
                          
                    }

                }
                switch (StnModel.StationCode)
                {
                    case "C-OP020A1_1":
                        r = AnalOCVData(bytesarr, 0);
                        break;
                    case "C-OP020A2_1":
                        r = AnalOCVData(bytesarr, 0);
                        break;
                    case "C-OP020A1_2":
                        r = AnalOCVData(bytesarr, 0);
                        break;
                    case "C-OP020A2_2":
                        r = AnalOCVData(bytesarr, 0);
                        break;
                    case "C-OP020A1_3":
                        r = AnalOCVData(bytesarr, 0);
                        break;
                    case "C-OP020A2_3":
                        r = AnalOCVData(bytesarr, 0);
                        break;
                    case "C-OP030A1_2":
                        r = AnalC_OP030Data(bytesarr);
                        break;
                    case "C-OP030A2_2":
                        r = AnalC_OP030Data(bytesarr);
                        break;
                    case "C-OP030A1_3":
                        r = AnalC_OP030Data(bytesarr);
                        break;
                    case "C-OP030A2_3":
                        r = AnalC_OP030Data(bytesarr);
                        break;
                    case "C-OP040A1":
                        r = AnalC_OP040Data(bytesarr);
                        break;
                    case "C-OP040A2":
                        r = AnalC_OP040Data(bytesarr);
                        break;
                    case "OP010A":
                        r = AnalOP10Data(bytesarr, 0);
                        break;
                    case "OP020A1_1":
                        r = AnalOP020A1Data(bytesarr, 0);
                        break;
                    case "OP020A1_3":
                        r = AnalOP020A_3Data(bytesarr, 0);
                        break;
                    case "OP020A2_1":
                        r = AnalOP020A1Data(bytesarr, 0);
                        break;
                    case "OP020A2_3":
                        r = AnalOP020A_3Data(bytesarr, 0);
                        break;
                    case "OP040A_1":
                        r = AnalOP40InsulationData(bytesarr, 0);
                        break;
                    case "OP040A_2":
                        r = AnalOP40InsulationData(bytesarr, 0);
                        break;
                    case "OP040A_3":
                        r = AnalOP40OvenData(bytesarr, 0);
                        break;
                    case "OP040A_4":
                        r = AnalOP40OvenData(bytesarr, 0);
                        break;
                    case "OP050A":
                        r = AnalOP050Data(bytesarr);
                        break;
                    case "OP060A":
                        r = AnalyLaserClear(bytesarr);
                        break;
                    case "OP070A":
                        r = CheckPartDataComplete(bytesarr);
                        break;
                    case "OP080A":
                        r = AnalyWeldData(bytesarr);
                        break;
                    case "OP090A":
                        r = AnalyImageData(bytesarr);
                        break;
                    case "OP100A_1":
                        r = AnalyScrewData(bytesarr);
                        break;
                    case "OP100A_2":
                        r = CheckPartDataComplete(bytesarr);
                        break;
                    case "OP110A":
                        r = AnalyWeldData(bytesarr);
                        break;
                    case "OP130A_1":
                        r = CheckPartDataComplete(bytesarr);
                        break;
                    case "OP130A_2":
                        r = AnalyScrewData(bytesarr);
                        break;
                    case "OP200A_1":
                        r = AnalyScrewData(bytesarr);
                        break;
                    case "OP200A_2":
                        r = AnalyScrewData(bytesarr);
                        break;
                    case "OP210A":
                        r = AnalOP210Data(bytesarr);
                        break;
                    case "OP240A":
                        r = AnalOP240Data(bytesarr);
                        break;
                    default:
                        r = 0;
                        break;
                }
                //  return r;
                if (r == 0)
                {
                    r = SavePassStationData(bytesarr);
                    return r;
                }
                else
                {
                    return r;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;

            }
        }


        public int AnalOP050Data(byte[] bytes)
        {
            string stncode = StnModel.StationCode;
            StringBuilder sb = new StringBuilder();
            string ordnum = ConvertHelper.ByteToString(bytes, 60, 40);//订单号
            string packcode = ConvertHelper.ByteToString(bytes, 100, 20);//pack物料号
            string packID = ConvertHelper.ByteToString(bytes, 120, 70);//packID
            string keticode = ConvertHelper.ByteToString(bytes, 280, 70);//下壳体二维码
            string operatorNum = ConvertHelper.ByteToString(bytes, 362, 10);//操作员号
            if (String.IsNullOrEmpty(operatorNum))
            {
                operatorNum = "自动";
            }
            int workstatus = bytes[453];//工位加工状态
            string devicecode = ConvertHelper.ByteToString(bytes, 464, 16);//设备编号，
            string deviceid = ConvertHelper.ByteToString(bytes, 480, 16);//设备id
            short taotongNum = ConvertHelper.BytesToShort(bytes, 496);//套筒号 
            short pronum = ConvertHelper.BytesToShort(bytes, 498);//程序调用号 
            List<float> datalist = new List<float>();//数据完整性检验集合
            int deleteRes = DeleteQualitiesData(packID, stncode);
            if (deleteRes == 0)
            {
                StringBuilder sqlsb = new StringBuilder();
                sqlsb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],DetectedResult,TypeGroup,PassStatus,DetectionAccessory,DeviceCode) values ");
                float value, max = 0, min = 0;
                int j = 1;
                short tempres;
                float[,] gluedatamat = new float[4, 1];
                int tempindex = 0;
                for (int i = 518; i <= 544;)//解析左右溢胶量
                {
                    tempres = ConvertHelper.BytesToShort(bytes, i + 24);//检测结果
                    value = ConvertHelper.ByteArrtoFolat(bytes, i);//溢胶最大
                    max= ConvertHelper.ByteArrtoFolat(bytes, i + 4);//最大限定值
                    min = ConvertHelper.ByteArrtoFolat(bytes, i + 8);//最小限定值
                    sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','12','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "mm", deviceid, pronum, tempres,workstatus,taotongNum,devicecode);
                    datalist.Add(value);
                    gluedatamat[tempindex, 0] = value;
                    value = ConvertHelper.ByteArrtoFolat(bytes, i + 12);//溢胶最小
                    max = ConvertHelper.ByteArrtoFolat(bytes, i + 16);//最大限定值
                    min = ConvertHelper.ByteArrtoFolat(bytes, i + 20);//最小限定值
                    sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','12','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "mm", deviceid, pronum, tempres,workstatus, taotongNum, devicecode);
                    datalist.Add(value);
                    gluedatamat[tempindex + 1, 0] = value;
                    i += 26;
                    j++;
                    tempindex += 2;
                }
                #region 数据校验
                if (bytes[453] == 1)
                {
                    int partValid = CheckPartDataComplete(packID, stncode, packcode);
                    if (partValid != 0)
                    {
                        return partValid;
                    }
                    if (CheckDataIntegrity(datalist))
                    {
                        bool r = CheckDataVaid(stncode, "胶深检测", packcode, gluedatamat);
                        if (r == false)
                        {
                            return 14;
                        }
                    }
                    else
                    {
                        return 15;
                    }
                }
                #endregion
                string sqlstr = sqlsb.ToString().Substring(0, sqlsb.Length - 1);
                int res = sqlHelper.ExecNonQuery(sqlstr);
                if (res > 0)
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
                return deleteRes;
            }

        }

        /// <summary>
        /// 解析OP010数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startAdd">拧紧数据开始</param>
        /// <param name="datacount"> 拧紧数据数量</param>
        public int AnalOP10Data(byte[] bytes, int startAdd)
        {
            #region 基础信息
            int start = Convert.ToInt32(StnModel.DataAddress);
            int end = Convert.ToInt32(StnModel.DataLength);
            string stncode = StnModel.StationCode;
            StringBuilder sb = new StringBuilder();
            string ordnum = ConvertHelper.ByteToString(bytes, 60, 40);//订单号
            string partNo = ConvertHelper.ByteToString(bytes, 100, 20);//pack物料号
            string packID = ConvertHelper.ByteToString(bytes, 120, 70);//packID
            string mzcode = ConvertHelper.ByteToString(bytes, 190, 20);//模组物料号
            string mzsncode = ConvertHelper.ByteToString(bytes, 210, 70);//模组序列号
            string keticode = ConvertHelper.ByteToString(bytes, 280, 70);//下壳体二维码
            string operatorNum = ConvertHelper.ByteToString(bytes, 362, 10);//操作员号
            if (String.IsNullOrEmpty(operatorNum))
            {
                operatorNum = "自动";
            }
            string devicecode = ConvertHelper.ByteToString(bytes, 464, 16);//设备编号，
            string deviceid = ConvertHelper.ByteToString(bytes, 480, 16);//设备id
            short  taotongNum = ConvertHelper.BytesToShort(bytes, 496);//套筒号 
            short pronum = ConvertHelper.BytesToShort(bytes,498);//程序调用号 
            int workstatus = bytes[453];//加工状态
            #endregion
            int r = UpdateKetiCode(packID, keticode);//下壳体码与packid绑定
            if (r < 0)
            {
                return 10;
            }
            #region 下壳体二维码存库
            string partNum = "";
            if (keticode.Length != 64)
            {
                return 12;
               
            }
            partNum = keticode.Substring(0, 10);
            string batchNo = keticode.Substring(16, 8);//批次信息
            string providerCode = keticode.Substring(10, 6);
            DataTable prodRecord = GetProdRecordTb(packID);
            StringBuilder sb2 = new StringBuilder();
            sb2.AppendFormat("insert into [tblComponentPartData] (ProductID,WorkOrderID,ComponentPartNo,ComponentPartBarcode,OperationTime,StationCode,[Description],UserNo,AllocNum,ComponentPartOnly,BatchNo,ProviderCode) values ");
            int prodid = -1, orderid = -1;
            if (prodRecord.Rows.Count > 0)
            {
                prodid = Convert.ToInt32(prodRecord.Rows[0]["ProductID"]);
                orderid = Convert.ToInt32(prodRecord.Rows[0]["WorkOrderID"]);
            }
            else
            {
                return 28;
            }
            sb2.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','下壳体','{6}','{7}','{8}','{9}','{10}'),", prodid, orderid, partNum, keticode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), "OP010A", operatorNum,"1","1", batchNo, providerCode);
            string str = sb2.ToString().Substring(0, sb2.Length - 1);
           
            #endregion

            #region 解析质量数据,客户要求只保留一次，先删除，在插入
            StringBuilder sqlsb = new StringBuilder();
            sqlsb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],DetectedResult,TypeGroup,PassStatus,DeviceCode,DetectionAccessory) values ");
            float value, max, min;
            int j = 1;
            short tempres;
            float[,] datamat = new float[1, 3];//有效性检验矩阵
            List<float> datalist = new List<float>();// 完整性检验集合
            for (int i = start; i <= end;)
            {
                tempres = ConvertHelper.BytesToShort(bytes, i + 40);
                value = ConvertHelper.ByteArrtoFolat(bytes, i);//功率
                datamat[0, 0] = value;
                datalist.Add(value);
                max = ConvertHelper.ByteArrtoFolat(bytes, i + 4);
                min = ConvertHelper.ByteArrtoFolat(bytes, i + 8);
                sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordnum, operatorNum, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), j, value, max, min, "W", deviceid, pronum, tempres, workstatus, devicecode, taotongNum);

                value = ConvertHelper.ByteArrtoFolat(bytes, i + 12);//保护气流量
                datamat[0, 1] = value;
                datalist.Add(value);
                max = ConvertHelper.ByteArrtoFolat(bytes, i + 16);
                min = ConvertHelper.ByteArrtoFolat(bytes, i + 20);
                sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordnum, operatorNum, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), j, value, max, min, "L/min", deviceid, pronum, tempres, workstatus, devicecode, taotongNum);

                value = ConvertHelper.ByteArrtoFolat(bytes, i + 24);//清洗头转速
                datamat[0, 2] = value;
                datalist.Add(value);
                max = ConvertHelper.ByteArrtoFolat(bytes, i + 28);
                min = ConvertHelper.ByteArrtoFolat(bytes, i + 32);
                sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordnum, operatorNum, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), j, value, max, min, "r/min", deviceid, pronum, tempres, workstatus, devicecode, taotongNum);

                value = ConvertHelper.ByteArrtoFolat(bytes, i + 36);//占空比
                datalist.Add(value);
                max = 0;
                min = 0;
                sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordnum, operatorNum, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), j, value, max, min, "", deviceid, pronum, tempres, workstatus, devicecode, taotongNum);
                j++;
                i += 42;
            }
            #region 数据校验
            if (bytes[453] == 1)//  PLC 给过来的加工状态 ，0未加工，1合格，2ng
            {
                
                if (CheckDataIntegrity(datalist))
                {
                    bool checkres = CheckDataVaid(stncode, "等离子清洗", partNo, datamat);
                    if (checkres == false)
                    {
                        return 14;
                    }
                }
                else
                {
                    return 15;
                }
            }
            #endregion
            int deleteRes = DeleteQualitiesData(packID, stncode);
            if (deleteRes == 0)
            {
                int r1 = sqlHelper.ExecNonQuery(str);//物料存库
                if (r1 <= 0)
                {
                    return 10;
                }
                int partValid = CheckPartDataComplete(packID, stncode, partNo);//校验
                if (partValid != 0)
                {
                    return partValid;
                }
                string sqlstr = sqlsb.ToString().Substring(0, sqlsb.Length - 1);//质量数据存库
                int res = sqlHelper.ExecNonQuery(sqlstr);
                if (res > 0)
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
                return deleteRes;
            }
            #endregion
        }
        public int AnalOP20Data(byte[] bytes, int startAdd)
        {
            try
            {
                List<string> sqllist = new List<string>();
                #region 基础信息
                string stncode = StnModel.StationCode;
                string ordnum = ConvertHelper.ByteToString(bytes, 60, 40);//订单号
                string partno = ConvertHelper.ByteToString(bytes, 100, 20);//pack总成物料号
                string packID = ConvertHelper.ByteToString(bytes, 120, 70);//packID
                string mzcode = ConvertHelper.ByteToString(bytes, 190, 20);//模组物料号
                string mzsncode = ConvertHelper.ByteToString(bytes, 210, 70);//模组序列号
                string keticode = ConvertHelper.ByteToString(bytes, 280, 70);//下壳体二维码
                string operatorNum = ConvertHelper.ByteToString(bytes, 362, 10);//操作员号 
                if (String.IsNullOrEmpty(operatorNum))
                {
                    operatorNum = "自动";
                }
                int workstatus = bytes[453];//工位加工状态
                #endregion
                #region 存储电芯二维码
                DataTable prodRecord = GetProdRecordTb(packID);
                StringBuilder sb2 = new StringBuilder();
                sb2.AppendFormat("insert into [tblComponentPartData] (ProductID,WorkOrderID,ComponentPartNo,ComponentPartBarcode,[AllocNum],[UserNo],OperationTime,StationCode,[Description]) values ");
                int prodid = -1, orderid = -1;
                if (prodRecord.Rows.Count > 0)
                {
                    prodid = Convert.ToInt32(prodRecord.Rows[0]["ProductID"]);
                    orderid = Convert.ToInt32(prodRecord.Rows[0]["WorkOrderID"]);
                }
                else
                {
                    return 21;//未获取到生产记录
                }
                for (int i = 500; i <= 1460;)
                {
                    string partcode = ConvertHelper.ByteToString(bytes, i, 80);
                    if (partcode.Length<24)
                    {
                        return 23;
                    }
                    string partNo = "7020002766"; //partcode.Substring(0, 10);
                    sb2.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','电芯'),", prodid, orderid, partNo, partcode, 1, operatorNum, DateTime.Now, StnModel.StationCode);
                    i = i + 80;
                }
                string str = sb2.ToString().Substring(0, sb2.Length - 1);
                sqllist.Add(str);
                
                #endregion
                int deleteRes = DeleteQualitiesData(packID, stncode);
                if (deleteRes == 0)
                {
                    StringBuilder datasb = new StringBuilder();
                    datasb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],DetectedResult,TypeGroup,PassStatus,DetectionAccessory,DeviceCode) values ");
                    string devicecode;
                    string deviceid;
                    float value, max, min;
                    short datares;
                    short pronum;//程序号
                    short taotongNum;//套筒号
                    List<float> datalist = new List<float>();//数据完整性校验集合
                    float[,] datamatpress = new float[1, 2];//成组电芯压装校验矩阵
                    float[,] datamatplasma = new float[5, 3];//等离子清洗校验矩阵
                    float[,] datamatglue = new float[1, 3];//涂胶数据校验矩阵
                    float[,] yldatas = new float[1, 2];//入壳压力数据校验矩阵
                    #region 成组电芯压装, 类型16
                    int j = 1;
                    devicecode = ConvertHelper.ByteToString(bytes, 1540, 16);//设备编号
                    deviceid = ConvertHelper.ByteToString(bytes, 1556, 16);//设备id
                    taotongNum = ConvertHelper.BytesToShort(bytes, 1572);//套筒选择
                    pronum = ConvertHelper.BytesToShort(bytes, 1574);//程序调用号
                    datares = ConvertHelper.BytesToShort(bytes, 1600);//结果
                    value = ConvertHelper.ByteArrtoFolat(bytes, 1576);//压力
                    max= ConvertHelper.ByteArrtoFolat(bytes, 1580);//压力最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 1584);//压力最小
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','16','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "N", deviceid, pronum, datares, workstatus,taotongNum,devicecode);
                    datalist.Add(value);
                    datamatpress[0, 0] = value;
                    value = ConvertHelper.ByteArrtoFolat(bytes, 1588);//电芯高度值
                    max = ConvertHelper.ByteArrtoFolat(bytes, 1592);//电芯高度值最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 1596);//电芯高度值最小
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','16','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "mm", deviceid, pronum, datares, workstatus, taotongNum, devicecode);
                    datalist.Add(value);
                    datamatpress[0, 1] = value;
                    #endregion
                    j = 2;
                    #region 等离子清洗数据  类型是4
                    int index = 0;
                    devicecode = ConvertHelper.ByteToString(bytes, 1602, 16);//等离子设备编号
                    deviceid = ConvertHelper.ByteToString(bytes, 1618, 16);//等离子设备id
                    taotongNum = ConvertHelper.BytesToShort(bytes, 1634);//套筒选择
                    pronum = ConvertHelper.BytesToShort(bytes, 1636);//程序调用号
                    for (int i = 1638; i <= 1806;)
                    {
                        datares = ConvertHelper.BytesToShort(bytes, i + 40);//结果
                        value = ConvertHelper.ByteArrtoFolat(bytes, i);//功率
                        datalist.Add(value);
                        datamatplasma[index, 0] = value;
                        max = ConvertHelper.ByteArrtoFolat(bytes, i +4);//功率最大
                        min = ConvertHelper.ByteArrtoFolat(bytes, i +8);//功率最小
                        datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "W", deviceid, pronum, datares, workstatus, taotongNum, devicecode);

                        value = ConvertHelper.ByteArrtoFolat(bytes, i + 12);//气流量
                        datamatplasma[index, 1] = value;
                        datalist.Add(value);
                        max = ConvertHelper.ByteArrtoFolat(bytes, i + 16);//气流量最大
                        min = ConvertHelper.ByteArrtoFolat(bytes, i + 20);//气流量最小
                        datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "L/min", deviceid, pronum, datares, workstatus, taotongNum, devicecode);

                        value = ConvertHelper.ByteArrtoFolat(bytes, i + 24);//转速
                        datamatplasma[index, 2] = value;
                        datalist.Add(value);
                        max = ConvertHelper.ByteArrtoFolat(bytes, i + 28);//转速最大
                        min = ConvertHelper.ByteArrtoFolat(bytes, i + 32);//转速最小
                        datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "r/min", deviceid, pronum, datares,workstatus, taotongNum, devicecode);

                        value = ConvertHelper.ByteArrtoFolat(bytes, i + 36);//占空比
                        datalist.Add(value);
                        datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, 0, 0, "", deviceid, pronum, datares,workstatus, taotongNum, devicecode);
                        j++;
                        i = i + 42;
                        index++;
                    }

                    #endregion
                    #region 涂胶数据   类型是7
                    devicecode = ConvertHelper.ByteToString(bytes, 1890, 16);//设备编号
                    deviceid = ConvertHelper.ByteToString(bytes, 1906, 16);//设备id
                    taotongNum = ConvertHelper.BytesToShort(bytes, 1922);//套筒选择
                    pronum = ConvertHelper.BytesToShort(bytes, 1924);//程序调用号
                    datares = ConvertHelper.BytesToShort(bytes, 1998);//涂胶结果
                    //龙泽宇说总胶量不要
                    //value = ConvertHelper.ByteArrtoFolat(bytes, 1402);//总胶量
                    //datalist.Add(value);
                    //max = ConvertHelper.ByteArrtoFolat(bytes, 1406);//总胶量最大
                    //min = ConvertHelper.ByteArrtoFolat(bytes, 1410);//总胶量最小
                    //datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','7'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "cc", deviceid, pronum, datares);

                    value = ConvertHelper.ByteArrtoFolat(bytes, 1938);//涂胶速度
                    datalist.Add(value);
                    datamatglue[0, 0] = value;
                    max = ConvertHelper.ByteArrtoFolat(bytes, 1942);//速度最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 1946);//速度最小
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','7','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "cc/s", deviceid, pronum, datares,workstatus, taotongNum, devicecode);

                    value = ConvertHelper.ByteArrtoFolat(bytes, 1962);//A胶涂胶速度
                    datalist.Add(value);
                    datamatglue[0, 1] = value;
                    max = ConvertHelper.ByteArrtoFolat(bytes, 1966);//A胶涂胶速度最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 1970);//A胶涂胶速度最小
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','7','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "cc/s", deviceid, pronum, datares,workstatus, taotongNum, devicecode);

                    value = ConvertHelper.ByteArrtoFolat(bytes, 1986);//B胶涂胶速度
                    datalist.Add(value);
                    datamatglue[0, 2] = value;
                    max = ConvertHelper.ByteArrtoFolat(bytes, 1990);//b胶量最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 1994);//b胶量最小
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','7','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "cc/s", deviceid, pronum, datares,workstatus, taotongNum, devicecode);
                    #endregion
                    j += 1;
                    #region 成组电芯入壳压力  类型是6
                    devicecode = ConvertHelper.ByteToString(bytes, 2000, 16);//设备编号
                    deviceid = ConvertHelper.ByteToString(bytes, 2016, 16);//设备id
                    taotongNum = ConvertHelper.BytesToShort(bytes, 2032);//套筒选择
                    pronum = ConvertHelper.BytesToShort(bytes, 2034);//程序调用号
                    datares = ConvertHelper.BytesToShort(bytes, 2060);
                    value = ConvertHelper.ByteArrtoFolat(bytes, 2036);//压力
                    max = ConvertHelper.ByteArrtoFolat(bytes, 2040);//压力最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 2044);//压力最小
                    yldatas[0, 0] = value;
                    datalist.Add(value);
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','6','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max,min, "N", deviceid, pronum, datares,workstatus, taotongNum, devicecode);

                    value = ConvertHelper.ByteArrtoFolat(bytes, 2048);//位移
                    max = ConvertHelper.ByteArrtoFolat(bytes, 2052);//位移最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 2056);//位移最小
                    yldatas[0, 1] = value;
                    datalist.Add(value);
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','6','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "mm", deviceid, pronum, datares,workstatus, taotongNum, devicecode);
                    #endregion
                    #region 数据检验
                    if (bytes[453] == 1)//  PLC 给过来的加工状态 ，0未加工，1合格，2ng
                    {
                        int partValid = CheckPartDataComplete(packID, stncode,partno);
                        if (partValid != 0)
                        {
                            return partValid;
                        }
                        if (CheckDataIntegrity(datalist))//数据完整性校验
                        {
                            bool r1 = CheckDataVaid(stncode, "电芯压装", partno, datamatpress);
                            bool r2 = CheckDataVaid(stncode, "等离子清洗", partno, datamatplasma);
                            bool r3 = CheckDataVaid(stncode, "涂胶", partno, datamatglue);
                            bool r4 = CheckDataVaid(stncode, "入壳压装", partno, yldatas);
                            if ((r1 && r2 && r3 && r4))
                            {

                            }
                            else
                            {
                                return 14;
                            }
                        }
                        else
                        {
                            return 15;
                        }
                    }
                    #endregion
                    #region 数据写库
                    string sqlstr = datasb.ToString().Substring(0, datasb.Length - 1);
                    sqllist.Add(sqlstr);
                    int res = sqlHelper.ExecNonQuery(sqllist);
                    if (res < 0)
                    {
                        return 10;
                    }
                    else
                    {
                        return 0;
                    }
                    #endregion
                }
                else
                {
                    return deleteRes;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;
            }
        }


        /// <summary>
        /// OP020 第三分度数据解析
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startAdd"></param>
        /// <returns></returns>
        public int AnalOP020A_3Data(byte[] bytes, int startAdd)
        {
            try
            {
                List<string> sqllist = new List<string>();
                #region 基础信息
                string stncode = StnModel.StationCode;
                string ordnum = ConvertHelper.ByteToString(bytes, 60, 40);//订单号
                string partno = ConvertHelper.ByteToString(bytes, 100, 20);//pack总成物料号
                string packID = ConvertHelper.ByteToString(bytes, 120, 70);//packID
                string mzcode = ConvertHelper.ByteToString(bytes, 190, 20);//模组物料号
                string mzsncode = ConvertHelper.ByteToString(bytes, 210, 70);//模组序列号
                string keticode = ConvertHelper.ByteToString(bytes, 280, 70);//下壳体二维码
                string operatorNum = ConvertHelper.ByteToString(bytes, 362, 10);//操作员号 
                if (String.IsNullOrEmpty(operatorNum))
                {
                    operatorNum = "自动";
                }
                int workstatus = bytes[453];//工位加工状态
                #endregion
             
                int deleteRes = DeleteQualitiesData(packID, stncode);
                if (deleteRes == 0)
                {
                    StringBuilder datasb = new StringBuilder();
                    datasb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],DetectedResult,TypeGroup,PassStatus,DetectionAccessory,DeviceCode) values ");
                    string devicecode;
                    string deviceid;
                    float value, max, min;
                    short datares;
                    short pronum;//程序号
                    short taotongNum;//套筒号
                    List<float> datalist = new List<float>();//数据完整性校验集合
                    float[,] datamatpress = new float[1, 2];//成组电芯压装校验矩阵
                    float[,] datamatplasma = new float[5, 3];//等离子清洗校验矩阵
                    float[,] yldatas = new float[1, 2];//入壳压力数据校验矩阵
                    #region 成组电芯压装, 类型16
                    int j = 1;
                    devicecode = ConvertHelper.ByteToString(bytes, 1540, 16);//设备编号
                    deviceid = ConvertHelper.ByteToString(bytes, 1556, 16);//设备id
                    taotongNum = ConvertHelper.BytesToShort(bytes, 1572);//套筒选择
                    pronum = ConvertHelper.BytesToShort(bytes, 1574);//程序调用号
                    datares = ConvertHelper.BytesToShort(bytes, 1600);//结果
                    value = ConvertHelper.ByteArrtoFolat(bytes, 1576);//压力
                    max = ConvertHelper.ByteArrtoFolat(bytes, 1580);//压力最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 1584);//压力最小
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','16','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "N", deviceid, pronum, datares, workstatus, taotongNum, devicecode);
                    datalist.Add(value);
                    datamatpress[0, 0] = value;
                    value = ConvertHelper.ByteArrtoFolat(bytes, 1588);//电芯高度值
                    max = ConvertHelper.ByteArrtoFolat(bytes, 1592);//电芯高度值最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 1596);//电芯高度值最小
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','16','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "mm", deviceid, pronum, datares, workstatus, taotongNum, devicecode);
                    datalist.Add(value);
                    datamatpress[0, 1] = value;
                    #endregion
                    j = 2;
                    #region 等离子清洗数据  类型是4
                    int index = 0;
                    devicecode = ConvertHelper.ByteToString(bytes, 1602, 16);//等离子设备编号
                    deviceid = ConvertHelper.ByteToString(bytes, 1618, 16);//等离子设备id
                    taotongNum = ConvertHelper.BytesToShort(bytes, 1634);//套筒选择
                    pronum = ConvertHelper.BytesToShort(bytes, 1636);//程序调用号
                    for (int i = 1638; i <= 1806;)
                    {
                        datares = ConvertHelper.BytesToShort(bytes, i + 40);//结果
                        value = ConvertHelper.ByteArrtoFolat(bytes, i);//功率
                        datalist.Add(value);
                        datamatplasma[index, 0] = value;
                        max = ConvertHelper.ByteArrtoFolat(bytes, i + 4);//功率最大
                        min = ConvertHelper.ByteArrtoFolat(bytes, i + 8);//功率最小
                        datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "W", deviceid, pronum, datares, workstatus, taotongNum, devicecode);

                        value = ConvertHelper.ByteArrtoFolat(bytes, i + 12);//气流量
                        datamatplasma[index, 1] = value;
                        datalist.Add(value);
                        max = ConvertHelper.ByteArrtoFolat(bytes, i + 16);//气流量最大
                        min = ConvertHelper.ByteArrtoFolat(bytes, i + 20);//气流量最小
                        datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "L/min", deviceid, pronum, datares, workstatus, taotongNum, devicecode);

                        value = ConvertHelper.ByteArrtoFolat(bytes, i + 24);//转速
                        datamatplasma[index, 2] = value;
                        datalist.Add(value);
                        max = ConvertHelper.ByteArrtoFolat(bytes, i + 28);//转速最大
                        min = ConvertHelper.ByteArrtoFolat(bytes, i + 32);//转速最小
                        datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "r/min", deviceid, pronum, datares, workstatus, taotongNum, devicecode);

                        value = ConvertHelper.ByteArrtoFolat(bytes, i + 36);//占空比
                        datalist.Add(value);
                        datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','4','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, 0, 0, "", deviceid, pronum, datares, workstatus, taotongNum, devicecode);
                        j++;
                        i = i + 42;
                        index++;
                    }

                    #endregion
             
                    #region 成组电芯入壳压力  类型是6
                    devicecode = ConvertHelper.ByteToString(bytes, 1890, 16);//设备编号
                    deviceid = ConvertHelper.ByteToString(bytes, 1906, 16);//设备id
                    taotongNum = ConvertHelper.BytesToShort(bytes, 1922);//套筒选择
                    pronum = ConvertHelper.BytesToShort(bytes, 1924);//程序调用号
                    datares = ConvertHelper.BytesToShort(bytes, 1950);//入壳压装结果
                    value = ConvertHelper.ByteArrtoFolat(bytes, 1926);//压力
                    max = ConvertHelper.ByteArrtoFolat(bytes, 1930);//压力最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 1934);//压力最小
                    yldatas[0, 0] = value;
                    datalist.Add(value);
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','6','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "N", deviceid, pronum, datares, workstatus, taotongNum, devicecode);

                    value = ConvertHelper.ByteArrtoFolat(bytes, 1938);//位移
                    max = ConvertHelper.ByteArrtoFolat(bytes, 1942);//位移最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 1946);//位移最小
                    yldatas[0, 1] = value;
                    datalist.Add(value);
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','6','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "mm", deviceid, pronum, datares, workstatus, taotongNum, devicecode);
                    #endregion
                    #region 数据检验
                    if (bytes[453] == 1)//  PLC 给过来的加工状态 ，0未加工，1合格，2ng
                    {
                      
                        if (CheckDataIntegrity(datalist))//数据完整性校验
                        {
                            bool r1 = CheckDataVaid(stncode, "电芯压装", partno, datamatpress);
                            bool r2 = CheckDataVaid(stncode, "等离子清洗", partno, datamatplasma);
                            bool r4 = CheckDataVaid(stncode, "入壳压装", partno, yldatas);
                            if ((r1 && r2 &&  r4))
                            {
                             
                            }
                            else
                            {
                                return 14;
                            }
                        }
                        else
                        {
                            return 15;
                        }
                    }
                    #endregion
                    #region 存储电芯二维码
                    DataTable prodRecord = GetProdRecordTb(packID);
                    StringBuilder sb2 = new StringBuilder();
                    List<string> dxCodeList = new List<string>();
                    sb2.AppendFormat("insert into [tblComponentPartData] (ProductID,WorkOrderID,ComponentPartNo,ComponentPartBarcode,[AllocNum],[UserNo],OperationTime,StationCode,[Description]) values ");
                    int prodid = -1, orderid = -1;
                    if (prodRecord.Rows.Count > 0)
                    {
                        prodid = Convert.ToInt32(prodRecord.Rows[0]["ProductID"]);
                        orderid = Convert.ToInt32(prodRecord.Rows[0]["WorkOrderID"]);
                    }
                    else
                    {
                        return 21;//未获取到生产记录
                    }
                    for (int i = 500; i <= 1460;)
                    {
                        string partcode = ConvertHelper.ByteToString(bytes, i, 80);
                        if (partcode.Length != 24)
                        {
                            return 23;
                        }
                        dxCodeList.Add(partcode);
                        string partNo = "7020002766"; //partcode.Substring(0, 10);
                        sb2.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','电芯'),", prodid, orderid, partNo, partcode, 1, operatorNum, DateTime.Now, StnModel.StationCode);
                        i = i + 80;
                    }
                   
                    string str = sb2.ToString().Substring(0, sb2.Length - 1);
                    int tempRes = sqlHelper.ExecNonQuery(str);
                    if (tempRes <= 0)
                    {
                        return 10;
                    }
                    else
                    {
                        //校验物料完整性
                        int partValid = CheckPartDataComplete(packID, stncode, partno);
                        if (partValid != 0)
                        {
                            return partValid;
                        }
                        string dclistStr = string.Join(",", dxCodeList);
                        string tempSqlstr = "exec SP_InsertDXPartData '" + dclistStr + "'";
                        int  tempr= sqlHelper.ExecNonQuery(tempSqlstr);
                    }
                    #endregion
                    #region 数据写库
                    string sqlstr = datasb.ToString().Substring(0, datasb.Length - 1);
                    sqllist.Add(sqlstr);
                    int res = sqlHelper.ExecNonQuery(sqllist);
                    if (res < 0)
                    {
                        return 10;
                    }
                    else
                    {
                        return 0;
                    }
                    #endregion
                }
                else
                {
                    return deleteRes;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;
            }
        }


        /// <summary>
        /// OP020 第一分度数据解析
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startAdd"></param>
        /// <returns></returns>
        public  int AnalOP020A1Data(byte[] bytes, int startAdd)
        {
            try
            {
                List<string> sqllist = new List<string>();
                #region 基础信息
                string stncode = StnModel.StationCode;
                string ordnum = ConvertHelper.ByteToString(bytes, 60, 40);//订单号
                string partno = ConvertHelper.ByteToString(bytes, 100, 20);//pack总成物料号
                string packID = ConvertHelper.ByteToString(bytes, 120, 70);//packID
                string mzcode = ConvertHelper.ByteToString(bytes, 190, 20);//模组物料号
                string mzsncode = ConvertHelper.ByteToString(bytes, 210, 70);//模组序列号
                string keticode = ConvertHelper.ByteToString(bytes, 280, 70);//下壳体二维码
                string operatorNum = ConvertHelper.ByteToString(bytes, 362, 10);//操作员号 
                if (String.IsNullOrEmpty(operatorNum))
                {
                    operatorNum = "自动";
                }
                int workstatus = bytes[453];//工位加工状态
                #endregion
                int deleteRes = DeleteQualitiesData(packID, stncode);
                if (deleteRes == 0)
                {
                    StringBuilder datasb = new StringBuilder();
                    datasb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],DetectedResult,TypeGroup,PassStatus,DetectionAccessory,DeviceCode) values ");
                    string devicecode;
                    string deviceid;
                    float value, max, min;
                    short datares;
                    short pronum;//程序号
                    short taotongNum;//套筒号
                    List<float> datalist = new List<float>();//数据完整性校验集合
                    float[,] datamatglue = new float[1, 3];//涂胶数据校验矩阵
                    #region 涂胶数据   类型是7
                    devicecode = ConvertHelper.ByteToString(bytes, 464, 16);//设备编号
                    deviceid = ConvertHelper.ByteToString(bytes, 480, 16);//设备id
                    taotongNum = ConvertHelper.BytesToShort(bytes, 496);//套筒选择
                    pronum = ConvertHelper.BytesToShort(bytes, 498);//程序调用号
                    datares = ConvertHelper.BytesToShort(bytes, 572);//涂胶结果
                    //龙泽宇说总胶量不要
                    //value = ConvertHelper.ByteArrtoFolat(bytes, 1402);//总胶量
                    //datalist.Add(value);
                    //max = ConvertHelper.ByteArrtoFolat(bytes, 1406);//总胶量最大
                    //min = ConvertHelper.ByteArrtoFolat(bytes, 1410);//总胶量最小
                    //datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','7'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "cc", deviceid, pronum, datares);
                    int j = 1;
                    value = ConvertHelper.ByteArrtoFolat(bytes, 512);//涂胶速度
                    datalist.Add(value);
                    datamatglue[0, 0] = value;
                    max = ConvertHelper.ByteArrtoFolat(bytes, 516);//速度最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 520);//速度最小
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','7','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "cc/s", deviceid, pronum, datares, workstatus, taotongNum, devicecode);

                    value = ConvertHelper.ByteArrtoFolat(bytes, 536);//A胶涂胶速度
                    datalist.Add(value);
                    datamatglue[0, 1] = value;
                    max = ConvertHelper.ByteArrtoFolat(bytes, 540);//A胶涂胶速度最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 544);//A胶涂胶速度最小
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','7','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "cc/s", deviceid, pronum, datares, workstatus, taotongNum, devicecode);

                    value = ConvertHelper.ByteArrtoFolat(bytes, 560);//B胶涂胶速度
                    datalist.Add(value);
                    datamatglue[0, 2] = value;
                    max = ConvertHelper.ByteArrtoFolat(bytes, 564);//b胶量最大
                    min = ConvertHelper.ByteArrtoFolat(bytes, 568);//b胶量最小
                    datasb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','7','{13}','{14}','{15}'),", packID, stncode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "cc/s", deviceid, pronum, datares, workstatus, taotongNum, devicecode);
                    #endregion
                    j += 1;
                 
                    #region 数据检验
                    if (bytes[453] == 1)//  PLC 给过来的加工状态 ，0未加工，1合格，2ng
                    {
                        int partValid = CheckPartDataComplete(packID, stncode, partno);
                        if (partValid != 0)
                        {
                            return partValid;
                        }
                        if (CheckDataIntegrity(datalist))//数据完整性校验d
                        {
                            bool r3 = CheckDataVaid(stncode, "涂胶", partno, datamatglue);
                            if (r3)
                            {
                            }
                            else
                            {
                                return 14;
                            }
                        }
                        else
                        {
                            return 15;
                        }
                    }
                    #endregion
                    #region 数据写库
                    string sqlstr = datasb.ToString().Substring(0, datasb.Length - 1);
                    sqllist.Add(sqlstr);
                    int res = sqlHelper.ExecNonQuery(sqllist);
                    if (res < 0)
                    {
                        return 10;
                    }
                    else
                    {
                        return 0;
                    }
                    #endregion
                }
                else
                {
                    return deleteRes;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;
            }
        }


        /// <summary>
        /// 解析OP210气密性测试数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public int AnalOP210Data(byte[] bytes)
        {
            try
            {
                #region 基础信息
                int start = Convert.ToInt32(StnModel.DataAddress);
                int end = Convert.ToInt32(StnModel.DataLength);
                string stncode = StnModel.StationCode;
                StringBuilder sb = new StringBuilder();
                string ordnum = ConvertHelper.ByteToString(bytes, 60, 40);//订单号
                string packcode = ConvertHelper.ByteToString(bytes, 100, 20);//pack物料号
                string packID = ConvertHelper.ByteToString(bytes, 120, 70);//packID
                string keticode = ConvertHelper.ByteToString(bytes, 280, 70);//下壳体二维码
                string operatorNum = ConvertHelper.ByteToString(bytes, 362, 10);//操作员号
                if (String.IsNullOrEmpty(operatorNum))
                {
                    operatorNum = "自动";
                }
                int workstatus = bytes[453];
                string devicecode = ConvertHelper.ByteToString(bytes, 464, 16);//设备编号
                string deviceid = ConvertHelper.ByteToString(bytes, 480, 16);//设备id
                short taotongNum = ConvertHelper.BytesToShort(bytes, 496);//程序调用号 
                short pronum = ConvertHelper.BytesToShort(bytes,498);//程序调用号 
                #endregion
                List<float> datalist = new List<float>();//完整性检验集合
                float[,] datamat = new float[1, 2];//有效性校验数据矩阵

                #region 解析质量数据
                int deleteRes = DeleteQualitiesData(packID, StnModel.StationCode);
                if (deleteRes == 0)
                {
                    StringBuilder sqlsb = new StringBuilder();
                    sqlsb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],DetectedResult,TypeGroup,PassStatus,DetectionAccessory,DeviceCode) values ");
                    float value, max, min;
                    int j = 1;
                    short tempres;
                    tempres = ConvertHelper.BytesToShort(bytes, 524);
                    value = ConvertHelper.ByteArrtoFolat(bytes, 500);
                    max = ConvertHelper.ByteArrtoFolat(bytes, 504);
                    min = ConvertHelper.ByteArrtoFolat(bytes, 508);
                    datalist.Add(value);
                    datamat[0, 0] = value;
                    sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','2','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "P", deviceid, pronum, tempres,workstatus,taotongNum,devicecode);
                    value = ConvertHelper.ByteArrtoFolat(bytes, 512);
                    max = ConvertHelper.ByteArrtoFolat(bytes, 516);
                    min = ConvertHelper.ByteArrtoFolat(bytes, 520);
                    datalist.Add(value);
                    datamat[0, 1] = value;
                    sqlsb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','2','{13}','{14}','{15}'),", packID, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "L/min", deviceid, pronum, tempres,workstatus, taotongNum, devicecode);
                    #region 数据校验
                    if (bytes[453] == 1)
                    {
                        int partValid = CheckPartDataComplete(packID, stncode, packcode);
                        if (partValid != 0)
                        {
                            return partValid;
                        }
                        if (CheckDataIntegrity(datalist))
                        {
                            bool checkres = CheckDataVaid(stncode, "气密测试", packcode, datamat);
                            if (checkres == false)
                            {
                                return 14;
                            }
                        }
                        else
                        {
                            return 15;
                        }
                    }
                    #endregion

                    string sqlstr = sqlsb.ToString().Substring(0, sqlsb.Length - 1);
                    int res = sqlHelper.ExecNonQuery(sqlstr);
                    if (res > 0)
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
                    return deleteRes;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;
            }
            #endregion
        }

        public int AnalOP240Data(byte[] bytes)
        {
            #region 基础值
            string ordnum = ConvertHelper.ByteToString(bytes, 60, 40);//订单号
            string packcode = ConvertHelper.ByteToString(bytes, 100, 20);//pack物料号
            string packID = ConvertHelper.ByteToString(bytes, 120, 70);//packID
            string operatorNum = ConvertHelper.ByteToString(bytes, 362, 10);//操作员号
            if (String.IsNullOrEmpty(operatorNum))
            {
                operatorNum = "自动";
            }
            int workstatus = bytes[453];
            string devicecode = ConvertHelper.ByteToString(bytes, 472, 10);//设备编号，不存
            string deviceid = ConvertHelper.ByteToString(bytes, 482, 10);//设备id
            short taotongNum = ConvertHelper.BytesToShort(bytes, 496);//套筒选择 
            short pronum = ConvertHelper.BytesToShort(bytes,498);//程序调用号 
            List<float> datalist = new List<float>();//数据完整性检验集合
            float[,] datamat = new float[1, 1];//数据有效性校验矩阵
        
            #endregion
            int deleteRes = DeleteQualitiesData(packID, StnModel.StationCode);
            if (deleteRes == 0)
            {
                float value = ConvertHelper.ByteArrtoFolat(bytes, 500);//电池包重量
                float max = ConvertHelper.ByteArrtoFolat(bytes, 504);
                float min = ConvertHelper.ByteArrtoFolat(bytes, 508);
                short tempres = ConvertHelper.BytesToShort(bytes, 512);
                datalist.Add(value);
                datamat[0, 0] = value;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],DetectedResult,TypeGroup,PassStatus,DetectionAccessory,DeviceCode) values ");
                sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','5','{13}','{14}','{15}')", packID, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, 1, value, max, min, "g", deviceid, pronum, tempres,workstatus,taotongNum, pronum);
                string sqlstr = "update tblWorkOrder set FinishedQty=FinishedQty+1 where WorkOrderNo='" + ordnum + "'";
                string sqlstr2 = "update tblProduct set [FinishedDatetime]='" + DateTime.Now + "'where [ProductSerialNumber]='" + packID + "'";
                List<string> sqllist = new List<string>();
                sqllist.Add(sb.ToString());
                sqllist.Add(sqlstr);
                sqllist.Add(sqlstr2);
                #region 数据校验
                if (bytes[453] == 1)
                {
                    int partValid = CheckPartDataComplete(packID, StnModel.StationCode, packcode);
                    if (partValid != 0)
                    {
                        return partValid;
                    }
                    if (CheckDataIntegrity(datalist))
                    {
                        bool checkres = CheckDataVaid(StnModel.StationCode, "称重", packcode, datamat);
                        if (checkres == false)
                        {
                            return 14;
                        }
                    }
                    else
                    {
                        return 15;
                    }
                }
                #endregion
                int r = sqlHelper.ExecNonQuery(sqllist);
                if (r < 0)
                {
                    return 10;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return deleteRes;
            }
        }
        /// <summary>
        /// 解析OP040-1.OP040-2绝缘数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startAdd"></param>
      
        public int AnalOP40InsulationData(byte[] bytes, int startAdd)
        {
            try
            {
                #region 基础值
                string stncode = StnModel.StationCode;
                StringBuilder sb = new StringBuilder();
                string ordnum = ConvertHelper.ByteToString(bytes, 60, 40);//订单号
                string packcode = ConvertHelper.ByteToString(bytes, 100, 20);//pack物料号
                string packid = ConvertHelper.ByteToString(bytes, 120, 70);//packID
                string mzcode = ConvertHelper.ByteToString(bytes, 190, 20);//模组物料号
                string mzsncode = ConvertHelper.ByteToString(bytes, 210, 70);//模组序列号
                string keticode = ConvertHelper.ByteToString(bytes, 280, 70);//下壳体二维码
                string operatorNum = ConvertHelper.ByteToString(bytes, 362, 10);//操作员号
                if (String.IsNullOrEmpty(operatorNum))
                {
                    operatorNum = "自动";
                }
                int workstatus = bytes[453];//加工状态
                #endregion
                sb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],[DetectionAccessory],DetectedResult,TypeGroup,PassStatus,DeviceCode) values  ");
                float value, max, min;
                short datares;//结果
                datares = ConvertHelper.BytesToShort(bytes, 594);//绝缘性检测结果
                string devicecode = ConvertHelper.ByteToString(bytes, 464, 16);//设备编号，
                string deviceid = ConvertHelper.ByteToString(bytes, 480, 16);//设备id
                short taotongNum = ConvertHelper.BytesToShort(bytes, 496);//套筒号 
                short pronum = ConvertHelper.BytesToShort(bytes, 498);//程序调用号 
                List<float> list = new List<float>();//数据校验集合
                #region 绝缘性测试
                float[,] resdatamat = new float[1, 2];//电阻检验矩阵
                float[,] voldatamat = new float[13, 1];////电压检验矩阵

                value = ConvertHelper.ByteArrtoFolat(bytes, 504);//偶数对奇数电阻值
                max= ConvertHelper.ByteArrtoFolat(bytes, 508);//偶数对奇数电阻最大值
                min = ConvertHelper.ByteArrtoFolat(bytes, 512);//偶数对奇数电阻最小值
                list.Add(value);
                resdatamat[0, 0] = value;
                sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','9','{14}','{15}'),", packid, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, 1, value, max, min, "Ω", deviceid, pronum, taotongNum, datares,workstatus,devicecode);

                value = ConvertHelper.ByteArrtoFolat(bytes, 516);//电芯正极与外壳绝缘电阻值
                max = ConvertHelper.ByteArrtoFolat(bytes, 520);//电芯正极与外壳绝缘电阻最大值
                min = ConvertHelper.ByteArrtoFolat(bytes, 524);//电芯正极与外壳绝缘电阻最小值
                list.Add(value);
                resdatamat[0, 1] = value;
                sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','9','{14}','{15}'),", packid, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, 1, value, max, min, "Ω", deviceid, pronum, taotongNum, datares,workstatus, devicecode);
                int tempindex = 0;
                max = ConvertHelper.ByteArrtoFolat(bytes, 580);//电压最大
                min = ConvertHelper.ByteArrtoFolat(bytes, 584);//电压最小
                for (int i = 528; i <= 576;)
                {
                    value = ConvertHelper.ByteArrtoFolat(bytes, i);
                    list.Add(value);
                    voldatamat[tempindex, 0] = value;
                    sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','9','{14}','{15}'),", packid, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, 1, value, max, min, "V", deviceid, pronum, taotongNum, datares,workstatus,devicecode);
                    i += 4;
                    tempindex++;
                }
                #endregion

                #region 数据校验
                if (bytes[453] == 1)
                {
                    int partValid = CheckPartDataComplete(packid, StnModel.StationCode, packcode);
                    if (partValid != 0)
                    {
                        return partValid;
                    }
                    if (CheckDataIntegrity(list))
                    {
                        if (CheckOP040InsulationData(packcode, resdatamat, voldatamat,StnModel.StationCode))
                        {
                            
                        }
                        else
                        {
                            return 14;
                        }
                    }
                    else
                    {
                        return 15;
                    }
                }
                #endregion

                int deleteRes = DeleteQualitiesData(packid, stncode);
                if (deleteRes != 0)
                {
                    return deleteRes;
                }
                string sqlstr = sb.ToString().Substring(0, sb.Length - 1);
                int res = sqlHelper.ExecNonQuery(sqlstr);
                if (res < 0)
                {
                    return 10;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;
            }
        }
        public int AnalOP40OvenData(byte[] bytes, int startAdd)
        {
            try
            {
                #region 基础值
                string stncode = StnModel.StationCode;
                StringBuilder sb = new StringBuilder();
                string ordnum = ConvertHelper.ByteToString(bytes, 60, 40);//订单号
                string packcode = ConvertHelper.ByteToString(bytes, 100, 20);//pack物料号
                string packid = ConvertHelper.ByteToString(bytes, 120, 70);//packID
                string mzcode = ConvertHelper.ByteToString(bytes, 190, 20);//模组物料号
                string mzsncode = ConvertHelper.ByteToString(bytes, 210, 70);//模组序列号
                string keticode = ConvertHelper.ByteToString(bytes, 280, 70);//下壳体二维码
                string operatorNum = ConvertHelper.ByteToString(bytes, 362, 10);//操作员号
                if (String.IsNullOrEmpty(operatorNum))
                {
                    operatorNum = "自动";
                }
                int workstatus = bytes[453];//加工状态
                #endregion
                sb.AppendFormat("insert into tblQualityData(SerialNumber,StationCode,WorkOrderNr,UserNo,EnterStationTime,DetectionSeq,DetectedValue,MaxValue,MinValue,[DetectionUnit],[DetectionDeviceID],[DetectionProgramName],[DetectionAccessory],DetectedResult,TypeGroup,PassStatus,DeviceCode) values  ");
                float value, max, min;
                short datares;//结果
                string devicecode = ConvertHelper.ByteToString(bytes, 464, 16);//设备编号，
                string deviceid = ConvertHelper.ByteToString(bytes, 480, 16);//设备id
                short taotongNum = ConvertHelper.BytesToShort(bytes, 496);//套筒号 
                short pronum = ConvertHelper.BytesToShort(bytes, 498);//程序调用号 
                List<float> list = new List<float>();//数据校验集合
                int j =1;
                #region 烘烤数据
                float[,] ovendatamat = new float[1, 2];
                datares = ConvertHelper.BytesToShort(bytes, 542);
                short kxnum = ConvertHelper.BytesToShort(bytes, 500);//烤箱标号
                sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','11','{14}','{15}'),", packid, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, j, kxnum, 0, 0, "", deviceid, pronum, taotongNum, datares, workstatus, devicecode);
                value = ConvertHelper.ByteArrtoFolat(bytes, 502);//烘烤温度
                max = ConvertHelper.ByteArrtoFolat(bytes, 506);
                min = ConvertHelper.ByteArrtoFolat(bytes, 510);
                list.Add(value);
                ovendatamat[0, 0] = value;
                sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','11','{14}','{15}'),", packid, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, j, value, max, min, "", deviceid, pronum, "", datares, workstatus, devicecode);
                value = ConvertHelper.ByteArrtoFolat(bytes, 514);//烘烤时间
                max = ConvertHelper.ByteArrtoFolat(bytes, 518);
                min = ConvertHelper.ByteArrtoFolat(bytes, 522);
                list.Add(value);
                ovendatamat[0, 1] = value;
                sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','11','{14}','{15}'),", packid, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, j, value, 0, 0, "", deviceid, pronum, "", datares, workstatus, devicecode);

                DateTime starttime = ConvertHelper.BytesToTime(bytes, 526);
                long startStamp = ConvertHelper.GetTimeStamp(starttime);
                sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','11','{14}','{15}'),", packid, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, j, startStamp, 0, 0, "", deviceid, pronum, "", datares, workstatus, devicecode);
                DateTime endtime = ConvertHelper.BytesToTime(bytes, 534);
                long endStamp = ConvertHelper.GetTimeStamp(endtime);
                sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','11','{14}','{15}'),", packid, StnModel.StationCode, ordnum, operatorNum, DateTime.Now, j, endStamp, 0, 0, "", deviceid, pronum, "", datares, workstatus, devicecode);

                #endregion

                #region 数据校验
                if (bytes[453] == 1)
                {
                    int partValid = CheckPartDataComplete(packid, StnModel.StationCode, packcode);
                    if (partValid != 0)
                    {
                        return partValid;
                    }
                    if (CheckDataIntegrity(list))
                    {
                        bool r2 = CheckDataVaid(StnModel.StationCode, "模组烘烤", packcode, ovendatamat);
                        if (r2)
                        {

                        }
                        else
                        {
                            return 14;
                        }
                    }
                    else
                    {
                        return 15;
                    }
                }
                #endregion

                int deleteRes = DeleteQualitiesData(packid, stncode);
                if (deleteRes != 0)
                {
                    return deleteRes;
                }
                string sqlstr = sb.ToString().Substring(0, sb.Length - 1);
                int res = sqlHelper.ExecNonQuery(sqlstr);
                if (res < 0)
                {
                    return 10;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;
            }
        }
        public int SavePassStationData(byte[] datas)
        {
            try
            {
                if (StnModel.StationCode.Contains('C'))//电芯段
                {
                    return SaveDianXinPassStnData(datas, StnModel.StationCode);
                }
                string packID = ConvertHelper.ByteToString(datas, 120, 70);//packid
                if (String.IsNullOrEmpty(packID))
                {
                    return 20;//packID  位空
                }
                short palletnum = ConvertHelper.BytesToShort(datas, 48);
                short passmode = ConvertHelper.BytesToShort(datas, 360);//过站信息，0无操作过站，1有操作过站
                string operatorNum = ConvertHelper.ByteToString(datas, 362, 10);//操作员号
                if (String.IsNullOrEmpty(operatorNum))
                {
                    operatorNum = "自动";
                }
                int status = datas[453];//0未加工，1合格，2NG,3切除
                int ngcdoe = datas[454];//ng代码
                short timeSpan = ConvertHelper.BytesToShort(datas, 456);//工位节拍
                string ordernum = ConvertHelper.ByteToString(datas, 60, 40);//订单号
                int status1 = (datas[452] & 1) == 1 ? 1 : 0;
                int status2 = (datas[452] & 2) == 2 ? 1 : 0;
                int status3 = (datas[452] & 4) == 4 ? 1 : 0;
                int status4 = (datas[452] & 8) == 8 ? 1 : 0;
                int status5 = (datas[452] & 16) == 16 ? 1 : 0;
                int status6 = (datas[452] & 32) == 32 ? 1 : 0;
                int status7 = (datas[452] & 64) == 64 ? 1 : 0;
                int status8 = (datas[452] & 128) == 128 ? 1 : 0;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("insert into [dbo].[tblPassStationData](SerialNumber,WorkOrderNr,StationCode,UserNo,PassMode,PassDatetime,[StationSatus1],[StationSatus2],[StationSatus3],[StationSatus4],[StationSatus5],[StationSatus6],[StationSatus7],[StationSatus8],PassStatus,NGCode,LineCode,PalletNum,TimeSpans) values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}')", packID, ordernum, StnModel.StationCode, operatorNum, passmode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), status1, status2, status3, status4, status5, status6, status7, status8, status, ngcdoe, 2,palletnum, timeSpan);
                int res = sqlHelper.ExecNonQuery(sb.ToString());
                if (res < 0)
                {
                    return 11;//保存过点失败
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, "system");
                return 100;
            }
        }

        public int SaveDianXinPassStnData(byte[] datas, string stncode)
        {
            if (stncode.Contains("C-OP010") || stncode.Contains("C-OP040"))//c-op010不存，40单独存了已经
            {
                return 0;
            }
            else
            {
                int status = datas[453];//0未加工，1合格，2NG
                int ngcdoe = datas[454];//ng代码
                short timeSpan = ConvertHelper.BytesToShort(datas, 456);//工位节拍
                int status1 = (datas[452] & 1) == 1 ? 1 : 0;
                int status2 = (datas[452] & 2) == 2 ? 1 : 0;
                int status3 = (datas[452] & 4) == 4 ? 1 : 0;
                int status4 = (datas[452] & 8) == 8 ? 1 : 0;
                int status5 = (datas[452] & 16) == 16 ? 1 : 0;
                int status6 = (datas[452] & 32) == 32 ? 1 : 0;
                int status7 = (datas[452] & 64) == 64 ? 1 : 0;
                int status8 = (datas[452] & 128) == 128 ? 1 : 0;
                short passmode = ConvertHelper.BytesToShort(datas, 360);//过站信息，0无操作过站，1有操作过站
                string operatorNum = ConvertHelper.ByteToString(datas, 362, 10);//操作员号
                string ordernum = ConvertHelper.ByteToString(datas, 60, 40);//订单号
                int res;
                if (stncode.Contains("C-OP020"))//c-op020一次过一个
                {
                    string dianxin = ConvertHelper.ByteToString(datas, 500, 80);//电芯二维码
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("insert into [dbo].[tblPassStationData](SerialNumber,WorkOrderNr,StationCode,UserNo,PassMode,PassDatetime,[StationSatus1],[StationSatus2],[StationSatus3],[StationSatus4],[StationSatus5],[StationSatus6],[StationSatus7],[StationSatus8],PassStatus,NGCode,LineCode,TimeSpans) values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}')", dianxin, ordernum, stncode, operatorNum, passmode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), status1, status2, status3, status4, status5, status6, status7, status8, status, ngcdoe, 1, timeSpan);
                    res = sqlHelper.ExecNonQuery(sb.ToString());
                }
                else//c-op030一次过2个
                {
                    string dianxin1 = ConvertHelper.ByteToString(datas, 500, 80);//电芯二维码
                    string dianxin2 = ConvertHelper.ByteToString(datas, 644, 80);//电芯二维码
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("insert into [dbo].[tblPassStationData](SerialNumber,WorkOrderNr,StationCode,UserNo,PassMode,PassDatetime,[StationSatus1],[StationSatus2],[StationSatus3],[StationSatus4],[StationSatus5],[StationSatus6],[StationSatus7],[StationSatus8],PassStatus,NGCode,LineCode,TimeSpans) values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}')", dianxin1, ordernum, stncode, operatorNum, passmode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), status1, status2, status3, status4, status5, status6, status7, status8, status, ngcdoe, 1, timeSpan);
                    
                    sb.AppendFormat(",('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}')", dianxin2, ordernum, stncode, operatorNum, passmode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), status1, status2, status3, status4, status5, status6, status7, status8, status, ngcdoe, 1, timeSpan);
                    res = sqlHelper.ExecNonQuery(sb.ToString());
                }
                if (res < 0)
                {
                    return 11;
                }
                else
                {
                    return 0;
                }

            }
        }

        /// <summary>
        /// 解析 C-OP020电芯线，包括OCV电压电阻，两个高度值，K值
        /// </summary>
        /// <param name="bytesarray"></param>
        /// <param name="datastart"></param>
        public int AnalOCVData(byte[] bytesarray, int datastart)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("insert into [tb|DianXinData](StationCode,DianXinCode,TestValueVol,TestValueRes,TestValueHig,TestName,TestTime,Result,TestValueHig2,PassStatus,WorkOrderNr,Kvalue,DeviceCode,DetectionDeviceID,DetectionAccessory,DetectionProgramName) values  ");
            short result1, result2, result3, result4;
            result1 = ConvertHelper.BytesToShort(bytesarray, 584);
            result2 = ConvertHelper.BytesToShort(bytesarray, 590);
            result3 = ConvertHelper.BytesToShort(bytesarray, 596);
            result4 = ConvertHelper.BytesToShort(bytesarray, 602);
            int result = (result1 == 1 && result2 == 1 && result3 == 1 && result4 == 1) ? 1 : 0;
            string ordnum = ConvertHelper.ByteToString(bytesarray, 60, 40);
            if (String.IsNullOrEmpty(ordnum))
            {
                return 31;//PLC那边订单号总丢，这里卡一下 让PLC排查问题
            }
            string dx = ConvertHelper.ByteToString(bytesarray, 500, 80);//点芯二维码
            if (dx.Length<24)
            {
                return 23;
            }
            string deviceCode = ConvertHelper.ByteToString(bytesarray, 464, 16);
            string deviceid = ConvertHelper.ByteToString(bytesarray, 480, 16);
            short  taotongNum = ConvertHelper.BytesToShort(bytesarray,496);
            short progromNum = ConvertHelper.BytesToShort(bytesarray, 498);
            float vol = ConvertHelper.ByteArrtoFolat(bytesarray, 580);
            float res = ConvertHelper.ByteArrtoFolat(bytesarray, 586);
            float hig1 = ConvertHelper.ByteArrtoFolat(bytesarray, 592);
            float hig2 = ConvertHelper.ByteArrtoFolat(bytesarray, 598);
            float kvalue = 0.0f;
            //计算K值，存库
            string sqlGetkValue = "exec SP_CalcOCVKvalue  '"+StnModel.StationCode+"', '" + dx + "','" + vol + "'";
            DataTable kvalueTable= sqlHelper.GetDataTb(sqlGetkValue);
            if (kvalueTable!=null)
            {
                int dianxinCount = Convert.ToInt32(kvalueTable.Rows[0]["DianXinCount"]);
                if (dianxinCount>1)
                {
                    return 25;//电芯二维码来料重复
                }
                else if(dianxinCount<1)
                {
                    return 24;//电芯二维码不在库里
                }
                else
                {
                    if (bytesarray[453]==1)
                    {
                        #region 判断电压是否在配方范围里
                        string sqlStr = "select PartNo,StationCode,Operation, MaxValue1,MinValue1,MaxValue2,MinValue3,MaxValue3,MinValue3 from  [VI_Formula]   where  StationCode ='"+StnModel.StationCode+"' AND  Operation='OCV测试'";
                        DataTable tempTable = sqlHelper.GetDataTb(sqlStr);
                        if (tempTable.Rows.Count <= 0)
                        {
                            return 28;//未获取到配方信息
                        }
                        else
                        {
                            float tempMax = Convert.ToSingle(tempTable.Rows[0]["MaxValue1"]);
                            float tempMin = Convert.ToSingle(tempTable.Rows[0]["MinValue1"]);
                            if (!(tempMin <= vol && vol <= tempMax))//电压不在配方范围里
                            {
                                return 14;
                            }
                        } 
                        #endregion
                        if (kvalueTable.Rows[0]["Result"].ToString() == "1")
                        {
                            kvalue = Convert.ToSingle(kvalueTable.Rows[0]["KValue"]);
                            int workStatus = bytesarray[453];
                            sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}'),", StnModel.StationCode, dx, vol, res, hig1, "电芯检测", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), result, hig2, workStatus, ordnum, kvalue,deviceCode,deviceid,taotongNum,progromNum);
                            string sqlstr = sb.ToString().Substring(0, sb.Length - 1);
                            int r = sqlHelper.ExecNonQuery(sqlstr);
                            if (r > 0)
                            {
                                r = 0;
                            }
                            else
                            {
                                r = 10;
                            }
                            return r;
                        }
                        else
                        {
                            return 26; //ocv测试NG
                        }
                    }
                    else
                    {
                        kvalue = Convert.ToSingle(kvalueTable.Rows[0]["KValue"]);
                        int workStatus = bytesarray[453];
                        sb.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}'),", StnModel.StationCode, dx, vol, res, hig1, "电芯检测", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), result, hig2, workStatus, ordnum, kvalue, deviceCode, deviceid, taotongNum, progromNum);
                        string sqlstr = sb.ToString().Substring(0, sb.Length - 1);
                        int r = sqlHelper.ExecNonQuery(sqlstr);
                        if (r > 0)
                        {
                            r = 0;
                        }
                        else
                        {
                            r = 10;
                        }
                        return r;
                    }
                }
            }
            else
            {
                return 9;
            }

           
        }

        /// <summary>
        /// 保存电芯段C-OP030泡棉
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public int AnalC_OP030Data(byte[] datas)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("insert into [dbo].[tblDianXinPartData] ([DianXinCode],[ComponentPartNo],[ComponentPartBarcode],[AllocNum],[UserNo],[OperationTime],[Description],[StationCode],[WorkOrderNr],BatchNo,ProviderCode) values");
            string ordnumber = ConvertHelper.ByteToString(datas, 60, 40);//订单号
            if (String.IsNullOrEmpty(ordnumber))
            {
                return 31;
            }
            string operatorNum = ConvertHelper.ByteToString(datas, 362, 10);//操作员号
            if (String.IsNullOrEmpty(operatorNum))
            {
                operatorNum = "自动";
            }
            string dx = ConvertHelper.ByteToString(datas, 500, 80);
            if (dx.Length!=24)
            {
                return 23;
            }
            string partcode = ConvertHelper.ByteToString(datas, 580, 64);
            string partnum = "";
            string providerCode = "";
            string batchNo = "";
            if (partcode.Length == 58)
            {
                partnum = partcode.Substring(0, 10);
                batchNo = partcode.Substring(17, 8);//批次信息
                providerCode = partcode.Substring(10, 7);
            }
            else
            {
                return 12;
            }
            sb.AppendFormat("('{0}', '{1}', '{2}', {3}, '{4}', '{5}', '缓冲泡棉', '{6}','{7}','{8}','{9}'),", dx, partnum, partcode, 1, operatorNum, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), StnModel.StationCode, ordnumber, batchNo, providerCode);
            dx = ConvertHelper.ByteToString(datas, 664, 80);
            if (dx.Length!=24)
            {
                return 23;
            }
            partcode = ConvertHelper.ByteToString(datas, 724, 64);
            partnum = "";
            if (partcode.Length ==58)
            {
                partnum = partcode.Substring(0, 10);
                batchNo = partcode.Substring(17, 8);//批次信息
                providerCode = partcode.Substring(10, 7);
            }
            else
            {
                return 12;
            }
            sb.AppendFormat("('{0}', '{1}', '{2}', {3}, '{4}', '{5}', '缓冲泡棉', '{6}','{7}','{8}','{9}'),", dx, partnum, partcode, 1, operatorNum, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), StnModel.StationCode, ordnumber, batchNo, providerCode);
            string sqlstr = sb.ToString().Substring(0, sb.Length - 1);
            int r = sqlHelper.ExecNonQuery(sqlstr);
            if (r > 0)
            {
                r = 0;
            }
            else
            {
                r = 10;
            }
            return r;
        }
        public int AnalC_OP040Data(byte[] datas)
        {
           
            #region 电芯压差和电阻和校验
            List<string> codeList = new List<string>();
            for (int i = 500; i <= 1460;)
            {
                string rempdxcode = ConvertHelper.ByteToString(datas, i, 80);
                if (rempdxcode.Length < 24)
                {
                    return 23;
                }
                i += 80;
                codeList.Add(rempdxcode);
            }
            float volRange = 0;
            float resSum = 0;
            float volmax, volmin, resmax, resmin;
            int workStatus = 0;
            int dataResult = 0;
            int res = CalaDianXinYacha(codeList, StnModel.StationCode,out volRange,out resSum,out volmax,out volmin, out resmax,out resmin);
            if (datas[453]==1)
            {
              //  int res = CalaDianXinYacha(codeList,StnModel.StationCode);
                if (res != 0)
                {
                    return res;
                } 
            }
            workStatus = datas[453];
            dataResult = (res == 0 ? 1 : 2);
            #endregion
            string ordnum = ConvertHelper.ByteToString(datas, 60, 40);//订单号
            short passmode = ConvertHelper.BytesToShort(datas, 360);//过站类型
            string ordernum = ConvertHelper.ByteToString(datas, 60, 40);//订单号
            if (String.IsNullOrEmpty(ordernum))
            {
                return 31;
            }
            string operatorNum = ConvertHelper.ByteToString(datas, 362, 10);//操作员号
            if (String.IsNullOrEmpty(operatorNum))
            {
                operatorNum = "自动";
            }

            string mozusn = GetMozuSerialNumber(ordernum);//模组序列号
            List<string> sqllist = new List<string>();

            #region 电芯内阻和与压差极值存储
            StringBuilder volsb = new StringBuilder();
            volsb.AppendFormat("insert into tblVolResData (DianXinCode,MoZuSerialNumber,VolDiff,VolDiffMax,VolDiffMim,ResSum,ResSumMax,ResSumMin,CreateTime,Result,PassStatus,VolUnit,ResUnit,WorkOrderNr,StationCode,DataDescription,UserNo) values   ");
            for (int i = 0; i < codeList.Count; i++)
            {
                volsb.AppendFormat("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}','{15}','{16}'),", codeList[i],mozusn,volRange,volmax,volmin,resSum,resmax,resmin,DateTime.Now.ToString("yyyy-MM-dd HH: mm:ss.fff"),dataResult,workStatus,"mv","mΩ",ordernum,StnModel.StationCode,"内阻和与电压极差", operatorNum);
            }
            string volsqlstr = volsb.ToString().Substring(0, volsb.Length - 1);
            sqllist.Add(volsqlstr);
            #endregion

            #region 电芯过站信息存错

            StringBuilder sqlsb = new StringBuilder();
            sqlsb.AppendFormat("insert into [dbo].[tblPassStationData](SerialNumber,WorkOrderNr,StationCode,UserNo,PassMode,PassDatetime,[StationSatus1],[StationSatus2],[StationSatus3],[StationSatus4],[StationSatus5],[StationSatus6],[StationSatus7],[StationSatus8],PassStatus,NGCode,LineCode,TimeSpans,MozuSerialNumber,PalletNum) values ");
           
       
            int status1 = (datas[452] & 1) == 1 ? 1 : 0;
            int status2 = (datas[452] & 2) == 2 ? 1 : 0;
            int status3 = (datas[452] & 4) == 4 ? 1 : 0;
            int status4 = (datas[452] & 8) == 8 ? 1 : 0;
            int status5 = (datas[452] & 16) == 16 ? 1 : 0;
            int status6 = (datas[452] & 32) == 32 ? 1 : 0;
            int status7 = (datas[452] & 64) == 64 ? 1 : 0;
            int status8 = (datas[452] & 128) == 128 ? 1 : 0;
            int passstatus = datas[453];//合格状态
            int ngcode = datas[454];//缺陷代码
            short timeSpan = ConvertHelper.BytesToShort(datas, 456);
            short palletNum = ConvertHelper.BytesToShort(datas, 48);
            string dxcode = "";
       
            for (int i = 500; i <= 1460;)
            {
                dxcode = ConvertHelper.ByteToString(datas, i, 80);
                if (dxcode.Length<24)
                {
                    return 23; 
                }
                sqlsb.AppendFormat("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}','{14}','{15}','{16}','{17}','{18}','{19}'),", dxcode, ordernum, StnModel.StationCode, operatorNum, passmode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), status1, status2, status3, status4, status5, status6, status7, status8, passstatus, ngcode, 1, timeSpan, mozusn, palletNum);
                i += 80;
           
            }

            string sql = sqlsb.ToString().Substring(0, sqlsb.Length - 1);
            sqllist.Add(sql);

            #endregion
            #region 电芯与绝缘片物料绑定和存库
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("insert into [dbo].[tblDianXinPartData] ([DianXinCode],[ComponentPartNo],[ComponentPartBarcode],[AllocNum],[UserNo],[OperationTime],[Description],[StationCode],WorkOrderNr,BatchNo,ProviderCode) values");
            string dx1 = ConvertHelper.ByteToString(datas, 500, 80);//电芯1二维码
            string dx13 = ConvertHelper.ByteToString(datas, 1460, 80);//电芯2 二维码
            string partcode = ConvertHelper.ByteToString(datas, 1540, 64);
            string partnum = "";
            string batchNo = "";
            string providerCode = "";
            if (partcode.Length ==58)
            {
                partnum = partcode.Substring(0, 10);
                batchNo = partcode.Substring(17, 8);//批次信息
                providerCode = partcode.Substring(10, 7);
            }
            else
            {
                return 12;
            }
            sb.AppendFormat("('{0}', '{1}', '{2}', {3}, '{4}', '{5}', '绝缘片(PC片)', '{6}','{7}', '{8}','{9}'),", dx1, partnum, partcode, 1, operatorNum, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), StnModel.StationCode, ordnum,batchNo,providerCode);
            partcode = ConvertHelper.ByteToString(datas, 1604, 64);
            partnum = "";
           
            if (partcode.Length == 58)
            {
                partnum = partcode.Substring(0, 10);
                batchNo = partcode.Substring(17, 8);//批次信息
                providerCode = partcode.Substring(10, 7);
            }
            else
            {
                return 12;
            }
            sb.AppendFormat("('{0}', '{1}', '{2}', {3}, '{4}', '{5}', '绝缘片(PC片)', '{6}','{7}', '{8}','{9}'),", dx13, partnum, partcode, 1, operatorNum, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), StnModel.StationCode, ordnum, batchNo, providerCode);
            string sqlstr = sb.ToString().Substring(0, sb.Length - 1);
            sqllist.Add(sqlstr);
            int r = sqlHelper.ExecNonQuery(sqllist);
            if (r > 0)
            {
                r = 0;
                int  r2= UpdateMozuSerialNumber();
                if (r2==0)
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
                r = 10;
            }
            return r; 
            #endregion
        }


        private  string GetMozuSerialNumber(string  orderNum)
        {
            string sqlstr = "DECLARE @TEMP VARCHAR(100) EXEC[dbo].[SP_CreateMozuCode]   '" + orderNum + "', @TEMP OUT SELECT @TEMP as MozuSn";
             DataTable  tb=  sqlHelper.GetDataTb(sqlstr);
            string mozusn = "";
            if (tb.Rows.Count>0)
            {
                mozusn = tb.Rows[0]["MozuSn"].ToString();
            }
            else
            {
                mozusn = "";
            }
            return mozusn;
        }
        private   int UpdateMozuSerialNumber()
        {
            StringBuilder sb2 = new StringBuilder();
            sb2.AppendFormat("select count(1) from tbMozuDaySerialNumber where YearTag='{0}' and MonthTag='{1}' and DayTag='{2}'", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);//查询当天是否有上线记录
            int record = Convert.ToInt32(sqlHelper.GetObjectVal(sb2.ToString()));//查询当天是否有上线记录
            int res;
            if (record > 0)
            {
                sb2.Clear();
                sb2.AppendFormat("update tbMozuDaySerialNumber set SearialNumber=SearialNumber+1 where YearTag='{0}' and MonthTag='{1}' and DayTag='{2}'", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                res= sqlHelper.ExecNonQuery(sb2.ToString());
            }
            else
            {
                sb2.Clear();
                sb2.AppendFormat("insert into [dbo].[tbMozuDaySerialNumber] (YearTag,MonthTag,DayTag,SearialNumber) values ('{0}','{1}','{2}','{3}') ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1);
                res= sqlHelper.ExecNonQuery(sb2.ToString());
            }
            if (res>0)
            {
                return 0;
            }
            else
            {
                return  -1;
            }
          
        }


        public int UpdateKetiCode(string sncode, string code)
        {
            string str = "update tblProduct set KetiCode='" + code + "' where ProductSerialNumber='" + sncode + "'";
            int r = sqlHelper.ExecNonQuery(str);
            return r;
        }


        /// <summary>
        /// 保存NG下线tag数据
        /// </summary>
        /// <param name="dataarray"></param>
        /// <returns></returns>
        public int SaveTagValue(byte[] dataarray)
        {
            try
            {
                string tagvalue = string.Join(",", dataarray);
                string packid = ConvertHelper.ByteToString(dataarray, 196, 70);//packID
                if (String.IsNullOrEmpty(packid))
                {
                    return 20;
                }
                string keticode = ConvertHelper.ByteToString(dataarray, 386, 70);//下壳体条码
                string sqlstr = string.Format("delete tblTagValue  where KetiCode='{0}'", keticode);//先删除在存储
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("insert into tblTagValue (Pack_ID,KetiCode,StationCode,TagValue,CreateTime)  values('{0}','{1}','{2}','{3}','{4}')", packid, keticode, StnModel.StationCode, tagvalue, DateTime.Now);
                string sqlstr2 = "update tblProduct  set IsDelete=1  where ProductSerialNumber='" + packid + "' and IsDelete!=2";
                List<string> sqllist = new List<string>();
                sqllist.Add(sqlstr);
                sqllist.Add(sb.ToString());
                sqllist.Add(sqlstr2);
                int r = sqlHelper.ExecNonQuery(sqllist);
                if (r > 0)
                {
                    return 0;
                }
                else
                {
                    return 10;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write("保存tag数据时候发生异常:" + ex.Message + "\r\n" + ex.StackTrace, "system");
                return 100;
            }
        }

        /// <summary>
        /// 保存240下线的TAG值
        /// </summary>
        /// <param name="dataarray"></param>
        /// <returns></returns>
        public int SaveOP240TagValue(byte[] dataarray)
        {
            try
            {
                string tagvalue = string.Join(",", dataarray);
                string packid = ConvertHelper.ByteToString(dataarray, 196, 70);//packID
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("insert into tblTagDataSrc (Pack_ID,StationCode,TagValue,CreateTime)  values('{0}','{1}','{2}','{3}')", packid,  StnModel.StationCode, tagvalue, DateTime.Now);
                List<string> sqllist = new List<string>();
                sqllist.Add(sb.ToString());
                int r = sqlHelper.ExecNonQuery(sqllist);
                if (r > 0)
                {
                    return 0;
                }
                else
                {
                    return 10;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write("保存OP240下线tag数据时候发生异常:" + ex.Message + "\r\n" + ex.StackTrace, "system");
                return 100;
            }
        }


        /// <summary>
        /// 根据sn号查询生产记录
        /// </summary>
        /// <param name="snnum"></param>
        /// <returns></returns>
        public DataTable GetProdRecordTb(string snnum)
        {
            StringBuilder sb2 = new StringBuilder();
            sb2.AppendFormat("select * from [tblProduct]   where ProductSerialNumber='{0}'", snnum);
            return sqlHelper.GetDataTb(sb2.ToString());
        }

        public int UpdateDianXinOrder(byte[] bytes)
        {
            string ordnum = ConvertHelper.ByteToString(bytes, 60, 40);//订单号
            string sqlstr = "select * from tblDianXinOrder where WorkOrderNo='" + ordnum + "'";
            DataTable ordtb = sqlHelper.GetDataTb(sqlstr);
            if (ordtb.Rows.Count > 0)
            {
                int planCount = Convert.ToInt32(ordtb.Rows[0]["ActualQty"]);
                int onlineCount = Convert.ToInt32(ordtb.Rows[0]["OnlineCount"]);
                int r;
                if (onlineCount + 1 < planCount)     //上线数量99 计划数量100
                {
                    string updatesql = "update [tblDianXinOrder] set [OnlineCount]=[OnlineCount]+1 ,OrderStatus=1 where WorkOrderNo='" + ordnum + "' ";
                    r = sqlHelper.ExecNonQuery(updatesql);
                }
                else
                {
                    string updatesql = "update [tblDianXinOrder] set [OnlineCount]=[OnlineCount]+1 ,OrderStatus=2 where WorkOrderNo='" + ordnum + "' ";
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



        /// <summary>
        /// 校验数据是否符合配方上下限
        /// </summary>
        /// <param name="stncode">工位号</param>
        /// <param name="datatypename">数据类型名称</param>
        /// <param name="partno">总成物料号</param>
        /// <param name="datas">数据</param>
        /// <returns></returns>
        private bool CheckDataVaid(string stncode, string datatypename, string partno, float[,] datas)
        {
            bool res = false;
            string sqlstr = "select [PartNo],[StationCode],[Operation],[TargetValue1] ,[MaxValue1],[MinValue1],[IncludeEdge1],[TargetValue2] ,[MaxValue2],[MinValue2] ,[TargetValue3],[MaxValue3],[MinValue3],[TargetValue4] ,[MaxValue4] ,[MinValue4] from [VI_Formula]  where [PartNo]='" + partno + "' and  [StationCode]='" + stncode + "' and Operation='" + datatypename + "' ";
            DataTable dataTable = sqlHelper.GetDataTb(sqlstr);
            string maxValue = "MaxValue", minValue = "MinValue";
            int rows = datas.GetLength(0);//获取行数
            int cols = datas.GetLength(1);//获取列数
            for (int i = 0; i < rows; i++)
            {
                int temp = 1;
                for (int j = 0; j < cols; j++)
                {
                  
                    float max = Convert.ToSingle(dataTable.Rows[0][maxValue + temp]);
                    float min = Convert.ToSingle(dataTable.Rows[0][minValue + temp]);
                    if (min <= datas[i, j] && datas[i, j] <= max)
                    {
                        temp++;
                        res = true;
                    }
                    else
                    {
                        res = false;
                        return res;
                    }
                }
            }
            return res;
        }



        /// <summary>
        /// 数据完整性校验，只要>0就认为完整
        /// </summary>
        /// <param name="datalist">从PLC那边读上来的所有数据</param>
        /// <returns></returns>
        private bool CheckDataIntegrity(List<float> datalist)
        {
            bool res = false;
            for (int i = 0; i < datalist.Count; i++)
            {
                if (datalist[i] < 0)
                {
                    res = false;
                    break;
                }
                else
                {
                    res = true;
                }
            }
            return res;
        }


        public int DeleteQualitiesData(string packid, string stncode)
        {
            StringBuilder sqlsb = new StringBuilder();
            sqlsb.AppendFormat("update tblQualityData  set IsDelete=1  where SerialNumber='{0}' and StationCode='{1}'  and IsDelete=0 ", packid, stncode);
            int res = sqlHelper.ExecNonQuery(sqlsb.ToString());
            if (res >= 0)
            {
                return 0;
            }
            else
            {
                return 16;
            }

        }
        public int DeleteImageData(string packid, string stncode)
        {
            StringBuilder sqlsb = new StringBuilder();
            sqlsb.AppendFormat("update tblSKQImage  set IsDelete=1  where SerialNumber='{0}' and StationCode='{1}' ", packid, stncode);
            int res = sqlHelper.ExecNonQuery(sqlsb.ToString());
            if (res >= 0)
            {
                return 0;
            }
            else
            {
                return 16;
            }

        }


        /// <summary>
        /// 校验OP040 绝缘性测试
        /// </summary>
        /// <param name="partno">总成物料号</param>
        /// <param name="resmat">电阻矩阵(1x2)</param>
        /// <param name="volmat">电压矩阵(13x1)</param>
        /// <returns></returns>
        private bool CheckOP040InsulationData(string partno, float[,] resmat, float[,] volmat,string  stncode)
        {
            bool res = false;
            string sqlstr = "select [PartNo],[StationCode],[Operation],[TargetValue1] ,[MaxValue1],[MinValue1],[IncludeEdge1],[TargetValue2] ,[MaxValue2],[MinValue2] ,[TargetValue3],[MaxValue3],[MinValue3],[TargetValue4] ,[MaxValue4] ,[MinValue4] from [VI_Formula]  where [PartNo]='" + partno + "' and  [StationCode]='"+ stncode + "' and Operation='绝缘阻抗测试' ";
            DataTable dataTable = sqlHelper.GetDataTb(sqlstr);

            float max1 = Convert.ToSingle(dataTable.Rows[0]["MaxValue1"]);
            float min1 = Convert.ToSingle(dataTable.Rows[0]["MinValue1"]);

            float max2 = Convert.ToSingle(dataTable.Rows[0]["MaxValue2"]);
            float min2 = Convert.ToSingle(dataTable.Rows[0]["MinValue2"]);
            //if ((min1 <= resmat[0, 0] && resmat[0, 0] <= max1) && (min2 <= resmat[0, 1] && resmat[0, 1] <= max2))
            if ((min1 <= resmat[0, 0] ) && (min2 <= resmat[0, 1] ))
            {
                float max3 = Convert.ToSingle(dataTable.Rows[0]["MaxValue3"]);
                float min3 = Convert.ToSingle(dataTable.Rows[0]["MinValue3"]);
                for (int i = 0; i < 13; i++)
                {
                    if (min3 <= volmat[i, 0] && volmat[i, 0] <= max3)
                    {
                        res = true;
                    }
                    else
                    {
                        res = false;
                        break;
                    }
                }
            }
            else
            {
                res = false;
            }

            return res;

        }


       /// <summary>
       /// NG剔料，先把订单计划数量+1， 然后把该电芯记录到过点表
       /// </summary>
       /// <param name="datas"></param>
       /// <returns></returns>
        public  int DianXinNg(byte[] datas)
        {
            string ordnum = ConvertHelper.ByteToString(datas, 60, 40);
            string sqlstr1 = "update tblDianXinOrder set ActualQty=ActualQty+1  where  WorkOrderNo='" + ordnum + "'";
            #region 电芯数据解析
            string dianxincode = ConvertHelper.ByteToString(datas, 500, 80);
            int status = datas[453];//0未加工，1合格，2NG
            int ngcdoe = datas[454];//ng代码
            short timespan = ConvertHelper.BytesToShort(datas, 456);//工位节拍
            int status1 = (datas[452] & 1) == 1 ? 1 : 0;
            int status2 = (datas[452] & 2) == 2 ? 1 : 0;
            int status3 = (datas[452] & 4) == 4 ? 1 : 0;
            int status4 = (datas[452] & 8) == 8 ? 1 : 0;
            int status5 = (datas[452] & 16) == 16 ? 1 : 0;
            int status6 = (datas[452] & 32) == 32 ? 1 : 0;
            int status7 = (datas[452] & 64) == 64 ? 1 : 0;
            int status8 = (datas[452] & 128) == 128 ? 1 : 0;
            short passmode = ConvertHelper.BytesToShort(datas, 360);//过站信息，0无操作过站，1有操作过站
            string operatorNum = ConvertHelper.ByteToString(datas, 362, 10);//操作员号
            if (String.IsNullOrEmpty(operatorNum))
            {
                operatorNum = "自动";
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("insert into [dbo].[tblPassStationData](SerialNumber,WorkOrderNr,StationCode,UserNo,PassMode,PassDatetime,[StationSatus1],[StationSatus2],[StationSatus3],[StationSatus4],[StationSatus5],[StationSatus6],[StationSatus7],[StationSatus8],PassStatus,NGCode,LineCode,TimeSpans) values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}')", dianxincode, ordnum, StnModel.StationCode, operatorNum, passmode, DateTime.Now, status1, status2, status3, status4, status5, status6, status7, status8, status, ngcdoe, 1, timespan); 
            #endregion
            List<string> sqllist = new List<string>();
            sqllist.Add(sqlstr1);
            sqllist.Add(sb.ToString());
            int  res = sqlHelper.ExecNonQuery(sqllist);
            if (res>0)
            {
                return 0;
            }
            else
            {
                return 10;
            }
        }

/// <summary>
/// 计算压差极值和内阻和 单位毫伏 毫欧
/// </summary>
/// <param name="codelist">13个电芯</param>
/// <param name="stncode">工位号</param>
/// <param name="outvolRange">压差</param>
/// <param name="outresSum">内阻和</param>
/// <param name="volmax">压差配方最大</param>
/// <param name="volmin">压差配方最小</param>
/// <param name="resmax">内阻和最大</param>
/// <param name="resmin">内阻和最小</param>
/// <returns></returns>
         private   int CalaDianXinYacha(List<string>  codelist,string stncode,out float outvolRange,out float outresSum,out float volmax, out float volmin, out float resmax, out float resmin)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select  MAX(TestValueVol) -MIN(TestValueVol) as VolRange,SUM(TestValueRes)  as ResSum ,COUNT(1) as BatteryCount from [tb|DianXinData]  where IsDelete=0 and  DianXinCode  in(");
            for (int i = 0; i < codelist.Count; i++)
            {
                sb.AppendFormat("'{0}',", codelist[i]);
            }
            string sqlstr=  sb.ToString().Substring(0, sb.Length - 1)+")";
            DataTable resultTable = sqlHelper.GetDataTb(sqlstr);
            int count = Convert.ToInt32(resultTable.Rows[0]["BatteryCount"]);
            if (count < 13)
            {
                outvolRange = 0;
                outresSum = 0;
                volmax = 0;
                volmin = 0;
                resmax = 0;
                resmin = 0;
                return 29;
            }
            else  if(count>13)
            {
                outvolRange = 0;
                outresSum = 0;
                volmax = 0;
                volmin = 0;
                resmax = 0;
                resmin = 0;
                return 30;
            }
            else
            {
                string   tempsql = "select PartNo,StationCode,Operation, MaxValue1,MinValue1,MaxValue2,MinValue2,MinValue3,MaxValue3,MinValue3 from  [VI_Formula]   where  StationCode ='"+ stncode + "' AND  Operation='堆叠'";
                DataTable tempTable = sqlHelper.GetDataTb(tempsql);
                if (tempTable.Rows.Count<=0)
                {
                    outvolRange = 0;
                    outresSum = 0;
                    volmax = 0;
                    volmin = 0;
                    resmax = 0;
                    resmin = 0;
                    return 28;// 未获取到配方信息
                }
                else
                {
                    //压差最大最小
                    float volMax;
                    float volMin;
                    float.TryParse(tempTable.Rows[0]["MaxValue1"].ToString(), out volMax);
                    float.TryParse(tempTable.Rows[0]["MinValue1"].ToString(), out volMin);
                    //电阻和最大最小
                    float resMax;
                    float resMin;
                    float.TryParse(tempTable.Rows[0]["MaxValue2"].ToString(), out resMax);
                    float.TryParse(tempTable.Rows[0]["MinValue2"].ToString(), out resMin);


                    //计算出来的压差
                    float volrange = Convert.ToSingle(resultTable.Rows[0]["VolRange"])*1000.0f;
                    //计算出来的电阻和
                    float resSum = Convert.ToSingle(resultTable.Rows[0]["ResSum"])*1000.0f;
                    if (volMin<= volrange  && volrange<=volMax && resMin<=resSum && resSum<=resMax)
                    {
                        outresSum = resSum;
                        outvolRange = volrange;
                        volmax = volMax;
                        volmin = volMin;
                        resmax = resMax;
                        resmin = resMin;
                        return 0;
                    }
                    else
                    {
                        outresSum = resSum;
                        outvolRange = volrange;
                        volmax = volMax;
                        volmin = volMin;
                        resmax = resMax;
                        resmin = resMin;
                        return 14;
                    }
                }  
      
            }
            
        }


       /// <summary>
       /// 校验物料完整性
       /// </summary>
       /// <param name="packid">packid</param>
       /// <param name="stncode">工位号</param>
       /// <param name="partno">成品物料号</param>
       /// <returns></returns>
        private  int CheckPartDataComplete(string packid,string stncode,string   partno)
        {
            string sqlstr = String.Format("exec SP_CheckPartDataComplete '{0}','{1}','{2}'", packid, stncode, partno);
            DataTable tb = sqlHelper.GetDataTb(sqlstr);
            int res = Convert.ToInt32(tb.Rows[0]["Result"]);
            if (res==1)
            {
                return 0;
            }
            else
            {
                return 17;
            }
        }


        private int CheckPartDataComplete(byte[] datas)
        {
            string partNo = ConvertHelper.ByteToString(datas, 100, 20);//总成物料号
            string packID = ConvertHelper.ByteToString(datas, 120, 70);//packid
            int workstatus = datas[453];
            if (workstatus==1)
            {
                return  CheckPartDataComplete(packID, StnModel.StationCode, partNo);
            }
            else
            {
                return 0;
            }
            
        }
    }
}
