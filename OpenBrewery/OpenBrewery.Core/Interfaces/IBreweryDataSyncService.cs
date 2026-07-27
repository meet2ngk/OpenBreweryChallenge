using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBrewery.Core.Interfaces
{
    public interface IBreweryDataSyncService
    {
        Task InitializeDatabaseAsync();
    }
}
