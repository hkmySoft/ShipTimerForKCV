using Google.Apis.Auth.OAuth2;
using Google.Apis.Plus.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using MetroTrilithon.Lifetime;
using MetroTrilithon.Linq;
using MetroTrilithon.Mvvm;
using StatefulModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Threading;
using Amazon.DynamoDBv2;
using Amazon;
using Amazon.DynamoDBv2.Model;
using Amazon.CognitoIdentity;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using ShipTimerForKCV.Properties;
using System.Threading.Tasks;
using System.Net;
using ShipTimerForKCV.Wrappers;
using ShipTimerForKCV.Models;
using ShipTimerForKCV.ViewModels;
using System.Text;
using System.Windows;
using ShipTimerForKCV.Views;

namespace ShipTimerForKCV
{
    [Export(typeof(IPlugin))]
    [ExportMetadata("Guid", guid)]
    [ExportMetadata("Title", "艦これタイマー連携ツール for 提督業も忙しい！")]
    [ExportMetadata("Description", "iPhone版「艦これタイマー」と連携するためのプラグインです。")]
    [ExportMetadata("Version", "1.0.0")]
    [ExportMetadata("Author", "@Kozeni_50yen")]
    [Export(typeof(ITool))]
    class ShipTimerForKCV : IPlugin, ITool, IDisposableHolder
    {

        private const string guid = "0CF2113A-6E4B-4857-8508-3E4B860662BB";
        // == メッセージ ==
        private const string MES_GOOGLE_AUTH = "Googleの認証に失敗しました。有効なGoogleアカウントでログインし、アクセスの「許可」を行ってください。";
        private const string MES_DEVICE_AUTH = "iPhoneの認証に失敗しました。先にiPhone版「艦これタイマー」から認証を行ってください。";
        public const string MES_DEVICE_UNAUTH = "デバイスの解除を行いました。\r\nGoogleの認証まで解除する場合は\r\n" +
                                                "https://security.google.com/settings/security/permissions から削除を行い、\r\n" +
                                                "C:\\Users\\{ユーザ名}\\AppData\\Roaming\\Api.Plus.Storeフォルダを削除してください";                                                
        private const string MES_LIB_NOT_FOUND = "デバイス処理に失敗しました。\r\nKanColleViewer.exeと同じフォルダ内に以下のファイルがあることを確認してください。\r\n" +
                                                "AWSSDK.dll\r\n" +
                                                "BouncyCastle.Crypto.dll\r\n" +
                                                "Google.Apis.Auth.dll\r\n" +
                                                "Google.Apis.Auth.PlatformServices.dll\r\n" +
                                                "Google.Apis.Core.dll\r\n" +
                                                "Google.Apis.dll\r\n" +
                                                "Google.Apis.PlatformServices.dll\r\n" +
                                                "Google.Apis.Plus.v1.dll\r\n" +
                                                "Microsoft.Threading.Tasks.dll\r\n" +
                                                "Microsoft.Threading.Tasks.Extensions.Desktop.dll\r\n" +
                                                "Microsoft.Threading.Tasks.Extensions.dll\r\n" +
                                                "System.Net.Http.Extensions.dll\r\n" +
                                                "System.Net.Http.Primitives.dll\r\n" +
                                                "Zlib.Portable.dll\r\n";


        // 監視
        private readonly MultipleDisposable compositDisposable = new MultipleDisposable();

        // == 遠征 ==
        // 監視用クラス
        private EnseiWrapper[] enseiWrappers;
        // 通知データ
        private NoticeData[] enseiDataList;
        // 送信用JSON
        private string enseiSendJsonData;
        // 実行用タイマー
        private DispatcherTimer enseiDoTimer;

        // == 入渠 ==
        // 監視用クラス
        private NyukyoWrapper[] nyukyoWrappers;
        // 通知データ
        private NoticeData[] nyukyoDataList;
        // 送信用JSON
        private string nyukyoSendJsonData;
        // 実行用タイマー
        private DispatcherTimer nyukyoDoTimer;

        // == 建造 ==
        // 監視用クラス
        private BuildWrapper[] buildWrappers;
        // 通知データ
        private NoticeData[] buildDataList;
        // 送信用JSON
        private string buildSendJsonData;
        // 実行用タイマー
        private DispatcherTimer buildDoTimer;

        // ログ用ViewModel
        private LogViewModel viewModel;

        // 初期化フラグ
        private bool initialized;

        // ツール名称
        public string Name => "艦これタイマー";

