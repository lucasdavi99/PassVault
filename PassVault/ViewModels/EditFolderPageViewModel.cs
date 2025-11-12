using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OrionVault.Data;
using OrionVault.Interfaces;
using OrionVault.Messages;
using OrionVault.Models;
using OrionVault.Services;

namespace OrionVault.ViewModels
{
    public partial class EditFolderPageViewModel : ObservableValidator, IQueryAttributable
    {
        private readonly FolderDatabase _database;
        private readonly ILocalizationService _localizationService;
        private Folder _currentFolder;

        [ObservableProperty] private int folderId;
        [ObservableProperty][Required(ErrorMessage = "Título é obrigatório")] private string _title;
        [ObservableProperty] private Color _selectedColor = Colors.Blue;
        [ObservableProperty] private string _selectedColorHex = Colors.Blue.ToHex();
        [ObservableProperty] private bool _isColorPickerVisible = false;
        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private List<Folder> _folders = new();
        [ObservableProperty] private string _selectedFolderName;
        [ObservableProperty] private int? _selectedParentFolderId;

        // Localized Properties
        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string headerTitle;
        [ObservableProperty] private string headerSubtitle;
        [ObservableProperty] private string formSectionTitle;
        [ObservableProperty] private string formSectionSubtitle;
        [ObservableProperty] private string folderNameLabel;
        [ObservableProperty] private string folderNamePlaceholder;
        [ObservableProperty] private string colorLabel;
        [ObservableProperty] private string organizationLabel;
        [ObservableProperty] private string parentFolderLabel;
        [ObservableProperty] private string editButtonText;
        [ObservableProperty] private string cancelButtonText;
        [ObservableProperty] private string saveButtonText;
        [ObservableProperty] private string customColorTitle;
        [ObservableProperty] private string customColorSubtitle;
        [ObservableProperty] private string selectedColorLabel;
        [ObservableProperty] private string applyColorButtonText;

        public EditFolderPageViewModel(FolderDatabase database, ILocalizationService localizationService)
        {
            _database = database;
            _localizationService = localizationService;
            IsEditing = false;

            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(UpdateLocalizedTexts);
        }

        private void UpdateLocalizedTexts()
        {
            PageTitle = L.Text("edit_folder_page.title");
            HeaderTitle = L.Text("edit_folder_page.header_title");
            HeaderSubtitle = IsEditing ? L.Text("edit_folder_page.header_subtitle_edit") : L.Text("edit_folder_page.header_subtitle_view");
            FormSectionTitle = L.Text("edit_folder_page.form_title");
            FormSectionSubtitle = L.Text("edit_folder_page.form_subtitle");
            FolderNameLabel = L.Text("edit_folder_page.folder_name_label");
            FolderNamePlaceholder = L.Text("edit_folder_page.folder_name_placeholder");
            ColorLabel = L.Text("edit_folder_page.color_label");
            OrganizationLabel = L.Text("edit_folder_page.organization_label");
            ParentFolderLabel = L.Text("edit_folder_page.parent_folder_label");
            EditButtonText = L.Text("edit_folder_page.edit_button");
            CancelButtonText = L.Text("edit_folder_page.cancel_button");
            SaveButtonText = L.Text("edit_folder_page.save_button");
            CustomColorTitle = L.Text("edit_folder_page.custom_color_title");
            CustomColorSubtitle = L.Text("edit_folder_page.custom_color_subtitle");
            SelectedColorLabel = L.Text("edit_folder_page.selected_color_label");
            ApplyColorButtonText = L.Text("edit_folder_page.apply_color_button");

            if (SelectedFolderName == null)
            {
                SelectedFolderName = L.Text("accounts.select_folder");
            }
        }

