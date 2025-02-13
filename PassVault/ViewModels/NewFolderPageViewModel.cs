using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.ViewModels
{
    public partial class NewFolderPageViewModel : ObservableValidator
    {
        private readonly FolderDatabase _database;

        [ObservableProperty]
        [Required(ErrorMessage = "Título é obrigatório")]
        private string _title;

        [ObservableProperty]
        private Color _selectedColor = Colors.Purple;

        [ObservableProperty]
        private bool _isColorPickerVisible = false;

        public NewFolderPageViewModel(FolderDatabase database)
        {
            _database = database;
        }

        [RelayCommand]
        public async Task SaveFolderAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Title))
                {
                    await Shell.Current.DisplayAlert("Erro", "O campo Titulo é obrigatório", "OK");
                    return;
                }

                var folder = new Folder
                {
                    Title = Title,
                    Color = SelectedColor.ToHex(),
                };

                await _database.SaveFolderAsync(folder);
                await Shell.Current.DisplayAlert("Sucesso", "Pasta criada com sucesso", "OK");
                WeakReferenceMessenger.Default.Send(new FolderSavedMessage(true));
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private static async Task Close() => await Shell.Current.GoToAsync("..");

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

        partial void OnSelectedColorChanged(Color value)
        {
            // Força a atualização da interface
            OnPropertyChanged(nameof(SelectedColor));
        }
    }
}

