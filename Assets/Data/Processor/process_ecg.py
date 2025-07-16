# === process_ecg.py ===
# This script generates ecg_phases{record_num}.json and ecg_plot{record_num}.json
# Requires: wfdb, heartpy, numpy, scipy

import sys
import wfdb
import numpy as np
import json
import heartpy as hp
from scipy import signal as sp_signal
import os

# === Read patient record number from command line ===
record_num = int(sys.argv[1])

# === Paths ===
base_data_path = r"C:/Users/Kiran/Desktop/PESticide/HEADACHE/CAVE Labs/project/unity/Heart_Simulation/Assets/Data"
dataset_path = os.path.join(base_data_path, "mit-bih-arrhythmia-database-1.0.0/mit-bih-arrhythmia-database-1.0.0")
record_path = os.path.join(dataset_path, str(record_num))

# === Load ECG ===
record = wfdb.rdrecord(record_path)
ecg_signal = record.p_signal[:, 0]
fs = record.fs

# === FIR Bandpass Filter (3-45 Hz) ===
def fir_bandpass_filter(ecg_signal, fs, lowcut=3.0, highcut=45.0, numtaps=101):
    nyquist = 0.5 * fs
    low = lowcut / nyquist
    high = highcut / nyquist
    taps = sp_signal.firwin(numtaps, [low, high], pass_zero=False)
    filtered_ecg = sp_signal.lfilter(taps, 1.0, ecg_signal)
    return filtered_ecg

filtered_ecg = fir_bandpass_filter(ecg_signal, fs)

# === R-peak detection using HeartPy ===
def detect_r_peaks(filtered_ecg, fs):
    wd, m = hp.process(filtered_ecg, sample_rate=fs)
    r_peaks = np.array(wd['peaklist'])
    return r_peaks

r_peaks = detect_r_peaks(filtered_ecg, fs)

# === Detect PQRST points ===
def detect_pqrst_points(filtered_ecg, r_peaks, fs):
    q_peaks, s_peaks, p_peaks, t_peaks = [], [], [], []

    for r_peak in r_peaks:
        q_window_start = max(0, r_peak - int(0.08 * fs))
        q_window = filtered_ecg[q_window_start:r_peak]
        q_peak = q_window_start + np.argmin(q_window) if len(q_window) > 0 else None
        q_peaks.append(q_peak)

        s_window_end = min(len(filtered_ecg), r_peak + int(0.08 * fs))
        s_window = filtered_ecg[r_peak:s_window_end]
        s_peak = r_peak + np.argmin(s_window) if len(s_window) > 0 else None
        s_peaks.append(s_peak)

        if q_peak:
            p_window_start = max(0, q_peak - int(0.2 * fs))
            p_window = filtered_ecg[p_window_start:q_peak]
            p_peak = p_window_start + np.argmax(p_window) if len(p_window) > 0 else None
            p_peaks.append(p_peak)

        if s_peak:
            t_window_end = min(len(filtered_ecg), s_peak + int(0.4 * fs))
            t_window = filtered_ecg[s_peak:t_window_end]
            t_peak = s_peak + np.argmax(t_window) if len(t_window) > 0 else None
            t_peaks.append(t_peak)

    return {
        'ECG_R_Peaks': r_peaks,
        'ECG_Q_Peaks': np.array(q_peaks),
        'ECG_S_Peaks': np.array(s_peaks),
        'ECG_P_Peaks': np.array(p_peaks),
        'ECG_T_Peaks': np.array(t_peaks)
    }

info = detect_pqrst_points(filtered_ecg, r_peaks, fs)

# === Create output directory if not exists ===
os.makedirs(base_data_path, exist_ok=True)

# === Save Plot JSON ===
ecg_segment = filtered_ecg[:fs*60]  # First 60 seconds only
plot_path = os.path.join(base_data_path, f'ecg_plot{record_num}.json')
with open(plot_path, 'w') as f:
    json.dump(ecg_segment.tolist(), f)

# === Save PQRST intervals ===
pqrst_intervals = []
peaks = {
    wave: [idx / fs for idx in info.get(f'ECG_{wave}_Peaks', []) if idx is not None]
    for wave in ['P', 'Q', 'R', 'S', 'T']
}
for i in range(min(len(peaks['P']), len(peaks['Q']), len(peaks['S']), len(peaks['T']))):
    try:
        pqrst_intervals.append({"entry": peaks['P'][i], "duration": peaks['Q'][i] - peaks['P'][i], "phase": "PQ"})
        pqrst_intervals.append({"entry": peaks['Q'][i], "duration": peaks['S'][i] - peaks['Q'][i], "phase": "QRS"})
        pqrst_intervals.append({"entry": peaks['S'][i], "duration": peaks['T'][i] - peaks['S'][i], "phase": "ST"})
    except:
        continue

phases_path = os.path.join(base_data_path, f'ecg_phases{record_num}.json')
with open(phases_path, 'w') as f:
    json.dump(pqrst_intervals, f, indent=2)

print(f"✅ Generated:\n - {plot_path}\n - {phases_path}")
