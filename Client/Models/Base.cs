using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public abstract class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged<T>(ref T obj, T newVal, string? caller = null)
        {
            if (null != obj && obj.Equals(newVal))
                return;
            obj = newVal;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller ?? nameof(obj)));
        }
    }
}
