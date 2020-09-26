import argparse
import numpy as np
import time
from seating import Seating


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--filename",
        type=str,
        default="instances/instance.txt",
        help="Filename with offline instance",
    )
    parser.add_argument("--search", type=str, default="n", help="Apply search? [y|n]")
    args = parser.parse_args()

    a = Seating(*read_instance(args.filename))
    start = time.time()
    seats, no_seat = a.greedy()
    print()
    print("Output (greedy) (2=person seated)")
    print(seats)
    print("Not seated", no_seat, "out of", a.totalpeople)
    print("Execution time %s" % (time.time() - start))
    if no_seat > 0 and args.search == "y":
        print("Not everyone seated... Starting branch and bound search...")
        a = Seating(*read_instance(args.filename))
        start = time.time()

        a.dfs()
        print("Execution time %s" % (time.time() - start))
