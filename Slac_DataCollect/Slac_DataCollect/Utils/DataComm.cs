using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

namespace Slac_DataCollect.Utils
{
    /// <summary>
    /// 数据通信：UDP连接服务
    /// </summary>
    public class DataComm
    {
        public delegate void DATAAcceptDataHandler(Tuple<IPEndPoint, byte[], int> t);
        public event DATAAcceptDataHandler EventAcceptData;
        private Thread _thDATA = null;
        private static Socket _serverDATASocket = null;
        public static bool isRun; // UDP通讯状态位             

        private readonly ConcurrentQueue<Tuple<IPEndPoint, byte[], int>> tuples; // 接收到的数据

        public DataComm(ConcurrentQueue<Tuple<IPEndPoint, byte[], int>> tuple)
        {
            tuples = tuple;
            isRun = false;
        }



        #region DATA
        /// <summary>
        /// 建立PLC数据连接，接收数据：UDP通讯
        /// </summary>
        /// <param name="tcpIp"></param>
        /// <param name="tcpPort"></param>
        /// <returns></returns>
        public bool StartDATAServer(string tcpIp, int tcpPort)
        {
            bool success = false;
            try
            {
                _serverDATASocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);                
                IPAddress iP = IPAddress.Parse(tcpIp);
                IPEndPoint endPoint = new IPEndPoint(iP, tcpPort);
                _serverDATASocket.Bind(endPoint);
                if (_thDATA == null)
                {
                    _thDATA = new Thread(ReciveDATAMsg);
                    _thDATA.IsBackground = true;
                    _thDATA.Start();
                }
                isRun = true; // 状态位
                LogConfig.WriteRunLog("ReceiveMQ", $"PLC连接UDP通讯成功：IP：{tcpIp}，端口：{tcpPort}");
            }
            catch(Exception ex)
            {
                LogConfig.WriteErrLog("ReceiveMQ", $"PLC连接UDP通讯失败：{ex.Message}");
                isRun = false;
                success = false;
            }
            success = true;

            return success;
        }

        /// <summary>
        /// PLC数据接收服务
        /// </summary>
        private void ReciveDATAMsg()
        {
            while (true)
            {
                try
                {
                    EndPoint point = new IPEndPoint(IPAddress.Any, 0);
                    byte[] buffer = new byte[1024 * 1024];
                    int length = _serverDATASocket.ReceiveFrom(buffer, ref point);
                    byte[] buffer_out = new byte[length];
                    Buffer.BlockCopy(buffer, 0, buffer_out, 0, length);
                    //string msg = Encoding.UTF8.GetString(buffer, 0, length);
                    //Tuple<IPEndPoint, string> tuple = Tuple.Create<IPEndPoint, string>((IPEndPoint)point, msg);
                    Tuple<IPEndPoint, byte[], int> tuple = Tuple.Create<IPEndPoint, byte[], int>((IPEndPoint)point, buffer_out, length);                                       
                    tuples.Enqueue(tuple);   
                    EventAcceptData?.Invoke(tuple); //触发事件

                }
                catch (Exception ex)
                {
                    isRun = false;
                    //ex.ToString();
                }
            }
        }

        public void SendDATA(IPEndPoint ipend, byte[] headbyte)
        {
            _serverDATASocket.BeginSendTo(headbyte, 0, headbyte.Length, 0, ipend, new AsyncCallback(DATASendCallback), _serverDATASocket);
        }

        private void DATASendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 关闭数据接收服务
        /// </summary>
        public void StopDATAServer()
        {
            if (_serverDATASocket != null)
            {

                try
                {
                    if (_thDATA != null)
                    {
                        _thDATA.Abort();
                        _thDATA = null;
                    }

                    _serverDATASocket.Shutdown(SocketShutdown.Both);
                    _serverDATASocket.Close();
                    _serverDATASocket.Dispose();
                    _serverDATASocket = null;
                    LogConfig.WriteRunLog("ReceiveMQ", "PLC连接UDP通讯关闭成功");
                }
                catch
                {
                    _serverDATASocket.Close();
                    _serverDATASocket.Dispose();
                    _serverDATASocket = null;

                }
            }
        }

        #endregion
    }
}
