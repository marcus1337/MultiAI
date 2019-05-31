
from sc2 import run_game, maps, Race, Difficulty
from sc2.player import Bot, Computer
from TheBot import TheBot


run_game(maps.get("ProximaStationLE"), [
    Bot(Race.Zerg, TheBot()),
    #Computer(Race.Protoss, Difficulty.VeryEasy)
    Computer(Race.Protoss, Difficulty.Medium)
], realtime=True)