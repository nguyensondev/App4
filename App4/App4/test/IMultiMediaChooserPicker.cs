using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App4.test
{
    public interface IMultiMediaChooserPicker
    {
        Task<List<FileData>> PickMultiImage(bool isMulti, int maxImage = 5); // true la chon nhieu tam hinh va nguoc lai
    }

    public class FileData
    {
        public ImageSource Source { get; set; }
        public byte[] DataImage { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
