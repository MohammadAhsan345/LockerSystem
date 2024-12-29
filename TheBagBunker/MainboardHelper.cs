using MainBoardLib;

namespace MainboardHelperLib
{
    public class MainboardHelper
    {
        private bool _scannerOn = false;
        HldMainBoard hldMainboard;
        string _curPort = "";
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainboardHelper()
        {
            hldMainboard = new HldMainBoard();
            _scannerOn = false;
        }

        public bool ScannerStatus()
        {
            return _scannerOn;
        }

        public int[] GetAllLockStatus(int nSide, out string strMsg)
        {
            strMsg = "";
            return hldMainboard.GetLockAllStatus(nSide, ref strMsg);
        }
        public int[] GetAllSensorStatus(int nSide, out string strMsg)
        {
            strMsg = "";
            return hldMainboard.GetSensorAllStatus(nSide, ref strMsg);
        }

        public int GetLockStatus(int nSide, int nLockID, out string strMsg)
        {
            strMsg = "";
            return hldMainboard.GetLockStatus(nSide, nLockID, ref strMsg);
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="port">串口名称</param>
        /// <param name="baudrate">波特率</param>
        /// <param name="strMsg">错误信息</param>
        /// <returns></returns>
        public bool Open(string port, int baudrate, out string strMsg)
        {
            strMsg = "";
            _curPort = port;
            bool result = hldMainboard.OpenSerialPort(port, baudrate, ref strMsg);
            return result;
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <param name="strMsg">错误信息</param>
        /// <returns></returns>
        public bool Close(out string strMsg)
        {
            strMsg = "";
            bool result = hldMainboard.CloseSerialPort(ref strMsg);
            return result;
        }

        public bool OpenLock(int nSide, int nLockID, out string strMsg)
        {
            bool result = false;
            strMsg = "";
            int ret = hldMainboard.OpenLock(nSide, nLockID, ref strMsg);
            if (ret == 0)
            {   //Command submit succeed,check lock status
                DateTime t = DateTime.Now;
                while ((DateTime.Now - t).TotalMilliseconds < CCommon.timeoutUnlock)
                {
                    Thread.Sleep(100);
                    if (GetRelayStatus(nSide, nLockID, out strMsg))
                    {
                        result = true;
                        break;
                    }
                    //lock haven't Open
                }
            }
            else
            {
                Thread.Sleep(400);
            }
            return result;
        }

        //Get Relay Status
        private bool GetRelayStatus(int nSide, int nLockID, out string strMsg)
        {
            bool result = false;
            int index = nLockID - 1;
            strMsg = "";
            int lockStatus = hldMainboard.GetLockStatus(nSide, nLockID, ref strMsg);        //Get new lock status of the relay
            if (lockStatus == 0)
                result = true;
            return result;
        }

        /// <summary>
        /// 取设备掉电状态
        /// </summary>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public int GetPowerStatus(out string strMsg)
        {
            int result = 0;
            strMsg = "";
            result = hldMainboard.GetPowerStatus(ref strMsg);
            return result;
        }

        /// <summary>
        /// 取设备版本号
        /// </summary>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public string GetVersion(out string strMsg)
        {
            string result = "";
            strMsg = "";
            result = hldMainboard.GetVersion(ref strMsg);
            return result;

        }

        /// <summary>
        /// 取IC卡号
        /// </summary>
        /// <returns></returns>
        public string GetICCardData()
        {
            string result = "";
            result = hldMainboard.GetICCardData();
            return result;
        }

        /// <summary>
        /// 取扫描的条码
        /// </summary>
        /// <returns></returns>
        public string GetCoderTextData()
        {
            string result = "";
            result = hldMainboard.GetCoderTextData();
            return result;
        }

        /// <summary>
        /// 扫描器唤醒
        /// </summary>
        /// <returns></returns>
        public int SetCoderWakeup(out string strMsg)
        {
            int result = 0;
            strMsg = "";
            result = hldMainboard.SetCoderWakeup(ref strMsg); ;
            if (result == 0)
                _scannerOn = true;
            return result;
        }

        /// <summary>
        /// 扫描器休眠
        /// </summary>
        /// <returns></returns>
        public int SetCoderSleep(out string strMsg)
        {
            int result = 0;
            strMsg = "";
            object[] args = new object[] { strMsg };
            result = hldMainboard.SetCoderSleep(ref strMsg);
            if (result == 0)
                _scannerOn = false;
            return result;
        }
    }
}
