using System;
using System.Collections.Generic;
using System.Data;

namespace MsgDao
{
    public class MsgDb
    {
        static private readonly MsgDb DbInstance = new MsgDb();

        private MsgDb()
        {
        }
        static public MsgDb GetDb()
        {
            return DbInstance;
        }

        public long AddMessage(long chatId, long userId, string htmlMsg, string plainMsg, string conversationId)
        {
            if (!DbUtil.ExecuteSqlNoQuery(LyncDb.GetDb().SqlCnn,
                "insert into Message (HtmlMsg, PlainMsg, Data, Type, ChatId, UserId, LyncConvId) values (@HtmlMsg, @PlainMsg, @Data, @Type, @ChatId, @UserId, @LyncConvId)",
                new[]
                {
                    DbUtil.BuildParameter("@HtmlMsg", DbType.String, htmlMsg),
                    DbUtil.BuildParameter("@PlainMsg", DbType.String, plainMsg),
                    DbUtil.BuildParameter("@Data", DbType.String, ""),
                    DbUtil.BuildParameter("@Type", DbType.Int16, MessageType.PlainText),
                    DbUtil.BuildParameter("@ChatId", DbType.Int64, chatId),
                    DbUtil.BuildParameter("@UserId", DbType.Int64, userId),
                    DbUtil.BuildParameter("@LyncConvId", DbType.String, conversationId)
                }))
            {
                return -1;
            }
            // Update Chat.LastTime
            ChatDb.GetDb().UpdateLastTime(chatId);
            return LyncDb.GetDb().SqlCnn.LastInsertRowId;
        }

        public MsgInfo GetNextMsg(long chatId, long msgId)
        {
            using (
                var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn,
                    "select MessageId, PlainMsg, DateTime, UserId, ChatId from Message where ChatId=@ChatId and MessageId > @MsgId limit 1",
                    new[]
                    {
                        DbUtil.BuildParameter("@MsgId", DbType.Int64, msgId),
                        DbUtil.BuildParameter("@ChatId", DbType.Int64, chatId)
                    }))
            {
                if (!reader.Read())
                    return null;
                var result = new MsgInfo
                {
                    MsgId = reader.GetInt64(0),
                    Date = reader.GetDateTime(2).ToString("yyyy-MM-dd hh:mm:ss"),
                    Message = reader.GetString(1),
                    UserId = reader.GetInt64(3),
                    UserName = UserDb.GetDb().GetUserInfoById(reader.GetInt64(3)).Name,
                    ChatId = reader.GetInt64(4)
                };
                return result;
            }
        }

        public long GetMsgCount(long chatId)
        {
            using (
                var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn,
                    "select count(1) from Message where ChatId=@ChatId",
                    new[]
                    {
                        DbUtil.BuildParameter("@ChatId", DbType.Int64, chatId)
                    }))
            {
                if (!reader.Read())
                    return 0;
                return reader.GetInt64(0);
            }
        }

        public int LoadMsg(long chatId, int pageIndex, int pageSize, ref List<MsgInfo> listMsgInfo)
        {
            using (
                var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn,
                    "select a.MessageId, a.PlainMsg, a.DateTime, a.UserId, a.ChatId from message a where a.ChatId=@ChatId and a.Messageid not in (select b.messageid from message b where b.ChatId=@ChatId limit @OverCount) limit @PageSize",
                    new[]
                    {
                        DbUtil.BuildParameter("@OverCount", DbType.Int32, pageSize * pageIndex),
                        DbUtil.BuildParameter("@PageSize", DbType.Int32, pageSize),
                        DbUtil.BuildParameter("@ChatId", DbType.Int64, chatId)
                    }))
            {
                if (!reader.Read())
                    return 0;
                do
                {
                    var result = new MsgInfo
                    {
                        MsgId = reader.GetInt64(0),
                        Date = reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss"),
                        Message = reader.GetString(1),
                        UserId = reader.GetInt64(3),
                        UserName = UserDb.GetDb().GetUserInfoById(reader.GetInt64(3)).Name,
                        ChatId = reader.GetInt64(4)
                    };
                    listMsgInfo.Add(result);
                } while (reader.Read());
                return listMsgInfo.Count;
            }
        }

        public long SearchMsgCount(string keyword, long chatId)
        {
            if (!keyword.Contains("%"))
                keyword = "%" + keyword + "%";
            using (
                var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn,
                    "select count(1) from Message a where (@ChatId=0 or a.ChatId=@ChatId) and (a.PlainMsg like @SearchData)",
                    new[]
                    {
                        DbUtil.BuildParameter("@SearchData", DbType.String, keyword),
                        DbUtil.BuildParameter("@ChatId", DbType.Int64, chatId)
                    }))
            {
                if (!reader.Read())
                    return 0;
                return reader.GetInt64(0);
            }
        }

        public List<MsgInfo> SearchMsg(string keyword, long chatId, int pageIndex, int pageSize)
        {
            if(keyword.Length == 0)
                throw new Exception("空查找串");
            if (!keyword.Contains("%"))
                keyword = "%" + keyword + "%";
            var result = new List<MsgInfo>();
            using (
                var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn,
                    "select a.MessageId, a.PlainMsg, a.DateTime, a.UserId, a.ChatId from Message a where (@ChatId=0 or a.ChatId=@ChatId) and (a.PlainMsg like @SearchData) limit @FromIndex,@MsgCount",
                    new[]
                    {
                        DbUtil.BuildParameter("@ChatId", DbType.Int64, chatId),
                        DbUtil.BuildParameter("@SearchData", DbType.String, keyword),
                        DbUtil.BuildParameter("@FromIndex", DbType.Int32, pageIndex * pageSize),
                        DbUtil.BuildParameter("@MsgCount", DbType.Int32, pageSize)
                    }))
            {
                if (reader == null || !reader.Read())
                    return null;
                do
                {
                    var data = new MsgInfo
                    {
                        MsgId = reader.GetInt64(0),
                        Date = reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss"),
                        Message = reader.GetString(1),
                        UserId = reader.GetInt64(3),
                        UserName = UserDb.GetDb().GetUserInfoById(reader.GetInt64(3)).Name,
                        ChatId = reader.GetInt64(4)
                    };
                    result.Add(data);
                } while (reader.Read());
            }
            return result;
        }
    }
}
