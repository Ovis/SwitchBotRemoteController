using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwitchBotRemoteController.Extensions;
using SwitchBotRemoteController.Options;

namespace SwitchBotRemoteController
{
    public class SwitchBotClient(
        ILogger<SwitchBotClient> logger,
        IOptions<SwitchBotOption> option,
        IHttpClientFactory httpClientFactory)
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient();

        private readonly SwitchBotOption _option = option.Value;

        private const string BaseUri = "https://api.switch-bot.com/v1.1";


        /// <summary>
        /// デバイス一覧取得
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<(bool IsSuccess, string Json, Exception? Error)> GetDevices(CancellationToken cancellationToken = default)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{BaseUri}/devices");
            AddAuthHeaders(requestMessage);

            try
            {
                var response = await _client.SendAsync(requestMessage, cancellationToken);
                response.EnsureSuccessStatusCode();

                return (true, (await response.Content.ReadAsStringAsync(cancellationToken)).JsonFormatting(), default);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occurred while getting devices.");
                return (false,string.Empty, e);
            }
        }


        /// <summary>
        /// デバイス状態取得
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<(bool IsSuccess, string Json, Exception? Error)> GetDeviceStatus(string deviceId, CancellationToken cancellationToken = default)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{BaseUri}/devices/{deviceId}/status");
            AddAuthHeaders(requestMessage);

            try
            {
                var response = await _client.SendAsync(requestMessage, cancellationToken);
                response.EnsureSuccessStatusCode();

                return (true, (await response.Content.ReadAsStringAsync(cancellationToken)).JsonFormatting(), default);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occurred while getting device status.");
                return (false, string.Empty, e);
            }
        }

        
        /// <summary>
        /// シーン取得
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<(bool IsSuccess, string Json, Exception? Error)> GetScenes(CancellationToken cancellationToken = default)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{BaseUri}/scenes");
            AddAuthHeaders(requestMessage);

            try
            {
                var response = await _client.SendAsync(requestMessage, cancellationToken);
                response.EnsureSuccessStatusCode();

                return (true, (await response.Content.ReadAsStringAsync(cancellationToken)).JsonFormatting(), default);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get scenes.");
                return (false, string.Empty, e);
            }
        }


        /// <summary>
        /// コマンド実行
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="requestBody"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<(bool IsSuccess, string Json, Exception? Error)> Commands(string deviceId, string requestBody, CancellationToken cancellationToken = default)
        {
            // 10回試してダメならエラーを返す
            var failedCount = 0;

            while (true)
            {
                if (failedCount >= 10)
                {
                    return (false, string.Empty, new Exception("Failed to send command."));
                }

                var (isSuccess, json, error) = await CommandsInternal(deviceId, requestBody, cancellationToken);

                if (isSuccess)
                {
                    return (isSuccess, json, error);
                }
                else
                {
                    if (error is HttpRequestException)
                    {
                        failedCount++;
                        await Task.Delay(1000, cancellationToken);
                    }
                    else
                    {
                        return (isSuccess, json, error);
                    }
                }
            }
        }


        /// <summary>
        /// コマンド実行内部処理
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="requestBody"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async ValueTask<(bool IsSuccess, string Json, Exception? Error)> CommandsInternal(string deviceId, string requestBody, CancellationToken cancellationToken = default)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{BaseUri}/devices/{deviceId}/commands");
            AddAuthHeaders(requestMessage);

            requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.SendAsync(requestMessage, cancellationToken);
                response.EnsureSuccessStatusCode();

                return (true, (await response.Content.ReadAsStringAsync(cancellationToken)).JsonFormatting(), default);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send command.");
                return (false, string.Empty, e);
            }
        }



        /// <summary>
        /// 認証ヘッダー生成
        /// </summary>
        /// <param name="requestMessage"></param>
        private void AddAuthHeaders(HttpRequestMessage requestMessage)
        {
            var unixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var nonce = Guid.NewGuid().ToString();
            var data = _option.Token + unixTime + nonce;
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_option.Secret));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));

            requestMessage.Headers.TryAddWithoutValidation(@"Authorization", _option.Token);
            requestMessage.Headers.TryAddWithoutValidation(@"sign", signature);
            requestMessage.Headers.TryAddWithoutValidation(@"nonce", nonce);
            requestMessage.Headers.TryAddWithoutValidation(@"t", unixTime.ToString());
        }
    }
}
