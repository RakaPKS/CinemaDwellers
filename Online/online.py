import argparse
import numpy as np

def placeGroup(problem, x, y, groupSize):
    cinema = problem.copy()
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
            if x + i >= 0  and x + i < len(cinema) and y + j < len(cinema[0]) and x + j >= 0 and cinema[x + i][y + j] == 1:
                result += 1
    if x - 2 >= 0 and cinema[x - 2][y] == 1:
        result += 1
    if x + groupSize + 1 < len(cinema) and cinema[x + groupSize + 1][y] == 1:
        result += 1
    return result

def doesGroupFit(cinema, x, y, groupSize):
    for i in range(groupSize):
        if x + i < len(cinema):
            if cinema[x + i][y] != 1:
                return False
    return True

def findBestPos(groupSize, cinema):
    startPos = (-1, -1)
    disabledSeats = 9999
    for i in range(len(cinema)):
        for j in range(len(cinema[0])):
            if doesGroupFit(cinema, i, j, groupSize):
                newDisabledSeats = countDisabledSeats(cinema, i, j, groupSize)
                if(newDisabledSeats < disabledSeats):
                    startPos = (i, j)
                    disabledSeats = newDisabledSeats
    return startPos

def printCinema(cinema):
    res = ""
    for i in range(len(cinema)):
        for j in range(len(cinema[0])):
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
    ##for i, line in enumerate(lines[2 : v - 3]):
        ###problem[i, :] = np.array([bool(int(z)) for z in line.strip()])
    cinemaText = lines[2 : v - 3]
    
    for i in range(v):
        for j in range(h):
            problem[i][j] = int(cinemaText[j][i])

    blob = problem.copy()
    
    ###blob = placeGroup(blob, 0, 0, 4).copy()

    for i in range(len(people)):
        (x, y) = findBestPos(people[i], problem)
        if (x, y) != (-1, -1):
            blob = placeGroup(blob, x, y, people[i]).copy()
    
    printCinema(blob)