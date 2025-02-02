﻿using System;
using System.Threading.Tasks;

using Microsoft.Practices.ServiceLocation;

using Sh8ps.Activation;
using Sh8ps.Helpers;

using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Sh8ps.Services
{
    internal class SuspendAndResumeService : ActivationHandler<LaunchActivatedEventArgs>
    {
        //// TODO WTS: For more information regarding the application lifecycle and how to handle suspend and resume, please see:
        //// Documentation: https://docs.microsoft.com/windows/uwp/launch-resume/app-lifecycle

        private const string StateFilename = "suspensionState";

        // TODO WTS: This event is fired just before the app enters in background. Subscribe to this event if you want to save your current state.
        public event EventHandler<OnBackgroundEnteringEventArgs> OnBackgroundEntering;

        public async Task SaveStateAsync()
        {
            var suspensionState = new SuspensionState()
            {
                SuspensionDate = DateTime.Now
            };

            var target = OnBackgroundEntering?.Target.GetType();
            var onBackgroundEnteringArgs = new OnBackgroundEnteringEventArgs(suspensionState, target);

            OnBackgroundEntering?.Invoke(this, onBackgroundEnteringArgs);

            await ApplicationData.Current.LocalFolder.SaveAsync(StateFilename, onBackgroundEnteringArgs);
        }

        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            await RestoreStateAsync();
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            return args.PreviousExecutionState == ApplicationExecutionState.Terminated;
        }

        private async Task RestoreStateAsync()
        {
            var saveState = await ApplicationData.Current.LocalFolder.ReadAsync<OnBackgroundEnteringEventArgs>(StateFilename);
            if (saveState?.Target != null)
            {
                var navigationService = ServiceLocator.Current.GetInstance<NavigationServiceEx>();
                navigationService.Navigate(saveState.Target.FullName, saveState.SuspensionState);
            }
        }
    }
}
