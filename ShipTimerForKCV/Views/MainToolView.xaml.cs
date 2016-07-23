using ShipTimerForKCV.Properties;
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

namespace ShipTimerForKCV.Views
{
    /// <summary>
    /// MainToolView.xaml の相互作用ロジック
    /// </summary>
    public partial class MainToolView : UserControl
    {
        // 親クラスを指定
        ShipTimerForKCV parent = new ShipTimerForKCV();

        /// <summary>
        /// 初期化処理
        /// </summary>
        public MainToolView()
        {
            // 初期処理
            InitializeComponent();

            // 認証済み判定
            if (Settings.Default.EndPointArn.Length > 0)
            {
                // 認証がある場合
                // 登録済みボタン表示
                deviceRegistedBtn.Visibility = Visibility.Visible;
                deviceRegistBtn.Visibility = Visibility.Collapsed;
                // 認証解除ボタン表示
                deleteBtn.Visibility = Visibility.Visible;
                // 環境設定表示
                EnseiArea.Visibility = Visibility.Visible;
                NyukyoArea.Visibility = Visibility.Visible;
                BuildArea.Visibility = Visibility.Visible;
                // 環境設定判定
                EnseiBtn.Content = parent.EnseiSetting ? "通知" : "-";
                NyukyoBtn.Content = parent.NyukyoSetting ? "通知" : "-";
                BuildBtn.Content = parent.BuildSetting ? "通知" : "-";
            }

        }
        /// <summary>
        /// デバイス認証失敗ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deviceRegistFailBtn_Click(object sender, RoutedEventArgs e)
        {
            // 登録失敗ボタン非表示
            deviceRegistFailButton.Visibility = Visibility.Collapsed;
            deviceRegistBtn.Visibility = Visibility.Visible;
            deviceRegistBtn_Click(sender, e);
        }

        /// <summary>
        /// デバイス認証ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void deviceRegistBtn_Click(object sender, RoutedEventArgs e)
        {

            // ボタン文字変更
            deviceRegistBtn.Content = "認証中．．．";
            deviceRegistBtn.IsEnabled = false;


            // 非同期処理(認証処理)
            string Message = await Task.Run(() =>
            {
                return parent.mngDeviceButtn_Auth();
            });
            // エラーメッセージの存在判定
            if (Message.Length == 0)
            {
                // 登録済みボタン表示
                deviceRegistedBtn.Visibility = Visibility.Visible;
                deviceRegistBtn.Visibility = Visibility.Collapsed;
                // 認証解除ボタン表示
                deleteBtn.Visibility = Visibility.Visible;
                // 環境設定表示
                EnseiArea.Visibility = Visibility.Visible;
                NyukyoArea.Visibility = Visibility.Visible;
                BuildArea.Visibility = Visibility.Visible;
                // 認証解除ボタン表示
                deleteBtn.Visibility = Visibility.Visible;
                // 表示を戻す
                deviceRegistBtn.Content = "デバイス認証";
                deviceRegistBtn.IsEnabled = true;
            }
            else
            {
                // 認証がない場合
                // 登録失敗ボタン表示
                deviceRegistFailButton.Visibility = Visibility.Visible;
                deviceRegistBtn.Visibility = Visibility.Collapsed;
                // 表示を戻す
                deviceRegistBtn.Content = "デバイス認証";
                deviceRegistBtn.IsEnabled = true;
                // ログ表示
                logTextBox.Text += Message;
            }
        }
        /// <summary>
        /// 遠征ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnseiBtn_Click(object sender, RoutedEventArgs e)
        {
            // 通知設定取得
            parent.EnseiSetting = !parent.EnseiSetting;
            // 表示状態判定
            EnseiBtn.Content = parent.EnseiSetting ? "通知" : "-";
        }
        /// <summary>
        /// 入渠ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NyukyoBtn_Click(object sender, RoutedEventArgs e)
        {
            // 通知設定取得
            parent.NyukyoSetting = !parent.NyukyoSetting;
            // 表示状態判定
            NyukyoBtn.Content = parent.NyukyoSetting ? "通知" : "-";
        }
        /// <summary>
        /// 建造ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BuildBtn_Click(object sender, RoutedEventArgs e)
        {
            // 通知設定取得
            parent.BuildSetting = !parent.BuildSetting;
            // 表示状態判定
            BuildBtn.Content = parent.BuildSetting ? "通知" : "-";
        }
        /// <summary>
        /// 認証解除ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // ボタン文字変更
            deleteBtn.Content = "解除中";
            deleteBtn.IsEnabled = false;
            // 非同期処理
            // 非同期処理(認証処理)
            string Message = await Task.Run(() =>
            {
                return parent.mngDeviceButtn_Unauth();
            });
            // エラーメッセージの存在判定
            if (Message.Length == 0)
            {
                // ボタンを初期化
                // 登録ボタン表示
                deviceRegistedBtn.Visibility = Visibility.Collapsed;
                deviceRegistBtn.Visibility = Visibility.Visible;
                // 環境設定表示
                EnseiArea.Visibility = Visibility.Hidden;
                NyukyoArea.Visibility = Visibility.Hidden;
                BuildArea.Visibility = Visibility.Hidden;
                // 認証解除ボタン非表示
                deleteBtn.Visibility = Visibility.Hidden;
                // 表示を戻す
                deleteBtn.Content = "認証解除";
                deleteBtn.IsEnabled = true;
            }
            else
            {
                // ボタン文字変更
                deleteBtn.Content = "解除失敗";
                deleteBtn.IsEnabled = true;
                // ログ表示
                logTextBox.Text += Message;
            }

        }

    }
}
