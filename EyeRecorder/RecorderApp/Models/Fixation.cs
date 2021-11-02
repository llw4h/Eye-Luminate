using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderApp.Models
{
    public class Fixation 
    {
        public Fixation()
        {

        }

        public Fixation(double _centroidX, double _centroidY, int _timeStart, int _timeEnd, int _duration)
        {
            centroidX = _centroidX;
            centroidY = _centroidY;
            timeStart = _timeStart;
            timeEnd = _timeEnd;
            duration = _duration;
        }

        // [Index(x)] is header for csv file; replaceable by [Name("header_name")]
        [Index(0)]
        public double centroidX { get; set; }

        [Index(1)]
        public double centroidY { get; set; }

        [Index(2)]
        public int timeStart { get; set; }

        [Index(3)]
        public int timeEnd { get; set; }

        [Index(4)]
        public int duration { get; set; }

    }
}
