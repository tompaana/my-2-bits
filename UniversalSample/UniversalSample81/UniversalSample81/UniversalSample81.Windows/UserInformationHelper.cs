using System;
using System.Threading.Tasks;
using Windows.System.UserProfile;

namespace UniversalSampleSharedCode
{
    public class UserInformationHelper : IUserInformationHelper
    {
        public async Task<string> GetDisplayNameAsync()
        {
            return await UserInformation.GetDisplayNameAsync();
        }

        public async Task<string> GetFirstNameAsync()
        {
            return await UserInformation.GetFirstNameAsync();
        }

        public async Task<string> GetLastNameAsync()
        {
            return await UserInformation.GetLastNameAsync();
        }
    }
}
