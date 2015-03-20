using System.Collections.Generic;
using Microsoft.Lync.Model.Conversation;
using MsgDao;

namespace LyncMsg.Demon
{
    class LConversation
    {
        private readonly string _convId;
        private long _dbChatId;
        private readonly Dictionary<string, UserInfo> _mapMsgRecHandlers = new Dictionary<string, UserInfo>();

        public delegate void MessageDelegate(long chatId, long userId, string htmlMsg, string plainMsg);

        private readonly MessageDelegate _msgDelegate;

        public LConversation(Conversation conversation, MessageDelegate messageDelegate)
        {
            _msgDelegate = messageDelegate;
            object idValue;
            if (conversation.Properties.TryGetValue(ConversationProperty.Id, out idValue))
                _convId = (string)idValue;
            else
                _convId = "";
            int count = conversation.Participants.Count;
            for (int i = 0; i < count; ++i)
            {
                var p = conversation.Participants[i];
                AddParticipant(p);
            }
            conversation.ParticipantAdded += ParticipantAdded;
            conversation.ParticipantRemoved += ParticipantRemoved;
        }

        private void AddParticipant(Participant participant)
        {
            if (_mapMsgRecHandlers.ContainsKey(participant.Contact.Uri))
                return;

            long userId = UserDb.GetDb().GetOrAddUserId(participant.Contact);
            if (userId <= 0)
                return;
            var userInfo = UserDb.GetDb().GetUserInfoById(userId);
            if (userInfo == null)
                return;
            _mapMsgRecHandlers.Add(participant.Contact.Uri, userInfo);

            var modality = ((InstantMessageModality)participant.Modalities[ModalityTypes.InstantMessage]);
            modality.InstantMessageReceived += MsgReceived;
        }

        private void RemoveParticipant(Participant participant)
        {
            UserInfo userInfo;
            if (!_mapMsgRecHandlers.TryGetValue(participant.Contact.Uri, out userInfo))
                return;

            var modality = ((InstantMessageModality)participant.Modalities[ModalityTypes.InstantMessage]);
            modality.InstantMessageReceived -= MsgReceived;
            _mapMsgRecHandlers.Remove(participant.Contact.Uri);
        }

        private void ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs args)
        {
            AddParticipant(args.Participant);
        }

        private void ParticipantRemoved(object sender, ParticipantCollectionChangedEventArgs args)
        {
            RemoveParticipant(args.Participant);
        }

        private void RefreshChatId()
        {
            var userIds = new List<long>();
            foreach (var pair in _mapMsgRecHandlers)
            {
                userIds.Add(pair.Value.UserId);
            }
            _dbChatId = ChatDb.GetDb().GetChatId(userIds);
            if (_dbChatId <= 0)
                _dbChatId = ChatDb.GetDb().CreateChat(userIds);
        }

        private void MsgReceived(object sender, MessageSentEventArgs data)
        {
            RefreshChatId();
            var modality = (InstantMessageModality)sender;
            long userId = UserDb.GetDb().GetOrAddUserId(modality.Participant.Contact);
            if (userId <= 0)
                return;
            string plainMsg;
            if (!data.Contents.TryGetValue(InstantMessageContentType.PlainText, out plainMsg))
            {
                plainMsg = data.Text;
            }

            string htmlMsg;
            if (!data.Contents.TryGetValue(InstantMessageContentType.Html, out htmlMsg))
            {
                htmlMsg = plainMsg;
            }
            htmlMsg = htmlMsg ?? "";
            plainMsg = plainMsg ?? "";
            AddMessage(_dbChatId, userId, htmlMsg, plainMsg, _convId);
        }

        private void AddMessage(long chatId, long userId, string htmlMsg, string plainMsg, string conversationId)
        {
            MsgDb.GetDb().AddMessage(chatId, userId, htmlMsg, plainMsg, conversationId);
            if (_msgDelegate != null)
                _msgDelegate(chatId, userId, htmlMsg, plainMsg);
        }
    }
}
