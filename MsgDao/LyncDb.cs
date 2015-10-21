using System;
using System.Data.SQLite;

namespace MsgDao
{
    public class LyncDb
    {
        public SQLiteConnection SqlCnn { get; set; }

        private LyncDb()
        {
        }
        static private readonly LyncDb DbInstance = new LyncDb();

        static public LyncDb GetDb()
        {
            return DbInstance;
        }

        public bool Init()
        {
            string filePath = GetMsgDbPath();
            if (filePath == null)
                return false;
            string sqlCnnString = "Data Source='" + filePath + "'";
            SqlCnn = new SQLiteConnection(sqlCnnString);
            SqlCnn.Open();

            return EnsureTables();
        }

        private bool EnsureTables()
        {
            const string chatSql =
                "CREATE TABLE if not exists [Chat] ( [ChatId] integer PRIMARY KEY AUTOINCREMENT NOT NULL, [UserIds] nvarchar(1000) NOT NULL, [LastTime] TIMESTAMP DEFAULT (datetime('now', 'localtime')), [Name] nvarchar(200) COLLATE NOCASE)";
            const string msgSql =
                "CREATE TABLE if not exists [Message] ( [MessageId] integer PRIMARY KEY AUTOINCREMENT NOT NULL, [HtmlMsg] ntext NOT NULL, [PlainMsg] ntext NOT NULL, [Data] ntext, [DateTime] TIMESTAMP default (datetime('now', 'localtime')), [Type] smallint NOT NULL, [ChatId] bigint NOT NULL, [UserId] bigint NOT NULL, [LyncConvId] ntext)";
            const string userSql =
                "CREATE TABLE if not exists [User] ( [UserId] integer PRIMARY KEY AUTOINCREMENT NOT NULL, [Uri] varchar(100) NOT NULL, [Name] varchar(100) NOT NULL, [Email] varchar(200) NOT NULL);";
            return DbUtil.ExecuteSqlNoQuery(SqlCnn, chatSql, null)
                && DbUtil.ExecuteSqlNoQuery(SqlCnn, msgSql, null)
                && DbUtil.ExecuteSqlNoQuery(SqlCnn, userSql, null);
        }

        public static string GetAppDataPath(string subPath)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            filePath += "\\" + subPath;
            filePath = filePath.Replace("\\\\", "\\");
            return filePath;
        }

        private string GetMsgDbPath()
        {
            string filePath = FileUtil.GetAppDataPath("LyncMsg\\MsgDb.db");
            return filePath;
        }
    }
}
