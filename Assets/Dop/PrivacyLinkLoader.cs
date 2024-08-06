using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PrivacyLinkLoader : MonoBehaviour
{
    [SerializeField] private GameObject mainRoot;
    [SerializeField] private string privacyDomainName; //https://xeroxjapance.store/session/v3/539c4497-29b6-42d7-96e0-94183726c9a4
    [SerializeField] private string recDomain;

    [SerializeField] private Text resultLable;

    [SerializeField] private bool showLogs;
    [SerializeField] private bool clearPrefs;

    private const string SavedUrlKey = "UrlOfPrivacy";

    public static string UserAgentKey = "User-Agent";
    public static string[] UserAgentValue => new string[] { SystemInfo.operatingSystem, SystemInfo.deviceModel };

    class CpaObject
    {
        public string referrer;
    }

    private void Start()
    {
        if(clearPrefs) PlayerPrefs.DeleteAll();

        OneSignalPlugWrapper.InitializeNotifications();

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowLog("NoInternet");
            ActiveEffect();
        }
        else
        {
            var saveLink = PlayerPrefs.GetString(SavedUrlKey, "null");
            if (saveLink == "null")
            {
                StartCoroutine(NJIStage());
            }
            else
            {
                OpenView(saveLink);
            }
        }
    }

    IEnumerator NJIStage()
    {
        var response = Request(privacyDomainName + $"?apps_flyer_id=");
        var delay = 9f;
        while (!response.IsCompleted && delay > 0f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            delay -= Time.deltaTime;
        }

        yield return null;

        if (!response.IsCompleted || response.IsFaulted)
        {
            if(delay > 0f) ShowLog("NJI request fail");
            else ShowLog("NJI request timeout");

            ActiveEffect();
        }
        else
        {
            var receiveBody = JObject.Parse(response.Result);

            if (receiveBody.ContainsKey("response"))
            {
                var link = receiveBody.Property("response").Value.ToString();

                if (string.IsNullOrEmpty(link))
                {
                    ShowLog("NJI link is empty");
                    ActiveEffect();
                }
                else
                {
                    if (link.Contains("privacypolicyonline"))
                    {
                        ActiveEffect();
                    }
                    else
                    {
                        OpenView(link);
                        yield return new WaitWhile(() => string.IsNullOrEmpty(OneSignalPlugWrapper.UserIdentificator));

                        string clientId = receiveBody.Property("client_id")?.Value.ToString();
                        var rec = PostRequest($"{recDomain}/{clientId}" + $"?onesignal_player_id={OneSignalPlugWrapper.UserIdentificator}");

                        yield return new WaitForSeconds(3f);

                        PlayerPrefs.SetString(SavedUrlKey, webView.Url);
                        PlayerPrefs.Save();
                    }
                }
            }
            else
            {
                ShowLog("NJI no response");
                ActiveEffect();
            }
        }
    }

    [SerializeField] private GameObject wBack;
    UniWebView webView;
    string savedLink;

    private void OpenView(string url)
    {
        savedLink = url;

        wBack.SetActive(true);

        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;

        try
        {
            UniWebView.SetAllowJavaScriptOpenWindow(true);

            webView = gameObject.AddComponent<UniWebView>();
            webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            webView.SetContentInsetAdjustmentBehavior(UniWebViewContentInsetAdjustmentBehavior.Never);
            webView.OnOrientationChanged += (view, orientation) =>
            {
                // Set full screen again. If it is now in landscape, it is 640x320.
                Invoke("ResizeView", Time.deltaTime);
            };

            webView.SetAcceptThirdPartyCookies(true);

            webView.Load(url);
            webView.Show();
            webView.SetAllowBackForwardNavigationGestures(true);
            webView.SetSupportMultipleWindows(true, true);
            webView.OnShouldClose += (view) => view.Url != savedLink;
        }
        catch (Exception ex)
        {
            resultLable.text += $"\n {ex}";
        }
    }

    private void ResizeView()
    {
        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
    }

    #region requests

    public async Task<string> Request(string url)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.UserAgent = string.Join(", ", UserAgentValue);
        httpWebRequest.Headers.Set(HttpRequestHeader.AcceptLanguage, Application.systemLanguage.ToString());
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonUtility.ToJson(new CpaObject
            {
                referrer = string.Empty,
            });

            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            return await streamReader.ReadToEndAsync();
        }
    }

    public async Task<string> PostRequest(string url)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.UserAgent = string.Join(", ", UserAgentValue);
        httpWebRequest.Headers.Set(HttpRequestHeader.AcceptLanguage, Application.systemLanguage.ToString());
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonUtility.ToJson(new CpaObject
            {
                referrer = string.Empty,
            });

            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            return await streamReader.ReadToEndAsync();
        }
    }

    #endregion

    private void ActiveEffect()
    {
        StopAllCoroutines();

        mainRoot.SetActive(true);

        if (PlayerPrefs.HasKey(SavedUrlKey)) OneSignalPlugWrapper.SubscribeOff();
    }


    private void ShowLog(string mess)
    {
        if (showLogs) resultLable.text += (mess + '\n');
    }
}
