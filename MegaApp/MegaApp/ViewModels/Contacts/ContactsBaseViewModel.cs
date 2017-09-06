﻿using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.ViewModels.Contacts
{
    public class ContactsBaseViewModel<T> : BaseSdkViewModel
    {
        public ContactsBaseViewModel(bool? isOutgoing = null)
        {
            this.isOutgoing = isOutgoing;

            this.MultiSelectCommand = new RelayCommand(MultiSelect);
            this.SelectionChangedCommand = new RelayCommand(SelectionChanged);
        }

        #region Events

        /// <summary>
        /// Event triggered when the add contact menu option is tapped
        /// </summary>
        public event EventHandler AddContactTapped;

        /// <summary>
        /// Event invocator method called when the add contact menu option is tapped
        /// </summary>
        protected virtual void OnAddContactTapped()
        {
            this.AddContactTapped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the multi select scenario is enabled
        /// </summary>
        public event EventHandler MultiSelectEnabled;

        /// <summary>
        /// Event invocator method called when the multi select scenario is enabled
        /// </summary>
        protected virtual void OnMultiSelectEnabled()
        {
            this.MultiSelectEnabled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the multi select scenario is disabled
        /// </summary>
        public event EventHandler MultiSelectDisabled;

        /// <summary>
        /// Event invocator method called when the multi select scenario is disabled
        /// </summary>
        protected virtual void OnMultiSelectDisabled()
        {
            this.MultiSelectDisabled?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Commands
        
        public ICommand MultiSelectCommand { get; }
        public ICommand SelectionChangedCommand { get; }

        public virtual ICommand AddContactCommand { get; }
        public virtual ICommand RemoveContactCommand { get; }

        public virtual ICommand AcceptContactRequestCommand { get; }
        public virtual ICommand CancelContactRequestCommand { get; }
        public virtual ICommand DeclineContactRequestCommand { get; }
        public virtual ICommand RemindContactRequestCommand { get; }

        public virtual ICommand OpenContactProfileCommand { get; }
        public virtual ICommand CloseContactProfileCommand { get; }

        public virtual ICommand InvertOrderCommand { get; }

        #endregion

        #region Methods

        private void ListOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ItemCollection.Items))
            {
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems));
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        private void SelectionChanged()
        {
            if (DeviceService.GetDeviceType() == DeviceFormFactorType.Desktop)
                this.IsMultiSelectActive = (this.IsMultiSelectActive && this.ItemCollection.OneOrMoreSelected) ||
                    this.ItemCollection.MoreThanOneSelected;
            else
                this.IsMultiSelectActive = this.IsMultiSelectActive && this.ItemCollection.OneOrMoreSelected;

            if (this.ItemCollection.HasSelectedItems)
            {
                var focusedItem = this.ItemCollection.SelectedItems.Last();
                this.FocusedItem = focusedItem;
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        /// <summary>
        /// Sets if multiselect is active or not.
        /// </summary>
        private void MultiSelect() => this.IsMultiSelectActive = !this.IsMultiSelectActive;

        #endregion

        #region Properties

        private bool? isOutgoing { get; set; }

        private CollectionViewModel<T> _itemCollection;
        public CollectionViewModel<T> ItemCollection
        {
            get { return _itemCollection; }
            set
            {
                if (_itemCollection != null)
                    _itemCollection.PropertyChanged -= ListOnPropertyChanged;

                SetField(ref _itemCollection, value);

                if (_itemCollection != null)
                    _itemCollection.PropertyChanged += ListOnPropertyChanged;
            }
        }

        private T _focusedItem;
        public T FocusedItem
        {
            get { return _focusedItem; }
            set { SetField(ref _focusedItem, value); }
        }

        private ContactsContentType _contentType;
        public ContactsContentType ContentType
        {
            get { return _contentType; }
            set { SetField(ref _contentType, value); }
        }

        public string OrderTypeAndNumberOfItems
        {
            get
            {
                switch(this.CurrentOrder)
                {
                    case ContactsSortOptions.EmailAscending:
                    case ContactsSortOptions.EmailDescending:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByEmail"), 
                            this.ItemCollection.Items.Count);

                    case ContactsSortOptions.NameAscending:
                    case ContactsSortOptions.NameDescending:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByName"),
                            this.ItemCollection.Items.Count);

                    default:
                        return string.Empty;
                }
            }
        }

        public string OrderTypeAndNumberOfSelectedItems
        {
            get
            {
                switch (this.CurrentOrder)
                {
                    case ContactsSortOptions.EmailAscending:
                    case ContactsSortOptions.EmailDescending:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByEmailMultiSelect"),
                            this.ItemCollection.SelectedItems.Count, this.ItemCollection.Items.Count);

                    case ContactsSortOptions.NameAscending:
                    case ContactsSortOptions.NameDescending:
                        return string.Format(ResourceService.UiResources.GetString("UI_ListSortedByNameMultiSelect"),
                            this.ItemCollection.SelectedItems.Count, this.ItemCollection.Items.Count);

                    default:
                        return string.Empty;
                }
            }
        }

        private bool _isMultiSelectActive;
        public bool IsMultiSelectActive
        {
            get { return _isMultiSelectActive || this.ItemCollection.MoreThanOneSelected; }
            set
            {
                if (!SetField(ref _isMultiSelectActive, value)) return;

                if (_isMultiSelectActive)
                {
                    this.OnMultiSelectEnabled();
                }
                else
                {
                    this.ItemCollection.ClearSelection();
                    OnPropertyChanged(nameof(this.IsMultiSelectActive));
                    this.OnMultiSelectDisabled();
                }
            }
        }

        private ContactsSortOptions _currentOrder;
        public ContactsSortOptions CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                SetField(ref _currentOrder, value);

                OnPropertyChanged(nameof(this.IsCurrentOrderAscending));
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfItems));
                OnPropertyChanged(nameof(this.OrderTypeAndNumberOfSelectedItems));
            }
        }

        public bool IsCurrentOrderAscending
        {
            get
            {
                switch(this.CurrentOrder)
                {
                    case ContactsSortOptions.EmailAscending:
                    case ContactsSortOptions.NameAscending:
                    default:
                        return true;

                    case ContactsSortOptions.EmailDescending:
                    case ContactsSortOptions.NameDescending:
                        return false;
                }
            }
        }

        #endregion

        #region UiResources

        public string AddContactText => ResourceService.UiResources.GetString("UI_AddContact");
        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string MultiSelectText => ResourceService.UiResources.GetString("UI_MultiSelect");
        public string RemoveContactText => ResourceService.UiResources.GetString("UI_RemoveContact");
        public string SortByText => ResourceService.UiResources.GetString("UI_SortBy");
        public string ViewProfileText => ResourceService.UiResources.GetString("UI_ViewProfile");

        public string AcceptContactText => ResourceService.UiResources.GetString("UI_AcceptContact");
        public string CancelInviteText => ResourceService.UiResources.GetString("UI_CancelInvite");
        public string DenyContactText => ResourceService.UiResources.GetString("UI_DenyContact");
        public string RemindContactText => ResourceService.UiResources.GetString("UI_RemindContact");

        public string DeselectAllText => ResourceService.UiResources.GetString("UI_DeselectAll");
        public string SelectAllText => ResourceService.UiResources.GetString("UI_SelectAll");

        #endregion

        #region VisualResources

        public string AddContactPathData => ResourceService.VisualResources.GetString("VR_AddContactPathData");
        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string MultiSelectPathData => ResourceService.VisualResources.GetString("VR_MultiSelectPathData");
        public string RemovePathData => ResourceService.VisualResources.GetString("VR_RemovePathData");
        public string SortByPathData => ResourceService.VisualResources.GetString("VR_SortByPathData");
        public string ViewProfilePathData => ResourceService.VisualResources.GetString("VR_ViewProfilePathData");

        public string AcceptPathData => ResourceService.VisualResources.GetString("VR_ConfirmPathData");
        public string DeclinePathData => ResourceService.VisualResources.GetString("VR_CancelPathData");

        #endregion
    }
}
