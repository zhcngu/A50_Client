using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OPC_UA_Client_A50.Comom
{
  public static  class LogHelper
    {
        static object logLock = new object();
        static object logLock1 = new object();
        static object logLock2 = new object();
        /// <summary>
        /// 写日志文件
        /// </summary>
        /// <param name="message"></param>
        public static void Write(string message)
        {
            lock (logLock)
            {
                string logfilepath = Application.StartupPath + "\\system.log";
                using (StreamWriter sw = new StreamWriter(logfilepath.Trim(), true))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "    " + message);
                    sw.WriteLine("");
                    sw.Flush();
                    sw.Close();
                }
            }
        }
        /// <summary>
        /// 写日志文件
        /// </summary>
        /// <param name="message"></param>
        public static void Write(string message, string fileName)
        {
            lock (logLock1)
            {
                string logfilepath = Application.StartupPath + "\\Log\\" + fileName + ".log";
                using (StreamWriter sw = new StreamWriter(logfilepath.Trim(), true))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"    "+message  );
                    sw.WriteLine("");
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static void Write(Exception ex, string fileName)
        {
            lock (logLock2)
            {
                string logfilepath = Application.StartupPath + "\\Log\\" + fileName + ".log";
                using (StreamWriter sw = new StreamWriter(logfilepath.Trim(), true))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 异常消息: " + ex.Message);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 堆栈消息: " + ex.StackTrace);
                    sw.WriteLine("");
                    sw.Flush();
                    sw.Close();
                }
            }
        }
    }
}