        // ツール画面
        public object View => new MainToolView { DataContext = this.viewModel, };

        // == ユーザー設定値(変更可能) ==
        // GooglePlus認証のユーザーID
        private string GoogleUserID
        {
            get { return Settings.Default.GoogleUserID; }
            set
            {
                Settings.Default.GoogleUserID = value;
                Settings.Default.Save();
            }
        }
        // 使用端末のDeviceID
        private string DeviceID
        {
            get { return Settings.Default.DeviceID; }
            set
            {
                Settings.Default.DeviceID = value;
                Settings.Default.Save();
            }
        }
        // 使用端末のEndPoint
        private string EndPointArn
        {
            get { return Settings.Default.EndPointArn; }
            set
            {
                Settings.Default.EndPointArn = value;
                Settings.Default.Save();
            }
        }
        // 遠征の通知を実施するフラグ
        public bool EnseiSetting
        {
            get { return Settings.Default.EnseiSetting; }
            set
            {
                Settings.Default.EnseiSetting = value;
                Settings.Default.Save();
            }
        }
        // 入渠の通知を実施するフラグ
        public bool NyukyoSetting
        {
            get { return Settings.Default.NyukyoSetting; }
            set
            {
                Settings.Default.NyukyoSetting = value;
                Settings.Default.Save();
            }
        }
        // 建造の通知を実施するフラグ
        public bool BuildSetting
        {
            get { return Settings.Default.BuildSetting; }
            set
            {
                Settings.Default.BuildSetting = value;
                Settings.Default.Save();
            }
        }
        // == Google/AWS 認証関連 ==
        private CognitoAWSCredentials credentials;

