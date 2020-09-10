import numpy as np

import random

debug = False

n_rows = 10
n_columns = 10
n_columns_break = 5
n_rows_break = 5
perc_empty = 0.8

if not debug:
    n_rows = int(input("Number of rows: "))
    n_columns = int(input("Number of columns: "))
    n_rows_break = int(input("Every n seats add an empty row: "))
    n_columns_break = int(input("Every n seats add an empty column: "))
    perc_empty = float(input("Percentage of empty spaces: "))

cinema = np.full((n_rows, n_columns), 1)

for i in range(n_rows):
    if((i + 1) % n_rows_break) == 0:
        for j in range(n_columns):        
            cinema[i,j] = 0

for i in range (n_columns):
    if((i + 1) % n_columns_break) == 0:
        for j in range (n_rows):
            cinema[j,i] = 0

for i in range(n_rows):
    for j in range(n_columns):
        if cinema[i,j] == 1:
            temp = random.random()
            if(temp < perc_empty):
                cinema[i,j] = 0

def pretty_print(cinema):
    res = ""
    for i in range(len(cinema)):
        for j in range(len(cinema[i])):
            res += str(cinema[i,j])
        res += "\n"
    print(res)

pretty_print(cinema)