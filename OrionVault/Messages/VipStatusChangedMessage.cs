using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OrionVault.Messages
{
    public sealed class VipStatusChangedMessage : ValueChangedMessage<bool>
    {
        public VipStatusChangedMessage(bool isVip) : base(isVip)
        {
        }
    }
}
