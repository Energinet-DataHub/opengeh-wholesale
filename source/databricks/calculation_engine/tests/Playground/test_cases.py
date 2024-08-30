from dataclasses import dataclass


@dataclass
class TestCases:
    TC001_mp_is_out_of_whack: str
    TC002: str = "Multiple MP all over the place"
    TC003: str = "GA is beyond repair"
    TC004: str = "Ny test"
    # Add more test cases as needed
