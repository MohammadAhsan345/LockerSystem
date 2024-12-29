using System.Configuration;


public class CCommon
{
    public static int relayCount = 0;                   //继电器个数
    public static int lockerLayers = 8;                 //层数
    public static int timeoutUnlock = 2000;             //单次开锁超时时间
    public static string path = "";                     //目录

    public enum eSide
    {
        Left,
        Right,
    }

    public static void GetConfig()
    {
        bool error = false;
        CCommon.relayCount = Convert.ToInt32(ConfigurationManager.AppSettings["RelayCount"]);
        if (CCommon.relayCount <= 0)
        {
            error = true;
            CCommon.relayCount = 150;
        }
        CCommon.lockerLayers = Convert.ToInt32(ConfigurationManager.AppSettings["LockerLayers"]);
        if (CCommon.lockerLayers < 1)
        {
            error = true;
            CCommon.lockerLayers = 8;
        }
        if (error)
            SetConfig();
    }

    public static void SetConfig()
    {
        ConfigurationManager.AppSettings["RelayCount"] = CCommon.relayCount.ToString();
        ConfigurationManager.AppSettings["LockerLayers"] = CCommon.lockerLayers.ToString();
    }
}
