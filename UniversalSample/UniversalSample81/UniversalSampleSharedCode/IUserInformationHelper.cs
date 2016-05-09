using System;
using System.Threading.Tasks;

namespace UniversalSampleSharedCode
{
    public interface IUserInformationHelper
    {
        Task<string> GetDisplayNameAsync();

        Task<string> GetFirstNameAsync();

        Task<string> GetLastNameAsync();
    }
}
