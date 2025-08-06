using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PassVault.Messages
{
    public class PasswordGeneratedMessage : ValueChangedMessage<string>
    {
        public PasswordGeneratedMessage(string value) : base(value)
        {
        }
    }
}
