using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExtFileCopy
{
    class MainWindowData : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null){
            field = value;
            var h = this.PropertyChanged;
            if (h != null) { h(this, new PropertyChangedEventArgs(propertyName)); }
        }


        private string _DispInfo;
        public string DispInfo
        {
            get { return _DispInfo; }
            set { this.SetProperty(ref this._DispInfo, value); }
        }

        private int _Progress;
        public int Progress
        {
            get { return _Progress; }
            set { this.SetProperty(ref this._Progress, value); }
        }

        private string _CopySettingTitle;
        public string CopySettingTitle
        {
            get { return _CopySettingTitle; }
            set { this.SetProperty(ref this._CopySettingTitle, value); }
        }

        private string _TotalCopyCount;
        public string TotalCopyCount
        {
            get { return _TotalCopyCount; }
            set { this.SetProperty(ref this._TotalCopyCount, value); }
        }
    }
    
}
