namespace TastyMealPlanner.Helpers;

/// <summary>Auto-advances a CarouselView on a timer interval. Disposable to clean up on page disappear.</summary>
public class CarouselAutoPlayer : IDisposable
{
    private readonly CarouselView _carousel;
    private IDispatcherTimer? _timer;

    public CarouselAutoPlayer(CarouselView carousel, int intervalSeconds = 3)
    {
        _carousel = carousel;
        _timer = carousel.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(intervalSeconds);
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_carousel.ItemsSource is System.Collections.IList list && list.Count > 0)
            _carousel.Position = (_carousel.Position + 1) % list.Count;
    }

    public void Dispose()
    {
        _timer?.Stop();
        _timer = null;
    }
}
