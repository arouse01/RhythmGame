import numpy as np
import pandas as pd
import os
import datetime as dt
import math


# def find_nearest_in_array(array, values):
#
#     indices = np.searchsorted(array, values, side="left")
#
#     # Correct indices where necessary
#
#     if len(indices) > 0:  # if only one value in array
#         mask = (indices > 0) &
#         indices[mask] = np.where(
#             math.fabs(values[mask] - array[indices[mask] - 1]) < math.fabs(array[indices[mask]] - values[mask]),
#             indices[mask] - 1, indices[mask])
#
#         return array[indices]
#     else:
#         return array.iat[0]


def find_nearest(xx, yy):
    # For non-isochronous stimuli, just return idx and mask, and those will tell you which element is the closest and
    # whether the second closest is before or after
    xx = np.asarray(xx)
    yy = np.asarray(yy)
    idx = np.searchsorted(xx, yy, side="left").clip(max=xx.size - 1)
    mask = (idx > 0) & \
           ((idx == len(xx)) | (np.fabs(yy - xx[idx - 1]) < np.fabs(yy - xx[idx])))
    return xx[idx - mask]


# analyze all
# get all session data files
rawFolder = 'D:\\Rouse\\My Documents\\GitHub\\RhythmGame\\Assets\\SessionData'
outputFolder = 'D:\\Rouse\\My Documents\\GitHub\\RhythmGame\\AnalysisCode\\GameAnalysis\\Output'

# get list of folders in data folder, which is list of subjects
# https://stackoverflow.com/questions/7781545/how-to-get-all-folder-only-in-a-given-path-in-python
subjectList = next(os.walk(rawFolder))[1]
for subject in subjectList:
    subjectFolder = os.path.join(rawFolder, subject)

    # get subject info
    sessionList = list(filter(lambda s: s.endswith('.txt'), next(os.walk(subjectFolder))[2]))

    for currFile in sessionList:
        currPath = os.path.join(subjectFolder, currFile)
        currData = pd.read_table(currPath,
                                 header=None,
                                 names=['Time', 'Type', 'Message', 'Value'],
                                 # parse_dates=[1],  # Which columns to parse as dates/datetimes
                                 # date_format='%Y-%m-%d %H:%M:%S.%f',  # automatically infer datetime formatting
                                 dtype={
                                     'Time': float,
                                     'Type': str,
                                     'Message': str,
                                     'Value': str

                                 }  # manually assign dtypes to each column
                                 )
        currDate = dt.datetime.strptime(currFile[-18:-4], "%Y%m%d%H%M%S")
        # currData['Timestamp'] = pd.to_datetime(currData['Timestamp'])
        currData.insert(0, column='File', value=currFile)
        currData.insert(1, column='Subject', value=subject)
        currData.insert(3, column='Date', value=currDate)
        # currData.insert(4, column='Time', value=currData['Timestamp'].dt.time)

        # add trial number
        trialNumbers = []
        currTrialNum = -1
        for rowIndex, row in currData.iterrows():
            if ("Trial" in row['Type']) & ('started' in row['Message']):
                currTrialNum += 1
            trialNumbers.append(currTrialNum)

        currData['Trial'] = trialNumbers

        # # calculate event times based on trial start
        trialStartTimes = currData.groupby(['Trial'])['Time'].min()
        currData['TrialStart'] = currData['Trial'].map(trialStartTimes)
        currData['TrialTime'] = (currData['Time'] - currData['TrialStart'])

        currSessionData = currData[currData['Type'].str.match('Trial Param')]
        currTickTimes = currData[currData['Message'].str.match('Beat tick')]

        # double check that all ticks are present - number of ticks should equal number of Beat Zone Starts
        currBeatZoneTimes = currData[currData['Message'].str.match('Beat zone start')]
        if len(currTickTimes) < len(currBeatZoneTimes):
            print("Warning, possible tick(s) missing: {} ticks vs {} beat zones".format(len(currTickTimes), len(currBeatZoneTimes)))
            # assume beats are equally spaced, interpolate

        currTapTimes = currData[currData['Type'].str.match('Response')]

        # calculate angles for each tap
        trialCount = currData['Trial'].max()+1
        angleColumn = []
        for i in range(trialCount):
            # break into trials
            currTrial = currData[currData['Trial'] == i]
            tempTapTimes = currTrial[currTrial['Type'].str.match('Response')]
            tempTickTimes = currTrial[currTrial['Message'].str.match('Beat tick')]['TrialTime']
            avgIOI = np.diff(tempTickTimes).mean()  # THIS ASSUMES STIMULUS ISOCHRONY
            if len(tempTapTimes) > 0:
                if np.isnan(avgIOI):
                    angles = np.empty((len(tempTapTimes), 1))
                    angles[:] = np.nan
                else:
                    nearestTicks = find_nearest(tempTickTimes, tempTapTimes['TrialTime'])
                    angles = 2*np.pi*(tempTapTimes['TrialTime'] - nearestTicks)/avgIOI
                angleColumn.extend(angles)
        currTapTimes['Angle'] = angleColumn
        # LRS makes angles slightly more complicated as the stimulus is no longer isochronous
        # break into trials

        # get session-level data

        # what session-level data to store? Or keep session-level data in separate file for lookup?
        # session level data to get/calc: session number, phase number
        #
        currData.head(5)

        # # add current data to total dataset
        # self.allData = pd.concat([self.allData, currData], ignore_index=True)

# extract beat times
# extract tap times
# calculate phases
# export phases in table format with all data
# session ID, Date, Session #, Subject ID, Phase, Trial #, Time, Tap #, Correct, Phase angle

# analyze
# import phase data files
# calculate circular stats per trial
# concatenate sessions and trials per subject
# export one table per subject

# group
# analyze
