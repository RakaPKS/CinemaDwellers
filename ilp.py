import sys
import math
import argparse
from pprint import pprint
import numpy as np
import gurobipy as gp
from gurobipy import GRB

from solve import read_instance
from seating import Seating
from utils import check_legal, count_seated, verify_cinema


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

    # Only one group per position
    for x in range(xs):
        for y in range(ys):
            model.addConstr(
                gp.quicksum([seated[x, y, g] for g in range(group_amount)]),
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

    # For every combination of groups g1, g2
    for g1 in range(group_amount):
        for g2 in range(group_amount):
            if g1 != g2:
                # For each combination of positions (x1,y1), (x2,y2)
                # TODO: Loop only over possible positions for each group_size.
                # This can for instance be done using find_legal_startpositions
                #for x1 in range(xs):
                #    for y1 in range(ys):
                #        prev_is_legal = False
                #        # Process left hand side first
                #        for xl2 in range(x1, -1, -1):
                #            # if the previous is legal, we do not need to go deeper
                #            if (prev_is_legal):
                #                break;

                #            (is_legal, _) = check_legal(group_sizes[g1], group_sizes[g2], x1, xl2 - 1, y1, y1)

                #            if not is_legal:
                #                model.addConstr(seated[x1, y1, g1] + seated[xl2, y1, g2], GRB.LESS_EQUAL,1)

                #            prev_is_legal = is_legal

                #        # Process right hand side
                #        prev_is_legal = False
                #        for xr2 in range(x1,xs):
                #            if (prev_is_legal):
                #                break;

                #            (is_legal, _) = check_legal(group_sizes[g1], group_sizes[g2], x1, xr2, y1, y1)

                #            if not is_legal:
                #                model.addConstr(seated[x1, y1, g1] + seated[xr2, y1, g2], GRB.LESS_EQUAL,1)

                #            prev_is_legal = is_legal

                #        # Process towards the screen
                #        prev_is_legal = False
                #        for yu2 in range(y1,0, -1):
                #            if (prev_is_legal):
                #                break;

                #            (is_legal, _) = check_legal(group_sizes[g1], group_sizes[g2], x1, x1, y1, yu2 - 1)

                #            if not is_legal:
                #                model.addConstr(seated[x1, y1, g1] + seated[x1, yu2, g2], GRB.LESS_EQUAL,1)

                #            prev_is_legal = is_legal
                        
                #        # Process away fron the screen
                #        prev_is_legal = False
                #        for yb2 in range(y1,ys):
                #            if (prev_is_legal):
                #                break;

                #            (is_legal, _) = check_legal(group_sizes[g1], group_sizes[g2], x1, x1, y1, yb2)

                #            if not is_legal:
                #                model.addConstr(seated[x1, y1, g1] + seated[x1, yb2, g2], GRB.LESS_EQUAL,1)

                #            prev_is_legal = is_legal

                for x1 in range(xs):
                    for x2 in range(xs):
                        for y1 in range(ys):
                            for y2 in range(ys):
                                # Check if this combination of positions is illegal, if so, add a constraint
                                (is_legal, _) = check_legal(group_sizes[g1], group_sizes[g2], x1, x1, y1, y2)
                                if not is_legal:
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
        default="instances/instance.txt",
        help="Filename with offline instance",
    )
    args = parser.parse_args()

    make_and_solve_ILP(args.filename)
