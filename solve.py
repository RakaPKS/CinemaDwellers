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
        for i, line in enumerate(lines[2 : -1]):
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
    args = parser.parse_args()

    a = Seating(*read_instance(args.filename))
    seats, no_seat = a.greedy()
    print()
    print("Output (2=person seated)")
    print(seats)
    print("Not seated", no_seat)
