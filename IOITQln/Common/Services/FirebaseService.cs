using FcmSharp;
using FcmSharp.Requests;
using FcmSharp.Settings;
using IOITQln.Common.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IOITQln.Common.Services
{
    public class FirebaseService : IFirebaseService
    {
        private static readonly ILog log = LogMaster.GetLogger("FirebaseService", "FirebaseService");

        public FirebaseService ()
        {

        }

        public async Task sendNotification(string tokenKey, long? userId, int badge, int badgeTotal, string name, string contents, bool notificationSound, string returnUrl, string domain)
        {
            string fileName = "serviceAccountKey.json";
            string webRootPath = Directory.GetCurrentDirectory();
            string configPath = Path.Combine(webRootPath, fileName);
            var settings = FileBasedFcmClientSettings.CreateFromFile(configPath);

            using (var client = new FcmClient(settings))
            {
                string key = "NEW_NOTIFI";
                string body = contents;
                string sound = "";
                
                if (notificationSound)
                    sound = "default";
                else
                    sound = "disabled";

                CancellationTokenSource cts = new CancellationTokenSource();
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("userId", userId + "");
                data.Add("key", key);
                data.Add("badge", badge + "");
                data.Add("sound", sound);
                
                Dictionary<string, string> header = new Dictionary<string, string>();
                header.Add("TTL", "3600");
                header.Add("Urgency", "high");

                var android = new AndroidConfig
                {
                    Notification = new AndroidNotification
                    {
                        Body = body,
                        Title = "Thông báo mới từ " + name,
                        Sound = sound,
                        ChannelId = "qln-channel",
                    },
                    Priority = AndroidMessagePriorityEnum.HIGH,
                };

                var apns = new ApnsConfig
                {
                    Payload = new ApnsConfigPayload
                    {
                        Aps = new Aps
                        {
                            Category = "NEW_MESSAGE_CATEGORY",
                            Badge = badgeTotal,
                            ContentAvailable = true,
                            Sound = sound,
                        }
                    }
                };

                string link = returnUrl == "" ? domain : (domain + returnUrl);
                var web = new WebpushConfig
                {
                    Headers = header,
                    Data = data,
                    Notification = new WebpushNotification
                    {
                        Title = "Thông báo mới từ " + name,
                        Body = body,
                        Icon = domain + "/assets/favicon.ico"
                    },
                    FcmOptions = new WebpushFcmOptions
                    {
                        Link = link
                    },
                };

                var notification = new FcmSharp.Requests.Notification
                {
                    Title = "Thông báo mới từ " + name,
                    Body = body
                };

                // The Message should be sent to the News Topic:
                var message = new FcmMessage()
                {
                    ValidateOnly = false,
                    Message = new Message
                    {
                        Token = tokenKey,
                        //Topic = "news",
                        Notification = notification,
                        Data = data,
                        AndroidConfig = android,
                        ApnsConfig = apns,
                        WebpushConfig = web
                    }
                };

                try
                {
                    await client.SendAsync(message, cts.Token);
                }
                catch (Exception ex)
                {
                    log.Error("GetByPage Error:" + ex);
                    log.Error("GetByPage Error Message:" + ex.Message);
                }
            }
        }
    }
}
