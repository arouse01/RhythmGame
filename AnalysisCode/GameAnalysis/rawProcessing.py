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


def find_nearest_iso(tickTimes, tapTimes):
    # For non-isochronous stimuli, just return idx and mask, and those will tell you which element is the closest and
    # whether the second closest is before or after
    tickTimes = np.asarray(tickTimes)
    tapTimes = np.asarray(tapTimes)
    idx = np.searchsorted(tickTimes, tapTimes, side="left").clip(max=tickTimes.size - 1)
    mask = (idx > 0) & \
           ((idx == len(tickTimes)) | (np.fabs(tapTimes - tickTimes[idx - 1]) < np.fabs(tapTimes - tickTimes[idx])))
    return tickTimes[idx - mask]


def get_angles(tickTimes, tapTimes):
    # For non-isochronous stimuli, just return idx and mask, and those will tell you which element is the closest and
    # whether the second closest is before or after
    tickTimes = np.asarray(tickTimes)
    tapTimes = np.asarray(tapTimes)

    # np.searchsorted effectively returns the index of the next tick, so idx-1 is the tick before
    idx = np.searchsorted(tickTimes, tapTimes, side="left").clip(max=tickTimes.size - 1)

    # If tap is closer to previous tick then shift index by -1
    mask = (idx > 0) & \
           ((idx == len(tickTimes)) | (np.fabs(tapTimes - tickTimes[idx - 1]) < np.fabs(tapTimes - tickTimes[idx])))
    nearestTicks = tickTimes[idx - mask]

    IOIlist = np.diff(tickTimes)
    # assume interval before first tick is the same as the first actual interval
    IOIlist = np.insert(IOIlist, 0, IOIlist[0])
    phaseAngles = 2 * np.pi * (tapTimes - nearestTicks) / IOIlist[idx]

    # are there any taps after the last tick?
    anglesAfter = np.abs(phaseAngles[-1]) > np.pi
    if anglesAfter:
        # add a phantom tick with the same interval as the last one
        tickTimes = np.append(tickTimes, tickTimes[-1]+IOIlist[-1])
        # and rerun the analysis
        nearestTicks, phaseAngles = get_angles(tickTimes, tapTimes)
    return nearestTicks, phaseAngles


# analyze all
# get all session data files
rawFolder = 'D:\\Rouse\\My Documents\\GitHub\\RhythmGame\\Builds\\Build_2_5_2_Win\\Rhythm Game_Data\\SessionData'
outputFolder = 'D:\\Rouse\\My Documents\\GitHub\\RhythmGame\\AnalysisCode\\GameAnalysis\\Output'

allData = pd.DataFrame()
allTaps = pd.DataFrame()

print("Analyzing...")

