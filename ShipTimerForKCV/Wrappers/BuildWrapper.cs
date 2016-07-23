using System;
using System.Collections.Generic;
using MetroTrilithon.Lifetime;
using Grabacr07.KanColleWrapper.Models;
using MetroTrilithon.Mvvm;
using StatefulModel;
using System.Diagnostics;

namespace ShipTimerForKCV.Wrappers
{
    internal class BuildWrapper : Notifier, IDisposableHolder
    {
        private readonly MultipleDisposable compositeDisposable = new MultipleDisposable();
        // 実行監視解放用
        private MultipleDisposable dockyardDisposables = new MultipleDisposable();
        // 初期化フラグ
        private bool initialized;

        /// <summary>
        /// ドック状態格納
        /// </summary>
        public BuildingDock Source { get; }

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
        public BuildWrapper(BuildingDock buildingDock)
        {
            // コンストラクタ実行前設定
            this.initialized = false;
            // 状態を格納
            this.Source = buildingDock;
            // 実行状態の変更時に通知を行う監視を登録 → 実行通知メソッド(残時間検知) を実行
            this.Source.Subscribe(nameof(BuildingDock.CompleteTime), () => this.addCheck()).AddTo(this);
            // コンストラクタ実行後設定
            this.initialized = true;
        }
        /// <summary>
        /// 実行通知メソッド(残時間検知) 
        /// </summary>
        private void addCheck()
        {
            // ドック監視を解放
            this.dockyardDisposables?.Dispose();
            this.dockyardDisposables = new MultipleDisposable();
            if (this.Source.State == BuildingDockState.Building)
            {
                // 工廠状態が"Building"で残り時間が変更されたら実行通知メソッドを実行
                dockyardDisposables.Add(this.Source.Subscribe(nameof(BuildingDock.Remaining), () => this.UpdateState()));

            } else
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
            if (this.Source.State == BuildingDockState.Building && this.Source.Remaining.HasValue)
            {
                // ステータスを"実行中"に変更する。
                this.State = true;
                // ステータスを変更後は監視が不要のため解放
                if (initialized) {
                    this.dockyardDisposables?.Dispose();
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
            this.dockyardDisposables?.Dispose();
        }
    }
}