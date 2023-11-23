using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro;
using MahApps.Metro.Controls;
using OtpSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using Base32;
using System.Web.UI.WebControls;

namespace CreateQrCode
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// ハッシュアルゴリズム
        /// <br></br>
        /// ※Google AuthenticatorはSHA1固定
        /// </summary>
        private const OtpHashMode OTP_HASH_MODE = OtpHashMode.Sha1;

        /// <summary>
        /// トークン更新間隔（秒）
        /// <br></br>
        /// ※Google Authenticatorは30秒固定
        /// </summary>
        private const int TIME_STEP = 30;

        /// <summary>
        /// トークン桁数
        /// <br></br>
        /// ※Google Authenticatorは6桁固定
        /// </summary>
        private const int TOTP_SIZE = 6;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServiceName.Text))
            {
                MessageBox.Show("サービス名を入力してください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAccountName.Text))
            {
                MessageBox.Show("アカウント名を入力してください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ---------------------------------------------------------------------------------------
            // TOTPのURLフォーマット：
            //      otpauth://totp/[issuer]:[accountname]?secret=[secret]&issuer=[issuer]
            //  例）otpauth://totp/Example:hoge@example.com?secret=JBSWY3DPEHPK3PXP&issuer=Example
            //
            //  issuer(発行者)           ：サービス名等
            //  accountname(アカウント名)：アカウント名、ログインID等
            // ---------------------------------------------------------------------------------------

            // 秘密鍵生成
            byte[] secretKey = KeyGeneration.GenerateRandomKey(OTP_HASH_MODE);
            // 発行者
            string issuer = "MyApp";
            // アカウント名
            string accountName = "yamamoti";
            // LABEL
            string label = $"{issuer}:{accountName}";
            // URL生成
            string qrCodeUrl = KeyUrl.GetTotpUrl(secretKey, label, TIME_STEP, OTP_HASH_MODE, TOTP_SIZE) + $"&issuer={issuer}";

            // URLをQRコードに変換
            BarcodeWriter qrCode = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    // QRコードバージョン
                    QrVersion = 10,
                    // 誤り訂正レベル
                    ErrorCorrection = ErrorCorrectionLevel.M,
                    // 文字エンコード
                    CharacterSet = "UTF-8",
                    // QRコード高さ
                    Height = 400,
                    // QRコード横幅
                    Width = 400,
                    // マージン
                    Margin = 5
                }
            };

            // 変換したQRコードをFormに表示
            using (Bitmap bmp = qrCode.Write(qrCodeUrl))
            using (MemoryStream ms = new MemoryStream())
            {
                imgQrCode.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                //bmp.Save(ms, ImageFormat.Bmp);
                //imgQrCode.Source = System.Drawing.Image.FromStream(ms);
            }

            // 秘密鍵をテキストに表示
            txtQr.Text = Base32Encoder.Encode(secretKey);
        }
    }
}
