using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.exceptions;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.ViewModels
{
    public partial class BackupViewModel : ObservableObject
    {
        private readonly ExportService _exportService;
        private readonly ImportService _importService;
        private readonly AccountDatabase _accountDatabase;
        private readonly FolderDatabase _folderDatabase;

        [ObservableProperty]
        private string exportFilePath;

        [ObservableProperty]
        private string exportPassword;

        [ObservableProperty]
        private string importFilePath;

        [ObservableProperty]
        private string importPassword;

        public BackupViewModel(ExportService exportService, ImportService importService, AccountDatabase accountDatabase, FolderDatabase folderDatabase)
        {
            _exportService = exportService;
            _importService = importService;
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
        }

        [RelayCommand]
        public async Task ExportBackupAsync()
        {
            var accounts = await _accountDatabase.GetAccountsAsync();
            var folders = await _folderDatabase.GetFoldersAsync();

            var (filePath, password) = await _exportService.ExportBackupAsync(accounts, folders);

            ExportFilePath = filePath;
            ExportPassword = password;

            await Shell.Current.DisplayAlert("Backup Exportado",$"Senha: {password}", "OK");
        }

        [RelayCommand]
        public async Task ImportBackupAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o arquivo de backup",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "application/octet-stream" } },
                        { DevicePlatform.iOS, new[] { "public.data" } },
                        { DevicePlatform.WinUI, new[] { ".dat" } }
                    })
                });

                if (result == null) return;

                string filePath = result.FullPath;

                if (string.IsNullOrEmpty(filePath))
                {
                    using (var stream = await result.OpenReadAsync())
                    {
                        filePath = Path.Combine(FileSystem.AppDataDirectory, result.FileName);
                        using (var fileStream = File.Create(filePath))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }

                ImportFilePath = filePath;

                string senha = await Shell.Current.DisplayPromptAsync("Senha de Importação", "Digite a senha para importar o backup:");
                ImportPassword = senha;

                if (string.IsNullOrWhiteSpace(senha))
                {
                    await Shell.Current.DisplayAlert("Erro", "A senha é obrigatória para importar o backup.", "OK");
                    return;
                }

                BackupData backupData = await _importService.ImportBackupAsync(filePath, senha);

                Dictionary<int, int> folderIdMapping = new Dictionary<int, int>();

                foreach (var folder in backupData.Folders)
                {
                    int oldId = folder.Id;

                    folder.Id = 0;
                    await _folderDatabase.SaveFolderAsync(folder);

                    int newId = folder.Id;
                    folderIdMapping.Add(oldId, newId);
                }

                foreach (var account in backupData.Accounts)
                {
                    if (account.FolderId.HasValue && folderIdMapping.ContainsKey(account.FolderId.Value))
                    {
                        account.FolderId = folderIdMapping[account.FolderId.Value];
                    }
                    else
                    {
                        account.FolderId = null;
                    }

                    account.Id = 0;
                    await _accountDatabase.SaveAccountAsync(account);
                }

                WeakReferenceMessenger.Default.Send(new AccountSavedMessage(true));
                WeakReferenceMessenger.Default.Send(new FolderSavedMessage(true));

                await Shell.Current.DisplayAlert("Importado", "Backup importado com sucesso!", "OK");
                await Shell.Current.Navigation.PopAsync();
            }
            catch (InvalidImportPasswordException)
            {
                await Shell.Current.DisplayAlert("Erro", "A senha fornecida está incorreta.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
            }
        }
    }
}