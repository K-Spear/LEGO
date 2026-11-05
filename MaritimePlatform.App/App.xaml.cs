using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MaritimePlatform.App.ViewModels;
using MaritimePlatform.Core.Services;
using MaritimePlatform.Modules.ImoDcs;

namespace MaritimePlatform.App
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. UI 스레드 렌더링/이벤트 예외 방어
            this.DispatcherUnhandledException += (s, args) =>
            {
                LogCrash("DISPATCHER_UNHANDLED", args.Exception);
                MessageBox.Show($"일시적인 렌더링 또는 UI 이벤트 오류가 감지되어 복구되었습니다.\n\n내용: {args.Exception.Message}",
                    "플랫폼 안전 보호", MessageBoxButton.OK, MessageBoxImage.Warning);
                args.Handled = true; // 프로세스 종료 차단
            };

            // 2. 백그라운드 스레드 도메인 예외 방어
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    LogCrash("DOMAIN_UNHANDLED", ex);
                }
            };

            // 3. 비동기 Task Unobserved 예외 방어
            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                LogCrash("ASYNC_TASK_UNOBSERVED", args.Exception);
                args.SetObserved(); // 크래시 차단
            };

            // Core Data Lake & Plugin Registration
            IVesselDataRepository repository = new InMemoryVesselDataRepository();
            var imoDcsModule = new ImoDcsModule(repository);

            var plugins = new List<IModulePlugin>
            {
                imoDcsModule,
                new RoadmapPlaceholderModule(
                    "EU_MRV",
                    "EU MRV",
                    "MRV",
                    2,
                    "Regulation (EU) 2015/757 기반 유럽경제지역(EEA) 기항 항차(50% In/Out, 100% Intra-EU & Berth) CO2/CH4/N2O 배출량 모니터링 및 THETIS-MRV 연간 배출량 보고서(ER) 산출 파트입니다.",
                    "IMO DCS 항차/Noon Report 데이터의 UN/LOCODE 기준 50%/100% 스코프 자동 필터링"),
                new RoadmapPlaceholderModule(
                    "CII_MGMT",
                    "CII 등급",
                    "CII",
                    3,
                    "MARPOL Annex VI Reg.28 및 SEEMP Part III(2026~2028 차기 3개년 계획 포함) 기반 실시간 달성 AER/CII 추적, 연말 A~E 등급 시뮬레이터 및 감속 운항(Just-in-Time) 최적화 제언 파트입니다.",
                    "연간 누적 운항거리(NM), DWT/GT 제원 및 유종별 CO2 배출량 실시간 연동"),
                new RoadmapPlaceholderModule(
                    "EU_ETS",
                    "EU ETS",
                    "ETS",
                    4,
                    "EU ETS 해운 부문 배출권(EUA) 납부 수량 산정 및 FuelEU Maritime WtW(Well-to-Wake) 온실가스 집약도(gCO2eq/MJ) 과부족(Surplus/Deficit/Pooling) 비용 정산 파트입니다.",
                    "EU MRV 검증 완료된 CO2eq 배출량 및 연료별 LCV/WtW 계수 직결")
            };

            var shellVm = new ShellViewModel(plugins, imoDcsModule);
            await shellVm.InitializeAllModulesAsync();

            var mainWindow = new MainWindow
            {
                DataContext = shellVm
            };
            mainWindow.Show();
        }

        private static void LogCrash(string type, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FATAL CRASH PREVENTED] [{type}] {ex}");
        }
    }
}