        [RelayCommand]
        private async Task SelectFolderAsync()
        {
            Folders = await _database.GetRootFoldersAsync();

            if (Folders == null || Folders.Count == 0)
            {
                await Shell.Current.DisplayAlert(L.Text("common.warning"), L.Text("messages.no_root_folders_found"), L.Text("common.ok"));
                return;
            }

            var sortedFolders = Folders
                .Where(f => f.Id != _currentFolder.Id) // Exclude current folder
                .OrderBy(f => f.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var folderNames = sortedFolders.Select(f => f.Title).ToList();
            folderNames.Insert(0, L.Text("accounts.no_folder"));

            string chosenOption = await Shell.Current.DisplayActionSheet(L.Text("accounts.select_folder"), L.Text("common.cancel"), null, folderNames.ToArray());

            if (chosenOption != null && chosenOption != L.Text("common.cancel"))
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    L.Text("messages.confirm_move_folder_title"),
                    string.Format(L.Text("messages.confirm_move_folder"), chosenOption),
                    L.Text("common.yes"),
                    L.Text("common.cancel")
                );

                if (confirm)
                {
                    if (chosenOption == L.Text("accounts.no_folder"))
                    {
                        SelectedFolderName = L.Text("accounts.no_folder");
                        SelectedParentFolderId = null;
                    }
                    else
                    {
                        SelectedFolderName = chosenOption;
                        var selectedFolder = sortedFolders.First(f => f.Title == chosenOption);
                        SelectedParentFolderId = selectedFolder.Id;
                    }
                }
            }
        }

        [RelayCommand]
        private async Task SaveFolderAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Title))
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.fill_required_fields"), L.Text("common.ok"));
                    return;
                }

                // Verificar se já existe uma pasta com o mesmo nome no mesmo local (excluindo a pasta atual)
                bool folderExists = await _database.FolderNameExistsAsync(Title, SelectedParentFolderId, _currentFolder.Id);
                
                if (folderExists)
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.duplicate_folder_name"), L.Text("common.ok"));
                    return;
                }

                _currentFolder.Title = Title;
                _currentFolder.Color = SelectedColor.ToHex();
                _currentFolder.ParentFolderId = SelectedParentFolderId;

                await _database.SaveFolderAsync(_currentFolder);
                await Shell.Current.DisplayAlert(L.Text("common.success"), L.Text("messages.folder_updated_success"), L.Text("common.ok"));
                WeakReferenceMessenger.Default.Send(new FolderSavedMessage(true));
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
            }
        }

        [RelayCommand]
        private void ToggleColorPicker() => IsColorPickerVisible = !IsColorPickerVisible;

        [RelayCommand]
        private void CloseColorPicker() => IsColorPickerVisible = false;

        [RelayCommand]
        private void ToggleEditMode()
        {
            IsEditing = !IsEditing;
            HeaderSubtitle = IsEditing ? L.Text("edit_folder_page.header_subtitle_edit") : L.Text("edit_folder_page.header_subtitle_view");
        }


        partial void OnSelectedColorChanged(Color value) => SelectedColorHex = value.ToHex();
        partial void OnSelectedColorHexChanged(string value) => SelectedColor = Color.FromArgb(value);

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("folderId") && int.TryParse(query["folderId"]?.ToString(), out int folderId))
            {
                FolderId = folderId;
                _currentFolder = await _database.GetFolderAsync(FolderId);

                if (_currentFolder != null)
                {
                    Title = _currentFolder.Title;
                    SelectedColor = Color.FromArgb(_currentFolder.Color);
                    SelectedParentFolderId = _currentFolder.ParentFolderId;
                }

                if (SelectedParentFolderId.HasValue)
                {
                    var parentFolder = await _database.GetFolderAsync(SelectedParentFolderId.Value);
                    SelectedFolderName = parentFolder?.Title ?? L.Text("accounts.folder_not_found");
                }
                else
                {
                    SelectedFolderName = L.Text("accounts.no_folder");
                }
            }
        }

        ~EditFolderPageViewModel()
        {
            if (_localizationService != null)
                _localizationService.LanguageChanged -= OnLanguageChanged;
        }
    }
}