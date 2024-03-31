using Android.App;
using Android.Content.PM;

namespace FileEncryptor.Platforms.Android;
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
//[IntentFilter(["android.intent.action.MAIN"], Categories = ["android.intent.category.LAUNCHER"], DataScheme = "file", DataHost = "*", DataPathPattern = ".*\\.lcenc")]
public class MainActivity : MauiAppCompatActivity {
}
