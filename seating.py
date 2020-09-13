import numpy as np


class Seating:
    def __init__(self, seats, people, h, v):
        """
        Seats: Matrix
        people: array with group sizes
        h,v : size of matrix
        """
        # Add padding so no out of range errors
        self.available_seats = np.pad(seats, (2, 2), "constant", constant_values=(0, 0))
        self.people = people
        self.h = h
        self.v = v

    def greedy(self):
        """
        Greedily searches, places people from large groups first
        If there are multiple possible starting positions, 
        """
        no_seat = 0
        seats = self.available_seats.copy()

        # Go through group sizes from big to small
        for group_size in range(8, 0, -1):

            # The amount of groups with this size
            for _ in range(0, self.people[group_size - 1]):

                # Get possible legal start positions for the group
                pos = self.find_legal_start_position(group_size, self.available_seats)

                # If there is at least one place these guys can sit
                if len(pos) > 0:
                    # Find the best position with a loop
                    # best = (minimal amount of seats occupied after seating the group on that position)
                    best_amt = np.size(self.available_seats)

                    amts = np.zeros(len(pos)) - 1
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
                # No places this group can sit, time to move on.
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
