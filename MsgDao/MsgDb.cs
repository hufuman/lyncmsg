﻿using System.Data;

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

        public long AddMessage(long chatId, long userId, string htmlMsg, string plainMsg)
        {
            if (!DbUtil.ExecuteSqlNoQuery(LyncDb.GetDb().SqlCnn,
                "insert into Message (HtmlMsg, PlainMsg, Data, Type, ChatId, UserId) values (@HtmlMsg, @PlainMsg, @Data, @Type, @ChatId, @UserId)",
                new[]
                {
                    DbUtil.BuildParameter("@HtmlMsg", DbType.String, htmlMsg),
                    DbUtil.BuildParameter("@PlainMsg", DbType.String, plainMsg),
                    DbUtil.BuildParameter("@Data", DbType.String, ""),
                    DbUtil.BuildParameter("@Type", DbType.Int16, MessageType.PlainText),
                    DbUtil.BuildParameter("@ChatId", DbType.Int64, chatId),
                    DbUtil.BuildParameter("@UserId", DbType.Int64, userId)
                }))
            {
                return -1;
            }
            return LyncDb.GetDb().SqlCnn.LastInsertRowId;
        }

        public MsgInfo GetNextMsg(long chatId, long msgId)
        {
            using (
                var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn,
                    "select MessageId, PlainMsg, DateTime, UserId from Message where ChatId=@ChatId and MessageId > @MsgId limit 1",
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
                    UserName = UserDb.GetDb().GetUserInfoById(reader.GetInt64(3)).Name
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

        public int LoadMsg(long chatId, int pageIndex, int pageSize, ref System.Collections.Generic.List<MsgInfo> listMsgInfo)
        {
            using (
                var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn,
                    "select a.MessageId, a.PlainMsg, a.DateTime, a.UserId from message a where a.ChatId=@ChatId and a.Messageid not in (select b.messageid from message b where b.ChatId=@ChatId limit @OverCount) limit @PageSize",
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
                        Date = reader.GetDateTime(2).ToString("yyyy-MM-dd hh:mm:ss"),
                        Message = reader.GetString(1),
                        UserId = reader.GetInt64(3),
                        UserName = UserDb.GetDb().GetUserInfoById(reader.GetInt64(3)).Name
                    };
                    listMsgInfo.Add(result);
                } while (reader.Read());
                return listMsgInfo.Count;
            }
        }
    }
}