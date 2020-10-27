using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HubbubMapper.Shared.Pages
{
    public interface ISaveContents
    {
        bool IsSaveEnabled { get; }
        Task SaveContents();
    }
}
