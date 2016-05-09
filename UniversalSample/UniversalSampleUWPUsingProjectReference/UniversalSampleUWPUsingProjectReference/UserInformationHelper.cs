using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;

namespace UniversalSampleSharedCode
{
    public class UserInformationHelper : IUserInformationHelper
    {
        IReadOnlyList<User> _userList;
        private string _displayName;
        private string _firstName;
        private string _lastName;

        /// <summary>
        /// Retrieves the user information values.
        /// </summary>
        /// <returns></returns>
        public async Task GetUserProperties()
        {
            if (_userList == null)
            {
                _userList = await User.FindAllAsync();

                if (_userList.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("UserInformationHelper.GetUserProperties: Found "
                        + _userList.Count + " user(s)");

                    // Just get the first user
                    User user = _userList[0];

                    _displayName = (string)await user.GetPropertyAsync(KnownUserProperties.DisplayName);
                    _firstName = (string)await user.GetPropertyAsync(KnownUserProperties.FirstName);
                    _lastName = (string)await user.GetPropertyAsync(KnownUserProperties.LastName);

                    System.Diagnostics.Debug.WriteLine("UserInformationHelper.GetUserProperties: "
                        + _displayName + ", " + _firstName + ", " + _lastName);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("UserInformationHelper.GetUserProperties: No users found");
                }
            }
        }

        public async Task<string> GetDisplayNameAsync()
        {
            await GetUserProperties();
            return string.IsNullOrEmpty(_displayName) ? "n/a" : _displayName;
        }

        public async Task<string> GetFirstNameAsync()
        {
            await GetUserProperties();
            return string.IsNullOrEmpty(_firstName) ? "n/a" : _firstName;
        }

        public async Task<string> GetLastNameAsync()
        {
            await GetUserProperties();
            return string.IsNullOrEmpty(_lastName) ? "n/a" : _lastName;
        }
    }
}
