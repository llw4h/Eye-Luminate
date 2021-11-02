import cv2
import os
import time
import csv
import sys
import re

class Fixation:
    
    def __init__(self, centroidX, centroidY, time_start, time_end, duration):
        self.centroidX = centroidX
        self.centroidY = centroidY
        self.time_start = time_start
        self.time_end = time_end
        self.duration = duration

#Generate directory path
def makeDir(count):
    timestr = time.strftime("%Y%m%d-%H%M%S")
    folderpath = "./Results/" + str(timestr) + "-" + str(count)
    if not os.path.exists(folderpath):
        os.makedirs(folderpath)
    if not os.path.exists(folderpath):
        os.makedirs(folderpath)
    print ("Saving frames to " + folderpath)

    return folderpath



def readFile(filename):
    readList = []
    with open(filename) as csv_file:
        reader = csv.reader(csv_file, delimiter=',')

        next(reader, None) #skip header lines

        for centroid_x, centroid_y, timeStart, timeEnd, duration in reader:
            readList.append(Fixation(centroid_x, centroid_y, int(timeStart), int(timeEnd), int(duration)))
    
    return readList

def writeFile(filename, data):
    print("Making file '", filename, "'...")

    with open(filename, mode='w', newline='') as validGazeData:
        header = ['Centroid X', 'Centroid Y', 'Time Start', 'Time End', 'Duration']
        writer = csv.DictWriter(validGazeData, fieldnames=header)

        writer.writeheader()
        for x in data: 
            writer.writerow({'Centroid X': str(x.centroidX), 'Centroid Y': str(x.centroidY), 'Time Start': str(x.time_start), 
            'Time End': str(x.time_end), 'Duration': str(x.duration)})   
            
    print("Created file '", filename, "'...")

# * sorts data according to fixation duration, descending order
def sortByDuration(data):
    data.sort(key=lambda x: x.duration, reverse=True)
    return data

# * gets highest n from list of data where n is # set by user
def getHighestN(data, n):
    return data[:n] 

# * sorts selected scenes on chronological order before frames are extracted 
def sortByTimeStart(data):
    data.sort(key=lambda x: x.time_start, reverse=False)
    return data

#for testing purposes only
def printData(data):
    for x in data:
        print(x.centroidX, "start: ", x.time_start)


def getFrameSets(vidPath, time_start, time_end, folderpath):
    cap = cv2.VideoCapture(vidPath)
    fps = cap.get(cv2.CAP_PROP_FPS)
    
    timestamps = [cap.get(cv2.CAP_PROP_POS_MSEC)]
    calc_timestamps = [0.0]
    count = 0

    while(cap.isOpened()):
        frame_exists, curr_frame = cap.read()
        ts = cap.get(cv2.CAP_PROP_POS_MSEC)

        if frame_exists:
            timestamps.append(cap.get(cv2.CAP_PROP_POS_MSEC))
            calc_timestamps.append(calc_timestamps[-1] + 1000/fps)

            if ts >= time_start and ts <= time_end:
                cv2.imwrite(os.path.join(folderpath, "frame%d.jpg" % count), curr_frame)
                count+=1
        else:
            break

    cap.release()


def selectScenes(data, vidPath):
    count = 1
    for x in data:
        folderpath = makeDir(count)
        getFrameSets(vidPath, x.time_start, x.time_end, folderpath)

        createClip(folderpath)
        print("Done", count, "out of", len(data))
        count += 1

#region sort filename with int/float properly
def atof(text):
    try:
        retval = float(text)
    except ValueError:
        retval = text
    return retval

def natural_keys(text):
    return [ atof(c) for c in re.split(r'[+-]?([0-9]+(?:[.][0-9]*)?|[.][0-9]+)', text) ]

#endregion

def createClip(folderpath):
    x = folderpath.rfind('/')
    video_name = folderpath[x+1:] + ".avi"

    each_image_duration = 5 # in secs
    fourcc = cv2.VideoWriter_fourcc(*'XVID') # define the video codec

    #sort filenames with float value
    images = [img for img in os.listdir(folderpath) if img.endswith(".jpg")]
    images.sort(key=natural_keys)

    img_count = len(images)
    video_secs = 1
    frame = cv2.imread(os.path.join(folderpath, images[0]))
    height, width, layers = frame.shape

    #video = cv2.VideoWriter(video_name, fourcc, 30.0, (width, height))
    video = cv2.VideoWriter(video_name, fourcc, float(img_count/video_secs), (width, height))
    print("Creating clip from frames...")
    for image in images:
        for _ in range(each_image_duration):
            video.write(cv2.imread(os.path.join(folderpath, image)))

    print("Created Clip", video_name)
    cv2.destroyAllWindows()
    video.release()


if __name__ == "__main__":
    filename = sys.argv[1]
    n = int(sys.argv[2])
    vidPath = sys.argv[3]
    
    fixData = readFile(filename)

    sortedData = sortByDuration(fixData)
    writeFile('sorted.csv', sortedData)

    highestN = getHighestN(sortedData, n)
    #writeFile('highestN.csv', highestN)

    chronData = sortByTimeStart(highestN)

    selectScenes(chronData, vidPath)