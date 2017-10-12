using System;
using Microsoft.Lync.Model;

namespace LyncTools
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = LyncClient.GetClient();
            Console.WriteLine(client.State);
        }
    }
}
