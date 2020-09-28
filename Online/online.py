import argparse
import numpy as np
import utils

maxdisabled = 0

def doesGroupFit(cinema, x, y, groupSize):
    for i in range(groupSize):
        if x + i >= len(cinema) or cinema[x + i][y] != 1:
            return False
    return True

def placeGroup(cinema, x, y, groupSize):
    if not doesGroupFit(cinema, x, y, groupSize):
        print("oopsie")
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
    ###return cinema

def countDisabledSeats(cinema, x, y, groupSize):
    result = 0
    for i in range(-1, groupSize + 1):
        for j in range(-1, 2):
            if x + i >= 0  and x + i < len(cinema) and y + j < len(cinema[0]) and y + j >= 0 and cinema[x + i][y + j] == 1:
                result += 1.0 - abs(j) * (1.0/100)
    if x - 2 >= 0 and cinema[x - 2][y] == 1:
        result += 1
    if x + groupSize + 1 < len(cinema) and cinema[x + groupSize + 1][y] == 1:
        result += 1
    global maxdisabled
    if result > maxdisabled:
        maxdisabled = result
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

def initialize(cinema, v, h):
    result = np.zeros((8, v, h))
    for i in range(8):
        for x in range(v):
            for y in range(h):
                if doesGroupFit(cinema, x, y, i + 1):
                    result[i,x,y] = countDisabledSeats(cinema, x, y, i + 1)
                else:
                    result[i,x,y] = -1
    return result

def printCinema(cinema):
    res = ""
    for j in range(len(cinema[0])):
        for i in range(len(cinema)):
            res += str(int(cinema[i][j])) + " "
        res += "\n"
    print(res)

def updateData(problem, x, y, data):
    asdf = 1
    for k in range(8):
        for i in range (-3, k + 2):
            for j in range(-3, 4):
                if x + i >= 0 and x + i < len(problem) and y + j >= 0 and y + j < len(problem[1]):
                    disabledSeats = countDisabledSeats(problem, x + i, y + j, k + 1) 
                    if disabledSeats == 0:
                        disabledSeats = - 1
                    data[k][x + i][y + j] = disabledSeats
        if x - 2 >= 0:
            data[k][x - 2][y] = countDisabledSeats(problem, x - 2, y, k + 1)
        if x + k + 2 < len(problem):
            data[k][x + k + 2][y] = countDisabledSeats(problem, x + k + 2, y, k + 1)

def countSeated(cinema):
    res = 0
    for i in range(len(cinema)):
        for j in range(len(cinema[0])):
            if cinema[i][j] == 2:
                res += 1
    return res

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

        data = initialize(problem, v, h)

        for i in range(len(people)):
            (x, y) = findBestPos(people[i], problem, data)
            if (x, y) != (-1, -1):
                placeGroup(problem, x, y, people[i])
                printCinema(data[people[i] - 1])
                updateData(problem, x, y, data)
                printCinema(data[people[i] - 1])
                adsf = 1

        printCinema(problem)

        totalPeople = 0
        for (i, amt) in people.items():
            totalPeople += amt

        print("Seated " + str(countSeated(problem))  + " people out of " + str(totalPeople) + " total people.")

        utils.verify_cinema(problem, h ,v)
        
if __name__ == "__main__":
    main()
