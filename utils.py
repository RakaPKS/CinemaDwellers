import unittest
import numpy as np


def check_legal(size1, size2, x1, x2, y1, y2):
    """Checks whether two group start positions are legal"""
    # Groups seated in the same row
    if y1 == y2:
        # Illegal if they are not far enough apart
        # group 1 is to the left of group 2
        if x1 < x2:
            if x2 - (x1 + (size1 - 1)) <= 2:
                return False
        # group 1 is to the right of group 2
        elif x1 > x2:
            if x1 - (x2 + (size2 - 1)) <= 2:
                return False

    # Groups are seated in the same "column"
    elif x1 == x2:
        if np.abs(y2 - y1) < 2:
            return False

    # Only one row between the two groups
    elif np.abs(y1 - y2) == 1:
        if x1 < x2:
            if x2 - (x1 + (size1 - 1)) <= 1:
                return False
        elif x1 > x2:
            if x1 - (x2 + (size2 - 1)) <= 1:
                return False

    return True


class TestCheckLegal(unittest.TestCase):
    def test_horizontal(self):
        self.assertEqual(
            check_legal(2, 2, x1=0, x2=3, y1=0, y2=0), False, "Group 1 to left"
        )
        self.assertEqual(
            check_legal(2, 2, x1=3, x2=0, y1=0, y2=0), False, "Group 1 to right"
        )
        self.assertEqual(
            check_legal(1, 1, x1=0, x2=2, y1=0, y2=0), False, "Group 1 to right"
        )
        self.assertEqual(
            check_legal(1, 1, x1=0, x2=1, y1=0, y2=0), False, "Group 1 to right"
        )
        self.assertEqual(check_legal(2, 1, x1=0, x2=3, y1=0, y2=0), False)
        self.assertEqual(check_legal(2, 1, x1=0, x2=4, y1=0, y2=0), True)
        self.assertEqual(check_legal(1, 2, x1=4, x2=0, y1=0, y2=0), True)
        self.assertEqual(
            check_legal(1, 2, x1=3, x2=0, y1=0, y2=0), False, "Group sizes"
        )
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
    unittest.main()