        /// <summary>
        /// プラグイン読み込み時に実行
        /// </summary>
        public void Initialize()
        {
            // 艦これが開始(終了)したら実行する監視を登録 → 初期化処理
            KanColleClient.Current
                .Subscribe(nameof(KanColleClient.IsStarted), () => this.InitializeCore(), false)
                .AddTo(this);

            // 遠征タイマーインスタンス生成
            enseiDoTimer = new DispatcherTimer(DispatcherPriority.Normal);
            enseiDoTimer.Interval = TimeSpan.FromSeconds(15);
            enseiDoTimer.Tick += new EventHandler(SendDataForEnsei);

            // 入渠タイマーインスタンス生成
            nyukyoDoTimer = new DispatcherTimer(DispatcherPriority.Normal);
            nyukyoDoTimer.Interval = TimeSpan.FromSeconds(15);
            nyukyoDoTimer.Tick += new EventHandler(SendDataForNyukyo);

            // 建造タイマーインスタンス生成
            buildDoTimer = new DispatcherTimer(DispatcherPriority.Normal);
            buildDoTimer.Interval = TimeSpan.FromSeconds(15);
            buildDoTimer.Tick += new EventHandler(SendDataForBuild);

            // ログ表示
            this.viewModel = new LogViewModel();

        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void InitializeCore()
        {
            // 一度呼ばれていたら終了
            if (this.initialized) return;

            // 母港データの取得
            var homeport = KanColleClient.Current.Homeport;
            if (homeport != null)
            {
                // 母港データの取得に成功したら次回(艦これ終了時等)は呼び出し不要
                this.initialized = true;

                // 艦隊データ変更時に実行する監視を登録
                // 遠征更新時処理
                homeport.Organization
                    .Subscribe(nameof(Organization.Fleets), this.UpdateEnsei)
                    .AddTo(this);
                // 入渠更新時処理
                homeport.Repairyard
                    .Subscribe(nameof(Repairyard.Docks), this.UpdateNyukyo)
                    .AddTo(this);
                // 建造更新時処理
                homeport.Dockyard
                    .Subscribe(nameof(Dockyard.Docks), this.UpdateBuild)
                    .AddTo(this);
            }

        }
        /// <summary>
        /// タイマー開始
        /// </summary>
        public void disTimeStart(DispatcherTimer dotimer) { dotimer.Start(); }

        /// <summary>
        /// 遠征更新時処理
        /// </summary>
        private void UpdateEnsei()
        {
            // 母港データの取得に成功していなければ返却
            if (!this.initialized) return;

            // クラス呼び出し時に監視用クラスを初期化
            if (this.enseiWrappers != null)
            {
                foreach (var wrapper in this.enseiWrappers) wrapper.Dispose();
            }

            // 各艦隊について処理をする。
            this.enseiWrappers = KanColleClient.Current.Homeport.Organization.Fleets
                .Skip(1)                                                                                    // 第1艦隊を除外
                .Select(x => new { x.Value.Id, x.Value.Expedition, })                                       // 各艦隊のIDに対する遠征情報のKeyVlueを生成
                .Where(a => a.Expedition != null)                                                           // 生成したKeyValueの遠征情報存在チェック
                .Select(a => new EnseiWrapper(a.Id, a.Expedition).AddTo(this))                              // 各艦隊の監視クラスを生成
                .Do(x => x.Subscribe(nameof(EnseiWrapper.State), () => this.UpdateEnseiChk(x.Id)).AddTo(this))   // 各遠征監視クラスのステータスを監視して変更があれば → 更新通知判定メソッドを実行
                .ToArray();
        }

        /// <summary>
        /// 遠征更新通知判定処理
        /// </summary>
        private void UpdateEnseiChk(int fleetId)
        {

            // 母港データの取得をしていない場合は後続の実施をしない
            if (!this.initialized) return;

            // 初回起動時の処理
            if (this.enseiWrappers.Length == 0) return;


            // 処理対象となった艦隊が帰投済→遠征中となった場合
            if (this.enseiWrappers.Any(x => x.Id == fleetId && x.State))
            {
                this.enseiDataList = this.enseiWrappers.Where(x => x.Source.Remaining.HasValue && x.Source.ReturnTime.HasValue)
                    .Select(a => new NoticeData(
                        a.Id,
                        a.Source.Mission.Title,
                        a.Source.ReturnTime.Value.Subtract(TimeSpan.FromMinutes(a.Source.Mission.RawData.api_time)),
                        a.Source.ReturnTime.Value
                    )).ToArray();

                // 通知用JSON変換
                enseiSendJsonData = this.convertJsonData(enseiDataList, NoticeJson.TYPE_ENSEI);

                // タイマーが起動していなければ起動する
                if (!enseiDoTimer.IsEnabled)
                {
                    disTimeStart(enseiDoTimer);
                }


            }
        }

        /// <summary>
        /// 入渠更新時処理
        /// </summary>
        private void UpdateNyukyo()
        {
            // 母港データの取得に成功していなければ返却
            if (!this.initialized) return;
            // クラス呼び出し時に監視用クラスを初期化
            if (this.nyukyoWrappers != null)
            {
                foreach (var wrapper in this.nyukyoWrappers) wrapper.Dispose();
            }
            // 各ドックについて処理をする。
            this.nyukyoWrappers = KanColleClient.Current.Homeport.Repairyard.Docks
                .Select(x => new { x.Value, })
                .Where(a => a.Value != null)
                .Select(a => new NyukyoWrapper(a.Value).AddTo(this))
                .Do(x => x.Subscribe(nameof(NyukyoWrapper.State), () => this.UpdateNyukyoChk(x.Source.Id)).AddTo(this))
                .ToArray();
        }
        /// <summary>
        /// 入渠更新通知判定処理
        /// </summary>
        private void UpdateNyukyoChk(int dockId)
        {
            // 母港データの取得をしていない場合は後続の実施をしない
            if (!this.initialized) return;

            // 初回起動時の処理
            if (this.nyukyoWrappers == null || this.nyukyoWrappers.Length == 0) return;

            // 処理対象となったドックが不使用→入渠中となった場合
            if (this.nyukyoWrappers.Any(x => x.Source.Id == dockId && x.State))
            {
                this.nyukyoDataList = this.nyukyoWrappers.Where(x => x.Source.Remaining.HasValue && x.Source.CompleteTime.HasValue)
                    .Select(a => new NoticeData(
                        a.Source.Id,
                        "",
                        a.Source.CompleteTime.Value.Subtract(a.Source.Remaining.Value),
                        a.Source.CompleteTime.Value
                    )).ToArray();
                // 通知用JSON変換
                nyukyoSendJsonData = this.convertJsonData(nyukyoDataList, NoticeJson.TYPE_NYUKYO);

                // タイマーが起動していなければ起動する
                if (!nyukyoDoTimer.IsEnabled)
                {
                    disTimeStart(nyukyoDoTimer);
                }


            }
        }

        /// <summary>
        /// 建造更新時処理
        /// </summary>
        private void UpdateBuild()
        {
            // 母港データの取得に成功していなければ返却
            if (!this.initialized) return;
            // クラス呼び出し時に監視用クラスを初期化
            if (this.buildWrappers != null)
            {
                foreach (var wrapper in this.buildWrappers) wrapper.Dispose();

            }
            // 各ドックについて処理をする。
            this.buildWrappers = KanColleClient.Current.Homeport.Dockyard.Docks
                .Select(x => new { x.Value, })
                .Where(a => a.Value != null)
                .Select(a => new BuildWrapper(a.Value).AddTo(this))
                .Do(x => x.Subscribe(nameof(BuildWrapper.State), () => this.UpdateBuildChk(x.Source.Id)).AddTo(this))
                .ToArray();
        }
        /// <summary>
        /// 建造更新通知判定処理
        /// </summary>
        private void UpdateBuildChk(int dockId)
        {
            // 母港データの取得をしていない場合は後続の実施をしない
            if (!this.initialized) return;

            // 初回起動時の処理
            if (this.buildWrappers == null || this.buildWrappers.Length == 0) return;


            // 処理対象となったドックが不使用→入渠中となった場合
            if (this.buildWrappers.Any(x => x.Source.Id == dockId && x.State))
            {
                this.buildDataList = this.buildWrappers.Where(x => x.Source.Remaining.HasValue && x.Source.CompleteTime.HasValue)
                    .Select(a => new NoticeData(
                        a.Source.Id,
                        "",
                        a.Source.CompleteTime.Value.Subtract(a.Source.Remaining.Value),
                        a.Source.CompleteTime.Value
                    )).ToArray();

                // 通知用JSON変換
                buildSendJsonData = this.convertJsonData(buildDataList, NoticeJson.TYPE_BUILD);

                // タイマーが起動していなければ起動する
                if (!buildDoTimer.IsEnabled)
                {
                    disTimeStart(buildDoTimer);
                }
            }
        }
        /// <summary>
        /// JSON形式に変換
        /// </summary>
        private string convertJsonData(NoticeData[] datalist, string type)
        {
            // JSON形式に変換
            int fleetDeckCnt = 0;
            var json = new NoticeJson();
            foreach (NoticeData notice in datalist)
            {
                // 通知の種別
                json.ty = type;

                // 各データを設定
                // d[*]：艦隊IDまたはドックID
                // l[*]：表示名
                // sT[*]：開始時間
                // eT[*]：終了時間
                if (fleetDeckCnt == 0)
                {
                    json.d1 = notice.Id;
                    json.l1 = notice.Label;
                    json.sT1 = notice.StartTime;
                    json.eT1 = notice.EndTime;

                }
                else if (fleetDeckCnt == 1)
                {
                    json.d2 = notice.Id;
                    json.l2 = notice.Label;
                    json.sT2 = notice.StartTime;
                    json.eT2 = notice.EndTime;

                }
                else if (fleetDeckCnt == 2)
                {
                    json.d3 = notice.Id;
                    json.l3 = notice.Label;
                    json.sT3 = notice.StartTime;
                    json.eT3 = notice.EndTime;

                }
                else if (fleetDeckCnt == 3)
                {
                    json.d4 = notice.Id;
                    json.l4 = notice.Label;
                    json.sT4 = notice.StartTime;
                    json.eT4 = notice.EndTime;

                }
                fleetDeckCnt++;
            }
            // JSON変換インスタンス生成
            var convert = new JavaScriptSerializer();
            if (Settings.Default.sandboxSetting)
            {
                // AWS用コンバート "APNS_SANDBOX"を設定
                var awsConvertDict = new
                {
                    APNS_SANDBOX = convert.Serialize(json)
                };
                return convert.Serialize(awsConvertDict);
            }
            else
            {
                // AWS用コンバート "APNS"を設定
                var awsConvertDict = new
                {
                    APNS = convert.Serialize(json)
                };
                return convert.Serialize(awsConvertDict);
            }
        }

        /// <summary>
        /// 遠征情報送信処理
        /// </summary>
        private void SendDataForEnsei(object sender, EventArgs e)
        {
            // タイマー停止
            enseiDoTimer.Stop();
            // メッセージ送信設定判定
            if (Settings.Default.EnseiSetting && Settings.Default.EndPointArn.Length > 0)
            {
                CreateMessage("遠征情報を送信しました");
                // メッセージ送信(非同期処理)
                SendMessage(enseiSendJsonData);
            }
        }

        /// <summary>
        /// 入渠情報送信処理
        /// </summary>
        private void SendDataForNyukyo(object sender, EventArgs e)
        {
            // タイマー停止
            nyukyoDoTimer.Stop();
            // メッセージ送信設定判定
            if (Settings.Default.NyukyoSetting && Settings.Default.EndPointArn.Length > 0)
            {
                CreateMessage("入渠情報を送信しました");
                // メッセージ送信(非同期処理)
                SendMessage(nyukyoSendJsonData);
            }
        }

        /// <summary>
        /// 建造情報送信処理
        /// </summary>
        private void SendDataForBuild(object sender, EventArgs e)
        {
            // タイマー停止
            buildDoTimer.Stop();
            // メッセージ送信設定判定
            if (Settings.Default.BuildSetting && Settings.Default.EndPointArn.Length > 0)
            {
                CreateMessage("建造情報を送信しました");
                // メッセージ送信(非同期処理)
                SendMessage(buildSendJsonData);
            }
        }

        /// <summary>
        /// デバイス認証ボタン管理
        /// </summary>
        public string mngDeviceButtn_Auth()
        {

            try
            {
                // ユーザー情報を取得する。
                GetUserDataForGoogle();
                // 取得したユーザーIDよりデバイスIDを取得する
                GetAwsDynamoDB_DeviceID();
                // 認証完了のメッセージ内容作成
                var authMessage = "";
                // JSON変換インスタンス生成
                var convert = new JavaScriptSerializer();
                if (Settings.Default.sandboxSetting)
                {
                    // AWS用コンバート "APNS_SANDBOX"を設定
                    var awsConvertDict = new
                    {
                        APNS_SANDBOX = convert.Serialize(new AuthJson())
                    };
                    authMessage = convert.Serialize(awsConvertDict);
                }
                else
                {
                    // AWS用コンバート "APNS"を設定
                    var awsConvertDict = new
                    {
                        APNS = convert.Serialize(new AuthJson())
                    };
                    authMessage = convert.Serialize(awsConvertDict);
                }
                // メッセージ送信
                SendMessage(authMessage);
                return "";

            }
            catch (FileNotFoundException)
            {
                return CreateMessage(MES_LIB_NOT_FOUND);
            }
            catch (Exception e)
            {
                return CreateMessage(e.Message);

            }

        }

        /// <summary>
        /// デバイス解除ボタン管理
        /// </summary>
        public string mngDeviceButtn_Unauth()
        {
            try
            {
                // AWS Cognito 認証情報の取得
                GetAwsCognito_Auth();
                // エンドポイントが存在する場合
                if (Settings.Default.EndPointArn.Length > 0)
                {
                    // エンドポイントの削除を実施
                    DeleteEndPoint();
                }
                return MES_DEVICE_UNAUTH;

            }
            catch (FileNotFoundException)
            {
                return CreateMessage(MES_LIB_NOT_FOUND);
            }
            catch (Exception e)
            {
                return CreateMessage(e.Message);

            }
        }

        /// <summary>
        /// ユーザー情報(GoogleID)の取得
        /// </summary>
        private void GetUserDataForGoogle()
        {
            try
            {
                // 認証画面の表示
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    // クライアント認証情報設定
                    new ClientSecrets
                    {
                        ClientId = Settings.Default.ClientID,
                        ClientSecret = Settings.Default.ClientSecret
                    },
                    // 接続先設定
                    new[] {
                    PlusService.Scope.UserinfoProfile
                    },
                    "user",
                    CancellationToken.None,
                    // データ保存 
                    new FileDataStore("Api.Plus.Store")
                ).Result;

                // GooglePlusサービスの利用
                var service = new PlusService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "ShipTimerProject",
                });
                // GooglePlusより個人情報の取得
                var person = service.People.Get("me").Execute();

