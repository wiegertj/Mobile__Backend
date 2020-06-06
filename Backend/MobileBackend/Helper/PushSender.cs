﻿using System.IO;
using System.Net;
using System.Text;
using System;
using NLog;
using Contracts;
using System.Collections.Generic;
using Entities.Models;

namespace Mobile_Backend.Helper
{
    public class PushSender : IPushSender
    {
        private ILogger Logger = LogManager.GetCurrentClassLogger();
        private IRepositoryWrapper _repository;

        public PushSender(IRepositoryWrapper repository)
        {
            _repository = repository;
        }

        public void SendGroupPush(long groupId, long entryId, string text, long userId)
        {
            var group = _repository.Group.GetGroupById(groupId);
            IEnumerable<User> groupUsers = _repository.UserToGroup.GetMembersForGroup(group);

            SendPush(
                groupUsers,
                $"{{\"GroupId\":{groupId},\"EntryId\":{entryId}}}",
                text,
                $"Neue Gruppennachricht in {group.Name}",
                userId);
        }

        public void SendSubGroupPush(long groupId, long entryId, string text, long userId)
        {
            var group = _repository.Subgroup.GetSubgroupById(groupId);
            IEnumerable<User> subGroupUsers = _repository.UserToSubgroup.GetMembersForSubgroup(group);

            SendPush(
                subGroupUsers,
                $"{{\"SubGroupId\":{groupId},\"EntryId\":{entryId}}}",
                text,
                $"Neue Untergruppennachricht in {group.Name}",
                userId);
        }

        private void SendPush(IEnumerable<User> users, string data, string text, string title, long userId)
        {
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            request.Headers.Add("authorization", "Basic OGQ0OWMzYjYtZTNhNC00MDk4LWE4ZjEtYjk5Nzg3NWZiNTAx");

            string filter = "";
            foreach (User user in users)
            {
                if (user.Id.Equals(userId))
                    continue;
                if (filter != "")
                {
                    filter += ",{\"operator\": \"OR\"},";
                }

                filter += $"{{\"field\":\"tag\",\"key\":\"user\",\"relation\":\"=\",\"value\":\"{user.Email}\"}}";
            }

            string body = "{"
                + "\"app_id\": \"71f3050c-2a04-4d17-81ec-33f76361bf19\","
                + $"\"headings\":{{\"en\":\"{title}\"}},"
                + $"\"contents\":{{\"en\": \"{text}\"}},"
                + $"\"data\":{data},"
                + $"\"filters\": [{filter}]}}";
            byte[] byteArray = Encoding.UTF8.GetBytes(body);

            string responseContent = null;

            Logger.Info(body);

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
