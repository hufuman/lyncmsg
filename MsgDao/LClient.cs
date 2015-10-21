using System.Reflection;
using Microsoft.Lync.Model;

namespace MsgDao
{
    public class LClient
    {
        private readonly LyncClient _lyncClient = LyncClient.GetClient();

        private LClient()
        {
        }

        static private readonly LClient ClientInstance = new LClient();

        static public LClient GetClient()
        {
            return ClientInstance;
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
