using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MaritimePlatform.Core.Models;
using MaritimePlatform.Core.Services;
using MaritimePlatform.Modules.ImoDcs;

namespace MaritimePlatform.App.ViewModels
{
    public class ModuleNavItem : INotifyPropertyChanged
    {
        private bool _isActive;
        public IModulePlugin Plugin { get; }

        public string ModuleId => Plugin.ModuleId;
        public string DisplayName => Plugin.DisplayName;
        public string ShortBadge => Plugin.ShortBadge;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public ModuleNavItem(IModulePlugin plugin)
        {
            Plugin = plugin;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ShellViewModel : INotifyPropertyChanged
    {
        private readonly ImoDcsModule _imoDcsModule;
        private object? _currentModuleView;
        private ModuleNavItem? _activeNavItem;

        public ObservableCollection<ModuleNavItem> Modules { get; } = new();
        public ObservableCollection<VesselInfo> FleetVessels => _imoDcsModule.ViewModel.FleetVessels;
        public ObservableCollection<int> AvailableYears => _imoDcsModule.ViewModel.AvailableYears;

        public VesselInfo SelectedVessel
        {
            get => _imoDcsModule.ViewModel.SelectedVessel;
            set
            {
                if (_imoDcsModule.ViewModel.SelectedVessel != value)
                {
                    _imoDcsModule.ViewModel.SelectedVessel = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedYear
        {
            get => _imoDcsModule.ViewModel.SelectedYear;
            set
            {
                if (_imoDcsModule.ViewModel.SelectedYear != value)
                {
                    _imoDcsModule.ViewModel.SelectedYear = value;
                    OnPropertyChanged();
                }
            }
        }

        public object? CurrentModuleView
        {
            get => _currentModuleView;
            private set
            {
                _currentModuleView = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateModuleCommand { get; }
        public ICommand PrevYearCommand { get; }
        public ICommand NextYearCommand { get; }

        public ShellViewModel(IEnumerable<IModulePlugin> plugins, ImoDcsModule imoDcsModule)
        {
            _imoDcsModule = imoDcsModule;

            foreach (var plugin in plugins.OrderBy(p => p.DisplayOrder))
            {
                Modules.Add(new ModuleNavItem(plugin));
            }

            NavigateModuleCommand = new RelayCommand(param =>
            {
                if (param is ModuleNavItem item)
                {
                    SwitchModule(item);
                }
            });

            PrevYearCommand = new RelayCommand(_ =>
            {
                int idx = AvailableYears.IndexOf(SelectedYear);
                if (idx > 0)
                    SelectedYear = AvailableYears[idx - 1];
            });

            NextYearCommand = new RelayCommand(_ =>
            {
                int idx = AvailableYears.IndexOf(SelectedYear);
                if (idx >= 0 && idx < AvailableYears.Count - 1)
                    SelectedYear = AvailableYears[idx + 1];
            });

            if (Modules.Count > 0)
            {
                SwitchModule(Modules[0]);
            }
        }

        public async Task InitializeAllModulesAsync()
        {
            foreach (var item in Modules)
            {
                await item.Plugin.InitializeAsync();
            }
        }

        private void SwitchModule(ModuleNavItem target)
        {
            if (_activeNavItem == target)
                return;

            _activeNavItem?.Plugin.OnNavigatedFrom();
            foreach (var m in Modules)
            {
                m.IsActive = (m == target);
            }

            _activeNavItem = target;
            _activeNavItem.Plugin.OnNavigatedTo();
            CurrentModuleView = _activeNavItem.Plugin.CreateMainView();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
