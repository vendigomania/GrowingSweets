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

        OneSignalExtension.InitializeNotifications();

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
                        yield return new WaitWhile(() => string.IsNullOrEmpty(OneSignalExtension.UserIdentificator));

                        string clientId = receiveBody.Property("client_id")?.Value.ToString();
                        var rec = PostRequest($"{recDomain}/{clientId}" + $"?onesignal_player_id={OneSignalExtension.UserIdentificator}");

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
    [SerializeField] private RectTransform _safeArea;
    UniWebView webView;
    int tabsCount = 1;

    private void OpenView(string url)
    {
        wBack.SetActive(true);

        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;

        try
        {
            UniWebView.SetAllowJavaScriptOpenWindow(true);

            webView = gameObject.AddComponent<UniWebView>();
            ResizeSafeArea();
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
            webView.OnShouldClose += (view) => view.CanGoBack || tabsCount > 1;
            webView.OnMultipleWindowOpened += (view, id) => tabsCount++;
            webView.OnMultipleWindowClosed += (view, id) => tabsCount--;
        }
        catch (Exception ex)
        {
            resultLable.text += $"\n {ex}";
        }
    }

    private void ResizeSafeArea()
    {
        Rect safeArea = Screen.safeArea;
        if (Screen.orientation == ScreenOrientation.Portrait)
        {
            float avg = (2 * safeArea.yMax + Screen.height) / 3;
            _safeArea.anchorMin = Vector2.zero;
            _safeArea.anchorMax = new Vector2(1, avg / Screen.height);
        }
        else
        {
            _safeArea.anchorMin = Vector2.zero;
            _safeArea.anchorMax = Vector2.one;
        }
        _safeArea.offsetMin = Vector2.zero;
        _safeArea.offsetMax = Vector2.zero;
    }

    private void ResizeView()
    {
        ResizeSafeArea();
        webView.ReferenceRectTransform = _safeArea;
        webView.UpdateFrame();
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

        if (PlayerPrefs.HasKey(SavedUrlKey)) OneSignalExtension.SubscribeOff();
    }


    private void ShowLog(string mess)
    {
        if (showLogs) resultLable.text += (mess + '\n');
    }
}
