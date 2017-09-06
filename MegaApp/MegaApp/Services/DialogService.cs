﻿using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.ViewModels;
using MegaApp.Views;
using MegaApp.Views.Dialogs;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.Services
{
    /// <summary>
    /// Service to display Dialogs to the user
    /// </summary>
    internal static class DialogService
    {
        /// <summary>
        /// Show an Alert Dialog that can be dismissed by a button.
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Content message of the dialog</param>
        /// <param name="button">Label of the dialog button</param>
        public static async Task ShowAlertAsync(string title, string message, string button = null)
        {
            if (button == null)
                button = ResourceService.UiResources.GetString("UI_Ok");

            var dialog = new AlertDialog(title, message, button);
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Show a dialog with a message and an "OK" and a "Cancel" button option
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Message of the dialog</param>
        /// <param name="okButton">Label for the "Ok" button</param>
        /// <param name="cancelButton">Label for the "Cancel" button</param>
        /// <returns>True if the "OK" button is pressed, else False</returns>
        public static async Task<bool> ShowOkCancelAsync(string title, string message,
            string acceptButton = null, string cancelButton = null)
        {
            return await ShowOkCancelAndWarningAsync(title, message, null, acceptButton, cancelButton);
        }

        /// <summary>
        /// Show a dialog with a message, a warning and an "OK" and a "Cancel" button option
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Message of the dialog</param>
        /// <param name="warning">Warning of the dialog</param>
        /// <param name="okButton">Label for the "Ok" button</param>
        /// <param name="cancelButton">Label for the "Cancel" button</param>
        /// <returns>True if the "OK" button is pressed, else False</returns>
        public static async Task<bool> ShowOkCancelAndWarningAsync(string title, string message, 
            string warning, string acceptButton = null, string cancelButton = null)
        {
            if (acceptButton == null)
                acceptButton = ResourceService.UiResources.GetString("UI_Ok");
            if (cancelButton == null)
                cancelButton = ResourceService.UiResources.GetString("UI_Cancel");

            var dialog = new OkCancelAndWarningDialog(title, message, warning, acceptButton, cancelButton);
            await dialog.ShowAsync();

            return dialog.DialogResult;
        }

        public static async void ShowOverquotaAlert()
        {
            var customMessageDialog = new CustomMessageDialog(
                ResourceService.AppMessages.GetString("AM_OverquotaAlert_Title"),
                ResourceService.AppMessages.GetString("AM_OverquotaAlert"),
                App.AppInformation, MessageDialogButtons.YesNo);

            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
            {
                UiService.OnUiThread(() =>
                {
                    NavigateService.Instance.Navigate(typeof(MyAccountPage), false,
                        NavigationObject.Create(typeof(MainViewModel), NavigationActionType.Upgrade));
                });
            };

            await customMessageDialog.ShowDialogAsync();
        }

        public static async void ShowTransferOverquotaWarning()
        {
            await ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_TransferOverquotaWarning_Title"),
                ResourceService.AppMessages.GetString("AM_TransferOverquotaWarning"));
        }

        /// <summary>
        /// Storage the instance of the <see cref="AwaitEmailConfirmationDialog"/>
        /// </summary>
        private static AwaitEmailConfirmationDialog awaitEmailConfirmationDialog;

        /// <summary>
        /// Show a dialog indicating that is waiting for an email confirmation
        /// </summary>
        /// <param name="email">Email for which is waiting confirmation</param>
        public static async void ShowAwaitEmailConfirmationDialog(string email)
        {
            if (awaitEmailConfirmationDialog == null)
                awaitEmailConfirmationDialog = new AwaitEmailConfirmationDialog(email);
            else
                awaitEmailConfirmationDialog.ViewModel.Email = email;

            await awaitEmailConfirmationDialog.ShowAsync();
        }

        /// <summary>
        /// Close the await email confirmation dialog if exists
        /// </summary>
        public static void CloseAwaitEmailConfirmationDialog()
        {
            awaitEmailConfirmationDialog?.Hide();
        }

        /// <summary>
        /// Show an Input Dialog to the uses
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="content">Content message of the dialog</param>
        /// <param name="settings">Input dialog behavior/option settings</param>
        /// <returns>The value of the input dialog when primary button was pressed, else NULL</returns>
        public static async Task<string> ShowInputDialogAsync(string title, string content,
            InputDialogSettings settings = null)
        {
            // Create default input settings if null
            if (settings == null)
                settings = new InputDialogSettings();

            var dialog = new ContentDialog
            {
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = ResourceService.UiResources.GetString("UI_Ok"),
                SecondaryButtonText = ResourceService.UiResources.GetString("UI_Cancel"),
                Title = title
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(0, 20, 0, 0)
            };
            var messageText = new TextBlock
            {
                Text = content,
                Margin = new Thickness(0, 0, 0, 12),
                TextWrapping = TextWrapping.WrapWholeWords,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            var input = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Text = settings.InputText,
                SelectionStart = 0,
            };

            if (settings.IgnoreExtensionInSelection)
            {
                var fileName = Path.GetFileNameWithoutExtension(settings.InputText);
                input.SelectionLength = fileName.Length;
            }
            else
            {
                input.SelectionLength = settings.InputText.Length;
            }

            stackPanel.Children.Add(messageText);
            stackPanel.Children.Add(input);
            dialog.Content = stackPanel;
            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.None:
                    return null;
                case ContentDialogResult.Primary:
                    return input.Text;
                case ContentDialogResult.Secondary:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Shows a dialog to allow copy a node link to the clipboard or share it using other app
        /// </summary>
        /// <param name="node">Node to share the link</param>
        public static async void ShowShareLink(NodeViewModel node)
        {
            var dialog = new ContentDialog
            {
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = ResourceService.UiResources.GetString("UI_Copy"),
                SecondaryButtonText = ResourceService.UiResources.GetString("UI_Share"),
                Title = ResourceService.UiResources.GetString("UI_ExportLink")
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(0, 20, 0, 0)
            };

            var messageText = new TextBlock
            {
                Text = node.OriginalMNode.getPublicLink(true),
                Margin = new Thickness(0, 20, 0, 12),
                TextWrapping = TextWrapping.WrapWholeWords,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var linkWithoutKey = new RadioButton
            {
                Content = ResourceService.UiResources.GetString("UI_LinkWithoutKey")
            };
            linkWithoutKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getPublicLink(false);

            var decryptionKey = new RadioButton
            {
                Content = ResourceService.UiResources.GetString("UI_DecryptionKey")
            };
            decryptionKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getBase64Key();

            var linkWithKey = new RadioButton
            {
                Content = ResourceService.UiResources.GetString("UI_LinkWithKey"),
                IsChecked = true
            };
            linkWithKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getPublicLink(true);

            stackPanel.Children.Add(linkWithoutKey);
            stackPanel.Children.Add(decryptionKey);
            stackPanel.Children.Add(linkWithKey);

            var stackPanelLinkWithExpirationDate = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var linkWithExpirationDateLabel = new TextBlock
            {
                Text = node.SetLinkExpirationDateText,
                Margin = new Thickness(0, 20, 0, 8),
                TextWrapping = TextWrapping.WrapWholeWords,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var enableLinkExpirationDateSwitch = new ToggleSwitch
            {
                IsOn = node.LinkWithExpirationTime,
                IsEnabled = AccountService.AccountDetails.IsProAccount
            };

            var expirationDateCalendarDatePicker = new CalendarDatePicker
            {
                IsEnabled = enableLinkExpirationDateSwitch.IsOn,
                DateFormat = "{day.integer(2)}‎/‎{month.integer(2)}‎/‎{year.full}",
                Date = node.LinkExpirationDate
            };
            expirationDateCalendarDatePicker.Opened += (sender, args) =>
            {
                expirationDateCalendarDatePicker.LightDismissOverlayMode = LightDismissOverlayMode.On;
                expirationDateCalendarDatePicker.MinDate = DateTime.Today.AddDays(1);                
            };
            expirationDateCalendarDatePicker.DateChanged += (sender, args) =>
            {
                expirationDateCalendarDatePicker.IsCalendarOpen = false;

                if (expirationDateCalendarDatePicker.Date == null)
                {
                    enableLinkExpirationDateSwitch.IsOn = false;
                    if (node.LinkExpirationTime > 0)
                        node.SetLinkExpirationTime(0);
                }
                else if (node.LinkExpirationDate == null ||
                    !node.LinkExpirationDate.Value.ToUniversalTime().Equals(expirationDateCalendarDatePicker.Date.Value.ToUniversalTime()))
                {
                    node.SetLinkExpirationTime(expirationDateCalendarDatePicker.Date.Value.ToUniversalTime().ToUnixTimeSeconds());
                }
            };

            enableLinkExpirationDateSwitch.Toggled += (sender, args) =>
            {
                expirationDateCalendarDatePicker.IsEnabled = enableLinkExpirationDateSwitch.IsOn;
                if (enableLinkExpirationDateSwitch.IsOn)
                    expirationDateCalendarDatePicker.Date = node.LinkExpirationDate;
                else
                    expirationDateCalendarDatePicker.Date = null;
            };

            stackPanelLinkWithExpirationDate.Children.Add(enableLinkExpirationDateSwitch);
            stackPanelLinkWithExpirationDate.Children.Add(expirationDateCalendarDatePicker);

            stackPanel.Children.Add(linkWithExpirationDateLabel);
            stackPanel.Children.Add(stackPanelLinkWithExpirationDate);
            stackPanel.Children.Add(messageText);
            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.None:
                    break;

                case ContentDialogResult.Primary:
                    ShareService.CopyLinkToClipboard(messageText.Text);
                    break;

                case ContentDialogResult.Secondary:
                    ShareService.ShareLink(messageText.Text);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates the sort menu with all the sort options.
        /// </summary>
        /// <param name="folder">Folder to sort.</param>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateSortMenu(FolderViewModel folder)
        {
            var isCameraUploadsGrid = (folder is CameraUploadsViewModel) &&
                                      ((CameraUploadsViewModel) folder).IsGridViewMode;

            MenuFlyout menuFlyout = new MenuFlyout();

            if (isCameraUploadsGrid)
            {
                menuFlyout.Items.Add(new MenuFlyoutItem()
                {
                    Text = ResourceService.UiResources.GetString("UI_SortOptionNewest"),
                    Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_MODIFICATION_DESC),
                    Command = new RelayCommand(() =>
                    {
                        UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                            (int)MSortOrderType.ORDER_MODIFICATION_DESC);
                        folder.LoadChildNodes();
                    })
                });

                menuFlyout.Items.Add(new MenuFlyoutItem()
                {
                    Text = ResourceService.UiResources.GetString("UI_SortOptionOldest"),
                    Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_MODIFICATION_ASC),
                    Command = new RelayCommand(() =>
                    {
                        UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                            (int)MSortOrderType.ORDER_MODIFICATION_ASC);
                        folder.LoadChildNodes();
                    })
                });

                return menuFlyout;
            }

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionNameAscending"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_ALPHABETICAL_ASC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_ALPHABETICAL_ASC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionNameDescending"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_ALPHABETICAL_DESC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_ALPHABETICAL_DESC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionLargest"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_SIZE_DESC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_SIZE_DESC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionSmallest"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_SIZE_ASC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_SIZE_ASC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionNewest"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_MODIFICATION_DESC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_MODIFICATION_DESC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionOldest"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_MODIFICATION_ASC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_MODIFICATION_ASC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionFilesAscending"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_DEFAULT_ASC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_DEFAULT_ASC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionFilesDescending"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_DEFAULT_DESC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_DEFAULT_DESC);
                    folder.LoadChildNodes();
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Gets the sort menu item foreground color depending on the current sort order of the folder.
        /// </summary>
        /// <param name="folder">Folder to check the current sort order.</param>
        /// <param name="sortOrder">Sort order to check.</param>
        /// <returns>The brush object with the color.</returns>
        private static Brush GetSortMenuItemForeground(FolderViewModel folder, int sortOrder)
        {
            if(folder?.FolderRootNode != null)
            {
                if (UiService.GetSortOrder(folder?.FolderRootNode?.Base64Handle, folder?.FolderRootNode?.Name) == sortOrder)
                    return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
            }

            return (SolidColorBrush)Application.Current.Resources["MegaAppForegroundBrush"];
        }

        /// <summary>
        /// Creates a sort menu for contacts.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateContactsSortMenu(ContactsListViewModel contacts)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionNameAscending"),
                Foreground = GetContactsSortMenuItemForeground(contacts, ContactsSortOptions.NameAscending),
                Command = new RelayCommand(() =>
                {
                    contacts.CurrentOrder = ContactsSortOptions.NameAscending;
                    contacts.SortBy(ContactsSortOptions.NameAscending);
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionNameDescending"),
                Foreground = GetContactsSortMenuItemForeground(contacts, ContactsSortOptions.NameDescending),
                Command = new RelayCommand(() =>
                {
                    contacts.CurrentOrder = ContactsSortOptions.NameDescending;
                    contacts.SortBy(ContactsSortOptions.NameDescending);
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Creates a sort menu for contact requests.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateContactRequestsSortMenu(ContactRequestsListViewModel contactRequests)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionEmailAscending"),
                Foreground = GetContactsSortMenuItemForeground(contactRequests, ContactsSortOptions.EmailAscending),
                Command = new RelayCommand(() =>
                {
                    contactRequests.CurrentOrder = ContactsSortOptions.EmailAscending;
                    contactRequests.SortBy(ContactsSortOptions.EmailAscending);
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionEmailDescending"),
                Foreground = GetContactsSortMenuItemForeground(contactRequests, ContactsSortOptions.EmailDescending),
                Command = new RelayCommand(() =>
                {
                    contactRequests.CurrentOrder = ContactsSortOptions.EmailDescending;
                    contactRequests.SortBy(ContactsSortOptions.EmailDescending);
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Gets the sort <see cref="ContactsListViewModel"/> or <see cref="ContactRequestsListViewModel"/> menu item foreground color depending on the current sort order.
        /// </summary>
        /// <param name="list"><see cref="ContactsListViewModel"/> or <see cref="ContactRequestsListViewModel"/> to check the current sort order.</param>
        /// <param name="sortOrder">Sort order to check.</param>
        /// <returns>The brush object with the color.</returns>
        private static Brush GetContactsSortMenuItemForeground(object list, ContactsSortOptions sortOrder)
        {
            if ((list as ContactsListViewModel)?.CurrentOrder == sortOrder || 
                (list as ContactRequestsListViewModel)?.CurrentOrder == sortOrder)
            {
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
            }

            return (SolidColorBrush)Application.Current.Resources["MegaAppForegroundBrush"];
        }

        /// <summary>
        /// Creates a sort menu for contacts.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateContactSharedItemsSortMenu(ContactSharedItemsViewModel sharedItems)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionNameAscending"),
                Foreground = GetContactInSharesSortMenuItemForeground(sharedItems, MSortOrderType.ORDER_ALPHABETICAL_ASC),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = MSortOrderType.ORDER_ALPHABETICAL_ASC;
                    sharedItems.SortBy(MSortOrderType.ORDER_ALPHABETICAL_ASC);
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionNameDescending"),
                Foreground = GetContactInSharesSortMenuItemForeground(sharedItems, MSortOrderType.ORDER_ALPHABETICAL_DESC),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = MSortOrderType.ORDER_ALPHABETICAL_DESC;
                    sharedItems.SortBy(MSortOrderType.ORDER_ALPHABETICAL_DESC);
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionLastModifiedAscending"),
                Foreground = GetContactInSharesSortMenuItemForeground(sharedItems, MSortOrderType.ORDER_MODIFICATION_ASC),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = MSortOrderType.ORDER_MODIFICATION_ASC;
                    sharedItems.SortBy(MSortOrderType.ORDER_MODIFICATION_ASC);
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionLastModifiedDescending"),
                Foreground = GetContactInSharesSortMenuItemForeground(sharedItems, MSortOrderType.ORDER_MODIFICATION_DESC),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = MSortOrderType.ORDER_MODIFICATION_DESC;
                    sharedItems.SortBy(MSortOrderType.ORDER_MODIFICATION_DESC);
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Gets the sort <see cref="ContactSharedItemsViewModel"/> menu item foreground color depending on the current sort order.
        /// </summary>
        /// <param name="sharedItems"><see cref="ContactSharedItemsViewModel"/> to check the current sort order.</param>
        /// <param name="sortOrder">Sort order to check.</param>
        /// <returns>The brush object with the color.</returns>
        private static Brush GetContactInSharesSortMenuItemForeground(ContactSharedItemsViewModel sharedItems, MSortOrderType sortOrder)
        {
            if (sharedItems?.CurrentOrder == sortOrder)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            return (SolidColorBrush)Application.Current.Resources["MegaAppForegroundBrush"];
        }
    }

    public class InputDialogSettings
    {
        /// <summary>
        /// Default text for the input in the dialog
        /// </summary>
        public string InputText { get; set; } = string.Empty;

        /// <summary>
        /// Is the text in the input selected as default
        /// </summary>
        public bool IsTextSelected { get; set; }

        /// <summary>
        /// Ignore extensions when the text is default selected.
        /// </summary>
        public bool IgnoreExtensionInSelection { get; set; }
    }
}
