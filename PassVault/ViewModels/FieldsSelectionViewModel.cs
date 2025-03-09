using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Views;

namespace PassVault.ViewModels
{
    public partial class FieldsSelectionViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        private bool isUsernameChecked = true;

        [ObservableProperty]
        private bool isEmailChecked = true;

        [ObservableProperty]
        private int folderId;

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
                    { "selectedFields", selectedFields },
                };

            if (FolderId != 0)
                query["folderId"] = FolderId;

            await Shell.Current.GoToAsync(nameof(NewAccountPage), query);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("folderId") && int.TryParse(query["folderId"]?.ToString(), out int folderId))
            {
                FolderId = folderId;
            }
        }
    }
}
