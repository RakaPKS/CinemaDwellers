import sys
import math
import argparse
import time
from pprint import pprint
import numpy as np
from tqdm import tqdm
import gurobipy as gp
from gurobipy import GRB
from collections import defaultdict
import os
import csv
from datetime import datetime

from seating import Seating
from utils import (
    read_instance,
    check_legal,
    count_seated,
    verify_cinema,
    get_invalid_seats,
    find_legal_start_positions,
    filter_people,
)


def make_and_solve_ILP(filename, optimized=False, configFile=""):
    """
    Encodes and solves ILP
    """
    # Prepare our problem instance
    cinema, people, ys, xs = read_instance(filename)

    # Filter all the people that cannot be seated.
    # This will mean less variables in our ILP problem
    people = filter_people(cinema, people)

    group_amount = int(np.sum([z for _, z in people.items()]))
    group_sizes = np.concatenate(
        [np.array(n * [i + 1], dtype=int) for i, n in people.items()]
    )
    max_group_size = np.max(group_sizes)
    people_amount = np.sum(group_sizes)
    print(
        group_amount,
        " groups with sizes: ",
        group_sizes,
        "total people waiting:",
        people_amount,
    )
    start = time.time()

    # Collect legal positions per group size only once
    legals = dict()
    for size, amt in people.items():
        if amt > 0:
            legals[size + 1] = find_legal_start_positions(size + 1, cinema)

    # Collect group sizes
    size_to_group = defaultdict(list)
    for i, group_size in enumerate(group_sizes):
        size_to_group[group_size].append(i)
    # sys.exit(0)
    print(size_to_group)
    # Instantiate a gurobi ILP model
    model = gp.Model()

    if not configFile == "":
        model.read(configFile)

    # Each group has a binary variable per possible seat
    seated = model.addVars(xs, ys, group_amount,
                           vtype=GRB.BINARY, name="seated")

    # Every group has a constant size
    size = model.addVars(
        group_amount,
        lb=tuple(group_sizes),
        ub=tuple(group_sizes),
        vtype=GRB.INTEGER,
        name="groupsize",
    )

    # Only one position per group
    for g in range(group_amount):
        model.addConstr(
            gp.quicksum([seated[x, y, g] for x in range(xs)
                         for y in range(ys)]),
            GRB.LESS_EQUAL,
            1,
        )

    # Check for non-seats and out-of-bounds groups
    # Do not seat a group when it will overlap with a 0-position/the end of the cinema
    for x in range(xs):
        for y in range(ys):
            for g in range(group_amount):
                # Loop over all positions from here to here + group_size
                any_zeros = False
                for i in range(0, group_sizes[g]):
                    if x + i >= xs or cinema[y, x + i] == 0:
                        any_zeros = True
                        break

                # If any positions were zero, the starting position was illegal
                if any_zeros:
                    model.addConstr(seated[x, y, g], GRB.EQUAL, 0)

    if optimized:
        for size1 in tqdm(range(1, max_group_size+1)):
            for size2 in range(1, max_group_size+1):
                    # Look for all illegal combinations
                    # Start with every possible position where g1 can be seated using the legals dictionary
                for (y1, x1) in legals[size1]:
                        # Calculate illegal seats for g2 given this start position
                    invalid_seats = get_invalid_seats(
                        x1, y1, size1, size2, xs, ys)
                    # Add a constraint, only one group can be seated in this area (<= 1)
                    for (x2, y2) in invalid_seats:
                        # For every combination of groups with these specific sizes
                        for g1 in size_to_group[size1]:
                            for g2 in size_to_group[size2]:
                                if g1 != g2:
                                    model.addConstr(
                                        seated[x1, y1, g1] + seated[x2,
                                                                    y2, g2], GRB.LESS_EQUAL, 1,
                                    )
    else:
        # For every combination of groups g1, g2
        for g1 in tqdm(range(group_amount)):
            for g2 in range(group_amount):
                if g1 > g2:
                    size1 = group_sizes[g1]
                    size2 = group_sizes[g2]
                    for (y1, x1) in legals[size1]:
                        # Calculate illegal seats for g2 given this start position
                        invalid_seats = get_invalid_seats(
                            x1, y1, size1, size2, xs, ys)
                        # Add a constraint, only one group can be seated in this area (<= 1)
                        for (x2, y2) in invalid_seats:
                            model.addConstr(
                                seated[x1, y1, g1] + seated[x2,
                                                            y2, g2], GRB.LESS_EQUAL, 1,
                            )
                        del invalid_seats

    # TODO: Add more constraints that will help fastness of solver

    # Maximize number of people seated = seated_g * group_size_g
    # TODO: Set objective maximum manually to help gurobi.
    model.setObjective(
        gp.quicksum(
            [
                seated[x, y, g] * size[g]
                for x in range(xs)
                for y in range(ys)
                for g in range(group_amount)
            ]
        ),
        GRB.MAXIMIZE,
    )

    constraintTime = time.time() - start

    print(
        "DONE ENCODING IN %s seconds.. STARTING OPTIMIZATION... "
        % (time.time() - start)
    )
    start = time.time()

    model.optimize()

    optimizeTime = time.time() - start

    # Get the solution
    solution = cinema.copy()
    for x in range(xs):
        for y in range(ys):
            for g in range(group_amount):
                if seated[x, y, g].x > 0:
                    solution[y, x: x + group_sizes[g]
                             ] = np.zeros(group_sizes[g]) + 2

    print(cinema)
    print("--- Was solved in %s seconds ---" % (time.time() - start))
    print(solution)
    print("Not seated", people_amount -
          count_seated(solution), "out of", people_amount)

    valid = verify_cinema(solution, xs, ys)

    return (filename, configFile, constraintTime, optimizeTime, group_amount, people_amount, valid, people_amount - count_seated(solution))


def experiment_runner(optimize=False):
    instanceFolder = "./Offline/instances"
    configFolder = "./Offline/configs"
    fields = ['InstanceFile', 'ConfigFile', 'ConstraintTime', 'OptimizationTime',
              'TotalNumberOfGroups', 'TotalNumberOfPeople', 'Valid', 'Seated']
    resultsFile = "./Offline/results/python_{}.csv".format(
        datetime.now().strftime("%d-%m-%Y-%H-%M-%S"))

    with open(resultsFile, 'w+', newline='') as csvfile:
        # creating a csv writer object
        csvwriter = csv.writer(csvfile)
        csvwriter.writerow(fields)

    for i in range(1, len(os.listdir(instanceFolder))):
        instanceFile = "{}/Exact{}.txt".format(instanceFolder, i)
        configFile = "{}/tune_Exact{}_0.prm".format(configFolder, i)

        solveResult = make_and_solve_ILP(instanceFile, optimize, configFile)

        with open(resultsFile, 'a+', newline='') as csvfile:
            # creating a csv writer object
            csvwriter = csv.writer(csvfile)

            csvwriter.writerow(np.asarray(solveResult))


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--filename",
        type=str,
        default="instances/instance3.txt",
        help="Filename with offline instance",
    )
    parser.add_argument(
        "--optimize",
        type=bool,
        default='',
        help="Use our Optimized^TM group looper",
    )
    args = parser.parse_args()

    args.experiments = True

    if args.experiments:
        experiment_runner(args.optimize)
    else:
        make_and_solve_ILP(args.filename, args.optimize)
