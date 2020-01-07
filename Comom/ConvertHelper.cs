using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_UA_Client_A50.Comom
{
    public static  class ConvertHelper
    {


        /*
         * 微软里面的 bitconver 是 从 低位到高位排列的数组，从plc那读上来 是从高到低的顺序 所以转换一下
         */
        public static int ByteArrtoInt(byte[] arr)
        {
           Array.Reverse(arr);
           return  BitConverter.ToInt32(arr,0);
        }
        public static double ByteArrtoDouble(byte[] arr)
        {
            Array.Reverse(arr);
            return BitConverter.ToDouble(arr, 0);
        }
        public static float ByteArrtoFolat(byte[] srcdatas,int index)
        {
            byte[] temp = new byte[4];
            Array.Copy(srcdatas, index, temp, 0, 4);
            Array.Reverse(temp);
            return BitConverter.ToSingle(temp, 0);
        }
        public static float ByteArrtoFolat(byte[] arr)
        {
            Array.Reverse(arr);
            return BitConverter.ToSingle(arr, 0);
        }
        public static ushort ByteArrtoUshort(byte[] arr)
        {
            Array.Reverse(arr);
            return BitConverter.ToUInt16(arr, 0);
        }

        public static byte[] StringToByteArray(string value, int needLen)
        {
            byte[] retu = new byte[needLen];
            if (!String.IsNullOrEmpty(value))
            {
                char[] temp = value.ToCharArray();
                for (int i = 0; i < needLen; i++)
                {
                    if (i < temp.Length)
                    {
                        retu[i] = Convert.ToByte( temp[i]);
                    }
                    else
                    {
                        retu[i] = 0;
                    }
                }
                return retu;
            }
            else
            {
                return retu;
            }
            
        }

        public static string ByteToString(byte[] val, int byteIndex, int byteCount)
        {
            return new ASCIIEncoding().GetString(val, byteIndex, byteCount).Replace("\0", "").Trim();
        }
        public static short BytesToShort(byte[] arr)
        {
            Array.Reverse(arr);
            return BitConverter.ToInt16(arr, 0);
        }
        public static short BytesToShort(byte[] srcdatas,int index)
        {
            byte[] temp = new byte[2];
            Array.Copy(srcdatas, index, temp, 0, 2);
            Array.Reverse(temp);
            return BitConverter.ToInt16(temp, 0);
        }

        public static DateTime BytesToTime(byte[] srcdatas, int index)
        {
      
            string timeOfYear = DateTime.Now.Year.ToString().Substring(0, 2);
            byte year = ConvertBCDByteToByte(srcdatas[index]);
            byte mon = ConvertBCDByteToByte(srcdatas[index+1]);
            byte day = ConvertBCDByteToByte(srcdatas[index+2]);
            byte hour = ConvertBCDByteToByte(srcdatas[index+3]);
            byte min = ConvertBCDByteToByte(srcdatas[index+4]);
            byte sec = ConvertBCDByteToByte(srcdatas[index+5]);
            byte msec = ConvertBCDByteToByte(srcdatas[index+6]);
            string timestr= string.Format("{0}{1}-{2}-{3} {4}:{5}:{6}.{7}", timeOfYear, year, mon, day, hour, min, sec, msec);
            DateTime time = new DateTime();
            DateTime.TryParse(timestr, out time);
            return time;
        }
        private static byte ConvertBCDToByte(byte b)//byte转换为BCD码
        {
            //高四位  
            byte b1 = (byte)(b / 10);
            //低四位  
            byte b2 = (byte)(b % 10);
            return (byte)((b1 << 4) | b2);
        }

        /// <summary>
        /// 将PLC那边BCD编码的字节数，转换成 普通10进制的字节数
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static byte ConvertBCDByteToByte(byte b)
        {
            //高四位  
            byte b1 = (byte)((b >> 4) & 0xF);
            //低四位  
            byte b2 = (byte)(b & 0xF);

            return (byte)(b1 * 10 + b2);
        }



        /// <summary>
        /// 计算 0001-01-01 00:00:00 到现在的毫秒数。
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            return (long)(DateTime.Now.ToLocalTime() -DateTime.MinValue).TotalSeconds;
        }

        /// <summary>
        /// 计算 0001-01-01 00:00:00 到指定 <see cref="DateTime"/> 的毫秒数。
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetTimeStamp(DateTime dateTime)
        {
            return dateTime.Ticks;
        }

    }
}
