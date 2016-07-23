using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipTimerForKCV.Models
{
    /// <summary>
    /// JSONデータの格納クラス
    /// </summary>
    class NoticeJson
    {

        public const string TYPE_ENSEI = "mission";
        public const string TYPE_NYUKYO = "nyukyo";
        public const string TYPE_BUILD = "createship";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NoticeJson()
        {
            
            aps = new Dictionary<string, Object>
            {
                { "content-available", 1 },
                { "sound", "" },
                { "priority", 10 }
            };
            this.ty = "";
            this.d1 = "";
            this.k1 = "";
            this.l1 = "";
            this.sT1 = "";
            this.eT1 = "";
            this.d2 = "";
            this.k2 = "";
            this.l2 = "";
            this.sT2 = "";
            this.eT2 = "";
            this.d3 = "";
            this.k3 = "";
            this.l3 = "";
            this.sT3 = "";
            this.eT3 = "";
            this.d4 = "";
            this.k4 = "";
            this.l4 = "";
            this.sT4 = "";
            this.eT4 = "";
        }
        /// <summary>
        /// JSONヘッダ部
        /// </summary>
        public Dictionary<string, Object> aps { get; set; }

        /// <summary>
        /// "遠征","入渠","建造"の分類
        /// </summary>
        public string ty { get; set; }
        /// <summary>
        /// 艦隊またはドックID ※数字はn番目データの意味
        /// </summary>
        public string d1 { get; set; }
        /// <summary>
        /// 遠征IDの値(当プラグインでは指定不要) ※数字はn番目データの意味
        /// </summary>
        public string k1 { get; set; }
        /// <summary>
        /// 遠征名 ※数字はn番目データの意味
        /// </summary>
        public string l1 { get; set; }
        /// <summary>
        /// 開始時間(UNIX時間で指定) ※数字はn番目データの意味
        /// </summary>
        public string sT1 { get; set; }
        /// <summary>
        /// 終了時間(UNIX時間で指定) ※数字はn番目データの意味
        /// </summary>
        public string eT1 { get; set; }
        /// <summary>
        /// 艦隊またはドックID ※数字はn番目データの意味
        /// </summary>
        public string d2 { get; set; }
        /// <summary>
        /// 遠征IDの値(当プラグインでは指定不要) ※数字はn番目データの意味
        /// </summary>
        public string k2 { get; set; }
        /// <summary>
        /// 遠征名 ※数字はn番目データの意味
        /// </summary>
        public string l2 { get; set; }
        /// <summary>
        /// 開始時間(UNIX時間で指定) ※数字はn番目データの意味
        /// </summary>
        public string sT2 { get; set; }
        /// <summary>
        /// 終了時間(UNIX時間で指定) ※数字はn番目データの意味
        /// </summary>
        public string eT2 { get; set; }
        /// <summary>
        /// 艦隊またはドックID ※数字はn番目データの意味
        /// </summary>
        public string d3 { get; set; }
        /// <summary>
        /// 遠征IDの値(当プラグインでは指定不要) ※数字はn番目データの意味
        /// </summary>
        public string k3 { get; set; }
        /// <summary>
        /// 遠征名 ※数字はn番目データの意味
        /// </summary>
        public string l3 { get; set; }
        /// <summary>
        /// 開始時間(UNIX時間で指定) ※数字はn番目データの意味
        /// </summary>
        public string sT3 { get; set; }
        /// <summary>
        /// 終了時間(UNIX時間で指定) ※数字はn番目データの意味
        /// </summary>
        public string eT3 { get; set; }
        /// <summary>
        /// 艦隊またはドックID ※数字はn番目データの意味
        /// </summary>
        public string d4 { get; set; }
        /// <summary>
        /// 遠征IDの値(当プラグインでは指定不要) ※数字はn番目データの意味
        /// </summary>
        public string k4 { get; set; }
        /// <summary>
        /// 遠征名 ※数字はn番目データの意味
        /// </summary>
        public string l4 { get; set; }
        /// <summary>
        /// 開始時間(UNIX時間で指定) ※数字はn番目データの意味
        /// </summary>
        public string sT4 { get; set; }
        /// <summary>
        /// 終了時間(UNIX時間で指定) ※数字はn番目データの意味
        /// </summary>
        public string eT4 { get; set; }
    }
}
