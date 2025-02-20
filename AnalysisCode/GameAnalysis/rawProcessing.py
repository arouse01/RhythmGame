import numpy as np
import pandas as pd
import os
import datetime as dt


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
                                 names=['Timestamp', 'Type', 'Message'],
                                 parse_dates=[1],  # Which columns to parse as dates/datetimes
                                 date_format='%Y-%m-%d %H:%M:%S.%f',  # automatically infer datetime formatting
                                 dtype={
                                     'Timestamp': str,  # groupby
                                     'Type': str,  # groupby
                                     'Message': str  # groupby

                                 }  # manually assign dtypes to each column
                                 )
        currData.insert(0, column='File', value=currFile)
        currData.insert(1, column='Subject', value=subject)

        currDataUser = currData[currData['Type'].str.match('UserInputObject') |
                                currData['Type'].str.match('Session')]
        # get session number
        # get phase number
        # break into trials
        # get trial start time
        # get trial end time (or last time if no "trial end" event)
        # what session-level data to store? Or keep session-level data in separate file for lookup?
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
