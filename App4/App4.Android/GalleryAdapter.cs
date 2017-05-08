using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using Com.Nostra13.Universalimageloader.Core;
using Com.Nostra13.Universalimageloader.Core.Listener;
using Android.Graphics;
using Android.Media;
using Com.Nostra13.Universalimageloader.Core.Assist;
using Java.IO;


namespace App4.Droid
{
    public class GalleryAdapter : BaseAdapter<CustomGallery>
    {
        public class ViewHolder : Java.Lang.Object
        {
            public ImageView ImgQueue { get; set; }

            public TextView ImgQueueMultiSelected { get; set; }
        }

        private class SimpleImageLoadingListenerImpl : SimpleImageLoadingListener
        {
            public SimpleImageLoadingListenerImpl(ViewHolder holder)
            {
                _holder = holder;
            }

            private readonly ViewHolder _holder;

            public override void OnLoadingStarted(string imageUri, View view)
            {
                _holder.
                    ImgQueue.
                    SetImageResource(Resource.Drawable.no_media);

                base.OnLoadingStarted(imageUri, view);
            }
            public override void OnLoadingComplete(string p0, View p1, Bitmap p2)
            {
                try
                {
                    if (p2 != null)
                    {

                        File file = ImageLoader.Instance.DiskCache.Get(p0);
                        if (file == null)
                        {
                            return;
                        }
                        int rotation = GetCameraPhotoOrientation(file);
                        // Bitmap bitmap = null;
                        if (rotation != 0)
                        {
                            Matrix mtx = new Matrix();
                            mtx.PreRotate(rotation);
                            //float ratio = Math.Min( (float)100 / p2.Width, (float)100 / p2.Height);
                            //int width = int.Parse((ratio * p2.Width).ToString());
                            //int height = int.Parse((ratio * p2.Height).ToString());
                            p2 = Bitmap.CreateBitmap(p2, 0, 0, p2.Width, p2.Height, mtx, false);
                            mtx.Dispose();
                            mtx = null;
                            if (p2 != null)
                            {
                                if (p1 is ImageView)
                                {
                                    ((ImageView)p1).SetImageBitmap(p2);
                                }
                            }
                        }
                        p1.SetBackgroundDrawable(null);
                    }
                }
                catch (Exception)
                {


                }
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
        }

        private readonly LayoutInflater _inflater;
        private readonly List<CustomGallery> _data;
        private readonly ImageLoader _imageLoader;

        public GalleryAdapter(Context c, ImageLoader imageLoader)
        {

            _data = new List<CustomGallery>();
            _inflater = (LayoutInflater)c.GetSystemService(Context.LayoutInflaterService);

            _imageLoader = imageLoader;
            // clearCache();
        }

        public override int Count => _data.Count;

        public override CustomGallery this[int index] => _data[index];

        public override long GetItemId(int position)
        {
            return position;
        }

        public bool IsMultiplePick { get; set; }


        public void SelectAll(bool selection)
        {
            foreach (var t in _data)
            {
                t.IsSelected = selection;
            }
            NotifyDataSetChanged();
        }

        public bool AllSelected
        {
            get { return _data.All(x => x.IsSelected); }
        }

        public bool AnySelected
        {
            get { return _data.Any(x => x.IsSelected); }
        }

        public IEnumerable<CustomGallery> Selected
        {
            get
            {
                return
                    _data.
                        Where(x => x.IsSelected).
                        ToList();
            }
        }

        public void AddAll(IEnumerable<CustomGallery> files)
        {

            try
            {
                _data.Clear();
                _data.AddRange(files);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e.Message);
                throw;
            }

