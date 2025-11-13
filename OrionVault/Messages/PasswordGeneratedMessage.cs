using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OrionVault.Messages
{
    public class PasswordGeneratedMessage : ValueChangedMessage<string>
    {
        public PasswordGeneratedMessage(string value) : base(value)
        {
        }
    }
}
