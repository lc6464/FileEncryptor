using System.Text;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;

namespace FileEncryptor;

public partial class MainPage : ContentPage {
	public MainPage() => InitializeComponent();

	private async void OnEncryptClicked(object sender, EventArgs e) {
		if (string.IsNullOrEmpty(PasswordInput.Text)) {
			using var toast = Toast.Make("没有输入密码！");
			await toast.Show().ConfigureAwait(false);
			return;
		}
		try {
			var result = await FilePicker.Default.PickAsync();
			if (result != null) {
				if (result.FileName.EndsWith(".lcenc", StringComparison.OrdinalIgnoreCase)) { // 解密
																							  // LCEN(4) + 扩展名长度(1) + 扩展名(0~255) + 初始化向量(16) + 加密数据(16+)
					try {
						using var file = await result.OpenReadAsync().ConfigureAwait(false);
						var bytes = new byte[255];
						file.Read(bytes, 0, 4);
						if (file.Length < 37 || !bytes[..4].SequenceEqual("LCEN"u8.ToArray())) { // 判断是否足够长并以 LCEN 开头
							using var toast0 = Toast.Make("解密失败，文件已损坏。");
							await toast0.Show().ConfigureAwait(false);
							return;
						}
						file.Read(bytes, 0, 1); // 读取扩展名的长度
						var extensionBytesLength = bytes[0];
						string? extension = null;
						if (extensionBytesLength > 0) {
							file.Read(bytes, 0, extensionBytesLength); // 读取扩展名
							extension = Encoding.UTF8.GetString(bytes, 0, extensionBytesLength);
						}

						file.Read(bytes, 0, 16); // 读取初始化向量
						var iv = bytes[..16];
						var passwordHash = Encryptor.HashPassword(PasswordInput.Text, iv);

						using MemoryStream memoryStream = new();

						Encryptor.Decrypt256(file, memoryStream, passwordHash, iv);

						using var toast = Toast.Make("解密成功！");
						await toast.Show().ConfigureAwait(false);

						var fileSaverResult = await FileSaver.Default.SaveAsync($"{Path.GetFileNameWithoutExtension(result.FileName)}{(extension is null ? "" : $".{extension}")}", memoryStream);
						if (fileSaverResult.IsSuccessful) {
							using var toast0 = Toast.Make("保存成功！");
							await toast0.Show().ConfigureAwait(false);
						} else {
							using var toast0 = Toast.Make($"保存失败：{fileSaverResult.Exception.Message}");
							await toast0.Show().ConfigureAwait(false);
						}
					} catch (Exception ex) {
						using var toast = Toast.Make($"解密失败：{ex.Message}");
						await toast.Show().ConfigureAwait(false);
					}
				} else { // 加密
					try {
						using var file = await result.OpenReadAsync().ConfigureAwait(false);
						var iv = Encryptor.GetRandomIV();
						var passwordHash = Encryptor.HashPassword(PasswordInput.Text, iv);
						var extension = Path.GetExtension(result.FileName)[1..]; // 获取并编码扩展名
						var extensionBytesLength = Encoding.UTF8.GetByteCount(extension);
						if (extensionBytesLength > 255) {
							using var toast0 = Toast.Make("加密失败，扩展名过长。");
							await toast0.Show().ConfigureAwait(false);
							return;
						}

						using MemoryStream memoryStream = new();
						memoryStream.Write("LCEN"u8); // 写入 LCEN
						memoryStream.WriteByte((byte)extensionBytesLength); // 扩展名长度
						memoryStream.Write(Encoding.UTF8.GetBytes(extension)); // 写入扩展名
						memoryStream.Write(iv); // 写入初始化向量

						Encryptor.Encrypt256(file, memoryStream, passwordHash, iv);

						using var toast = Toast.Make("加密成功！");
						await toast.Show().ConfigureAwait(false);

						var fileSaverResult = await FileSaver.Default.SaveAsync($"{Path.GetFileNameWithoutExtension(result.FileName)}.lcenc", memoryStream);
						if (fileSaverResult.IsSuccessful) {
							using var toast0 = Toast.Make("保存成功！");
							await toast0.Show().ConfigureAwait(false);
						} else {
							using var toast0 = Toast.Make($"保存失败：{fileSaverResult.Exception.Message}");
							await toast0.Show().ConfigureAwait(false);
						}
					} catch (Exception ex) {
						using var toast = Toast.Make($"加密失败：{ex.Message}");
						await toast.Show().ConfigureAwait(false);
					}
				}
			}
		} catch (Exception ex) {
			using var toast = Toast.Make($"选择文件失败：{ex.Message}");
			await toast.Show().ConfigureAwait(false);
		}
	}

	private async void OnGetPasswordClicked(object sender, EventArgs e) {
		try {
			var password = await SecureStorage.Default.GetAsync("password").ConfigureAwait(false);
			if (password == null) {
				MainThread.BeginInvokeOnMainThread(() => GetPassword.Text = "没有保存密码");
			} else {
				MainThread.BeginInvokeOnMainThread(() => {
					PasswordInput.Text = password;
					GetPassword.Text = "读取成功";
				});
			}
		} catch {
			MainThread.BeginInvokeOnMainThread(() => GetPassword.Text = "读取失败");

			SecureStorage.Default.Remove("password");
		}

		await Task.Delay(1000).ConfigureAwait(false);

		MainThread.BeginInvokeOnMainThread(() => GetPassword.Text = "读取密码");
	}

	private async void OnSavePasswordClicked(object sender, EventArgs e) {
		if (string.IsNullOrEmpty(PasswordInput.Text)) {
			MainThread.BeginInvokeOnMainThread(() => SavePassword.Text = "没有输入密码");
		} else {
			try {
				await SecureStorage.Default.SetAsync("password", PasswordInput.Text).ConfigureAwait(false);
				MainThread.BeginInvokeOnMainThread(() => SavePassword.Text = "保存成功");
			} catch {
				if (SecureStorage.Default.Remove("password")) {
					try {
						await SecureStorage.Default.SetAsync("password", PasswordInput.Text).ConfigureAwait(false);
						MainThread.BeginInvokeOnMainThread(() => SavePassword.Text = "保存成功");
					} catch {
						MainThread.BeginInvokeOnMainThread(() => SavePassword.Text = "保存失败");
					}
				} else {
					MainThread.BeginInvokeOnMainThread(() => SavePassword.Text = "保存失败");
				}
			}
		}

		await Task.Delay(1000).ConfigureAwait(false);

		MainThread.BeginInvokeOnMainThread(() => SavePassword.Text = "保存密码");
	}
}