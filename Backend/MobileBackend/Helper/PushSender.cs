using System.IO;
using System.Net;
using System.Text;
using System;
using NLog;
using Contracts;

namespace Mobile_Backend.Helper
{
    public class PushSender : IPushSender
    {
        private ILogger Logger = LogManager.GetCurrentClassLogger();

        public void SendGroupPush(long groupId, long entryId, string text)
        {
            SendPush(
                $"{{\"field\":\"tag\",\"key\":\"Group{groupId}\",\"relation\":\"exists\"}},"
                + $"{{\"field\":\"tag\",\"key\":\"Group{groupId}\",\"relation\":\"=\",\"value\":\"true\"}}",
                $"{{\"GroupId\":{groupId},\"EntryId\":{entryId}}}",
                text,
                "Neue Gruppennachricht");
        }

        public void SendSubGroupPush(long groupId, long entryId, string text)
        {
            SendPush(
                $"{{\"field\":\"tag\",\"key\":\"SubGroup{groupId}\",\"relation\":\"exists\"}},"
                + $"{{\"field\":\"tag\",\"key\":\"SubGroup{groupId}\",\"relation\":\"=\",\"value\":\"true\"}}",
                $"{{\"SubGroupId\":{groupId},\"EntryId\":{entryId}}}",
                text,
                "Neue Untergruppennachricht");
        }

        private void SendPush(string filters, string data, string text, string title)
        {
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            request.Headers.Add("authorization", "Basic OGQ0OWMzYjYtZTNhNC00MDk4LWE4ZjEtYjk5Nzg3NWZiNTAx");

            byte[] byteArray = Encoding.UTF8.GetBytes("{"
                                                    + "\"app_id\": \"71f3050c-2a04-4d17-81ec-33f76361bf19\","
                                                    + $"\"headings\":{{\"de\":\"{title}\"}},"
                                                    + $"\"contents\":{{\"de\": \"{text}\"}},"
                                                    + $"\"data\":{{{data}}},"
                                                    + "\"included_segments\": [\"All\"],"
                                                    + $"\"filters\": [{filters}]}}");

            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                Logger.Error(ex.Message);
                Logger.Error(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            Logger.Info(responseContent);
        }
    }
}
