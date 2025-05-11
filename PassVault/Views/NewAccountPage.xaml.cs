using PassVault.ViewModels;

namespace PassVault.Views;

public partial class NewAccountPage : ContentPage, IQueryAttributable
{
    public NewAccountPage(NewAccountPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("folderId", out var folderIdObj))
        {
            if (int.TryParse(folderIdObj?.ToString(), out int folderId))
            {
                // Define a propriedade FolderId no ViewModel
                ((NewAccountPageViewModel)BindingContext).FolderId = folderId;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Falha na conversão do folderId.");
            }
        }
    }
}