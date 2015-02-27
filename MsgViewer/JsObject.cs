using System.Diagnostics;
using MsgDao;
using System.Collections.Generic;

namespace MsgViewer
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JsChat
    {
        public List<long> UserIds;

        public long ChatId { get; set; }
        public long GetUserId(int index)
        {
            if (index >= UserIds.Count)
                return -1;
            return UserIds[index];
        }

        public int GetUserCount()
        {
            return UserIds.Count;
        }
    }

    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JsUser
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
    }

    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JsObject
    {
        private System.Windows.Controls.WebBrowser _browser;
        private List<MsgInfo> _listMsgInfo = null;
        private readonly Dictionary<int, JsUser> _userData = new Dictionary<int, JsUser>();

        #region 辅助方法

        public void ClearMsg()
        {
            if (_listMsgInfo != null)
            {
                _listMsgInfo.Clear();
                _listMsgInfo = null;
            }
        }

        public long SearchMsgCount(long chatId, string keyword)
        {
            return MsgDb.GetDb().SearchMsgCount(keyword, chatId);
        }

        public void SearchMsg(string funcName, long chatId, string keyword, int pageIndex, int pageSize)
        {
            var msgs = MsgDb.GetDb().SearchMsg(keyword, chatId, pageIndex, pageSize);
            if (msgs != null && msgs.Count > 0)
                msgs.ForEach(msg => _browser.InvokeScript(funcName, msg));
        }

        public int PrepareMsg(long charId, int pageIndex, int pageSize)
        {
            ClearMsg();
            _listMsgInfo = new List<MsgInfo>();
            return MsgDb.GetDb().LoadMsg(charId, pageIndex, pageSize, ref _listMsgInfo);
        }

        public MsgInfo NextMsg(int index)
        {
            if (index < 0 || index >= _listMsgInfo.Count)
                return null;
            return _listMsgInfo[index];
        }

        public JsUser GetUserInfo(int userId)
        {
            JsUser result;
            if (_userData.TryGetValue(userId, out result))
                return result;
            var data = UserDb.GetDb().GetUserInfoById(userId);
            if (data == null)
                return null;
            result = new JsUser
            {
                Name = data.Name,
                UserId = data.UserId,
                Uri = data.Uri
            };
            _userData.Add(userId, result);
            return result;
        }

        public long GetMsgCount(long chatId)
        {
            return MsgDb.GetDb().GetMsgCount(chatId);
        }

        public long GetSelfUserId()
        {
            return UserDb.GetDb().GetSelfUserId();
        }

        public void StartConversation(long chatId)
        {
            var chatInfo = ChatDb.GetDb().GetChatInfoById(chatId);
            if (chatInfo == null)
                return;
            string command = "im:";
            chatInfo.UserIds.ForEach(userId =>
            {
                var userInfo = UserDb.GetDb().GetUserInfoById(userId);
                if (userInfo != null)
                {
                    command += "<" + userInfo.Uri + ">";
                }
            });
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                }
            };
            proc.Start();
        }

        public bool ReloadChatInfo(string funcName)
        {
            var chatInfos = ChatDb.GetDb().GetAllChatInfo();
            chatInfos.ForEach(info => _browser.InvokeScript(funcName, new JsChat
            {
                ChatId = info.ChatId,
                UserIds = info.UserIds
            }));
            return true;
        }
        #endregion

        internal void SetBrowser(System.Windows.Controls.WebBrowser browser)
        {
            _browser = browser;
        }
    }
}
