using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public LyncClient LyncClient
        {
            get { return _lyncClient; }
        }
    }
}
