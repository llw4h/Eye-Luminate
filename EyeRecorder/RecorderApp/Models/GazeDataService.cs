using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderApp.Models
{
    public class GazeDataService
    {
        private static List<GazeData> GazeDataList;

        public GazeDataService()
        {

        }

        public List<GazeData> GetAll()
        {
            return GazeDataList;
        }

        public bool Add(GazeData newData)
        {
            GazeDataList.Add(newData);
            return true;
        }
    }
}
