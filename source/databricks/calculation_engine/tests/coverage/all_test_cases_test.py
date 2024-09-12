# fmt: off

from dataclasses import dataclass


@dataclass
class Root:
    class Level1:
        Level1_Scenario1: str
        Level1_Scenario2: str
        class Level1-1:
            Level1_1Scenario1: str
            class Level1-1-1:
                Level1_1_1Scenario1: str
                Level1_1_1Scenario2: str
        class Level1-2:
            Level1_2Scenario1: str
    class Level2:
        Level2_Scenario1: str
        class Level2-1:
            Level2_1Scenario1: str