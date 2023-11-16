using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XelerateAvalonia.Auxilaries
{
    public interface ILoadable
    {
        Task Load(params object[] parameters);
    }
}
