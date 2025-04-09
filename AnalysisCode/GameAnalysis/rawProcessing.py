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

    # are there any taps after the last tick that would technically be closer to the following tick?
    anglesAfter = np.abs(phaseAngles[-1]) > np.pi
    if anglesAfter:
        # add a phantom tick with the same interval as the last one
        tickTimes = np.append(tickTimes, tickTimes[-1]+IOIlist[-1])
        # and rerun the analysis
        nearestTicks, phaseAngles = get_angles(tickTimes, tapTimes)
    return nearestTicks, phaseAngles


# def get_angles_single(tickTime, tapTimes, wheelTempo, eventList):
#     # For non-isochronous stimuli, just return idx and mask, and those will tell you which element is the closest and
#     # whether the second closest is before or after
#     tickTimes = np.asarray(tickTimes)
#     tapTimes = np.asarray(tapTimes)
#
#     # np.searchsorted effectively returns the index of the next tick, so idx-1 is the tick before
#     idx = np.searchsorted(tickTimes, tapTimes, side="left").clip(max=tickTimes.size - 1)
#
#     # If tap is closer to previous tick then shift index by -1
#     mask = (idx > 0) & \
#            ((idx == len(tickTimes)) | (np.fabs(tapTimes - tickTimes[idx - 1]) < np.fabs(tapTimes - tickTimes[idx])))
#     nearestTicks = tickTimes[idx - mask]
#
#     IOIlist = np.diff(tickTimes)
#     # assume interval before first tick is the same as the first actual interval
#     IOIlist = np.insert(IOIlist, 0, IOIlist[0])
#     phaseAngles = 2 * np.pi * (tapTimes - nearestTicks) / IOIlist[idx]
#
#     # are there any taps after the last tick that would technically be closer to the following tick?
#     anglesAfter = np.abs(phaseAngles[-1]) > np.pi
#     if anglesAfter:
#         # add a phantom tick with the same interval as the last one
#         tickTimes = np.append(tickTimes, tickTimes[-1]+IOIlist[-1])
#         # and rerun the analysis
#         nearestTicks, phaseAngles = get_angles(tickTimes, tapTimes)
#     return nearestTicks, phaseAngles

def version_atleast(currVersion, targetVersion):
    # quick function to check if currVersion is targetVersion or later
    currVerArray = currVersion.split(".")
    targetVerArray = targetVersion.split(".")
    currVerArray = [int(x) for x in currVerArray]
    targetVerArray = [int(x) for x in targetVerArray]

    for y in range(len(currVerArray)):
        if currVerArray[y] < targetVerArray[y]:
            return False
    return True


