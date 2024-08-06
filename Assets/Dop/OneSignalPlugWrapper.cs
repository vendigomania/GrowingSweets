using UnityEngine;
using OneSignalSDK;

public class OneSignalPlugWrapper : MonoBehaviour
{
    public static string UserIdentificator => OneSignal.Default?.User?.OneSignalId;

    public static void InitializeNotifications()
    {
        OneSignal.Initialize("7d56068b-fcb1-464f-949c-757d75a51045");
    }

    public static void SubscribeOff()
    {
        OneSignal.Notifications?.ClearAllNotifications();
        OneSignal.Logout();
    }
}
