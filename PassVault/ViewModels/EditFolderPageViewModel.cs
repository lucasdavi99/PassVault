using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using System.ComponentModel.DataAnnotations;

namespace PassVault.ViewModels
{
    public partial class EditFolderPageViewModel : ObservableValidator, IQueryAttributable
    {
        private readonly FolderDatabase _folderDatabase;
        private Folder _currentFolder;

        [ObservableProperty]
        private int _folderId;

        [ObservableProperty]
        [Required(ErrorMessage = "Título é obrigatório")]
        private string _title;

        [ObservableProperty]
        private Color _selectedColor = Colors.Purple;

        [ObservableProperty]
        private string _selectedColorHex = Colors.Purple.ToHex();

        [ObservableProperty]
        private bool _isColorPickerVisible = false;

        [ObservableProperty]
        private bool _isEditing;

        public EditFolderPageViewModel(FolderDatabase folderDatabase)
        {
            _folderDatabase = folderDatabase;
            IsEditing = false;
        }

        [RelayCommand]
        private async Task SaveEditFolderAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Title))
                {
                    await Shell.Current.DisplayAlert("Erro", "Preencha os campos obrigatórios", "OK");
                    return;
                }

                {
                    _currentFolder.Title = Title;                 
                    _currentFolder.Color = SelectedColor.ToHex();
                };

                await _folderDatabase.SaveFolderAsync(_currentFolder);
                await Shell.Current.DisplayAlert("Sucesso", "Pasta atualizada com sucesso", "OK");
                WeakReferenceMessenger.Default.Send(new FolderSavedMessage(true));
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task Close() => await Shell.Current.GoToAsync("..");

        [RelayCommand]
        private void ToggleColorPicker() => IsColorPickerVisible = !IsColorPickerVisible;

        [RelayCommand]
        private void CloseColorPicker() => IsColorPickerVisible = false;

        [RelayCommand]
        private void ToggleEditMode() => IsEditing = !IsEditing;

        partial void OnSelectedColorChanged(Color value)
        {
            // Força a atualização da interface
            SelectedColorHex = value.ToHex();
            OnPropertyChanged(nameof(SelectedColor));
        }

        partial void OnSelectedColorHexChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && (value.Length == 7 || value.Length == 9))
            {
                var newColor = Color.FromArgb(value);
                if (newColor != SelectedColor)
                {
                    SelectedColor = newColor;
                }
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("folderId") && int.TryParse(query["folderId"]?.ToString(), out int folderId))
            {
                _currentFolder = await _folderDatabase.GetFolderAsync(folderId);
                
                if (_currentFolder != null)
                {
                    Title = _currentFolder.Title;                    
                    SelectedColor = Color.FromArgb(_currentFolder.Color);
                }
            }
        }
    }
}
