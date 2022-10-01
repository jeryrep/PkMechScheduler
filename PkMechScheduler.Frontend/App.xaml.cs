using PkMechScheduler.Frontend.Enums;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace PkMechScheduler.Frontend;

public partial class App
{
    private const int WindowWidth = 1366;
    private const int WindowHeight = 766;
    public App()
    {
        InitializeComponent();
        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
#if WINDOWS
            var mauiWindow = handler.VirtualView;
            var nativeWindow = handler.PlatformView;
            nativeWindow.Activate();
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(WindowWidth, WindowHeight));
#endif
        });

        if (!Preferences.ContainsKey(((char)SubjectType.Exercise).ToString()))
            Preferences.Set(((char)SubjectType.Exercise).ToString(), "Ć01");
        if (!Preferences.ContainsKey(((char)SubjectType.Lecture).ToString()))
            Preferences.Set(((char)SubjectType.Lecture).ToString(), "W01");
        if (!Preferences.ContainsKey(((char)SubjectType.Laboratory).ToString()))
            Preferences.Set(((char)SubjectType.Laboratory).ToString(), "L01");
        if (!Preferences.ContainsKey(((char)SubjectType.ComputersLaboratory).ToString()))
            Preferences.Set(((char)SubjectType.ComputersLaboratory).ToString(), "K01");
        if (!Preferences.ContainsKey(((char)SubjectType.Projects).ToString()))
            Preferences.Set(((char)SubjectType.Projects).ToString(), "P01");
        if (!Preferences.ContainsKey(((char)SubjectType.Seminars).ToString()))
            Preferences.Set(((char)SubjectType.Seminars).ToString(), "S01");

        MainPage = new AppShell();
    }
}