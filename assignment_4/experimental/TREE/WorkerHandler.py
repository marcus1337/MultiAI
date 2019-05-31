import sc2
import math

class WorkerHandler(object):

    def __init__(self, my_bot):
        self.bot: sc2.BotAI = my_bot

    def update(self):
        queens = self.bot.units(sc2.UnitTypeId.QUEEN)
        hatcheries = self.bot.units(sc2.UnitTypeId.HATCHERY)

        for queen in queens:
            hatch = hatcheries[0]
            for h in hatcheries:
                if h.position.distance_to(queen.position) < hatch.position.distance_to(queen.position):
                    hatch = h
            self.bot.commands.append(queen(sc2.AbilityId.EFFECT_INJECTLARVA, hatch))

        return "success"