                // GoogleUserIDの設定
                this.GoogleUserID = person.Id;
                if (this.GoogleUserID.Length <= 0)
                {
                    // GoogleUserIDが存在しない場合
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                // GoogleUserIDが存在しない場合
                throw new Exception(MES_GOOGLE_AUTH);
            }

        }

        /// <summary>
        /// AWS Cognito 認証情報の取得
        /// </summary>
        private void GetAwsCognito_Auth()
        {
            // AWS Cognito呼び出し
            this.credentials = new CognitoAWSCredentials(
                Settings.Default.IdentityPoolID,
                RegionEndpoint.USEast1
            );
        }

        /// <summary>
        /// AWS DynamoDB デバイスIDの取得
        /// </summary>
        private void GetAwsDynamoDB_DeviceID()
        {
            try
            {
                // AWS Cognito 認証情報の取得
                GetAwsCognito_Auth();
                // DynamoDBクライアントの生成
                AmazonDynamoDBClient client = new AmazonDynamoDBClient(this.credentials, RegionEndpoint.USEast1);

                // データ取得リクエスト生成
                var request = new GetItemRequest
                {
                    // テーブル名
                    TableName = "DevTokTbl",
                    // キー値：ユーザーID
                    Key = new Dictionary<string, AttributeValue>(){
                    {
                        "user_Id", new AttributeValue { S = Settings.Default.GoogleUserID }
                    }
                }
                };
                // データ取得実施
                var result = client.GetItem(request);

                // 取得結果反映
                Dictionary<string, AttributeValue> dictItem = result.Item;

                // KeyにデバイスIDがあるか
                if (dictItem.ContainsKey("device_Id"))
                {
                    // デバイスIDを設定
                    this.DeviceID = dictItem["device_Id"].S;
                    if (this.DeviceID.Length <= 0)
                    {
                        // デバイスIDが存在しない場合
                        throw new Exception();
                    }
                }
                else
                {
                    // デバイスIDが取得できなかった場合
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                // デバイスIDが取得できなかった場合
                throw new Exception(MES_DEVICE_AUTH);
            }


        }

        /// <summary>
        /// AWS SNS メッセージの送信
        /// </summary>
        /// <param name="messageJson">送信メッセージ(JSON形式)</param>
        private async void SendMessage(string messageJson)
        {
            // 非同期処理
            await Task.Run(() =>
            {
                try
                {

                    // AWS Cognito 認証情報の取得
                    GetAwsCognito_Auth();
                    // 非同期処理
                    using (var client = new AmazonSimpleNotificationServiceClient(this.credentials, RegionEndpoint.USEast1))
                    {
                        // エンドポイントの作成リクエスト作成
                        var endpointRequest = new CreatePlatformEndpointRequest
                        {
                            // 送信先設定
                            PlatformApplicationArn = Settings.Default.PlatformApplicationArn,
                            // DynamoDBから取得したデバイスID
                            Token = Settings.Default.DeviceID
                        };
                        // エンドポイントの作成実施
                        var result = client.CreatePlatformEndpoint(endpointRequest);

                        // エンドポイントの保管
                        this.EndPointArn = result.EndpointArn;

                        // メッセージの送信
                        var response = client.Publish(new PublishRequest
                        {
                            // メッセージ形式："JSON"
                            MessageStructure = "json",
                            // 送信メッセージ
                            Message = messageJson,
                            // 生成したエンドポイント
                            TargetArn = result.EndpointArn
                        });
                    }

                }
                catch (FileNotFoundException)
                {
                    Application.Current.Dispatcher.Invoke(() => CreateMessage(MES_LIB_NOT_FOUND));
                    CreateMessage(MES_LIB_NOT_FOUND);
                }
                catch (Exception e)
                {
                    Application.Current.Dispatcher.Invoke(() => CreateMessage(e.Message));

                    // エンドポイントが存在する場合
                    if (Settings.Default.EndPointArn.Length > 0)
                    {
                        // エンドポイントの削除を実施
                        DeleteEndPoint();
                    }
                }
            });

        }

        /// <summary>
        /// AWS SNS エンドポイントの削除
        /// (デバイス解除時・メッセージ送信失敗時)
        /// </summary>
        public void DeleteEndPoint()
        {
            using (var client = new AmazonSimpleNotificationServiceClient(this.credentials, RegionEndpoint.USEast1))
            {
                // エンドポイントの削除リクエスト作成
                var endpointRequest = new DeleteEndpointRequest
                {
                    // エンドポイントID
                    EndpointArn = Settings.Default.EndPointArn
                };
                // エンドポイントの削除実施
                var result = client.DeleteEndpoint(endpointRequest);
                if (result.HttpStatusCode == HttpStatusCode.OK)
                {
                    // エンドポイントの削除が実施された場合
                    this.GoogleUserID = "";
                    this.EndPointArn = "";
                    this.DeviceID = "";
                }

            }
        }

        /// <summary>
        /// ログエリア表示処理
        /// </summary>
        private string CreateMessage(string message)
        {
            DateTime now = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append(now.ToString("yyyy/MM/dd HH:mm:ss"));
            sb.Append(" ");
            sb.Append(message);
            sb.Append(Environment.NewLine);
            if (this.viewModel != null)
            {
                // ログ表示エリアに表示
                this.viewModel.LogText += sb.ToString();
            }
            return sb.ToString();

        }

        public void Dispose() => this.compositDisposable.Dispose();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositDisposable;

    }


}
