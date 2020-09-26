import sys
import math
import argparse
from pprint import pprint
import numpy as np
import gurobipy as gp
from gurobipy import GRB

from solve import read_instance
from seating import Seating
from utils import check_legal, count_seated, verify_cinema, get_invalid_seats, find_legal_start_positions


def make_and_solve_ILP(filename):
    """
    Encodes and solves ILP
    """
    # Prepare our problem instance
    cinema, people, ys, xs = read_instance(filename)

    group_amount = int(np.sum([z for _, z in people.items()]))
    group_sizes = np.concatenate(
        [np.array(n * [i + 1], dtype=int) for i, n in people.items()]
    )
    people_amount = np.sum(group_sizes)
    print(
        group_amount,
        " groups with sizes: ",
        group_sizes,
        "total people waiting:",
        people_amount,
    )

    legals = dict() 
    for size, amt in people.items():
        if amt > 0:
            legals[size] = find_legal_start_positions(size+1, cinema) 
            print(size, legals[size])
    sys.exit(0)


    # Instantiate a gurobi ILP model
    model = gp.Model()
    model.setParam("Presolve", 1)

    seated = model.addVars(xs, ys, group_amount, vtype=GRB.BINARY, name="seated")

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
            gp.quicksum([seated[x, y, g] for x in range(xs) for y in range(ys)]),
            GRB.LESS_EQUAL,
            1,
        )

    # Only one group per position
    #for x in range(xs):
    #    for y in range(ys):
    #        model.addConstr(
    #            gp.quicksum([seated[x, y, g] for g in range(group_amount)]),
    #            GRB.LESS_EQUAL,
    #            1,
    #        )

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

    # For every combination of groups g1, g2
    for g1 in range(group_amount):
        for g2 in range(group_amount):
            if g1 != g2:
                for x1 in range(xs):
                    for y1 in range(ys):
                        invalid_seats = get_invalid_seats(x1, y1, group_sizes[g1], group_sizes[g2], xs, ys)

                        for (x2, y2) in invalid_seats:
                            model.addConstr(
                                seated[x1, y1, g1] + seated[x2, y2, g2],
                                GRB.LESS_EQUAL,
                                1,
                            )

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
    print("DONE ENCODING.... STARTING OPTIMIZATION... ")
    model.optimize()

    # Get the solution
    solution = cinema.copy()
    for x in range(xs):
        for y in range(ys):
            for g in range(group_amount):
                if seated[x, y, g].x > 0:
                    solution[y, x : x + group_sizes[g]] = np.zeros(group_sizes[g]) + 2

    print(cinema)
    print("---")
    print(solution)
    print("Not seated", people_amount - count_seated(solution), "out of", people_amount)

    verify_cinema(solution, xs, ys)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--filename",
        type=str,
        default="instances/instance2.txt",
        help="Filename with offline instance",
    )
    args = parser.parse_args()

    make_and_solve_ILP(args.filename)
