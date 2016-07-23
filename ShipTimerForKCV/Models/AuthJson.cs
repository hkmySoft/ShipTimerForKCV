using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipTimerForKCV.Models
{
    /// <summary>
    /// 認証登録時のJSONデータの格納クラス
    /// </summary>
    class AuthJson
    {

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AuthJson()
        {
            
            aps = new Dictionary<string, Object>
            {
                { "alert", "iPhoneが正常に認証されました！ via 提督業も忙しい！" },
                { "content-available", 2 },
                { "sound", "default" }
            };
        }
        /// <summary>
        /// JSONヘッダ部
        /// </summary>
        public Dictionary<string, Object> aps { get; set; }

    }
}
