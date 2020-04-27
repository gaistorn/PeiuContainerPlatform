using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PeiuPlatform.Model.ExchangeModel
{
    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
