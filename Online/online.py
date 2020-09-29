import argparse
import numpy as np
import utils

def doesGroupFit(cinema, x, y, groupSize):
    for i in range(groupSize):
        if x + i >= len(cinema) or cinema[x + i][y] != 1:
            return False
    return True

def placeGroup(cinema, x, y, groupSize):
    if not doesGroupFit(cinema, x, y, groupSize):
        print("oopsie, you cannot place a group here")
    for i in range(-1, groupSize + 1):
        for j in range(-1, 2):
            if x + i >= 0  and x + i < len(cinema) and y + j >= 0 and y + j < len(cinema[0]):
                cinema[x + i][y + j] = 0
    if x - 2 >= 0 and x - 2 < len(cinema):
        cinema[x - 2][y] = 0
    if x + groupSize + 1 >= 0 and x + groupSize + 1 < len(cinema):
        cinema[x + groupSize + 1][y] = 0
    for i in range(groupSize):
        if x + i < len(cinema):
            cinema[x + i][y] = 2
    ###return cinema

def countDisabledSeats(cinema, x, y, groupSize):
    result = 0
    for i in range(-1, groupSize + 1):
        for j in range(-1, 2):
            if x + i >= 0  and x + i < len(cinema) and y + j < len(cinema[0]) and y + j >= 0 and cinema[x + i][y + j] == 1:
                result += 1 ###- abs(j) * (0.0/100)
    if x - 2 >= 0 and x - 2 < len(cinema) and cinema[x - 2][y] == 1:
        result += 1
    if x + groupSize + 1 >= 0 and x + groupSize + 1 < len(cinema) and cinema[x + groupSize + 1][y] == 1:
        result += 1
    return result

def findBestPos(groupSize, cinema, data):
    startPos = (-1, -1)
    disabledSeats = 9999
    for i in range(len(cinema)):
        for j in range(len(cinema[0])):
            if data[groupSize - 1][i][j] != -1 and data[groupSize - 1][i][j] < disabledSeats:
                startPos = (i, j)
                disabledSeats = data[groupSize - 1][i][j]

            ###if doesGroupFit(cinema, i, j, groupSize):
                ###newDisabledSeats = countDisabledSeats(cinema, i, j, groupSize)
                ###if(newDisabledSeats < disabledSeats):
                    ###startPos = (i, j)
                    ###disabledSeats = newDisabledSeats
    return startPos

def printCinema(cinema):
    res = ""
    for j in range(len(cinema[0])):
        for i in range(len(cinema)):
            res += (" " * (2 - len(str(int(cinema[i][j]))))) + str(int(cinema[i][j])) + " "
        res += "\n"
    print(res)

def initialize(cinema, v, h):
    result = np.zeros((8, v, h))
    for i in range(8):
        for x in range(v):
            for y in range(h):
                if doesGroupFit(cinema, x, y, i + 1):
                    result[i][x][y] = countDisabledSeats(cinema, x, y, i + 1)
                else:
                    result[i][x][y] = -1
    return result

def updateData(problem, x, y, data):
    asdf = 1
    for k in range(8):
        for i in range (-6, k + 7):
            for j in range(-6, 7):
                if x + i >= 0 and x + i < len(problem) and y + j >= 0 and y + j < len(problem[0]):
                    ###print("k: " + str(k) + " x: " + str(x) + " i :" + str(i) + " y:" + str(y) + " j:" + str(j))
                    if doesGroupFit(problem, x + i, y + j, k + 1):
                        data[k][x + i][y + j] = countDisabledSeats(problem, x + i, y + j, k + 1) 
                    else:
                        data[k][x + i][y + j] = -1
        ###if x - 2 >= 0:
            ###data[k][x - 2][y] = countDisabledSeats(problem, x - 2, y, k + 1)
        ###if x + k + 2 < len(problem):
            ###data[k][x + k + 2][y] = countDisabledSeats(problem, x + k + 2, y, k + 1)

def countSeated(cinema):
    res = 0
    for i in range(len(cinema)):
        for j in range(len(cinema[0])):
            if cinema[i][j] == 2:
                res += 1
    return res

def checkDataVsCalc(problem, data, v, h):
    equal = True
    newCalc = initialize(problem, v, h)
    for i in range(v):
        for j in range(h):
            if abs(newCalc[0][i][j] - data[0][i][j]) > 0.001:
                print("error at: " + str(i) + ", " + str(j))
                equal = False
    printCinema(problem)
    printCinema(newCalc[0])
    printCinema(data[0])
    return equal

def main():
    filename="cinema_online.txt"
    with open(filename, "r") as f:
        lines = f.readlines()
        h = int(lines[0].strip())
        v = int(lines[1].strip())
        people = {}
        problem = np.zeros((v, h))
        for amt, i in enumerate(lines[-1].split()):
            people[amt] = int(i)
        for i, line in enumerate(lines[2 : v + 2]):
            problem[:, i] = np.array([bool(int(z)) for z in line.strip()])

        test = countDisabledSeats(problem, 0, 0, 1)

        data = initialize(problem, v, h)

        printCinema(problem)

        for i in range(len(people)):
            (x, y) = findBestPos(people[i], problem, data)
            if (x, y) != (-1, -1):
                placeGroup(problem, x, y, people[i])
                print("placed group at " + str(x) + "," + str(y) + " of size: " + str(people[i]))
                updateData(problem, x, y, data)
                if checkDataVsCalc(problem, data, v, h):
                    print("nice")
                else:
                    print("not nice")
                adsf = 1

        printCinema(problem)

        totalPeople = 0
        for (i, amt) in people.items():
            totalPeople += amt

        print("Seated " + str(countSeated(problem))  + " people out of " + str(totalPeople) + " total people.")

        utils.verify_cinema(problem, h ,v)
        
if __name__ == "__main__":
    main()
