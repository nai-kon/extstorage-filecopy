using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExtStorageTrans
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

        private string _TransSettingTitle;
        public string TransSettingTitle
        {
            get { return _TransSettingTitle; }
            set { this.SetProperty(ref this._TransSettingTitle, value); }
        }

        private string _TotalTransCount;
        public string TotalTransCount
        {
            get { return _TotalTransCount; }
            set { this.SetProperty(ref this._TotalTransCount, value); }
        }
    }
    
}
