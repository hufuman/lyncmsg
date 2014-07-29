using System.Collections.Generic;
using Microsoft.Lync.Model.Conversation;

namespace LyncMsg
{
    class LConversationManager
    {
        private readonly Dictionary<Conversation, LConversation> _mapConversations = new Dictionary<Conversation, LConversation>();

        public void Init(ConversationManager conversationManager)
        {
            int count = conversationManager.Conversations.Count;
            for (int i = 0; i < count; ++i)
            {
                Conversation conversation = conversationManager.Conversations[i];
                var lconversation = new LConversation(conversation);
                _mapConversations.Add(conversation, lconversation);
            }

            conversationManager.ConversationAdded += ConversationAdded;
            conversationManager.ConversationRemoved += ConversationRemoved;
        }

        private void ConversationAdded(object sender, ConversationManagerEventArgs args)
        {
            var lconversation = new LConversation(args.Conversation);
            _mapConversations.Add(args.Conversation, lconversation);
        }

        private void ConversationRemoved(object sender, ConversationManagerEventArgs args)
        {
            _mapConversations.Remove(args.Conversation);
        }
    }
}
