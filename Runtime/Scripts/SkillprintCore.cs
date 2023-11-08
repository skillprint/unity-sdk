using System;
using System.Collections;
using System.Collections.Generic;
// FIXME this reference works in the Unity editor but Rider can't resolve it
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_2018_4_OR_NEWER
using UnityEngine.Networking;
#endif

// [TODO]: Standardize this to support multiple ad slot sizes
static class SkillprintConstants
{
    public const int MOBILE_LEADERBOARD_HEIGHT = 50;
    public const int MOBILE_LEADERBOARD_WIDTH = 320;
    public const int INLINE_RECTANGLE_HEIGHT = 250;
    public const int INLINE_RECTANGLE_WIDTH = 300;
}

static class SkillprintEvents
{
    public const string GAME_START = "GAME_START";
    public const string GAME_END = "GAME_END";
}


public class SkillprintCore : MonoBehaviour
{
    // No matter what the APIHost value is here the React UI will look up PROD
    public string APIHost = "https://api.skillprint.co";
    public string AppHost = "https://app.skillprint.co"; // Change it to staging to test staging changes
    public string ResultUrl = "/session-score";
    public string GameId = null;
    public string WebViewAdPosition = "Middle";
    public string deeplinkURL;
    private WebViewObject _webViewObject;
    private int _windowHeight;
    private int _windowWidth;
    private int _webViewHeight = SkillprintConstants.INLINE_RECTANGLE_HEIGHT;
    private int _webViewWidth = SkillprintConstants.INLINE_RECTANGLE_WIDTH;
    private string _redirectPath = null;
    private bool isLoggedIn = false;

    
    private class Session
    {
        // ID is set automatically when an instance is constructed so it is read-only
        public string Id { get; }
        
        // once closed, a session cannot be re-opened, so we only provide a
        //  public Close method instead of a public setter for IsClosed
        public bool IsClosed { get; private set; }
        
        public Session()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public void Close()
        {
            IsClosed = true;
        }
    
    }
    
    private Session _session = null;

    IEnumerator Start()
    {
        Init();
        yield break;
    }

    public void Init(string playerId)
    {
        _persistPlayerId(playerId);
        Debug.Log($"[Start Skillprint Core]: Starting Skillprint Core");
        _webViewObject = new GameObject("WebViewObject").AddComponent<WebViewObject>();
        _webViewObject.Init(
            cb: (msg) =>
            {
                Debug.Log($"CallFromJS[{msg}]");

                if (msg == "Close WebView")
                {
                    HideWebViewContent();
                }

                if (msg == "Open WebView")
                {
                    SetMarginsWebView();
                    _webViewObject.SetVisibility(true);
                }

                if (msg == "Navigate to Skillprint")
                {
                    string LoginUrl = $"{AppHost}/auth?redirectHost=token&redirectScheme={GameId}";

                    Debug.Log($"Loggin in to skillprint: {LoginUrl}");
                    Application.OpenURL(LoginUrl);
                }
                
                if (msg == "Logged In" && _redirectPath != null)
                {
                    isLoggedIn = true;
                    string redirectScript = "window.location.replace('" + _redirectPath  + "');";
                    Debug.Log($"Attempting to redirect to: {_redirectPath}");
                    _webViewObject.EvaluateJS(redirectScript);
                }
            },
            err: (msg) =>
            {
                Debug.Log($"CallOnError[{msg}]");
            },
            httpErr: (msg) =>
            {
                Debug.Log($"CallOnHttpError[{msg}]");
                if (msg.Contains("403") || msg == "403")
                {
                    // Invalid login most likely due an ivalid token so set the token to null
                    // to avoid infinite loop
                    Debug.Log($"Resetting token since token invalid");
                    ResetBearerToken();
                }
            },
            started: (msg) =>
            {
                Debug.Log($"CallOnStarted[{msg}, {_windowWidth == 0}, {_windowHeight}]");
            },
            hooked: (msg) =>
            {
                Debug.Log($"CallOnHooked[{msg}]");
            },
            ld: (msg) =>
            {
                Debug.Log($"CallOnLoaded[{msg}]");
                /*
                 * Attempt to Login when a page is loaded and it in the skillprint domain
                 * The javascript below ensures that they we do not log in if already logged in
                 * In the react app we have defined a global variable `isLoggedIn` indicating whether
                 * the user is already logged in
                 */

                string _bearerToken = GetBearerToken();

                if (!string.IsNullOrEmpty(_bearerToken))
                {
                    string arguments = $"'{_bearerToken}', '{GameId}', '{_session.Id}'";
                    if (isLoggedIn)
                    {
                        arguments = $"'{_bearerToken}'";
                    }
                    string loginScript = "if(!window.isLoggedIn && window.location.href.includes('skillprint') && window.ExternalLogin) {"
                        + "window?.ExternalLogin(" + arguments + ");"
                        + "}";
                    _webViewObject.EvaluateJS(loginScript);
                    Debug.Log($"Attempting to login with this script: {loginScript}");
                }

            },
            enableWKWebView: true
            );
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        _webViewObject.bitmapRefreshCycle = 1;
#endif

#if UNITY_EDITOR
        int width = Convert.ToInt32(0.88 * Screen.width);
        int height = Convert.ToInt32(_webViewHeight * width / _webViewWidth);
        SetWindowWidthHeight(width, height);
#elif UNITY_ANDROID
        float scaledScreenSize = GetAndroidScreenSize();
        int width = Convert.ToInt32(Screen.width / scaledScreenSize * _webViewWidth);
        int height = Convert.ToInt32(Screen.width / scaledScreenSize * _webViewHeight);
        SetWindowWidthHeight(width, height);
        Debug.Log($"Call From Android native Density: {scaledScreenSize} ");
#else
// Hard coding the density to 2 as of now
        int width = Convert.ToInt32(2 * _webViewWidth);
        int height = Convert.ToInt32(2 * _webViewHeight);
        SetWindowWidthHeight(width, height);
        Debug.Log($"Call From iOS native Density: 2 ");

#endif
        SetMarginsWebView();
        _webViewObject.SetTextZoom(100);
        _webViewObject.SetVisibility(false);

    }

