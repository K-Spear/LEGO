using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaritimePlatform.Core.Services;

namespace MaritimePlatform.App.ViewModels
{
    public class RoadmapPlaceholderModule : IModulePlugin
    {
        public string ModuleId { get; }
        public string DisplayName { get; }
        public string ShortBadge { get; }
        public string IconResourceKey { get; }
        public int DisplayOrder { get; }
        public bool IsImplemented => false;
        private readonly string _regulationSummary;
        private readonly string _dataLinkPoint;

        public RoadmapPlaceholderModule(
            string moduleId,
            string displayName,
            string shortBadge,
            int order,
            string regulationSummary,
            string dataLinkPoint)
        {
            ModuleId = moduleId;
            DisplayName = displayName;
            ShortBadge = shortBadge;
            IconResourceKey = $"Icon_{moduleId}";
            DisplayOrder = order;
            _regulationSummary = regulationSummary;
            _dataLinkPoint = dataLinkPoint;
        }

        public object CreateMainView()
        {
            var root = new Border
            {
                Background = Brushes.White,
                Padding = new Thickness(32)
            };

            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(248, 155, 108)),
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(28),
                VerticalAlignment = VerticalAlignment.Top
            };

            var stack = new StackPanel();
            stack.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(243, 115, 33)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12),
                Child = new TextBlock
                {
                    Text = $"[{ShortBadge}] 확장 플러그인 모듈 (Modular Shell Ready)",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12
                }
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"{DisplayName} 모듈 아키텍처 슬롯",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                Margin = new Thickness(0, 0, 0, 8)
            });

            stack.Children.Add(new TextBlock
            {
                Text = _regulationSummary,
                FontSize = 13.5,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 14)
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"• 공통 데이터 연계 지점 (Core Data Lake): {_dataLinkPoint}\n" +
                       $"• 플러그인 결합 방식: IModulePlugin 인터페이스를 구현하여 단일 EXE(MaritimePlatform.App)에 재작업 없이 즉시 마운트됩니다.",
                FontSize = 12.5,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                TextWrapping = TextWrapping.Wrap
            });

            card.Child = stack;
            root.Child = card;
            return root;
        }

        public Task InitializeAsync() => Task.CompletedTask;
        public void OnNavigatedTo() { }
        public void OnNavigatedFrom() { }
    }
}
