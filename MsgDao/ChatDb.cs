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
                "insert into Chat (UserIds) values (@UserIds)",
                new[]
                {
                    DbUtil.BuildParameter("@UserIds", DbType.String, GetUserIds(userIds))
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

        public List<ChatInfo> GetChatInfo()
        {
            var result = new List<ChatInfo>();
            const string sql = "select c.ChatId, c.UserIds from Chat c "
                + "order by "
                + " (select max(m.messageid) from message m where m.chatid=c.chatid)"
                + " desc";
            using (var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn, sql, null))
            {
                while (reader.Read())
                {
                    long chatId = reader.GetInt64(0);
                    string userIds = reader.GetString(1);
                    var data = userIds.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    var info = new ChatInfo
                    {
                        ChatId = chatId,
                        UserIds = new List<long>()
                    };
                    foreach (var id in data)
                    {
                        if(id.Length > 0)
                            info.UserIds.Add(Int64.Parse(id));
                    }
                    result.Add(info);
                }
            }
            return result;
        } 
    }
}
