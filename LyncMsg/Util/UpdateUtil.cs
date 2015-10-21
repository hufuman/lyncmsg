using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace LyncMsg.Util
{
    static class UpdateUtil
    {
        public static string GetData(string url, Dictionary<string, string> headers)
        {
            try
            {
                var client = new WebClient();
                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        client.Headers.Add(header.Key, header.Value);
                    }
                }
                var byDatas = client.DownloadData(url);
                var result = Encoding.UTF8.GetString(byDatas);
                return result;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static TObj Json2Obj<TObj>(string jsonData) where TObj : class
        {
            try
            {
                var result = JsonConvert.DeserializeObject(jsonData, typeof(TObj)) as TObj;
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static bool GetFile(string fileUrl, string filePath)
        {
            try
            {
                var client = new WebClient();
                client.DownloadFile(fileUrl, filePath);
                return true;
            }
            catch (Exception)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return false;
            }
        }

        internal static bool MakeSureDirectory(string path)
        {
            try
            {
                var parts = path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    return false;
                var result = parts[0];
                for (int i = 1; i < parts.Length; ++i)
                {
                    result = Path.Combine(result, parts[i]);
                    if (!Directory.Exists(result))
                        Directory.CreateDirectory(result);
                }
                return Directory.Exists(result);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
