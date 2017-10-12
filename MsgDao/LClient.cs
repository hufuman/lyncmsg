using System;
using System.Reflection;
using Microsoft.Lync.Model;

namespace MsgDao
{
    public class LClient
    {
        private LyncClient _lyncClient;

        private LClient()
        {
        }

        private static readonly LClient ClientInstance = new LClient();

        public static LClient GetClient()
        {
            return ClientInstance;
        }

        public bool Init()
        {
            try
            {
                _lyncClient = LyncClient.GetClient();
                return _lyncClient != null && _lyncClient.State == ClientState.SignedIn;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string GetVersion()
        {
            var field = typeof(LyncClient).GetField("s_office_integration_version",
                BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.ExactBinding | BindingFlags.Static);
            if (field == null)
                return "";
            var version = (string)field.GetValue(_lyncClient);
            return version ?? "";
        }

        public LyncClient LyncClient
        {
            get { return _lyncClient; }
        }
    }
}
