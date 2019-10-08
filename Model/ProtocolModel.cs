using Opc.Ua;
using Siemens.OpcUA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_UA_Client_A50.Model
{
    [Serializable]
    public class StationModel
    {
        public StationModel()
        {

        }
        public StationModel(string  stncode)
        {
            StationCode = stncode;
        }
        public   ushort ServerIndex { get; set; }
        public Server StationOPCServer { get; set; }
        public NodeIdCollection MonitorNodes { get; set; }
        public int[] MonitorClientHandl { get; set; }
        public string StationCode { get; set; }
        public string STN_Status { get; set; }
        public string CommunicationDB { get; set; }
        public string DataReadDB { get; set; }
        public string DataWriteDB { get; set; }
        public string RepairDataReadDB { get; set; }
        public string RepairDataWriteDB { get; set; }
        public string DataAddress { get; set; }
        public string DataLength { get; set; }
        public string DBLength { get; set; }

    }
    [Serializable]
    public class BaseProtocol
    {
        public string PLC_MES_Heart { get; set; }
        public string PLC_MES_Remark1 { get; set; }
        public string PLC_MES_DataRequest { get; set; }
        public string PLC_MES_DataSave { get; set; }
        public string PLC_MES_ReadBom { get; set; }
        public string PLC_MES_RepairRequest { get; set; }
        public string PLC_MES_RepairSave { get; set; }
        public string PLC_MES_Remark2 { get; set; }
        public string PLC_MES_OCV_Test { get; set; }
        public string PLC_MES_Index { get; set; }


        public string MES_PLC_Heart { get; set; }
        public string MES_PLC_Ready { get; set; }
        public string MES_PLC_Writing { get; set; }
        public string MES_PLC_WriteDowd { get; set; }
        public string MES_PLC_Reading { get; set; }
        public string MES_PLC_ReadDown { get; set; }
        public string MES_PLC_BomOK { get; set; }
        public string MES_PLC_BomNG { get; set; }
        public string MES_PLC_RepairWriting { get; set; }
        public string MES_PLC_RepairWriteDown { get; set; }
        public string MES_PLC_RepairReading { get; set; }
        public string MES_PLC_RepairReadDown { get; set; }
        public string MES_PLC_CanWork { get; set; }
        public string MES_PLC_NotWork { get; set; }
        public string MES_PLC_RepairPart { get; set; }
        public string MES_PLC_Remark { get; set; }
        public string MES_PLC_OCVOK { get; set; }
        public string MES_PLC_OCVNG { get; set; }
        public string MES_PLC_ErrorCode { get; set; }

        public string MES_PLC_UniqOK { get; set; }

        public string MES_PLC_UniqNG { get; set; }

    }
    [Serializable]
    public class ProtocolModel
    {
        public ProtocolModel()
        {
            STN_BaseProtocol = new BaseProtocol();
            StationModelList = new List<StationModel>();
        }
        public BaseProtocol STN_BaseProtocol { get; set; }
        public List<StationModel> StationModelList { get; set; }
    }
}
