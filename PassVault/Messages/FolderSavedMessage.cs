using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OrionVault.Messages
{
    public class FolderSavedMessage : ValueChangedMessage<bool>
    {
        public FolderSavedMessage(bool value) : base(value) { }
    }
}
