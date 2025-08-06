using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PassVault.Messages
{
    public class AccountSavedMessage : ValueChangedMessage<bool>
    {
        public AccountSavedMessage(bool value) : base(value) { }
    }
}
