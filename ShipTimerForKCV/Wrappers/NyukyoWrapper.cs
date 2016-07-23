using System;
using System.Collections.Generic;
using MetroTrilithon.Lifetime;
using Grabacr07.KanColleWrapper.Models;
using MetroTrilithon.Mvvm;
using StatefulModel;
using System.Diagnostics;

namespace ShipTimerForKCV.Wrappers
{
    internal class NyukyoWrapper : Notifier, IDisposableHolder
    {
        private readonly MultipleDisposable compositeDisposable = new MultipleDisposable();
        // 実行監視解放用
        private MultipleDisposable repairDisposables = new MultipleDisposable();
        // 初期化フラグ
        private bool initialized;

        /// <summary>
        /// ドック状態格納
        /// </summary>
        public RepairingDock Source { get; }

        // プロパティ変更通知設定 =====================
        #region State notification property
        // 監視ステータス
        private bool _State;

        /// <summary>
        /// 監視ステータス true : 実行中 false ： 不使用
        /// </summary>
        public bool State
        {
            get { return this._State; }
            set
            {
                if (this._State != value)
                {
                    this._State = value;
                    // 監視ステータスを変更したことを通知
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion
        // ============================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NyukyoWrapper(RepairingDock repairingDock)
        {
            // コンストラクタ実行前設定
            this.initialized = false;
            // 状態を格納
            this.Source = repairingDock;
            // 実行状態の変更時に通知を行う監視を登録 → 実行通知メソッドを実行
            this.Source.Subscribe(nameof(RepairingDock.CompleteTime), () => this.addCheck()).AddTo(this);
            // コンストラクタ実行後設定
            this.initialized = true;
        }
        /// <summary>
        /// 実行通知メソッド(残時間検知) 
        /// </summary>
        private void addCheck()
        {
            // ドック監視を解放
            this.repairDisposables?.Dispose();
            this.repairDisposables = new MultipleDisposable();
            if (this.Source.State == RepairingDockState.Repairing)
            {
                // 工廠状態が"Repairing"で残り時間が変更されたら実行通知メソッドを実行
                repairDisposables.Add(this.Source.Subscribe(nameof(RepairingDock.Remaining), () => this.UpdateState()));
                // 初回動作時には"入渠状態"の場合は"実行中"にしておく
                if (!initialized)
                {
                    // ステータスを"実行中"に変更する。
                    this.State = true;
                }
            }
            else
            {
                // 工廠状態が"Building"以外に変更された場合ステータスを"不使用"に変更
                this.State = false;
            }
        }
        /// <summary>
        /// 実行通知メソッド
        /// </summary>
        private void UpdateState()
        {
            // "実行中"かつ"残り時間"が存在＝情報取得可能
            if (this.Source.State == RepairingDockState.Repairing && this.Source.Remaining.HasValue)
            {
                // ステータスを"実行中"に変更する。
                this.State = true;
                // ステータスを変更後は監視が不要のため解放
                if (initialized)
                {
                    this.repairDisposables?.Dispose();
                }
            }
        }

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositeDisposable;
        
        /// <summary>
        /// 解放用メソッド
        /// </summary>
        public void Dispose()
        {
            this.compositeDisposable.Dispose();
            this.repairDisposables?.Dispose();
        }
    }
}