# get list of folders in data folder, which is list of subjects
# https://stackoverflow.com/questions/7781545/how-to-get-all-folder-only-in-a-given-path-in-python
subjectList = next(os.walk(rawFolder))[1]
for subject in subjectList:
    print("  Subject: {}".format(subject))
    subjectFolder = os.path.join(rawFolder, subject)

    # get subject info
    sessionList = list(filter(lambda s: s.endswith('.txt'), next(os.walk(subjectFolder))[2]))
    # reorder files by date so session numbering is accurate
    dfSession = pd.DataFrame({'Filename': sessionList})
    sessionTS = dfSession['Filename'].str.slice(start=-18, stop=-4)
    dfSession['TS'] = pd.to_datetime(sessionTS)
    dfSession = dfSession.sort_values(by='TS')

    sessionNumber = 0

    for fileIndex, fileRow in dfSession.iterrows():
        sessionNumber += 1
        currFile = fileRow['Filename']
        print("    File: {}".format(currFile))

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
        currDate = fileRow['TS'].date()
        # currData['Timestamp'] = pd.to_datetime(currData['Timestamp'])
        currData.insert(0, column='File', value=currFile)
        currData.insert(1, column='Subject', value=subject)
        currData.insert(3, column='Date', value=currDate)
        # currData['Date'] = currData['Date'].dt.date
        currData['SessionNum'] = sessionNumber
        # get phase number
        currPhase = currData[currData['Type'].str.match('Session') & currData['Message'].str.match('Phase')]['Value'].min()
        currData['PhaseNum'] = currPhase
        # currData.insert(4, column='Time', value=currData['Timestamp'].dt.time)

        # # add trial number
        # vectorized function, faster than iterrows()
        currData['Trial'] = currData['Type'].str.contains('Trial') & currData['Message'].str.contains('started')
        currData['Trial'] = currData['Trial'].cumsum() - 1
        # trialNumbers = []
        # currTrialNum = -1
        # for rowIndex, row in currData.iterrows():
        #     if ("Trial" in row['Type']) & ('started' in row['Message']):
        #         currTrialNum += 1
        #     trialNumbers.append(currTrialNum)
        # currData['Trial'] = trialNumbers

        # # calculate event times based on trial start
        trialStartTimes = currData.groupby(['Trial'])['Time'].min()
        currData['TrialStart'] = currData['Trial'].map(trialStartTimes)
        currData['TrialTime'] = (currData['Time'] - currData['TrialStart'])
        currData['RawTrialTime'] = currData['TrialTime']  # copy of trial time because TrialTime will be modified by LRS

        # # remove LRS times - create column for running total of LRS time, subtract from trial time
        # mark events for LRS start and LRS end
        currData['LRS_start'] = currData['Type'].str.contains('Feedback') & currData['Message'].str.contains(
            'initiated')
        currData['LRS_end'] = currData['Type'].str.contains('Feedback') & currData['Message'].str.contains(
            'ended')
        # cumsum both, get min time for each group in each column
        currData['LRS_start_group'] = currData.groupby('Trial')['LRS_start'].cumsum()
        currData['LRS_end_group'] = currData.groupby('Trial')['LRS_end'].cumsum()
        # get min time of LRS_start_group and LRS_end_group for each group in both columns
        LRSstart = currData.groupby(['Trial', 'LRS_start_group'])['TrialTime'].min().reset_index()
        currData = currData.merge(LRSstart, on=['Trial', 'LRS_start_group'], how="left", suffixes=("", "_LRS_start"))
        LRSend = currData.groupby(['Trial', 'LRS_end_group'])['TrialTime'].min().reset_index()
        currData = currData.merge(LRSend, on=['Trial', 'LRS_end_group'], how="left", suffixes=("", "_LRS_end"))
        # LRS_diff = group_n_end - group_n_start
        currData["LRS_diff"] = currData['LRS_end']*(currData['TrialTime_LRS_end'] - currData['TrialTime_LRS_start'])
        # LRSoffset = LRS_diff where LRS ends
        currData["LRSoffset"] = currData.groupby('Trial')["LRS_diff"].cumsum()
        # drop temp columns
        currData.drop(columns=["LRS_start", "LRS_end", "LRS_start_group", "LRS_end_group",
                               "TrialTime_LRS_end", "TrialTime_LRS_start", "LRS_diff"],
                      inplace=True)
        currData['TrialTime'] = currData['TrialTime'] - currData['LRSoffset']

        # get LRS start times and LRS end times
        # should never be more end times than start times
        # at end of each LRS, put duration of that LRS, then cumsum that column
        # dfLRS = currData['TrialTime']
        #
        # LRSoffset = []
        # LRSstart = -1
        # offset = 0
        # for rowIndex, row in currData.iterrows():
        #     if ("Feedback" in row['Type']) & ('initiated' in row['Message']):
        #         LRSstart = row['TrialTime']
        #     if ("Feedback" in row['Type']) & ('ended' in row['Message']) & LRSstart > 0:
        #         LRSend = row['TrialTime']
        #         LRStime = LRSend - LRSstart
        #         offset = offset + LRStime
        #         LRSstart = -1  # reset LRSstart to make sure the end time always has a matched start time
        #     LRSoffset.append(offset)
        # currData['LRSoffset'] = LRSoffset
        # currData['TrialTime'] = currData['TrialTime'] - currData['LRSoffset']

        currTapTimes = currData[currData['Type'].str.match('Response')].copy()

        # calculate angles for each tap
        trialCount = currData['Trial'].max()+1
        angleColumn = []
        closeTickColumn = []
        for i in range(trialCount):
            # break into trials
            print("      Trial %i" % i)
            currTrial = currData[currData['Trial'] == i]
            tempTapTimes = currTrial[currTrial['Type'].str.match('Response')]['TrialTime']
            if len(tempTapTimes) > 0:
                # double check that all ticks are present - number of ticks should equal number of Beat Zone Starts
                # (or be within one in case where last tap is before tick)
                tempTickTimes = currTrial[currTrial['Message'].str.match('Beat tick')]['TrialTime']
                tempBeatZoneTimes = currTrial[currTrial['Message'].str.match('Beat zone start')]['TrialTime']

                if len(tempTickTimes) < len(tempBeatZoneTimes) - 1:
                    print("        Warning, possible ticks missing: {} ticks vs {} beat zones".format(
                        len(tempTickTimes), len(tempBeatZoneTimes)))

                # # Get tap angles

                # if len(tempTickTimes > 1):
                #     avgIOI = np.diff(tempTickTimes).mean()  # THIS ASSUMES STIMULUS ISOCHRONY
                # else:
                #     avgIOI = np.nan

                if len(tempTickTimes) < 2:
                    angles = np.empty((len(tempTapTimes), 1))
                    angles[:] = np.nan
                    closeTicks = np.empty((len(tempTapTimes), 1))
                else:
                    [closeTicks, angles] = get_angles(tempTickTimes, tempTapTimes)

                closeTickColumn.extend(closeTicks)
                angleColumn.extend(angles)
        currTapTimes['Closest Ticks'] = closeTickColumn
        currTapTimes['Angle'] = angleColumn
        currTapTimes.drop(columns=["Type", "Value"], inplace=True)
        # what session-level data to store? Or keep session-level data in separate file for lookup?
        # session level data to get/calc: session number, phase number
        # get session-level data

        currSessionData = currData[currData['Type'].str.match('Trial Param')]

        # # add current data to total dataset
        allTaps = pd.concat([allTaps, currTapTimes], ignore_index=True)
        allData = pd.concat([allData, currData], ignore_index=True)


# export phases in table format with all data
allData.to_csv(os.path.join(outputFolder, 'allData.csv'))
allTaps.to_csv(os.path.join(outputFolder, 'allTapData.csv'))
print("Finished!")

