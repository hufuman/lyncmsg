
using System.Collections.Generic;

namespace MsgDao
{
    public enum MessageType
    {
        PlainText,
    }

    public class ChatInfo
    {
        public long ChatId { get; set; }
        public List<long> UserIds { get; set; } 
    }

    public class UserInfo
    {
        public long UserId { get; set; }
        public string Uri { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }

    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class MsgInfo
    {
        public long MsgId { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
    }
}
