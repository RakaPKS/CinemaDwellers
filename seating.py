import numpy as np
from collections import deque
import heapq


class Seating:
    def __init__(self, seats, people, h, v, seated=None, group_index=None):
        """
        Args:
            seats: Matrix
            people: array with group sizes
            h,v : size of matrix
            seated : For when copying a problem: where are people seated?
            group_index: What is the index of the next group to be seated in self.groups?
        """

        # Add padding so no out of range errors
        # Don't pad already padded seats
        if seated is None:
            self.available_seats = np.pad(
                seats, (2, 2), "constant", constant_values=(0, 0)
            )
            self.seated = self.available_seats.copy()
        else:
            # Where people are seated: when initializing,
            self.seated = seated
            self.available_seats = seats.copy()

        self.people = people
        self.h = h
        self.v = v

        # Go through groups in reverse order (big to small groups)
        self.groups = np.flip(
            np.concatenate(
                [np.array(n * [i + 1], dtype=int) for i, n in people.items()]
            )
        )
        self.totalpeople = np.sum(self.groups)
        if group_index is None:
            self.group_index = 0
        else:
            self.group_index = group_index

    def copy_seats(self, seats, seated, index):
        """
        Copies this instance of seating, but updates seats and seated
        Args:
            seats: Where are the current available seats?
            seated: Where are the people seated? (Matrix with 2s)
            index: What is the index of the next group to be seated in self.groups?
        """
        return Seating(
            seats, self.people, self.h, self.v, seated=seated, group_index=index
        )

    def dfs(self):
        """
        Runs dfs to find the optimal seating (==most people seated in total)
        """

        def hashed(c):
            return hash(c.data.tobytes())

        # Stack of tuples (Seating, Amt_of_People_Seated)
        stack = []  # deque()
        stack.append((self, 0))
        best = 0
        best_seats = None
        visited = set()

        while len(stack) > 0 and len(stack) < 1000:
            if np.random.rand() > 0.9:
                print("Stack size", len(stack), self.totalpeople - best)
            (current, amt_seated) = stack.pop()
            h = hashed(current.seated)
            visited.add(h)

            if amt_seated > best:
                best = amt_seated
                best_seats = current

            # Go over all the groups that need to be seated, and add them depth-first (IF they can be seated)
            found_one = False
            while (current.group_index) < len(current.groups) and not found_one:
                # Find all ways this person can be seated
                current_group_size = current.groups[current.group_index]
                pos = current.find_legal_start_position(
                    current_group_size, current.available_seats
                )

                # TODO: Or possibly use a priority queue?
                # Right now: adding all possibilities at once.
                if len(pos) > 0:
                    opts = []
                    for _, possible in enumerate(pos):
                        # Hypothetical update
                        x, y = possible
                        a_s, _ = current.update_seats(
                            possible, current_group_size, current.available_seats
                        )

                        # Add hypothetical update to the stack/queue
                        new_seated = current.seated.copy()
                        new_seated[x, y : y + current_group_size] = (
                            np.zeros(current_group_size) + 2
                        )
                        seated_amount = np.count_nonzero(new_seated == 2)
                        free = np.count_nonzero(new_seated == 1)

                        # (Seating, Amount_Seated)
                        if hashed(new_seated) not in visited and best < (
                            seated_amount + free
                        ):
                            opts.append((free, new_seated, a_s, seated_amount))

                    for opt in sorted(opts, key=lambda x: x[0] + x[3], reverse=True)[:80]:
                        _, new_seated, a_s, seated_amount = opt
                        stack.append(
                            (
                                current.copy_seats(
                                    a_s, new_seated, current.group_index + 1
                                ),
                                seated_amount,
                            )
                        )
                    found_one = True
                else:
                    # This group could not be seated, try the next group
                    # This is necessary if there are groups too large for the cinema
                    current.group_index += 1
        if best_seats is None: 
            print("Stack became 2 Big")
        else:
            print(best_seats.seated[2:-2, 2:-2])
            print("Not seated", np.sum(self.groups) - best)

    def greedy(self):
        """
        Greedily searches, places people from large groups first
        If there are multiple possible starting positions, 
        """
        no_seat = 0
        seats = self.available_seats.copy()

        # Go through group sizes from big
        for group_size in range(8, 0, -1):

            # The amount of groups with this size
            for _ in range(0, self.people[group_size - 1]):

                # Get possible legal start positions for the group
                pos = self.find_legal_start_position(group_size, self.available_seats)
                # print(pos)
                # If there is at least one place these guys can sit
                if len(pos) > 0:
                    # Find the best position with a loop (not very fast)
                    # (minimal amount of seats occupied after seating the group on that position)
                    best_amt = np.size(self.available_seats)

                    amts = np.zeros(len(pos)) - 1
                    # a_s_s = np.zeros((len(pos), self.h + 4, self.v + 4)) - 1
                    poses = [-1] * len(pos)

                    for index, possible in enumerate(pos):
                        _, amt = self.update_seats(
                            possible, group_size, self.available_seats
                        )
                        amts[index] = amt

                        if amt <= best_amt:
                            best_amt = amt
                            poses[index] = possible

                    # Choose one of the best solutions
                    choices = np.nonzero(amts == best_amt)[0]
                    choice = np.random.choice(choices)
                    x, y = poses[choice]

                    # Update the available seats (add zeros)
                    self.available_seats, _ = self.update_seats(
                        poses[choice], group_size, self.available_seats
                    )

                    # Update where people are sitting
                    seats[x, y : y + group_size] = np.zeros(group_size) + 2

                # Else
                else:
                    no_seat += group_size

        # Remove padding
        return seats[2:-2, 2:-2], no_seat

    def find_legal_start_position(self, n, seats):
        """
        n:     amount of people in the group
        seats: 
        """
        opts = []
        for j, row in enumerate(seats):
            indices = np.where(np.concatenate(([row[0]], row[:-1] != row[1:], [True])))[
                0
            ]
            a = np.diff(indices)[::2]
            for i, z in enumerate(a):
                if n <= z:
                    opts.append((j, indices[i * 2]))
        return opts

    def update_seats(self, pos, n, a_s):
        """
        pos: (x,y) position in the cinema 
        n:   amount of people in the group
        a_s: current matrix of available_seats 

        Returns: a tuple with
            (matrix of available seats after the update, amount of free spaces in the matrix)
        """
        available_seats = a_s.copy()
        x, y = pos
        available_seats[x, y - 2 : y + n + 2] = np.zeros(n + 4)
        available_seats[x + 1, y - 1 : y + n + 1] = np.zeros(n + 2)
        available_seats[x - 1, y - 1 : y + n + 1] = np.zeros(n + 2)

        return available_seats, np.size(a_s) - np.count_nonzero(available_seats)
