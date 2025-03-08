using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace PassVault.ViewModels
{
    public partial class FieldsSelectionViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isUsernameChecked = true;

        [ObservableProperty]
        private bool isEmailChecked = true;

        [RelayCommand]
        private async Task ConfirmSelectionAsync()
        {
            var selectedFields = new Dictionary<string, bool>
            {
                { "Username", IsUsernameChecked },
                { "Email", IsEmailChecked },               
            };

            WeakReferenceMessenger.Default.Send(selectedFields);
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
