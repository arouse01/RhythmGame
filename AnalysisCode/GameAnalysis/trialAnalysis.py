import numpy as np
import pandas as pd
import os
import datetime as dt
import math


def circ_mean(angles):
    mean_sine = np.mean(np.sin(angles))
    mean_cosine = np.mean(np.cos(angles))
    circular_mean_radians = np.arctan2(mean_sine, mean_cosine)
    return circular_mean_radians


outputFolder = 'D:\\Rouse\\My Documents\\GitHub\\RhythmGame\\AnalysisCode\\GameAnalysis\\Output'
outputFile = 'D:\\Rouse\\My Documents\\GitHub\\RhythmGame\\AnalysisCode\\GameAnalysis\\Output\\allTapData.csv'
allData = pd.read_table(outputFile,
                        header=0,
                        sep=','
                        # parse_dates=[1],  # Which columns to parse as dates/datetimes
                        # date_format='%Y-%m-%d %H:%M:%S.%f',  # automatically infer datetime formatting
                        # dtype={
                        #     'index': int,
                        #     'File': str,
                        #     'Subject': str,
                        #     'Time': float,
                        #     'Date': str,
                        #     'Message': str,
                        #     'SessionNum': int,
                        #     'PhaseNum': int,
                        #     'Trial': int,
                        #     'TrialStart': float,
                        #     'TrialTime': float,
                        #     'RawTrialTime': float,
                        #     'Closest Ticks': float,
                        #     'Angle': float
                        # }  # manually assign dtypes to each column
                        )
allData['Closest Ticks'] = allData['Closest Ticks'].str.replace(r'[][]', '', regex=True)
allData['Closest Ticks'] = allData['Closest Ticks'].replace('nan', np.nan)
allData['Closest Ticks'] = allData['Closest Ticks'].replace('0.', np.nan).astype(float)
# allData['Closest Ticks'] = allData['Closest Ticks'].replace('[1.4998791]', np.nan)
allData['Angle'] = allData['Angle'].replace('[nan]', np.nan).astype(float)

subjectList = allData['Subject'].unique()

for subject in subjectList:
    currSubData = allData[allData['Subject'].str.match(subject)]
    sessionList = currSubData['SessionNum'].unique()
    for currSession in sessionList:
        currSessionData = currSubData[currSubData['SessionNum'] == currSession]
        trialList = currSubData['Trial'].unique()
        for currTrial in trialList:
            currTrialData = currSessionData[currSessionData['Trial'] == currTrial]
            # get angle and vector of trial

outputData = allData.groupby(["Subject", "PhaseNum"])['Angle'].apply(circ_mean)

outputData.to_csv(os.path.join(outputFolder, 'trialSummaryData.csv'))

