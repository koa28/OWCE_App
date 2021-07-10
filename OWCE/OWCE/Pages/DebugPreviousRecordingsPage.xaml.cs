using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OWCE.Pages
{
    public partial class DebugPreviousRecordingsPage : BaseContentPage
    {
        public class DebugRecordingSummary : INotifyPropertyChanged, IEquatable<DebugRecordingSummary>
        {
            string _filename;
            public string Filename
            {
                get { return _filename; }
                set {
                    _filename = value;
                    NotifyPropertyChanged(nameof(Filename));
                    NotifyPropertyChanged(nameof(Name));
                }
            }
            public string Name => Path.GetFileNameWithoutExtension(_filename);
            public DateTime Created { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public bool Equals(DebugRecordingSummary other)
            {
                return (other.Filename == Filename);
            }

            void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObservableCollection<DebugRecordingSummary> PreviousRecordings { get; } = new ObservableCollection<DebugRecordingSummary>();
        
        public DebugPreviousRecordingsPage()
        {
            InitializeComponent();


            var files = Directory.GetFiles(App.Current.LogsDirectory, "*.bin");
            foreach (var file in files)
            {
                var debugRecordingSummary = new DebugRecordingSummary()
                {
                    Filename = file,
                    Created = File.GetCreationTime(file),
                };
                PreviousRecordings.Add(debugRecordingSummary);
            }

            BindingContext = this;

            CustomToolbarItems.Add(new Views.CustomToolbarItem()
            {
                Position = Views.CustomToolbarItemPosition.Left,
                Text = "Close",
                Command = new Command(() =>
                {
                    Navigation.PopModalAsync();
                })
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        async void CollectionView_SelectionChanged(System.Object sender, Xamarin.Forms.SelectionChangedEventArgs e)
        {
            if (sender is CollectionView collectionView)
            {
                if (e.CurrentSelection.Count == 0)
                {
                    return;
                }

                if (e.CurrentSelection[0] is DebugRecordingSummary debugRecordingSummary)
                {
                    collectionView.SelectedItem = null;

                    var shareFileRequest = new ShareFileRequest($"Share {debugRecordingSummary.Name}", new Xamarin.Essentials.ShareFile(debugRecordingSummary.Filename, "application/octet-stream"));
                    await Share.RequestAsync(shareFileRequest);
                }
            }
        }

        async void SwipeItem_Delete(System.Object sender, System.EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is DebugRecordingSummary debugRecordingSummary)
            {
                var shouldDelete = await DisplayAlert("Delete", $"Are you sure you want to delete \"{debugRecordingSummary.Name}\"?", "Delete", "Cancel");
                if (shouldDelete)
                {

                    try
                    {
                        File.Delete(debugRecordingSummary.Filename);
                        PreviousRecordings.Remove(debugRecordingSummary);
                    }
                    catch (Exception )
                    {
                        await DisplayAlert("Error", "There was a problem deleting your ride.", "OK");
                    }
                }
            }
        }

        async void SwipeItem_Rename(System.Object sender, System.EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is DebugRecordingSummary debugRecordingSummary)
            {
                var oldName = Path.GetFileNameWithoutExtension(debugRecordingSummary.Filename);
                var newName = await DisplayPromptAsync("Rename Ride", "Please enter a new ride name below", initialValue: oldName);

                if (String.IsNullOrWhiteSpace(newName))
                {
                    await DisplayAlert("Sorry", "Invalid name for new ride.", "OK");
                    return;
                }

                var directory = Path.GetDirectoryName(debugRecordingSummary.Filename);
                var oldFilename = Path.GetFileName(debugRecordingSummary.Filename);
                var newFilename = Path.Combine(directory, $"{newName}.bin");

                if (File.Exists(newFilename))
                {
                    await DisplayAlert("Sorry", "Ride with the same name already exists.", "OK");
                    return;
                }

                try
                {
                    File.Move(oldFilename, newFilename);
                    debugRecordingSummary.Filename = newFilename;
                }
                catch (Exception )
                {
                    await DisplayAlert("Error", "There was a problem renaming your ride.", "OK");
                }
            }
        }
    }
}