    private void OnApplicationPause(bool pause)
    {
        Debug.Log($"In [Application Pause]: {pause} of SkillprintCore: {Application.absoluteURL}");
        Debug.Log($"In [Application Pause]: {pause} of SkillprintCore: Current Bearer Token: {GetBearerToken()}");
        if (!pause)
        {
            ProcessDeepLink(Application.absoluteURL);
        }
    }

    private string ParseTokenFromDeepLink(string url)
    {
        string token = null;
        Debug.Log($"Received Deep Link Url: {url}");
        bool hasToken = url.Contains("token?");
        if (hasToken)
        {
            token = url.Split("?")[1];
        }
        Debug.Log($"Parsed Deep Link Url to Token: {token}");
        return token;
    }

    private void SetBearerToken(string token)
    {
        string currentToken = GetBearerToken();
        Debug.Log($"Starting the SetBearerToken: Parsed Token: {token} Existing Token: {currentToken}");
        // Only set token if there are no current token set
        if (string.IsNullOrEmpty(currentToken))
        {
            PlayerPrefs.SetString("bearer_token", token);
            PlayerPrefs.Save();
            Debug.Log($"Parsed Bearer Token: {token}");
            Debug.Log($"Setting Bearer Token: {GetBearerToken()}");

        }
    }

    private void ResetBearerToken()
    {
        PlayerPrefs.SetString("bearer_token", null);
        PlayerPrefs.Save();
    }

    private string GetBearerToken()
    {
        return PlayerPrefs.GetString("bearer_token");
    }

