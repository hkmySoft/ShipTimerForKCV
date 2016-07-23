using System;
using System.Collections.Generic;
using MetroTrilithon.Lifetime;
using Grabacr07.KanColleWrapper.Models;
using MetroTrilithon.Mvvm;
using StatefulModel;
using System.Diagnostics;

namespace ShipTimerForKCV.Wrappers
{
    internal class EnseiWrapper : Notifier, IDisposableHolder
    {
        private readonly MultipleDisposable compositeDisposable = new MultipleDisposable();

        /// <summary>
        /// 艦隊ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// 艦隊遠征状態格納
        /// </summary>
        public Expedition Source { get; }

        // プロパティ変更通知設定 =====================
        #region State notification property
        // 遠征監視ステータス
        private bool _State;

        /// <summary>
        /// 遠征監視ステータス true : 遠征中 false ： 帰投済
        /// </summary>
        public bool State
        {
            get { return this._State; }
            set
            {
                if (this._State != value)
                {
                    this._State = value;
                    // 遠征監視ステータスを変更したことを通知
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion
        // ============================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnseiWrapper(int id, Expedition expedition)
        {
            // 艦隊IDを格納
            this.Id = id;
            // 遠征状態を格納
            this.Source = expedition;

            // 遠征の実行状態の変更時に通知を行う監視を登録 → 遠征実行通知メソッドを実行
            this.Source.Subscribe(nameof(Expedition.IsInExecution), () => this.UpdateState()).AddTo(this);

        }

        /// <summary>
        /// 遠征実行通知メソッド
        /// </summary>
        private void UpdateState()
        {
            // 遠征中であればステータスを"遠征実行中"にそれ以外は"帰投済"とする。
            this.State = this.Source.IsInExecution;
        }

        public void Dispose() => this.compositeDisposable.Dispose();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositeDisposable;
    }
}