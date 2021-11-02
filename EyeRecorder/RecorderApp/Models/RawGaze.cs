using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderApp.Models
{
    public class RawGaze
    {
        public RawGaze()
        {

        }

        public RawGaze(int _gazeX, int _gazeY, int _time)
        {
            gazeX = _gazeX;
            gazeY = _gazeY;
            time = _time;
        }

        // [Index(x)] is header for csv file; replaceable by [Name("header_name")]
        [Index(0)]
        public int gazeX { get; set; }

        [Index(1)]
        public int gazeY { get; set; }

        [Index(2)]
        public int time { get; set; }
    }
}