    private void ProcessDeepLink(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            string token = ParseTokenFromDeepLink(url);
            if (!string.IsNullOrEmpty(token))
            {
                SetBearerToken(token);
                Debug.Log($"Since a deep link request was received with Token: {GetBearerToken()}. Logging In");
                _webViewObject.Reload();
            }
        }
           
    }

    private float GetAndroidScreenSize()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject currentResources = currentActivity.Call<AndroidJavaObject>("getResources");
        AndroidJavaObject displayMetrics = currentResources.Call<AndroidJavaObject>("getDisplayMetrics");
        float density = displayMetrics.Get<float>("density");
        int widthPixels = displayMetrics.Get<int>("widthPixels");

        float scaledScreenSize = widthPixels / density;

        return scaledScreenSize;
    }
    
    private void SetWindowWidthHeight(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }


    private void SetMarginsWebView()
    {

        int horizontal = (Screen.width - _windowWidth) / 2;
        int bottom = 0;
        int top = 0;
        if (WebViewAdPosition == "Bottom")
        {
            bottom = 10;
            top = Screen.height - _windowHeight - bottom;
        } else if (WebViewAdPosition == "Middle") {
            bottom = (Screen.height - _windowHeight) / 2;
            top = (Screen.height - _windowHeight) / 2;
        } else if (WebViewAdPosition == "Top") {
            bottom = Screen.height - _windowHeight - bottom;
            top = 10;
        }

        _webViewObject.SetMargins(horizontal, top, horizontal, bottom);
    }

    private void SetFullScreen()
    {
        _webViewObject.SetMargins(100, 0, 100, 0);
    }

    private void ShowWebView()
    {
        _webViewObject.LoadURL(
            AppHost
            + ResultUrl
            + $"?slug={GameId}&sessionId={_session.Id}&type=InlineRectangle"
        );
        // Notice that the display true is not set in this function
        // That is because the expectation is the final UI rendering JS will call 'Open WebView'
        // To render the webview
    }
    
    private IEnumerator _postDataNative(string url, string payload)
    {
        Debug.Log($"Posting Data Natively: {url} {payload}");
        byte[] rawData = System.Text.Encoding.UTF8.GetBytes(payload);
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, payload))
        {
            string _bearerToken = GetBearerToken();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(_bearerToken))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
            }
            webRequest.uploadHandler = new UploadHandlerRaw(rawData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Native WebRequest Error: {webRequest.url} {webRequest.error}");
            }
            else
            {
                Debug.Log("Native Event Posted Successfully");
            }
            webRequest.Dispose();
        }
    }
    
    /// <summary>
    /// Sets required event properties and flattens any additional parameters provided 
    /// </summary>
    /// <param name="eventName">The string name of the event</param>
    /// <param name="eventParams">An optional IDictionary of parameters with string keys and dynamic values</param>
    /// <returns>JSON-encoded object with `event`, `timestamp`, and any extra params flattened into it</returns>
    private static string _prepareEventString(string eventName, IDictionary<string, dynamic> eventParams)
    {
        eventParams.Add("event", eventName);
        // "o" format is ISO 8601 with fractional seconds, so it's sortable and maintains full precision
        eventParams.Add("timestamp", DateTime.Now.ToUniversalTime().ToString("o"));
        return JsonConvert.SerializeObject(eventParams);
    }

    /// <summary>
    /// Returns the full URL (scheme/host/path) to use when sending event messages  
    /// </summary>
    private string _getLogEventUrl()
    {
        return $"{APIHost}/v2/games/{GameId}/log-event/{_session.Id}/";
    }

    private string _generateNewPlayerId()
    {
        return Guid.NewGuid().ToString();
    }

    private void _setPlayerId(string playerId)
    {
        PlayerPrefs.SetString("player_id", playerId);
        PlayerPrefs.Save();
    }

    private void _getPlayerId()
    {
        return PlayerPrefs.GetString("player_id");
    }

    /// <summary>
    /// Set the player id provided by the developer if different than
    /// currently set player id. In case, player id is not provided by
    /// the developer, generate a uuid and store it
    /// </summary>
    /// <param name="playerId">The optional string name of the player id</param>
    private void _persistPlayerId(string playerId)
    {
        string currentPlayerId = _getPlayerId();
        if (playerId)
        {
            if (currentPlayerId !== playerId) {
                _setPlayerId(playerId);
            }
        }
        else
        {
            if (!currentPlayerId)
            {
                _setPlayerId(_generateNewPlayerId());
            }
        }
    }
    
    private IEnumerator _sendEventNativeCoroutine(string eventName, IDictionary<string, dynamic> eventParams = null)
    {
        eventParams ??= new Dictionary<string, dynamic>();
        string payload = _prepareEventString(eventName, eventParams);
        string url = _getLogEventUrl();
        if (_session is null)
        {
            Debug.LogWarning($"Attempted to write data before initializing Session: {url}, {payload}");
            yield break;
        }
        if (_session.IsClosed)
        {
            Debug.LogWarning($"Attempted to write data into a closed Session: {url}, {payload}");
            yield break;
        }
        yield return _postDataNative(url, payload);
    }

    public void SendEvent(string eventName, IDictionary<string, dynamic> eventParams = null)
    {
        eventParams ??= new Dictionary<string, dynamic>();
        string payload = _prepareEventString(eventName, eventParams);
        string url = _getLogEventUrl();
        string jsCommand = $"window?.postData?.(\"{url}\", {payload})";
        Debug.Log($"Calling the skillprint SDK JS Command: {jsCommand}");
        _webViewObject.EvaluateJS(jsCommand);
    }

    public void SendEventNative(string eventName, IDictionary<string, dynamic> eventParams = null)
    {
        StartCoroutine(_sendEventNativeCoroutine(eventName, eventParams));
    }

    public void GameSessionStartNative()
    {
        _session = new Session();
        SendEventNative(SkillprintEvents.GAME_START);
    }

    public void GameSessionEndNative()
    {
        if (!_session.IsClosed)
        {
            SendEventNative(SkillprintEvents.GAME_END);
            _session.Close();
        }
    }

    public void ShowWebViewContentNative()
    {
        GameSessionEndNative();
        ShowWebView();
    }

    public void HideWebViewContent()
    {
        _webViewObject.SetVisibility(false);
    }

    public void Login(string redirectPath)
    {
        Debug.Log($"In Login Function: {GetBearerToken()}");
        _webViewObject.Reload();
        // Optional
        _redirectPath = redirectPath;
    }

    public void Logout()
    {
        SetBearerToken(null);
        Debug.Log($"In Logout Function: {GetBearerToken()}");
        _webViewObject.SetVisibility(false);
        _webViewObject.Reload();
    }

}
