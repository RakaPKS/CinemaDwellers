import unittest
import numpy as np
from enum import Enum


def count_seated(matrix):
    return np.count_nonzero(matrix == 2)

class LegalError(Enum):
    horizontal = 1
    vertical = 2
    diagnol = 3
    none = 4


def find_legal_start_positions(n, seats):
    """
    n:     amount of people in the group
    seats: matrix of seats 
    output: List of possible start positions
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


def check_legal(size1, size2, x1, x2, y1, y2):
    """Checks whether two group start positions are legal"""
    # Groups seated in the same row
    if x1 == x2 and y1 == y2:
        return (False, LegalError.horizontal)
    if y1 == y2:
        # Illegal if they are not far enough apart
        # group 1 is to the left of group 2
        if x1 < x2:
            if x2 - (x1 + (size1 - 1)) <= 2:
                return (False, LegalError.horizontal)
        # group 1 is to the right of group 2
        elif x1 > x2:
            if x1 - (x2 + (size2 - 1)) <= 2:
                return (False, LegalError.horizontal)

    # Groups are seated in the same "column"
    elif x1 == x2:
        if np.abs(y2 - y1) < 2:
            return (False, LegalError.vertical)

    # Only one row between the two groups
    # Treat the same as "same row" case, except that they can be 1 seat apart
    # instead of 2
    elif np.abs(y1 - y2) == 1:
        if x1 < x2:
            if x2 - (x1 + (size1 - 1)) <= 1:
                return (False, LegalError.diagnol)
        elif x1 > x2:
            if x1 - (x2 + (size2 - 1)) <= 1:
                return (False, LegalError.diagnol)

    return (True, LegalError.none)

def get_invalid_seats(x1, y1, g1, g2, xdim, ydim):
    result = []
    g_size1 = (g1 - 1)
    g_size2 = (g2 - 1)
    last_seat = x1 + g_size1

    for x2 in range(x1 - 2 - g_size2, x1 + g_size1 + 3):
        # only add inbounds
        if x2 >= 0 and x2 < xdim:
            result.append((x2, y1))

    for x2 in range(x1 - 2 - g_size2 + 1, x1 + g_size1 + 2):
        if (x2 >= 0 and x2 < xdim and (y1 + 1) >= 0 and (y1 + 1) < ydim):
            result.append((x2, y1 + 1))

        if x2 >= 0 and x2 < xdim and (y1 - 1) >= 0:
            result.append((x2, y1 - 1))

    return result


def verify_cinema(cinema, xlen, ylen):
    seated_groups = dict()
    group_size = 0
    first_person = (-1,-1)

    for y in range(0, ylen):
        for x in range(0, xlen):
            if cinema[y,x] == 2:
                # We have found our first person of the group
                if first_person == (-1,-1):
                    first_person = (x,y)
                    group_size += 1
                # This is not the first person, so we only increase group size
                else:
                   group_size += 1
            else:
                # We have reached the end of the group, lets store it
                if first_person != (-1,-1):
                    seated_groups[first_person] = group_size
                    group_size = 0
                    first_person = (-1,-1)
        # This makes sure we store the group at the edge of the cinema
        if first_person != (-1,-1):
            seated_groups[first_person] = group_size
            group_size = 0
            first_person = (-1,-1)

    cinema_meets_corona_guidelines = True

    print("---- VERIFYING CINEMA ----")
    for (x1,y1), g1 in seated_groups.items():
        for (x2, y2), g2 in seated_groups.items():
            if x1 != x2 and y1 != y2:
                (is_legal, err) = check_legal(g1, g2, x1, x2, y1, y2)
                if not is_legal:
                    print("CORONA ALERT")
                    print("{} guidline has been violated".format(str(err)))
                    print("Group seated at coordinates {} and size {}".format((x1,y1), g1))
                    print("Group seated at coordinates {} and size {}".format((x2,y2), g2))
                    print("----")
                    cinema_meets_corona_guidelines = False
    if cinema_meets_corona_guidelines:
        print("CONGRATULATIONS! YOUR CINEMA SEATING MEETS THE CORONA GUIDELINES!")
    else:
        print("DOOOOH! YOUR CINEMA SEATING DOES NOT MEET THE CORONA GUIDELINES!")




class TestCheckLegal(unittest.TestCase):
    """Checks for check_legal"""

    def test_horizontal(self):
        self.assertEqual(check_legal(2, 2, x1=0, x2=3, y1=0, y2=0), False, "Group 1 to left")
        self.assertEqual(check_legal(2, 2, x1=3, x2=0, y1=0, y2=0), False, "Group 1 to right")
        self.assertEqual(check_legal(1, 1, x1=0, x2=2, y1=0, y2=0), False, "Group 1 to right")
        self.assertEqual(check_legal(1, 1, x1=0, x2=1, y1=0, y2=0), False, "Group 1 to right")
        self.assertEqual(check_legal(2, 1, x1=0, x2=3, y1=0, y2=0), False)
        self.assertEqual(check_legal(2, 1, x1=0, x2=4, y1=0, y2=0), True)
        self.assertEqual(check_legal(1, 2, x1=4, x2=0, y1=0, y2=0), True)
        self.assertEqual(check_legal(1, 2, x1=3, x2=0, y1=0, y2=0), False, "Group sizes")
        self.assertEqual(check_legal(1, 1, x1=0, x2=3, y1=0, y2=0), True)
        self.assertEqual(check_legal(1, 2, x1=0, x2=2, y1=0, y2=0), False)

    def test_diagonal(self):
        self.assertEqual(check_legal(1, 1, x1=0, x2=1, y1=0, y2=1), False, "Diagonal")
        self.assertEqual(check_legal(5, 5, x1=0, x2=2, y1=2, y2=0), True)
        self.assertEqual(check_legal(1, 1, x1=0, x2=1, y1=0, y2=1), False)
        self.assertEqual(check_legal(1, 1, x1=0, x2=2, y1=0, y2=1), True)
        self.assertEqual(check_legal(2, 1, x1=0, x2=2, y1=0, y2=1), False)
        self.assertEqual(check_legal(2, 1, x1=0, x2=2, y1=0, y2=1), False)
        self.assertEqual(check_legal(2, 1, x1=3, x2=2, y1=0, y2=1), False)

    def test_vertical(self):
        self.assertEqual(check_legal(1, 1, x1=0, x2=0, y1=0, y2=1), False)
        self.assertEqual(check_legal(1, 1, x1=0, x2=0, y1=0, y2=2), True)
        self.assertEqual(check_legal(5, 5, x1=0, x2=0, y1=2, y2=0), True)
        self.assertEqual(check_legal(5, 5, x1=0, x2=0, y1=2, y2=1), False)


if __name__ == "__main__":
    # unittest.main()
    print(invalid_seats(9, 0, 1, 2, 10, 6))
