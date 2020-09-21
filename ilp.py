import sys
import math
import argparse
from pprint import pprint
import numpy as np
import gurobipy as gp
from gurobipy import GRB

from solve import read_instance
from seating import Seating
from utils import check_legal


def make_ILP(filename):
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

    # Instantiate a gurobi ILP model
    model = gp.Model()

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

    # Only one position per group
    for x in range(xs):
        for y in range(ys):
            model.addConstr(
                gp.quicksum([seated[x, y, g] for g in range(group_amount)]),
                GRB.LESS_EQUAL,
                1,
            )

    # If there is no seat, do not start the group there
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

    # For every combination of groups, add all illegal would-be combinations
    for g1 in range(group_amount):
        for g2 in range(group_amount):
            if g1 != g2:
                for x1 in range(xs):
                    for x2 in range(xs):
                        for y1 in range(ys):
                            for y2 in range(ys):
                                # Check if this combination is illegal, if so add a constraint
                                if not check_legal(
                                    group_sizes[g1], group_sizes[g2], x1, x2, y1, y2
                                ):
                                    model.addConstr(
                                        seated[x1, y1, g1] + seated[x2, y2, g2],
                                        GRB.LESS_EQUAL,
                                        1,
                                    )

    # Maximize number of people seated = seated_g * group_size_g
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


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--filename",
        type=str,
        default="instances/myinstance.txt",
        help="Filename with offline instance",
    )
    args = parser.parse_args()

    make_ILP(args.filename)
