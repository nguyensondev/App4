using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Com.Nostra13.Universalimageloader.Cache.Disc.Impl;
using Com.Nostra13.Universalimageloader.Cache.Memory.Impl;
using Com.Nostra13.Universalimageloader.Core;
using Com.Nostra13.Universalimageloader.Core.Assist;
using Com.Nostra13.Universalimageloader.Core.Listener;
using Com.Nostra13.Universalimageloader.Utils;
using Xamarin.Forms;
using OS = Android.OS;
using Button = Android.Widget.Button;
using File = Java.IO.File;
using Android.Media;
using Java.IO;
using System.ComponentModel;
using App4.test;

namespace App4.Droid
{
    [IntentFilter(
        new[] { Action.ActionPick, Action.ActionPickMultiple },
        Categories = new[] { Intent.CategoryDefault })]
    [Activity]
    public class CustomGalleryActivity : Activity, GridView.IMultiChoiceModeListener
    {
        internal static event EventHandler<XViewEventArgs> MediaSelected;
        private GridView _gridGallery;
        private Handler _handler;
        private GalleryAdapter _adapter;

        private ImageView _imgNoMedia;
        private Button _btnGalleryOk;

        private string _action;
        private ImageLoader _imageLoader;
        private int limitChossen = 5;
        private int numcurent = 0;
        private TextView _currentImage;
        private ImageView _imgBack;
        private TextView lblDone;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            limitChossen = Intent.GetIntExtra("maxImage", 5);
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.gallery);