            NotifyDataSetChanged();
        }
        //Dictionary<int, string> saveState = new Dictionary<int, string>();
        List<TextView> lst = new List<TextView>();
        public void ChangeSelection(View v, int position, ref int num, bool isNonMax)
        {
            bool result = !_data[position].IsSelected;
            if (isNonMax || !result)
            {
                var hol = (ViewHolder)v.Tag;
                TextView txt = hol.ImgQueueMultiSelected;
                _data[position].IsSelected = result;
                if (result)
                {

                    num++;
                    // saveState.Add(position, num.ToString());
                    v.SetBackgroundResource(Resource.Color.landsoft);
                    txt.Visibility = ViewStates.Visible;
                    txt.Text = num.ToString();
                    lst.Add(txt);
                    _data[position].curentValue = txt.Text;
                }
                else
                {
                    if (!string.IsNullOrEmpty(txt.Text))
                    {
                        int getImage = int.Parse(txt.Text.ToString());
                        lst.Remove(txt);
                        // saveState.Remove(position);
                        num--;
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                            foreach (var ii in lst)
                            {
                                if (getImage < int.Parse(ii.Text))
                                {
                                    int temp = (int.Parse(ii.Text) - 1);
                                    if (temp <= 0)
                                        temp = 1;
                                    var find = _data.FirstOrDefault(p => !string.IsNullOrEmpty(p.curentValue) && p.curentValue.Equals(ii.Text));
                                    if (find != null)
                                        find.curentValue = temp.ToString();

                                    ii.Text = temp.ToString();
                                    //_data[(int)ii.Tag].curentValue = ii.Text;
                                }
                            }
                        });
                        _data[position].curentValue = null;
                        txt.Visibility = ViewStates.Gone;
                        v.SetBackgroundResource(0);
                    }

                }
            }



            //((ViewHolder) v.Tag).ImgQueueMultiSelected.Selected = _data[position].IsSelected;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            ViewHolder holder;
            if (convertView == null)
            {

                convertView = _inflater.Inflate(Resource.Layout.gallery_item_new, null);

                holder = new ViewHolder
                {
                    ImgQueue = (ImageView)convertView.FindViewById(Resource.Id.imgQueue),
                    ImgQueueMultiSelected = (TextView)convertView.FindViewById(Resource.Id.lblNumChossen)

                };

                holder.ImgQueueMultiSelected.Tag = position;

                // holder.ImgQueueMultiSelected.Visibility = (IsMultiplePick) ? ViewStates.Visible : ViewStates.Gone;

                convertView.Tag = holder;

            }
            else
            {
                holder = (ViewHolder)convertView.Tag;

            }

            holder.ImgQueue.Tag = position;

            try
            {
                //if (saveState.Count > 0)
                //{
                //    //if (!saveState.ContainsKey(position))
                //    //{
                //    //    holder.ImgQueueMultiSelected.Visibility = ViewStates.Gone;
                //    //    convertView.SetBackgroundResource(0);
                //    //}

                //    //else
                //    //{

                //    //    holder.ImgQueueMultiSelected.Visibility = ViewStates.Visible;
                //    //    holder.ImgQueueMultiSelected.Text = saveState[position];
                //    //    convertView.SetBackgroundResource(Resource.Color.landsoft);
                //    //}

                //}
                _imageLoader.DisplayImage(
                    "file://" + _data[position].SdCardPath,
                    holder.ImgQueue,
                    new SimpleImageLoadingListenerImpl(holder));

                if (IsMultiplePick)
                {
                    if (_data[position].IsSelected)
                    {
                        holder.ImgQueueMultiSelected.Visibility = ViewStates.Visible;
                        holder.ImgQueueMultiSelected.Text = _data[position].curentValue;
                        convertView.SetBackgroundResource(Resource.Color.landsoft);
                    }
                    else
                    {
                        holder.ImgQueueMultiSelected.Visibility = ViewStates.Gone;
                        convertView.SetBackgroundResource(0);
                    }
                    //holder.ImgQueueMultiSelected.Selected = _data[position].IsSelected;
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e.Message);
                throw;
            }

            return convertView;
        }
        //private void CheckImageRotate(string filename)
        //{
        //    try
        //    {
        //        Matrix mtx = new Matrix();
        //        ExifInterface exif = new ExifInterface(filename);
        //        string orientation = exif.GetAttribute(ExifInterface.TagOrientation);

        //        switch (orientation)
        //        {
        //            case "6": // portrait
        //                mtx.PreRotate(90);
        //                resizedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
        //                mtx.Dispose();
        //                mtx = null;
        //                break;
        //            case "1": // landscape
        //                break;
        //            default:
        //                mtx.PreRotate(90);
        //                resizedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
        //                mtx.Dispose();
        //                mtx = null;
        //                break;
        //        }

        //    }
        //    catch (Exception)
        //    {


        //    }
        //}

        public void ClearCache()
        {
            _imageLoader.ClearDiskCache();
            _imageLoader.ClearMemoryCache();
        }

        public void Clear()
        {
            _data.Clear();
            NotifyDataSetChanged();
        }






    }
}