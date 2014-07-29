using System;
using System.Collections.Generic;
using Microsoft.Lync.Model.Conversation;
using MsgDao;

namespace LyncMsg
{
    class LConversation
    {
        private long _dbChatId;
        private readonly Dictionary<string, UserInfo> _mapMsgRecHandlers = new Dictionary<string, UserInfo>();

        public LConversation(Conversation conversation)
        {
            int count = conversation.Participants.Count;
            for (int i = 0; i < count; ++i)
            {
                var p = conversation.Participants[i];
                AddParticipant(p);
            }
            conversation.ParticipantAdded += ParticipantAdded;
            conversation.ParticipantRemoved += ParticipantRemoved;
            RefreshMsgId();
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
            RefreshMsgId();
        }

        private void ParticipantRemoved(object sender, ParticipantCollectionChangedEventArgs args)
        {
            RemoveParticipant(args.Participant);
            RefreshMsgId();
        }

        private void RefreshMsgId()
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
            var modality = (InstantMessageModality) sender;
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
            AddMessage(_dbChatId, userId, htmlMsg, plainMsg);
        }

        private void AddMessage(long chatId, long userId, string htmlMsg, string plainMsg)
        {
            MsgDb.GetDb().AddMessage(chatId, userId, htmlMsg, plainMsg);
            Console.WriteLine("[Chat] " + plainMsg);
        }
    }
}
