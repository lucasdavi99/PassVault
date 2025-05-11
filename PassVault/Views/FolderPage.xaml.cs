using PassVault.ViewModels;

namespace PassVault.Views;

public partial class FolderPage : ContentPage, IQueryAttributable
{
    public FolderPage(FolderPageViewModel viewModel)
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
                ((FolderPageViewModel)BindingContext).FolderId = folderId;
                _ = ((FolderPageViewModel)BindingContext).LoadDataAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Falha na conversão do folderId.");
            }
        }
    }
}