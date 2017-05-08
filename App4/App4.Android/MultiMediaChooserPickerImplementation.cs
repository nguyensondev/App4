using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;

using Plugin.CurrentActivity;
using Plugin.Permissions;
using Xamarin.Forms;
using App4.Droid;
using App4.test;

[assembly: Dependency(typeof(MultiMediaChooserPickerImplementation))]
namespace App4.Droid
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public class MultiMediaChooserPickerImplementation : IMultiMediaChooserPicker
    {
        private int GetRequestId()
        {
            var id = _requestId;
            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;

            return id;
        }

        private int _requestId;
        private TaskCompletionSource<List<FileData>> _completionSource;

        public async Task<List<FileData>> PickMultiImage(bool isMulti, int maxImage)
        {
            if (!await RequestStoragePermission())
            {
                return null;
            }

            var result = await ExecutePickMultiImage(isMulti, maxImage);
            return result;
        }

        public Task<List<FileData>> ExecutePickMultiImage(bool isMulti, int maxImage)
        {
            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<List<FileData>>(id);
            if (Interlocked.CompareExchange(ref _completionSource, ntcs, null) != null)
            {
#if DEBUG
                throw new InvalidOperationException("Only one operation can be active at a time");
#else
                return null;
#endif
            }
            Intent intent = null;
            if (isMulti)
            {
                intent = new Intent(Action.ActionPickMultiple);
                intent.PutExtra("maxImage", maxImage);
            }

            else
                intent = new Intent(Action.ActionPick);
            //event
            EventHandler<XViewEventArgs> handler = null;
            handler = (s, e) =>
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);

                CustomGalleryActivity.MediaSelected -= handler;
                if (e.CastObject != null)
                {
                    if (isMulti)
                    {
                        var result = e.CastObject as List<FileData>;
                        tcs.SetResult(result);
                    }
                    else
                    {
                        var result = new List<FileData>();
                        var image = e.CastObject as FileData;
                        result.Add(image);
                        tcs.SetResult(result);
                    }
                }
                   

            };

            CustomGalleryActivity.MediaSelected += handler;
            CrossCurrentActivity.Current.Activity.StartActivityForResult(intent, 200);

            return _completionSource.Task;
        }

        private static async Task<bool> RequestStoragePermission()
        {
            //We always have permission on anything lower than marshmallow.
            if ((int)Build.VERSION.SdkInt < 23) return true;

            var status =
                await CrossPermissions.Current.CheckPermissionStatusAsync(
                    Plugin.Permissions.Abstractions.Permission.Storage);
            if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                System.Diagnostics.Debug.WriteLine("Does not have storage permission granted, requesting.");
                var results =
                    await CrossPermissions.Current.RequestPermissionsAsync(
                        Plugin.Permissions.Abstractions.Permission.Storage);
                if (results.ContainsKey(Plugin.Permissions.Abstractions.Permission.Storage) &&
                    results[Plugin.Permissions.Abstractions.Permission.Storage] !=
                    Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("Storage permission Denied.");
                    return false;
                }
            }

            return true;
        }
    }
}