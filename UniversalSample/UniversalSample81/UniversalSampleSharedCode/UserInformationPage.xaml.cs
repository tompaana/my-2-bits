using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UniversalSampleSharedCode
{
    /// <summary>
    /// Page for displaying the user information.
    /// </summary>
    public sealed partial class UserInformationPage : Page
    {
        private IUserInformationHelper _userInformationHelper;

        public UserInformationPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _userInformationHelper = e.Parameter as IUserInformationHelper;

            if (_userInformationHelper != null)
            {
                displayNameTextBlock.Text = await _userInformationHelper.GetDisplayNameAsync();
                firstNameTextBlock.Text = await _userInformationHelper.GetFirstNameAsync();
                lastNameTextBlock.Text = await _userInformationHelper.GetLastNameAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No user information helper!");
            }
        }
    }
}
