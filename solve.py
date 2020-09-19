import argparse
import numpy as np

from seating import Seating


def read_instance(filename="instances/instance.txt"):
    with open(filename, "r") as f:
        lines = f.readlines()
        h = int(lines[0].strip())
        v = int(lines[1].strip())
        people = {}
        problem = np.zeros((h, v))

        for amt, i in enumerate(lines[-1].split()):
            people[amt] = int(i)
        for i, line in enumerate(lines[2:-1]):
            # print(line.strip())
            problem[i, :] = np.array([bool(int(z)) for z in line.strip()])

    print("Input")
    print(problem)
    return problem, people, h, v


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--filename",
        type=str,
        default="instances/instance.txt",
        help="Filename with offline instance",
    )
    parser.add_argument(
        "--search", type=str, 
        default="n", help="Apply search? [y|n]"
    )
    args = parser.parse_args()

    a = Seating(*read_instance(args.filename))
    seats, no_seat = a.greedy()
    print()
    print("Output (greedy) (2=person seated)")
    print(seats)
    print("Not seated", no_seat, "out of", a.totalpeople)
    if no_seat > 0 and args.search =='y':
        print()
        print("Output (opt) (2=person seated)")
        a = Seating(*read_instance(args.filename))
        a.dfs()
