import gurobipy as gp
from gurobipy import GRB
import numpy as np
from solve import read_instance
from seating import Seating
import math
import argparse


def make_ILP(filename="instances/mini.txt"):
    # Prepare our problem instance
    problem, people, rows, columns = read_instance(filename)

    group_amount = int(np.sum([z for _, z in people.items()]))
    group_sizes = np.concatenate(
        [np.array(n * [i + 1], dtype=int) for i, n in people.items()]
    )

    people_amount = np.sum(group_sizes)
    print(group_sizes, group_amount, people_amount)

    # Instantiate a gurobi ILP model
    model = gp.Model()

    # Start position (Col, Row) for every group
    # (-1,-1) means a group is not seated
    # The maximum Col-position is columns-1
    col = model.addVars(
        group_amount, lb=-1, ub=columns - 1, vtype=GRB.INTEGER, name="col"
    )
    row = model.addVars(group_amount, lb=-1, ub=rows - 1, vtype=GRB.INTEGER, name="row")

    # Every group has a constant size
    size = model.addVars(
        group_amount,
        lb=tuple(group_sizes),
        ub=tuple(group_sizes),
        vtype=GRB.INTEGER,
        name="groupsize",
    )

    # Binary indicators of whether the groups are to the left/right up/down of each other
    hor1 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="hor1")
    hor2 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="hor2")
    ver1 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="ver1")
    ver2 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="ver2")

    # Store difference in both directions so we can apply diagonal formula |x1 + s1 - x2| == |y1 - y2|
    # Difference can be negative, -infinity necessary as lower bound
    diff_hor = model.addVars(
        group_amount, group_amount, lb=-GRB.INFINITY, vtype=GRB.INTEGER, name="diff_hor"
    )
    diff_ver = model.addVars(
        group_amount, group_amount, lb=-GRB.INFINITY, vtype=GRB.INTEGER, name="diff_ver"
    )
    diff_ver2 = model.addVars(
        group_amount,
        group_amount,
        lb=-GRB.INFINITY,
        vtype=GRB.INTEGER,
        name="diff_ver2",
    )

    # Stores absolute difference
    abs_diff_hor = model.addVars(
        group_amount, group_amount, vtype=GRB.INTEGER, name="diagguy1"
    )
    abs_diff_ver = model.addVars(
        group_amount, group_amount, vtype=GRB.INTEGER, name="diagguy2"
    )
    abs_diff_ver2 = model.addVars(
        group_amount, group_amount, vtype=GRB.INTEGER, name="diagguy3"
    )

    # Stores whether the absolute differences are the same
    diag1 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="diag1")
    diag2 = model.addVars(group_amount, group_amount, vtype=GRB.BINARY, name="diag2")

    # Check that X1 + S1 != X2 for any group combination (1,2)
    # X1 + S1 > X2
    notequal_1 = model.addVars(
        group_amount, group_amount, vtype=GRB.BINARY, name="hotfix1"
    )

    # X1 + S1 < X2
    notequal_2 = model.addVars(
        group_amount, group_amount, vtype=GRB.BINARY, name="hotfix2"
    )

    # Variables that tell us whether a group is seated
    groupseated = model.addVars(group_amount, vtype=GRB.BINARY, name="groupseated")
    groupseatedx = model.addVars(group_amount, vtype=GRB.BINARY, name="groupseatedx")
    groupseatedy = model.addVars(group_amount, vtype=GRB.BINARY, name="groupseatedy")

    # Reward[i] holds the value seated[i]
    # (0/1) * group_size
    reward = model.addVars(group_amount, vtype=GRB.INTEGER, name="reward")

    # What should be maximized in the end
    finalreward = model.addVar(
        lb=0, ub=people_amount, vtype=GRB.INTEGER, name="finalreward"
    )

    for i in range(group_amount):
        # For every group, add booleans that store whether the group is seated
        model.addGenConstrIndicator(groupseatedx[i], 1, col[i], GRB.GREATER_EQUAL, 0)
        model.addGenConstrIndicator(groupseatedy[i], 1, row[i], GRB.GREATER_EQUAL, 0)

        # If both ate > -1, the group is seated
        model.addGenConstrIndicator(
            groupseated[i], 1, groupseatedx[i] + groupseatedy[i], GRB.EQUAL, 2
        )

        # When a group is not seated, both coordinates should be -1 (no "hacking")
        model.addGenConstrIndicator(groupseatedy[i], 0, col[i], GRB.EQUAL, -1)
        model.addGenConstrIndicator(groupseatedx[i], 0, row[i], GRB.EQUAL, -1)

        # Add the reward for this group
        model.addConstr(reward[i], GRB.EQUAL, groupseated[i] * size[i])

        # Any start_X position + group_size of that group should not "fall out of cinema"
        # TODO: This should not be activated if the start position is -1
        model.addConstr(col[i] + size[i], GRB.LESS_EQUAL, columns)

        # Every group combination should keep their distance
        for j in range(group_amount):
            if i != j:
                # TODO: These constraints should not be activated if the start position is -1
                # Hor: Either si is to the right of sj by at least 3 spots
                model.addGenConstrIndicator(
                    hor1[i, j], 1, col[i] - (col[j] + size[j]), GRB.GREATER_EQUAL, 2
                )
                model.addGenConstrIndicator(
                    hor2[i, j], 1, col[j] - col[i], GRB.GREATER_EQUAL, 2
                )

                # Ver: Either si is two rows away from sj
                model.addGenConstrIndicator(
                    ver1[i, j], 1, row[i] - row[j], GRB.GREATER_EQUAL, 2
                )
                model.addGenConstrIndicator(
                    ver2[i, j], 1, row[j] - row[i], GRB.GREATER_EQUAL, 2
                )

                # Store all info for the diagonal problem
                # Differences
                model.addConstr(diff_hor[i, j] == (row[i] - row[j]))
                model.addConstr(diff_ver[i, j] == (col[i] + size[i]) - col[j])
                model.addConstr(diff_ver2[i, j] == (col[j] + size[j]) - col[i])

                # Absolute values of those differences
                model.addGenConstrAbs(abs_diff_hor[i, j], diff_hor[i, j])
                model.addGenConstrAbs(abs_diff_ver[i, j], diff_ver[i, j])
                model.addGenConstrAbs(abs_diff_ver2[i, j], diff_ver2[i, j])

                model.addGenConstrIndicator(
                    diag1[i, j],
                    1,
                    abs_diff_hor[i, j] - abs_diff_ver[i, j],
                    GRB.EQUAL,
                    0,
                )
                model.addGenConstrIndicator(
                    diag2[i, j],
                    1,
                    abs_diff_hor[i, j] - abs_diff_ver2[i, j],
                    GRB.EQUAL,
                    0,
                )

                # Store all info for the inequality constraint
                model.addGenConstrIndicator(
                    notequal_1[i, j],
                    1,
                    (col[i] + size[i]) - col[j],
                    GRB.GREATER_EQUAL,
                    1,
                )
                model.addGenConstrIndicator(
                    notequal_2[i, j],
                    1,
                    col[j] - (col[i] + size[i]),
                    GRB.GREATER_EQUAL,
                    1,
                )

                # Add the inequality constraint
                model.addConstr(
                    notequal_1[i, j] + notequal_2[i, j], GRB.GREATER_EQUAL, 1
                )

                # Finally, see if one of these is true
                model.addConstr(
                    hor1[i, j] + hor2[i, j] + ver1[i, j] + ver2[i, j] + diag2[i, j],
                    GRB.GREATER_EQUAL,
                    1,
                )

    model.addConstr(finalreward, GRB.EQUAL, gp.quicksum(reward))
    model.setObjective(finalreward, GRB.MAXIMIZE)

    # model.relax()
    model.optimize()
    solution = problem.copy()
    for i in range(group_amount):
        col_position = int(col[i].x)
        row_position = int(row[i].x)
        grp = int(size[i].x)
        print(
            "Position (", col_position, ", ", row_position, ") for group of size:", grp
        )
        print("Group seated", groupseatedx[i].x, groupseatedy[i].x, groupseated[i].x)
        if row_position >= 0 and col_position >= 0:
            solution[row_position, col_position : col_position + grp] = (
                np.zeros(grp) + 2
            )
    print(solution)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--filename",
        type=str,
        default="instances/instance.txt",
        help="Filename with offline instance",
    )
    args = parser.parse_args()

    make_ILP(args.filename)
