using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PassVault.Messages
{
    public class FolderSavedMessage : ValueChangedMessage<bool>
    {
        public FolderSavedMessage(bool value) : base(value) { }
    }
}
