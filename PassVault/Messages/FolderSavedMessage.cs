using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.Messages
{
    public class FolderSavedMessage : ValueChangedMessage<bool>
    {
        public FolderSavedMessage(bool value) : base(value) { }
    }
}
