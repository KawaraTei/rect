using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using LectImageRenamer.Models;

namespace LectImageRenamer;

public partial class PreviewWindow : Window, INotifyPropertyChanged
{
    private BitmapImage? _previewImage;
    private string _previewPath = string.Empty;

    public PreviewWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public BitmapImage? PreviewImage
    {
        get => _previewImage;
        private set
        {
            _previewImage = value;
            OnPropertyChanged();
        }
    }

    public string PreviewPath
    {
        get => _previewPath;
        private set
        {
            _previewPath = value;
            OnPropertyChanged();
        }
    }

    public void ShowImage(ImageItem item)
    {
        if (!File.Exists(item.FullPath))
        {
            ClearImage();
            return;
        }

        PreviewImage = ImageItem.LoadBitmap(item.FullPath);
        PreviewPath = item.FullPath;
        Title = $"プレビュー - {item.FileName}";

        if (!IsVisible)
        {
            Show();
        }

        Activate();
    }

    public void ClearImage()
    {
        PreviewImage = null;
        PreviewPath = string.Empty;
        Title = "プレビュー";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
