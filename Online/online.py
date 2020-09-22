import argparse
import numpy as np

def placeGroup(cinema, x, y, groupSize):
    for i in range(-1, groupSize + 1):
        for j in range(-1, 2):
            if x + i >= 0  and x + i < len(cinema) and y + j < len(cinema[0]):
                cinema[x + i][y + j] = 0
    if x - 2 >= 0:
        cinema[x - 2][y] = 0
    if x + groupSize + 1 < len(cinema):
        cinema[x + groupSize + 1][y] = 0
    for i in range(groupSize):
        if x + i < len(cinema):
            cinema[x + i][y] = 2
    return cinema

def countDisabledSeats(cinema, x, y, groupSize):
    result = 0
    for i in range(-1, groupSize + 1):
        for j in range(-1, 2):
            if x + i >= 0  and x + i < len(cinema) and y + j < len(cinema[0]) and y + j >= 0 and cinema[x + i][y + j] == 1:
                result += 1
    if x - 2 >= 0 and cinema[x - 2][y] == 1:
        result += 1
    if x + groupSize + 1 < len(cinema) and cinema[x + groupSize + 1][y] == 1:
        result += 1
    return result

def doesGroupFit(cinema, x, y, groupSize):
    for i in range(groupSize):
        if x + i >= len(cinema) or cinema[x + i][y] != 1:
            return False
    ###for i in range(-1, groupSize + 1):
        ###for j in range(-1, 2):
            ###if x + i < 0 or x + i >= len(cinema) or y + j < 0 or y + j >= len(cinema[0]) or cinema[x + i][y + j] != 1:
                ###return False
    ###if x - 2 < 0 or cinema[x - 2][y] != 1:
        ###return False
    ###if x + groupSize + 1 < len(cinema) and cinema[x + groupSize + 1][y] != 1:
        ###return False
    return True

def findBestPos(groupSize, cinema):
    startPos = (-1, -1)
    disabledSeats = 9999
    for j in range(len(cinema[0])):
        for i in range(len(cinema)):
            if doesGroupFit(cinema, i, j, groupSize):
                newDisabledSeats = countDisabledSeats(cinema, i, j, groupSize)
                if newDisabledSeats > 0 and newDisabledSeats < disabledSeats:
                    startPos = (i, j)
                    disabledSeats = newDisabledSeats
    return startPos

def printCinema(cinema):
    res = ""
    for j in range(len(cinema[0])):
        for i in range(len(cinema)):
            res += str(cinema[i][j]) + " "
        res += "\n"
    print(res)

filename="cinema_online.txt"
with open(filename, "r") as f:
    lines = f.readlines()
    h = int(lines[0].strip())
    v = int(lines[1].strip())
    people = {}
    problem = np.zeros((v, h))
    for amt, i in enumerate(lines[-1].split()):
        people[amt] = int(i)
    ###for i, line in enumerate(lines[2 : v - 3]):
        ###problem[i, :] = np.array([bool(int(z)) for z in line.strip()])
    
    cinemaText = lines[2 : v - 3]

    for i in range(v):
        for j in range(h):
            problem[i][j] = int(cinemaText[j][i])
    
    blob = problem.copy()
    
    for i in range(len(people)):
        (x, y) = findBestPos(people[i], blob)
        if (x, y) != (-1, -1):
            blob = placeGroup(blob, x, y, people[i]).copy()
    
    printCinema(blob)