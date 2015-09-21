using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MsgDao;

namespace LyncTools
{
    class Program
    {
        public static string GetAppDataPath(string subPath)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            filePath += "\\" + subPath;
            filePath = filePath.Replace("\\\\", "\\");
            return filePath;
        }

        class MsgData
        {
            public long MsgId { get; set; }
            public string HtmlMsg { get; set; }
            public string PlainMsg { get; set; }
        }

        static bool UpdateMsgs(SQLiteConnection sqlCnn, List<MsgData> listDatas)
        {
            foreach (var data in listDatas)
            {
                if (!DbUtil.ExecuteSqlNoQuery(sqlCnn,
                    "update Message set PlainMsg=@PlainMsg where MessageId=@MsgId",
                    new[]
                {
                    DbUtil.BuildParameter("@PlainMsg", DbType.String, data.PlainMsg),
                    DbUtil.BuildParameter("@MsgId", DbType.Int64, data.MsgId)
                }))
                {
                    Console.WriteLine("Error");
                }
            }
            return true;
        }


        static string RemoveHtmlTags(string htmlString)
        {
            if (string.IsNullOrEmpty(htmlString))
                return string.Empty;
            htmlString = HttpUtility.HtmlDecode(htmlString);
            htmlString = Regex.Replace(htmlString, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, "</p\\s*>", "\n", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, "<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, "\n\n+", "\n\n", RegexOptions.ECMAScript);
            htmlString = System.Web.HttpUtility.UrlDecode(htmlString, Encoding.UTF8);
            return htmlString;
        }

        static List<MsgData> GetNextMsgs(SQLiteConnection sqlCnn, long msgId)
        {
            var listResult = new List<MsgData>();
            using (
                var reader = DbUtil.ExecuteSql(sqlCnn,
                    "select MessageId, PlainMsg, HtmlMsg from Message where MessageId > @MsgId order by MessageId limit 100",
                    new[]
                    {
                        DbUtil.BuildParameter("@MsgId", DbType.Int64, msgId)
                    }))
            {
                while (reader.Read())
                {
                    var data = new MsgData
                    {
                        MsgId = reader.GetInt64(0),
                        PlainMsg = reader.GetString(1),
                        HtmlMsg = reader.GetString(2)
                    };
                    if (!string.IsNullOrEmpty(data.HtmlMsg))
                    {
                        data.PlainMsg = RemoveHtmlTags(data.HtmlMsg);
                    }
                    listResult.Add(data);
                }
            }
            return listResult;
        }

        static void Main(string[] args)
        {
            string filePath = GetAppDataPath("LyncMsg\\MsgDb.db");
            if (filePath == null)
                return;
            string sqlCnnString = "Data Source='" + filePath + "'";
            var sqlCnn = new SQLiteConnection(sqlCnnString);
            sqlCnn.Open();

            // 
            long msgId = 11995;
            for (; ; )
            {
                var listDatas = GetNextMsgs(sqlCnn, msgId);
                if (listDatas == null || listDatas.Count == 0)
                    break;
                UpdateMsgs(sqlCnn, listDatas);
                msgId = listDatas[listDatas.Count - 1].MsgId;
                Console.Write("\rMsgId: {0}", msgId);
            }
        }
    }
}
