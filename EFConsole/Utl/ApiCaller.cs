using System;
using System.Net.Http;

namespace OrderHelperLib.Utl
{
    public class ApiCaller
    {
        private string _accessToken;
        private string ServerUrl { get; }

        private string AccessToken
        {
            get
            {
                if (string.IsNullOrEmpty(_accessToken))
                    throw new Exception("Access token is null or empty");
                return _accessToken;
            }
        }

        public ApiCaller(string serverUrl)
        {
            ServerUrl = serverUrl;
        }

        public void RegAccessToken(string accessToken) => _accessToken = accessToken;

        public void Call(string method, Action<string> onSuccessAction, Action<string> onFailedAction) => Call(method,
            string.Empty, onSuccessAction, onFailedAction, true, Array.Empty<(string, string)>());

        public void Call(string method, string content, Action<string> onSuccessAction,
            Action<string> onFailedAction) =>
            Call(method, content, onSuccessAction, onFailedAction, true, Array.Empty<(string, string)>());

        public void CallWithoutToken(string method, string content, Action<string> onSuccessAction,
            Action<string> onFailedAction) =>
            Call(method, content, onSuccessAction, onFailedAction, false, Array.Empty<(string, string)>());

        public async void RefreshTokenCall(string method, string refreshToken, string content,
            Action<string> onSuccessAction,
            Action<string> onFailedAction)
        {
            var (isSuccess, response) =
                await Http.SendRequestAsync(ServerUrl + method, HttpMethod.Post, content, refreshToken);
            if (!isSuccess)
            {
                onFailedAction?.Invoke(response);
                return;
            }

            onSuccessAction?.Invoke(response);
        }

        public async void Call(string method, string content,
            Action<string> onSuccessAction,
            Action<string> onFailedAction,
            bool isNeedAccessToken = true,
            params (string, string)[] queryParams)
        {
            var (isSuccess, response) =
                await Http.SendRequestAsync(ServerUrl + method, HttpMethod.Post, content,
                    isNeedAccessToken ? AccessToken : null, queryParams);
            if (!isSuccess)
            {
                onFailedAction?.Invoke(response);
                return;
            }

            onSuccessAction?.Invoke(response);
        }
    }
}