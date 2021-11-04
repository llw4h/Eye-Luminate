using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderApp.Models
{
    public class ImageBtns
    {
        public ImageBtns()
        {

        }
        public ImageBtns(string _imgPath, string _imgValue)
        {
            imgPath = _imgPath;
            imgValue = _imgValue;
        }

        public string imgPath { get; set; }

        public string imgValue { get; set; }

    }
}
