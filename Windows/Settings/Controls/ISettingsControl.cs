using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperSearch.Windows.Settings
{
    public interface ISettingsControl
    {
        bool IsActive { get; set; }

        void ResetValue();
    }
}
