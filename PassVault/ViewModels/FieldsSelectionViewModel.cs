using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Views;

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

            var query = new Dictionary<string, object>
            {
                { "selectedFields", selectedFields }
            };

            await Shell.Current.GoToAsync(nameof(NewAccountPage), query);
        }
    }
}
