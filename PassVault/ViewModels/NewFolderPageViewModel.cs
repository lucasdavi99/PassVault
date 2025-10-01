using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Interfaces;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Services;
using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace PassVault.ViewModels
{
    public partial class NewFolderPageViewModel : ObservableValidator, IQueryAttributable
    {
        private readonly FolderDatabase _database;
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private int? parentFolderId;

        [ObservableProperty]
        [Required(ErrorMessage = "Título é obrigatório")]
        private string _title;

        [ObservableProperty]
        private Color _selectedColor = Colors.Purple;

        [ObservableProperty]
        private string _selectedColorHex = Colors.Purple.ToHex();

        [ObservableProperty]
        private bool _isColorPickerVisible = false;

        // Localized Properties
        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string pageSubtitle;
        [ObservableProperty] private string previewTitle;
        [ObservableProperty] private string previewSubtitle;
        [ObservableProperty] private string previewNamePlaceholder;
        [ObservableProperty] private string formTitle;
        [ObservableProperty] private string formSubtitle;
        [ObservableProperty] private string nameLabel;
        [ObservableProperty] private string namePlaceholder;
        [ObservableProperty] private string colorLabel;
        [ObservableProperty] private string colorTapHere;
        [ObservableProperty] private string createButton;
        [ObservableProperty] private string colorPickerTitle;
        [ObservableProperty] private string colorPickerSubtitle;
        [ObservableProperty] private string hexCodeLabel;
        [ObservableProperty] private string hexCodePlaceholder;
        [ObservableProperty] private string confirmColorButton;


        public NewFolderPageViewModel(FolderDatabase database, ILocalizationService localizationService)
        {
            _database = database;
            _localizationService = localizationService;
            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += (s, e) => UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            PageTitle = L.Text("new_folder.title");
            PageSubtitle = L.Text("new_folder.subtitle");
            PreviewTitle = L.Text("new_folder.preview_title");
            PreviewSubtitle = L.Text("new_folder.preview_subtitle");
            PreviewNamePlaceholder = L.Text("new_folder.preview_name_placeholder");
            FormTitle = L.Text("new_folder.form_title");
            FormSubtitle = L.Text("new_folder.form_subtitle");
            NameLabel = L.Text("new_folder.name_label");
            NamePlaceholder = L.Text("new_folder.name_placeholder");
            ColorLabel = L.Text("new_folder.color_label");
            ColorTapHere = L.Text("new_folder.color_tap_here");
            CreateButton = L.Text("new_folder.create_button");
            ColorPickerTitle = L.Text("new_folder.color_picker_title");
            ColorPickerSubtitle = L.Text("new_folder.color_picker_subtitle");
            HexCodeLabel = L.Text("folders.hex_code");
            HexCodePlaceholder = L.Text("new_folder.hex_code_placeholder");
            ConfirmColorButton = L.Text("folders.confirm_color");
        }

        [RelayCommand]
        public async Task SaveFolderAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Title))
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.folder_title_required"), L.Text("common.ok"));
                    return;
                }

                // Verificar se já existe uma pasta com o mesmo nome no mesmo local
                bool folderExists = await _database.FolderNameExistsAsync(Title, ParentFolderId);
                
                if (folderExists)
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.duplicate_folder_name"), L.Text("common.ok"));
                    return;
                }

                var folder = new Folder
                {
                    Title = Title,
                    Color = SelectedColor.ToHex(),
                    ParentFolderId = ParentFolderId
                };

                await _database.SaveFolderAsync(folder);
                await Shell.Current.DisplayAlert(L.Text("common.success"), L.Text("messages.folder_created"), L.Text("common.ok"));
                WeakReferenceMessenger.Default.Send(new FolderSavedMessage(true));
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
            }
        }

        [RelayCommand]
        private void ToggleColorPicker()
        {
            IsColorPickerVisible = !IsColorPickerVisible;
        }

        [RelayCommand]
        private void CloseColorPicker()
        {
            IsColorPickerVisible = false;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("parentFolderId") && int.TryParse(query["parentFolderId"]?.ToString(), out int parentId))
            {
                ParentFolderId = parentId;
            }
        }

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
    }
}