using Livet;
using MetroTrilithon.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipTimerForKCV.ViewModels
{
    public class LogViewModel : ViewModel
    {
        #region log 変更通知プロパティ

        private string _LogText;

        public virtual string LogText
        {
            get { return this._LogText; }
            set
            {
                if (this._LogText != value)
                {
                    this._LogText = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion
    }
}
