using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace SwitchBotRemoteController
{
    public class SwitchBotApiFunctions(
        SwitchBotClient client)
    {
        /// <summary>
        /// デバイス一覧取得
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("GetDevices")]
        public async ValueTask<IActionResult> GetDevicesAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices")] HttpRequest req)
        {
            var (isSuccess, json, error) = await client.GetDevices();

            if (!isSuccess)
            {
                return new BadRequestObjectResult(error);
            }
            else
            {
                return new OkObjectResult(json);
            }
        }


        /// <summary>
        /// デバイス状態取得
        /// </summary>
        /// <param name="req"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Function("GetDeviceStatus")]
        public async ValueTask<IActionResult> GetDeviceStatusAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices/{deviceId}/status")]
            HttpRequest req, string deviceId)
        {
            var (isSuccess, json, error) = await client.GetDeviceStatus(deviceId);

            if (!isSuccess)
            {
                return new BadRequestObjectResult(error);
            }
            else
            {
                return new OkObjectResult(json);
            }
        }


        /// <summary>
        /// シーン情報取得
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("GetScenes")]
        public async ValueTask<IActionResult> GetScenesAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "scenes")]
            HttpRequest req)
        {
            var (isSuccess, json, error) = await client.GetScenes();

            if (!isSuccess)
            {
                return new BadRequestObjectResult(error);
            }
            else
            {
                return new OkObjectResult(json);
            }
        }
        

        /// <summary>
        /// コマンド実行
        /// </summary>
        /// <param name="req"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Function("Commands")]
        public async ValueTask<IActionResult> CommandsAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/{deviceId}/commands")]
            HttpRequest req, string deviceId)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                JsonDocument.Parse(requestBody);
            }
            catch(Exception e)
            {
                return new BadRequestObjectResult(e);
            }

            var (isSuccess, json, error) = await client.Commands(deviceId, requestBody);

            if (!isSuccess)
            {
                return new BadRequestObjectResult(error);
            }
            else
            {
                return new OkObjectResult(json);
            }
        }
    }
}
