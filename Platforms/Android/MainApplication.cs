﻿using Android.App;
using Android.Runtime;

namespace FileEncryptor.Platforms.Android;
[Application]
public class MainApplication(nint handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership) {
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}