import numpy as np

import random

debug = False

n_rows = 10
n_columns = 10
n_columns_break = 5
n_rows_break = 5
perc_empty = 0.2
perc_seats_occupied = 0.3
online = True

if not debug:
    n_rows = int(input("Number of rows: "))
    n_columns = int(input("Number of columns: "))
    n_rows_break = int(input("Every n seats add an empty row: "))
    n_columns_break = int(input("Every n seats add an empty column: "))
    perc_empty = float(input("Percentage of empty spaces: "))
    perc_seats_occupied = float(input("Percentage of seats occupied: "))
    online = bool(input("Online(boolean):" ))

def generate():
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
    return cinema

def pretty_print(cinema):
    res = ""
    for i in range(len(cinema)):
        for j in range(len(cinema[i])):
            res += str(cinema[i,j])
        res += "\n"
    return res

def write_to_file(cinema, group_sizes):
    pretty_cinema = pretty_print(cinema)
    f = open("cinema_online.txt" if online else "cinema_offline.txt", "w")
    f.write(str(n_rows) + "\n")
    f.write(str(n_columns) + "\n")
    f.write(pretty_cinema)

    for group in group_sizes:
          f.write(str(group) + " ")
  

def count_seats(cinema):
    n_of_people = 0
    for i in range(len(cinema)):
        for j in range(len(cinema[i])):
            if (cinema[i,j] == 1):
                n_of_people += 1
    return n_of_people

def generate_groups_offline(cinema):
    ## first calculate total number of seats available
    ## do we always generate group sizes up to the max seats available
    group_sizes = np.full(8, 0)
    n_of_people = 0;
    n_of_seats = perc_seats_occupied * count_seats(cinema)

    while(n_of_people < n_of_seats):
        rand = random.random()

        if rand < 0.2:
            group_sizes[0] += 1
            n_of_people += 1
        elif rand < 0.4:
            group_sizes[1] += 1
            n_of_people += 2
        elif rand < 0.6:
            group_sizes[2] += 1
            n_of_people += 3
        elif rand < 0.7:
            group_sizes[3] += 1
            n_of_people += 4
        elif rand < 0.8:
            group_sizes[4] += 1
            n_of_people += 5
        elif rand < 0.9:
            group_sizes[5] += 1
            n_of_people += 6
        elif rand < 0.95:
            group_sizes[6] += 1
            n_of_people += 7
        else:
            group_sizes[7] += 1
            n_of_people += 8

    return group_sizes

def generate_groups_online(cinema):
    group_sizes = np.full(0, int)
    n_of_people = 0;
    n_of_seats = perc_seats_occupied * count_seats(cinema)


    while(n_of_people < n_of_seats):
        rand = random.random()

        if rand < 0.2:
            group_sizes = np.append(group_sizes, 1)
            n_of_people += 1
        elif rand < 0.4:
            group_sizes = np.append(group_sizes, 2)
            n_of_people += 2
        elif rand < 0.6:
            group_sizes = np.append(group_sizes, 3)
            n_of_people += 3
        elif rand < 0.7:
            group_sizes = np.append(group_sizes, 4)
            n_of_people += 4
        elif rand < 0.8:
            group_sizes = np.append(group_sizes, 5)
            n_of_people += 5
        elif rand < 0.9:
            group_sizes = np.append(group_sizes, 6)
            n_of_people += 6
        elif rand < 0.95:
            group_sizes = np.append(group_sizes, 7)
            n_of_people += 7
        else:
            group_sizes = np.append(group_sizes, 8)
            n_of_people += 8

    group_sizes = np.append(group_sizes, 0)
    return group_sizes

cinema = generate()
groups = generate_groups_online(cinema) if online else generate_groups_offline(cinema)
write_to_file(cinema, groups)

print("Done")