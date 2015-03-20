using System.Collections.Generic;
using Microsoft.Lync.Model.Conversation;

namespace LyncMsg.Demon
{
    class LConversationManager
    {
        private readonly Dictionary<Conversation, LConversation> _mapConversations = new Dictionary<Conversation, LConversation>();

        private LConversation.MessageDelegate _messageDelegate;

        public void Init(ConversationManager conversationManager, LConversation.MessageDelegate messageDelegate)
        {
            _messageDelegate = messageDelegate;
            int count = conversationManager.Conversations.Count;
            for (int i = 0; i < count; ++i)
            {
                Conversation conversation = conversationManager.Conversations[i];
                var lconversation = new LConversation(conversation, _messageDelegate);
                _mapConversations.Add(conversation, lconversation);
            }

            conversationManager.ConversationAdded += ConversationAdded;
            conversationManager.ConversationRemoved += ConversationRemoved;
        }

        private void ConversationAdded(object sender, ConversationManagerEventArgs args)
        {
            var lconversation = new LConversation(args.Conversation, _messageDelegate);
            _mapConversations.Add(args.Conversation, lconversation);
        }

        private void ConversationRemoved(object sender, ConversationManagerEventArgs args)
        {
            _mapConversations.Remove(args.Conversation);
        }
    }
}
