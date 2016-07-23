using System;

namespace ShipTimerForKCV.Models
{
    /// <summary>
    /// 通知用のデータの格納クラス
    /// </summary>
    internal class NoticeData
    {
        /// <summary>
        /// 艦隊ID・ドックID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// ラベル(表示名)
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// 開始時間(UnixTime(ms))
        /// </summary>
        public string StartTime { get; }

        /// <summary>
        /// 終了時間(UnixTime(ms))
        /// </summary>
        public string EndTime { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NoticeData(int id, string label, DateTimeOffset startTimeOffset, DateTimeOffset endTimeOffset)
        {
            // 艦隊IDを格納
            this.Id = id.ToString();
            // ラベル(遠征名)
            this.Label = label;
            // 開始時間(UnixTime(ms))
            this.StartTime = startTimeOffset.ToUnixTimeMilliseconds().ToString();
            // 開始時間(UnixTime(ms))
            this.EndTime = endTimeOffset.ToUnixTimeMilliseconds().ToString();
        }
    }
}