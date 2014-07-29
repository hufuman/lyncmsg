using System;
using System.Threading;
using Microsoft.Lync.Model;
using MsgDao;

namespace LyncMsg
{
    class Program
    {
        static void Main(string[] args)
        {
            LyncDb.GetDb().Init();

            var conversationManager = new LConversationManager();
            conversationManager.Init(LClient.GetClient().LyncClient.ConversationManager);

            Console.WriteLine("LyncMsg Started.");
            for (;;)
            {
                Thread.Sleep(100);
            }
        }
    }
}
