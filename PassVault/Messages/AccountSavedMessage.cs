using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OrionVault.Messages
{
    public class AccountSavedMessage : ValueChangedMessage<bool>
    {
        public AccountSavedMessage(bool value) : base(value) { }
    }
}
