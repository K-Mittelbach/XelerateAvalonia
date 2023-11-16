using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace XelerateAvalonia.Auxilaries
{
    public interface IServiceLocator
    {
        IServiceCollection ServiceCollection { get; }

        ServiceProvider ServiceProvider { get; }
    }
}