# analyze all
# get all session data files
rawFolder = 'D:\\Rouse\\My Documents\\GitHub\\RhythmGame\\AnalysisCode\\GameAnalysis\\Input'
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

        # get current version (some things change between versions)
        gameVersion = currData[currData['Type'].str.match('Game') & currData['Message'].str.match('Version')]['Value']
        if len(gameVersion) == 0:
            # default to version number of 2.5.2 - version numbering was introduced in 2.5.3
            gameVersion = '2.5.2'
        else:
            gameVersion = gameVersion[0]
        # # add trial number
        # vectorized function, faster than iterrows()
        currData['Trial'] = currData['Type'].str.contains('Trial') & currData['Message'].str.contains('started')
        currData['Trial'] = currData['Trial'].cumsum() - 1
        # replace trial number for Trial Data events if game version is below 2.5.3
        if not version_atleast(gameVersion, '2.5.3'):
            # prior to 2.5.3, trial params were recorded before the trial start message
            currData['Trial'] = np.where(currData['Type'].str.contains('Trial Param'), currData['Trial']+1, currData['Trial'])
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

        currTapTimes = currData[currData['Type'].str.match('Response')].copy()

        # calculate angles for each tap
        trialCount = currData['Trial'].max() + 1
        angleColumn = []
        closeTickColumn = []
        if currPhase != 0:
            # phase 0 has no tempo so no phases can be calculated
            for i in range(trialCount):
                # break into trials
                print("      Trial %i" % i)
                currTrial = currData[currData['Trial'] == i]
                tempTapTimes = currTrial[currTrial['Type'].str.match('Response')]['TrialTime']
                if len(tempTapTimes) > 0:

                    # add next tick time to tick times if final tap occurred before tick onset (beatZone entered but
                    # no tick)

                    # To be a criterion tap, tap has to fall within the beatZone, which means beatZone start would
                    # be logged. If we have that, beatZone width, and the wheel tempo we can calculate where the
                    # beat would have been
                    # Unless we have the beatZone timing, we can't be 100% confident in the onset of the next beat
                    # because we can't be 100% sure we can match up the recorded beat times with the pattern for
                    # nonisochronous patterns - if a beat event isn't recorded we might assume the wrong IOI

                    tempTickTimes = currTrial[currTrial['Message'].str.match('Beat tick')]['TrialTime'].reset_index(drop=True)
                    tempBeatZoneTimes = currTrial[currTrial['Message'].str.match('Beat zone start')]['TrialTime'].reset_index(drop=True)
                    if len(tempBeatZoneTimes) == 0:
                        # fringe case where trial was cancelled before any beats were registered but subject still
                        # tapped
                        if len(tempTapTimes) > 1:
                            # multiple taps need an empty array for angles
                            angles = np.array([np.nan] * len(tempTapTimes))
                            # angles[:] = np.nan
                            closeTicks = np.array([np.nan] * len(tempTapTimes))
                        else:
                            # but a single tap only needs one value
                            angles = np.nan
                            closeTicks = np.nan
                    else:
                        # at least one beatZone occurred so we can calculate onset of beat, if needed
                        lastTickTime = 0 if len(tempTickTimes) == 0 else tempTickTimes.iloc[-1]  # if there are no ticks
                        lastBZtime = tempBeatZoneTimes.iloc[-1]
                        if lastBZtime > lastTickTime:
                            # if the last beatZone time is after the last tick time, we can assume the trial ended
                            # before the final tick was recorded
                            tempTrialParam = currTrial[currTrial['Type'].str.match('Trial Param')]
                            tempWheelSpeed = tempTrialParam[tempTrialParam['Message'].str.match('Wheel Tempo')]['Value']
                            tempWheelSpeed = float(tempWheelSpeed.item())
                            tempWheelSpeed = 360*tempWheelSpeed  # convert revolutions/sec to degrees/sec
                            tempBZsize = tempTrialParam[tempTrialParam['Message'].str.match('Beat Zone Size')]['Value']
                            tempBZsize = float(tempBZsize.item())  # bzSize is in degrees

                            # note this is not perfectly accurate because the physics engine updates at a fixed rate
                            # so the time between beatZone start and tick onset gets rounded
                            # We don't know width of beatMarker because it's scaled to screen, 0.01 is approx
                            # might be easier to figure out the actual beatZone to beat offsets and just use the
                            # average of those
                            nextBeatTime = lastBZtime + ((tempBZsize-0.01)/(2*tempWheelSpeed))
                            tempTickTimes.loc[tempTickTimes.index.max()+1] = nextBeatTime

                        # double check that all ticks are present - should equal number of Beat Zone Starts
                        if len(tempTickTimes) < len(tempBeatZoneTimes):
                            print("        Warning, possible ticks missing: {} ticks vs {} beat zones".format(
                                len(tempTickTimes), len(tempBeatZoneTimes)))

                        # # Now that we have ticks and taps, tet tap angles
                        # get_angles() assumes that interval before the first beat is the same as the first interval
                        if len(tempTickTimes) == 1:
                            # # skipping the following because in theory one tick is not enough time to get a good
                            # sense of beat, so returning NA.
                            # 1 tick means there's not enough information to get actual intervals, but we can
                            # calculate onset of next beat
                            #
                            # # Make get_angles_single() that accepts tap time(s), single tick time, and tempo,
                            # # just returns angles
                            # #
                            # # Technically we could get it from the wheel tempo and beat pattern, but we have to
                            # assume the current beat time is for the first beat in the sequence
                            # tempTrialParam = currTrial[currTrial['Type'].str.match('Trial Param')]
                            # tempTempo = tempTrialParam[tempTrialParam['Message'].str.match('Wheel Tempo')]['Value']
                            # tempTempo = float(tempTempo.item())
                            # tempPattern = tempTrialParam[tempTrialParam['Message'].str.match('Event List')]['Value']
                            # tempPattern = [int(x) for x in tempPattern.item().split(", ")]
                            # # parse event list to angles, then convert angles to IOIs
                            if len(tempTapTimes) > 1:
                                # multiple taps need an empty array for angles
                                angles = np.array([np.nan]*len(tempTapTimes))
                                # angles[:] = np.nan
                                closeTicks = np.array([np.nan]*len(tempTapTimes))
                            else:
                                # but a single tap only needs one value
                                angles = np.nan
                                closeTicks = np.nan

                        else:
                            [closeTicks, angles] = get_angles(tempTickTimes, tempTapTimes)

                    # np.ravel is required because sometimes closeTicks is ndarray and sometimes a single float
                    # extend() only works on iterables, and append() adds as a single item
                    closeTickColumn.extend(np.ravel(closeTicks))
                    angleColumn.extend(np.ravel(angles))

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

