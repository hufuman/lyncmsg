using System.Collections.Generic;
using System.Data;
using Microsoft.Lync.Model;

namespace MsgDao
{
    public class UserDb
    {
        static private readonly UserDb DbInstance = new UserDb();
        private readonly Dictionary<string, long> _mapUserIds = new Dictionary<string, long>();
        private long _selfUserId = -1;

        private UserDb()
        {
        }
        static public UserDb GetDb()
        {
            return DbInstance;
        }

        public long GetOrAddUserId(Contact contact)
        {
            long userId;
            if (_mapUserIds.TryGetValue(contact.Uri, out userId))
                return userId;
            object data = DbUtil.ExecuteSqlReturnValue(LyncDb.GetDb().SqlCnn, "select UserId from User where Uri=@Uri", new[]
            {
                DbUtil.BuildParameter("@Uri", DbType.String, contact.Uri)
            });
            if (data == null)
            {
                userId = AddUser(contact);
            }
            else
            {
                userId = (long) data;
            }
            _mapUserIds.Add(contact.Uri, userId);
            return userId;
        }

        private long AddUser(Contact contact)
        {
            var info = contact.GetContactInformation(new List<ContactInformationType>
            {
                ContactInformationType.DisplayName,
                ContactInformationType.EmailAddresses
            });
            object displayName = info[ContactInformationType.DisplayName] ?? "";
            object email = info[ContactInformationType.EmailAddresses];
            if (email != null)
            {
                email = ((List<object>)email)[0].ToString();
            }
            if (!DbUtil.ExecuteSqlNoQuery(LyncDb.GetDb().SqlCnn,
                "insert into User (Uri, Name, Email) values (@Uri, @Name, @Email)",
                new[]
                {
                    DbUtil.BuildParameter("@Uri", DbType.String, contact.Uri),
                    DbUtil.BuildParameter("@Name", DbType.String, displayName),
                    DbUtil.BuildParameter("@Email", DbType.String, email)
                }))
            {
                return -1;
            }
            long lastInsertRowId = LyncDb.GetDb().SqlCnn.LastInsertRowId;
            return lastInsertRowId;
        }

        public UserInfo GetUserInfoById(long userId)
        {
            const string sql = "select UserId, Uri, Email, Name from User where UserId=@UserId";
            using (var reader = DbUtil.ExecuteSql(LyncDb.GetDb().SqlCnn, sql, new[]
            {
                DbUtil.BuildParameter("@UserId", DbType.Int64, userId)
            })
                )
            {
                if (reader.Read())
                {
                    var result = new UserInfo
                    {
                        Email = reader.GetString(2),
                        Name = reader.GetString(3),
                        Uri = reader.GetString(1),
                        UserId = reader.GetInt64(0)
                    };
                    return result;
                }
            }
            return null;
        }

        public long GetSelfUserId()
        {
            if (_selfUserId > 0)
                return _selfUserId;
            _selfUserId = GetOrAddUserId(LClient.GetClient().LyncClient.Self.Contact);
            return _selfUserId;
        }
    }
}
