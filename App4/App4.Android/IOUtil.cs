using System;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Webkit;
using Java.IO;
using Android.Media;
using Android.Graphics;
using System.ComponentModel;

namespace App4.Droid
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public class IOUtil
    {
        public static string GetPath(Context context, Android.Net.Uri uri)
        {
            var isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

            // DocumentProvider
            if (isKitKat && DocumentsContract.IsDocumentUri(context, uri))
            {
                // ExternalStorageProvider
                if (IsExternalStorageDocument(uri))
                {
                    var docId = DocumentsContract.GetDocumentId(uri);
                    var split = docId.Split(':');
                    var type = split[0];

                    if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                    }

                    // TODO handle non-primary volumes
                }
                // DownloadsProvider
                else if (IsDownloadsDocument(uri))
                {

                    var id = DocumentsContract.GetDocumentId(uri);
                    var contentUri = ContentUris.WithAppendedId(
                            Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    return GetDataColumn(context, contentUri, null, null);
                }
                // MediaProvider
                else if (IsMediaDocument(uri))
                {
                    var docId = DocumentsContract.GetDocumentId(uri);
                    var split = docId.Split(':');
                    var type = split[0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    var selection = "_id=?";
                    var selectionArgs = new[] {
                    split[1]
            };

                    return GetDataColumn(context, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            else if ("content".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return GetDataColumn(context, uri, null, null);
            }
            // File
            else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }

        public static string GetDataColumn(Context context, Android.Net.Uri uri, string selection,
        string[] selectionArgs)
        {

            ICursor cursor = null;
            const string column = "_data";
            string[] projection = {
                column
            };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs,
                        null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    var columnIndex = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(columnIndex);
                }
            }
            finally
            {
                cursor?.Close();
            }
            return null;
        }

        /**
 * @param uri The Uri to check.
 * @return Whether the Uri authority is ExternalStorageProvider.
 */
        public static byte[] ResizeImageNew(byte[] imageData, int width, int height, string name)
        {

            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options
            {
                InPurgeable = true,
                InJustDecodeBounds = true
            };
            BitmapFactory.DecodeFile(name, options);
            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }
            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length, options);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                resizedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                return ms.ToArray();
            }
        }
        public static byte[] ResizeImageBitmap(Bitmap imageData, int width, int height, string name)
        {

            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options
            {
                InPurgeable = true,
                InJustDecodeBounds = true
            };
            BitmapFactory.DecodeFile(name, options);
            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }
            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = Bitmap.CreateScaledBitmap(imageData, inSampleSize, inSampleSize, true);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                resizedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                return ms.ToArray();
            }
        }
        public static bool IsExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is DownloadsProvider.
         */
        public static bool IsDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is MediaProvider.
         */
        public static bool IsMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        public static byte[] ReadFileFromPath(string file)
        {
            try
            {
                return ReadFileFromPath(new File(file));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return null;
            }
        }



        public static byte[] ReadFileFromPath(File file)
        {
            // Open file
            var f = new RandomAccessFile(file, "r");
            try
            {
                // Get and check length
                var longlength = f.Length();
                var length = (int)longlength;
                if (length != longlength)
                    throw new IOException("Tamanho do arquivo excede o permitido!");
                // Read file and return data
                var data = new byte[length];
                f.ReadFully(data);
                return data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return null;
            }
            finally
            {
                f.Close();
                f.Dispose();
            }
        }

        public static string GetMimeType(string url)
        {
            string type = null;
            var extension = MimeTypeMap.GetFileExtensionFromUrl(url);

            if (extension != null)
            {
                type = MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);
            }

            return type;
        }
    }
}