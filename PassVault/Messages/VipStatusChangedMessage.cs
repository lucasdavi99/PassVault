using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PassVault.Messages
{
    public sealed class VipStatusChangedMessage : ValueChangedMessage<bool>
    {
        public VipStatusChangedMessage(bool isVip) : base(isVip)
        {
        }
    }
}
