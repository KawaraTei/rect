using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LectImageRenamer.Models;
using LectImageRenamer.ViewModels;

namespace LectImageRenamer;

public partial class MainWindow : Window
{
    private const string ImageItemsDragFormat = "LectImageRenamer.ImageItems";

    private readonly MainWindowViewModel _viewModel;
    private Point _dragStartPoint;
    private ImageItem? _dragStartItem;
    private PreviewWindow? _previewWindow;
    private bool _suppressPreviewUpdates;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        _viewModel.ImagesDeleting += OnImagesDeleting;
        _viewModel.ImagesDeleted += OnImagesDeleted;
        _viewModel.ImagesCleared += OnImagesCleared;
        DataContext = _viewModel;
    }

    private void OnImagesDeleting(object? sender, EventArgs e)
    {
        _suppressPreviewUpdates = true;
        _previewWindow?.ClearImage();
    }

    private void OnImagesDeleted(object? sender, EventArgs e)
    {
        _suppressPreviewUpdates = false;
    }

    private void OnImagesCleared(object? sender, EventArgs e)
    {
        _previewWindow?.ClearImage();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete || IsTextInputElement(e.OriginalSource as DependencyObject))
        {
            return;
        }

        if (_viewModel.DeleteSelectedCommand.CanExecute(null))
        {
            _viewModel.DeleteSelectedCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void ImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _viewModel.NotifySelectionChanged();

        if (_suppressPreviewUpdates)
        {
            return;
        }

        IReadOnlyList<ImageItem> selectedItems = _viewModel.SelectedItems;
        if (selectedItems.Count != 1)
        {
            return;
        }

        _previewWindow ??= new PreviewWindow();
        _previewWindow.Closed += (_, _) => _previewWindow = null;
        _previewWindow.ShowImage(selectedItems[0]);
    }

    private void ImageList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _dragStartItem = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject)?.DataContext as ImageItem;
    }

    private void ImageList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _dragStartItem is null)
        {
            return;
        }

        Point currentPoint = e.GetPosition(null);
        if (Math.Abs(currentPoint.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(currentPoint.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        List<ImageItem> movingItems = _dragStartItem.IsSelected
            ? _viewModel.SelectedItems.ToList()
            : [_dragStartItem];

        DataObject dataObject = new();
        dataObject.SetData(ImageItemsDragFormat, movingItems);
        DragDrop.DoDragDrop((DependencyObject)sender, dataObject, DragDropEffects.Move);
        _dragStartItem = null;
    }

    private void ImageList_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
            return;
        }

        if (e.Data.GetDataPresent(ImageItemsDragFormat))
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
            return;
        }

        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void ImageList_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
            e.Data.GetData(DataFormats.FileDrop) is string[] paths)
        {
            _viewModel.AddPaths(paths);
            e.Handled = true;
            return;
        }

        if (e.Data.GetDataPresent(ImageItemsDragFormat) &&
            e.Data.GetData(ImageItemsDragFormat) is IReadOnlyList<ImageItem> movingItems)
        {
            ImageItem? targetItem = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject)?.DataContext as ImageItem;
            int targetIndex = targetItem is null ? _viewModel.Images.Count - 1 : _viewModel.Images.IndexOf(targetItem);
            _viewModel.MoveItems(movingItems, targetIndex);
            e.Handled = true;
        }
    }

    private static T? FindAncestor<T>(DependencyObject? dependencyObject)
        where T : DependencyObject
    {
        while (dependencyObject is not null)
        {
            if (dependencyObject is T ancestor)
            {
                return ancestor;
            }

            dependencyObject = System.Windows.Media.VisualTreeHelper.GetParent(dependencyObject);
        }

        return null;
    }

    private static bool IsTextInputElement(DependencyObject? dependencyObject)
    {
        return FindAncestor<TextBox>(dependencyObject) is not null ||
               FindAncestor<PasswordBox>(dependencyObject) is not null ||
               FindAncestor<RichTextBox>(dependencyObject) is not null;
    }
}
