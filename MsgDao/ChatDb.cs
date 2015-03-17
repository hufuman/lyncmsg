using System;
using System.Collections.Generic;
using System.Data;

namespace MsgDao
{
    public class ChatDb
    {
        static private readonly ChatDb DbInstance = new ChatDb();

        private ChatDb()
        {
        }
        static public ChatDb GetDb()
        {
            return DbInstance;
        }

        public long GetChatId(IEnumerable<long> userIds)
        {
            object chatId = DbUtil.ExecuteSqlReturnValue(LyncDb.GetDb().SqlCnn, "select ChatId from Chat where UserIds=@UserIds",
                new[]
                {
                    DbUtil.BuildParameter("@UserIds", DbType.String, GetUserIds(userIds))
                });
            return chatId == null ? -1 : (long)chatId;
        }

        public long CreateChat(IEnumerable<long> userIds)
        {
            if (!DbUtil.ExecuteSqlNoQuery(LyncDb.GetDb().SqlCnn,
                "insert into Chat (UserIds, LastTime) values (@UserIds, @LastTime)",
                new[]
                {
                    DbUtil.BuildParameter("@UserIds", DbType.String, GetUserIds(userIds)),
                    DbUtil.BuildParameter("@LastTime", DbType.String, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                }))
            {
                return -1;
            }
            return LyncDb.GetDb().SqlCnn.LastInsertRowId;
        }

        private string GetUserIds(IEnumerable<long> userIds)
        {
            var ids = new List<long>();
            ids.AddRange(userIds);
            ids.Sort();
            string userIdsString = "";
            ids.ForEach(id => userIdsString += id + ",");
            return userIdsString;
        }

        public ChatInfo GetChatInfoById(long chatId)
        {
            const string sql = "select c.UserIds, c.Name from Chat c where c.ChatId=@ChatId";
            using (var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn, sql, new[]
            {
                    DbUtil.BuildParameter("@ChatId", DbType.Int64, chatId),
            }))
            {
                while (reader.Read())
                {
                    string userIds = reader.GetString(0);
                    string name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var data = userIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var info = new ChatInfo
                    {
                        Name = name,
                        ChatId = chatId,
                        UserIds = new List<long>()
                    };
                    foreach (var id in data)
                    {
                        if (id.Length > 0)
                            info.UserIds.Add(Int64.Parse(id));
                    }
                    return info;
                }
            }
            return null;
        }

        public List<ChatInfo> GetAllChatInfo()
        {
            var result = new List<ChatInfo>();
            const string sql = "select c.ChatId, c.UserIds, c.Name from Chat c "
                + "order by c.LastTime desc";
            using (var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn, sql, null))
            {
                while (reader.Read())
                {
                    long chatId = reader.GetInt64(0);
                    string userIds = reader.GetString(1);
                    string name = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var data = userIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var info = new ChatInfo
                    {
                        Name = name,
                        ChatId = chatId,
                        UserIds = new List<long>()
                    };
                    foreach (var id in data)
                    {
                        if (id.Length > 0)
                            info.UserIds.Add(Int64.Parse(id));
                    }
                    result.Add(info);
                }
            }
            return result;
        }

        internal bool UpdateLastTime(long chatId)
        {
            return DbUtil.ExecuteSqlNoQuery(LyncDb.GetDb().SqlCnn,
                "Update Chat set LastTime=@LastTime where ChatId=@ChatId",
                new[]
                {
                    DbUtil.BuildParameter("@LastTime", DbType.String, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    DbUtil.BuildParameter("@ChatId", DbType.String, chatId),
                });
        }

        public bool UpdateChatName(long chatId, string name)
        {
            return DbUtil.ExecuteSqlNoQuery(LyncDb.GetDb().SqlCnn,
                "Update Chat set Name=@Name where ChatId=@ChatId",
                new[]
                {
                    DbUtil.BuildParameter("@Name", DbType.String, name),
                    DbUtil.BuildParameter("@ChatId", DbType.String, chatId),
                });
        }
    }
}
