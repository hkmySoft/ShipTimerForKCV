ShipTimerForKCV
======================
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/hkmySoft/ShipTimerForKCV/master/LICENSE)
<!--[![Release]()]()-->

iPhone版「[艦これタイマー](https://itunes.apple.com/jp/app/shiptimer/id684642180?l=ja&ls=1&mt=8)」に遠征・入渠・建造時間を自動セットする  
[KanColleViewer](http://grabacr.net/kancolleviewer)用プラグインです。

## 機能
* 遠征・入渠・建造の完了時間をiPhone版「[艦これタイマー](https://itunes.apple.com/jp/app/shiptimer/id684642180?l=ja&ls=1&mt=8)」に自動連携します。
* 連携する項目は設定にて変更可能です。

## 必要動作環境

#### KanColleViewer （提督業も忙しい！）
* **[Ver 4.6](http://grabacr.net/kancolleviewer)** 以降
* その他 動作環境は KanColleViewer に準拠します。

#### iPhone版 艦これタイマー （艦Colleタイマー for 艦これ）
* **[Ver 10.0](https://itunes.apple.com/jp/app/shiptimer/id684642180?l=ja&ls=1&mt=8)** 以降
* iOS 7 以上

#### その他
* Googleアカウント(デバイス認証に使用します)

## インストール

* "ShipTimerForKCV.dll"をKanColleViewerの"Plugins"ディレクトリに移動してください。
* 以下のDLLを **すべて** KanColleViewer.exeと同じディレクトリに移動してください。(一つでも足りないと動作しません。)
 * AWSSDK.dll
 * BouncyCastle.Crypto.dll
 * Google.Apis.Auth.dll
 * Google.Apis.Auth.PlatformServices.dll
 * Google.Apis.Core.dll
 * Google.Apis.dll
 * Google.Apis.PlatformServices.dll
 * Google.Apis.Plus.v1.dll
 * Newtonsoft.Json.dll
 * Zlib.Portable.dll

## 使い方


## ライセンス

* [The MIT License (MIT)](LICENSE)  


## ダウンロード

 [ShipTimerForKCV - GitHub Releases Page]()



## お問い合わせ/機能要望/バグ報告
- Twitterから
 - [twitter.com/Kozeni_50yen](https://twitter.com/Kozeni_50yen)
- GitHubから
 - [github.com/hkmySoft/ShipTimerForKCV/issues/new](https://github.com/hkmySoft/ShipTimerForKCV/issues/new)



## 使用ライブラリ

以下のライブラリを使用して開発を行っています。

#### [KanColleViewer](https://github.com/Grabacr07/KanColleViewer)

> The MIT License (MIT)
>
> Copyright (c) 2013 Grabacr07

* **ライセンス :** The MIT License (MIT)
* **ライセンス全文 :** [licenses/KanColleViewer.txt](licenses/KanColleViewer.txt)

#### [MetroRadiance](https://github.com/Grabacr07/MetroRadiance)

> The MIT License (MIT)
>
> Copyright (c) 2014 Manato KAMEYA

* **ライセンス :** The MIT License (MIT)
* **ライセンス全文 :** [licenses/MetroRadiance.txt](licenses/MetroRadiance.txt)

#### [Livet](http://ugaya40.hateblo.jp/entry/Livet)

* **ライセンス :** zlib/libpng

#### [Rx (Reactive Extensions)](https://rx.codeplex.com/)

* **ライセンス :** Apache License Version 2.0
* **ライセンス全文 :** [licenses/ApacheLicense2.0.txt](licenses/ApacheLicense2.0.txt)

#### [StatefulModel](http://ugaya40.hateblo.jp/entry/StatefulModel)

> The MIT License (MIT)
>
> Copyright (c) 2015 Masanori Onoue

* **ライセンス :** The MIT License (MIT)
* **ライセンス全文 :** [licenses/StatefulModel.txt](licenses/StatefulModel.txt)


#### [AWS SDK for .NET](https://aws.amazon.com/jp/sdk-for-net/)

* **ライセンス :** Apache License 2.0
* **ライセンス全文 :** [licenses/ApacheLicense2.0.txt](licenses/ApacheLicense2.0.txt)

#### [Google APIs from .NET](https://developers.google.com/api-client-library/dotnet/)

* **ライセンス :** Apache License 2.0
* **ライセンス全文 :** [licenses/ApacheLicense2.0.txt](licenses/ApacheLicense2.0.txt)
