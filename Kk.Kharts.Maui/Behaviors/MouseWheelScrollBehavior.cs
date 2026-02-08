using Microsoft.Maui.Controls;

namespace Kk.Kharts.Maui.Behaviors;

/// <summary>
/// Behavior that enables mouse wheel scrolling for CollectionView on Windows.
/// This is a workaround for a known .NET MAUI limitation where CollectionView
/// doesn't respond to mouse wheel events on Windows desktop.
/// </summary>
public class MouseWheelScrollBehavior : Behavior<CollectionView>
{
    private CollectionView? _collectionView;

    protected override void OnAttachedTo(CollectionView bindable)
    {
        base.OnAttachedTo(bindable);
        _collectionView = bindable;

#if WINDOWS
        // Only apply on Windows platform
        if (_collectionView.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.ListView listView)
        {
            listView.PointerWheelChanged += OnPointerWheelChanged;
        }
        else
        {
            // If handler not ready, subscribe to HandlerChanged
            _collectionView.HandlerChanged += OnHandlerChanged;
        }
#endif
    }

    protected override void OnDetachingFrom(CollectionView bindable)
    {
        base.OnDetachingFrom(bindable);

#if WINDOWS
        if (_collectionView != null)
        {
            _collectionView.HandlerChanged -= OnHandlerChanged;
            
            if (_collectionView.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.ListView listView)
            {
                listView.PointerWheelChanged -= OnPointerWheelChanged;
            }
        }
#endif
        _collectionView = null;
    }

#if WINDOWS
    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        if (_collectionView?.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.ListView listView)
        {
            listView.PointerWheelChanged += OnPointerWheelChanged;
            _collectionView.HandlerChanged -= OnHandlerChanged;
        }
    }

    private void OnPointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not Microsoft.UI.Xaml.Controls.ListView listView)
            return;

        var scrollViewer = FindScrollViewer(listView);
        if (scrollViewer == null)
            return;

        var delta = e.GetCurrentPoint(listView).Properties.MouseWheelDelta;
        var scrollAmount = delta * 0.5; // Adjust scroll sensitivity

        scrollViewer.ChangeView(null, scrollViewer.VerticalOffset - scrollAmount, null, false);
        e.Handled = true;
    }

    private Microsoft.UI.Xaml.Controls.ScrollViewer? FindScrollViewer(Microsoft.UI.Xaml.DependencyObject element)
    {
        if (element is Microsoft.UI.Xaml.Controls.ScrollViewer scrollViewer)
            return scrollViewer;

        var childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childCount; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(element, i);
            var result = FindScrollViewer(child);
            if (result != null)
                return result;
        }

        return null;
    }
#endif
}