            _action = Intent.Action;
            if (string.IsNullOrEmpty(_action))
            {
                SetResult(Result.Canceled, null);
                Finish();
            }
            InitImageLoader();
            Init();
        }

        private void InitImageLoader()
        {
            try
            {
                var CACHE_DIR = OS.Environment.ExternalStorageDirectory.AbsoluteFile + "/.temp_tmp";
                new File(CACHE_DIR).Mkdirs();

                var cacheDir = StorageUtils.GetOwnCacheDirectory(BaseContext, CACHE_DIR);

                var defaultOptions =
                    new DisplayImageOptions.
                            Builder()
                        .CacheOnDisk(true).
                        ImageScaleType(ImageScaleType.Exactly).
                        BitmapConfig(Bitmap.Config.Rgb565).
                        Build();

                var builder =
                    new ImageLoaderConfiguration.
                            Builder(BaseContext).
                        DefaultDisplayImageOptions(defaultOptions).
                        DiskCache(new UnlimitedDiskCache(cacheDir)).
                        MemoryCache(new WeakMemoryCache());

                var config = builder.Build();
                _imageLoader = ImageLoader.Instance;
                _imageLoader.Init(config);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e.Message);
                // not going to swallow the exception
                throw;
            }
        }

        private void Init()
        {
            _handler = new Handler();

            _gridGallery = (GridView)FindViewById(Resource.Id.gridGallery);
            _gridGallery.FastScrollEnabled = true;
            _gridGallery.SetMultiChoiceModeListener(this);
            var listener = new PauseOnScrollListener(_imageLoader, true, true);
            _gridGallery.SetOnScrollListener(listener);

            _adapter = new GalleryAdapter(ApplicationContext, _imageLoader);

            if (string.Compare(_action, Action.ActionPickMultiple, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                FindViewById(Resource.Id.llBottomContainer).Visibility = ViewStates.Visible;
                lblDone = (TextView)FindViewById(Resource.Id.lblDone);
                lblDone.Click += OnOkClicked;
                _currentImage = FindViewById<TextView>(Resource.Id.lblCountImage);
                _currentImage.Text = "(" + numcurent + "/" + limitChossen + ")";
                _adapter.IsMultiplePick = true;

            }
            else if (string.Compare(_action, Action.ActionPick, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                FindViewById(Resource.Id.llBottomContainer).Visibility = ViewStates.Gone;
                FindViewById(Resource.Id.lblDone).Visibility = ViewStates.Gone;
                FindViewById<TextView>(Resource.Id.lblCountImage).Visibility = ViewStates.Gone;
                _adapter.IsMultiplePick = false;
            }


            _gridGallery.Adapter = _adapter;
            _imgNoMedia = (ImageView)FindViewById(Resource.Id.imgNoMedia);
            _imgBack = FindViewById<ImageView>(Resource.Id.imgBack);
            _imgBack.Click += _imgBack_Click;
            _gridGallery.ItemClick += OnItemClicked;



            Task.Run(() =>
            {
                _handler.Post(() =>
                {
                    _adapter.AddAll(GalleryPhotos);
                    CheckImageStatus();
                });
            });
        }
        public static ProgressDialog _progressing = null;
        public void hideProcessingDialog()
        {
            try
            {
                if (_progressing != null)
                {
                    _progressing.Dismiss();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void showProcessingDialog()
        {
            try
            {
                //if (_progressing == null){
                _progressing = new ProgressDialog(this);
                _progressing.Indeterminate = true;
                _progressing.SetProgressStyle(ProgressDialogStyle.Spinner);
                //_progressing.SetMessage(CmmFunction.getTitle("MESS_PLEASE_WAIT"));
                _progressing.SetMessage("Xin đợi");
                _progressing.SetCancelable(false);
                //}
                _progressing.Show();
            }
            catch (Exception ex)
            {

            }
        }
        private void _imgBack_Click(object sender, EventArgs e)
        {
            numcurent = 0;
            MediaSelected?.Invoke(this, new XViewEventArgs(nameof(MediaSelected), null));
            Finish();
        }

        private void CheckImageStatus()
        {
            _imgNoMedia.Visibility = _adapter.IsEmpty ? ViewStates.Visible : ViewStates.Gone;
        }

        /// <summary>
        /// Handles the click event for the 'OK' button, rather than using a Listener
        /// </summary>
        private async void OnOkClicked(object sender, EventArgs args)
        {
            TextView th = sender as TextView;
            th.Enabled = false;
            try
            {
                showProcessingDialog();
                var allPath =
               _adapter.
                   Selected.
                   Select(x => x.SdCardPath).
                   ToArray();
                if (allPath.Length > 0)
                {
                    var listStream = new List<FileData>();
                    await Task.Delay(500);
                    await Task.Run(() =>
                    {
                        for (int i = 0; i < allPath.Length; i++)
                        {
                            FileData img = ConvertURIImageSource(allPath[i]);
                            if (img != null)
                                listStream.Add(img);
                        }
                    });
                    if (listStream.Count > 0)
                        MediaSelected?.Invoke(this, new XViewEventArgs(nameof(MediaSelected), listStream));
                }
                else
                    MediaSelected?.Invoke(this, new XViewEventArgs(nameof(MediaSelected), null));
                //linq
                // var listStream = allPath.Select(IOUtil.ReadFileFromPath).Select(file => ImageSource.FromStream(() => new MemoryStream(file))).ToList();

                Finish();
            }
            catch (Exception)
            {


            }
            finally
            {
                th.Enabled = true;
                hideProcessingDialog();
            }

        }

        public override void OnBackPressed()
        {
            MediaSelected?.Invoke(this, new XViewEventArgs(nameof(MediaSelected), null));
            base.OnBackPressed();
        }

        /// <summary>
        /// Handles the click event for a photo on the gallery
        /// </summary>
        /// ov
        /// 

        private async void OnItemClicked(object sender, AdapterView.ItemClickEventArgs args)
        {
            if (_adapter.IsMultiplePick)
            {
                _adapter.ChangeSelection(args.View, args.Position, ref numcurent, (numcurent < limitChossen) ? true : false);
                _currentImage.Text = "(" + numcurent + "/" + limitChossen + ")";
            }
            else
            {
                try
                {
                    showProcessingDialog();
                    var item = _adapter[args.Position];
                    var data = new Intent().PutExtra("single_path", item.SdCardPath);
                    SetResult(Result.Ok, data);
                    FileData imageSource = null;
                    await Task.Run(() => { imageSource = ConvertURIImageSource(item.SdCardPath); });

                    if (imageSource != null)
                        MediaSelected?.Invoke(this, new XViewEventArgs(nameof(MediaSelected), imageSource));
                    else
                        MediaSelected?.Invoke(this, new XViewEventArgs(nameof(MediaSelected), null));
                    Finish();
                }
                catch (Exception ex)
                {


                }
                finally
                {
                    hideProcessingDialog();
                }

            }
        }
        private FileData ConvertURIImageSource(string input)
        {
            try
            {
                FileData reulst = new FileData();
                reulst.FilePath = input;
                var ime = IOUtil.ReadFileFromPath(input);
                File temp = new File(input);
                int rotation = GetCameraPhotoOrientation(temp);
                ImageSource imageSource = null;
                if (rotation != 0)
                {
                    ime = IOUtil.ResizeImageNew(ime, 100, 100, input);
                    Bitmap bitmap1 = BitmapFactory.DecodeByteArray(ime, 0, ime.Length);
                    Matrix mtx = new Matrix();
                    mtx.PreRotate(rotation);
                    bitmap1 = Bitmap.CreateBitmap(bitmap1, 0, 0, bitmap1.Width, bitmap1.Height, mtx, false);
                    mtx.Dispose();
                    mtx = null;
                    imageSource = ImageSource.FromStream(() =>
                    {
                        MemoryStream ms = new MemoryStream();
                        bitmap1.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                        ms.Seek(0L, SeekOrigin.Begin);
                        return ms;
                    });
                }
                else
                {
                    imageSource = ImageSource.FromStream(() => new MemoryStream(ime));
                }
                reulst.FileName = System.IO.Path.GetFileName(input);
                reulst.DataImage = ime;
                reulst.Source = imageSource;
                return reulst;
            }
            catch (Exception)
            {


            }
            return null;
        }

        private int GetCameraPhotoOrientation(File file)
        {
            int rotate = 0;
            try
            {
                if (file.Exists())
                {
                    ExifInterface exif = new ExifInterface(file.AbsolutePath);
                    int orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, (int)Android.Media.Orientation.Undefined);
                    switch (orientation)
                    {
                        case 1:
                            rotate = 0;
                            break;

                        case 8:
                            rotate = 270;
                            break;

                        case 3:
                            rotate = 180;
                            break;

                        case 6:
                            rotate = 90;
                            break;
                    }
                }
            }
            catch (Exception e)
            {

            }
            return rotate;
        }

        public void OnItemCheckedStateChanged(ActionMode mode, int position, long id, bool @checked)
        {
            if (_gridGallery.CheckedItemCount > limitChossen)
                _gridGallery.SetItemChecked(position, false);
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            return true;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        public void OnDestroyActionMode(ActionMode mode)
        {

        }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        private IEnumerable<CustomGallery> GalleryPhotos
        {
            get
            {
                var galleryList = new List<CustomGallery>();

                try
                {
                    var columns =
                        new[]
                        {
                            MediaStore.Images.ImageColumns.Data,
                            MediaStore.Images.ImageColumns.Id
                        };

                    var orderBy = MediaStore.Images.ImageColumns.Id;

                    var imagecursor = ManagedQuery(
                        MediaStore.Images.Media.ExternalContentUri,
                        columns,
                        null,
                        null,
                        orderBy);

                    if (imagecursor != null && imagecursor.Count > 0)
                    {

                        while (imagecursor.MoveToNext())
                        {
                            var item = new CustomGallery();

                            var dataColumnIndex = imagecursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);

                            item.SdCardPath = imagecursor.GetString(dataColumnIndex);

                            galleryList.Add(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e.Message);
                    throw;
                }

                // show newest photo at beginning of the list
                galleryList.Reverse();

                return galleryList;
            }
        }
    }
}