using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.ViewModels
{
    public partial class PasswordGeneratorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _generatedPassword = string.Empty;

        [ObservableProperty]
        private int _minLength = 4;

        [ObservableProperty]
        private int _maxLength = 16;

        [ObservableProperty]
        private bool _includeNumbers = true;

        [ObservableProperty]
        private bool _includeSpecialChars = true;

        private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Numbers = "0123456789";
        private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        [RelayCommand]
        private async Task GeneratePasswordAsync()
        {
            try
            {
                if (!ValidateInputs()) return;
                GeneratedPassword = await Task.Run(() => GenerateSecurePassword());

                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                {
                    await Shell.Current.DisplayAlert("Sucesso", "Senha gerada com sucesso!", "OK");
                }
            }
            catch (Exception ex)
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                {
                    await Shell.Current.DisplayAlert("Erro", $"Falha ao gerar senha: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task CopyPassword()
        {
            if (!string.IsNullOrEmpty(GeneratedPassword))
            {
                await Clipboard.Default.SetTextAsync(GeneratedPassword);
                
                WeakReferenceMessenger.Default.Send(new PasswordGeneratedMessage(GeneratedPassword));
                               
                await Shell.Current.GoToAsync("..");
            }
        }

        private string GenerateSecurePassword()
        {
            var passwordChars = new List<char>();
            var charPool = BuildCharacterPool();

            
            if (IncludeNumbers) passwordChars.Add(GetRandomChar(Numbers));
            if (IncludeSpecialChars) passwordChars.Add(GetRandomChar(SpecialChars));
            passwordChars.Add(GetRandomChar(LowerCase));
            passwordChars.Add(GetRandomChar(UpperCase));

            
            var remainingLength = RandomNumberGenerator.GetInt32(MinLength, MaxLength + 1) - passwordChars.Count;
            for (var i = 0; i < remainingLength; i++)
            {
                passwordChars.Add(GetRandomChar(charPool));
            }

            
            return new string(passwordChars.OrderBy(c => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
        }
        
        private string BuildCharacterPool()
        {
            var pool = LowerCase + UpperCase;
            if (IncludeNumbers) pool += Numbers;
            if (IncludeSpecialChars) pool += SpecialChars;
            return pool;
        }

        private static char GetRandomChar(string validChars)
        {
            return validChars[RandomNumberGenerator.GetInt32(validChars.Length)];
        }       

        private bool ValidateInputs()
        {
            if (MinLength < 4)
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                {
                    Shell.Current.DisplayAlert("Erro", "O comprimento mínimo deve ser pelo menos 4 caracteres", "OK");
                }
                return false;
            }

            if (MaxLength < MinLength)
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                {
                    Shell.Current.DisplayAlert("Erro", "O comprimento máximo deve ser maior ou igual a o mínimo", "OK");
                }
                return false;
            }
            return true;
        }
    }
}