using Hubbub;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;

namespace HubbubMapper.Shared
{
    public class GlobalProperty : BindableBase
    {
        public readonly static GlobalProperty Common = new GlobalProperty();

        public ModbusHubbubMappingTemplate HubbubTemplate
        {
            get => mHubbubTemplate;
            set
            {
                this.SetProperty(ref this.mHubbubTemplate, value);
            }
        }
        private ModbusHubbubMappingTemplate mHubbubTemplate;




        public ModbusHubbub Hubbub
        {
            get => mHubbub;
            set
            {
                this.SetProperty(ref this.mHubbub, value);
            }
        }
        private ModbusHubbub mHubbub;

    }
}
