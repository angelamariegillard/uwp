﻿using System;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.Views.MultiFactorAuth;

namespace MegaApp.ViewModels.Dialogs
{
    public class MultiFactorAuthSetupDialogViewModel : BaseContentDialogViewModel
    {
        public MultiFactorAuthSetupDialogViewModel() : base()
        {
            this.SetupTwoFactorAuthCommand = new RelayCommand(SetupTwoFactorAuth);

            this.TitleText = ResourceService.AppMessages.GetString("AM_2FA_SetupDialogTitle");
            this.MessageText = ResourceService.AppMessages.GetString("AM_2FA_SetupDialogDescription");
        }

        #region Commands

        public ICommand SetupTwoFactorAuthCommand { get; }        

        #endregion

        #region Properties

        /// <summary>
        /// Result value of the dialog.
        /// TRUE if the user continues with the setup process or FALSE in other case.
        /// </summary>
        public bool DialogResult = false;

        /// <summary>
        /// Uri image to display in the dialog
        /// </summary>
        public Uri MultiFactorAuthImageUri => 
            new Uri("ms-appx:///Assets/MultiFactorAuth/multiFactorAuth.png");

        #endregion

        #region Methods

        private void SetupTwoFactorAuth()
        {
            this.DialogResult = true;
            this.OnHideDialog();
            NavigateService.Instance.Navigate(typeof(MultiFactorAuthAppSetupPage));
        }

        #endregion

        #region UiResources

        public string SetupTwoFactorAuthText => ResourceService.UiResources.GetString("UI_Setup2FA");
        
        #endregion
    }
}
