using System.Security.Cryptography;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel;
using OrionVault.Interfaces;
using OrionVault.Messages;
using OrionVault.Services;

namespace OrionVault.ViewModels
{
    public partial class PasswordGeneratorViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;

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

        // Localized Properties
        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string pageSubtitle;
        [ObservableProperty] private string configTitle;
        [ObservableProperty] private string minLengthLabel;
        [ObservableProperty] private string minLengthPlaceholder;
        [ObservableProperty] private string maxLengthLabel;
        [ObservableProperty] private string maxLengthPlaceholder;
        [ObservableProperty] private string includeNumbersLabel;
        [ObservableProperty] private string includeSpecialCharsLabel;
        [ObservableProperty] private string generateButtonText;
        [ObservableProperty] private string generatedPasswordTitle;
        [ObservableProperty] private string copyPasswordButtonText;
        [ObservableProperty] private string copyPasswordTooltip;
        [ObservableProperty] private string securityTipsTitle;
        [ObservableProperty] private string tip1;
        [ObservableProperty] private string tip2;
        [ObservableProperty] private string tip3;
        [ObservableProperty] private string tip4;

        private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Numbers = "0123456789";
        private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        public PasswordGeneratorViewModel(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += (s, e) => UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            PageTitle = L.Text("password_generator.title");
            PageSubtitle = L.Text("password_generator.subtitle");
            ConfigTitle = L.Text("password_generator.config_title");
            MinLengthLabel = L.Text("password_generator.min_length");
            MinLengthPlaceholder = L.Text("password_generator.min_length_placeholder");
            MaxLengthLabel = L.Text("password_generator.max_length");
            MaxLengthPlaceholder = L.Text("password_generator.max_length_placeholder");
            IncludeNumbersLabel = L.Text("password_generator.include_numbers");
            IncludeSpecialCharsLabel = L.Text("password_generator.include_special_chars");
            GenerateButtonText = L.Text("password_generator.generate_button");
            GeneratedPasswordTitle = L.Text("password_generator.generated_password_title");
            CopyPasswordButtonText = L.Text("password_generator.copy_button");
            CopyPasswordTooltip = L.Text("password_generator.copy_tooltip");
            SecurityTipsTitle = L.Text("password_generator.security_tips_title");
            Tip1 = L.Text("password_generator.tip1");
            Tip2 = L.Text("password_generator.tip2");
            Tip3 = L.Text("password_generator.tip3");
            Tip4 = L.Text("password_generator.tip4");
        }


        [RelayCommand]
        private async Task GeneratePasswordAsync()
        {
            try
            {
                if (!ValidateInputs()) return;
                GeneratedPassword = await Task.Run(() => GenerateSecurePassword());

                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                {
                    await Shell.Current.DisplayAlert(L.Text("common.success"), L.Text("password_generator.success_message"), L.Text("common.ok"));
                }
            }
            catch (Exception ex)
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                {
                    await Shell.Current.DisplayAlert(L.Text("common.error"), string.Format(L.Text("password_generator.error_message"), ex.Message), L.Text("common.ok"));
                }
            }
        }

        [RelayCommand]
        private async Task CopyPassword()
        {
            if (string.IsNullOrEmpty(GeneratedPassword))
                return;

            if (await TryCopyToClipboardAsync(GeneratedPassword))
            {
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
                    Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("password_generator.min_length_error"), L.Text("common.ok"));
                }
                return false;
            }

            if (MaxLength < MinLength)
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                {
                    Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("password_generator.max_length_error"), L.Text("common.ok"));
                }
                return false;
            }
            return true;
        }

        private static async Task<bool> TryCopyToClipboardAsync(string text)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() => Clipboard.Default.SetTextAsync(text));
                return true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
                return false;
            }
        }
    }
}
